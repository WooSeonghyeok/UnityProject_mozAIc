using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;

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

/// <summary>
/// Plays the Episode 3 lobby intro once, using a Cinemachine Smooth Path and Tracked Dolly.
/// </summary>
[AddComponentMenu("Episode3/EP3 Lobby Intro Cutscene Controller")]
public class Ep3LobbyIntroCutsceneController : MonoBehaviour
{
    public const string SceneName = "Episode3_Scene";

    private const string PlayerTag = "Player";
    private const string DefaultResourcePath = "Data/ep3_lobby_intro_cutscene";
    private const string DefaultSequenceAssetPath = "Data/EP3LobbyIntroSequence";
    private const float DefaultLookDistance = 8f;

    [SerializeField] private string inspectorSequenceId = "EP3_LOBBY_INTRO";
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

    private Vector3 initialCameraPosition;
    private Quaternion initialCameraRotation = Quaternion.identity;
    private float initialFieldOfView = 60f;
    private Vector3 initialLookAtPosition;

    public void InitializeAsRuntimeFallback()
    {
        destroyWhenFinished = true;
    }

    private void Reset()
    {
        ApplyDefaultInspectorShots();
    }

    private void Start()
    {
        if (!isPlaying)
        {
            StartCoroutine(BeginCutsceneIfNeeded());
        }
    }

    private IEnumerator BeginCutsceneIfNeeded()
    {
        yield return null;

        if (!ShouldPlayCutscene())
        {
            FinishController();
            yield break;
        }

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

        if (playerMovement == null)
        {
            Debug.LogWarning("[Ep3LobbyIntroCutsceneController] PlayerMovement was not found, so the intro cutscene was skipped.");
            RestoreState();
            FinishController();
            yield break;
        }

        yield return new WaitForSeconds(startDelay);
        yield return PlayCutscene(sequence);

        FinishController();
    }

    private bool ShouldPlayCutscene()
    {
        SaveDataObj data = SaveManager.instance != null ? SaveManager.instance.curData : SaveManager.ReadCurJSON();
        return data != null && !data.isFirstEnterAtEP3Lobby;
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
            if (candidate == null || !candidate.isActiveAndEnabled)
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

    private void CaptureCurrentCameraState()
    {
        Transform currentCameraTransform = Camera.main != null ? Camera.main.transform : (gameplayCamera != null ? gameplayCamera.transform : null);
        if (currentCameraTransform != null)
        {
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
            initialLookAtPosition = lookAtTarget.position;
            initialFieldOfView = introCamera.m_Lens.FieldOfView;
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

    private IEnumerator PlayCutscene(Ep3LobbyIntroSequenceData sequence)
    {
        isPlaying = true;
        SetPlayerControl(false);

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
            yield return PlayShot(sequence.shots[i], Mathf.Min(i + 1f, introPath != null ? introPath.MaxPos : i + 1f));
        }

        MarkCutscenePlayed();
        RestoreState();
        isPlaying = false;
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
            introTrackedDolly.m_PathPosition = targetPathPosition;
            lookAtTarget.position = shot.lookAtPosition;
            introCamera.m_Lens.FieldOfView = ResolveFieldOfView(shot.fieldOfView, startFieldOfView);
        }
        else
        {
            float elapsed = 0f;
            float targetFieldOfView = ResolveFieldOfView(shot.fieldOfView, startFieldOfView);
            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / moveDuration);
                t = t * t * (3f - (2f * t));

                introTrackedDolly.m_PathPosition = Mathf.Lerp(startPathPosition, targetPathPosition, t);
                lookAtTarget.position = Vector3.Lerp(startLookAt, shot.lookAtPosition, t);
                introCamera.m_Lens.FieldOfView = Mathf.Lerp(startFieldOfView, targetFieldOfView, t);
                yield return null;
            }

            introTrackedDolly.m_PathPosition = targetPathPosition;
            lookAtTarget.position = shot.lookAtPosition;
            introCamera.m_Lens.FieldOfView = targetFieldOfView;
        }

        yield return new WaitForSeconds(Mathf.Max(0.5f, shot.holdDuration));
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
                playerInput.isLookLock = true;
                playerInput.isJumpLock = true;
                playerInput.enabled = false;
            }
            else
            {
                playerInput.enabled = previousInputEnabled;
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

        if (gameplayCamera != null)
        {
            gameplayCamera.Priority = originalGameplayPriority;
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

    private void FinishController()
    {
        if (destroyWhenFinished)
        {
            Destroy(gameObject);
            return;
        }

        enabled = false;
    }

    private void MarkCutscenePlayed()
    {
        SaveDataObj data = SaveManager.instance != null ? SaveManager.instance.curData : SaveManager.ReadCurJSON();
        if (data == null)
        {
            return;
        }

        data.isFirstEnterAtEP3Lobby = true;

        if (SaveManager.instance != null)
        {
            SaveManager.instance.curData = data;
            SaveManager.instance.WriteCurJSON();
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
                sequence = sequenceAsset.ToSequenceData();
            }
        }

        if (sequence == null)
        {
            TextAsset cutsceneAsset = Resources.Load<TextAsset>(cutsceneResourcePath);
            if (cutsceneAsset != null && !string.IsNullOrWhiteSpace(cutsceneAsset.text))
            {
                Ep3LobbyIntroSequenceData loaded = JsonUtility.FromJson<Ep3LobbyIntroSequenceData>(cutsceneAsset.text);
                if (loaded != null && loaded.shots != null && loaded.shots.Count > 0)
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
                    ApplyNarration(shot, "이번엔 색이 아니라, 소리가 비어 있다.");
                    break;

                case "EP3_002":
                    ApplyNarration(shot, "음악이 머물렀던 자리인데, 지금은 울림만 비어 있다.");
                    break;

                case "EP3_003":
                    ApplyMonologue(shot, "낯선 공간인데도, 이상하게 누군가를 기다리던 감각이 남아 있다.");
                    break;

                case "EP3_004":
                    ApplyMonologue(shot, "누군지는 기억나지 않는데… 왜 이렇게 오래 누군가를 기다렸던 것만 같지?");
                    break;

                case "EP3_005":
                    ApplyMonologue(shot, "끝까지 완성해야 했던 노래가 있었던 것 같다. 멜로디도, 박자도 전부 끊겨 있다.");
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
            subtitlePresenter.Configure(subtitleFont, FindCutsceneManager());
            return;
        }

        CutsceneManager cutsceneManager = FindCutsceneManager();
        if (cutsceneManager != null)
        {
            GameObject subtitleObject = new GameObject("EP3 Lobby Intro Subtitles", typeof(RectTransform));
            subtitleObject.transform.SetParent(cutsceneManager.transform, false);
            subtitlePresenter = subtitleObject.AddComponent<Ep3CutsceneSubtitlePresenter>();
            subtitlePresenter.Configure(subtitleFont, cutsceneManager);
            ownsSubtitlePresenter = true;
            return;
        }

        subtitlePresenter = FindObjectOfType<Ep3CutsceneSubtitlePresenter>();
        if (subtitlePresenter != null)
        {
            subtitlePresenter.Configure(subtitleFont);
            return;
        }

        GameObject fallbackSubtitleObject = new GameObject("EP3 Lobby Intro Subtitles");
        subtitlePresenter = fallbackSubtitleObject.AddComponent<Ep3CutsceneSubtitlePresenter>();
        subtitlePresenter.Configure(subtitleFont);
        ownsSubtitlePresenter = true;
    }

    private static CutsceneManager FindCutsceneManager()
    {
        CutsceneManager[] managers = FindObjectsOfType<CutsceneManager>(true);
        if (managers == null || managers.Length == 0)
        {
            return null;
        }

        foreach (CutsceneManager manager in managers)
        {
            if (manager == null || manager.gameObject == null)
            {
                continue;
            }

            string objectName = manager.gameObject.name;
            if (objectName.IndexOf("Canvas_Cutscene", StringComparison.OrdinalIgnoreCase) >= 0 ||
                objectName.IndexOf("Canvas_NPC_Chat", StringComparison.OrdinalIgnoreCase) >= 0)
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
