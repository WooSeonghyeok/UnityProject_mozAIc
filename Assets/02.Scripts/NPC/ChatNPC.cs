using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ChatNPC : MonoBehaviour
{
    private const float InteractionDistance = 3f;

    [Header("Setting")]
    [SerializeField] private GameObject interChatUI;
    [SerializeField] private Transform chatPos;
    [SerializeField] private Transform playerTr;
    [SerializeField] private NPCFollower npcFollower;
    [SerializeField] private PlayerInput user;
    private float distance;
    private NPCData npcData;  // NPC의 이름/성격/프롬프트 데이터 참조
    private bool isInteractBound;

    public bool isChat = false;

    private void Awake()
    {
        // 시작할 때 같은 오브젝트의 NPCData를 캐싱
        npcData = GetComponent<NPCData>();
        ResolvePlayerReferences();

        if (npcFollower == null)
            npcFollower = GetComponent<NPCFollower>();
    }
    private void OnEnable()
    {
        ResolvePlayerReferences();
        BindInteractInput();
    }
    private void OnDisable()
    {
        UnbindInteractInput();
    }

    void Update()
    {
        ResolvePlayerReferences();
        BindInteractInput();

        if (playerTr == null || ChatNPCManager.instance == null)
        {
            SetInteractionUi(false);
            return;
        }

        distance = Vector3.Distance(transform.position, playerTr.position);

        if (distance < InteractionDistance && !ChatNPCManager.instance.isTalking)
        {
            // 플레이어 쪽을 바라보기
            Vector3 targetPos = playerTr.position;
            targetPos.y = transform.position.y;
            transform.LookAt(targetPos);
            SetInteractionUi(true);
        }
        else
        {
            SetInteractionUi(false);
        }
    }

    private void StartNPCChat()
    {
        if (ChatNPCManager.instance == null || npcData == null)
        {
            return;
        }

        if (!ResolvePlayerReferences())
        {
            return;
        }

        if (distance < InteractionDistance && !ChatNPCManager.instance.isTalking)
        {
            if (npcFollower != null) npcFollower.SetFollow(false);
            ChatNPCManager.instance.NpcPersonTalk(chatPos, npcData);
        }
        else return;
    }

    private void LateUpdate()
    {
        if (interChatUI != null && interChatUI.activeSelf && Camera.main != null)
        {
            interChatUI.transform.forward = Camera.main.transform.forward;
        }
    }

    private bool ResolvePlayerReferences()
    {
        GameObject player = null;

        if (playerTr == null || user == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");

            if (player == null)
            {
                return false;
            }
        }

        if (playerTr == null && player != null)
        {
            playerTr = player.transform;
        }

        if (user == null && player != null)
        {
            user = player.GetComponent<PlayerInput>();
        }

        return playerTr != null;
    }

    private void BindInteractInput()
    {
        if (isInteractBound || user == null)
        {
            return;
        }

        user.Interact += StartNPCChat;
        isInteractBound = true;
    }

    private void UnbindInteractInput()
    {
        if (!isInteractBound || user == null)
        {
            return;
        }

        user.Interact -= StartNPCChat;
        isInteractBound = false;
    }

    private void SetInteractionUi(bool isActive)
    {
        if (interChatUI == null)
        {
            return;
        }

        if (interChatUI.activeSelf != isActive)
        {
            interChatUI.SetActive(isActive);
        }
    }
}
