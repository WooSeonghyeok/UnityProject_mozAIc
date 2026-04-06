using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalSequenceController : MonoBehaviour
{
    [Header("별 수집")]
    [SerializeField] private PlayerStarCollector playerStarCollector;
    [SerializeField] private int targetStarCount = 5; 
    [Header("NPC")]
    [SerializeField] private NPCFollower npcFollower;
    [SerializeField] private NPCData npcData; // 말풍선을 띄울 NPC 데이터
    [SerializeField] private Transform npcPortalMovePoint;       // NPC가 먼저 걸어갈 위치
    [SerializeField] private Transform npcDestinationSpawnPoint; // 다음 위치에서 NPC가 나타날 위치
    [Header("플레이어 포탈")]
    [SerializeField] private PortalTeleport portalTeleport;
    private Collider portalCollider;
    [Header("연출")]
    [SerializeField] private GameObject npcObject;
    [SerializeField] private float moveStartDelay = 1.2f; // 말풍선 후 약간 텀 두고 이동 시작
    private bool sequenceStarted = false;
    private void OnEnable()
    {
        // 별 개수 변경 이벤트 구독
        if (playerStarCollector != null)
        {
            playerStarCollector.OnStarCountChanged += HandleStarCountChanged;
        }
    }
    private void OnDisable()
    {
        // 이벤트 해제
        if (playerStarCollector != null)
        {
            playerStarCollector.OnStarCountChanged -= HandleStarCountChanged;
        }
    }
    // 별 개수 변화 감지
    private void HandleStarCountChanged(int currentStarCount)
    {
        // 이미 시퀀스가 시작됐으면 중복 실행 방지
        if (sequenceStarted)  return;
        // 별을 5개 이상 모았을 때 포탈 시퀀스 시작
        if (currentStarCount >= targetStarCount)
        {
            sequenceStarted = true;
            // 먼저 말풍선 대사 출력
            if (npcData != null && ChatNPCManager.instance != null)
            {
                ChatNPCManager.instance.PlayNpcBubbleDialogue(npcData, "all_star_collected_hint");
            }
            // 말풍선이 뜨는 동안 미리 플레이어 추적 끄기
            if (npcFollower != null)
            {
                npcFollower.SetFollow(false);
            }
            // 약간의 텀을 둔 뒤 NPC 이동 시작
            StartCoroutine(CoStartPortalSequence());
        }
    }
    // 말풍선 후 시퀀스 시작용 코루틴
    private IEnumerator CoStartPortalSequence()
    {
        yield return new WaitForSeconds(moveStartDelay);
        StartPortalSequence();
    }
    // 별 5개 획득 완료 시 호출
    public void StartPortalSequence()
    {
        if (npcFollower == null || npcPortalMovePoint == null)
        {
            Debug.LogWarning("[PortalSequenceController] NPCFollower 또는 npcPortalMovePoint가 비어 있음");
            return;
        }
        // NPC가 플레이어 추적을 멈추고 포탈 앞으로 이동
        npcFollower.MoveToPoint(npcPortalMovePoint, OnNpcArrivedAtPortal);
    }
    private void Start()
    {
        if (portalTeleport != null)
        {
            portalCollider = portalTeleport.GetComponent<Collider>();
            if (portalCollider != null)
            {
                portalCollider.enabled = false; // 시작 시 포탈 사용 불가
            }
        }
    }
    private void OnNpcArrivedAtPortal()
    {
        if (npcFollower == null || npcDestinationSpawnPoint == null)
            return;
        // NPC를 다음 위치로 워프
        npcFollower.WarpTo(npcDestinationSpawnPoint.position, npcDestinationSpawnPoint.rotation);
        // 퍼즐 구역에서는 다시 추적하지 않도록 유지
        npcFollower.SetFollow(false);
        // 필요하면 여기서 대사 출력, 이펙트, 포탈 활성화 등을 처리
        Debug.Log("NPC가 먼저 이동했다.");
        // 동굴 진입 상태 기록
        if (GameManager_Ep1.Instance != null)
        {
            GameManager_Ep1.Instance.OnEnterCave();
        }
        // NPC 도착 후 플레이어 포탈 충돌 활성화
        if (portalCollider != null)
        {
            portalCollider.enabled = true;
        }
    }
}
