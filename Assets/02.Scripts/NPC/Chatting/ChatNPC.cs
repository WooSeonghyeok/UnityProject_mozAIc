using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Episode3.Common;

public class ChatNPC : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private GameObject interChatUI;
    [SerializeField] private Transform chatPos;
    [SerializeField] private Transform playerTr;
    [SerializeField] private NPCFollower npcFollower;
    [SerializeField] private PlayerInput user;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private bool lookAtPlayer = true;
    [SerializeField] private float higherPriorityInteractCheckRadius = 1.1f;
    private float distance;
    private NPCData npcData;  // NPC의 이름/성격/프롬프트 데이터 참조
    private PlayerInput subscribedUser;
    private readonly Collider[] interactionCheckResults = new Collider[24];

    public bool isChat = false;

    private void Awake()
    {
        // 시작할 때 같은 오브젝트의 NPCData를 캐싱
        npcData = GetComponent<NPCData>();
        if (npcFollower == null)
            npcFollower = GetComponent<NPCFollower>();

        TryResolvePlayerReferences();
    }
    private void OnEnable()
    {
        TryResolvePlayerReferences();
        RefreshInteractSubscription();
    }
    private void OnDisable()
    {
        RefreshInteractSubscription(clearSubscription: true);
    }

    void Update()
    {
        if (!TryResolvePlayerReferences())
        {
            RefreshInteractSubscription(clearSubscription: true);
            if (interChatUI != null && interChatUI.activeSelf)
            {
                interChatUI.SetActive(false);
            }
            return;
        }

        RefreshInteractSubscription();
        distance = Vector3.Distance(this.transform.position, playerTr.position);
        bool shouldYieldToObjectInteraction = HasHigherPriorityObjectInteraction();

        if (playerMovement != null && playerMovement.IsMoveLocked)
        {
            if (interChatUI != null && interChatUI.activeSelf)
            {
                interChatUI.SetActive(false);
            }
            return;
        }

        if (!shouldYieldToObjectInteraction && distance < 3 && ChatNPCManager.instance != null && !ChatNPCManager.instance.isTalking)
        {
            // 필요할 때만 플레이어를 바라보게 함
            if (lookAtPlayer)
            {
                Vector3 targetPos = playerTr.position;
                targetPos.y = transform.position.y; // 상하 회전 방지
                transform.LookAt(targetPos);
            }

            if (interChatUI != null)
            {
                interChatUI.SetActive(true);
            }
        }
        else
        {
            if (interChatUI != null)
            {
                interChatUI.SetActive(false);
            }
        }
    }

    private void StartNPCChat()
    {
        if (playerMovement != null && playerMovement.IsMoveLocked)
        {
            return;
        }

        if (HasHigherPriorityObjectInteraction())
        {
            return;
        }

        if (distance < 3 && ChatNPCManager.instance != null && !ChatNPCManager.instance.isTalking)
        {
            if (npcFollower != null) npcFollower.SetFollow(false);
            ChatNPCManager.instance.NpcPersonTalk(chatPos, npcData);
        }
        else return;
    }

    private void LateUpdate()
    {
        if (interChatUI != null && Camera.main != null)
        {
            interChatUI.transform.forward = Camera.main.transform.forward;
        }
    }

    private bool TryResolvePlayerReferences()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            user = null;
            playerTr = null;
            return false;
        }

        if (playerTr == null)
        {
            playerTr = player.transform;
        }

        if (user == null)
        {
            user = player.GetComponent<PlayerInput>();
        }

        if (playerMovement == null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
        }

        return playerTr != null && user != null;
    }

    private bool HasHigherPriorityObjectInteraction()
    {
        if (playerTr == null)
        {
            return false;
        }

        int hitCount = Physics.OverlapSphereNonAlloc(
            playerTr.position,
            higherPriorityInteractCheckRadius,
            interactionCheckResults,
            ~0,
            QueryTriggerInteraction.Collide);

        for (int i = 0; i < hitCount; i++)
        {
            Collider hitCollider = interactionCheckResults[i];
            if (hitCollider == null)
            {
                continue;
            }

            Transform hitTransform = hitCollider.transform;
            if (hitTransform == transform || hitTransform.IsChildOf(transform))
            {
                continue;
            }

            if (HasObjectInteractionComponent(hitTransform))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasObjectInteractionComponent(Transform target)
    {
        InteractableSymbol interactableSymbol = target.GetComponentInParent<InteractableSymbol>();
        if (interactableSymbol != null && interactableSymbol.isActiveAndEnabled && interactableSymbol.HasPriorityOverNpcInteraction())
        {
            return true;
        }

        Ep3_3InteractPoint ep3FinalInteractPoint = target.GetComponentInParent<Ep3_3InteractPoint>();
        if (ep3FinalInteractPoint != null && ep3FinalInteractPoint.isActiveAndEnabled)
        {
            return true;
        }

        EP2_InteractObject ep2InteractObject = target.GetComponentInParent<EP2_InteractObject>();
        if (ep2InteractObject != null && ep2InteractObject.isActiveAndEnabled)
        {
            return true;
        }

        AltarInteractable altarInteractable = target.GetComponentInParent<AltarInteractable>();
        if (altarInteractable != null && altarInteractable.isActiveAndEnabled)
        {
            return true;
        }

        EP4_CubeSwitch ep4CubeSwitch = target.GetComponentInParent<EP4_CubeSwitch>();
        if (ep4CubeSwitch != null && ep4CubeSwitch.isActiveAndEnabled)
        {
            return true;
        }

        return false;
    }

    private void RefreshInteractSubscription(bool clearSubscription = false)
    {
        PlayerInput nextUser = clearSubscription ? null : user;
        if (subscribedUser == nextUser)
        {
            return;
        }

        if (subscribedUser != null)
        {
            subscribedUser.Interact -= StartNPCChat;
        }

        subscribedUser = nextUser;

        if (subscribedUser != null && isActiveAndEnabled)
        {
            subscribedUser.Interact += StartNPCChat;
        }
    }
}
