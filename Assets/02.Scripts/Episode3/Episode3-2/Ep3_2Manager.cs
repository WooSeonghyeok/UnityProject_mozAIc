using Episode3.Common;
using System.Collections.Generic;
using UnityEngine;

public class Ep3_2Manager : MonoBehaviour
{
    [Header("퍼즐 매니저 참조")]
    [SerializeField] private RhythmPuzzleManager rhythmPuzzleManager;

    [Header("출구 문 상호작용")]
    [SerializeField] private InteractableSymbol exitDoorInteractable;

    [Header("AI / 힌트 기록")]
    [SerializeField] private int hintCount = 0;
    [SerializeField] private int hintIntensity = 0;
    [SerializeField] private int aiInteractionCount = 0;

    [Header("획득 태그")]
    [SerializeField] private List<string> collectedTags = new List<string>();

    // 동일 스테이지 완료 처리 중복 실행 방지용 플래그
    private bool isStageFinished = false;

    private void Start()
    {
        if (rhythmPuzzleManager == null)
        {
            Debug.LogWarning("[Ep3_2Manager] RhythmPuzzleManager가 연결되지 않았습니다.");
            return;
        }

        rhythmPuzzleManager.Initialize(this);

        // 시작 시 출구 문은 잠금 상태로 둔다.
        if (exitDoorInteractable != null)
        {
            exitDoorInteractable.enabled = false;
        }
    }

    public void StartRhythmStage()
    {
        if (rhythmPuzzleManager == null)
        {
            Debug.LogWarning("[Ep3_2Manager] RhythmPuzzleManager가 연결되지 않았습니다.");
            return;
        }

        rhythmPuzzleManager.StartPuzzle();
        Debug.Log("[Ep3_2Manager] 3-2 퍼즐 시작");
    }

    public void RequestHint(int intensity = 1)
    {
        hintCount++;
        hintIntensity += intensity;
        aiInteractionCount++;

        Debug.Log($"[Ep3_2Manager] 힌트 요청: {hintCount}, 강도합: {hintIntensity}");
    }

    public void AddTag(string tag)
    {
        if (!collectedTags.Contains(tag))
        {
            collectedTags.Add(tag);
        }
    }

    // 리듬 퍼즐 성공 시 호출된다.
    //
    // 여기서는 결과 저장과 출구 문 해금만 처리한다.
    // 다음 씬 이동은 플레이어가 문과 상호작용했을 때 진행된다.
    public void OnRhythmPuzzleCompleted(int puzzleScore)
    {
        if (isStageFinished) return;
        isStageFinished = true;

        Ep3StageResult result = new Ep3StageResult();
        result.isCleared = true;
        result.relationScore = 0;
        result.puzzleScore = puzzleScore;
        result.emotionScore = 0;
        result.hintCount = hintCount;
        result.hintIntensity = hintIntensity;
        result.aiInteractionCount = aiInteractionCount;
        result.collectedTags = new List<string>(collectedTags);

        if (Ep_3Manager.Instance != null)
        {
            Ep_3Manager.Instance.ReportStage3_2Result(result);
        }

        // 퍼즐 클리어 후 출구 문 상호작용 해금
        if (exitDoorInteractable != null)
        {
            exitDoorInteractable.enabled = true;
        }

        Debug.Log("[Ep3_2Manager] 3-2 클리어 - 출구 문 상호작용 가능");
    }

    public void OnRhythmPuzzleFailed()
    {
        Debug.Log("[Ep3_2Manager] 3-2 실패");
    }
}