using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class Ep4_Puzzle3Manager : MonoBehaviour
{
    [Header("조각/수집")]
    [SerializeField] private int collectedPieceCount = 0;
    [SerializeField] private int requiredPieceCount = 7;
    [Header("출력 UI")]
    public GameObject pieceBox;
    public Text pieceCnt;
    public TextboxCtrl_Ep4 cutscene;
    private bool isMidCutsceneOn;
    [Header("옵션 이벤트")]
    [Tooltip("모든 조각을 수집했을 때 호출되는 이벤트. PuzzleComplete가 구독합니다.")]
    public UnityEvent onAllPiecesCollected;
    [Header("힌트/AI")]
    [SerializeField] private int hintCount = 0;
    [SerializeField] private int hintIntensity = 0;
    [SerializeField] private int aiInteractionCount = 0;
    [Header("태그")]
    [SerializeField] private List<string> collectedTags = new List<string>();
    private bool isCleared = false;
    private bool _allPiecesEventRaised = false;  // 이벤트가 중복 호출되지 않도록 보호
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            pieceCnt.text = $"{collectedPieceCount} / {requiredPieceCount}";
            pieceBox.SetActive(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            pieceBox.SetActive(false);
        }
    }
    public void AddPiece()
    {
        collectedPieceCount++;
        Debug.Log($"[Ep4_3Manager] 악보 조각 수집: {collectedPieceCount}/{requiredPieceCount}");
        pieceCnt.text = $"{collectedPieceCount} / {requiredPieceCount}";
        if (collectedPieceCount == 1) cutscene.StartCoroutine(cutscene._manager.TalkSay(TextboxManager.TalkType.player, "분명 무언가의 일부인 소리."));  // 첫 조각 획득 시 대사 출력
        if ((float)(collectedPieceCount) / (float)(requiredPieceCount) >= 0.5f && !isMidCutsceneOn)
        {
            cutscene.StartCoroutine(cutscene._manager.TalkSay(TextboxManager.TalkType.player, "이건 멈춘 노래가 아니다. 끝을 기다리고 있던 노래다."));
            isMidCutsceneOn = true;
        }
        if (collectedPieceCount >= requiredPieceCount && !_allPiecesEventRaised)
        {
            _allPiecesEventRaised = true;
            if (SaveManager.instance != null) SaveManager.instance.curData.ep4_puzzle3Clear = true;
            try
            {
                onAllPiecesCollected?.Invoke();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[Ep4_3Manager] onAllPiecesCollected 호출 중 예외: {ex.Message}");
            }
        }
    }
    public void RequestHint(int intensity = 1)
    {
        hintCount++;
        hintIntensity += intensity;
        aiInteractionCount++;
        Debug.Log($"[Ep4_3Manager] 힌트 요청: {hintCount}, 강도: {hintIntensity}");
    }
    public void AddTag(string tag)
    {
        if (!collectedTags.Contains(tag))
        {
            collectedTags.Add(tag);
        }
    }
    public void CompleteStage()
    {
        if (isCleared) return;
        isCleared = true;
        Ep3StageResult result = new Ep3StageResult();
        result.isCleared = true;
        result.relationScore = 15;
        result.puzzleScore = 0;
        result.emotionScore = 10;
        result.hintCount = hintCount;
        result.hintIntensity = hintIntensity;
        result.aiInteractionCount = aiInteractionCount;
        result.collectedTags = new List<string>(collectedTags);
        if (Ep_3Manager.Instance != null)
        {
            Ep_3Manager.Instance.ReportStage3_1Result(result);
        }
        Debug.Log("[Ep3_1Manager] 4-3 클리어 처리 완료");
    }
    [ContextMenu("디버그 - 상태 초기화")]
    public void ResetState()
    {
        collectedPieceCount = 0;
        hintCount = 0;
        hintIntensity = 0;
        aiInteractionCount = 0;
        collectedTags.Clear();
        isCleared = false;
        _allPiecesEventRaised = false;
        Debug.Log("[Ep3_1Manager] 상태 초기화");
    }
}