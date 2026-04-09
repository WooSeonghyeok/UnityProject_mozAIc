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
    /// 벽에 붙이는 심볼(상호작용 오브젝트).
    /// - 입력 처리 책임을 플레이어 인풋 스크립트에 두지 않고,
    ///   콜라이더 범위 내에 있는 `Player` 태그 오브젝트가 지정된 키를 누르면 상호작용 수행.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class InteractableSymbol : MonoBehaviour
    {
        [Header("상호작용 설정")]
        [Tooltip("체크하면 sceneName을 사용하여 SceneManager.LoadScene 호출")]
        [SerializeField] private bool useSceneName = true;
        [SerializeField] private string sceneNameValue;

        [Header("잠금 설정")]
        [SerializeField] private SymbolLockRequirement lockRequirement = SymbolLockRequirement.None;
        [SerializeField] private string lockedMessage = "지금은 갈 수 없습니다.";
        [SerializeField] private float lockedMessageDuration = 1.5f;
        [SerializeField] private string lockedPopupResourcePath = "Prefabs/UI/Episode3SymbolLockPopup";

        [Header("키 설정")]
        [Tooltip("범위 내 플레이어가 누르면 상호작용이 발생하는 키")]
        private PlayerInput user;

        private readonly string playerTag = "Player";

        [Header("디자이너 훅 (인스펙터에서 연결)")]
        [SerializeField] private UnityEvent onInteract;

        private bool playerInRange;

        private void Awake()
        {
            user = GameObject.FindGameObjectWithTag(playerTag).GetComponent<PlayerInput>();
        }

        private void OnEnable()
        {
            user.Interact += SymbolInteract;
        }

        private void OnDisable()
        {
            user.Interact -= SymbolInteract;
        }

        private void Reset()
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;
            playerInRange = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;
            playerInRange = false;
        }

        public void SymbolInteract()
        {
            if (!playerInRange) return;
            PerformInteraction();
        }

        private void PerformInteraction()
        {
            if (!CanInteract())
            {
                SymbolLockMessageUI.Show(lockedMessage, lockedMessageDuration, lockedPopupResourcePath);
                return;
            }

            if (useSceneName)
            {
                if (!string.IsNullOrEmpty(sceneNameValue))
                {
                    SceneManager.LoadScene(sceneNameValue);
                }
                else
                {
                    Debug.LogWarning($"[InteractableSymbol] sceneName이 비어있습니다. 오브젝트: {name}");
                }
            }
            else
            {
                onInteract?.Invoke();
            }
        }

        private bool CanInteract()
        {
            return lockRequirement switch
            {
                SymbolLockRequirement.None => true,
                SymbolLockRequirement.Stage3_1Visited => Ep_3Manager.Instance != null && Ep_3Manager.Instance.HasVisitedStage3_1,
                _ => true
            };
        }
    }

    public sealed class SymbolLockMessageUI : MonoBehaviour
    {
        private const string DefaultPopupResourcePath = "Prefabs/UI/Episode3SymbolLockPopup";

        private static SymbolLockMessageUI instance;

        private GameObject panelRoot;
        private TMP_Text popupText;
        private Text fallbackText;
        private Coroutine hideCoroutine;
        private string currentPopupResourcePath = string.Empty;

        public static void Show(string message, float duration, string popupResourcePath)
        {
            EnsureInstance(popupResourcePath);
            instance.ShowInternal(message, duration);
        }

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

        private Font LoadDefaultFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null) return font;

            return Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

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

        private IEnumerator HideAfter(float duration)
        {
            yield return new WaitForSecondsRealtime(duration);
            panelRoot.SetActive(false);
            hideCoroutine = null;
        }
    }
}
