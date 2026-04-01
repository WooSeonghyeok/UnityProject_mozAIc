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

    private float distance;
    private NPCData npcData;  // NPC의 이름/성격/프롬프트 데이터 참조

    public bool isChat = false;

    private void Awake()
    {
        // 시작할 때 같은 오브젝트의 NPCData를 캐싱
        npcData = GetComponent<NPCData>();

        if (npcFollower == null)
            npcFollower = GetComponent<NPCFollower>();
    }

    void Update()
    {
        distance = Vector3.Distance(this.transform.position, playerTr.transform.position);

        if (distance < 3 && !ChatNPCManager.instance.isTalking)
        {
            // 플레이어 쪽을 바라보기
            Vector3 targetPos = playerTr.position;
            targetPos.y = this.transform.position.y;
            this.transform.LookAt(targetPos);

            interChatUI.SetActive(true);

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (npcFollower != null)
                    npcFollower.SetFollow(false);

                ChatNPCManager.instance.NpcPersonTalk(chatPos, npcData);
            }
        }
        else
        {
            interChatUI.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (Camera.main != null)
        {
            interChatUI.transform.forward = Camera.main.transform.forward;
        }
    }
}
