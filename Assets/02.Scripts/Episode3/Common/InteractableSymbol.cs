namespace Episode3.Common
{
    using System.Collections;
    using TMPro;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    public enum SymbolLockRequirement
    {
        None = 0,
        Stage3_1Visited = 1
    }

    /// <summary>
    /// 벽에 붙이는 심볼(상호작용 오브젝트)
    /// 
    /// 기능:
    /// 1. 플레이어가 범위 안에 들어오면 상호작용 UI를 켠다
    /// 2. 플레이어가 범위 밖으로 나가면 상호작용 UI를 끈다
    /// 3. 플레이어가 상호작용 키를 누르면 심볼과 상호작용한다
    /// 4. 잠금 조건이 있으면 잠금 메시지를 띄운다
    /// 5. 잠금이 없으면 씬 이동 또는 UnityEvent를 실행한다
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class InteractableSymbol : MonoBehaviour
    {
        [Header("상호작용 설정")]
        [Tooltip("체크하면 sceneName을 사용하여 SceneManager.LoadScene 호출")]
        [SerializeField] private bool useSceneName = true;

        [Tooltip("이동할 씬 이름")]
        [SerializeField] private string sceneNameValue;

        [Header("잠금 설정")]
        [Tooltip("이 심볼이 열리기 위해 필요한 조건")]
        [SerializeField] private SymbolLockRequirement lockRequirement = SymbolLockRequirement.None;

        [Tooltip("잠겨 있을 때 띄울 메시지")]
        [SerializeField] private string lockedMessage = "다른 곳을 먼저 가야 할 것 같아";

        [Tooltip("잠금 메시지를 몇 초 동안 보여줄지")]
        [SerializeField] private float lockedMessageDuration = 1.5f;

        [Tooltip("잠금 팝업 프리팹의 Resources 경로")]
        [SerializeField] private string lockedPopupResourcePath = "Prefabs/UI/Episode3SymbolLockPopup";

        [Header("상호작용 안내 UI")]
        [Tooltip("하이라키에 만들어 둔 E키 안내 UI 오브젝트를 연결")]
        [SerializeField] private GameObject interactUI;

        [Header("디자이너 훅 (인스펙터에서 연결)")]
        [Tooltip("useSceneName이 꺼져 있을 때 실행할 이벤트")]
        [SerializeField] private UnityEvent onInteract;

        /// <summary>
        /// 플레이어 입력 스크립트 참조
        /// 기존 구조를 유지해서 PlayerInput의 Interact 이벤트를 구독함
        /// </summary>
        private PlayerInput user;

        /// <summary>
        /// 플레이어 태그 이름
        /// 플레이어 오브젝트가 이 태그를 가지고 있어야 함
        /// </summary>
        private readonly string playerTag = "Player";

        /// <summary>
        /// 현재 플레이어가 심볼 범위 안에 들어와 있는지 여부
        /// </summary>
        private bool playerInRange;

        private void Awake()
        {
            // Player 태그를 가진 오브젝트를 찾아서 PlayerInput 컴포넌트를 가져옴
            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

            if (playerObject != null)
            {
                user = playerObject.GetComponent<PlayerInput>();
            }
            else
            {
                Debug.LogWarning($"[InteractableSymbol] Player 태그 오브젝트를 찾지 못했습니다. 오브젝트: {name}");
            }

            // 시작할 때 상호작용 UI는 꺼 둠
            // 하이라키에서 이미 꺼져 있어도 안전하게 한 번 더 꺼 줌
            if (interactUI != null)
            {
                interactUI.SetActive(false);
            }
        }

        private void OnEnable()
        {
            // PlayerInput이 정상적으로 찾아졌을 때만 이벤트 구독
            if (user != null)
            {
                user.Interact += SymbolInteract;
            }
        }

        private void OnDisable()
        {
            // 비활성화될 때 이벤트 해제
            // 해제 안 하면 중복 구독 또는 예기치 않은 호출이 생길 수 있음
            if (user != null)
            {
                user.Interact -= SymbolInteract;
            }
        }

        private void Reset()
        {
            // 이 컴포넌트를 붙였을 때 Collider가 자동으로 Trigger가 되도록 설정
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // 플레이어가 아니면 무시
            if (!other.CompareTag(playerTag)) return;

            // 플레이어가 범위 안으로 들어왔음을 기록
            playerInRange = true;

            // 상호작용 안내 UI 켜기
            if (interactUI != null)
            {
                interactUI.SetActive(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // 플레이어가 아니면 무시
            if (!other.CompareTag(playerTag)) return;

            // 플레이어가 범위 밖으로 나갔음을 기록
            playerInRange = false;

            // 상호작용 안내 UI 끄기
            if (interactUI != null)
            {
                interactUI.SetActive(false);
            }
        }

        /// <summary>
        /// PlayerInput의 Interact 이벤트가 들어왔을 때 호출되는 함수
        /// 플레이어가 범위 안에 있을 때만 실제 상호작용 수행
        /// </summary>
        public void SymbolInteract()
        {
            if (!playerInRange) return;

            PerformInteraction();
        }

        /// <summary>
        /// 실제 상호작용 처리
        /// 1. UI 끄기
        /// 2. 잠금 조건 검사
        /// 3. 잠겨 있으면 메시지 출력
        /// 4. 잠금이 없으면 씬 이동 또는 이벤트 실행
        /// </summary>
        private void PerformInteraction()
        {
            // 상호작용이 시작되면 안내 UI는 끔
            if (interactUI != null)
            {
                interactUI.SetActive(false);
            }

            // 잠금 조건을 만족하지 못하면 잠금 메시지를 띄우고 종료
            if (!CanInteract())
            {
                SymbolLockMessageUI.Show(lockedMessage, lockedMessageDuration, lockedPopupResourcePath);
                return;
            }

            // 씬 이동 방식일 때
            if (useSceneName)
            {
                if (!string.IsNullOrEmpty(sceneNameValue))
                {
                    SceneManager.LoadScene(sceneNameValue);
                }
                else
                {
                    Debug.LogWarning($"[InteractableSymbol] sceneName이 비어 있습니다. 오브젝트: {name}");
                }
            }
            else
            {
                // 씬 이동 대신 인스펙터에서 연결한 이벤트를 실행
                onInteract?.Invoke();
            }
        }

        /// <summary>
        /// 이 심볼과 상호작용 가능한지 검사
        /// </summary>
        private bool CanInteract()
        {
            return lockRequirement switch
            {
                SymbolLockRequirement.None => true,

                // 3-1을 한 번이라도 방문했을 때만 열리는 조건
                SymbolLockRequirement.Stage3_1Visited =>
                    Ep_3Manager.Instance != null && Ep_3Manager.Instance.HasVisitedStage3_1,

                _ => true
            };
        }
    }

    /// <summary>
    /// 심볼이 잠겨 있을 때 메시지를 화면에 띄우는 UI 관리자
    /// 
    /// 기능:
    /// 1. Resources에서 잠금 팝업 프리팹을 불러옴
    /// 2. 없으면 코드로 간단한 대체 UI를 생성
    /// 3. 일정 시간 뒤 자동으로 숨김
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
        /// 외부에서 잠금 메시지를 띄울 때 사용하는 정적 함수
        /// </summary>
        public static void Show(string message, float duration, string popupResourcePath)
        {
            EnsureInstance(popupResourcePath);
            instance.ShowInternal(message, duration);
        }

        /// <summary>
        /// 인스턴스가 없으면 생성하고, 팝업도 준비함
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
        /// 잠금 메시지를 띄울 전용 Canvas 생성
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
        /// 지정한 경로의 팝업 프리팹을 불러옴
        /// 프리팹이 없으면 fallback UI 사용
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
                Debug.LogWarning($"[SymbolLockMessageUI] 잠금 팝업 프리팹을 찾지 못해 fallback UI를 사용합니다. path: {resolvedPath}");
                BuildFallbackPopup();
                currentPopupResourcePath = string.Empty;
                return;
            }

            currentPopupResourcePath = resolvedPath;
            panelRoot.SetActive(false);
        }

        /// <summary>
        /// 기존 팝업 제거
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
        /// 프리팹이 없을 때 사용할 간단한 코드 생성 UI
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
        /// 기본 폰트 로드
        /// </summary>
        private Font LoadDefaultFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null) return font;

            return Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        /// <summary>
        /// 실제 메시지를 화면에 표시
        /// </summary>
        private void ShowInternal(string message, float duration)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                message = "3-1 심볼을 먼저 확인해야 할 것 같아";
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
        /// 일정 시간 뒤 메시지를 자동으로 숨김
        /// </summary>
        private IEnumerator HideAfter(float duration)
        {
            yield return new WaitForSecondsRealtime(duration);
            panelRoot.SetActive(false);
            hideCoroutine = null;
        }
    }
}