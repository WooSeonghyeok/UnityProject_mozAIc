using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CutsceneHintOverlay : MonoBehaviour
{
    private const string DefaultSkipHint = "ESC 스킵";
    private const string DefaultAdvanceHint = "Space / Enter / 클릭 다음";

    private static readonly Vector2 SkipHintSize = new(260f, 44f);
    private static readonly Vector2 AdvanceHintSize = new(520f, 48f);
    private static readonly Vector2 SkipHintPosition = new(28f, -24f);
    private static readonly Vector2 AdvanceHintPosition = new(0f, 214f);

    private Canvas canvas;
    private CanvasScaler canvasScaler;
    private GraphicRaycaster graphicRaycaster;
    private CanvasGroup canvasGroup;
    private TextMeshProUGUI skipHintText;
    private TextMeshProUGUI advanceHintText;

    public static CutsceneHintOverlay GetOrCreate(Transform parent, TMP_FontAsset fontAsset = null)
    {
        if (parent == null)
        {
            return null;
        }

        CutsceneHintOverlay existing = parent.GetComponentInChildren<CutsceneHintOverlay>(true);
        if (existing != null)
        {
            existing.Configure(fontAsset);
            return existing;
        }

        GameObject overlayObject = new GameObject("CutsceneHintOverlay", typeof(RectTransform));
        overlayObject.transform.SetParent(parent, false);

        CutsceneHintOverlay overlay = overlayObject.AddComponent<CutsceneHintOverlay>();
        overlay.Configure(fontAsset);
        return overlay;
    }

    public void Configure(TMP_FontAsset fontAsset = null)
    {
        EnsureUi(fontAsset);
    }

    public void Show(string skipHint = null, string advanceHint = null, TMP_FontAsset fontAsset = null)
    {
        EnsureUi(fontAsset);

        if (skipHintText != null)
        {
            skipHintText.text = string.IsNullOrWhiteSpace(skipHint) ? DefaultSkipHint : skipHint;
        }

        if (advanceHintText != null)
        {
            advanceHintText.text = string.IsNullOrWhiteSpace(advanceHint) ? DefaultAdvanceHint : advanceHint;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }

    public void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    private void EnsureUi(TMP_FontAsset fontAsset)
    {
        RectTransform rootRect = transform as RectTransform;
        if (rootRect == null)
        {
            rootRect = gameObject.AddComponent<RectTransform>();
        }

        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;
        rootRect.localScale = Vector3.one;

        if (GetComponentInParent<Canvas>() == null)
        {
            if (canvas == null)
            {
                canvas = gameObject.GetComponent<Canvas>();
            }

            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            if (canvasScaler == null)
            {
                canvasScaler = gameObject.GetComponent<CanvasScaler>();
            }

            if (canvasScaler == null)
            {
                canvasScaler = gameObject.AddComponent<CanvasScaler>();
            }

            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;

            if (graphicRaycaster == null)
            {
                graphicRaycaster = gameObject.GetComponent<GraphicRaycaster>();
            }

            if (graphicRaycaster == null)
            {
                graphicRaycaster = gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.GetComponent<CanvasGroup>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        if (skipHintText == null)
        {
            skipHintText = CreateHintText(
                "SkipHint",
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                SkipHintPosition,
                SkipHintSize,
                TextAlignmentOptions.TopLeft,
                fontAsset);
        }

        if (advanceHintText == null)
        {
            advanceHintText = CreateHintText(
                "AdvanceHint",
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                AdvanceHintPosition,
                AdvanceHintSize,
                TextAlignmentOptions.Center,
                fontAsset);
        }

        if (fontAsset != null)
        {
            skipHintText.font = fontAsset;
            advanceHintText.font = fontAsset;
        }
    }

    private TextMeshProUGUI CreateHintText(
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        TextAlignmentOptions alignment,
        TMP_FontAsset fontAsset)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(Shadow));
        textObject.transform.SetParent(transform, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = fontAsset != null ? fontAsset : TMP_Settings.defaultFontAsset;
        text.fontSize = 28f;
        text.fontStyle = FontStyles.Bold;
        text.color = new Color(1f, 1f, 1f, 0.95f);
        text.alignment = alignment;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;

        Shadow shadow = textObject.GetComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
        shadow.effectDistance = new Vector2(2f, -2f);
        shadow.useGraphicAlpha = true;

        return text;
    }
}
