using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class NPCFollower : MonoBehaviour
{
    [Header("타겟")]
    [SerializeField] private Transform player;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("이동")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float resumeDistance = 3.5f;
    [SerializeField] private float stopDistance = 2f;
    [SerializeField] private float arriveThreshold = 0.2f;
    [SerializeField] private float targetSampleRadius = 3f;
    [SerializeField] private float repathInterval = 0.2f;
    [SerializeField] private float repathTargetMoveThreshold = 0.5f;

    [Header("애니메이션")]
    [SerializeField] private Animator animator;
    [SerializeField] private float moveThreshold = 0.1f;

    private readonly int hashMove = Animator.StringToHash("IsMove");

    private bool canFollow = true;
    private bool hasRequestedDestination;
    private float nextRepathTime;
    private Vector3 lastRequestedDestination;

    private Transform moveTarget;
    private System.Action onArrived;
    private bool hasWarnedMissingNavMesh;

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
        if (animator == null)
            animator = GetComponent<Animator>();

        TryResolvePlayerMovement();
        TryRestoreAgentOnNavMesh();
    }

    private void Start()
    {
        TryRestoreAgentOnNavMesh();
    }

    private void Update()
    {
        TryResolvePlayerMovement();
        TryRestoreAgentOnNavMesh();

        if (player == null || agent == null)
        {
            UpdateMoveAnimation(false);
            return;
        }

        if (!CanUseAgent())
        {
            ClearRequestedDestination();
            UpdateMoveAnimation(false);
            return;
        }

        // 컷씬/대화 등으로 플레이어 조작이 잠겨 있는 동안에는 NPC도 멈춘다.
        if (playerMovement != null && playerMovement.IsMoveLocked)
        {
            ResetAgentPathIfPossible();
            ClearRequestedDestination();
            UpdateMoveAnimation(false);
            return;
        }

        // 대화 중이면 이동 금지
        if (ChatNPCManager.instance != null && ChatNPCManager.instance.isTalking)
        {
            ResetAgentPathIfPossible();
            ClearRequestedDestination();
            UpdateMoveAnimation(false);
            return;
        }
        // 특정 위치로 이동 중이면 우선 처리
        if (moveTarget != null)
        {
            agent.stoppingDistance = stopDistance;
            RequestDestination(moveTarget.position, forceRefresh: true);

            // 목적지 도착 판정
            if (!agent.pathPending && agent.remainingDistance <= stopDistance + arriveThreshold)
            {
                ResetAgentPathIfPossible();
                ClearRequestedDestination();
                UpdateMoveAnimation(false);

                Transform arrivedTarget = moveTarget;
                moveTarget = null;

                // 도착 콜백 실행
                onArrived?.Invoke();
                onArrived = null;
            }
            else
            {
                bool forceMoving = agent.velocity.sqrMagnitude > moveThreshold * moveThreshold;
                UpdateMoveAnimation(forceMoving);
            }

            return;
        }
        // 외부에서 따라가기 금지 상태면 멈춤
        if (!canFollow)
        {
            ResetAgentPathIfPossible();
            ClearRequestedDestination();
            UpdateMoveAnimation(false);
            return;
        }

        float distance = GetFollowDistance(player.position);
        bool isPathActive = agent.hasPath || agent.pathPending;

        if (distance > resumeDistance)
        {
            agent.stoppingDistance = stopDistance;
            RequestDestination(player.position, forceRefresh: !isPathActive);
        }
        else if (isPathActive && distance <= stopDistance + arriveThreshold)
        {
            ResetAgentPathIfPossible();
            ClearRequestedDestination();
        }

        // 실제 속도를 기준으로 이동 애니메이션 판정
        bool isMoving = agent.velocity.sqrMagnitude > moveThreshold * moveThreshold;
        UpdateMoveAnimation(isMoving);
    }

    // 따라가기 허용/비허용 설정
    public void SetFollow(bool value)
    {
        canFollow = value;

        if (!canFollow && agent != null)
        {
            ResetAgentPathIfPossible();
            ClearRequestedDestination();
            UpdateMoveAnimation(false);
        }
    }

    // 따라갈 대상 지정
    public void SetTarget(Transform target)
    {
        player = target;
    }
    // 특정 위치로 이동시키기
    public void MoveToPoint(Transform targetPoint, System.Action arrivedCallback = null)
    {
        if (targetPoint == null || agent == null)
            return;

        canFollow = false;
        moveTarget = targetPoint;
        onArrived = arrivedCallback;
    }

    // 순간이동
    public void WarpTo(Vector3 position, Quaternion rotation)
    {
        if (agent != null)
        {
            ResetAgentPathIfPossible();
            ClearRequestedDestination();
            if (CanUseAgent())
            {
                agent.Warp(position);
            }
            else
            {
                transform.position = position;
            }
        }
        else
        {
            transform.position = position;
        }

        transform.rotation = rotation;
        UpdateMoveAnimation(false);
    }
    // 이동 여부에 따라 Animator Bool 갱신
    private void UpdateMoveAnimation(bool isMoving)
    {
        if (animator == null)
            return;

        animator.SetBool(hashMove, isMoving);
    }

    private void TryResolvePlayerMovement()
    {
        if (playerMovement != null)
            return;

        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
        }

        if (playerMovement == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                if (player == null)
                    player = playerObject.transform;

                playerMovement = playerObject.GetComponent<PlayerMovement>();
            }
        }
    }

    private float GetFollowDistance(Vector3 targetPosition)
    {
        if (TryGetNavMeshPathDistance(targetPosition, out float pathDistance))
            return pathDistance;

        return Vector3.Distance(transform.position, targetPosition);
    }

    private bool TryGetNavMeshPathDistance(Vector3 targetPosition, out float pathDistance)
    {
        pathDistance = 0f;

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return false;

        NavMeshPath followPath = new NavMeshPath();

        if (!TryGetSampledDestination(targetPosition, out Vector3 destination))
            return false;

        if (!agent.CalculatePath(destination, followPath))
            return false;

        if (followPath.status == NavMeshPathStatus.PathInvalid)
            return false;

        Vector3[] corners = followPath.corners;
        if (corners == null || corners.Length == 0)
            return false;

        if (corners.Length == 1)
        {
            pathDistance = Vector3.Distance(transform.position, corners[0]);
            return true;
        }

        for (int i = 1; i < corners.Length; i++)
        {
            pathDistance += Vector3.Distance(corners[i - 1], corners[i]);
        }

        return true;
    }

    private void RequestDestination(Vector3 targetPosition, bool forceRefresh = false)
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        Vector3 destination = targetPosition;
        TryGetSampledDestination(targetPosition, out destination);

        if (!forceRefresh && !ShouldRefreshDestination(destination))
            return;

        if (!agent.SetDestination(destination))
            return;

        hasRequestedDestination = true;
        lastRequestedDestination = destination;
        nextRepathTime = Time.time + repathInterval;
    }

    private bool TryGetSampledDestination(Vector3 targetPosition, out Vector3 sampledDestination)
    {
        sampledDestination = targetPosition;

        if (agent == null || !agent.enabled)
            return false;

        if (!NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, targetSampleRadius, agent.areaMask))
            return false;

        sampledDestination = hit.position;
        return true;
    }

    private bool ShouldRefreshDestination(Vector3 destination)
    {
        if (!hasRequestedDestination)
            return true;

        if (!agent.hasPath || agent.pathPending)
            return true;

        if (agent.pathStatus != NavMeshPathStatus.PathComplete)
            return true;

        if (Time.time >= nextRepathTime)
            return true;

        return Vector3.Distance(lastRequestedDestination, destination) >= repathTargetMoveThreshold;
    }

    private void ClearRequestedDestination()
    {
        hasRequestedDestination = false;
        nextRepathTime = 0f;
    }

    private void TryRestoreAgentOnNavMesh()
    {
        if (agent == null || !gameObject.activeInHierarchy)
            return;

        if (CanUseAgent())
        {
            hasWarnedMissingNavMesh = false;
            return;
        }

        if (!TryFindClosestNavMeshPosition(out Vector3 navMeshPosition))
        {
            if (agent.enabled)
            {
                agent.enabled = false;
            }

            if (!hasWarnedMissingNavMesh)
            {
                Debug.LogWarning($"[NPCFollower] '{name}' 주변에 유효한 NavMesh가 없어 에이전트를 대기 상태로 전환합니다.");
                hasWarnedMissingNavMesh = true;
            }

            return;
        }

        if (!agent.enabled)
        {
            agent.enabled = true;
        }

        if (agent.isOnNavMesh)
        {
            agent.Warp(navMeshPosition);
            hasWarnedMissingNavMesh = false;
        }
    }

    private bool TryFindClosestNavMeshPosition(out Vector3 navMeshPosition)
    {
        navMeshPosition = transform.position;

        if (agent == null)
            return false;

        float sampleRadius = Mathf.Max(targetSampleRadius, agent.radius + 0.5f);

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, sampleRadius, agent.areaMask) ||
            NavMesh.SamplePosition(transform.position, out hit, sampleRadius * 4f, agent.areaMask))
        {
            navMeshPosition = hit.position;
            return true;
        }

        return false;
    }

    private bool CanUseAgent()
    {
        return agent != null && agent.enabled && agent.isActiveAndEnabled && agent.isOnNavMesh;
    }

    private void ResetAgentPathIfPossible()
    {
        if (!CanUseAgent())
            return;

        agent.ResetPath();
    }
}
