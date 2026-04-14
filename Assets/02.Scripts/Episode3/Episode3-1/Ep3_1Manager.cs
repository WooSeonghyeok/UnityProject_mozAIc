using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
/// <summary>
/// 3-1 스테이지 진행 관리 (수정: 모든 조각 수집 이벤트 추가)
/// </summary>
public class Ep3_1Manager : MonoBehaviour
{
    [Header("다음 씬 이름")]
    [SerializeField] private string nextSceneName = "Ep3_2_Scene";
    [Header("조각/수집")]
    [SerializeField] private int collectedPieceCount = 0;
    [SerializeField] private int requiredPieceCount = 10;
    [Header("옵션 이벤트")]
    [Tooltip("모든 조각을 수집했을 때 호출되는 이벤트. PuzzleComplete가 구독합니다.")]
    public UnityEvent onAllPiecesCollected;
    // ... (나머지 기존 필드는 그대로 유지)
    [Header("힌트/AI")]
    [SerializeField] private int hintCount = 0;
    [SerializeField] private int hintIntensity = 0;
    [SerializeField] private int aiInteractionCount = 0;
    [Header("태그")]
    [SerializeField] private List<string> collectedTags = new List<string>();
    private bool isCleared = false;
    private bool _allPiecesEventRaised = false;  // 이벤트가 중복 호출되지 않도록 보호

    private void Start()
    {
        Ep_3Manager.Instance?.MarkStage3_1Visited();
    }

    /// <summary>
    /// 조각 하나를 획득했을 때 호출된다.
    /// 
    /// 현재 수집 개수를 올리고,
    /// 목표 수량에 도달하면 자동으로 스테이지를 클리어한다.
    /// </summary>
    public void AddPiece()
    {
        collectedPieceCount++;
        Debug.Log($"[Ep3_1Manager] 악보 조각 수집: {collectedPieceCount}/{requiredPieceCount}");

        if (collectedPieceCount >= requiredPieceCount && !_allPiecesEventRaised)
        {
            _allPiecesEventRaised = true;
            MarkPaperPuzzleCleared();
            // 인스펙터에서 연결한 리스너들을 호출 (PuzzleComplete 등)
            try
            {
                onAllPiecesCollected?.Invoke();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[Ep3_1Manager] onAllPiecesCollected 호출 중 예외: {ex.Message}");
            }

            // 완료 처리는 외부(예: PuzzleComplete)가 Ep3_1Manager.CompleteStage()를 호출하도록 위임합니다.
            // 기존에는 여기서 바로 CompleteStage()를 호출했습니다.
        }
    }
    private void MarkPaperPuzzleCleared()
    {
        SaveDataObj data = SaveManager.instance != null ? SaveManager.instance.curData : SaveManager.ReadCurJSON();
        if (data == null)
        {
            Debug.LogWarning("[Ep3_1Manager] SaveData를 찾을 수 없어 ep3_paperClear 저장을 건너뜁니다.");
            return;
        }

        data.ep3_paperClear = true;

        if (SaveManager.instance != null)
        {
            SaveManager.instance.curData = data;
            SaveManager.instance.WriteCurJSON();
            return;
        }

        SaveManager.WriteCurJSON(data);
        Debug.LogWarning("[Ep3_1Manager] SaveManager 인스턴스가 없어 정적 저장 경로로 ep3_paperClear를 기록했습니다.");
    }
    /// <summary>
    /// 힌트 요청 기록.
    /// intensity는 힌트 강도를 누적 기록하는 용도다.
    /// </summary>
    public void RequestHint(int intensity = 1)
    {
        hintCount++;
        hintIntensity += intensity;
        aiInteractionCount++;
        Debug.Log($"[Ep3_1Manager] 힌트 요청: {hintCount}, 강도: {hintIntensity}");
    }
    /// <summary>
    /// 중복 없이 태그를 기록한다.
    /// </summary>
    public void AddTag(string tag)
    {
        if (!collectedTags.Contains(tag))
        {
            collectedTags.Add(tag);
        }
    }
    /// <summary>
    /// 스테이지 클리어 처리.
    /// 
    /// 현재까지의 진행 결과를 Ep3StageResult에 담아 Ep_3Manager에 보고하고,
    /// 이후 다음 씬으로 이동한다.
    /// </summary>
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
        Debug.Log("[Ep3_1Manager] 3-1 클리어 처리 완료");
        SceneManager.LoadScene(nextSceneName);
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
