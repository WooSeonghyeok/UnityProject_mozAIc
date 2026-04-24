using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class Ep3_2TopDownRhythmPuzzle : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private Ep3_2Manager stageManager;
    [SerializeField] private RhythmAudioManager audioManager;
    [SerializeField] private RhythmScoreManager scoreManager;
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
    [SerializeField] private float laneSpacing = 3f;
    [SerializeField] private float laneForwardSpacing = 2.6f;
    [SerializeField] private float noteTravelDistance = 22f;
    [SerializeField] private float noteHeightOffset = 0.4f;

    [Header("카메라")]
    [SerializeField] private bool useRuntimePuzzleCamera = true;
    [SerializeField] private float cameraHeight = 22f;
    [SerializeField] private float cameraDistance = 2.5f;
    [SerializeField] private Vector3 cameraFocusOffset = new Vector3(0f, 1.4f, 0f);
    [SerializeField] private float cameraFollowSmooth = 12f;

    [Header("비주얼")]
    [SerializeField] private UnityEngine.Object noteVisualPrefab;
    [SerializeField] private UnityEngine.Object lanePadPrefab;
    [SerializeField] private Vector3 noteScale = new Vector3(2.2f, 0.35f, 2.2f);
    [SerializeField] private Vector3 lanePadScale = new Vector3(2.6f, 0.2f, 2.6f);
    [SerializeField] private Color leftLaneColor = new Color(1f, 0.56f, 0.62f, 1f);
    [SerializeField] private Color upLaneColor = new Color(1f, 0.85f, 0.45f, 1f);
    [SerializeField] private Color downLaneColor = new Color(0.48f, 0.88f, 0.96f, 1f);
    [SerializeField] private Color rightLaneColor = new Color(0.73f, 0.62f, 1f, 1f);

    [Header("실패 조건")]
    [SerializeField] private int maxAllowedMistakes = 8;

    private readonly List<Ep3_2RhythmLaneNote> activeNotes = new List<Ep3_2RhythmLaneNote>();
    private readonly Dictionary<Ep3_2LaneType, Transform> lanePads = new Dictionary<Ep3_2LaneType, Transform>();

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

    public bool IsRunning => isRunning;

    public void Initialize(Ep3_2Manager manager)
    {
        stageManager = manager;
        AutoResolveDependencies();
    }

    private void Awake()
    {
        AutoResolveDependencies();
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
        UpdatePuzzleCameraPose();

        if (scoreManager != null && scoreManager.MissCount + scoreManager.WrongCount >= maxAllowedMistakes)
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
        isRunning = true;

        CachePlayerState();
        LockPlayer();
        BuildRuntimeVisuals();
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

        GameObject noteObject = CreateRuntimeVisualObject(noteVisualPrefab, runtimeVisualRoot, spawnPosition, Quaternion.identity);
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

        note.Initialize(
            beatEvent.laneType,
            beatEvent.previewTime,
            beatEvent.judgeTime,
            beatEvent.judgeWindow,
            spawnPosition,
            judgePosition,
            GetLaneColor(beatEvent.laneType),
            noteScale);

        activeNotes.Add(note);
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
                scoreManager?.RegisterMiss();
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

        TryHandleLaneInput(Keyboard.current.aKey.wasPressedThisFrame, Ep3_2LaneType.Left, currentTime);
        TryHandleLaneInput(Keyboard.current.wKey.wasPressedThisFrame, Ep3_2LaneType.Up, currentTime);
        TryHandleLaneInput(Keyboard.current.sKey.wasPressedThisFrame, Ep3_2LaneType.Down, currentTime);
        TryHandleLaneInput(Keyboard.current.dKey.wasPressedThisFrame, Ep3_2LaneType.Right, currentTime);
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
            bestNote.Resolve();
            scoreManager?.RegisterCorrectStep();
            activeNotes.Remove(bestNote);
            Destroy(bestNote.gameObject);
            return;
        }

        scoreManager?.RegisterWrongStep();
    }

    private void BuildRuntimeVisuals()
    {
        EnsureRuntimeVisualRoot();
        GetOrCreateLanePad(Ep3_2LaneType.Left);
        GetOrCreateLanePad(Ep3_2LaneType.Up);
        GetOrCreateLanePad(Ep3_2LaneType.Down);
        GetOrCreateLanePad(Ep3_2LaneType.Right);
        UpdateLanePadLayout();
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
    }

    private Transform GetOrCreateLanePad(Ep3_2LaneType laneType)
    {
        if (lanePads.TryGetValue(laneType, out Transform lanePad) && lanePad != null)
        {
            return lanePad;
        }

        EnsureRuntimeVisualRoot();

        GameObject laneObject = CreateRuntimeVisualObject(lanePadPrefab, runtimeVisualRoot);
        if (laneObject == null)
        {
            laneObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            laneObject.transform.SetParent(runtimeVisualRoot, false);
        }

        laneObject.name = $"LanePad_{laneType}";
        DisableAllColliders(laneObject);

        MeshRenderer renderer = laneObject.GetComponentInChildren<MeshRenderer>(true);
        if (renderer != null)
        {
            renderer.material.color = GetLaneColor(laneType) * 0.6f;
        }

        laneObject.transform.localScale = lanePadScale;
        lanePads[laneType] = laneObject.transform;
        UpdateLanePadLayout();
        return laneObject.transform;
    }

    private void UpdateLanePadLayout()
    {
        if (runtimeVisualRoot == null)
        {
            return;
        }

        Vector3 judgeCenter = GetJudgeCenterPosition();
        Vector3 forward = GetReferenceForward();
        Vector3 right = GetReferenceRight();

        SetLanePadPose(Ep3_2LaneType.Left, judgeCenter - right * laneSpacing);
        SetLanePadPose(Ep3_2LaneType.Right, judgeCenter + right * laneSpacing);
        SetLanePadPose(Ep3_2LaneType.Up, judgeCenter + forward * laneForwardSpacing);
        SetLanePadPose(Ep3_2LaneType.Down, judgeCenter - forward * laneForwardSpacing);
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

    private void SetLanePadPose(Ep3_2LaneType laneType, Vector3 worldPosition)
    {
        if (!lanePads.TryGetValue(laneType, out Transform lanePad) || lanePad == null)
        {
            return;
        }

        lanePad.position = worldPosition;
        lanePad.rotation = Quaternion.LookRotation(GetReferenceForward(), Vector3.up);
    }

    private Color GetLaneColor(Ep3_2LaneType laneType)
    {
        switch (laneType)
        {
            case Ep3_2LaneType.Left:
                return leftLaneColor;
            case Ep3_2LaneType.Up:
                return upLaneColor;
            case Ep3_2LaneType.Right:
                return rightLaneColor;
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
        runtimePuzzleCameraRoot.SetActive(false);
    }

    private void UpdatePuzzleCameraPose(bool forceSnap = false)
    {
        if (!useRuntimePuzzleCamera || runtimePuzzleCamera == null || !runtimePuzzleCamera.enabled)
        {
            return;
        }

        Vector3 focusPoint = GetJudgeCenterPosition() + cameraFocusOffset;
        Vector3 forward = GetReferenceForward();
        Vector3 targetPosition = focusPoint + Vector3.up * cameraHeight - forward * cameraDistance;
        Quaternion targetRotation = Quaternion.LookRotation((focusPoint - targetPosition).normalized, Vector3.up);

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
    }

    private void OnDestroy()
    {
        DisablePuzzleCamera();
        UnlockPlayer();
    }
}
