using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Ep3_2TopDownRhythmPuzzle : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private Ep3_2Manager stageManager;
    [SerializeField] private RhythmAudioManager audioManager;
    [SerializeField] private RhythmScoreManager scoreManager;
    [SerializeField] private RhythmEffectManager effectManager;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Ep3_2StartPuzzle startPuzzleController;
    [SerializeField] private Transform puzzleReferenceTransform;

    [Header("플레이어 고정")]
    [SerializeField] private bool lockPlayerDuringPuzzle = true;
    [SerializeField] private bool teleportPlayerToHoldPoint = true;
    [SerializeField] private Transform playerHoldPoint;
    [SerializeField] private float playerHoldHeightOffset = 0.2f;
    [SerializeField] private float fallbackHoldDistanceFromJudge = 3.4f;
    [SerializeField] private Vector3 fallbackHoldLocalOffset = new Vector3(0f, 0f, 4f);

    [Header("판정 기준점")]
    [SerializeField] private Transform judgeCenterAnchor;
    [SerializeField] private Vector3 fallbackJudgeCenterOffset = new Vector3(0f, 0.5f, 8f);
    [SerializeField] private float laneSpacing = 1.1f;
    [SerializeField] private float laneForwardSpacing = 2.6f;
    [SerializeField] private float noteTravelDistance = 22f;
    [SerializeField] private float noteHeightOffset = 0.4f;

    [Header("카메라")]
    [SerializeField] private bool useRuntimePuzzleCamera = true;
    [SerializeField] private bool useOrthographicPuzzleCamera = true;
    [SerializeField] private float orthographicSize = 11.5f;
    [SerializeField] private float cameraHeight = 22f;
    [SerializeField] private float cameraDistance = 2.5f;
    [SerializeField] private Vector3 cameraFocusOffset = new Vector3(0f, 1.4f, 0f);
    [SerializeField] private float cameraFollowSmooth = 12f;

    [Header("비주얼")]
    [SerializeField] private UnityEngine.Object noteVisualPrefab;
    [SerializeField] private bool useSpriteNoteVisuals = true;
    [SerializeField] private bool useLaneSpecificNotePrefabs = true;
    [SerializeField] private UnityEngine.Object leftLaneNotePrefab;
    [SerializeField] private UnityEngine.Object upLaneNotePrefab;
    [SerializeField] private UnityEngine.Object downLaneNotePrefab;
    [SerializeField] private UnityEngine.Object rightLaneNotePrefab;
    [SerializeField] private UnityEngine.Object extraLaneNotePrefab;
    [SerializeField] private UnityEngine.Object lanePadPrefab;
    [SerializeField] private UnityEngine.Object dLanePadPrefab;
    [SerializeField] private UnityEngine.Object fLanePadPrefab;
    [SerializeField] private UnityEngine.Object spaceLanePadPrefab;
    [SerializeField] private UnityEngine.Object jLanePadPrefab;
    [SerializeField] private UnityEngine.Object kLanePadPrefab;
    [SerializeField] private bool useSimpleLanePadVisuals = true;
    [SerializeField] [Range(0.05f, 1f)] private float lanePadOpacity = 0.42f;
    [SerializeField] private bool showLaneTracks = true;
    [SerializeField] [Min(0.2f)] private float laneTrackWidth = 0.92f;
    [SerializeField] [Min(1f)] private float laneTrackLength = 8f;
    [SerializeField] private float laneTrackHeightOffset = 0.03f;
    [SerializeField] [Range(0.05f, 1f)] private float laneTrackOpacity = 0.24f;
    [SerializeField] [Min(0f)] private float laneTrackLineWidth = 0f;
    [SerializeField] private Color laneTrackLineColor = new Color(1f, 1f, 1f, 0f);
    [SerializeField] private Vector3 laneTrackEulerOffset = new Vector3(-90f, 0f, 0f);
    [SerializeField] private Vector3 noteScale = new Vector3(2.2f, 0.35f, 2.2f);
    [SerializeField] private Vector3 noteVisualEulerOffset = new Vector3(0f, 180f, 0f);
    [SerializeField] private Vector3 prefabLanePadEulerOffset = Vector3.zero;
    [SerializeField] private Vector3 lanePadEulerOffset = new Vector3(-90f, 0f, 0f);
    [SerializeField] private Vector3 lanePadScale = new Vector3(0.55f, 0.3f, 0.3f);
    [SerializeField] private bool showLaneLabels = true;
    [SerializeField] private float laneLabelHeightOffset = 0.45f;
    [SerializeField] private int laneLabelFontSize = 72;
    [SerializeField] private float laneLabelCharacterSize = 0.12f;
    [SerializeField] private Color laneLabelColor = Color.white;
    [SerializeField] private TMP_FontAsset laneLabelFontAsset;
    [SerializeField] private bool showJudgeInfoPanels = true;
    [SerializeField] private float judgeInfoPanelOffset = 2.1f;
    [SerializeField] private float judgeInfoPanelHeightOffset = 0.35f;
    [SerializeField] private float judgeInfoPanelGap = 24f;
    [SerializeField] private float judgeInfoPanelCharacterSize = 0.01f;
    [SerializeField] private Vector2 judgeInfoPanelSize = new Vector2(220f, 88f);
    [SerializeField] private Color judgeInfoPanelBackgroundColor = new Color(0f, 0f, 0f, 0.58f);
    [SerializeField] private Color judgeInfoTextColor = new Color(1f, 0.96f, 0.72f, 1f);
    [SerializeField] private Color leftLaneColor = new Color(1f, 0.56f, 0.62f, 1f);
    [SerializeField] private Color upLaneColor = new Color(1f, 0.85f, 0.45f, 1f);
    [SerializeField] private Color downLaneColor = new Color(0.48f, 0.88f, 0.96f, 1f);
    [SerializeField] private Color rightLaneColor = new Color(0.73f, 0.62f, 1f, 1f);
    [SerializeField] private Color extraLaneColor = new Color(0.62f, 1f, 0.72f, 1f);

    [Header("실패 조건")]
    [SerializeField] [Min(0.02f)] private float excellentJudgeWindow = 0.08f;
    [SerializeField] [Min(0.04f)] private float goodJudgeWindow = 0.16f;
    [SerializeField] [Min(0.06f)] private float badJudgeWindow = 0.28f;
    [SerializeField] private bool showLaneInputFeedback = true;
    [SerializeField] private bool playSuccessEffectOnCorrectInput = true;
    [SerializeField] private float lanePressDepth = 0.12f;
    [SerializeField] [Range(0.6f, 1f)] private float lanePressedScaleMultiplier = 0.9f;
    [SerializeField] private float lanePressRecoverSpeed = 9f;
    [SerializeField] private float laneFlashRecoverSpeed = 7f;
    [SerializeField] [Range(0f, 1f)] private float laneFlashIntensity = 0.72f;
    [SerializeField] [Range(0f, 4f)] private float laneEmissionBoost = 1.4f;
    [SerializeField] private Color badLaneFlashColor = new Color(1f, 0.82f, 0.55f, 1f);
    [SerializeField] private Color goodLaneFlashColor = new Color(0.84f, 0.95f, 1f, 1f);
    [SerializeField] private Color excellentLaneFlashColor = new Color(1f, 0.97f, 0.68f, 1f);
    [SerializeField] private Color wrongLaneFlashColor = new Color(1f, 0.4f, 0.4f, 1f);
    [SerializeField] private int maxAllowedMistakes = 8;

    private enum LaneFeedbackKind
    {
        Bad,
        Good,
        Excellent,
        Wrong,
    }

    private class LanePadFeedbackMaterialState
    {
        public Material material;
        public bool hasBaseColor;
        public bool hasColor;
        public bool hasEmissionColor;
        public Color baseColor;
        public Color baseEmissionColor;
    }

    private class LanePadFeedbackState
    {
        public readonly List<LanePadFeedbackMaterialState> materials = new List<LanePadFeedbackMaterialState>();
        public float pressAmount;
        public float flashAmount;
        public Color flashColor = Color.white;
    }

    private readonly List<Ep3_2RhythmLaneNote> activeNotes = new List<Ep3_2RhythmLaneNote>();
    private readonly Dictionary<Ep3_2LaneType, Transform> lanePads = new Dictionary<Ep3_2LaneType, Transform>();
    private readonly Dictionary<Ep3_2LaneType, Transform> laneTracks = new Dictionary<Ep3_2LaneType, Transform>();
    private readonly Dictionary<Ep3_2LaneType, bool> lanePadUsesPrefabVisual = new Dictionary<Ep3_2LaneType, bool>();
    private readonly Dictionary<Ep3_2LaneType, LanePadFeedbackState> lanePadFeedbackStates = new Dictionary<Ep3_2LaneType, LanePadFeedbackState>();
    private readonly Dictionary<Ep3_2LaneType, Sprite> cachedLaneSprites = new Dictionary<Ep3_2LaneType, Sprite>();
    private readonly Dictionary<Ep3_2LaneType, Material> lanePadMaterials = new Dictionary<Ep3_2LaneType, Material>();
    private readonly Dictionary<Ep3_2LaneType, Material> laneTrackMaterials = new Dictionary<Ep3_2LaneType, Material>();
    private readonly Dictionary<Ep3_2LaneType, Material> laneTrackLineMaterials = new Dictionary<Ep3_2LaneType, Material>();
    private Transform judgeInfoRoot;
    private TextMeshProUGUI judgeGradeText;
    private TextMeshProUGUI remainingMistakeText;

    private BeatMapData runtimeBeatMap;
    private Transform cachedPlayerTransform;
    private Transform runtimeVisualRoot;
    private Camera cachedMainCamera;
    private AudioListener cachedMainListener;
    private CinemachineBrain cachedMainBrain;
    private Camera runtimePuzzleCamera;
    private AudioListener runtimePuzzleListener;
    private GameObject runtimePuzzleCameraRoot;
    private Vector3 cachedPlayerPosition;
    private Quaternion cachedPlayerRotation;
    private bool cachedLookLock;
    private bool cachedJumpLock;
    private bool isRunning;
    private int nextBeatIndexToSpawn;
    private int processedBonusLifeThresholdCount;
    private int recoveredLivesFromScore;

    public bool IsRunning => isRunning;

    public void Initialize(Ep3_2Manager manager)
    {
        stageManager = manager;
        AutoResolveDependencies();
    }

    private void Awake()
    {
        AutoAssignDefaultVisualReferences();
        AutoResolveDependencies();
        SetRuntimeVisualRootActive(false);
    }

    private void OnValidate()
    {
        AutoAssignDefaultVisualReferences();
    }

    private void Update()
    {
        if (!isRunning)
        {
            return;
        }

        float currentTime = GetCurrentTime();
        SpawnPendingNotes(currentTime);
        UpdateActiveNotes(currentTime);
        HandleLaneInputs(currentTime);
        UpdateLanePadFeedbacks(Time.deltaTime);
        UpdatePuzzleCameraPose();
        UpdateJudgeInfoPanels();
        ProcessScoreLifeRecovery();

        if (scoreManager != null && GetUsedMistakeCount() >= GetTotalAllowedMistakes())
        {
            FailPuzzle();
            return;
        }

        if (nextBeatIndexToSpawn >= GetTotalBeatCount() && activeNotes.Count == 0)
        {
            CompletePuzzle();
        }
    }

    public void StartPuzzle()
    {
        AutoResolveDependencies();

        if (!HasValidSetup())
        {
            Debug.LogWarning("[Ep3_2TopDownRhythmPuzzle] 시작에 필요한 연결이 부족합니다.");
            return;
        }

        ResetRuntimeState();
        PrepareBeatMap();

        if (runtimeBeatMap == null || runtimeBeatMap.beatEvents == null || runtimeBeatMap.beatEvents.Count == 0)
        {
            Debug.LogWarning("[Ep3_2TopDownRhythmPuzzle] 생성된 비트맵이 비어 있습니다.");
            return;
        }

        scoreManager.ResetState();
        nextBeatIndexToSpawn = 0;
        processedBonusLifeThresholdCount = 0;
        recoveredLivesFromScore = 0;
        isRunning = true;

        CachePlayerState();
        LockPlayer();
        BuildRuntimeVisuals();
        SetRuntimeVisualRootActive(true);
        EnablePuzzleCamera();

        audioManager.Play();

        Debug.Log($"[Ep3_2TopDownRhythmPuzzle] 퍼즐 시작 - beatCount={runtimeBeatMap.beatEvents.Count}");
    }

    public void StopPuzzle()
    {
        if (!isRunning)
        {
            return;
        }

        isRunning = false;
        audioManager?.Stop();
        ClearActiveNotes();
        ResetLanePadFeedbackStates();
        SetRuntimeVisualRootActive(false);
        DisablePuzzleCamera();
        UnlockPlayer();
    }

    public void FailPuzzle()
    {
        if (!isRunning)
        {
            return;
        }

        StopPuzzle();
        stageManager?.OnRhythmPuzzleFailed();
    }

    private void CompletePuzzle()
    {
        if (!isRunning)
        {
            return;
        }

        int finalScore = scoreManager != null ? scoreManager.Score : 0;
        StopPuzzle();
        stageManager?.OnRhythmPuzzleCompleted(finalScore);
    }

    private void AutoResolveDependencies()
    {
        if (stageManager == null)
        {
            stageManager = GetComponent<Ep3_2Manager>();
        }

        if (stageManager == null)
        {
            stageManager = FindFirstObjectByType<Ep3_2Manager>();
        }

        if (audioManager == null)
        {
            audioManager = FindFirstObjectByType<RhythmAudioManager>();
        }

        if (scoreManager == null)
        {
            scoreManager = FindFirstObjectByType<RhythmScoreManager>();
        }

        if (effectManager == null)
        {
            effectManager = FindFirstObjectByType<RhythmEffectManager>();
        }

        if (playerMovement == null)
        {
            playerMovement = FindFirstObjectByType<PlayerMovement>();
        }

        if (playerInput == null)
        {
            playerInput = FindFirstObjectByType<PlayerInput>();
        }

        if (startPuzzleController == null && stageManager != null)
        {
            startPuzzleController = stageManager.StartPuzzleController;
        }

        if (startPuzzleController == null)
        {
            startPuzzleController = FindFirstObjectByType<Ep3_2StartPuzzle>();
        }

        if (cachedPlayerTransform == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                cachedPlayerTransform = playerObject.transform;
            }
        }

        if (cachedPlayerTransform == null && playerMovement != null)
        {
            cachedPlayerTransform = playerMovement.transform;
        }

        if (puzzleReferenceTransform == null)
        {
            puzzleReferenceTransform = GetBestReferenceTransform();
        }
    }

    private bool HasValidSetup()
    {
        return audioManager != null && scoreManager != null && cachedPlayerTransform != null;
    }

    private void PrepareBeatMap()
    {
        runtimeBeatMap = audioManager.CreateTopDownRuntimeBeatMap();
    }

    private void ResetRuntimeState()
    {
        audioManager?.Stop();
        ClearActiveNotes();
        ResetLanePadFeedbackStates();
        nextBeatIndexToSpawn = 0;
    }

    private int GetTotalBeatCount()
    {
        return runtimeBeatMap != null && runtimeBeatMap.beatEvents != null ? runtimeBeatMap.beatEvents.Count : 0;
    }

    private float GetCurrentTime()
    {
        return audioManager != null ? audioManager.GetPlaybackTime() : 0f;
    }

    private void SpawnPendingNotes(float currentTime)
    {
        while (nextBeatIndexToSpawn < GetTotalBeatCount())
        {
            BeatEvent beatEvent = runtimeBeatMap.beatEvents[nextBeatIndexToSpawn];
            if (currentTime < beatEvent.previewTime)
            {
                break;
            }

            SpawnNote(beatEvent);
            nextBeatIndexToSpawn++;
        }
    }

    private void SpawnNote(BeatEvent beatEvent)
    {
        EnsureRuntimeVisualRoot();

        Transform padTransform = GetOrCreateLanePad(beatEvent.laneType);
        if (padTransform == null)
        {
            return;
        }

        Vector3 judgePosition = padTransform.position + Vector3.up * noteHeightOffset;
        Vector3 spawnPosition = judgePosition + GetReferenceForward() * noteTravelDistance;

        UnityEngine.Object notePrefab = GetNoteVisualPrefab(beatEvent.laneType);
        GameObject noteObject = CreateRuntimeNoteObject(beatEvent.laneType, notePrefab, runtimeVisualRoot, spawnPosition);
        if (noteObject == null)
        {
            noteObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            noteObject.name = "RuntimeRhythmNote";
            noteObject.transform.SetParent(runtimeVisualRoot, false);
        }

        DisableAllColliders(noteObject);

        Ep3_2RhythmLaneNote note = noteObject.GetComponent<Ep3_2RhythmLaneNote>();
        if (note == null)
        {
            note = noteObject.AddComponent<Ep3_2RhythmLaneNote>();
        }

        float resolvedJudgeWindow = GetResolvedBadJudgeWindow(beatEvent.judgeWindow);

        note.Initialize(
            beatEvent.laneType,
            beatEvent.previewTime,
            beatEvent.judgeTime,
            resolvedJudgeWindow,
            spawnPosition,
            judgePosition,
            GetLaneColor(beatEvent.laneType),
            GetNoteVisualRotation(),
            noteScale);

        activeNotes.Add(note);
    }

    private GameObject CreateRuntimeNoteObject(Ep3_2LaneType laneType, UnityEngine.Object notePrefab, Transform parent, Vector3 spawnPosition)
    {
        if (useSpriteNoteVisuals)
        {
            Sprite sprite = GetLaneNoteSprite(laneType);
            if (sprite != null)
            {
                GameObject spriteObject = new GameObject($"RuntimeRhythmNote_{laneType}", typeof(SpriteRenderer));
                spriteObject.transform.SetParent(parent, false);
                spriteObject.transform.position = spawnPosition;

                SpriteRenderer spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
                spriteRenderer.sprite = sprite;
                spriteRenderer.color = Color.white;
                spriteRenderer.sortingOrder = 200;

                return spriteObject;
            }
        }

        if (notePrefab is Material noteMaterial)
        {
            GameObject materialNoteObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            materialNoteObject.name = $"RuntimeRhythmNote_{laneType}";
            materialNoteObject.transform.SetParent(parent, false);
            materialNoteObject.transform.position = spawnPosition;

            MeshRenderer meshRenderer = materialNoteObject.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.sharedMaterial = noteMaterial;
            }

            return materialNoteObject;
        }

        return CreateRuntimeVisualObject(notePrefab, parent, spawnPosition, Quaternion.identity);
    }

    private void UpdateActiveNotes(float currentTime)
    {
        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            Ep3_2RhythmLaneNote note = activeNotes[i];
            if (note == null)
            {
                activeNotes.RemoveAt(i);
                continue;
            }

            note.Tick(currentTime);

            if (note.IsExpired(currentTime))
            {
                scoreManager?.RegisterJudge(RhythmJudgeGrade.Miss);
                note.Resolve();
                Destroy(note.gameObject);
                activeNotes.RemoveAt(i);
            }
        }
    }

    private void HandleLaneInputs(float currentTime)
    {
        if (Keyboard.current == null)
        {
            return;
        }

        TryHandleLaneInput(Keyboard.current.dKey.wasPressedThisFrame, Ep3_2LaneType.D, currentTime);
        TryHandleLaneInput(Keyboard.current.fKey.wasPressedThisFrame, Ep3_2LaneType.F, currentTime);
        TryHandleLaneInput(Keyboard.current.spaceKey.wasPressedThisFrame, Ep3_2LaneType.Space, currentTime);
        TryHandleLaneInput(Keyboard.current.jKey.wasPressedThisFrame, Ep3_2LaneType.J, currentTime);
        TryHandleLaneInput(Keyboard.current.kKey.wasPressedThisFrame, Ep3_2LaneType.K, currentTime);
    }

    private void TryHandleLaneInput(bool wasPressed, Ep3_2LaneType laneType, float currentTime)
    {
        if (!wasPressed)
        {
            return;
        }

        Ep3_2RhythmLaneNote bestNote = null;
        float bestDelta = float.MaxValue;

        for (int i = 0; i < activeNotes.Count; i++)
        {
            Ep3_2RhythmLaneNote note = activeNotes[i];
            if (note == null || note.IsResolved || note.LaneType != laneType)
            {
                continue;
            }

            float delta = note.GetTimingDelta(currentTime);
            if (delta > note.JudgeWindow || delta >= bestDelta)
            {
                continue;
            }

            bestDelta = delta;
            bestNote = note;
        }

        if (bestNote != null)
        {
            RhythmJudgeGrade judgeGrade = EvaluateJudgeGrade(bestDelta, bestNote.JudgeWindow);
            if (judgeGrade == RhythmJudgeGrade.None)
            {
                scoreManager?.RegisterJudge(RhythmJudgeGrade.Wrong);
                TriggerLaneFeedback(laneType, LaneFeedbackKind.Wrong);
                return;
            }

            bestNote.Resolve();
            scoreManager?.RegisterJudge(judgeGrade);
            TriggerLaneFeedback(laneType, GetLaneFeedbackKind(judgeGrade));
            if (playSuccessEffectOnCorrectInput)
            {
                effectManager?.PlaySuccessEffect(GetLanePadWorldPosition(laneType));
            }
            activeNotes.Remove(bestNote);
            Destroy(bestNote.gameObject);
            return;
        }

        scoreManager?.RegisterJudge(RhythmJudgeGrade.Wrong);
        TriggerLaneFeedback(laneType, LaneFeedbackKind.Wrong);
    }

    private void BuildRuntimeVisuals()
    {
        EnsureRuntimeVisualRoot();
        GetOrCreateLaneTrack(Ep3_2LaneType.D);
        GetOrCreateLaneTrack(Ep3_2LaneType.F);
        GetOrCreateLaneTrack(Ep3_2LaneType.Space);
        GetOrCreateLaneTrack(Ep3_2LaneType.J);
        GetOrCreateLaneTrack(Ep3_2LaneType.K);
        GetOrCreateLanePad(Ep3_2LaneType.D);
        GetOrCreateLanePad(Ep3_2LaneType.F);
        GetOrCreateLanePad(Ep3_2LaneType.Space);
        GetOrCreateLanePad(Ep3_2LaneType.J);
        GetOrCreateLanePad(Ep3_2LaneType.K);
        EnsureJudgeInfoPanels();
        UpdateLanePadLayout();
        UpdateJudgeInfoPanels();
    }

    private void EnsureRuntimeVisualRoot()
    {
        if (runtimeVisualRoot != null)
        {
            return;
        }

        GameObject rootObject = new GameObject("Ep3_2RuntimeRhythmRoot");
        runtimeVisualRoot = rootObject.transform;
        runtimeVisualRoot.SetParent(transform, false);
        runtimeVisualRoot.gameObject.SetActive(false);
    }

    private void SetRuntimeVisualRootActive(bool isActive)
    {
        if (runtimeVisualRoot == null)
        {
            return;
        }

        runtimeVisualRoot.gameObject.SetActive(isActive);
    }

    private Transform GetOrCreateLanePad(Ep3_2LaneType laneType)
    {
        if (lanePads.TryGetValue(laneType, out Transform lanePad) && lanePad != null)
        {
            return lanePad;
        }

        EnsureRuntimeVisualRoot();

        UnityEngine.Object lanePadVisualReference = GetLanePadVisualPrefab(laneType);
        bool usesPrefabVisual = lanePadVisualReference != null;
        GameObject laneObject = usesPrefabVisual
            ? CreateRuntimeVisualObject(lanePadVisualReference, runtimeVisualRoot)
            : null;

        if (laneObject == null && useSimpleLanePadVisuals)
        {
            laneObject = CreateSimpleLanePadObject(laneType);
        }

        if (laneObject == null)
        {
            laneObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            laneObject.transform.SetParent(runtimeVisualRoot, false);
        }

        laneObject.name = $"LanePad_{laneType}";
        DisableAllColliders(laneObject);

        ApplyLanePadVisual(laneObject, laneType, !usesPrefabVisual);

        laneObject.transform.localScale = lanePadScale;
        lanePads[laneType] = laneObject.transform;
        lanePadUsesPrefabVisual[laneType] = usesPrefabVisual;
        CacheLaneFeedbackVisualState(laneType, laneObject);
        EnsureLanePadLabel(laneObject.transform, laneType);
        UpdateLanePadLayout();
        return laneObject.transform;
    }

    private Transform GetOrCreateLaneTrack(Ep3_2LaneType laneType)
    {
        if (laneTracks.TryGetValue(laneType, out Transform laneTrack) && laneTrack != null)
        {
            return laneTrack;
        }

        if (!showLaneTracks)
        {
            return null;
        }

        EnsureRuntimeVisualRoot();

        GameObject laneTrackObject = CreateLaneTrackObject(laneType);
        laneTrackObject.transform.SetParent(runtimeVisualRoot, false);
        laneTrackObject.name = $"LaneTrack_{laneType}";
        laneTracks[laneType] = laneTrackObject.transform;
        CacheLaneFeedbackVisualState(laneType, laneTrackObject);
        UpdateLanePadLayout();
        return laneTrackObject.transform;
    }

    private GameObject CreateLaneTrackObject(Ep3_2LaneType laneType)
    {
        GameObject rootObject = new GameObject("LaneTrackRoot");

        GameObject backgroundObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        backgroundObject.name = "Background";
        backgroundObject.transform.SetParent(rootObject.transform, false);
        DisableAllColliders(backgroundObject);
        ApplyLaneTrackVisual(backgroundObject, laneType);

        GameObject lineObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        lineObject.name = "CenterLine";
        lineObject.transform.SetParent(rootObject.transform, false);
        DisableAllColliders(lineObject);
        ApplyLaneTrackLineVisual(lineObject, laneType);

        return rootObject;
    }

    private GameObject CreateSimpleLanePadObject(Ep3_2LaneType laneType)
    {
        EnsureRuntimeVisualRoot();

        GameObject laneObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        laneObject.transform.SetParent(runtimeVisualRoot, false);
        laneObject.name = $"LanePad_{laneType}";
        return laneObject;
    }

    private void ApplyLanePadVisual(GameObject laneObject, Ep3_2LaneType laneType, bool applyTintedMaterial)
    {
        if (laneObject == null)
        {
            return;
        }

        Renderer[] renderers = laneObject.GetComponentsInChildren<Renderer>(true);
        if (renderers == null || renderers.Length == 0)
        {
            return;
        }

        Material laneMaterial = null;
        if (applyTintedMaterial)
        {
            Color laneColor = GetLaneColor(laneType);
            laneColor.a = lanePadOpacity;
            laneMaterial = GetOrCreateLanePadMaterial(laneType, laneColor);
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sortingOrder = 120;

            if (laneMaterial != null)
            {
                renderer.sharedMaterial = laneMaterial;
            }
        }
    }

    private void ApplyLaneTrackVisual(GameObject laneTrackObject, Ep3_2LaneType laneType)
    {
        if (laneTrackObject == null)
        {
            return;
        }

        Renderer[] renderers = laneTrackObject.GetComponentsInChildren<Renderer>(true);
        if (renderers == null || renderers.Length == 0)
        {
            return;
        }

        Material trackMaterial = GetOrCreateLaneTrackMaterial(laneType);

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sortingOrder = 40;

            if (trackMaterial != null)
            {
                renderer.sharedMaterial = trackMaterial;
            }
        }
    }

    private void ApplyLaneTrackLineVisual(GameObject laneLineObject, Ep3_2LaneType laneType)
    {
        if (laneLineObject == null)
        {
            return;
        }

        Renderer[] renderers = laneLineObject.GetComponentsInChildren<Renderer>(true);
        if (renderers == null || renderers.Length == 0)
        {
            return;
        }

        Material lineMaterial = GetOrCreateLaneTrackLineMaterial(laneType);

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sortingOrder = 45;

            if (lineMaterial != null)
            {
                renderer.sharedMaterial = lineMaterial;
            }
        }
    }

    private Material GetOrCreateLanePadMaterial(Ep3_2LaneType laneType, Color laneColor)
    {
        if (lanePadMaterials.TryGetValue(laneType, out Material cachedMaterial) && cachedMaterial != null)
        {
            ApplyColorToMaterial(cachedMaterial, laneColor);
            return cachedMaterial;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }

        if (shader == null)
        {
            return null;
        }

        Material material = new Material(shader)
        {
            name = $"RuntimeLanePad_{laneType}"
        };

        ApplyColorToMaterial(material, laneColor);
        material.renderQueue = 3000;
        lanePadMaterials[laneType] = material;
        return material;
    }

    private Material GetOrCreateLaneTrackMaterial(Ep3_2LaneType laneType)
    {
        if (laneTrackMaterials.TryGetValue(laneType, out Material cachedMaterial) && cachedMaterial != null)
        {
            ApplyColorToMaterial(cachedMaterial, new Color(0f, 0f, 0f, laneTrackOpacity));
            return cachedMaterial;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }

        if (shader == null)
        {
            return null;
        }

        Material laneTrackMaterial = new Material(shader)
        {
            name = $"RuntimeLaneTrack_{laneType}"
        };

        ApplyColorToMaterial(laneTrackMaterial, new Color(0f, 0f, 0f, laneTrackOpacity));
        laneTrackMaterial.renderQueue = 2950;
        laneTrackMaterials[laneType] = laneTrackMaterial;
        return laneTrackMaterial;
    }

    private Material GetOrCreateLaneTrackLineMaterial(Ep3_2LaneType laneType)
    {
        if (laneTrackLineMaterials.TryGetValue(laneType, out Material cachedMaterial) && cachedMaterial != null)
        {
            ApplyColorToMaterial(cachedMaterial, laneTrackLineColor);
            return cachedMaterial;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }

        if (shader == null)
        {
            return null;
        }

        Material laneTrackLineMaterial = new Material(shader)
        {
            name = $"RuntimeLaneTrackLine_{laneType}"
        };

        ApplyColorToMaterial(laneTrackLineMaterial, laneTrackLineColor);
        laneTrackLineMaterial.renderQueue = 2960;
        laneTrackLineMaterials[laneType] = laneTrackLineMaterial;
        return laneTrackLineMaterial;
    }

    private static void ApplyColorToMaterial(Material material, Color color)
    {
        if (material == null)
        {
            return;
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.color = color;
        }

        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 1f);
        }

        if (material.HasProperty("_Blend"))
        {
            material.SetFloat("_Blend", 0f);
        }

        if (material.HasProperty("_SrcBlend"))
        {
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        }

        if (material.HasProperty("_DstBlend"))
        {
            material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        }

        if (material.HasProperty("_ZWrite"))
        {
            material.SetFloat("_ZWrite", 0f);
        }

        if (material.HasProperty("_Cull"))
        {
            material.SetFloat("_Cull", 0f);
        }

        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }

    private void CacheLaneFeedbackVisualState(Ep3_2LaneType laneType, GameObject visualObject)
    {
        if (visualObject == null)
        {
            return;
        }

        if (!lanePadFeedbackStates.TryGetValue(laneType, out LanePadFeedbackState state) || state == null)
        {
            state = new LanePadFeedbackState();
            lanePadFeedbackStates[laneType] = state;
        }

        Renderer[] renderers = visualObject.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            Material[] materials = renderer.materials;
            for (int j = 0; j < materials.Length; j++)
            {
                Material material = materials[j];
                if (material == null)
                {
                    continue;
                }

                LanePadFeedbackMaterialState materialState = new LanePadFeedbackMaterialState
                {
                    material = material,
                    hasBaseColor = material.HasProperty("_BaseColor"),
                    hasColor = material.HasProperty("_Color"),
                    hasEmissionColor = material.HasProperty("_EmissionColor"),
                    baseColor = material.HasProperty("_BaseColor") ? material.GetColor("_BaseColor")
                        : material.HasProperty("_Color") ? material.color : Color.white,
                    baseEmissionColor = material.HasProperty("_EmissionColor") ? material.GetColor("_EmissionColor") : Color.black,
                };

                bool alreadyTracked = false;
                for (int k = 0; k < state.materials.Count; k++)
                {
                    if (state.materials[k] != null && state.materials[k].material == material)
                    {
                        alreadyTracked = true;
                        break;
                    }
                }

                if (!alreadyTracked)
                {
                    state.materials.Add(materialState);
                }
            }
        }
    }

    private void UpdateLanePadFeedbacks(float deltaTime)
    {
        if (!showLaneInputFeedback || lanePadFeedbackStates.Count == 0)
        {
            return;
        }

        bool anyAnimated = false;

        foreach (KeyValuePair<Ep3_2LaneType, LanePadFeedbackState> pair in lanePadFeedbackStates)
        {
            LanePadFeedbackState state = pair.Value;
            if (state == null)
            {
                continue;
            }

            if (state.pressAmount > 0f)
            {
                state.pressAmount = Mathf.MoveTowards(state.pressAmount, 0f, lanePressRecoverSpeed * deltaTime);
                anyAnimated = true;
            }

            if (state.flashAmount > 0f)
            {
                state.flashAmount = Mathf.MoveTowards(state.flashAmount, 0f, laneFlashRecoverSpeed * deltaTime);
                anyAnimated = true;
            }

            ApplyLanePadFeedbackVisuals(state);
        }

        if (anyAnimated)
        {
            UpdateLanePadLayout();
        }
    }

    private void ApplyLanePadFeedbackVisuals(LanePadFeedbackState state)
    {
        if (state == null)
        {
            return;
        }

        float flashLerp = state.flashAmount * laneFlashIntensity;
        Color flashEmission = state.flashColor * (state.flashAmount * laneEmissionBoost);

        for (int i = 0; i < state.materials.Count; i++)
        {
            LanePadFeedbackMaterialState materialState = state.materials[i];
            if (materialState == null || materialState.material == null)
            {
                continue;
            }

            Color targetColor = Color.Lerp(materialState.baseColor, state.flashColor, flashLerp);
            targetColor.a = materialState.baseColor.a;

            if (materialState.hasBaseColor)
            {
                materialState.material.SetColor("_BaseColor", targetColor);
            }

            if (materialState.hasColor)
            {
                materialState.material.color = targetColor;
            }

            if (materialState.hasEmissionColor)
            {
                materialState.material.SetColor("_EmissionColor", materialState.baseEmissionColor + flashEmission);
            }
        }
    }

    private RhythmJudgeGrade EvaluateJudgeGrade(float delta, float noteJudgeWindow)
    {
        float resolvedBadWindow = GetResolvedBadJudgeWindow(noteJudgeWindow);
        if (delta > resolvedBadWindow)
        {
            return RhythmJudgeGrade.None;
        }

        if (delta <= GetResolvedExcellentJudgeWindow())
        {
            return RhythmJudgeGrade.Excellent;
        }

        if (delta <= GetResolvedGoodJudgeWindow())
        {
            return RhythmJudgeGrade.Good;
        }

        return RhythmJudgeGrade.Bad;
    }

    private LaneFeedbackKind GetLaneFeedbackKind(RhythmJudgeGrade judgeGrade)
    {
        return judgeGrade switch
        {
            RhythmJudgeGrade.Bad => LaneFeedbackKind.Bad,
            RhythmJudgeGrade.Good => LaneFeedbackKind.Good,
            RhythmJudgeGrade.Excellent => LaneFeedbackKind.Excellent,
            _ => LaneFeedbackKind.Wrong
        };
    }

    private float GetResolvedExcellentJudgeWindow()
    {
        return Mathf.Max(0.02f, excellentJudgeWindow);
    }

    private float GetResolvedGoodJudgeWindow()
    {
        return Mathf.Max(GetResolvedExcellentJudgeWindow(), goodJudgeWindow);
    }

    private float GetResolvedBadJudgeWindow(float noteJudgeWindow = 0f)
    {
        return Mathf.Max(GetResolvedGoodJudgeWindow(), Mathf.Max(noteJudgeWindow, badJudgeWindow));
    }

    private void TriggerLaneFeedback(Ep3_2LaneType laneType, LaneFeedbackKind feedbackKind)
    {
        if (!showLaneInputFeedback)
        {
            return;
        }

        if (!lanePadFeedbackStates.TryGetValue(laneType, out LanePadFeedbackState state) || state == null)
        {
            return;
        }

        state.pressAmount = 1f;
        state.flashAmount = 1f;
        state.flashColor = GetLaneFeedbackColor(laneType, feedbackKind);
    }

    private Color GetLaneFeedbackColor(Ep3_2LaneType laneType, LaneFeedbackKind feedbackKind)
    {
        Color laneColor = GetLaneColor(laneType);
        return feedbackKind switch
        {
            LaneFeedbackKind.Bad => Color.Lerp(laneColor, badLaneFlashColor, 0.18f),
            LaneFeedbackKind.Good => Color.Lerp(laneColor, goodLaneFlashColor, 0.12f),
            LaneFeedbackKind.Excellent => Color.Lerp(laneColor, Color.white, 0.18f),
            _ => Color.Lerp(laneColor, wrongLaneFlashColor, 0.45f)
        };
    }

    private void UpdateLanePadLayout()
    {
        if (runtimeVisualRoot == null)
        {
            return;
        }

        Vector3 judgeCenter = GetJudgeCenterPosition();
        Vector3 right = GetReferenceRight();
        float step = Mathf.Max(0.1f, laneSpacing);

        SetLaneTrackPose(Ep3_2LaneType.D, judgeCenter - right * step * 2f);
        SetLaneTrackPose(Ep3_2LaneType.F, judgeCenter - right * step);
        SetLaneTrackPose(Ep3_2LaneType.Space, judgeCenter);
        SetLaneTrackPose(Ep3_2LaneType.J, judgeCenter + right * step);
        SetLaneTrackPose(Ep3_2LaneType.K, judgeCenter + right * step * 2f);

        SetLanePadPose(Ep3_2LaneType.D, judgeCenter - right * step * 2f);
        SetLanePadPose(Ep3_2LaneType.F, judgeCenter - right * step);
        SetLanePadPose(Ep3_2LaneType.Space, judgeCenter);
        SetLanePadPose(Ep3_2LaneType.J, judgeCenter + right * step);
        SetLanePadPose(Ep3_2LaneType.K, judgeCenter + right * step * 2f);
    }

    private GameObject CreateRuntimeVisualObject(UnityEngine.Object prefabReference, Transform parent)
    {
        return CreateRuntimeVisualObject(prefabReference, parent, Vector3.zero, Quaternion.identity, false);
    }

    private GameObject CreateRuntimeVisualObject(UnityEngine.Object prefabReference, Transform parent, Vector3 position, Quaternion rotation)
    {
        return CreateRuntimeVisualObject(prefabReference, parent, position, rotation, true);
    }

    private GameObject CreateRuntimeVisualObject(UnityEngine.Object prefabReference, Transform parent, Vector3 position, Quaternion rotation, bool useWorldPose)
    {
        if (prefabReference == null)
        {
            return null;
        }

        if (prefabReference is GameObject prefabGameObject)
        {
            return useWorldPose
                ? Instantiate(prefabGameObject, position, rotation, parent)
                : Instantiate(prefabGameObject, parent);
        }

        if (prefabReference is Component prefabComponent)
        {
            Component instance = useWorldPose
                ? Instantiate(prefabComponent, position, rotation, parent)
                : Instantiate(prefabComponent, parent);
            return instance != null ? instance.gameObject : null;
        }

        Debug.LogWarning($"[Ep3_2TopDownRhythmPuzzle] Unsupported visual prefab reference type: {prefabReference.GetType().Name}");
        return null;
    }

    private UnityEngine.Object GetNoteVisualPrefab(Ep3_2LaneType laneType)
    {
        if (useLaneSpecificNotePrefabs)
        {
            UnityEngine.Object lanePrefab = GetLaneSpecificNotePrefab(laneType);
            if (lanePrefab != null)
            {
                return lanePrefab;
            }

            lanePrefab = LoadDefaultLaneSpecificNotePrefab(laneType);
            if (lanePrefab != null)
            {
                return lanePrefab;
            }
        }

        return noteVisualPrefab;
    }

    private UnityEngine.Object GetLanePadVisualPrefab(Ep3_2LaneType laneType)
    {
        UnityEngine.Object lanePrefab = GetLaneSpecificPadPrefab(laneType);
        if (lanePrefab != null)
        {
            return lanePrefab;
        }

        lanePrefab = LoadDefaultLanePadPrefab(laneType);
        if (lanePrefab != null)
        {
            return lanePrefab;
        }

        return lanePadPrefab;
    }

    private UnityEngine.Object GetLaneSpecificNotePrefab(Ep3_2LaneType laneType)
    {
        switch (laneType)
        {
            case Ep3_2LaneType.D:
                return leftLaneNotePrefab;
            case Ep3_2LaneType.F:
                return upLaneNotePrefab;
            case Ep3_2LaneType.J:
                return rightLaneNotePrefab;
            case Ep3_2LaneType.K:
                return extraLaneNotePrefab;
            default:
                return downLaneNotePrefab;
        }
    }

    private UnityEngine.Object GetLaneSpecificPadPrefab(Ep3_2LaneType laneType)
    {
        switch (laneType)
        {
            case Ep3_2LaneType.D:
                return dLanePadPrefab;
            case Ep3_2LaneType.F:
                return fLanePadPrefab;
            case Ep3_2LaneType.Space:
                return spaceLanePadPrefab;
            case Ep3_2LaneType.J:
                return jLanePadPrefab;
            case Ep3_2LaneType.K:
                return kLanePadPrefab;
            default:
                return null;
        }
    }

    private static UnityEngine.Object LoadDefaultLaneSpecificNotePrefab(Ep3_2LaneType laneType)
    {
        switch (laneType)
        {
            case Ep3_2LaneType.D:
                return Resources.Load<GameObject>("Ep.3/Notes/Note-2");
            case Ep3_2LaneType.F:
                return Resources.Load<GameObject>("Ep.3/Notes/Note-4");
            case Ep3_2LaneType.Space:
                return Resources.Load<GameObject>("Ep.3/Notes/Note-8");
            case Ep3_2LaneType.J:
                return Resources.Load<GameObject>("Ep.3/Notes/8Notes");
            case Ep3_2LaneType.K:
                return Resources.Load<GameObject>("Ep.3/Notes/Note-8_TrebleClef");
            default:
                return null;
        }
    }

    private static UnityEngine.Object LoadDefaultLanePadPrefab(Ep3_2LaneType laneType)
    {
        switch (laneType)
        {
            case Ep3_2LaneType.D:
                return LoadResourcePrefab("Ep.3/Ep.3_2 Assets/Platform/Platform_Small_P");
            case Ep3_2LaneType.F:
                return LoadResourcePrefab("Ep.3/Ep.3_2 Assets/Platform/Platform_Small_B");
            case Ep3_2LaneType.Space:
                return LoadResourcePrefab("Ep.3/Ep.3_2 Assets/Platform/Platform_Small_G");
            case Ep3_2LaneType.J:
                return LoadResourcePrefab("Ep.3/Ep.3_2 Assets/Platform/Platform_Small_B");
            case Ep3_2LaneType.K:
                return LoadResourcePrefab("Ep.3/Ep.3_2 Assets/Platform/Platform_Small_P");
            default:
                return null;
        }
    }

    private static GameObject LoadResourcePrefab(string resourcePath)
    {
        if (string.IsNullOrWhiteSpace(resourcePath))
        {
            return null;
        }

        return Resources.Load<GameObject>(resourcePath);
    }

    private void AutoAssignDefaultVisualReferences()
    {
        if (dLanePadPrefab == null)
        {
            dLanePadPrefab = LoadDefaultLanePadPrefab(Ep3_2LaneType.D);
        }

        if (fLanePadPrefab == null)
        {
            fLanePadPrefab = LoadDefaultLanePadPrefab(Ep3_2LaneType.F);
        }

        if (spaceLanePadPrefab == null)
        {
            spaceLanePadPrefab = LoadDefaultLanePadPrefab(Ep3_2LaneType.Space);
        }

        if (jLanePadPrefab == null)
        {
            jLanePadPrefab = LoadDefaultLanePadPrefab(Ep3_2LaneType.J);
        }

        if (kLanePadPrefab == null)
        {
            kLanePadPrefab = LoadDefaultLanePadPrefab(Ep3_2LaneType.K);
        }
    }

    private Sprite GetLaneNoteSprite(Ep3_2LaneType laneType)
    {
        if (cachedLaneSprites.TryGetValue(laneType, out Sprite sprite) && sprite != null)
        {
            return sprite;
        }

        string resourcePath = laneType switch
        {
            Ep3_2LaneType.D => "Ep.3/Notes/Sprites/Note-2",
            Ep3_2LaneType.F => "Ep.3/Notes/Sprites/Note-4",
            Ep3_2LaneType.Space => "Ep.3/Notes/Sprites/Note-8",
            Ep3_2LaneType.J => "Ep.3/Notes/Sprites/8Notes",
            Ep3_2LaneType.K => "Ep.3/Notes/Sprites/Note-Treble Clef",
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(resourcePath))
        {
            return null;
        }

        sprite = Resources.Load<Sprite>(resourcePath);
        if (sprite != null)
        {
            cachedLaneSprites[laneType] = sprite;
        }

        return sprite;
    }

    private void SetLanePadPose(Ep3_2LaneType laneType, Vector3 worldPosition)
    {
        if (!lanePads.TryGetValue(laneType, out Transform lanePad) || lanePad == null)
        {
            return;
        }

        float pressAmount = 0f;
        if (lanePadFeedbackStates.TryGetValue(laneType, out LanePadFeedbackState feedbackState) && feedbackState != null)
        {
            pressAmount = feedbackState.pressAmount;
        }

        lanePad.position = worldPosition - Vector3.up * (lanePressDepth * pressAmount);
        bool usesPrefabVisual = lanePadUsesPrefabVisual.TryGetValue(laneType, out bool usesPrefab) && usesPrefab;
        Vector3 rotationOffset = usesPrefabVisual ? prefabLanePadEulerOffset : lanePadEulerOffset;
        lanePad.rotation = Quaternion.LookRotation(GetReferenceForward(), Vector3.up) * Quaternion.Euler(rotationOffset);
        lanePad.localScale = GetPressedLanePadScale(pressAmount);
        UpdateLanePadLabelPose(lanePad, laneType);
    }

    private void SetLaneTrackPose(Ep3_2LaneType laneType, Vector3 judgeWorldPosition)
    {
        if (!showLaneTracks || !laneTracks.TryGetValue(laneType, out Transform laneTrack) || laneTrack == null)
        {
            return;
        }

        Vector3 forward = GetReferenceForward();
        float trackLength = Mathf.Max(1f, laneTrackLength, noteTravelDistance);
        laneTrack.position = judgeWorldPosition + forward * (trackLength * 0.5f) + Vector3.up * laneTrackHeightOffset;
        laneTrack.rotation = Quaternion.LookRotation(forward, Vector3.up) * Quaternion.Euler(laneTrackEulerOffset);
        laneTrack.localScale = Vector3.one;

        Transform backgroundTransform = laneTrack.Find("Background");
        if (backgroundTransform != null)
        {
            backgroundTransform.localPosition = Vector3.zero;
            backgroundTransform.localRotation = Quaternion.identity;
            backgroundTransform.localScale = new Vector3(Mathf.Max(0.2f, laneTrackWidth), trackLength, 1f);
        }

        Transform lineTransform = laneTrack.Find("CenterLine");
        if (lineTransform != null)
        {
            bool showCenterLine = laneTrackLineWidth > 0.001f && laneTrackLineColor.a > 0.001f;
            lineTransform.gameObject.SetActive(showCenterLine);
            if (showCenterLine)
            {
                lineTransform.localPosition = new Vector3(0f, 0f, -0.01f);
                lineTransform.localRotation = Quaternion.identity;
                lineTransform.localScale = new Vector3(Mathf.Max(0.01f, laneTrackLineWidth), trackLength, 1f);
            }
        }
    }

    private Vector3 GetPressedLanePadScale(float pressAmount)
    {
        Vector3 scale = lanePadScale;
        float horizontalScale = Mathf.Lerp(1f, 0.96f, pressAmount);
        float verticalScale = Mathf.Lerp(1f, lanePressedScaleMultiplier, pressAmount);

        scale.x *= horizontalScale;
        scale.y *= verticalScale;
        scale.z *= horizontalScale;
        return scale;
    }

    private Vector3 GetLanePadWorldPosition(Ep3_2LaneType laneType)
    {
        if (lanePads.TryGetValue(laneType, out Transform lanePad) && lanePad != null)
        {
            return lanePad.position;
        }

        return GetJudgeCenterPosition();
    }

    private void ResetLanePadFeedbackStates()
    {
        foreach (KeyValuePair<Ep3_2LaneType, LanePadFeedbackState> pair in lanePadFeedbackStates)
        {
            LanePadFeedbackState state = pair.Value;
            if (state == null)
            {
                continue;
            }

            state.pressAmount = 0f;
            state.flashAmount = 0f;
            ApplyLanePadFeedbackVisuals(state);
        }

        if (lanePadFeedbackStates.Count > 0)
        {
            UpdateLanePadLayout();
        }
    }

    private Color GetLaneColor(Ep3_2LaneType laneType)
    {
        switch (laneType)
        {
            case Ep3_2LaneType.D:
                return leftLaneColor;
            case Ep3_2LaneType.F:
                return upLaneColor;
            case Ep3_2LaneType.J:
                return rightLaneColor;
            case Ep3_2LaneType.K:
                return extraLaneColor;
            default:
                return downLaneColor;
        }
    }

    private Transform GetBestReferenceTransform()
    {
        if (puzzleReferenceTransform != null)
        {
            return puzzleReferenceTransform;
        }

        if (judgeCenterAnchor != null)
        {
            return judgeCenterAnchor;
        }

        if (playerHoldPoint != null)
        {
            return playerHoldPoint;
        }

        if (startPuzzleController != null)
        {
            return startPuzzleController.transform;
        }

        if (stageManager != null)
        {
            Transform managerReference = stageManager.PuzzleSetupReferenceTransform;
            if (managerReference != null)
            {
                return managerReference;
            }
        }

        if (audioManager != null)
        {
            return audioManager.transform;
        }

        if (cachedPlayerTransform != null)
        {
            return cachedPlayerTransform;
        }

        return transform;
    }

    private Vector3 GetFallbackJudgeCenterPosition()
    {
        Transform reference = GetBestReferenceTransform();
        if (reference == null)
        {
            return transform.position + fallbackJudgeCenterOffset;
        }

        return reference.position + reference.TransformDirection(fallbackJudgeCenterOffset);
    }

    private Vector3 GetResolvedPlayerHoldPosition()
    {
        if (playerHoldPoint != null)
        {
            return playerHoldPoint.position + Vector3.up * playerHoldHeightOffset;
        }

        Transform reference = GetBestReferenceTransform();
        if (reference != null)
        {
            return reference.position
                + reference.TransformDirection(fallbackHoldLocalOffset)
                + Vector3.up * playerHoldHeightOffset;
        }

        return GetFallbackJudgeCenterPosition() - GetReferenceForward() * fallbackHoldDistanceFromJudge;
    }

    private Quaternion GetResolvedPlayerHoldRotation()
    {
        if (playerHoldPoint != null)
        {
            return playerHoldPoint.rotation;
        }

        Vector3 forward = GetReferenceForward();
        if (forward.sqrMagnitude <= 0.0001f)
        {
            return cachedPlayerTransform != null ? cachedPlayerTransform.rotation : Quaternion.identity;
        }

        return Quaternion.LookRotation(forward, Vector3.up);
    }

    private Vector3 GetJudgeCenterPosition()
    {
        if (judgeCenterAnchor != null)
        {
            return judgeCenterAnchor.position;
        }

        return GetFallbackJudgeCenterPosition();
    }

    private Vector3 GetReferenceForward()
    {
        Transform reference = judgeCenterAnchor != null ? judgeCenterAnchor : GetBestReferenceTransform();
        Vector3 forward = Vector3.ProjectOnPlane(reference.forward, Vector3.up);
        if (forward.sqrMagnitude <= 0.0001f)
        {
            forward = cachedPlayerTransform != null ? Vector3.ProjectOnPlane(cachedPlayerTransform.forward, Vector3.up) : Vector3.forward;
        }

        return forward.normalized;
    }

    private Vector3 GetReferenceRight()
    {
        return Vector3.Cross(Vector3.up, GetReferenceForward()).normalized;
    }

    private void CachePlayerState()
    {
        if (cachedPlayerTransform == null)
        {
            return;
        }

        cachedPlayerPosition = cachedPlayerTransform.position;
        cachedPlayerRotation = cachedPlayerTransform.rotation;

        if (playerInput != null)
        {
            cachedLookLock = playerInput.isLookLock;
            cachedJumpLock = playerInput.isJumpLock;
        }
    }

    private void LockPlayer()
    {
        if (!lockPlayerDuringPuzzle)
        {
            return;
        }

        if (teleportPlayerToHoldPoint && cachedPlayerTransform != null)
        {
            cachedPlayerTransform.SetPositionAndRotation(
                GetResolvedPlayerHoldPosition(),
                GetResolvedPlayerHoldRotation());
        }

        if (playerMovement != null)
        {
            playerMovement.SetMoveLock(true);
            playerMovement.SetInputLock(true);
        }

        if (playerInput != null)
        {
            playerInput.ResetInputState();
            playerInput.isLookLock = true;
            playerInput.isJumpLock = true;
        }
    }

    private void UnlockPlayer()
    {
        if (playerMovement != null)
        {
            playerMovement.SetMoveLock(false);
            playerMovement.SetInputLock(false);
        }

        if (playerInput != null)
        {
            playerInput.ResetInputState();
            playerInput.isLookLock = cachedLookLock;
            playerInput.isJumpLock = cachedJumpLock;
        }
    }

    private void EnablePuzzleCamera()
    {
        if (!useRuntimePuzzleCamera)
        {
            return;
        }

        if (cachedMainCamera == null)
        {
            cachedMainCamera = Camera.main;
            if (cachedMainCamera != null)
            {
                cachedMainListener = cachedMainCamera.GetComponent<AudioListener>();
                cachedMainBrain = cachedMainCamera.GetComponent<CinemachineBrain>();
            }
        }

        EnsureRuntimePuzzleCamera();

        if (cachedMainCamera != null && cachedMainCamera != runtimePuzzleCamera)
        {
            if (cachedMainCamera.CompareTag("MainCamera"))
            {
                cachedMainCamera.tag = "Untagged";
            }

            cachedMainCamera.enabled = false;
            if (cachedMainListener != null)
            {
                cachedMainListener.enabled = false;
            }

            if (cachedMainBrain != null)
            {
                cachedMainBrain.enabled = false;
            }
        }

        runtimePuzzleCameraRoot.SetActive(true);
        runtimePuzzleCamera.enabled = true;
        runtimePuzzleListener.enabled = true;
        runtimePuzzleCamera.tag = "MainCamera";
        UpdatePuzzleCameraPose(forceSnap: true);
    }

    private void DisablePuzzleCamera()
    {
        if (!useRuntimePuzzleCamera || runtimePuzzleCamera == null)
        {
            return;
        }

        runtimePuzzleCamera.tag = "Untagged";
        runtimePuzzleCamera.enabled = false;
        runtimePuzzleListener.enabled = false;
        runtimePuzzleCameraRoot.SetActive(false);

        if (cachedMainCamera != null)
        {
            cachedMainCamera.enabled = true;
            cachedMainCamera.tag = "MainCamera";

            if (cachedMainListener != null)
            {
                cachedMainListener.enabled = true;
            }

            if (cachedMainBrain != null)
            {
                cachedMainBrain.enabled = true;
            }
        }
    }

    private void EnsureRuntimePuzzleCamera()
    {
        if (runtimePuzzleCamera != null)
        {
            return;
        }

        runtimePuzzleCameraRoot = new GameObject("Ep3_2RuntimePuzzleCamera");
        runtimePuzzleCamera = runtimePuzzleCameraRoot.AddComponent<Camera>();
        runtimePuzzleListener = runtimePuzzleCameraRoot.AddComponent<AudioListener>();
        runtimePuzzleCamera.clearFlags = CameraClearFlags.Skybox;
        runtimePuzzleCamera.nearClipPlane = 0.1f;
        runtimePuzzleCamera.farClipPlane = 200f;
        runtimePuzzleCamera.depth = 100f;
        runtimePuzzleCamera.orthographic = useOrthographicPuzzleCamera;
        runtimePuzzleCamera.orthographicSize = orthographicSize;
        runtimePuzzleCameraRoot.SetActive(false);
    }

    private void UpdatePuzzleCameraPose(bool forceSnap = false)
    {
        if (!useRuntimePuzzleCamera || runtimePuzzleCamera == null || !runtimePuzzleCamera.enabled)
        {
            return;
        }

        Vector3 focusPoint = GetJudgeCenterPosition()
            + GetReferenceRight() * cameraFocusOffset.x
            + Vector3.up * cameraFocusOffset.y
            + GetReferenceForward() * cameraFocusOffset.z;
        Vector3 forward = GetReferenceForward();
        Vector3 targetPosition = focusPoint + Vector3.up * cameraHeight - forward * cameraDistance;
        Quaternion targetRotation = Quaternion.LookRotation((focusPoint - targetPosition).normalized, Vector3.up);

        runtimePuzzleCamera.orthographic = useOrthographicPuzzleCamera;
        if (useOrthographicPuzzleCamera)
        {
            runtimePuzzleCamera.orthographicSize = orthographicSize;
        }

        if (forceSnap)
        {
            runtimePuzzleCamera.transform.SetPositionAndRotation(targetPosition, targetRotation);
            return;
        }

        runtimePuzzleCamera.transform.position = Vector3.Lerp(
            runtimePuzzleCamera.transform.position,
            targetPosition,
            Time.deltaTime * cameraFollowSmooth);

        runtimePuzzleCamera.transform.rotation = Quaternion.Slerp(
            runtimePuzzleCamera.transform.rotation,
            targetRotation,
            Time.deltaTime * cameraFollowSmooth);
    }

    private Quaternion GetNoteVisualRotation()
    {
        Quaternion offsetRotation = Quaternion.Euler(noteVisualEulerOffset);

        if (runtimePuzzleCamera != null)
        {
            Quaternion cameraFacingRotation = Quaternion.LookRotation(-runtimePuzzleCamera.transform.forward, runtimePuzzleCamera.transform.up);
            return cameraFacingRotation * offsetRotation;
        }

        Vector3 forward = GetReferenceForward();
        if (forward.sqrMagnitude <= 0.0001f)
        {
            forward = Vector3.forward;
        }

        return Quaternion.LookRotation(-forward, Vector3.up) * offsetRotation;
    }

    private void ClearActiveNotes()
    {
        for (int i = 0; i < activeNotes.Count; i++)
        {
            if (activeNotes[i] != null)
            {
                Destroy(activeNotes[i].gameObject);
            }
        }

        activeNotes.Clear();
    }

    private void DisableAllColliders(GameObject target)
    {
        Collider[] colliders = target.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        NoteDeco[] noteDecorations = target.GetComponentsInChildren<NoteDeco>(true);
        for (int i = 0; i < noteDecorations.Length; i++)
        {
            noteDecorations[i].enabled = false;
        }
    }

    private void EnsureLanePadLabel(Transform lanePad, Ep3_2LaneType laneType)
    {
        if (!showLaneLabels || lanePad == null)
        {
            return;
        }

        string labelText = GetLaneLabelText(laneType);
        bool isWideLabel = labelText.Length > 1;
        Vector2 panelSize = isWideLabel ? new Vector2(260f, 96f) : new Vector2(120f, 96f);

        Transform labelCanvasTransform = lanePad.Find("LaneLabelCanvas");
        Canvas labelCanvas = labelCanvasTransform != null ? labelCanvasTransform.GetComponent<Canvas>() : null;
        TextMeshProUGUI textMesh = null;

        if (labelCanvas == null)
        {
            GameObject canvasObject = new GameObject("LaneLabelCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            labelCanvasTransform = canvasObject.transform;
            labelCanvasTransform.SetParent(lanePad, false);

            labelCanvas = canvasObject.GetComponent<Canvas>();
            labelCanvas.renderMode = RenderMode.WorldSpace;
            labelCanvas.overrideSorting = true;
            labelCanvas.sortingOrder = 500;

            CanvasScaler canvasScaler = canvasObject.GetComponent<CanvasScaler>();
            canvasScaler.dynamicPixelsPerUnit = 10f;

            GraphicRaycaster raycaster = canvasObject.GetComponent<GraphicRaycaster>();
            raycaster.enabled = false;

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.sizeDelta = panelSize;

            GameObject backgroundObject = new GameObject("LabelBackground", typeof(Image));
            backgroundObject.transform.SetParent(labelCanvasTransform, false);
            Image background = backgroundObject.GetComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.58f);
            background.raycastTarget = false;

            RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;

            GameObject textObject = new GameObject("LaneLabel", typeof(TextMeshProUGUI));
            textObject.transform.SetParent(labelCanvasTransform, false);
            textMesh = textObject.GetComponent<TextMeshProUGUI>();

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(20f, 8f);
            textRect.offsetMax = new Vector2(-20f, -8f);
        }
        else
        {
            Transform textTransform = labelCanvasTransform.Find("LaneLabel");
            if (textTransform != null)
            {
                textMesh = textTransform.GetComponent<TextMeshProUGUI>();
            }
        }

        if (textMesh == null)
        {
            return;
        }

        RectTransform existingCanvasRect = labelCanvasTransform as RectTransform;
        if (existingCanvasRect != null)
        {
            existingCanvasRect.sizeDelta = panelSize;
        }

        textMesh.text = labelText;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.enableWordWrapping = false;
        textMesh.richText = false;
        textMesh.fontSize = isWideLabel ? Mathf.RoundToInt(laneLabelFontSize * 0.72f) : laneLabelFontSize;
        textMesh.color = laneLabelColor;
        textMesh.fontStyle = FontStyles.Bold;
        textMesh.outlineWidth = 0.12f;
        textMesh.outlineColor = new Color(0f, 0f, 0f, 0.9f);
        textMesh.enableAutoSizing = true;
        textMesh.fontSizeMin = isWideLabel ? 18 : 24;
        textMesh.fontSizeMax = Mathf.Max(42, textMesh.fontSize);

        if (laneLabelFontAsset == null)
        {
            laneLabelFontAsset = TMP_Settings.defaultFontAsset;
        }

        if (laneLabelFontAsset == null)
        {
            laneLabelFontAsset = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }

        if (laneLabelFontAsset != null)
        {
            textMesh.font = laneLabelFontAsset;
        }
    }

    private void EnsureJudgeInfoPanels()
    {
        if (!showJudgeInfoPanels || runtimeVisualRoot == null)
        {
            return;
        }

        if (judgeInfoRoot == null)
        {
            GameObject rootObject = new GameObject("JudgeInfoPanels");
            judgeInfoRoot = rootObject.transform;
            judgeInfoRoot.SetParent(runtimeVisualRoot, false);
        }

        if (judgeGradeText == null)
        {
            judgeGradeText = CreateJudgeInfoPanel("JudgeGradePanel");
        }

        if (remainingMistakeText == null)
        {
            remainingMistakeText = CreateJudgeInfoPanel("RemainingMistakePanel");
        }
    }

    private TextMeshProUGUI CreateJudgeInfoPanel(string panelName)
    {
        if (judgeInfoRoot == null)
        {
            return null;
        }

        GameObject canvasObject = new GameObject(panelName, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(judgeInfoRoot, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 510;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;

        GraphicRaycaster raycaster = canvasObject.GetComponent<GraphicRaycaster>();
        raycaster.enabled = false;

        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = judgeInfoPanelSize;

        GameObject backgroundObject = new GameObject("Background", typeof(Image));
        backgroundObject.transform.SetParent(canvasObject.transform, false);
        Image background = backgroundObject.GetComponent<Image>();
        background.color = judgeInfoPanelBackgroundColor;
        background.raycastTarget = false;

        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        GameObject textObject = new GameObject("Text", typeof(TextMeshProUGUI));
        textObject.transform.SetParent(canvasObject.transform, false);
        TextMeshProUGUI textMesh = textObject.GetComponent<TextMeshProUGUI>();
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.enableWordWrapping = false;
        textMesh.richText = false;
        textMesh.color = judgeInfoTextColor;
        textMesh.fontStyle = FontStyles.Bold;
        textMesh.outlineWidth = 0.12f;
        textMesh.outlineColor = new Color(0f, 0f, 0f, 0.9f);
        textMesh.enableAutoSizing = true;
        textMesh.fontSizeMin = 18;
        textMesh.fontSizeMax = 54;

        if (laneLabelFontAsset == null)
        {
            laneLabelFontAsset = TMP_Settings.defaultFontAsset;
        }

        if (laneLabelFontAsset == null)
        {
            laneLabelFontAsset = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }

        if (laneLabelFontAsset != null)
        {
            textMesh.font = laneLabelFontAsset;
        }

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(18f, 12f);
        textRect.offsetMax = new Vector2(-18f, -12f);

        return textMesh;
    }

    private void UpdateJudgeInfoPanels()
    {
        if (!showJudgeInfoPanels)
        {
            return;
        }

        EnsureJudgeInfoPanels();
        if (judgeInfoRoot == null || judgeGradeText == null || remainingMistakeText == null)
        {
            return;
        }

        Vector3 judgeCenter = GetJudgeCenterPosition();
        Vector3 forward = GetReferenceForward();
        Quaternion rotation = GetLaneLabelRotation();
        Vector3 centerPosition = judgeCenter - forward * judgeInfoPanelOffset + Vector3.up * judgeInfoPanelHeightOffset;
        judgeInfoRoot.position = centerPosition;
        judgeInfoRoot.rotation = rotation;
        judgeInfoRoot.localScale = Vector3.one * Mathf.Max(0.001f, judgeInfoPanelCharacterSize);

        float verticalGap = Mathf.Max(16f, judgeInfoPanelGap);
        PositionJudgeInfoPanel(judgeGradeText, new Vector2(0f, 0f));
        PositionJudgeInfoPanel(remainingMistakeText, new Vector2(0f, -(judgeInfoPanelSize.y + verticalGap)));

        RhythmJudgeGrade judgeGrade = scoreManager != null ? scoreManager.LastJudgeGrade : RhythmJudgeGrade.None;
        judgeGradeText.text = GetJudgeInfoLabel(judgeGrade);
        judgeGradeText.color = GetJudgeGradeTextColor(judgeGrade);
        remainingMistakeText.text = GetDisplayRemainingMistakeLabel();
        remainingMistakeText.color = GetDisplayRemainingMistakeTextColor();
    }

    private void PositionJudgeInfoPanel(TextMeshProUGUI textMesh, Vector2 anchoredPosition)
    {
        if (textMesh == null)
        {
            return;
        }

        RectTransform canvasRect = textMesh.canvas != null ? textMesh.canvas.GetComponent<RectTransform>() : null;
        if (canvasRect == null)
        {
            return;
        }

        canvasRect.sizeDelta = judgeInfoPanelSize;
        canvasRect.anchoredPosition = anchoredPosition;
        canvasRect.localRotation = Quaternion.identity;
        canvasRect.localScale = Vector3.one;

        Transform backgroundTransform = canvasRect.Find("Background");
        if (backgroundTransform != null)
        {
            Image background = backgroundTransform.GetComponent<Image>();
            if (background != null)
            {
                background.color = judgeInfoPanelBackgroundColor;
            }
        }
    }

    private string GetJudgeInfoLabel(RhythmJudgeGrade judgeGrade)
    {
        return judgeGrade switch
        {
            RhythmJudgeGrade.Bad => "BAD",
            RhythmJudgeGrade.Good => "GOOD",
            RhythmJudgeGrade.Excellent => "EXCELLENT",
            RhythmJudgeGrade.Wrong => "WRONG",
            RhythmJudgeGrade.Miss => "MISS",
            _ => "READY"
        };
    }

    private Color GetJudgeGradeTextColor(RhythmJudgeGrade judgeGrade)
    {
        return judgeGrade switch
        {
            RhythmJudgeGrade.Bad => badLaneFlashColor,
            RhythmJudgeGrade.Good => goodLaneFlashColor,
            RhythmJudgeGrade.Excellent => excellentLaneFlashColor,
            RhythmJudgeGrade.Wrong => wrongLaneFlashColor,
            RhythmJudgeGrade.Miss => new Color(1f, 0.5f, 0.5f, 1f),
            _ => judgeInfoTextColor
        };
    }

    private string GetRemainingMistakeLabel()
    {
        int totalAllowedMistakes = GetTotalAllowedMistakes();
        int remainingMistakes = Mathf.Max(0, totalAllowedMistakes - GetUsedMistakeCount());
        int bonusLives = recoveredLivesFromScore;
        if (recoveredLivesFromScore > 0)
        {
            return $"?⑥? ?ㅼ닔\n{remainingMistakes} / {totalAllowedMistakes} (+{bonusLives})";
        }
        return $"남은 실수\n{remainingMistakes} / {maxAllowedMistakes}";
    }

    private Color GetRemainingMistakeTextColor()
    {
        int remainingMistakes = Mathf.Max(0, GetTotalAllowedMistakes() - GetUsedMistakeCount());

        if (remainingMistakes <= 1)
        {
            return wrongLaneFlashColor;
        }

        if (remainingMistakes <= 3)
        {
            return badLaneFlashColor;
        }

        return judgeInfoTextColor;
    }

    private int GetUsedMistakeCount()
    {
        return Mathf.Max(0, GetRawMistakeCount() - recoveredLivesFromScore);
    }

    private int GetTotalAllowedMistakes()
    {
        return Mathf.Max(1, maxAllowedMistakes);
    }

    private int GetRawMistakeCount()
    {
        return scoreManager != null ? scoreManager.MissCount + scoreManager.WrongCount : 0;
    }

    private void ProcessScoreLifeRecovery()
    {
        if (scoreManager == null)
        {
            return;
        }

        int earnedThresholdCount = scoreManager.BonusLivesEarned;
        while (processedBonusLifeThresholdCount < earnedThresholdCount)
        {
            processedBonusLifeThresholdCount++;

            if (GetRawMistakeCount() > recoveredLivesFromScore)
            {
                recoveredLivesFromScore++;
                Debug.Log($"[Ep3_2TopDownRhythmPuzzle] 점수 보너스로 라이프 1 회복. recovered={recoveredLivesFromScore}, rawMistakes={GetRawMistakeCount()}");
            }
        }
    }

    private string GetDisplayRemainingMistakeLabel()
    {
        int totalAllowedMistakes = GetTotalAllowedMistakes();
        int remainingMistakes = Mathf.Max(0, totalAllowedMistakes - GetUsedMistakeCount());

        if (recoveredLivesFromScore > 0)
        {
            return $"남은 실수\n{remainingMistakes} / {totalAllowedMistakes} (회복 {recoveredLivesFromScore})";
        }

        return $"남은 실수\n{remainingMistakes} / {totalAllowedMistakes}";
    }

    private Color GetDisplayRemainingMistakeTextColor()
    {
        int remainingMistakes = Mathf.Max(0, GetTotalAllowedMistakes() - GetUsedMistakeCount());

        if (remainingMistakes <= 1)
        {
            return wrongLaneFlashColor;
        }

        if (remainingMistakes <= 3)
        {
            return badLaneFlashColor;
        }

        return judgeInfoTextColor;
    }

    private void UpdateLanePadLabelPose(Transform lanePad, Ep3_2LaneType laneType)
    {
        if (!showLaneLabels || lanePad == null)
        {
            return;
        }

        Transform labelCanvasTransform = lanePad.Find("LaneLabelCanvas");
        if (labelCanvasTransform == null)
        {
            return;
        }

        labelCanvasTransform.position = lanePad.position + Vector3.up * laneLabelHeightOffset;
        labelCanvasTransform.rotation = GetLaneLabelRotation();
        labelCanvasTransform.localScale = Vector3.one * Mathf.Max(0.001f, laneLabelCharacterSize);

        TextMeshProUGUI textMesh = labelCanvasTransform.GetComponentInChildren<TextMeshProUGUI>(true);
        if (textMesh != null)
        {
            textMesh.text = GetLaneLabelText(laneType);
            textMesh.color = laneLabelColor;
        }
    }

    private Quaternion GetLaneLabelRotation()
    {
        if (runtimePuzzleCamera != null)
        {
            return Quaternion.LookRotation(runtimePuzzleCamera.transform.forward, runtimePuzzleCamera.transform.up);
        }

        Vector3 forward = GetReferenceForward();
        if (forward.sqrMagnitude <= 0.0001f)
        {
            forward = Vector3.forward;
        }

        return Quaternion.LookRotation(forward, Vector3.up);
    }

    private static string GetLaneLabelText(Ep3_2LaneType laneType)
    {
        switch (laneType)
        {
            case Ep3_2LaneType.D:
                return "D";
            case Ep3_2LaneType.F:
                return "F";
            case Ep3_2LaneType.Space:
                return "SPACE";
            case Ep3_2LaneType.J:
                return "J";
            case Ep3_2LaneType.K:
                return "K";
            default:
                return string.Empty;
        }
    }

    private void OnDestroy()
    {
        DisablePuzzleCamera();
        UnlockPlayer();

        foreach (Material material in lanePadMaterials.Values)
        {
            if (material != null)
            {
                Destroy(material);
            }
        }

        lanePadMaterials.Clear();
    }
}
