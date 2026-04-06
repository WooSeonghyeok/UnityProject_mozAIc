using System.Collections.Generic;

/// <summary>
/// 에피소드 3의 최종 엔딩 타입.
/// None:
/// - 아직 판정되지 않음
/// Normal:
/// - 일반 엔딩
/// True:
/// - 진엔딩
/// </summary>
public enum Ep3EndingType
{
    None,
    Normal,
    True
}

/// <summary>
/// 개별 스테이지 하나의 결과를 담는 데이터.
/// 
/// 이 구조는 3-1, 3-2, 3-3 모두 공통 포맷으로 사용된다.
/// 각 스테이지 매니저는 자신의 결과를 이 구조에 담아 Ep_3Manager에 보고한다.
/// </summary>
[System.Serializable]
public class Ep3StageResult
{
    /// <summary>
    /// 해당 스테이지가 정상 클리어되었는지 여부.
    /// </summary>
    public bool isCleared = false;

    /// <summary>
    /// 관계/퍼즐/감정 점수.
    /// 엔딩 판정 시 각각 합산된다.
    /// </summary>
    public int relationScore = 0;
    public int puzzleScore = 0;
    public int emotionScore = 0;

    /// <summary>
    /// 힌트 사용량 및 AI 상호작용 기록.
    /// 플레이 분석, 엔딩 분기, 후속 통계에 활용 가능하다.
    /// </summary>
    public int hintCount = 0;
    public int hintIntensity = 0;
    public int aiInteractionCount = 0;

    /// <summary>
    /// 스테이지 플레이 중 획득한 태그.
    /// 진엔딩 조건 확인 시 전체 스테이지 태그를 합쳐 검사한다.
    /// </summary>
    public List<string> collectedTags = new List<string>();
}

/// <summary>
/// 에피소드 전체 결과를 집계한 뒤 최종 엔딩 판정에 사용하는 데이터.
/// 
/// Ep_3Manager.EvaluateEnding()이 이 구조를 만들어 반환한다.
/// </summary>
[System.Serializable]
public class Ep3EndingStateData
{
    /// <summary>
    /// 최종 판정된 엔딩 타입.
    /// </summary>
    public Ep3EndingType endingType = Ep3EndingType.None;

    /// <summary>
    /// 각 점수 총합.
    /// totalMemoryReconstructionRate는 현재 구조상 세 점수 합으로 계산한다.
    /// </summary>
    public int totalRelationScore = 0;
    public int totalPuzzleScore = 0;
    public int totalEmotionScore = 0;
    public int totalMemoryReconstructionRate = 0;

    /// <summary>
    /// 전체 힌트/AI 상호작용 누적치.
    /// </summary>
    public int totalHintCount = 0;
    public int totalHintIntensity = 0;
    public int totalAIInteractionCount = 0;

    /// <summary>
    /// 전체 스테이지에서 수집된 태그와,
    /// 진엔딩에 필요한데 아직 부족한 태그 목록.
    /// </summary>
    public List<string> collectedTags = new List<string>();
    public List<string> missingRequiredTags = new List<string>();
}