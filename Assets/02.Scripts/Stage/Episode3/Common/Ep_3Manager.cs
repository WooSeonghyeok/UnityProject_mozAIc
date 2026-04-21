using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 에피소드 3 전체 진행 결과를 통합 관리하는 싱글톤 매니저.
/// </summary>
public class Ep_3Manager : MonoBehaviour
{
    /// <summary>
    /// 에피소드 3 전체에서 단 하나만 유지되는 싱글톤 인스턴스.
    /// </summary>
    public static Ep_3Manager Instance { get; private set; }
    [Header("스테이지 결과")]
    [SerializeField] private Ep3StageResult stage3_1Result = new Ep3StageResult();
    [SerializeField] private Ep3StageResult stage3_2Result = new Ep3StageResult();
    [SerializeField] private Ep3StageResult stage3_3Result = new Ep3StageResult();
    [Header("진엔딩 필수 태그")]
    [SerializeField]
    private List<string> requiredTrueEndingTags = new List<string>()
    {
        "shared_childhood",
        "star_promise",
        "shared_dream",
        "co_creation",
        "unfinished_confession",
        "lover_memory",
        "self_voice",
        "split_self"
    };
    [Header("엔딩 결과")]
    [SerializeField] private Ep3EndingType currentEndingType = Ep3EndingType.None;
    [SerializeField] private Ep3EndingStateData cachedEndingStateData;
    private bool hasVisitedStage3_1 = false;
    /// <summary>
    /// 현재 캐시된 엔딩 판정 결과가 유효한지 여부.
    /// </summary>
    private bool isEvaluated = false;
    public Ep3EndingType CurrentEndingType => currentEndingType;
    public Ep3EndingStateData CachedEndingStateData => cachedEndingStateData;
    public bool IsEvaluated => isEvaluated;
    public bool HasVisitedStage3_1 => hasVisitedStage3_1;
    [Header("기억 재구성 점수 계산")]
    public int Ep3_1puzzleLoss = 0;  // 3-1 퍼즐 점수 감점 누적값
    public int Ep3_2restarted = 0;  // 3-2 퍼즐 재시작 횟수 추적 (점수 감점용)
    /// <summary>
    /// 싱글톤 초기화.
    /// 이미 다른 인스턴스가 있으면 자신은 파괴하고,
    /// 최초 인스턴스만 씬 전환 후에도 유지한다.
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        RemoveDuplicateAudioListeners();  // 씬 로드/유지 시 중복 AudioListener가 있으면 런타임에서 정리
    }
    private void RemoveDuplicateAudioListeners()
    {
        var listeners = FindObjectsOfType<AudioListener>();
        if (listeners == null || listeners.Length <= 1) return;
        bool kept = false;
        foreach (var l in listeners)
        {
            if (!kept && l.enabled)
            {
                // 첫 번째 활성화된 리스너는 보존
                kept = true;
                continue;
            }
            Destroy(l);  // 중복 리스너는 컴포넌트만 제거
        }

        Debug.Log($"[Ep_3Manager] 중복 AudioListener 정리 완료. 남은 리스너 수: {FindObjectsOfType<AudioListener>().Length}");
    }
    // -----------------------------
    // 로비/AI 공통 기록
    // -----------------------------

    /// <summary>
    /// AI 상호작용 횟수를 기록하는 확장 지점.
    /// 
    /// 현재는 로그만 남기지만,
    /// 나중에 전체 통계나 호감도/연출 분기와 연결할 수 있다.
    /// </summary>
    public void AddAIInteraction(int count = 1)
    {
        Debug.Log($"[Ep_3Manager] AI 상호작용 +{count}");
    }

    public void MarkStage3_1Visited()
    {
        if (hasVisitedStage3_1) return;

        hasVisitedStage3_1 = true;
        Debug.Log("[Ep_3Manager] 3-1 진입 기록");
    }
    // -----------------------------
    // 3-1 결과 보고
    // -----------------------------

    /// <summary>
    /// 3-1 스테이지 결과를 저장한다.
    /// 결과가 바뀌면 엔딩 캐시는 무효화한다.
    /// </summary>
    public void ReportStage3_1Result(Ep3StageResult result)
    {
        stage3_1Result = result;
        InvalidateEndingEvaluation();
        Debug.Log("[Ep_3Manager] 3-1 결과 저장 완료");
    }
    // -----------------------------
    // 3-2 결과 보고
    // -----------------------------

    /// <summary>
    /// 3-2 스테이지 결과를 저장한다.
    /// 결과가 바뀌면 엔딩 캐시는 무효화한다.
    /// </summary>
    public void ReportStage3_2Result(Ep3StageResult result)
    {
        stage3_2Result = result;
        InvalidateEndingEvaluation();
        Debug.Log("[Ep_3Manager] 3-2 결과 저장 완료");
    }
    // -----------------------------
    // 3-3 결과 보고
    // -----------------------------

    /// <summary>
    /// 3-3 스테이지 결과를 저장한다.
    /// 결과가 바뀌면 엔딩 캐시는 무효화한다.
    /// </summary>
    public void ReportStage3_3Result(Ep3StageResult result)
    {
        stage3_3Result = result;
        InvalidateEndingEvaluation();
        Debug.Log("[Ep_3Manager] 3-3 결과 저장 완료");
    }

    // -----------------------------
    // 엔딩 판정
    // -----------------------------

    /// <summary>
    /// 현재까지 누적된 스테이지 결과를 합산해 엔딩을 판정한다.
    /// 
    /// 판정 순서:
    /// 1. 관계/퍼즐/감정 점수 합산
    /// 2. 힌트/AI 상호작용 횟수 합산
    /// 3. 태그를 모두 합쳐 중복 제거
    /// 4. 진엔딩 필수 태그 보유 여부 확인
    /// 5. 기억 재구성률과 태그 충족 여부를 기준으로 엔딩 결정
    /// 
    /// 이미 평가가 끝났고 캐시가 있으면 재계산하지 않고 캐시를 반환한다.
    /// </summary>
    public Ep3EndingStateData EvaluateEnding()
    {
        if (isEvaluated && cachedEndingStateData != null)
        {
            return cachedEndingStateData;
        }
        Ep3EndingStateData data = new Ep3EndingStateData();
        data.totalRelationScore =
            stage3_1Result.relationScore +
            stage3_2Result.relationScore +
            stage3_3Result.relationScore;
        data.totalPuzzleScore =
            stage3_1Result.puzzleScore +
            stage3_2Result.puzzleScore +
            stage3_3Result.puzzleScore;
        data.totalEmotionScore =
            stage3_1Result.emotionScore +
            stage3_2Result.emotionScore +
            stage3_3Result.emotionScore;
        data.totalHintCount =
            stage3_1Result.hintCount +
            stage3_2Result.hintCount +
            stage3_3Result.hintCount;
        data.totalHintIntensity =
            stage3_1Result.hintIntensity +
            stage3_2Result.hintIntensity +
            stage3_3Result.hintIntensity;
        data.totalAIInteractionCount =
            stage3_1Result.aiInteractionCount +
            stage3_2Result.aiInteractionCount +
            stage3_3Result.aiInteractionCount;
        HashSet<string> tagSet = new HashSet<string>();
        AddTagsToSet(tagSet, stage3_1Result.collectedTags);
        AddTagsToSet(tagSet, stage3_2Result.collectedTags);
        AddTagsToSet(tagSet, stage3_3Result.collectedTags);
        data.collectedTags = new List<string>(tagSet);
        // 현재 구조에서는 기억 재구성률을 세 종류 점수 합으로 계산한다.
        // 이후 밸런스 조정이 필요하면 가중치를 두는 방식으로 확장할 수 있다.
        data.totalMemoryReconstructionRate =
            data.totalRelationScore +
            data.totalPuzzleScore +
            data.totalEmotionScore;
        bool hasAllRequiredTags = true;
        foreach (string requiredTag in requiredTrueEndingTags)
        {
            if (!tagSet.Contains(requiredTag))
            {
                hasAllRequiredTags = false;
                data.missingRequiredTags.Add(requiredTag);
            }
        }
        if (data.totalMemoryReconstructionRate >= 80 && hasAllRequiredTags)
        {
            data.endingType = Ep3EndingType.True;
        }
        else
        {
            data.endingType = Ep3EndingType.Normal;
        }
        currentEndingType = data.endingType;
        cachedEndingStateData = data;
        isEvaluated = true;
        Debug.Log($"[Ep_3Manager] 엔딩 판정 완료: {currentEndingType}");
        Debug.Log($"[Ep_3Manager] 기억 재구성률: {data.totalMemoryReconstructionRate}");
        return data;
    }
    /// <summary>
    /// 태그 목록을 HashSet에 추가한다.
    /// null 방어를 함께 처리해 호출부가 간단하게 유지되도록 한다.
    /// </summary>
    private void AddTagsToSet(HashSet<string> set, List<string> tags)
    {
        if (tags == null) return;

        foreach (string tag in tags)
        {
            set.Add(tag);
        }
    }
    /// <summary>
    /// 엔딩 판정 캐시를 무효화한다.
    /// 
    /// 스테이지 결과가 변경되면 이전 엔딩 계산은 더 이상 믿을 수 없으므로
    /// 반드시 다시 평가하도록 상태를 초기화한다.
    /// </summary>
    private void InvalidateEndingEvaluation()
    {
        isEvaluated = false;
        currentEndingType = Ep3EndingType.None;
        cachedEndingStateData = null;
    }
    /// <summary>
    /// 모든 스테이지가 클리어 상태로 보고되었는지 확인한다.
    /// 최종 엔딩 화면 진입 가능 여부를 체크할 때 사용할 수 있다.
    /// </summary>
    public bool IsAllStageReported()
    {
        return stage3_1Result.isCleared &&
               stage3_2Result.isCleared &&
               stage3_3Result.isCleared;
    }
    /// <summary>
    /// 디버그용 전체 상태 초기화.
    /// 인스펙터 우클릭 ContextMenu에서 직접 호출 가능하다.
    /// </summary>
    [ContextMenu("디버그 - 상태 초기화")]
    public void ResetEpisode3Data()
    {
        stage3_1Result = new Ep3StageResult();
        stage3_2Result = new Ep3StageResult();
        stage3_3Result = new Ep3StageResult();
        hasVisitedStage3_1 = false;
        InvalidateEndingEvaluation();
        Debug.Log("[Ep_3Manager] 에피소드 3 데이터 초기화");
    }
}
