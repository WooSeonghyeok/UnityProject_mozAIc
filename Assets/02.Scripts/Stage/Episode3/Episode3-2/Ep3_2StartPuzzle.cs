using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Ep3_2StartPuzzle : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private Ep3_2Manager ep3_2Manager;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private MonoBehaviour backgroundDecorController;

    [Header("카운트다운 UI")]
    [SerializeField] private GameObject countdownPanel;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private Canvas countdownCanvas;
    [SerializeField] private bool autoCreateCountdownUi = true;
    [SerializeField] private Vector2 countdownPanelSize = new Vector2(360f, 180f);
    [SerializeField] private Vector2 countdownPanelOffset = new Vector2(0f, 120f);
    [SerializeField] private Color countdownPanelColor = new Color(0.04f, 0.04f, 0.08f, 0.72f);
    [SerializeField] private float countdownFontSize = 92f;

    [Header("추가 연출/트리거")]
    [SerializeField] private GameObject eventTriggerToEnable;
    [SerializeField] private GameObject interactableToDisable;

    private bool isStarting = false;
    private bool isStarted = false;
    private Coroutine startSequenceCoroutine;
    private bool ownsRuntimeCountdownCanvas;

    private void Awake()
    {
        EnsureCountdownUi();
        HideCountdownPanel();
    }

    public void BeginStartSequence()
    {
        if (isStarting || isStarted)
        {
            return;
        }

        startSequenceCoroutine = StartCoroutine(CoBeginStartSequence());
    }

    public void ResetStartSequence()
    {
        if (startSequenceCoroutine != null)
        {
            StopCoroutine(startSequenceCoroutine);
            startSequenceCoroutine = null;
        }

        isStarting = false;
        isStarted = false;

        HideCountdownPanel();
        SetCountdownLabel(string.Empty);

        if (interactableToDisable != null)
        {
            interactableToDisable.SetActive(true);
        }

        if (backgroundDecorController != null)
        {
            backgroundDecorController.SendMessage("StopFlow", SendMessageOptions.DontRequireReceiver);
            backgroundDecorController.SendMessage("ScatterChildrenNow", SendMessageOptions.DontRequireReceiver);
        }
    }

    private IEnumerator CoBeginStartSequence()
    {
        isStarting = true;

        if (interactableToDisable != null)
        {
            interactableToDisable.SetActive(false);
        }

        if (countdownPanel != null)
        {
            countdownPanel.SetActive(true);
        }

        for (int i = 3; i > 0; i--)
        {
            SetCountdownLabel(i.ToString());

            yield return new WaitForSeconds(1f);
        }

        SetCountdownLabel("START!");

        if (backgroundDecorController != null)
        {
            backgroundDecorController.SendMessage("BeginFlow", SendMessageOptions.DontRequireReceiver);
        }

        if (eventTriggerToEnable != null)
        {
            eventTriggerToEnable.SetActive(true);
        }

        bool shouldPlayLocalMusic = ep3_2Manager == null || !ep3_2Manager.UsesTopDownRhythmPuzzle;
        if (musicSource != null && shouldPlayLocalMusic)
        {
            musicSource.Play();
        }

        if (ep3_2Manager != null)
        {
            ep3_2Manager.StartRhythmStage();
        }

        yield return new WaitForSeconds(0.7f);

        HideCountdownPanel();
        SetCountdownLabel(string.Empty);

        isStarting = false;
        isStarted = true;
        startSequenceCoroutine = null;
    }

    private void EnsureCountdownUi()
    {
        if (countdownPanel != null && countdownText != null)
        {
            return;
        }

        if (!autoCreateCountdownUi)
        {
            return;
        }

        EnsureCountdownCanvas();
        if (countdownCanvas == null)
        {
            return;
        }

        if (countdownPanel == null)
        {
            GameObject panelObject = new GameObject("PuzzleCountdownPanel", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            panelObject.transform.SetParent(countdownCanvas.transform, false);

            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = countdownPanelOffset;
            panelRect.sizeDelta = countdownPanelSize;

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = countdownPanelColor;
            panelImage.raycastTarget = false;

            countdownPanel = panelObject;
        }

        if (countdownText == null && countdownPanel != null)
        {
            GameObject textObject = new GameObject("PuzzleCountdownText", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(countdownPanel.transform, false);

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(20f, 12f);
            textRect.offsetMax = new Vector2(-20f, -12f);

            countdownText = textObject.GetComponent<TextMeshProUGUI>();
            countdownText.font = TMP_Settings.defaultFontAsset;
            countdownText.fontSize = countdownFontSize;
            countdownText.alignment = TextAlignmentOptions.Center;
            countdownText.color = Color.white;
            countdownText.enableWordWrapping = false;
            countdownText.overflowMode = TextOverflowModes.Overflow;
            countdownText.raycastTarget = false;
        }
    }

    private void EnsureCountdownCanvas()
    {
        if (countdownCanvas != null)
        {
            return;
        }

        GameObject canvasRoot = new GameObject("RuntimeCountdownCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        countdownCanvas = canvasRoot.GetComponent<Canvas>();
        countdownCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        countdownCanvas.overrideSorting = true;
        countdownCanvas.sortingOrder = 3000;
        countdownCanvas.pixelPerfect = false;

        CanvasScaler scaler = canvasRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        ownsRuntimeCountdownCanvas = true;
    }

    private void HideCountdownPanel()
    {
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(false);
        }
    }

    private void SetCountdownLabel(string value)
    {
        if (countdownText != null)
        {
            countdownText.text = value;
        }
    }

    private void OnDestroy()
    {
        if (ownsRuntimeCountdownCanvas && countdownCanvas != null)
        {
            Destroy(countdownCanvas.gameObject);
        }
    }
}
