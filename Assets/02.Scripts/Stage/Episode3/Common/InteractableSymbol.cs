namespace Episode3.Common
{
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.AI;
    using UnityEngine.Events;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    public enum SymbolLockRequirement
    {
        None = 0,
        Stage3_1Visited = 1
    }

    /// <summary>
    /// 장면 내 상호작용 가능한 심볼(예: 문, 포탈 등).
    /// 사용법:
    /// 1. 플레이어가 범위에 들어오면 상호작용 UI를 표시합니다.
    /// 2. 플레이어가 범위를 벗어나면 UI를 숨깁니다.
    /// 3. 플레이어가 상호작용 입력을 하면 지정된 동작(씬 전환 또는 UnityEvent)을 수행합니다.
    /// 4. 잠금 조건이 만족되지 않으면 잠금 메시지를 표시합니다.
    /// 5. useSceneName이 false일 때는 디자이너 훅(UnityEvent)을 호출합니다.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class InteractableSymbol : MonoBehaviour
    {
        [Header("상호작용 설정")]
        [Tooltip("활성화하면 `sceneNameValue`를 사용해 SceneManager.LoadScene을 호출합니다.")]
        [SerializeField] private bool useSceneName = true;

        [Tooltip("이동할 씬 이름(빌드된 씬 이름)")]
        [SerializeField] private string sceneNameValue;

        [Header("잠금 설정")]
        [Tooltip("상호작용 가능 여부를 판정할 요구 조건")]
        [SerializeField] private SymbolLockRequirement lockRequirement = SymbolLockRequirement.None;

        [Tooltip("잠겨 있을 때 플레이어에게 보여줄 메시지")]
        [SerializeField] private string lockedMessage = "먼저 특정 조건을 만족해야 합니다.";

        [Tooltip("잠금 메시지 표시 시간(초)")]
        [SerializeField] private float lockedMessageDuration = 1.5f;

        [Tooltip("잠금 팝업 프리팹의 Resources 경로")]
        [SerializeField] private string lockedPopupResourcePath = "Prefabs/UI/Episode3SymbolLockPopup";

        [Header("상호작용 UI")]
        [Tooltip("플레이어가 범위에 있을 때 보여줄 UI 오브젝트")]
        [SerializeField] private GameObject interactUI;

        [Header("상호작용 우선순위")]
        [Tooltip("겹친 상호작용 오브젝트 중 더 큰 값이 우선 반응합니다.")]
        [SerializeField] private int interactionPriority = 0;

        [Header("1회 사용 후 비활성화")]
        [Tooltip("한 번 상호작용하면 이 심볼의 상호작용을 비활성화합니다.")]
        [SerializeField] private bool disableInteractionAfterUse = false;

        [Tooltip("한 번 상호작용하면 이 오브젝트의 콜라이더를 비활성화합니다.")]
        [SerializeField] private bool disableColliderAfterUse = false;

        [Header("도착 연출")]
        [Tooltip("플레이어가 범위에 처음 들어왔을 때 컷씬/연출을 실행합니다.")]
        [SerializeField] private bool playArrivalSequenceOnEnter = false;

        [Tooltip("도착 연출을 한 번만 실행할지 여부")]
        [SerializeField] private bool playArrivalSequenceOnlyOnce = true;

        [Tooltip("도착 연출에 사용할 컷씬 플레이어")]
        [SerializeField] private CutsceneImagePlayer arrivalCutscenePlayer;

        [SerializeField] private int arrivalCutsceneStartIndex = 0;
        [SerializeField] private int arrivalCutsceneStepCount = 0;

        [Tooltip("도착 연출이 끝난 직후 기존 상호작용을 자동 실행합니다.")]
        [SerializeField] private bool autoInteractAfterArrivalSequence = false;

        [Header("상호작용 직전 컷씬")]
        [Tooltip("상호작용을 실제로 수행하기 직전에 컷씬을 먼저 재생합니다.")]
        [SerializeField] private bool playInteractionCutsceneBeforeAction = false;

        [SerializeField] private CutsceneImagePlayer interactionCutscenePlayer;
        [SerializeField] private int interactionCutsceneStartIndex = 0;
        [SerializeField] private int interactionCutsceneStepCount = 0;
        [SerializeField] private bool playInteractionCutsceneOnlyOnce = true;

        [Header("NPC 합류 연출")]
        [Tooltip("도착 연출 시작 시 NPC를 지정 위치로 합류시킵니다.")]
        [SerializeField] private bool teleportNpcOnArrival = false;

        [SerializeField] private Transform npcTransform;
        [SerializeField] private Transform npcTeleportPoint;
        [SerializeField] private Transform npcLookTarget;

        [Header("Designer Hook")]
        [Tooltip("useSceneName이 비활성화되어 있을 때 호출되는 이벤트입니다.")]
        [SerializeField] private UnityEvent onInteract;

        /// <summary>
        /// 상호작용 입력을 받는 플레이어의 입력 컴포넌트 참조.
        /// Player 오브젝트에서 `PlayerInput`의 Interact 이벤트를 구독합니다.
        /// </summary>
        private PlayerInput user;
        private Transform playerTransform;
        private Collider playerCollider;

        /// <summary>
        /// 플레이어 태그 (찾기용)
        /// </summary>
        private readonly string playerTag = "Player";

        /// <summary>
        /// 플레이어가 범위 안에 있는지 여부
        /// </summary>
        private bool playerInRange;
        private bool interactionEnabled = true;
        private bool arrivalSequencePlayed;
        private bool arrivalSequenceRunning;
        private bool arrivalCutsceneListenerAdded;
        private bool interactionCutscenePlayed;
        private bool interactionCutsceneRunning;
        private bool interactionCutsceneListenerAdded;
        private Collider cachedCollider;

        private static readonly System.Collections.Generic.List<InteractableSymbol> ActiveSymbols = new();

        private void Awake()
        {
            // Player 오브젝트를 태그로 찾아 PlayerInput 컴포넌트를 가져온다.
            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

            if (playerObject != null)
            {
                user = playerObject.GetComponent<PlayerInput>();
                playerTransform = playerObject.transform;
                playerCollider = playerObject.GetComponent<Collider>();

                if (playerCollider == null)
                {
                    playerCollider = playerObject.GetComponentInChildren<Collider>();
                }
            }
            else
            {
                Debug.LogWarning($"[InteractableSymbol] Player 오브젝트를 찾지 못했습니다. 오브젝트: {name}");
            }

            // 초기에는 상호작용 UI를 비활성화
            if (interactUI != null)
            {
                interactUI.SetActive(false);
            }

            cachedCollider = ResolvePreferredCollider();
        }

        private void OnEnable()
        {
            if (!ActiveSymbols.Contains(this))
            {
                ActiveSymbols.Add(this);
            }

            // PlayerInput의 Interact 이벤트 구독
            if (user != null)
            {
                user.Interact += SymbolInteract;
            }

            SyncPlayerInRangeState();
            RefreshAllInteractUI();
        }

        private void OnDisable()
        {
            ActiveSymbols.Remove(this);

            // 이벤트 구독 해제
            if (user != null)
            {
                user.Interact -= SymbolInteract;
            }

            RemoveArrivalCutsceneListener();
            RemoveInteractionCutsceneListener();
            RefreshAllInteractUI();
        }

        private void OnDestroy()
        {
            RemoveArrivalCutsceneListener();
            RemoveInteractionCutsceneListener();
        }

        private void Reset()
        {
            // 기본 Collider를 트리거로 설정
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // 플레이어가 범위에 들어오면 표시 플래그 설정
            if (!other.CompareTag(playerTag)) return;

            playerInRange = true;
            RefreshAllInteractUI();
            TryStartArrivalSequence();
        }

        private void OnTriggerExit(Collider other)
        {
            // 플레이어가 범위를 벗어나면 표시 플래그 해제
            if (!other.CompareTag(playerTag)) return;

            playerInRange = false;
            RefreshAllInteractUI();
        }

        /// <summary>
        /// PlayerInput의 Interact 이벤트에 연결되는 메서드.
        /// 범위 내이고 상호작용이 활성화되어 있을 때 실제 상호작용을 수행한다.
        /// </summary>
        public void SymbolInteract()
        {
            if (!interactionEnabled) return;
            if (IsAnySequenceRunning()) return;
            if (!playerInRange) return;
            if (!IsTopPriorityInteractionCandidate()) return;

            PerformInteraction();
        }

        public void SetInteractionEnabled(bool enabled)
        {
            interactionEnabled = enabled;
            cachedCollider ??= ResolvePreferredCollider();
            SyncPlayerInRangeState();
            RefreshAllInteractUI();
        }

        public void SetColliderEnabled(bool enabled)
        {
            cachedCollider ??= ResolvePreferredCollider();

            if (cachedCollider != null)
            {
                cachedCollider.enabled = enabled;
            }

            SyncPlayerInRangeState();
            RefreshAllInteractUI();
        }

        public void RefreshInteractionState()
        {
            cachedCollider ??= ResolvePreferredCollider();
            SyncPlayerInRangeState();
            RefreshAllInteractUI();
        }

        /// <summary>
        /// 실제 상호작용 처리:
        /// 1. UI 숨김
        /// 2. 잠금 여부 검사
        /// 3. 씬 전환 또는 UnityEvent 호출
        /// 4. 잠금시 팝업 표시
        /// </summary>
        private void PerformInteraction()
        {
            // 잠금 상태인 경우 메시지 출력 후 종료
            if (!CanInteract())
            {
                HideInteractUI();
                SymbolLockMessageUI.Show(lockedMessage, lockedMessageDuration, lockedPopupResourcePath);
                return;
            }

            if (TryStartInteractionCutscene())
            {
                return;
            }

            ExecuteInteractionAction();
        }

        private void ExecuteInteractionAction()
        {
            HideInteractUI();
            HandlePostInteractionDisable();

            // 씬 전환 또는 이벤트 호출
            if (useSceneName)
            {
                if (!string.IsNullOrEmpty(sceneNameValue))
                {
                    SceneManager.LoadScene(sceneNameValue);
                }
                else
                {
                    Debug.LogWarning($"[InteractableSymbol] sceneNameValue가 비어있습니다. 오브젝트: {name}");
                }
            }
            else
            {
                onInteract?.Invoke();
            }
        }

        private void HandlePostInteractionDisable()
        {
            if (disableInteractionAfterUse)
            {
                interactionEnabled = false;
                playerInRange = false;
            }

            if (disableColliderAfterUse)
            {
                cachedCollider ??= GetComponent<Collider>();
                if (cachedCollider != null)
                {
                    cachedCollider.enabled = false;
                }
            }

            RefreshAllInteractUI();
        }

        /// <summary>
        /// 현재 상호작용이 가능한지 여부를 반환
        /// </summary>
        private bool CanInteract()
        {
            return lockRequirement switch
            {
                SymbolLockRequirement.None => true,

                // 3-1 스테이지 방문 여부에 따라 판정
                SymbolLockRequirement.Stage3_1Visited =>
                    Ep_3Manager.Instance != null && Ep_3Manager.Instance.HasVisitedStage3_1,

                _ => true
            };
        }

        private void UpdateInteractUI()
        {
            RefreshAllInteractUI();
        }

        private bool ShouldDisplayInteractUI()
        {
            return interactUI != null
                && interactionEnabled
                && playerInRange
                && !IsAnySequenceRunning()
                && IsTopPriorityInteractionCandidate();
        }

        private bool IsTopPriorityInteractionCandidate()
        {
            if (!interactionEnabled || !playerInRange || IsAnySequenceRunning())
            {
                return false;
            }

            float myDistance = GetInteractionDistance();

            foreach (InteractableSymbol symbol in ActiveSymbols)
            {
                if (symbol == null || symbol == this)
                {
                    continue;
                }

                if (!symbol.gameObject.activeInHierarchy || !symbol.interactionEnabled || !symbol.playerInRange || symbol.IsAnySequenceRunning())
                {
                    continue;
                }

                if (symbol.interactionPriority > interactionPriority)
                {
                    return false;
                }

                if (symbol.interactionPriority == interactionPriority)
                {
                    float otherDistance = symbol.GetInteractionDistance();
                    if (otherDistance + 0.01f < myDistance)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private float GetInteractionDistance()
        {
            if (playerTransform == null)
            {
                return float.MaxValue;
            }

            if (cachedCollider != null)
            {
                Vector3 closestPoint = cachedCollider.bounds.ClosestPoint(playerTransform.position);
                return Vector3.SqrMagnitude(playerTransform.position - closestPoint);
            }

            return Vector3.SqrMagnitude(playerTransform.position - transform.position);
        }

        private Collider ResolvePreferredCollider()
        {
            Collider[] colliders = GetComponents<Collider>();
            Collider fallbackCollider = null;

            foreach (Collider candidate in colliders)
            {
                if (candidate == null)
                {
                    continue;
                }

                fallbackCollider ??= candidate;

                if (!candidate.isTrigger)
                {
                    continue;
                }

                if (candidate is BoxCollider || candidate is SphereCollider || candidate is CapsuleCollider)
                {
                    return candidate;
                }

                if (candidate is MeshCollider meshCollider && meshCollider.convex)
                {
                    return candidate;
                }
            }

            return fallbackCollider;
        }

        private void SyncPlayerInRangeState()
        {
            if (!interactionEnabled || playerCollider == null || cachedCollider == null || !cachedCollider.enabled || !gameObject.activeInHierarchy)
            {
                playerInRange = false;
                return;
            }

            playerInRange = cachedCollider.bounds.Intersects(playerCollider.bounds);
        }

        private static void RefreshAllInteractUI()
        {
            Dictionary<GameObject, InteractableSymbol> bestByInteractUi = new();
            HashSet<GameObject> encounteredUiObjects = new();

            for (int i = ActiveSymbols.Count - 1; i >= 0; i--)
            {
                InteractableSymbol symbol = ActiveSymbols[i];
                if (symbol == null)
                {
                    ActiveSymbols.RemoveAt(i);
                    continue;
                }

                if (symbol.interactUI == null)
                {
                    continue;
                }

                encounteredUiObjects.Add(symbol.interactUI);

                if (!symbol.ShouldDisplayInteractUI())
                {
                    continue;
                }

                if (!bestByInteractUi.TryGetValue(symbol.interactUI, out InteractableSymbol currentBest))
                {
                    bestByInteractUi[symbol.interactUI] = symbol;
                    continue;
                }

                if (ComparePriority(symbol, currentBest) > 0)
                {
                    bestByInteractUi[symbol.interactUI] = symbol;
                }
            }

            foreach (GameObject uiObject in encounteredUiObjects)
            {
                if (uiObject != null)
                {
                    uiObject.SetActive(false);
                }
            }

            foreach (KeyValuePair<GameObject, InteractableSymbol> pair in bestByInteractUi)
            {
                if (pair.Key != null)
                {
                    pair.Key.SetActive(true);
                }
            }
        }

        private static int ComparePriority(InteractableSymbol candidate, InteractableSymbol currentBest)
        {
            if (candidate == null && currentBest == null)
            {
                return 0;
            }

            if (candidate == null)
            {
                return -1;
            }

            if (currentBest == null)
            {
                return 1;
            }

            if (candidate.interactionPriority != currentBest.interactionPriority)
            {
                return candidate.interactionPriority.CompareTo(currentBest.interactionPriority);
            }

            float candidateDistance = candidate.GetInteractionDistance();
            float currentBestDistance = currentBest.GetInteractionDistance();
            if (candidateDistance + 0.01f < currentBestDistance)
            {
                return 1;
            }

            if (currentBestDistance + 0.01f < candidateDistance)
            {
                return -1;
            }

            return 0;
        }

        private bool IsAnySequenceRunning()
        {
            return arrivalSequenceRunning || interactionCutsceneRunning;
        }

        private void TryStartArrivalSequence()
        {
            if (!playArrivalSequenceOnEnter || !interactionEnabled || !playerInRange)
            {
                return;
            }

            if (arrivalSequenceRunning)
            {
                return;
            }

            if (playArrivalSequenceOnlyOnce && arrivalSequencePlayed)
            {
                return;
            }

            if (!CanInteract())
            {
                return;
            }

            arrivalSequencePlayed = true;
            arrivalSequenceRunning = true;
            HideInteractUI();
            TeleportNpcToArrivalPoint();

            if (ShouldPlayArrivalCutscene())
            {
                AddArrivalCutsceneListener();
                arrivalCutscenePlayer.PlayCutsceneSegment(arrivalCutsceneStartIndex, arrivalCutsceneStepCount);
                return;
            }

            CompleteArrivalSequence();
        }

        private bool ShouldPlayArrivalCutscene()
        {
            return arrivalCutscenePlayer != null &&
                   arrivalCutscenePlayer.HasStepContent(arrivalCutsceneStartIndex) &&
                   !arrivalCutscenePlayer.IsPlaying;
        }

        private void OnArrivalCutsceneFinished()
        {
            CompleteArrivalSequence();
        }

        private void CompleteArrivalSequence()
        {
            RemoveArrivalCutsceneListener();

            if (!arrivalSequenceRunning)
            {
                return;
            }

            arrivalSequenceRunning = false;

            if (autoInteractAfterArrivalSequence && interactionEnabled && playerInRange)
            {
                PerformInteraction();
                return;
            }

            UpdateInteractUI();
        }

        private bool TryStartInteractionCutscene()
        {
            if (!playInteractionCutsceneBeforeAction)
            {
                return false;
            }

            if (interactionCutsceneRunning)
            {
                return true;
            }

            if (playInteractionCutsceneOnlyOnce && interactionCutscenePlayed)
            {
                return false;
            }

            if (!ShouldPlayInteractionCutscene())
            {
                return false;
            }

            interactionCutscenePlayed = true;
            interactionCutsceneRunning = true;
            HideInteractUI();
            AddInteractionCutsceneListener();
            interactionCutscenePlayer.PlayCutsceneSegment(interactionCutsceneStartIndex, interactionCutsceneStepCount);
            return true;
        }

        private bool ShouldPlayInteractionCutscene()
        {
            return interactionCutscenePlayer != null &&
                   !interactionCutscenePlayer.IsPlaying &&
                   interactionCutscenePlayer.HasStepContent(interactionCutsceneStartIndex);
        }

        private void OnInteractionCutsceneFinished()
        {
            RemoveInteractionCutsceneListener();

            if (!interactionCutsceneRunning)
            {
                return;
            }

            interactionCutsceneRunning = false;
            ExecuteInteractionAction();
        }

        private void AddArrivalCutsceneListener()
        {
            if (arrivalCutsceneListenerAdded || arrivalCutscenePlayer == null)
            {
                return;
            }

            arrivalCutscenePlayer.AddFinishedListener(OnArrivalCutsceneFinished);
            arrivalCutsceneListenerAdded = true;
        }

        private void RemoveArrivalCutsceneListener()
        {
            if (!arrivalCutsceneListenerAdded || arrivalCutscenePlayer == null)
            {
                return;
            }

            arrivalCutscenePlayer.RemoveFinishedListener(OnArrivalCutsceneFinished);
            arrivalCutsceneListenerAdded = false;
        }

        private void AddInteractionCutsceneListener()
        {
            if (interactionCutsceneListenerAdded || interactionCutscenePlayer == null)
            {
                return;
            }

            interactionCutscenePlayer.AddFinishedListener(OnInteractionCutsceneFinished);
            interactionCutsceneListenerAdded = true;
        }

        private void RemoveInteractionCutsceneListener()
        {
            if (!interactionCutsceneListenerAdded || interactionCutscenePlayer == null)
            {
                return;
            }

            interactionCutscenePlayer.RemoveFinishedListener(OnInteractionCutsceneFinished);
            interactionCutsceneListenerAdded = false;
        }

        private void HideInteractUI()
        {
            if (interactUI != null)
            {
                interactUI.SetActive(false);
            }
        }

        private void TeleportNpcToArrivalPoint()
        {
            if (!teleportNpcOnArrival || npcTransform == null || npcTeleportPoint == null)
            {
                return;
            }

            Vector3 targetPosition = npcTeleportPoint.position;
            Quaternion targetRotation = ResolveNpcRotation(targetPosition);

            NPCFollower npcFollower = npcTransform.GetComponent<NPCFollower>();
            if (npcFollower != null)
            {
                npcFollower.SetFollow(false);
                npcFollower.WarpTo(targetPosition, targetRotation);
                return;
            }

            NavMeshAgent agent = npcTransform.GetComponent<NavMeshAgent>();
            if (agent != null && agent.enabled)
            {
                if (agent.isOnNavMesh)
                {
                    agent.ResetPath();
                }

                if (!agent.Warp(targetPosition))
                {
                    npcTransform.position = targetPosition;
                }

                npcTransform.rotation = targetRotation;
                return;
            }

            Rigidbody targetRigidbody = npcTransform.GetComponent<Rigidbody>();
            if (targetRigidbody != null)
            {
                targetRigidbody.velocity = Vector3.zero;
                targetRigidbody.angularVelocity = Vector3.zero;
                targetRigidbody.position = targetPosition;
                targetRigidbody.rotation = targetRotation;
                return;
            }

            npcTransform.SetPositionAndRotation(targetPosition, targetRotation);
        }

        private Quaternion ResolveNpcRotation(Vector3 targetPosition)
        {
            if (npcLookTarget == null)
            {
                return npcTeleportPoint != null ? npcTeleportPoint.rotation : Quaternion.identity;
            }

            Vector3 lookDirection = npcLookTarget.position - targetPosition;
            lookDirection.y = 0f;

            if (lookDirection.sqrMagnitude <= 0.0001f)
            {
                return npcTeleportPoint != null ? npcTeleportPoint.rotation : Quaternion.identity;
            }

            return Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
        }
    }

    /// <summary>
    /// 잠긴 심볼에 대해 팝업 메시지를 표시하는 UI 헬퍼 클래스.
    /// 
    /// 동작:
    /// 1. Resources에서 팝업 프리팹을 로드 시도
    /// 2. 프리팹이 없으면 대체(fallback) UI를 동적으로 생성
    /// 3. 지정한 시간동안 메시지를 표시
    /// </summary>
    public sealed class SymbolLockMessageUI : MonoBehaviour
    {
        private const string DefaultPopupResourcePath = "Prefabs/UI/Episode3SymbolLockPopup";

        private static SymbolLockMessageUI instance;

        private GameObject panelRoot;
        private TMP_Text popupText;
        private Text fallbackText;
        private Coroutine hideCoroutine;
        private string currentPopupResourcePath = string.Empty;

        /// <summary>
        /// 외부에서 호출하여 잠금 메시지를 보여준다.
        /// </summary>
        public static void Show(string message, float duration, string popupResourcePath)
        {
            EnsureInstance(popupResourcePath);
            instance.ShowInternal(message, duration);
        }

        /// <summary>
        /// 싱글톤 인스턴스 보장 및 팝업 로드
        /// </summary>
        private static void EnsureInstance(string popupResourcePath)
        {
            instance ??= Object.FindObjectOfType<SymbolLockMessageUI>();

            if (instance == null)
            {
                GameObject root = new("SymbolLockMessageUI");
                instance = root.AddComponent<SymbolLockMessageUI>();
                Object.DontDestroyOnLoad(root);
                instance.BuildCanvasRoot();
            }

            instance.EnsurePopupLoaded(popupResourcePath);
        }

        /// <summary>
        /// 화면에 표시할 Canvas 루트를 동적으로 구성
        /// </summary>
        private void BuildCanvasRoot()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            gameObject.AddComponent<GraphicRaycaster>();
        }

        /// <summary>
        /// 지정한 리소스 경로에서 팝업 프리팹을 로드하고 없으면 fallback을 사용.
        /// </summary>
        private void EnsurePopupLoaded(string popupResourcePath)
        {
            string resolvedPath = string.IsNullOrWhiteSpace(popupResourcePath)
                ? DefaultPopupResourcePath
                : popupResourcePath;

            if (panelRoot != null && currentPopupResourcePath == resolvedPath)
            {
                return;
            }

            ClearPopup();

            GameObject popupPrefab = Resources.Load<GameObject>(resolvedPath);
            if (popupPrefab != null)
            {
                panelRoot = Object.Instantiate(popupPrefab, transform, false);
                panelRoot.name = popupPrefab.name;
                popupText = panelRoot.GetComponentInChildren<TMP_Text>(true);
                fallbackText = panelRoot.GetComponentInChildren<Text>(true);
            }

            if (panelRoot == null || (popupText == null && fallbackText == null))
            {
                Debug.LogWarning($"[SymbolLockMessageUI] 팝업 프리팹을 찾을 수 없어 대체 UI를 생성합니다. 경로: {resolvedPath}");
                BuildFallbackPopup();
                currentPopupResourcePath = string.Empty;
                return;
            }

            currentPopupResourcePath = resolvedPath;
            panelRoot.SetActive(false);
        }

        /// <summary>
        /// 기존 팝업 정리
        /// </summary>
        private void ClearPopup()
        {
            if (panelRoot != null)
            {
                Object.Destroy(panelRoot);
            }

            panelRoot = null;
            popupText = null;
            fallbackText = null;
        }

        /// <summary>
        /// 프리팹이 없을 때 사용할 대체 팝업을 동적으로 생성
        /// </summary>
        private void BuildFallbackPopup()
        {
            panelRoot = new GameObject("Panel");
            panelRoot.transform.SetParent(transform, false);

            Image panelImage = panelRoot.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.78f);
            panelImage.raycastTarget = false;

            RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.16f);
            panelRect.anchorMax = new Vector2(0.5f, 0.16f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(720f, 84f);
            panelRect.anchoredPosition = Vector2.zero;

            GameObject textObject = new GameObject("Message");
            textObject.transform.SetParent(panelRoot.transform, false);

            fallbackText = textObject.AddComponent<Text>();
            fallbackText.font = LoadDefaultFont();
            fallbackText.fontSize = 28;
            fallbackText.alignment = TextAnchor.MiddleCenter;
            fallbackText.color = Color.white;
            fallbackText.horizontalOverflow = HorizontalWrapMode.Wrap;
            fallbackText.verticalOverflow = VerticalWrapMode.Overflow;
            fallbackText.raycastTarget = false;

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.offsetMin = new Vector2(24f, 12f);
            textRect.offsetMax = new Vector2(-24f, -12f);

            panelRoot.SetActive(false);
        }

        /// <summary>
        /// 기본 폰트 로드(내장 폰트를 우선 사용)
        /// </summary>
        private Font LoadDefaultFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null) return font;

            return Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        /// <summary>
        /// 내부적으로 메시지를 표시한다.
        /// </summary>
        private void ShowInternal(string message, float duration)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                message = "먼저 3-1 스테이지를 방문해야 합니다.";
            }

            if (panelRoot == null)
            {
                EnsurePopupLoaded(DefaultPopupResourcePath);
            }

            if (popupText != null)
            {
                popupText.text = message;
            }
            else if (fallbackText != null)
            {
                fallbackText.text = message;
            }

            if (panelRoot == null)
            {
                return;
            }

            panelRoot.SetActive(true);

            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
            }

            hideCoroutine = StartCoroutine(HideAfter(Mathf.Max(0.1f, duration)));
        }

        /// <summary>
        /// 지정 시간(Realtime) 후 팝업을 숨긴다.
        /// </summary>
        private IEnumerator HideAfter(float duration)
        {
            yield return new WaitForSecondsRealtime(duration);
            panelRoot.SetActive(false);
            hideCoroutine = null;
        }
    }
}
