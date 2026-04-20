using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FinalPianoInteraction : MonoBehaviour
{
    [Header("컷씬 재생기")]
    [SerializeField] private CutsceneImagePlayer cutscenePlayer;
    [SerializeField] private GameObject cutscenePanel;
    [SerializeField] private Image cutsceneImage;
    [SerializeField] private FinalDialogueController finalDialogueController;
    [SerializeField] private bool autoResolveSceneReferences = true;
    [SerializeField] private bool unlockExitAfterCutscene = true;
    [SerializeField] private bool fallbackToMusicIfCutsceneUnavailable = true;

    [Header("오디오 재생기")]
    [SerializeField] private AudioSource audioSource;

    [Header("재생할 곡 2개")]
    [SerializeField] private AudioClip musicA;
    [SerializeField] private AudioClip musicB;

    [Header("재생 설정")]
    [SerializeField] private bool stopCurrentMusicBeforePlay = true;
    [SerializeField] private bool allowReplayWhilePlaying = false;
    [SerializeField] private bool playOnlyOnce = true;

    [Header("재생 종료 후 호출할 이벤트")]
    [SerializeField] private UnityEvent onMusicFinished;

    [Header("디버그")]
    [SerializeField] private bool debugLog = true;

    private bool hasPlayed = false;
    private bool isSequenceRunning = false;
    private Coroutine musicWaitRoutine;

    private void Awake()
    {
        ResolveReferences();
        BindCutsceneFinished();
    }

    private void OnDestroy()
    {
        if (cutscenePlayer != null)
        {
            cutscenePlayer.RemoveFinishedListener(HandleCutsceneFinished);
        }
    }

    public void PlayRandomMusic()
    {
        if (playOnlyOnce && hasPlayed)
        {
            if (debugLog)
            {
                Debug.Log("[FinalPianoInteraction] 이미 최종 연주가 실행되어 다시 재생하지 않습니다.");
            }
            return;
        }

        if (audioSource == null)
        {
            Debug.LogWarning("[FinalPianoInteraction] AudioSource가 연결되지 않았습니다.");
            return;
        }

        if (musicA == null && musicB == null)
        {
            Debug.LogWarning("[FinalPianoInteraction] 재생할 AudioClip이 없습니다.");
            return;
        }

        if (!allowReplayWhilePlaying && audioSource.isPlaying)
        {
            if (debugLog)
            {
                Debug.Log("[FinalPianoInteraction] 이미 곡이 재생 중이므로 다시 재생하지 않습니다.");
            }
            return;
        }

        AudioClip selectedClip = null;

        if (musicA != null && musicB != null)
        {
            selectedClip = Random.value < 0.5f ? musicA : musicB;
        }
        else if (musicA != null)
        {
            selectedClip = musicA;
        }
        else
        {
            selectedClip = musicB;
        }

        if (stopCurrentMusicBeforePlay)
        {
            audioSource.Stop();
        }

        audioSource.clip = selectedClip;
        audioSource.Play();
        hasPlayed = true;

        if (musicWaitRoutine != null)
        {
            StopCoroutine(musicWaitRoutine);
        }

        musicWaitRoutine = StartCoroutine(CoWaitMusicEnd(selectedClip.length));

        if (debugLog)
        {
            Debug.Log($"[FinalPianoInteraction] 랜덤 곡 재생: {selectedClip.name}");
        }

        PuzzleScore();
    }

    public void PuzzleScore()
    {
        if (Ep_3Manager.Instance == null || SaveManager.instance == null || SaveManager.instance.curData == null)
        {
            return;
        }

        const int ep3PuzzleBase = 10;
        int ep3PuzzleLoss = Ep_3Manager.Instance.Ep3_1puzzleLoss + Ep_3Manager.Instance.Ep3_2restarted;
        SaveManager.instance.curData.memory_reconstruction_rate[8] = ep3PuzzleBase - ep3PuzzleLoss;
    }

    public void PlayCutsceneThenMusic()
    {
        if (isSequenceRunning)
        {
            if (debugLog)
            {
                Debug.Log("[FinalPianoInteraction] 최종 연출이 이미 진행 중입니다.");
            }
            return;
        }

        if (playOnlyOnce && hasPlayed)
        {
            if (debugLog)
            {
                Debug.Log("[FinalPianoInteraction] 이미 최종 연출이 완료되어 다시 재생하지 않습니다.");
            }
            return;
        }

        ResolveReferences();
        BindCutsceneFinished();
        isSequenceRunning = true;

        if (finalDialogueController != null)
        {
            finalDialogueController.LockExit();
        }

        bool canPlayCutscene = cutscenePlayer != null
            && cutscenePanel != null
            && cutsceneImage != null
            && cutscenePlayer.HasConfiguredImages;

        if (canPlayCutscene)
        {
            if (debugLog)
            {
                Debug.Log("[FinalPianoInteraction] 피아노 컷씬 재생 시작");
            }

            cutscenePlayer.PlayCutscene();
            return;
        }

        if (debugLog)
        {
            Debug.LogWarning("[FinalPianoInteraction] 컷씬 참조가 부족해 음악 재생으로 대체합니다.");
        }

        if (fallbackToMusicIfCutsceneUnavailable)
        {
            HandleCutsceneFinished();
            return;
        }

        isSequenceRunning = false;
    }

    public void StopMusic()
    {
        if (audioSource == null)
        {
            return;
        }

        audioSource.Stop();

        if (musicWaitRoutine != null)
        {
            StopCoroutine(musicWaitRoutine);
            musicWaitRoutine = null;
        }

        if (debugLog)
        {
            Debug.Log("[FinalPianoInteraction] 곡 재생 중지");
        }
    }

    private void HandleCutsceneFinished()
    {
        PlayRandomMusic();
        MarkEpisode4Open();

        if (unlockExitAfterCutscene && finalDialogueController != null)
        {
            finalDialogueController.OnFinalDialogueEnd();
        }

        isSequenceRunning = false;
    }

    private void ResolveReferences()
    {
        if (cutscenePlayer == null)
        {
            cutscenePlayer = GetComponent<CutsceneImagePlayer>();
        }

        if (finalDialogueController == null)
        {
            finalDialogueController = FindSceneComponent<FinalDialogueController>();
        }

        if (!autoResolveSceneReferences)
        {
            if (cutscenePlayer != null)
            {
                cutscenePlayer.ApplyExternalUiRefs(cutscenePanel, cutsceneImage);
            }
            return;
        }

        if (cutscenePanel == null)
        {
            cutscenePanel = FindSceneGameObject("Canvas_PianoCutScene");
        }

        if (cutsceneImage == null && cutscenePanel != null)
        {
            cutsceneImage = FindBestCutsceneImage(cutscenePanel);
        }

        if (cutscenePlayer != null)
        {
            cutscenePlayer.ApplyExternalUiRefs(cutscenePanel, cutsceneImage);
        }
    }

    private static Image FindBestCutsceneImage(GameObject panelRoot)
    {
        if (panelRoot == null)
        {
            return null;
        }

        Image[] images = panelRoot.GetComponentsInChildren<Image>(true);
        Image bestImage = null;
        int bestScore = int.MinValue;

        foreach (Image candidate in images)
        {
            if (candidate == null || candidate.gameObject == panelRoot)
            {
                continue;
            }

            int score = GetCutsceneImageScore(candidate, panelRoot.transform);
            if (score <= bestScore)
            {
                continue;
            }

            bestScore = score;
            bestImage = candidate;
        }

        return bestImage;
    }

    private static int GetCutsceneImageScore(Image candidate, Transform panelRoot)
    {
        int score = 0;

        if (candidate.sprite == null)
        {
            score += 100;
        }

        if (candidate.transform.parent != null && candidate.transform.parent != panelRoot)
        {
            score += 50;
        }

        if (candidate.type == Image.Type.Simple)
        {
            score += 20;
        }

        score += GetTransformDepth(candidate.transform, panelRoot);
        return score;
    }

    private static int GetTransformDepth(Transform target, Transform stopAt)
    {
        int depth = 0;
        Transform current = target;

        while (current != null && current != stopAt)
        {
            depth++;
            current = current.parent;
        }

        return depth;
    }

    private void BindCutsceneFinished()
    {
        if (cutscenePlayer == null)
        {
            return;
        }

        cutscenePlayer.RemoveFinishedListener(HandleCutsceneFinished);
        cutscenePlayer.AddFinishedListener(HandleCutsceneFinished);
    }

    private void MarkEpisode4Open()
    {
        SaveDataObj saveData = SaveManager.instance != null
            ? SaveManager.instance.curData
            : SaveManager.ReadCurJSON();

        if (saveData == null)
        {
            Debug.LogWarning("[FinalPianoInteraction] SaveData를 찾지 못해 ep4_open 저장을 건너뜁니다.");
            return;
        }

        saveData.ep4_open = true;
        SaveManager.WriteCurJSON(saveData);

        if (SaveManager.instance != null)
        {
            SaveManager.instance.curData = saveData;
        }

        if (debugLog)
        {
            Debug.Log("[FinalPianoInteraction] ep4_open 저장 완료");
        }
    }

    private static T FindSceneComponent<T>() where T : Component
    {
        T[] components = Resources.FindObjectsOfTypeAll<T>();
        foreach (T component in components)
        {
            if (component == null || !component.gameObject.scene.IsValid())
            {
                continue;
            }

            return component;
        }

        return null;
    }

    private static GameObject FindSceneGameObject(string objectName)
    {
        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (Transform target in transforms)
        {
            if (target == null || !target.gameObject.scene.IsValid())
            {
                continue;
            }

            if (target.name == objectName)
            {
                return target.gameObject;
            }
        }

        return null;
    }

    private System.Collections.IEnumerator CoWaitMusicEnd(float clipLength)
    {
        yield return new WaitForSeconds(clipLength);

        if (debugLog)
        {
            Debug.Log("[FinalPianoInteraction] 곡 재생 종료");
        }

        onMusicFinished?.Invoke();
    }
}
