using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class Ep3LobbyIntroSequenceData
{
    public string sequenceId;
    public List<Ep3LobbyIntroShotData> shots = new List<Ep3LobbyIntroShotData>();
}

[Serializable]
public class Ep3LobbyIntroShotData
{
    public string dialogueId;
    public string speakerType;
    public string speakerName;
    [TextArea(2, 4)] public string subtitleText;
    public bool showSpeakerName = true;
    public Vector3 cameraPosition;
    public Vector3 lookAtPosition;
    public float moveDuration;
    public float holdDuration;
    public float fieldOfView;

    public Ep3LobbyIntroShotData Clone()
    {
        return new Ep3LobbyIntroShotData
        {
            dialogueId = dialogueId,
            speakerType = speakerType,
            speakerName = speakerName,
            subtitleText = subtitleText,
            showSpeakerName = showSpeakerName,
            cameraPosition = cameraPosition,
            lookAtPosition = lookAtPosition,
            moveDuration = moveDuration,
            holdDuration = holdDuration,
            fieldOfView = fieldOfView
        };
    }
}

public enum Ep3IntroCutsceneSaveKey
{
    None = -1,
    EP3Lobby = 0,
    EP3Stage3_1 = 1,
    EP3Stage3_1Completion = 2,
    EP3Stage3_2Intro = 3,
    EP3ReturnedLobbyIntro = 4
}

/// <summary>
/// Plays the Episode 3 lobby intro once, using a Cinemachine Smooth Path and Tracked Dolly.
/// </summary>
[AddComponentMenu("Episode3/EP3 Lobby Intro Cutscene Controller")]
public class Ep3LobbyIntroCutsceneController : MonoBehaviour
{
    public const string SceneName = "Episode3_Scene";
    public SaveDataObj CurData;

    private const string PlayerTag = "Player";
    private const string DefaultResourcePath = "Data/ep3_lobby_intro_cutscene";
    private const string DefaultSequenceAssetPath = "Data/EP3LobbyIntroSequence";
    private const float DefaultLookDistance = 8f;
    private const string Stage3_1IntroSequenceId = "EP3_STAGE3_1_INTRO";
    private const string Stage3_1CompletionSequenceId = "EP3_STAGE3_1_COMPLETION";
    private const string Stage3_2IntroSequenceId = "EP3_STAGE3_2_INTRO";
    private const string ReturnedLobbyIntroSequenceId = "EP3_STAGE3_2_COMPLETION";

    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool playOnlyOncePerSession = false;
    [SerializeField] private string inspectorSequenceId = "EP3_LOBBY_INTRO";
    [SerializeField] private Ep3IntroCutsceneSaveKey playOnceSaveKey = Ep3IntroCutsceneSaveKey.EP3Lobby;
    [SerializeField] private bool useInspectorPlayedStateOverride = false;
    [SerializeField] private bool inspectorPlayedState = false;
    [SerializeField] private List<Ep3LobbyIntroShotData> inspectorShots = new List<Ep3LobbyIntroShotData>();
    [SerializeField] private string cutsceneResourcePath = DefaultResourcePath;
    [SerializeField] private string cutsceneAssetResourcePath = DefaultSequenceAssetPath;
    [SerializeField] private float startDelay = 0.75f;
    [SerializeField] private int introCameraPriority = 100;
    [SerializeField] private int pathResolution = 24;
    [SerializeField] private CinemachineVirtualCamera sceneIntroCamera;
    [SerializeField] private CinemachineSmoothPath sceneIntroPath;
    [SerializeField] private Transform sceneLookAtTarget;
    [SerializeField] private bool syncSceneRigWithSequence = true;
    [SerializeField] private bool enableSubtitles = true;
    [SerializeField] private TMP_FontAsset subtitleFont;
    [SerializeField] private bool forceStandaloneSubtitleOverlay = true;
    [SerializeField] private bool useRuntimeHintOverlay = false;
    [SerializeField] private Button nextButton;
    [SerializeField] private GameObject nextButtonRoot;
    [SerializeField] private Button skipButton;
    [SerializeField] private GameObject skipButtonRoot;
    [SerializeField] private UnityEvent onCutsceneFinished;

    private PlayerInput playerInput;
    private PlayerMovement playerMovement;
    private InteractManager interactManager;

    private bool previousInputEnabled;
    private bool previousMovementEnabled;
    private bool previousLookLock;
    private bool previousJumpLock;

    private CinemachineVirtualCamera gameplayCamera;
    private CinemachineVirtualCamera introCamera;
    private CinemachineTrackedDolly introTrackedDolly;
    private CinemachineSmoothPath introPath;
    private Transform lookAtTarget;
    private Ep3CutsceneSubtitlePresenter subtitlePresenter;

    private int originalGameplayPriority;
    private bool isPlaying;
    private bool ownsRuntimeRig;
    private bool destroyWhenFinished;
    private bool ownsSubtitlePresenter;
    private bool hasPlayedThisSession;

    private Vector3 initialCameraPosition;
    private Quaternion initialCameraRotation = Quaternion.identity;
    private float initialFieldOfView = 60f;
    private Vector3 initialLookAtPosition;
    private bool hasCapturedCameraState;
    private bool buttonRequested;
    private bool advanceRequested;
    private bool skipRequested;
    private CutsceneHintOverlay hintOverlay;

    public bool PlaysOnStart => playOnStart;
    public bool IsPlaying => isPlaying;

    public void InitializeAsRuntimeFallback()
    {
        destroyWhenFinished = true;
    }

    public void AddFinishedListener(UnityAction listener)
    {
        onCutsceneFinished?.AddListener(listener);
    }

    public void RemoveFinishedListener(UnityAction listener)
    {
        onCutsceneFinished?.RemoveListener(listener);
    }

    private void Reset()
    {
        ApplyDefaultInspectorShots();
    }

    private void Start()
    {
        CurData = ResolveSaveData();
        RefreshInspectorPlayedStateFromSave();
        InitializeCutsceneButtons();
        SetCutsceneButtonsVisible(false);

        if (playOnStart && !isPlaying)
        {
            StartCoroutine(BeginCutsceneCoroutine(false));
        }
    }

    public void PlayCutsceneManually()
    {
        if (!isActiveAndEnabled || isPlaying)
        {
            return;
        }

        if (playOnlyOncePerSession && hasPlayedThisSession)
        {
            return;
        }

        StartCoroutine(BeginCutsceneCoroutine(true));
    }

    public void RequestAdvance()
    {
        if (isPlaying)
        {
            advanceRequested = true;
            Debug.Log("cut advanced: 1");
        }
    }

    public void RequestSkip()
    {
        if (isPlaying)
        {
            skipRequested = true;
        }
    }

    public void OnNextButton()
    {
        buttonRequested = true;
        RequestAdvance();
    }

    public void OnSkipButton()
    {
        buttonRequested = true;
        RequestSkip();
    }

    private IEnumerator BeginCutsceneCoroutine(bool ignorePlayOnceCheck)
    {
        yield return null;

        if (!ignorePlayOnceCheck && !ShouldPlayCutscene())
        {
            FinishController();
            yield break;
        }
        SetGameCutsceneMode(true);
        Ep3LobbyIntroSequenceData sequence = LoadSequenceData();
        if (sequence == null || sequence.shots == null || sequence.shots.Count == 0)
        {
            Debug.LogWarning("[Ep3LobbyIntroCutsceneController] No intro cutscene data was found.");
            FinishController();
            yield break;
        }

        CachePlayerComponents();
        CacheGameplayCamera();
        CaptureCurrentCameraState();
        EnsureIntroRig(sequence);
        EnsureSubtitlePresenter();
        InitializeCutsceneButtons();
        if (useRuntimeHintOverlay)
        {
            EnsureHintOverlay();
        }

        if (playerMovement == null)
        {
            Debug.LogWarning("[Ep3LobbyIntroCutsceneController] PlayerMovement was not found, so the intro cutscene was skipped.");
            RestoreState();
            FinishController();
            yield break;
        }

        yield return new WaitForSeconds(startDelay);
        CacheGameplayCamera();
        CaptureCurrentCameraState();
        yield return PlayCutscene(sequence, !ignorePlayOnceCheck);

        if (ignorePlayOnceCheck)
        {
            hasPlayedThisSession = true;
        }

        FinishController();
    }

    private bool ShouldPlayCutscene()
    {
        if (playOnlyOncePerSession && hasPlayedThisSession)
        {
            return false;
        }

        if (useInspectorPlayedStateOverride)
        {
            return !inspectorPlayedState;
        }

        if (playOnceSaveKey == Ep3IntroCutsceneSaveKey.None)
        {
            return true;
        }

        if (useInspectorPlayedStateOverride)
        {
            return !inspectorPlayedState;
        }

        SaveDataObj data = ResolveSaveData();
        bool hasPlayed = HasPlayedCutscene(data);
        inspectorPlayedState = hasPlayed;
        return data != null && !hasPlayed;
    }

    private void CachePlayerComponents()
    {
        GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
        if (player == null)
        {
            return;
        }

        playerInput = player.GetComponent<PlayerInput>();
        playerMovement = player.GetComponent<PlayerMovement>();
        interactManager = player.GetComponent<InteractManager>();
    }

    private void CacheGameplayCamera()
    {
        CinemachineVirtualCamera[] cameras = FindObjectsOfType<CinemachineVirtualCamera>();
        foreach (CinemachineVirtualCamera candidate in cameras)
        {
            if (!IsGameplayCameraCandidate(candidate))
            {
                continue;
            }

            if (gameplayCamera == null || candidate.Priority > gameplayCamera.Priority)
            {
                gameplayCamera = candidate;
            }
        }

        if (gameplayCamera != null)
        {
            originalGameplayPriority = gameplayCamera.Priority;
        }
    }

    private bool IsGameplayCameraCandidate(CinemachineVirtualCamera candidate)
    {
        if (candidate == null || !candidate.isActiveAndEnabled)
        {
            return false;
        }

        if (candidate == introCamera || candidate == sceneIntroCamera)
        {
            return false;
        }

        return true;
    }

    private void CaptureCurrentCameraState()
    {
        hasCapturedCameraState = false;

        Transform currentCameraTransform = Camera.main != null ? Camera.main.transform : (gameplayCamera != null ? gameplayCamera.transform : null);
        if (currentCameraTransform != null)
        {
            hasCapturedCameraState = true;
            initialCameraPosition = currentCameraTransform.position;
            initialCameraRotation = currentCameraTransform.rotation;
            initialLookAtPosition = initialCameraPosition + (currentCameraTransform.forward * DefaultLookDistance);
        }

        if (Camera.main != null)
        {
            initialFieldOfView = Camera.main.fieldOfView;
        }
        else if (gameplayCamera != null)
        {
            initialFieldOfView = gameplayCamera.m_Lens.FieldOfView;
        }
    }

    private void EnsureIntroRig(Ep3LobbyIntroSequenceData sequence)
    {
        if (introCamera != null && introTrackedDolly != null && introPath != null && lookAtTarget != null)
        {
            return;
        }

        ownsRuntimeRig = false;

        if (sceneIntroCamera != null && sceneIntroPath != null && sceneLookAtTarget != null)
        {
            introCamera = sceneIntroCamera;
            introPath = sceneIntroPath;
            lookAtTarget = sceneLookAtTarget;
            introTrackedDolly = introCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
            if (introTrackedDolly == null)
            {
                introTrackedDolly = introCamera.AddCinemachineComponent<CinemachineTrackedDolly>();
            }

            if (introCamera.GetCinemachineComponent<CinemachineHardLookAt>() == null)
            {
                introCamera.AddCinemachineComponent<CinemachineHardLookAt>();
            }

            introCamera.LookAt = lookAtTarget;
            if (syncSceneRigWithSequence)
            {
                introPath.m_Looped = false;
                introPath.m_Resolution = Mathf.Max(4, pathResolution);
                introPath.m_Waypoints = BuildWaypoints(sequence, introPath.transform);
                introPath.InvalidateDistanceCache();
            }

            introTrackedDolly.m_Path = introPath;
            introTrackedDolly.m_PositionUnits = CinemachinePathBase.PositionUnits.PathUnits;
            if (!hasCapturedCameraState)
            {
                initialLookAtPosition = lookAtTarget.position;
                initialFieldOfView = introCamera.m_Lens.FieldOfView;
            }

            return;
        }

        ownsRuntimeRig = true;

        GameObject pathObject = new GameObject("EP3 Lobby Intro Path");
        introPath = pathObject.AddComponent<CinemachineSmoothPath>();
        introPath.m_Looped = false;
        introPath.m_Resolution = Mathf.Max(4, pathResolution);
        introPath.m_Waypoints = BuildWaypoints(sequence, introPath.transform);
        introPath.InvalidateDistanceCache();

        GameObject lookAtObject = new GameObject("EP3 Lobby Intro LookAt");
        lookAtTarget = lookAtObject.transform;
        lookAtTarget.position = initialLookAtPosition;

        GameObject cameraObject = new GameObject("EP3 Lobby Intro Dolly Camera");
        introCamera = cameraObject.AddComponent<CinemachineVirtualCamera>();
        introCamera.Priority = 0;
        introCamera.LookAt = lookAtTarget;
        introCamera.PreviousStateIsValid = false;
        introCamera.m_Lens.FieldOfView = initialFieldOfView;

        if (gameplayCamera != null)
        {
            introCamera.m_Lens = gameplayCamera.m_Lens;
        }
        else if (Camera.main != null)
        {
            introCamera.m_Lens.FieldOfView = Camera.main.fieldOfView;
        }

        introTrackedDolly = introCamera.AddCinemachineComponent<CinemachineTrackedDolly>();
        introTrackedDolly.m_Path = introPath;
        introTrackedDolly.m_PositionUnits = CinemachinePathBase.PositionUnits.PathUnits;
        introTrackedDolly.m_PathPosition = 0f;
        introTrackedDolly.m_XDamping = 0f;
        introTrackedDolly.m_YDamping = 0f;
        introTrackedDolly.m_ZDamping = 0f;
        introTrackedDolly.m_PitchDamping = 0f;
        introTrackedDolly.m_YawDamping = 0f;
        introTrackedDolly.m_RollDamping = 0f;
        introTrackedDolly.m_CameraUp = CinemachineTrackedDolly.CameraUpMode.Default;

        introCamera.AddCinemachineComponent<CinemachineHardLookAt>();
    }

    private void PrepareIntroRigForStart(Ep3LobbyIntroSequenceData sequence)
    {
        if (sequence == null || introCamera == null || introTrackedDolly == null || introPath == null || lookAtTarget == null)
        {
            return;
        }

        if (ownsRuntimeRig || syncSceneRigWithSequence)
        {
            introPath.m_Looped = false;
            introPath.m_Resolution = Mathf.Max(4, pathResolution);
            introPath.m_Waypoints = BuildWaypoints(sequence, introPath.transform);
            introPath.InvalidateDistanceCache();
        }

        introTrackedDolly.m_Path = introPath;
        introTrackedDolly.m_PositionUnits = CinemachinePathBase.PositionUnits.PathUnits;
        introTrackedDolly.m_PathPosition = 0f;
        lookAtTarget.position = initialLookAtPosition;
        introCamera.LookAt = lookAtTarget;
        introCamera.m_Lens.FieldOfView = initialFieldOfView;
        introCamera.transform.SetPositionAndRotation(initialCameraPosition, initialCameraRotation);
        introCamera.PreviousStateIsValid = false;
    }

    private CinemachineSmoothPath.Waypoint[] BuildWaypoints(Ep3LobbyIntroSequenceData sequence, Transform pathTransform)
    {
        CinemachineSmoothPath.Waypoint[] waypoints = new CinemachineSmoothPath.Waypoint[sequence.shots.Count + 1];
        waypoints[0] = CreateWaypoint(initialCameraPosition, pathTransform);

        for (int i = 0; i < sequence.shots.Count; i++)
        {
            waypoints[i + 1] = CreateWaypoint(sequence.shots[i].cameraPosition, pathTransform);
        }

        return waypoints;
    }

    private static CinemachineSmoothPath.Waypoint CreateWaypoint(Vector3 worldPosition, Transform pathTransform)
    {
        return new CinemachineSmoothPath.Waypoint
        {
            position = pathTransform != null ? pathTransform.InverseTransformPoint(worldPosition) : worldPosition,
            roll = 0f
        };
    }

    private IEnumerator PlayCutscene(Ep3LobbyIntroSequenceData sequence, bool markPlayedOnFinish)
    {
        isPlaying = true;
        advanceRequested = false;
        skipRequested = false;
        PrepareIntroRigForStart(sequence);
        SetPlayerControl(false);
        SetCutsceneButtonsVisible(true);
        if (useRuntimeHintOverlay)
        {
            hintOverlay?.Show(fontAsset: subtitleFont);
        }

        if (gameplayCamera != null)
        {
            gameplayCamera.Priority = 0;
        }

        if (subtitlePresenter != null)
        {
            subtitlePresenter.Hide();
        }

        if (introTrackedDolly != null)
        {
            introTrackedDolly.m_PathPosition = 0f;
        }

        if (lookAtTarget != null)
        {
            lookAtTarget.position = initialLookAtPosition;
        }

        if (introCamera != null)
        {
            introCamera.m_Lens.FieldOfView = initialFieldOfView;
            introCamera.PreviousStateIsValid = false;
            introCamera.Priority = Mathf.Max(introCameraPriority, originalGameplayPriority + 20);
        }

        yield return null;

        for (int i = 0; i < sequence.shots.Count; i++)
        {
            advanceRequested = false;
            yield return PlayShot(sequence.shots[i], Mathf.Min(i + 1f, introPath != null ? introPath.MaxPos : i + 1f));
            if (skipRequested)
            {
                break;
            }
            if (advanceRequested)
            {
                advanceRequested = false;
                continue;
            }
        }

        if (markPlayedOnFinish)
        {
            MarkCutscenePlayed();
        }

        RestoreState();
        isPlaying = false;
        advanceRequested = false;
        skipRequested = false;
        onCutsceneFinished?.Invoke();
    }

    private IEnumerator PlayShot(Ep3LobbyIntroShotData shot, float targetPathPosition)
    {
        if (introTrackedDolly == null || introCamera == null || lookAtTarget == null)
        {
            yield break;
        }

        if (subtitlePresenter != null)
        {
            subtitlePresenter.Show(shot);
        }

        float startPathPosition = introTrackedDolly.m_PathPosition;
        Vector3 startLookAt = lookAtTarget.position;
        float startFieldOfView = introCamera.m_Lens.FieldOfView;

        float moveDuration = Mathf.Max(0f, shot.moveDuration);
        if (moveDuration <= 0.01f)
        {
            if (skipRequested)
            {
                yield break;
            }

            introTrackedDolly.m_PathPosition = targetPathPosition;
            lookAtTarget.position = shot.lookAtPosition;
            introCamera.m_Lens.FieldOfView = ResolveFieldOfView(shot.fieldOfView, startFieldOfView);
        }
        else
        {
            float elapsed = 0f;
            float targetFieldOfView = ResolveFieldOfView(shot.fieldOfView, startFieldOfView);
            bool interruptedBySkip = false;
            while (elapsed < moveDuration)
            {
                PollCutsceneInput();
                if (skipRequested)
                {
                    interruptedBySkip = true;
                    break;
                }

                if (advanceRequested)
                {
                    break;
                }

                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / moveDuration);
                t = t * t * (3f - (2f * t));

                introTrackedDolly.m_PathPosition = Mathf.Lerp(startPathPosition, targetPathPosition, t);
                lookAtTarget.position = Vector3.Lerp(startLookAt, shot.lookAtPosition, t);
                introCamera.m_Lens.FieldOfView = Mathf.Lerp(startFieldOfView, targetFieldOfView, t);
                yield return null;
            }

            if (interruptedBySkip)
            {
                yield break;
            }

            introTrackedDolly.m_PathPosition = targetPathPosition;
            lookAtTarget.position = shot.lookAtPosition;
            introCamera.m_Lens.FieldOfView = targetFieldOfView;
        }

        if (skipRequested || advanceRequested)
        {
            yield break;
        }

        float elapsedHold = 0f;
        float holdDuration = Mathf.Max(0.5f, shot.holdDuration);
        while (elapsedHold < holdDuration)
        {
            PollCutsceneInput();
            if (skipRequested || advanceRequested)
            {
                yield break;
            }

            elapsedHold += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private static float ResolveFieldOfView(float targetFieldOfView, float fallbackFieldOfView)
    {
        return targetFieldOfView > 0.01f ? targetFieldOfView : fallbackFieldOfView;
    }

    private void SetPlayerControl(bool enabled)
    {
        if (playerInput != null)
        {
            if (!enabled)
            {
                previousInputEnabled = playerInput.enabled;
                previousLookLock = playerInput.isLookLock;
                previousJumpLock = playerInput.isJumpLock;
                playerInput.ResetInputState();
                playerInput.isLookLock = true;
                playerInput.isJumpLock = true;
                playerInput.enabled = false;
            }
            else
            {
                playerInput.enabled = previousInputEnabled;
                playerInput.ResetInputState();
                playerInput.isLookLock = previousLookLock;
                playerInput.isJumpLock = previousJumpLock;
            }
        }

        if (playerMovement != null)
        {
            if (!enabled)
            {
                previousMovementEnabled = playerMovement.enabled;
                playerMovement.SetMoveLock(true);
                playerMovement.enabled = false;
            }
            else
            {
                playerMovement.enabled = previousMovementEnabled;
                playerMovement.SetMoveLock(false);
            }
        }

        if (interactManager != null)
        {
            interactManager.enabled = enabled;
        }
    }

    private void RestoreState()
    {
        SetPlayerControl(true);
        SetCutsceneButtonsVisible(false);
        if (useRuntimeHintOverlay)
        {
            hintOverlay?.Hide();
        }

        if (gameplayCamera != null)
        {
            gameplayCamera.Priority = originalGameplayPriority;
            gameplayCamera.PreviousStateIsValid = false;
        }

        if (introCamera != null)
        {
            introCamera.Priority = 0;
            introCamera.PreviousStateIsValid = false;
            if (ownsRuntimeRig)
            {
                Destroy(introCamera.gameObject);
                introCamera = null;
            }
        }

        introTrackedDolly = null;

        if (subtitlePresenter != null)
        {
            subtitlePresenter.Hide();
            if (ownsSubtitlePresenter)
            {
                Destroy(subtitlePresenter.gameObject);
                subtitlePresenter = null;
                ownsSubtitlePresenter = false;
            }
        }

        if (lookAtTarget != null && ownsRuntimeRig)
        {
            Destroy(lookAtTarget.gameObject);
            lookAtTarget = null;
        }

        if (introPath != null && ownsRuntimeRig)
        {
            Destroy(introPath.gameObject);
            introPath = null;
        }
    }
    private int lastInputFrame = -1;
    private void PollCutsceneInput()
    {
        if (lastInputFrame == Time.frameCount) return;
        lastInputFrame = Time.frameCount;
        if (buttonRequested)
        {
            buttonRequested = false;
            return;
        }
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (!skipRequested && CutsceneInputHelper.IsSkipPressedThisFrame())
        {
            skipRequested = true;
            return;
        }

        if (!advanceRequested && CutsceneInputHelper.IsAdvancePressedThisFrame())
        {
            advanceRequested = true;
            Debug.Log("cut advanced: 2");
        }
    }

    private void EnsureHintOverlay()
    {
        Transform overlayParent = subtitlePresenter != null ? subtitlePresenter.transform : transform;
        hintOverlay = CutsceneHintOverlay.GetOrCreate(overlayParent, subtitleFont);
        hintOverlay?.Hide();
    }

    private void InitializeCutsceneButtons()
    {
        CutsceneControlButtonHelper.TryAutoResolve(ref nextButton, ref nextButtonRoot, ref skipButton, ref skipButtonRoot);
        CutsceneControlButtonHelper.Register(nextButton, OnNextButton);
        CutsceneControlButtonHelper.Register(skipButton, OnSkipButton);
    }

    private void SetCutsceneButtonsVisible(bool visible)
    {
        CutsceneControlButtonHelper.SetVisible(nextButton, nextButtonRoot, visible);
        CutsceneControlButtonHelper.SetVisible(skipButton, skipButtonRoot, visible);
    }

    private void FinishController()
    {
        SetGameCutsceneMode(false);
        if (destroyWhenFinished)
        {
            Destroy(gameObject);
            return;
        }

        enabled = false;
    }

    private void SetGameCutsceneMode(bool isCutsceneMode)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[Ep3LobbyIntroCutsceneController] GameManager.Instance가 없어 CutsceneMode를 적용하지 못했습니다.");
            return;
        }

        GameManager.Instance.CutsceneMode(isCutsceneMode);
    }

    private void MarkCutscenePlayed()
    {
        inspectorPlayedState = true;

        if (useInspectorPlayedStateOverride)
        {
            return;
        }

        if (playOnceSaveKey == Ep3IntroCutsceneSaveKey.None)
        {
            return;
        }

        inspectorPlayedState = true;

        if (useInspectorPlayedStateOverride)
        {
            return;
        }

        SaveDataObj data = ResolveSaveData();
        if (data == null)
        {
            return;
        }

        SetPlayedCutscene(data, true);
        CurData = data;

        if (SaveManager.instance != null)
        {
            SaveManager.instance.curData = data;
        }

        SaveManager.WriteCurJSON(data);
    }

    private SaveDataObj ResolveSaveData()
    {
        if (SaveManager.instance != null)
        {
            if (SaveManager.instance.curData == null)
            {
                SaveManager.instance.curData = SaveManager.ReadCurJSON();
            }

            CurData = SaveManager.instance.curData;
            return CurData;
        }

        if (CurData == null)
        {
            CurData = SaveManager.ReadCurJSON();
        }

        return CurData;
    }

    private void RefreshInspectorPlayedStateFromSave()
    {
        RefreshInspectorPlayedStateFromSave(ResolveSaveData());
    }

    private void RefreshInspectorPlayedStateFromSave(SaveDataObj data)
    {
        if (useInspectorPlayedStateOverride || playOnceSaveKey == Ep3IntroCutsceneSaveKey.None)
        {
            return;
        }

        inspectorPlayedState = data != null && HasPlayedCutscene(data);
    }

    private bool HasPlayedCutscene(SaveDataObj data)
    {
        if (playOnceSaveKey == Ep3IntroCutsceneSaveKey.None)
        {
            return false;
        }

        if (data == null)
        {
            return true;
        }

        switch (playOnceSaveKey)
        {
            case Ep3IntroCutsceneSaveKey.None:
                return false;

            case Ep3IntroCutsceneSaveKey.EP3Stage3_1:
                return data.Played_EP3_Stage3_1Intro;

            case Ep3IntroCutsceneSaveKey.EP3Stage3_1Completion:
                return data.Played_EP3_Stage3_1Completion;

            case Ep3IntroCutsceneSaveKey.EP3Stage3_2Intro:
                return data.Played_EP3_Stage3_2Intro;

            case Ep3IntroCutsceneSaveKey.EP3ReturnedLobbyIntro:
                return data.Played_EP3_ReturnedLobbyIntro;

            default:
                return data.Played_EP3_LobbyIntro;
        }
    }

    private void SetPlayedCutscene(SaveDataObj data, bool played)
    {
        if (data == null)
        {
            return;
        }

        switch (playOnceSaveKey)
        {
            case Ep3IntroCutsceneSaveKey.None:
                break;

            case Ep3IntroCutsceneSaveKey.EP3Stage3_1:
                data.Played_EP3_Stage3_1Intro = played;
                break;

            case Ep3IntroCutsceneSaveKey.EP3Stage3_1Completion:
                data.Played_EP3_Stage3_1Completion = played;
                break;

            case Ep3IntroCutsceneSaveKey.EP3Stage3_2Intro:
                data.Played_EP3_Stage3_2Intro = played;
                break;

            case Ep3IntroCutsceneSaveKey.EP3ReturnedLobbyIntro:
                data.Played_EP3_ReturnedLobbyIntro = played;
                break;

            case Ep3IntroCutsceneSaveKey.EP3Lobby:
            default:
                data.Played_EP3_LobbyIntro = played;
                break;
        }
    }

    private Ep3LobbyIntroSequenceData LoadSequenceData()
    {
        Ep3LobbyIntroSequenceData sequence = null;

        if (inspectorShots != null && inspectorShots.Count > 0)
        {
            sequence = CreateSequenceFromInspector();
        }
        else
        {
            Ep3LobbyIntroSequenceAsset sequenceAsset = Resources.Load<Ep3LobbyIntroSequenceAsset>(cutsceneAssetResourcePath);
            if (sequenceAsset != null && sequenceAsset.shots != null && sequenceAsset.shots.Count > 0)
            {
                Ep3LobbyIntroSequenceData assetSequence = sequenceAsset.ToSequenceData();
                if (DoesSequenceMatchRequest(assetSequence))
                {
                    sequence = assetSequence;
                }
            }
        }

        if (sequence == null)
        {
            TextAsset cutsceneAsset = Resources.Load<TextAsset>(cutsceneResourcePath);
            if (cutsceneAsset != null && !string.IsNullOrWhiteSpace(cutsceneAsset.text))
            {
                Ep3LobbyIntroSequenceData loaded = JsonUtility.FromJson<Ep3LobbyIntroSequenceData>(cutsceneAsset.text);
                if (loaded != null && loaded.shots != null && loaded.shots.Count > 0 && DoesSequenceMatchRequest(loaded))
                {
                    sequence = loaded;
                }
            }
        }

        if (sequence == null)
        {
            sequence = CreatePreferredFallbackSequence();
        }

        ApplyDialogueDefaults(sequence);
        NormalizeSequencePresentationMetadata(sequence);
        return sequence;
    }

    private Ep3LobbyIntroSequenceData CreatePreferredFallbackSequence()
    {
        if (IsStage3_1CompletionSequence())
        {
            return CreateStage3_1CompletionFallbackSequence();
        }

        if (IsStage3_1IntroSequence())
        {
            return CreateStage3_1FallbackSequence();
        }

        return new Ep3LobbyIntroSequenceData
        {
            sequenceId = "EP3_LOBBY_INTRO",
            shots = new List<Ep3LobbyIntroShotData>
            {
                new Ep3LobbyIntroShotData
                {
                    dialogueId = "EP3_001",
                    speakerType = "Narration",
                    speakerName = string.Empty,
                    subtitleText = "낯선 정적이 작업실 안에 가라앉아 있었다.",
                    showSpeakerName = false,
                    cameraPosition = new Vector3(3.25f, 2.3f, -27.8f),
                    lookAtPosition = new Vector3(3.35f, 1.4f, -20.5f),
                    moveDuration = 1.8f,
                    holdDuration = 1.5f,
                    fieldOfView = 50f
                },
                new Ep3LobbyIntroShotData
                {
                    dialogueId = "EP3_002",
                    speakerType = "Narration",
                    speakerName = string.Empty,
                    subtitleText = "악기와 악보, 정리되지 못한 흔적들이 아직도 제자리를 지키고 있었다.",
                    showSpeakerName = false,
                    cameraPosition = new Vector3(6.2f, 2.35f, -18.7f),
                    lookAtPosition = new Vector3(0.8f, 1.2f, -12.2f),
                    moveDuration = 2.3f,
                    holdDuration = 1.6f,
                    fieldOfView = 48f
                },
                new Ep3LobbyIntroShotData
                {
                    dialogueId = "EP3_003",
                    speakerType = "Narration",
                    speakerName = string.Empty,
                    subtitleText = "마치 끝내 이어지지 못한 선율이 이 공간에 남아 있는 것 같았다.",
                    showSpeakerName = false,
                    cameraPosition = new Vector3(2.2f, 4.85f, -11.2f),
                    lookAtPosition = new Vector3(0.9f, 1.35f, -10.7f),
                    moveDuration = 2.4f,
                    holdDuration = 1.7f,
                    fieldOfView = 56f
                },
                new Ep3LobbyIntroShotData
                {
                    dialogueId = "EP3_004",
                    speakerType = "Narration",
                    speakerName = string.Empty,
                    subtitleText = "그리고 그 흔적들 사이에서, 누군가를 기다리는 듯한 기척이 느껴졌다.",
                    showSpeakerName = false,
                    cameraPosition = new Vector3(4.7f, 2.5f, -10.0f),
                    lookAtPosition = new Vector3(2.8f, 1.3f, -17.0f),
                    moveDuration = 2.0f,
                    holdDuration = 1.7f,
                    fieldOfView = 45f
                },
                new Ep3LobbyIntroShotData
                {
                    dialogueId = "EP3_005",
                    speakerType = "Monologue",
                    speakerName = string.Empty,
                    subtitleText = "이곳에 남겨진 이야기를 알아보려면, 조금 더 가까이 다가가 봐야 할 것 같다.",
                    showSpeakerName = false,
                    cameraPosition = new Vector3(3.4f, 2.25f, -24.6f),
                    lookAtPosition = new Vector3(3.3f, 1.2f, -34.3f),
                    moveDuration = 2.4f,
                    holdDuration = 2.0f,
                    fieldOfView = 43f
                }
            }
        };
    }

    private Ep3LobbyIntroSequenceData CreateFallbackSequence()
    {
        if (IsStage3_1CompletionSequence())
        {
            return CreateStage3_1CompletionFallbackSequence();
        }

        if (IsStage3_1IntroSequence())
        {
            return CreateStage3_1FallbackSequence();
        }

        return new Ep3LobbyIntroSequenceData
        {
            sequenceId = "EP3_LOBBY_INTRO",
            shots = new List<Ep3LobbyIntroShotData>
            {
                new Ep3LobbyIntroShotData
                {
                    dialogueId = "EP3_001",
                    speakerType = "Monologue",
                    speakerName = "",
                    subtitleText = "이번엔 색이 아니라, 소리가 비어 있다.",
                    showSpeakerName = false,
                    cameraPosition = new Vector3(3.25f, 2.3f, -27.8f),
                    lookAtPosition = new Vector3(3.35f, 1.4f, -20.5f),
                    moveDuration = 1.8f,
                    holdDuration = 1.5f,
                    fieldOfView = 50f
                },
                new Ep3LobbyIntroShotData
                {
                    dialogueId = "EP3_002",
                    speakerType = "Monologue",
                    speakerName = "",
                    subtitleText = "음악이 머물렀던 자리인데, 지금은 울림만 비어 있다.",
                    showSpeakerName = false,
                    cameraPosition = new Vector3(6.2f, 2.35f, -18.7f),
                    lookAtPosition = new Vector3(0.8f, 1.2f, -12.2f),
                    moveDuration = 2.3f,
                    holdDuration = 1.6f,
                    fieldOfView = 48f
                },
                new Ep3LobbyIntroShotData
                {
                    dialogueId = "EP3_003",
                    speakerType = "Dialogue",
                    speakerName = "음악가",
                    subtitleText = "…왔네.",
                    showSpeakerName = true,
                    cameraPosition = new Vector3(2.2f, 4.85f, -11.2f),
                    lookAtPosition = new Vector3(0.9f, 1.35f, -10.7f),
                    moveDuration = 2.4f,
                    holdDuration = 1.7f,
                    fieldOfView = 56f
                },
                new Ep3LobbyIntroShotData
                {
                    dialogueId = "EP3_004",
                    speakerType = "Dialogue",
                    speakerName = "음악가",
                    subtitleText = "누군지는 기억나지 않는데… 왜 이렇게, 네가 오기를 기다렸던 것 같지?",
                    showSpeakerName = true,
                    cameraPosition = new Vector3(4.7f, 2.5f, -10.0f),
                    lookAtPosition = new Vector3(2.8f, 1.3f, -17.0f),
                    moveDuration = 2.0f,
                    holdDuration = 1.7f,
                    fieldOfView = 45f
                },
                new Ep3LobbyIntroShotData
                {
                    dialogueId = "EP3_005",
                    speakerType = "Dialogue",
                    speakerName = "음악가",
                    subtitleText = "끝까지 완성해야 했던 노래가 있었는데… 멜로디도, 박자도 전부 끊겨 있어.",
                    showSpeakerName = true,
                    cameraPosition = new Vector3(3.4f, 2.25f, -24.6f),
                    lookAtPosition = new Vector3(3.3f, 1.2f, -34.3f),
                    moveDuration = 2.4f,
                    holdDuration = 2.0f,
                    fieldOfView = 43f
                }
            }
        };
    }

    private static Ep3LobbyIntroSequenceData CreateStage3_1FallbackSequence()
    {
        return new Ep3LobbyIntroSequenceData
        {
            sequenceId = Stage3_1IntroSequenceId,
            shots = new List<Ep3LobbyIntroShotData>
            {
                new Ep3LobbyIntroShotData
                {
                    dialogueId = "EP3_001",
                    speakerType = "Narration",
                    speakerName = string.Empty,
                    subtitleText = "이곳은 전에 지나온 공간들과는 분위기가 다르다.",
                    showSpeakerName = false,
                    cameraPosition = new Vector3(2.7829013f, 2.1086717f, -8.384186f),
                    lookAtPosition = new Vector3(-0.3103757f, 0.75217676f, 3.798327f),
                    moveDuration = 1.8f,
                    holdDuration = 1.5f,
                    fieldOfView = 54f
                },
                new Ep3LobbyIntroShotData
                {
                    dialogueId = "EP3_002",
                    speakerType = "Narration",
                    speakerName = string.Empty,
                    subtitleText = "발밑에는 황혼빛 구름이 낮게 깔려 있고, 주변에는 흩어진 음표와 선율의 흔적이 떠다닌다.",
                    showSpeakerName = false,
                    cameraPosition = new Vector3(-0.84414005f, 5.0954046f, 2.9775188f),
                    lookAtPosition = new Vector3(5.7418184f, 0.543448f, 3.2705357f),
                    moveDuration = 2.1f,
                    holdDuration = 1.6f,
                    fieldOfView = 49f
                },
                new Ep3LobbyIntroShotData
                {
                    dialogueId = "EP3_003",
                    speakerType = "Monologue",
                    speakerName = string.Empty,
                    subtitleText = "마치 하늘 위 어딘가에 홀로 떠 있는 공간에 들어선 듯한 느낌이 든다.",
                    showSpeakerName = false,
                    cameraPosition = new Vector3(0.7610698f, 0.94867814f, 4.8600473f),
                    lookAtPosition = new Vector3(0.049334288f, -0.93554145f, -0.3181162f),
                    moveDuration = 2.2f,
                    holdDuration = 1.7f,
                    fieldOfView = 46f
                },
                new Ep3LobbyIntroShotData
                {
                    dialogueId = "EP3_004",
                    speakerType = "Monologue",
                    speakerName = string.Empty,
                    subtitleText = "이곳에 남아 있는 것은, 아직 끝나지 않은 어떤 흐름뿐이다.",
                    showSpeakerName = false,
                    cameraPosition = new Vector3(6.5975795f, -1.8673064f, 12.0882015f),
                    lookAtPosition = new Vector3(6.40379f, -2.6425843f, 13.714412f),
                    moveDuration = 2.3f,
                    holdDuration = 2f,
                    fieldOfView = 44f
                }
            }
        };
    }

    private static Ep3LobbyIntroSequenceData CreateStage3_1CompletionFallbackSequence()
    {
        return new Ep3LobbyIntroSequenceData
        {
            sequenceId = Stage3_1CompletionSequenceId,
            shots = new List<Ep3LobbyIntroShotData>
            {
                new Ep3LobbyIntroShotData
                {
                    dialogueId = "EP3_031_CLEAR_001",
                    speakerType = "Narration",
                    speakerName = string.Empty,
                    subtitleText = "처음에는 그저 조용한 로비처럼 보였던 공간이다.",
                    showSpeakerName = false,
                    cameraPosition = new Vector3(6.5975795f, -1.8673064f, 12.0882015f),
                    lookAtPosition = new Vector3(6.40379f, -2.6425843f, 13.714412f),
                    moveDuration = 1.8f,
                    holdDuration = 1.3f,
                    fieldOfView = 38f
                },
                new Ep3LobbyIntroShotData
                {
                    dialogueId = "EP3_031_CLEAR_002",
                    speakerType = "Narration",
                    speakerName = string.Empty,
                    subtitleText = "하지만 기억이 되살아난 지금, 이곳의 정적은 더 이상 비어 있지 않다.",
                    showSpeakerName = false,
                    cameraPosition = new Vector3(4.8597326f, -2.3806152f, 20.503187f),
                    lookAtPosition = new Vector3(5.4538097f, -2.7240057f, 17.524256f),
                    moveDuration = 2f,
                    holdDuration = 1.5f,
                    fieldOfView = 46f
                },
                new Ep3LobbyIntroShotData
                {
                    dialogueId = "EP3_031_CLEAR_003",
                    speakerType = "Narration",
                    speakerName = string.Empty,
                    subtitleText = "새로 놓인 피아노와 머그컵, 사진 속에 남아 있는 작은 흔적들까지도",
                    showSpeakerName = false,
                    cameraPosition = new Vector3(9.406369f, 1.0203038f, 4.3200746f),
                    lookAtPosition = new Vector3(7.060455f, -1.8923355f, -4.0589986f),
                    moveDuration = 2.1f,
                    holdDuration = 1.6f,
                    fieldOfView = 44f
                },
                new Ep3LobbyIntroShotData
                {
                    dialogueId = "EP3_031_CLEAR_004",
                    speakerType = "Monologue",
                    speakerName = string.Empty,
                    subtitleText = "마치 오래전부터 이 자리에 있었던 의미처럼 다르게 다가온다.",
                    showSpeakerName = false,
                    cameraPosition = new Vector3(0.7610698f, 0.94867814f, 4.8600473f),
                    lookAtPosition = new Vector3(0.049334288f, -0.93554145f, -0.3181162f),
                    moveDuration = 2.2f,
                    holdDuration = 1.8f,
                    fieldOfView = 46f
                }
            }
        };
    }

    private bool DoesSequenceMatchRequest(Ep3LobbyIntroSequenceData sequence)
    {
        if (sequence == null)
        {
            return false;
        }

        return string.IsNullOrWhiteSpace(inspectorSequenceId) ||
               string.IsNullOrWhiteSpace(sequence.sequenceId) ||
               string.Equals(sequence.sequenceId, inspectorSequenceId, StringComparison.OrdinalIgnoreCase);
    }

    private bool IsStage3_1IntroSequence()
    {
        return playOnceSaveKey == Ep3IntroCutsceneSaveKey.EP3Stage3_1 ||
               string.Equals(inspectorSequenceId, Stage3_1IntroSequenceId, StringComparison.OrdinalIgnoreCase);
    }

    private bool IsStage3_1CompletionSequence()
    {
        return string.Equals(inspectorSequenceId, Stage3_1CompletionSequenceId, StringComparison.OrdinalIgnoreCase);
    }

    private bool IsStage3_2IntroSequence()
    {
        return playOnceSaveKey == Ep3IntroCutsceneSaveKey.EP3Stage3_2Intro ||
               string.Equals(inspectorSequenceId, Stage3_2IntroSequenceId, StringComparison.OrdinalIgnoreCase);
    }

    private bool IsReturnedLobbyIntroSequence()
    {
        return playOnceSaveKey == Ep3IntroCutsceneSaveKey.EP3ReturnedLobbyIntro ||
               string.Equals(inspectorSequenceId, ReturnedLobbyIntroSequenceId, StringComparison.OrdinalIgnoreCase);
    }

    private void ApplyDialogueDefaults(Ep3LobbyIntroSequenceData sequence)
    {
        if (sequence == null || sequence.shots == null || sequence.shots.Count == 0)
        {
            return;
        }

        Dictionary<string, Ep3LobbyIntroShotData> defaultsById = new Dictionary<string, Ep3LobbyIntroShotData>();
        foreach (Ep3LobbyIntroShotData defaultShot in CreatePreferredFallbackSequence().shots)
        {
            if (defaultShot == null || string.IsNullOrWhiteSpace(defaultShot.dialogueId))
            {
                continue;
            }

            defaultsById[defaultShot.dialogueId] = defaultShot;
        }

        foreach (Ep3LobbyIntroShotData shot in sequence.shots)
        {
            if (shot == null || string.IsNullOrWhiteSpace(shot.dialogueId))
            {
                continue;
            }

            if (!defaultsById.TryGetValue(shot.dialogueId, out Ep3LobbyIntroShotData defaultShot))
            {
                continue;
            }

            bool hasDialogueMetadata =
                !string.IsNullOrWhiteSpace(shot.speakerType) ||
                !string.IsNullOrWhiteSpace(shot.speakerName) ||
                !string.IsNullOrWhiteSpace(shot.subtitleText);

            if (!hasDialogueMetadata)
            {
                shot.speakerType = defaultShot.speakerType;
                shot.speakerName = defaultShot.speakerName;
                shot.subtitleText = defaultShot.subtitleText;
                shot.showSpeakerName = defaultShot.showSpeakerName;
            }
        }
    }

    private void NormalizeSequencePresentationMetadata(Ep3LobbyIntroSequenceData sequence)
    {
        if (sequence == null || sequence.shots == null || sequence.shots.Count == 0)
        {
            return;
        }

        foreach (Ep3LobbyIntroShotData shot in sequence.shots)
        {
            if (shot == null || string.IsNullOrWhiteSpace(shot.speakerType))
            {
                continue;
            }

            if (shot.speakerType.Equals("Narration", StringComparison.OrdinalIgnoreCase) ||
                shot.speakerType.Equals("Monologue", StringComparison.OrdinalIgnoreCase))
            {
                shot.speakerName = string.Empty;
                shot.showSpeakerName = false;
            }
        }
    }

    private void NormalizeSequencePresentation(Ep3LobbyIntroSequenceData sequence)
    {
        if (sequence == null || sequence.shots == null || sequence.shots.Count == 0)
        {
            return;
        }

        foreach (Ep3LobbyIntroShotData shot in sequence.shots)
        {
            if (shot == null || string.IsNullOrWhiteSpace(shot.dialogueId))
            {
                continue;
            }

            switch (shot.dialogueId)
            {
                case "EP3_001":
                    ApplyNarration(shot, "이곳은 전에 지나온 공간들과는 분위기가 다르다.");
                    break;

                case "EP3_002":
                    ApplyNarration(shot, "발밑에는 황혼빛 구름이 낮게 깔려 있고, 주변에는 흩어진 음표와 선율의 흔적이 떠다닌다.");
                    break;

                case "EP3_003":
                    ApplyMonologue(shot, "마치 하늘 위 어딘가에 홀로 떠 있는 공간에 들어선 듯한 느낌이 든다.");
                    break;

                case "EP3_004":
                    ApplyMonologue(shot, "이곳에 남아 있는 것은, 아직 끝나지 않은 어떤 흐름뿐이다.");
                    break;
            }
        }
    }

    private static void ApplyNarration(Ep3LobbyIntroShotData shot, string subtitleText)
    {
        if (shot == null)
        {
            return;
        }

        shot.speakerType = "Narration";
        shot.speakerName = string.Empty;
        shot.subtitleText = subtitleText;
        shot.showSpeakerName = false;
    }

    private static void ApplyMonologue(Ep3LobbyIntroShotData shot, string subtitleText)
    {
        if (shot == null)
        {
            return;
        }

        shot.speakerType = "Monologue";
        shot.speakerName = string.Empty;
        shot.subtitleText = subtitleText;
        shot.showSpeakerName = false;
    }

    private void EnsureSubtitlePresenter()
    {
        if (!enableSubtitles || subtitlePresenter != null)
        {
            return;
        }

        Ep3CutsceneSubtitlePresenter[] presenters = FindObjectsOfType<Ep3CutsceneSubtitlePresenter>(true);
        if (presenters != null && presenters.Length > 0)
        {
            subtitlePresenter = presenters[0];
            subtitlePresenter.Configure(subtitleFont, textboxManager, forceStandaloneSubtitleOverlay);
            return;
        }

        if (!forceStandaloneSubtitleOverlay)
        {
            if (textboxManager != null)
            {
                GameObject subtitleObject = new GameObject("EP3 Lobby Intro Subtitles", typeof(RectTransform));
                subtitleObject.transform.SetParent(textboxManager.transform, false);
                subtitlePresenter = subtitleObject.AddComponent<Ep3CutsceneSubtitlePresenter>();
                subtitlePresenter.Configure(subtitleFont, textboxManager, false);
                ownsSubtitlePresenter = true;
                return;
            }
        }

        subtitlePresenter = FindObjectOfType<Ep3CutsceneSubtitlePresenter>();
        if (subtitlePresenter != null)
        {
            subtitlePresenter.Configure(subtitleFont, null, forceStandaloneSubtitleOverlay);
            return;
        }

        GameObject fallbackSubtitleObject = new GameObject("EP3 Lobby Intro Subtitles");
        subtitlePresenter = fallbackSubtitleObject.AddComponent<Ep3CutsceneSubtitlePresenter>();
        subtitlePresenter.Configure(subtitleFont, null, forceStandaloneSubtitleOverlay);
        ownsSubtitlePresenter = true;
    }

    private static TextboxManager FindTextboxManager()
    {
        TextboxManager[] managers = FindObjectsOfType<TextboxManager>(true);
        if (managers == null || managers.Length == 0)
        {
            return null;
        }

        foreach (TextboxManager manager in managers)
        {
            if (manager == null || manager.gameObject == null)
            {
                continue;
            }

            string objectName = manager.gameObject.name;
            if (objectName.IndexOf("Canvas_Cutscene", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return manager;
            }
        }

        foreach (TextboxManager manager in managers)
        {
            if (manager == null || manager.gameObject == null)
            {
                continue;
            }

            string objectName = manager.gameObject.name;
            if (objectName.IndexOf("Canvas_NPC_Chat", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return manager;
            }
        }

        return managers[0];
    }

    private Ep3LobbyIntroSequenceData CreateSequenceFromInspector()
    {
        Ep3LobbyIntroSequenceData sequence = new Ep3LobbyIntroSequenceData
        {
            sequenceId = string.IsNullOrWhiteSpace(inspectorSequenceId) ? "EP3_LOBBY_INTRO" : inspectorSequenceId,
            shots = new List<Ep3LobbyIntroShotData>(inspectorShots.Count)
        };

        foreach (Ep3LobbyIntroShotData shot in inspectorShots)
        {
            if (shot == null)
            {
                continue;
            }

            sequence.shots.Add(shot.Clone());
        }

        return sequence;
    }

    private void ApplyDefaultInspectorShots()
    {
        if (inspectorShots != null && inspectorShots.Count > 0)
        {
            return;
        }

        Ep3LobbyIntroSequenceData defaults = CreateFallbackSequence();
        inspectorSequenceId = defaults.sequenceId;
        inspectorShots = new List<Ep3LobbyIntroShotData>(defaults.shots.Count);
        foreach (Ep3LobbyIntroShotData shot in defaults.shots)
        {
            inspectorShots.Add(shot.Clone());
        }
    }

}
