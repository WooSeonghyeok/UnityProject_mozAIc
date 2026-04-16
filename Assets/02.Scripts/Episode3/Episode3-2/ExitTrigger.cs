using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class ExitTrigger : MonoBehaviour
{
    [Header("이동할 씬 이름")]
    [SerializeField] private string nextSceneName;

    [Header("문 열림 확인용")]
    [SerializeField] private DoorOpen doorOpen;

    [Header("출구 도착 컷씬")]
    [SerializeField] private CutsceneImagePlayer arrivalCutscenePlayer;
    [SerializeField] private bool waitForCutsceneBeforeSceneLoad = true;
    [SerializeField] private bool loadSceneAfterArrivalSequence = true;

    [Header("NPC 출구 합류 연출")]
    [SerializeField] private bool teleportNpcOnArrival = false;
    [SerializeField] private Transform npcTransform;
    [SerializeField] private Transform npcTeleportPoint;
    [SerializeField] private Transform npcLookTarget;

    [Header("중복 이동 방지")]
    [SerializeField] private bool onlyOnce = true;

    private bool moved = false;
    private bool sequenceStarted = false;
    private bool cutsceneListenerAdded = false;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnDestroy()
    {
        RemoveCutsceneListener();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (sequenceStarted)
        {
            return;
        }

        if (moved && onlyOnce)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (doorOpen != null && !doorOpen.IsOpen)
        {
            return;
        }

        sequenceStarted = true;
        TeleportNpcToArrivalPoint();

        if (ShouldPlayArrivalCutscene())
        {
            if (waitForCutsceneBeforeSceneLoad)
            {
                AddCutsceneListener();
            }

            arrivalCutscenePlayer.PlayCutscene();

            if (waitForCutsceneBeforeSceneLoad)
            {
                return;
            }
        }

        CompleteArrivalSequence();
    }

    private void OnArrivalCutsceneFinished()
    {
        RemoveCutsceneListener();

        if (!sequenceStarted)
        {
            return;
        }

        CompleteArrivalSequence();
    }

    private void CompleteArrivalSequence()
    {
        moved = true;
        sequenceStarted = false;

        if (!loadSceneAfterArrivalSequence)
        {
            return;
        }

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("[ExitTrigger] nextSceneName이 비어 있습니다.");
            return;
        }

        SceneManager.LoadScene(nextSceneName);
    }

    private bool ShouldPlayArrivalCutscene()
    {
        return arrivalCutscenePlayer != null &&
               arrivalCutscenePlayer.HasConfiguredImages &&
               !arrivalCutscenePlayer.IsPlaying;
    }

    private void AddCutsceneListener()
    {
        if (cutsceneListenerAdded || arrivalCutscenePlayer == null)
        {
            return;
        }

        arrivalCutscenePlayer.AddFinishedListener(OnArrivalCutsceneFinished);
        cutsceneListenerAdded = true;
    }

    private void RemoveCutsceneListener()
    {
        if (!cutsceneListenerAdded || arrivalCutscenePlayer == null)
        {
            return;
        }

        arrivalCutscenePlayer.RemoveFinishedListener(OnArrivalCutsceneFinished);
        cutsceneListenerAdded = false;
    }

    private void TeleportNpcToArrivalPoint()
    {
        if (!teleportNpcOnArrival || npcTransform == null || npcTeleportPoint == null)
        {
            return;
        }

        Vector3 targetPosition = npcTeleportPoint.position;
        Quaternion targetRotation = ResolveNpcRotation(targetPosition);

        NPCFollower npcFollower = npcTransform.GetComponent<NPCFollower>();
        if (npcFollower != null)
        {
            npcFollower.SetFollow(false);
            npcFollower.WarpTo(targetPosition, targetRotation);
            return;
        }

        NavMeshAgent agent = npcTransform.GetComponent<NavMeshAgent>();
        if (agent != null && agent.enabled)
        {
            if (agent.isOnNavMesh)
            {
                agent.ResetPath();
            }

            if (!agent.Warp(targetPosition))
            {
                npcTransform.position = targetPosition;
            }

            npcTransform.rotation = targetRotation;
            return;
        }

        Rigidbody targetRigidbody = npcTransform.GetComponent<Rigidbody>();
        if (targetRigidbody != null)
        {
            targetRigidbody.velocity = Vector3.zero;
            targetRigidbody.angularVelocity = Vector3.zero;
            targetRigidbody.position = targetPosition;
            targetRigidbody.rotation = targetRotation;
            return;
        }

        npcTransform.SetPositionAndRotation(targetPosition, targetRotation);
    }

    private Quaternion ResolveNpcRotation(Vector3 targetPosition)
    {
        if (npcLookTarget == null)
        {
            return npcTeleportPoint.rotation;
        }

        Vector3 lookDirection = npcLookTarget.position - targetPosition;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude <= 0.0001f)
        {
            return npcTeleportPoint.rotation;
        }

        return Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
    }
}
