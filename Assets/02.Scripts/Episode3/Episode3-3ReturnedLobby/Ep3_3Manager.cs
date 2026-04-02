using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 에피소드 3-3 스테이지 전용 매니저.
/// 
/// 역할:
/// 1. 힌트/AI 상호작용/태그 기록
/// 2. 3-3 완료 시 최종 결과를 Ep_3Manager에 보고
/// 3. 전체 엔딩 판정을 실행
/// 4. 엔딩 컨트롤러에 최종 결과 전달
/// </summary>
public class Ep3_3Manager : MonoBehaviour
{
    [Header("엔딩 컨트롤러")]
    [SerializeField] private Ep_3EndingController endingController;
    [Header("기록")]
    [SerializeField] private int hintCount = 0;
    [SerializeField] private int hintIntensity = 0;
    [SerializeField] private int aiInteractionCount = 0;
    [Header("획득 태그")]
    [SerializeField] private List<string> collectedTags = new List<string>();
    /// <summary>
    /// 완료 처리 중복 실행 방지.
    /// 엔딩 재생이 두 번 이상 호출되지 않도록 막는다.
    /// </summary>
    private bool isFinished = false;
    /// <summary>
    /// 힌트 요청 기록.
    /// </summary>
    public void RequestHint(int intensity = 1)
    {
        hintCount++;
        hintIntensity += intensity;
        aiInteractionCount++;
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
    /// 3-3 완료 처리.
    /// 
    /// 처리 흐름:
    /// 1. 자신의 스테이지 결과를 Ep_3Manager에 보고
    /// 2. 전체 엔딩 판정 실행
    /// 3. 판정 결과를 엔딩 컨트롤러에 전달
    /// </summary>
    public void CompleteStage3_3()
    {
        if (isFinished) return;
        isFinished = true;
        SaveManager.instance.curData.ep4_open = true;
        Ep3StageResult result = new Ep3StageResult();
        result.isCleared = true;
        result.relationScore = 10;
        result.puzzleScore = 0;
        result.emotionScore = 15;
        result.hintCount = hintCount;
        result.hintIntensity = hintIntensity;
        result.aiInteractionCount = aiInteractionCount;
        result.collectedTags = new List<string>(collectedTags);
        if (Ep_3Manager.Instance != null)
        {
            Ep_3Manager.Instance.ReportStage3_3Result(result);

            Ep3EndingStateData endingData = Ep_3Manager.Instance.EvaluateEnding();

            if (endingController != null)
            {
                endingController.PlayEnding(endingData);
            }
        }
        Debug.Log("[Ep3_3Manager] 3-3 완료 및 엔딩 진입");
    }
}