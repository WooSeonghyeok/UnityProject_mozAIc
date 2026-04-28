using Episode3.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Ep3_2Manager : MonoBehaviour
{
    [Header("리듬 퍼즐 매니저")]
    [SerializeField] private RhythmPuzzleManager rhythmPuzzleManager;
    [SerializeField] private bool useTopDownRhythmPuzzle = true;
    [SerializeField] private Ep3_2TopDownRhythmPuzzle topDownRhythmPuzzle;

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

    [Header("클리어 이동 연출")]
    [SerializeField] private bool playClearPathSequenceOnPuzzleComplete = true;
    [SerializeField] private Ep3_2ClearStaffPathPreview clearStaffPathPreview;
    [SerializeField] private Transform playerClearArrivalPoint;
    [SerializeField] private float playerClearWalkSpeed = 1.35f;
    [SerializeField] private float playerClearMoveDuration = 0f;
    [SerializeField] private AnimationCurve playerClearMoveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private float playerClearHeightOffset = 0.05f;
    [SerializeField] private float playerClearWalkAnimationSpeed = 0.5f;
    [SerializeField] private float playerClearPostArrivalPause = 0.2f;
    [SerializeField] private float playerClearStopShortDistance = 0.9f;
    [SerializeField] private List<GameObject> clearSequenceHideObjects = new List<GameObject>();

    [Header("AI / 힌트 기록")]
    [SerializeField] private int hintCount = 0;
    [SerializeField] private int hintIntensity = 0;
    [SerializeField] private int aiInteractionCount = 0;

    [Header("획득 태그")]
    [SerializeField] private List<string> collectedTags = new List<string>();

    private bool isStageFinished;
    private bool isClearSequenceRunning;
    private PlayerMovement cachedPlayerMovement;
    private PlayerInput cachedPlayerInput;
    private Rigidbody cachedPlayerRigidbody;
    private Transform cachedPlayerTransform;
    private Animator cachedPlayerAnimator;

    public bool UsesTopDownRhythmPuzzle => useTopDownRhythmPuzzle;
    public Ep3_2StartPuzzle StartPuzzleController => startPuzzleController;
    public Transform PuzzleSetupReferenceTransform => startPuzzleController != null ? startPuzzleController.transform : transform;

    private void Start()
    {
        ResolvePlayerReferences();
        SetClearSequenceWorldObjectsVisible(true);
        PrepareStageNpc();
        ResetStartPuzzleController();
        ResolvePuzzleRunners();

        if (useTopDownRhythmPuzzle)
        {
            if (topDownRhythmPuzzle == null)
            {
                Debug.LogWarning("[Ep3_2Manager] Ep3_2TopDownRhythmPuzzle를 찾지 못했습니다.");
                return;
            }

            topDownRhythmPuzzle.Initialize(this);
        }
        else
        {
            if (rhythmPuzzleManager == null)
            {
                Debug.LogWarning("[Ep3_2Manager] RhythmPuzzleManager가 연결되지 않았습니다.");
                return;
            }

            rhythmPuzzleManager.Initialize(this);
        }

        if (exitDoorInteractable != null)
        {
            exitDoorInteractable.enabled = false;
        }
    }

    public void StartRhythmStage()
    {
        ResolvePuzzleRunners();

        if (useTopDownRhythmPuzzle)
        {
            if (topDownRhythmPuzzle == null)
            {
                Debug.LogWarning("[Ep3_2Manager] Ep3_2TopDownRhythmPuzzle가 연결되지 않았습니다.");
                return;
            }

            topDownRhythmPuzzle.StartPuzzle();
        }
        else
        {
            if (rhythmPuzzleManager == null)
            {
                Debug.LogWarning("[Ep3_2Manager] RhythmPuzzleManager가 연결되지 않았습니다.");
                return;
            }

            rhythmPuzzleManager.StartPuzzle();
        }

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
        if (isStageFinished || isClearSequenceRunning)
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

        StartCoroutine(PlayStageClearSequence(puzzleScore));
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

        if (NpcMemoryProgressManager.Instance != null)
        {
            NpcMemoryProgressManager.Instance.OnPuzzleStateChanged();
        }

        SaveManager.WriteCurJSON(saveData);

        if (SaveManager.instance != null)
        {
            SaveManager.instance.curData = saveData;
        }
    }

    public void OnRhythmPuzzleFailed()
    {
        SetClearSequenceWorldObjectsVisible(true);
        ResetStartPuzzleController();
        Debug.Log("[Ep3_2Manager] 3-2 실패");
    }

    private IEnumerator PlayStageClearSequence(int puzzleScore)
    {
        isClearSequenceRunning = true;
        ResolvePlayerReferences();
        WarpNpcToClearPoint();

        if (playClearPathSequenceOnPuzzleComplete)
        {
            yield return PlayClearPathSequenceIfNeeded();
        }

        yield return PlayClearCutsceneIfNeeded();

        if (exitDoorInteractable != null)
        {
            exitDoorInteractable.enabled = true;
        }

        SetPlayerSequenceLock(false);
        isClearSequenceRunning = false;
        Debug.Log($"[Ep3_2Manager] 3-2 클리어 연출 종료 - score={puzzleScore}, 출구 문 상호작용 가능");
    }

    private IEnumerator PlayClearPathSequenceIfNeeded()
    {
        Transform arrivalPoint = ResolvePlayerArrivalPoint();
        if (arrivalPoint == null || cachedPlayerTransform == null)
        {
            yield break;
        }

        Ep3_2ClearStaffPathPreview preview = ResolveClearStaffPathPreview();
        SetClearSequenceWorldObjectsVisible(false);

        Vector3 startPosition = cachedPlayerTransform.position;
        Vector3 arrivalCenterPosition = arrivalPoint.position + Vector3.up * playerClearHeightOffset;
        Vector3 flatDirectionToArrival = arrivalCenterPosition - startPosition;
        flatDirectionToArrival.y = 0f;
        Vector3 targetPosition = arrivalCenterPosition;
        if (flatDirectionToArrival.sqrMagnitude > 0.001f && playerClearStopShortDistance > 0f)
        {
            targetPosition -= flatDirectionToArrival.normalized * playerClearStopShortDistance;
        }

        Quaternion targetRotation = arrivalPoint.rotation;
        Vector3 pathStart = startPosition + Vector3.up * 0.05f;
        Vector3 pathEnd = targetPosition + Vector3.up * 0.05f;

        preview?.ShowPath(pathStart, pathEnd);
        SetPlayerAutoWalkLock(true);
        SetPlayerWalkingAnimation(true);

        float distance = Vector3.Distance(startPosition, targetPosition);
        float speedBasedDuration = distance / Mathf.Max(0.1f, playerClearWalkSpeed);
        float duration = playerClearMoveDuration > 0.05f
            ? playerClearMoveDuration
            : speedBasedDuration;
        float elapsed = 0f;
        Vector3 flatDirection = arrivalCenterPosition - startPosition;
        flatDirection.y = 0f;
        Quaternion moveRotation = flatDirection.sqrMagnitude > 0.001f
            ? Quaternion.LookRotation(flatDirection.normalized, Vector3.up)
            : targetRotation;

        // Start the clear walk already facing the destination so the camera
        // doesn't show the player stepping off while looking backward.
        ApplyPlayerPose(startPosition, moveRotation);
        yield return null;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = playerClearMoveCurve != null ? playerClearMoveCurve.Evaluate(t) : t;
            Vector3 nextPosition = Vector3.Lerp(startPosition, targetPosition, easedT);
            ApplyPlayerPose(nextPosition, moveRotation);
            yield return null;
        }

        ApplyPlayerPose(targetPosition, moveRotation);
        SetPlayerWalkingAnimation(false);
        preview?.HidePath();

        if (playerClearPostArrivalPause > 0f)
        {
            yield return new WaitForSeconds(playerClearPostArrivalPause);
        }
    }

    private IEnumerator PlayClearCutsceneIfNeeded()
    {
        if (!playClearCutsceneOnPuzzleComplete || clearCutscenePlayer == null)
        {
            yield break;
        }

        if (clearCutscenePlayer.IsPlaying)
        {
            while (clearCutscenePlayer.IsPlaying)
            {
                yield return null;
            }

            yield break;
        }

        bool finished = false;
        UnityAction listener = () => finished = true;
        clearCutscenePlayer.AddFinishedListener(listener);
        clearCutscenePlayer.PlayCutsceneSegment(clearCutsceneStartIndex, clearCutsceneStepCount);

        while (!finished && clearCutscenePlayer != null && clearCutscenePlayer.IsPlaying)
        {
            yield return null;
        }

        clearCutscenePlayer.RemoveFinishedListener(listener);
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

    private void ResetStartPuzzleController()
    {
        if (startPuzzleController == null)
        {
            return;
        }

        startPuzzleController.ResetStartSequence();
    }

    private void SetClearSequenceWorldObjectsVisible(bool isVisible)
    {
        if (startPuzzleController != null)
        {
            startPuzzleController.gameObject.SetActive(isVisible);
        }

        for (int i = 0; i < clearSequenceHideObjects.Count; i++)
        {
            GameObject target = clearSequenceHideObjects[i];
            if (target == null || (startPuzzleController != null && target == startPuzzleController.gameObject))
            {
                continue;
            }

            target.SetActive(isVisible);
        }
    }

    private void ResolvePuzzleRunners()
    {
        if (topDownRhythmPuzzle == null)
        {
            topDownRhythmPuzzle = GetComponent<Ep3_2TopDownRhythmPuzzle>();
        }

        if (topDownRhythmPuzzle == null)
        {
            topDownRhythmPuzzle = GetComponentInChildren<Ep3_2TopDownRhythmPuzzle>(true);
        }

        if (useTopDownRhythmPuzzle && topDownRhythmPuzzle == null)
        {
            topDownRhythmPuzzle = gameObject.AddComponent<Ep3_2TopDownRhythmPuzzle>();
        }

        if (rhythmPuzzleManager == null)
        {
            rhythmPuzzleManager = GetComponent<RhythmPuzzleManager>();
        }

        if (rhythmPuzzleManager == null)
        {
            rhythmPuzzleManager = GetComponentInChildren<RhythmPuzzleManager>(true);
        }
    }

    private void ResolvePlayerReferences()
    {
        if (cachedPlayerMovement == null)
        {
            cachedPlayerMovement = FindFirstObjectByType<PlayerMovement>();
        }

        if (cachedPlayerInput == null)
        {
            cachedPlayerInput = FindFirstObjectByType<PlayerInput>();
        }

        if (cachedPlayerMovement != null)
        {
            cachedPlayerTransform = cachedPlayerMovement.transform;
            if (cachedPlayerRigidbody == null)
            {
                cachedPlayerRigidbody = cachedPlayerMovement.GetComponent<Rigidbody>();
            }
            if (cachedPlayerAnimator == null)
            {
                cachedPlayerAnimator = cachedPlayerMovement.GetComponent<Animator>();
            }
        }
        else if (cachedPlayerInput != null)
        {
            cachedPlayerTransform = cachedPlayerInput.transform;
            if (cachedPlayerRigidbody == null)
            {
                cachedPlayerRigidbody = cachedPlayerInput.GetComponent<Rigidbody>();
            }
            if (cachedPlayerAnimator == null)
            {
                cachedPlayerAnimator = cachedPlayerInput.GetComponent<Animator>();
            }
        }
    }

    private Transform ResolvePlayerArrivalPoint()
    {
        if (playerClearArrivalPoint != null)
        {
            return playerClearArrivalPoint;
        }

        return npcClearWarpPoint;
    }

    private Ep3_2ClearStaffPathPreview ResolveClearStaffPathPreview()
    {
        if (clearStaffPathPreview != null)
        {
            return clearStaffPathPreview;
        }

        clearStaffPathPreview = GetComponentInChildren<Ep3_2ClearStaffPathPreview>(true);
        if (clearStaffPathPreview != null)
        {
            return clearStaffPathPreview;
        }

        GameObject previewObject = new GameObject("EP3_2_ClearStaffPathPreview");
        previewObject.transform.SetParent(transform, false);
        clearStaffPathPreview = previewObject.AddComponent<Ep3_2ClearStaffPathPreview>();
        previewObject.SetActive(false);
        return clearStaffPathPreview;
    }

    private void SetPlayerSequenceLock(bool isLocked)
    {
        ResolvePlayerReferences();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CutsceneMode(isLocked);
        }

        if (cachedPlayerMovement != null)
        {
            cachedPlayerMovement.SetMoveLock(isLocked);
            cachedPlayerMovement.SetInputLock(isLocked);
        }

        if (cachedPlayerInput != null)
        {
            cachedPlayerInput.isLookLock = isLocked;
            cachedPlayerInput.isJumpLock = isLocked;
            cachedPlayerInput.ResetInputState();
        }

        if (cachedPlayerRigidbody != null)
        {
            cachedPlayerRigidbody.velocity = Vector3.zero;
            cachedPlayerRigidbody.angularVelocity = Vector3.zero;
        }

        if (!isLocked)
        {
            SetPlayerWalkingAnimation(false);
        }
    }

    private void SetPlayerAutoWalkLock(bool isLocked)
    {
        ResolvePlayerReferences();

        if (cachedPlayerMovement != null)
        {
            cachedPlayerMovement.SetMoveLock(false);
            cachedPlayerMovement.SetInputLock(isLocked);
        }

        if (cachedPlayerInput != null)
        {
            cachedPlayerInput.isLookLock = isLocked;
            cachedPlayerInput.isJumpLock = isLocked;
            cachedPlayerInput.ResetInputState();
        }

        if (cachedPlayerRigidbody != null)
        {
            cachedPlayerRigidbody.velocity = Vector3.zero;
            cachedPlayerRigidbody.angularVelocity = Vector3.zero;
        }
    }

    private void SetPlayerWalkingAnimation(bool isWalking)
    {
        if (cachedPlayerAnimator == null)
        {
            return;
        }

        cachedPlayerAnimator.SetBool("CanMove", isWalking);
        cachedPlayerAnimator.SetBool("IsGrounded", true);
        cachedPlayerAnimator.SetFloat("Speed", isWalking ? playerClearWalkAnimationSpeed : 0f);
    }

    private void ApplyPlayerPose(Vector3 position, Quaternion rotation)
    {
        if (cachedPlayerRigidbody != null)
        {
            cachedPlayerRigidbody.position = position;
            cachedPlayerRigidbody.rotation = rotation;
            cachedPlayerRigidbody.velocity = Vector3.zero;
            cachedPlayerRigidbody.angularVelocity = Vector3.zero;
        }
        else if (cachedPlayerTransform != null)
        {
            cachedPlayerTransform.SetPositionAndRotation(position, rotation);
        }
    }
}

public class Ep3_2ClearStaffPathPreview : MonoBehaviour
{
    [Header("오선지 라인")]
    [SerializeField] private int lineCount = 5;
    [SerializeField] private float lineSpacing = 0.42f;
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private float lineHeightOffset = 0.18f;
    [SerializeField] private Color lineColor = new Color(0.94f, 0.84f, 0.98f, 0.92f);

    [Header("음표 장식")]
    [SerializeField] private bool showDecorNotes = true;
    [SerializeField] private int decorNoteCount = 6;
    [SerializeField] private float decorNoteHeightOffset = 0.34f;
    [SerializeField] private float decorNoteScale = 0.45f;
    [SerializeField] private Color decorNoteColor = new Color(1f, 0.96f, 0.78f, 0.95f);

    private readonly List<LineRenderer> lineRenderers = new List<LineRenderer>();
    private readonly List<SpriteRenderer> decorRenderers = new List<SpriteRenderer>();

    private Material lineMaterial;
    private bool visualsBuilt;

    public void ShowPath(Vector3 startWorld, Vector3 endWorld)
    {
        EnsureVisuals();
        UpdateLayout(startWorld, endWorld);
        gameObject.SetActive(true);
    }

    public void HidePath()
    {
        gameObject.SetActive(false);
    }

    private void EnsureVisuals()
    {
        if (visualsBuilt)
        {
            return;
        }

        if (lineMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            lineMaterial = new Material(shader != null ? shader : Shader.Find("Unlit/Color"));
            lineMaterial.color = lineColor;
        }

        for (int i = 0; i < Mathf.Max(1, lineCount); i++)
        {
            GameObject lineObject = new GameObject($"StaffLine_{i + 1}", typeof(LineRenderer));
            lineObject.transform.SetParent(transform, false);

            LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;
            lineRenderer.loop = false;
            lineRenderer.positionCount = 2;
            lineRenderer.numCapVertices = 4;
            lineRenderer.numCornerVertices = 4;
            lineRenderer.widthMultiplier = lineWidth;
            lineRenderer.material = lineMaterial;
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.textureMode = LineTextureMode.Stretch;
            lineRenderer.alignment = LineAlignment.View;
            lineRenderers.Add(lineRenderer);
        }

        if (showDecorNotes)
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>("Ep.3/Notes/Sprites");
            int safeDecorCount = Mathf.Max(0, decorNoteCount);
            for (int i = 0; i < safeDecorCount; i++)
            {
                GameObject noteObject = new GameObject($"StaffNote_{i + 1}", typeof(SpriteRenderer));
                noteObject.transform.SetParent(transform, false);

                SpriteRenderer spriteRenderer = noteObject.GetComponent<SpriteRenderer>();
                spriteRenderer.sprite = sprites != null && sprites.Length > 0 ? sprites[i % sprites.Length] : null;
                spriteRenderer.color = decorNoteColor;
                spriteRenderer.sortingOrder = 25;
                noteObject.transform.localScale = Vector3.one * decorNoteScale;
                decorRenderers.Add(spriteRenderer);
            }
        }

        visualsBuilt = true;
        gameObject.SetActive(false);
    }

    private void UpdateLayout(Vector3 startWorld, Vector3 endWorld)
    {
        Vector3 flatStart = startWorld;
        Vector3 flatEnd = endWorld;
        flatEnd.y = flatStart.y;

        Vector3 forward = flatEnd - flatStart;
        if (forward.sqrMagnitude < 0.001f)
        {
            forward = transform.forward.sqrMagnitude > 0.001f ? transform.forward : Vector3.forward;
            flatEnd = flatStart + forward.normalized * 4f;
        }

        forward.y = 0f;
        forward.Normalize();

        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        float centerOffset = (Mathf.Max(1, lineCount) - 1) * 0.5f;
        Vector3 baseHeight = Vector3.up * lineHeightOffset;

        for (int i = 0; i < lineRenderers.Count; i++)
        {
            float offset = (i - centerOffset) * lineSpacing;
            Vector3 lateral = right * offset;
            lineRenderers[i].SetPosition(0, flatStart + lateral + baseHeight);
            lineRenderers[i].SetPosition(1, flatEnd + lateral + baseHeight);
        }

        if (decorRenderers.Count == 0)
        {
            return;
        }

        float length = Vector3.Distance(flatStart, flatEnd);
        for (int i = 0; i < decorRenderers.Count; i++)
        {
            float t = decorRenderers.Count == 1 ? 0.5f : (i + 1f) / (decorRenderers.Count + 1f);
            float wave = Mathf.Sin((i + 1f) * 1.37f) * lineSpacing * 0.8f;
            Vector3 linePoint = Vector3.Lerp(flatStart, flatEnd, t);
            Vector3 noteOffset = right * wave + Vector3.up * decorNoteHeightOffset;
            SpriteRenderer decorRenderer = decorRenderers[i];
            decorRenderer.transform.position = linePoint + noteOffset;
            decorRenderer.transform.rotation = Quaternion.LookRotation(Camera.main != null ? Camera.main.transform.forward : Vector3.forward);
            decorRenderer.transform.localScale = Vector3.one * decorNoteScale * Mathf.Lerp(0.92f, 1.08f, t);
            decorRenderer.enabled = length > 0.5f;
        }
    }
}
