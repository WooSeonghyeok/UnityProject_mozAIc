using Episode3.Common;
using System.Collections.Generic;
using UnityEngine;

public class Ep3_2Manager : MonoBehaviour
{
    [Header("리듬 퍼즐 매니저")]
    [SerializeField] private RhythmPuzzleManager rhythmPuzzleManager;

    [Header("NPC 연출")]
    [SerializeField] private NPCFollower stageNpcFollower;
    [SerializeField] private Transform npcPuzzleWaitPoint;
    [SerializeField] private Transform npcClearWarpPoint;
    [SerializeField] private bool disableNpcFollowOnStart = true;

    [Header("퍼즐 시작 상호작용")]
    [SerializeField] private Ep3_2StartPuzzle startPuzzleController;

    [Header("출구 문 상호작용")]
    [SerializeField] private InteractableSymbol exitDoorInteractable;

    [Header("클리어 컷씬")]
    [SerializeField] private bool playClearCutsceneOnPuzzleComplete = false;
    [SerializeField] private CutsceneImagePlayer clearCutscenePlayer;
    [SerializeField] private int clearCutsceneStartIndex = 0;
    [SerializeField] private int clearCutsceneStepCount = 1;

    [Header("AI / 힌트 기록")]
    [SerializeField] private int hintCount = 0;
    [SerializeField] private int hintIntensity = 0;
    [SerializeField] private int aiInteractionCount = 0;

    [Header("획득 태그")]
    [SerializeField] private List<string> collectedTags = new List<string>();

    private bool isStageFinished = false;

    private void Start()
    {
        PrepareStageNpc();
        ResetStartPuzzleController();

        if (rhythmPuzzleManager == null)
        {
            Debug.LogWarning("[Ep3_2Manager] RhythmPuzzleManager가 연결되지 않았습니다.");
            return;
        }

        rhythmPuzzleManager.Initialize(this);

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
        Debug.Log("[Ep3_2Manager] 3-2 리듬 퍼즐 시작");
    }

    public void RequestHint(int intensity = 1)
    {
        hintCount++;
        hintIntensity += intensity;
        aiInteractionCount++;

        Debug.Log($"[Ep3_2Manager] 힌트 요청: {hintCount}, 강도 합: {hintIntensity}");
    }

    public void AddTag(string tag)
    {
        if (!collectedTags.Contains(tag))
        {
            collectedTags.Add(tag);
        }
    }

    public void OnRhythmPuzzleCompleted(int puzzleScore)
    {
        if (isStageFinished)
        {
            return;
        }

        isStageFinished = true;
        PersistPuzzleClearState();

        Ep3StageResult result = new Ep3StageResult
        {
            isCleared = true,
            relationScore = 0,
            puzzleScore = puzzleScore,
            emotionScore = 0,
            hintCount = hintCount,
            hintIntensity = hintIntensity,
            aiInteractionCount = aiInteractionCount,
            collectedTags = new List<string>(collectedTags)
        };

        if (Ep_3Manager.Instance != null)
        {
            Ep_3Manager.Instance.ReportStage3_2Result(result);
        }

        WarpNpcToClearPoint();
        PlayClearCutsceneIfNeeded();

        if (exitDoorInteractable != null)
        {
            exitDoorInteractable.enabled = true;
        }

        Debug.Log("[Ep3_2Manager] 3-2 클리어 - 출구 문 상호작용 가능");
    }

    private void PersistPuzzleClearState()
    {
        SaveDataObj saveData = null;

        if (SaveManager.instance != null)
        {
            if (SaveManager.instance.curData == null)
            {
                SaveManager.instance.curData = SaveManager.ReadCurJSON();
            }

            saveData = SaveManager.instance.curData;
        }
        else
        {
            saveData = SaveManager.ReadCurJSON();
        }

        if (saveData == null)
        {
            Debug.LogWarning("[Ep3_2Manager] SaveData를 찾지 못해 EP3 3-2 클리어 상태 저장을 건너뜁니다.");
            return;
        }

        saveData.ep3_jumpClear = true;
        SaveManager.WriteCurJSON(saveData);

        if (SaveManager.instance != null)
        {
            SaveManager.instance.curData = saveData;
        }
    }

    public void OnRhythmPuzzleFailed()
    {
        ResetStartPuzzleController();
        Debug.Log("[Ep3_2Manager] 3-2 실패");
    }

    private void PrepareStageNpc()
    {
        if (stageNpcFollower == null)
        {
            return;
        }

        if (disableNpcFollowOnStart)
        {
            stageNpcFollower.SetFollow(false);
        }

        if (npcPuzzleWaitPoint != null)
        {
            stageNpcFollower.WarpTo(npcPuzzleWaitPoint.position, npcPuzzleWaitPoint.rotation);
        }
    }

    private void WarpNpcToClearPoint()
    {
        if (stageNpcFollower == null || npcClearWarpPoint == null)
        {
            return;
        }

        stageNpcFollower.SetFollow(false);
        stageNpcFollower.WarpTo(npcClearWarpPoint.position, npcClearWarpPoint.rotation);
    }

    private void PlayClearCutsceneIfNeeded()
    {
        if (!playClearCutsceneOnPuzzleComplete || clearCutscenePlayer == null)
        {
            return;
        }

        if (clearCutscenePlayer.IsPlaying)
        {
            return;
        }

        clearCutscenePlayer.PlayCutsceneSegment(clearCutsceneStartIndex, clearCutsceneStepCount);
    }

    private void ResetStartPuzzleController()
    {
        if (startPuzzleController == null)
        {
            return;
        }

        startPuzzleController.ResetStartSequence();
    }
}
