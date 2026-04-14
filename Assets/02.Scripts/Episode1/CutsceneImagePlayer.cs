using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CutsceneImagePlayer : MonoBehaviour
{
    [Header("컷씬 UI")]
    [SerializeField] private GameObject cutscenePanel;
    [SerializeField] private Image cutsceneImage;

    [Header("컷씬 자막")]
    [SerializeField] private TextMeshProUGUI cutsceneText;
    [SerializeField] private TMP_FontAsset subtitleFont;
    [SerializeField] private bool autoCreateSubtitleUi = true;
    [SerializeField] private Vector2 subtitlePanelSize = new Vector2(1280f, 156f);
    [SerializeField] private Vector2 subtitlePanelOffset = new Vector2(0f, 42f);
    [SerializeField] private Vector4 subtitlePadding = new Vector4(34f, 24f, 34f, 24f);
    [SerializeField] private Color subtitleBackgroundColor = new Color(0.05f, 0.05f, 0.08f, 0.78f);
    [SerializeField] private float subtitleFontSize = 34f;
    [TextArea(2, 4)]
    [SerializeField] private string[] cutsceneTexts;

    [Header("컷씬 이미지")]
    [SerializeField] private Sprite[] cutsceneSprites;
    [SerializeField] private float imageShowTime = 3f;
    [SerializeField] private float fadeDuration = 1f;

    [Header("플레이어 제어")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("엔딩 컷씬")]
    [SerializeField] private bool isEndCutscene = false;

    [Header("이벤트")]
    [SerializeField] private UnityEvent onCutsceneFinished;

    private AspectRatioFitter aspectFitter;
    private CanvasGroup subtitleCanvasGroup;
    private Image subtitleBackground;
    private bool isPlaying = false;
    private bool subtitleVisible;

    public bool IsPlaying => isPlaying;
    public bool HasConfiguredImages => cutsceneSprites != null && cutsceneSprites.Length > 0;

    public void AddFinishedListener(UnityAction listener)
    {
        onCutsceneFinished?.AddListener(listener);
    }

    public void RemoveFinishedListener(UnityAction listener)
    {
        onCutsceneFinished?.RemoveListener(listener);
    }

    private void Awake()
    {
        if (cutsceneImage != null)
        {
            cutsceneImage.preserveAspect = true;
            aspectFitter = cutsceneImage.GetComponent<AspectRatioFitter>();
        }

        EnsureSubtitleUi();
        SetSubtitle(string.Empty);

        if (cutscenePanel != null)
        {
            cutscenePanel.SetActive(false);
        }
    }

    public void PlayCutscene()
    {
        if (isPlaying)
            return;

        if (!HasConfiguredImages)
        {
            onCutsceneFinished?.Invoke();
            return;
        }

        StartCoroutine(PlayCutsceneRoutine());
    }

    private IEnumerator PlayCutsceneRoutine()
    {
        isPlaying = true;

        if (cutscenePanel != null)
            cutscenePanel.SetActive(true);

        if (playerMovement != null)
            playerMovement.SetMoveLock(true);

        int stepCount = Mathf.Max(
            cutsceneSprites != null ? cutsceneSprites.Length : 0,
            cutsceneTexts != null ? cutsceneTexts.Length : 0);

        for (int i = 0; i < stepCount; i++)
        {
            Sprite currentSprite = ResolveSpriteForStep(i);
            if (cutsceneImage != null && currentSprite != null)
            {
                cutsceneImage.sprite = currentSprite;
            }

            SetSubtitle(ResolveSubtitleForStep(i));

            yield return StartCoroutine(Fade(0f, 1f));
            yield return new WaitForSeconds(imageShowTime);

            if (i < stepCount - 1)
            {
                yield return StartCoroutine(Fade(1f, 0f));
            }
        }

        if (!isEndCutscene)
        {
            yield return StartCoroutine(Fade(1f, 0f));
            SetSubtitle(string.Empty);

            if (cutscenePanel != null)
                cutscenePanel.SetActive(false);
        }
        else
        {
            SetVisualAlpha(1f);
        }

        if (playerMovement != null)
            playerMovement.SetMoveLock(false);

        isPlaying = false;
        onCutsceneFinished?.Invoke();
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        if (cutsceneImage == null)
            yield break;

        float elapsed = 0f;
        SetVisualAlpha(startAlpha);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            SetVisualAlpha(alpha);
            yield return null;
        }

        SetVisualAlpha(endAlpha);
    }

    private void SetImageAlpha(float alpha)
    {
        if (cutsceneImage == null)
            return;

        Color color = cutsceneImage.color;
        color.a = alpha;
        cutsceneImage.color = color;
    }

    private void SetVisualAlpha(float alpha)
    {
        SetImageAlpha(alpha);

        if (subtitleCanvasGroup != null)
        {
            subtitleCanvasGroup.alpha = subtitleVisible ? alpha : 0f;
        }
    }

    private void EnsureSubtitleUi()
    {
        if (cutsceneText != null)
        {
            ConfigureSubtitleText(cutsceneText);

            RectTransform existingRoot = cutsceneText.transform.parent as RectTransform ?? cutsceneText.rectTransform;
            subtitleCanvasGroup = existingRoot.GetComponent<CanvasGroup>();
            if (subtitleCanvasGroup == null)
            {
                subtitleCanvasGroup = existingRoot.gameObject.AddComponent<CanvasGroup>();
            }

            subtitleBackground = existingRoot.GetComponent<Image>();
            subtitleCanvasGroup.alpha = 0f;
            return;
        }

        if (!autoCreateSubtitleUi || cutscenePanel == null)
            return;

        GameObject subtitleRoot = new GameObject("SubtitlePanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        subtitleRoot.transform.SetParent(cutscenePanel.transform, false);

        RectTransform subtitleRootRect = subtitleRoot.GetComponent<RectTransform>();
        subtitleRootRect.anchorMin = new Vector2(0.5f, 0f);
        subtitleRootRect.anchorMax = new Vector2(0.5f, 0f);
        subtitleRootRect.pivot = new Vector2(0.5f, 0f);
        subtitleRootRect.anchoredPosition = subtitlePanelOffset;
        subtitleRootRect.sizeDelta = subtitlePanelSize;

        subtitleBackground = subtitleRoot.GetComponent<Image>();
        subtitleBackground.color = subtitleBackgroundColor;

        subtitleCanvasGroup = subtitleRoot.GetComponent<CanvasGroup>();
        subtitleCanvasGroup.alpha = 0f;

        GameObject textObject = new GameObject("SubtitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(subtitleRoot.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(subtitlePadding.x, subtitlePadding.y);
        textRect.offsetMax = new Vector2(-subtitlePadding.z, -subtitlePadding.w);

        cutsceneText = textObject.GetComponent<TextMeshProUGUI>();
        ConfigureSubtitleText(cutsceneText);
    }

    private void ConfigureSubtitleText(TextMeshProUGUI targetText)
    {
        if (targetText == null)
            return;

        targetText.font = subtitleFont != null ? subtitleFont : TMP_Settings.defaultFontAsset;
        targetText.fontSize = subtitleFontSize;
        targetText.color = Color.white;
        targetText.enableWordWrapping = true;
        targetText.overflowMode = TextOverflowModes.Overflow;
        targetText.alignment = TextAlignmentOptions.MidlineLeft;
    }

    private void SetSubtitle(string subtitle)
    {
        subtitleVisible = !string.IsNullOrWhiteSpace(subtitle) && cutsceneText != null;

        if (cutsceneText != null)
        {
            cutsceneText.text = subtitleVisible ? subtitle : string.Empty;
            cutsceneText.enabled = subtitleVisible;
        }

        if (subtitleBackground != null)
        {
            subtitleBackground.enabled = subtitleVisible;
        }

        if (subtitleCanvasGroup != null)
        {
            subtitleCanvasGroup.alpha = subtitleVisible ? 1f : 0f;
        }
    }

    private Sprite ResolveSpriteForStep(int stepIndex)
    {
        if (cutsceneSprites == null || cutsceneSprites.Length == 0)
            return null;

        int safeIndex = Mathf.Clamp(stepIndex, 0, cutsceneSprites.Length - 1);
        return cutsceneSprites[safeIndex];
    }

    private string ResolveSubtitleForStep(int stepIndex)
    {
        if (cutsceneTexts == null || stepIndex < 0 || stepIndex >= cutsceneTexts.Length)
            return string.Empty;

        return cutsceneTexts[stepIndex];
    }
}
