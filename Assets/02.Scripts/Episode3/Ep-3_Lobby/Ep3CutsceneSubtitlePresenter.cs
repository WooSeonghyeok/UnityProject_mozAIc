using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Ep3CutsceneSubtitlePresenter : MonoBehaviour
{
    private const float PanelWidth = 1280f;
    private const float PanelHeight = 156f;

    private enum CutsceneBoxType
    {
        System,
        Player,
        Voice
    }

    private CutsceneManager cutsceneManager;
    private Canvas canvas;
    private CanvasScaler canvasScaler;
    private GraphicRaycaster graphicRaycaster;
    private CanvasGroup canvasGroup;
    private Image panelImage;
    private TextMeshProUGUI messageText;

    public void Configure(TMP_FontAsset fontAsset, CutsceneManager existingCutsceneManager = null)
    {
        if (existingCutsceneManager != null)
        {
            cutsceneManager = existingCutsceneManager;
        }
        else if (cutsceneManager == null)
        {
            cutsceneManager = GetComponentInParent<CutsceneManager>();
        }

        EnsureUi(fontAsset);
    }

    public void Show(Ep3LobbyIntroShotData shot)
    {
        if (TryShowOnCutsceneCanvas(shot))
        {
            return;
        }

        EnsureUi(null);

        if (shot == null || string.IsNullOrWhiteSpace(shot.subtitleText))
        {
            Hide();
            return;
        }

        bool showSpeakerName = shot.showSpeakerName && !string.IsNullOrWhiteSpace(shot.speakerName);
        messageText.alignment = showSpeakerName
            ? TextAlignmentOptions.TopLeft
            : TextAlignmentOptions.MidlineLeft;

        messageText.text = showSpeakerName
            ? $"<size=70%><b>{shot.speakerName}</b></size>\n{shot.subtitleText}"
            : shot.subtitleText;

        canvasGroup.alpha = 1f;
        panelImage.enabled = true;
        messageText.enabled = true;
    }

    public void Hide()
    {
        if (HideCutsceneCanvas())
        {
            return;
        }

        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = 0f;
    }

    private void EnsureUi(TMP_FontAsset fontAsset)
    {
        if (HasBoundCutsceneCanvas())
        {
            HideCutsceneCanvas();
            return;
        }

        if (canvas != null)
        {
            if (fontAsset != null && messageText != null)
            {
                messageText.font = fontAsset;
            }

            return;
        }

        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 400;

        canvasScaler = gameObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0.5f;

        graphicRaycaster = gameObject.AddComponent<GraphicRaycaster>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        GameObject panelObject = new GameObject("SubtitlePanel", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(transform, false);
        RectTransform panelTransform = panelObject.GetComponent<RectTransform>();
        panelTransform.anchorMin = new Vector2(0.5f, 0f);
        panelTransform.anchorMax = new Vector2(0.5f, 0f);
        panelTransform.pivot = new Vector2(0.5f, 0f);
        panelTransform.anchoredPosition = new Vector2(0f, 42f);
        panelTransform.sizeDelta = new Vector2(PanelWidth, PanelHeight);

        panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0.05f, 0.05f, 0.08f, 0.78f);

        GameObject textObject = new GameObject("Message", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(panelTransform, false);
        RectTransform textTransform = textObject.GetComponent<RectTransform>();
        textTransform.anchorMin = Vector2.zero;
        textTransform.anchorMax = Vector2.one;
        textTransform.offsetMin = new Vector2(34f, 24f);
        textTransform.offsetMax = new Vector2(-34f, -24f);

        messageText = textObject.GetComponent<TextMeshProUGUI>();
        messageText.font = fontAsset != null ? fontAsset : TMP_Settings.defaultFontAsset;
        messageText.fontSize = 34f;
        messageText.color = Color.white;
        messageText.enableWordWrapping = true;
        messageText.overflowMode = TextOverflowModes.Overflow;
        messageText.alignment = TextAlignmentOptions.MidlineLeft;
    }

    private bool TryShowOnCutsceneCanvas(Ep3LobbyIntroShotData shot)
    {
        if (!HasBoundCutsceneCanvas())
        {
            return false;
        }

        if (shot == null || string.IsNullOrWhiteSpace(shot.subtitleText))
        {
            HideCutsceneCanvas();
            return true;
        }

        HideCutsceneCanvas();

        switch (ResolveCutsceneBoxType(shot))
        {
            case CutsceneBoxType.Player:
                if (cutsceneManager.text_player == null || cutsceneManager.box_player == null)
                {
                    return false;
                }

                cutsceneManager.text_player.text = shot.subtitleText;
                cutsceneManager.box_player.SetActive(true);
                return true;

            case CutsceneBoxType.Voice:
                if (cutsceneManager.text_voice == null || cutsceneManager.box_voice == null || cutsceneManager.voice_Name == null)
                {
                    return false;
                }

                cutsceneManager.text_voice.text = shot.subtitleText;
                cutsceneManager.voice_Name.text = ResolveSpeakerDisplayName(shot);
                cutsceneManager.voice_Name.color = ResolveSpeakerColor(shot);
                cutsceneManager.box_voice.SetActive(true);
                return true;

            default:
                if (cutsceneManager.text_system == null || cutsceneManager.box_system == null)
                {
                    return false;
                }

                cutsceneManager.text_system.text = shot.subtitleText;
                cutsceneManager.box_system.SetActive(true);
                return true;
        }
    }

    private bool HideCutsceneCanvas()
    {
        if (!HasBoundCutsceneCanvas())
        {
            return false;
        }

        if (cutsceneManager.box_system != null)
        {
            cutsceneManager.box_system.SetActive(false);
        }

        if (cutsceneManager.box_player != null)
        {
            cutsceneManager.box_player.SetActive(false);
        }

        if (cutsceneManager.box_voice != null)
        {
            cutsceneManager.box_voice.SetActive(false);
        }

        return true;
    }

    private bool HasBoundCutsceneCanvas()
    {
        if (cutsceneManager == null)
        {
            cutsceneManager = GetComponentInParent<CutsceneManager>();
        }

        return cutsceneManager != null &&
               cutsceneManager.box_system != null &&
               cutsceneManager.box_player != null &&
               cutsceneManager.box_voice != null &&
               cutsceneManager.text_system != null &&
               cutsceneManager.text_player != null &&
               cutsceneManager.text_voice != null &&
               cutsceneManager.voice_Name != null;
    }

    private static CutsceneBoxType ResolveCutsceneBoxType(Ep3LobbyIntroShotData shot)
    {
        string speakerType = shot != null && !string.IsNullOrWhiteSpace(shot.speakerType)
            ? shot.speakerType.Trim().ToLowerInvariant()
            : string.Empty;

        if (speakerType.Contains("narration") || speakerType.Contains("system"))
        {
            return CutsceneBoxType.System;
        }

        if (speakerType.Contains("monologue") || speakerType.Contains("player") || speakerType.Contains("self"))
        {
            return CutsceneBoxType.Player;
        }

        if (speakerType.Contains("dialogue") || speakerType.Contains("voice") || speakerType.Contains("npc"))
        {
            return CutsceneBoxType.Voice;
        }

        if (shot != null && shot.showSpeakerName && !string.IsNullOrWhiteSpace(shot.speakerName))
        {
            return CutsceneBoxType.Voice;
        }

        return CutsceneBoxType.System;
    }

    private static string ResolveSpeakerDisplayName(Ep3LobbyIntroShotData shot)
    {
        if (shot == null)
        {
            return string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(shot.speakerName))
        {
            return shot.speakerName;
        }

        string speakerType = !string.IsNullOrWhiteSpace(shot.speakerType)
            ? shot.speakerType.Trim().ToLowerInvariant()
            : string.Empty;

        if (speakerType.Contains("musician"))
        {
            return "음악가";
        }

        if (speakerType.Contains("painter"))
        {
            return "화가";
        }

        if (speakerType.Contains("girl"))
        {
            return "소녀";
        }

        if (speakerType.Contains("core"))
        {
            return "???";
        }

        return string.Empty;
    }

    private static Color ResolveSpeakerColor(Ep3LobbyIntroShotData shot)
    {
        string speakerType = shot != null && !string.IsNullOrWhiteSpace(shot.speakerType)
            ? shot.speakerType.Trim().ToLowerInvariant()
            : string.Empty;
        string speakerName = shot != null && !string.IsNullOrWhiteSpace(shot.speakerName)
            ? shot.speakerName.Trim().ToLowerInvariant()
            : string.Empty;

        if (speakerType.Contains("musician") || speakerName.Contains("음악") || speakerName.Contains("leon"))
        {
            return Color.blue;
        }

        if (speakerType.Contains("painter") || speakerName.Contains("화가") || speakerName.Contains("elio"))
        {
            return Color.green;
        }

        if (speakerType.Contains("girl") || speakerName.Contains("luna"))
        {
            return Color.red;
        }

        if (speakerType.Contains("core") || speakerName == "???")
        {
            return Color.gray;
        }

        return Color.white;
    }
}
