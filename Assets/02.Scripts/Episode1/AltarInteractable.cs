using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AltarInteractable : MonoBehaviour
{
    [Header("참조")]
    public AltarPuzzleManager puzzleManager;
    public GameObject interactionUI;   // "E" 상호작용 UI
    [Header("상호작용 설정")]
    public string playerTag = "Player";
    public PlayerInput user;
    private PlayerStarCollector currentPlayerCollector;
    private bool playerInRange;
    private void Awake()
    {
        user = GameObject.FindGameObjectWithTag(playerTag).GetComponent<PlayerInput>();
    }
    private void Start()
    {
        user.Interact += Interact;
        // 시작 시 UI 꺼두기
        if (interactionUI != null)
            interactionUI.SetActive(false);
    }
    private void Update()
    {
        // 플레이어가 범위 안에 없으면 입력 무시
        if (!playerInRange) return;
        // 퍼즐이 열려 있으면 안내 UI는 계속 숨김
        if (puzzleManager != null && puzzleManager.isPuzzleOpen)
        {
            if (interactionUI != null)  interactionUI.SetActive(false);
            return;
        }
        // 퍼즐이 닫혀 있고 범위 안이면 안내 UI 표시
        if (interactionUI != null)
        {
            interactionUI.SetActive(true);
        }
    }
    public void Interact()  // 제단 상호작용 실행
    {
        if (puzzleManager.isPuzzleCleared) return;
        if (currentPlayerCollector == null || puzzleManager == null) return;
        if (puzzleManager.isPuzzleOpen) return; // 이미 퍼즐이 열려 있으면 무시
        if (interactionUI != null) interactionUI.SetActive(false);  // 안내 UI 즉시 숨김
        puzzleManager.OpenPuzzle(currentPlayerCollector.GetCollectedStars());
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return; // 플레이어가 아니면 무시
        if (puzzleManager.isPuzzleCleared) return;
        // 플레이어 수집 컴포넌트 캐싱
        PlayerStarCollector collector = other.GetComponent<PlayerStarCollector>();
        if (collector == null) return;
        playerInRange = true;
        currentPlayerCollector = collector;
        // 퍼즐이 닫혀 있을 때만 안내 UI 표시
        if (interactionUI != null && (puzzleManager == null || !puzzleManager.isPuzzleOpen)) interactionUI.SetActive(true);
    }
    private void OnTriggerExit(Collider other)
    {
        // 플레이어가 아니면 무시
        if (!other.CompareTag(playerTag)) return;
        playerInRange = false;
        currentPlayerCollector = null;
        // 안내 UI 끄기
        if (interactionUI != null) interactionUI.SetActive(false);
    }
}
