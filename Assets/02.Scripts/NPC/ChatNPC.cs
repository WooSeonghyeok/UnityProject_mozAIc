using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ChatNPC : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private GameObject interChatUI;
    [SerializeField] private Transform chatPos;
    [SerializeField] private Transform playerTr;
    [SerializeField] private NPCFollower npcFollower;
    [SerializeField] private PlayerInput user;
    [SerializeField] private bool lookAtPlayer = true;
    private float distance;
    private NPCData npcData;  // NPC의 이름/성격/프롬프트 데이터 참조

    public bool isChat = false;

    private void Awake()
    {
        // 시작할 때 같은 오브젝트의 NPCData를 캐싱
        npcData = GetComponent<NPCData>();
        user = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInput>();

        if (npcFollower == null)
            npcFollower = GetComponent<NPCFollower>();
    }
    private void OnEnable()
    {
        user.Interact += StartNPCChat;
    }
    private void OnDisable()
    {
        user.Interact -= StartNPCChat;
    }

    void Update()
    {
        distance = Vector3.Distance(this.transform.position, playerTr.transform.position);

        if (distance < 3 && !ChatNPCManager.instance.isTalking)
        {
            // 필요할 때만 플레이어를 바라보게 함
            if (lookAtPlayer)
            {
                Vector3 targetPos = playerTr.position;
                targetPos.y = transform.position.y; // 상하 회전 방지
                transform.LookAt(targetPos);
            }

            interChatUI.SetActive(true);
        }
        else
        {
            interChatUI.SetActive(false);
        }
    }

    private void StartNPCChat()
    {
        if (distance < 3 && !ChatNPCManager.instance.isTalking)
        {
            if (npcFollower != null) npcFollower.SetFollow(false);
            ChatNPCManager.instance.NpcPersonTalk(chatPos, npcData);
        }
        else return;
    }

    private void LateUpdate()
    {
        if (Camera.main != null)
        {
            interChatUI.transform.forward = Camera.main.transform.forward;
        }
    }
}
