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

    [Header("애니메이션")]
    [SerializeField] private Animator animator;
    [SerializeField] private float moveThreshold = 0.1f;

    private readonly int hashMove = Animator.StringToHash("IsMove");

    private bool canFollow = true;

    private Transform moveTarget;
    private System.Action onArrived;

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
        if (animator == null)
            animator = GetComponent<Animator>();
        TryResolvePlayerMovement();
    }

    private void Update()
    {
        if (player == null || agent == null)
        {
            UpdateMoveAnimation(false);
            return;
        }

        TryResolvePlayerMovement();

        // 컷씬/대화 등으로 플레이어 조작이 잠겨 있는 동안에는 NPC도 멈춘다.
        if (playerMovement != null && playerMovement.IsMoveLocked)
        {
            agent.ResetPath();
            UpdateMoveAnimation(false);
            return;
        }

        // 대화 중이면 이동 금지
        if (ChatNPCManager.instance != null && ChatNPCManager.instance.isTalking)
        {
            agent.ResetPath();
            UpdateMoveAnimation(false);
            return;
        }
        // 특정 위치로 이동 중이면 우선 처리
        if (moveTarget != null)
        {
            agent.stoppingDistance = stopDistance;
            agent.SetDestination(moveTarget.position);

            // 목적지 도착 판정
            if (!agent.pathPending && agent.remainingDistance <= stopDistance + arriveThreshold)
            {
                agent.ResetPath();
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
            agent.ResetPath();
            UpdateMoveAnimation(false);
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > resumeDistance)
        {
            agent.stoppingDistance = stopDistance;
            agent.SetDestination(player.position);
        }
        else if (distance <= stopDistance)
        {
            agent.ResetPath();
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
            agent.ResetPath();
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
            agent.ResetPath();
            agent.Warp(position);
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
}
