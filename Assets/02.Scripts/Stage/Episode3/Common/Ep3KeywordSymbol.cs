using System.Collections;
using Episode3.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Ep3KeywordSymbol : MonoBehaviour
{
    [Header("키워드 설정")]
    [SerializeField] private string keywordId = "rhythm";
    [SerializeField] private string keywordDisplayName = "리듬";

    [TextArea(2, 4)]
    [SerializeField] private string discoverMessage = "갑자기 머릿속에 '리듬'이라는 단어가 떠올랐다.\n이 키워드로 음악가와 대화해 보자.";

    [SerializeField] private float popupDuration = 2.8f;

    [Header("표시 연출")]
    [SerializeField] private Color keywordColor = new(1f, 0.92f, 0.55f, 1f);
    [SerializeField] private Color messageColor = new(1f, 1f, 1f, 1f);

    [Header("참조")]
    [SerializeField] private InteractableSymbol interactableSymbol;
    [SerializeField] private Ep3_1Manager stage3_1Manager;
    [SerializeField] private Ep3_2Manager stage3_2Manager;
    [SerializeField] private Ep3_3Manager stage3_3Manager;

    [Header("획득 후 처리")]
    [SerializeField] private bool disableInteractionAfterDiscover = true;
    [SerializeField] private bool hideSymbolAfterDiscover = true;

    private bool isDiscovered;

    private void Awake()
    {
        interactableSymbol ??= GetComponent<InteractableSymbol>();
        stage3_1Manager ??= FindFirstObjectByType<Ep3_1Manager>();
        stage3_2Manager ??= FindFirstObjectByType<Ep3_2Manager>();
        stage3_3Manager ??= FindFirstObjectByType<Ep3_3Manager>();
    }

    public void TriggerKeywordDiscovery()
    {
        if (isDiscovered)
        {
            return;
        }

        isDiscovered = true;

        if (!string.IsNullOrWhiteSpace(keywordId) && TryRegisterKeywordTag(keywordId))
        {
        }
        else
        {
            Debug.LogWarning($"[Ep3KeywordSymbol] 스테이지 매니저를 찾지 못해 키워드 '{keywordId}'를 저장하지 못했습니다.");
        }

        string resolvedMessage = string.IsNullOrWhiteSpace(discoverMessage)
            ? $"갑자기 머릿속에 '{keywordDisplayName}'이라는 단어가 떠올랐다.\n이 키워드로 음악가와 대화해 보자."
            : discoverMessage;

        Ep3KeywordDiscoveryUI.Show(keywordDisplayName, resolvedMessage, popupDuration, keywordColor, messageColor);

        if (disableInteractionAfterDiscover && interactableSymbol != null)
        {
            interactableSymbol.SetInteractionEnabled(false);
        }

        if (hideSymbolAfterDiscover)
        {
            StartCoroutine(HideSymbolAtEndOfFrame());
        }
    }

    private IEnumerator HideSymbolAtEndOfFrame()
    {
        yield return null;
        gameObject.SetActive(false);
    }

    private bool TryRegisterKeywordTag(string tag)
    {
        if (stage3_2Manager != null)
        {
            stage3_2Manager.AddTag(tag);
            return true;
        }

        if (stage3_1Manager != null)
        {
            stage3_1Manager.AddTag(tag);
            return true;
        }

        if (stage3_3Manager != null)
        {
            stage3_3Manager.AddTag(tag);
            return true;
        }

        return false;
    }
}

public sealed class Ep3KeywordDiscoveryUI : MonoBehaviour
{
    private static Ep3KeywordDiscoveryUI instance;

    private Canvas rootCanvas;
    private TMP_Text keywordText;
    private TMP_Text messageText;
    private Coroutine hideCoroutine;

    public static void Show(string keyword, string message, float duration, Color keywordColor, Color messageColor)
    {
        EnsureInstance();
        instance.ShowInternal(keyword, message, duration, keywordColor, messageColor);
    }

    private static void EnsureInstance()
    {
        instance ??= FindFirstObjectByType<Ep3KeywordDiscoveryUI>();

        if (instance != null)
        {
            return;
        }

        GameObject root = new("Ep3KeywordDiscoveryUI");
        DontDestroyOnLoad(root);
        instance = root.AddComponent<Ep3KeywordDiscoveryUI>();
        instance.BuildUI();
    }

    private void BuildUI()
    {
        rootCanvas = gameObject.AddComponent<Canvas>();
        rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        rootCanvas.sortingOrder = 1300;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        gameObject.AddComponent<GraphicRaycaster>();

        keywordText = CreateText(
            "KeywordTitle",
            new Vector2(0.5f, 0.58f),
            new Vector2(1200f, 110f),
            70f,
            FontStyles.Bold);

        messageText = CreateText(
            "KeywordMessage",
            new Vector2(0.5f, 0.47f),
            new Vector2(1300f, 160f),
            34f,
            FontStyles.Normal);

        SetTextsVisible(false);
    }

    private TMP_Text CreateText(string objectName, Vector2 anchor, Vector2 size, float fontSize, FontStyles style)
    {
        GameObject textObject = new(objectName);
        textObject.transform.SetParent(transform, false);

        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Overflow;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.lineSpacing = 8f;
        text.raycastTarget = false;
        text.outlineWidth = 0.22f;
        text.outlineColor = new Color(0f, 0f, 0f, 0.85f);

        return text;
    }

    private void ShowInternal(string keyword, string message, float duration, Color keywordColor, Color messageColor)
    {
        if (keywordText == null || messageText == null)
        {
            BuildUI();
        }

        keywordText.text = keyword;
        keywordText.color = keywordColor;

        messageText.text = message;
        messageText.color = messageColor;

        SetTextsVisible(true);

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        hideCoroutine = StartCoroutine(HideAfter(Mathf.Max(0.1f, duration)));
    }

    private IEnumerator HideAfter(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        SetTextsVisible(false);
        hideCoroutine = null;
    }

    private void SetTextsVisible(bool visible)
    {
        if (keywordText != null)
        {
            keywordText.enabled = visible;
        }

        if (messageText != null)
        {
            messageText.enabled = visible;
        }
    }
}
