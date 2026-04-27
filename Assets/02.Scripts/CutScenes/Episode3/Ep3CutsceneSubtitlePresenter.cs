using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Ep3CutsceneSubtitlePresenter : MonoBehaviour
{
    private static readonly Vector2 DialogueBoxSize = new Vector2(1480f, 184f);
    private static readonly Vector2 SystemBoxSize = new Vector2(1460f, 112f);
    private static readonly Vector2 SystemBoxPosition = new Vector2(0f, -110f);
    private static readonly Vector2 PlayerBoxPosition = new Vector2(0f, 220f);
    private static readonly Vector2 VoiceBoxPosition = new Vector2(0f, 56f);
    private static readonly Color PlayerPanelColor = new Color(0.05f, 0.05f, 0.08f, 0.74f);
    private static readonly Color VoicePanelColor = new Color(0.04f, 0.04f, 0.07f, 0.84f);
    private static readonly Color DialogueTextColor = Color.white;

    private enum CutsceneBoxType
    {
        System,
        Player,
        Voice
    }

    private TextboxManager textboxManager;
    private Canvas canvas;
    private CanvasScaler canvasScaler;
    private GraphicRaycaster graphicRaycaster;
    private CanvasGroup canvasGroup;
    private Image panelImage;
    private TextMeshProUGUI speakerText;
    private TextMeshProUGUI messageText;
    private bool forceStandaloneOverlay;

    public void Configure(TMP_FontAsset fontAsset, TextboxManager existingTextboxManager = null, bool forceStandaloneOverlay = false)
    {
        this.forceStandaloneOverlay = forceStandaloneOverlay;

        if (forceStandaloneOverlay)
        {
            textboxManager = null;
        }
        else if (existingTextboxManager != null)
        {
            textboxManager = existingTextboxManager;
        }
        else if (textboxManager == null)
        {
            textboxManager = GetComponentInParent<TextboxManager>();
        }

        EnsureUi(fontAsset);
        Hide();
    }

    public void Show(Ep3LobbyIntroShotData shot)
    {
        if (!forceStandaloneOverlay && TryShowOnCutsceneCanvas(shot))
        {
            return;
        }

        EnsureUi(null);

        if (shot == null || string.IsNullOrWhiteSpace(shot.subtitleText))
        {
            Hide();
            return;
        }

        ApplyStandaloneStyle(shot);

        canvasGroup.alpha = 1f;
    }

    public void Hide()
    {
        if (!forceStandaloneOverlay && HideCutsceneCanvas())
        {
            return;
        }

        if (canvasGroup == null)
        {
            return;
        }

        if (panelImage != null)
        {
            panelImage.enabled = false;
        }

        if (messageText != null)
        {
            messageText.text = string.Empty;
            messageText.enabled = false;
        }

        if (speakerText != null)
        {
            speakerText.text = string.Empty;
            speakerText.enabled = false;
        }

        canvasGroup.alpha = 0f;

        if (panelImage != null)
        {
            panelImage.enabled = false;
        }

        if (speakerText != null)
        {
            speakerText.enabled = false;
        }

        if (messageText != null)
        {
            messageText.enabled = false;
        }
    }

    private void EnsureUi(TMP_FontAsset fontAsset)
    {
        if (!forceStandaloneOverlay && HasBoundCutsceneCanvas())
        {
            HideCutsceneCanvas();
            return;
        }

        if (canvas != null)
        {
            if (fontAsset != null)
            {
                if (messageText != null)
                {
                    messageText.font = fontAsset;
                }

                if (speakerText != null)
                {
                    speakerText.font = fontAsset;
                }
            }

            if (fontAsset != null && speakerText != null)
            {
                speakerText.font = fontAsset;
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
        panelTransform.anchoredPosition = VoiceBoxPosition;
        panelTransform.sizeDelta = DialogueBoxSize;

        panelImage = panelObject.GetComponent<Image>();
        panelImage.color = VoicePanelColor;
        panelImage.raycastTarget = false;

        GameObject speakerObject = new GameObject("Speaker", typeof(RectTransform), typeof(TextMeshProUGUI));
        speakerObject.transform.SetParent(panelTransform, false);

        RectTransform speakerRect = speakerObject.GetComponent<RectTransform>();
        speakerRect.anchorMin = new Vector2(0f, 1f);
        speakerRect.anchorMax = new Vector2(0f, 1f);
        speakerRect.pivot = new Vector2(0f, 1f);
        speakerRect.anchoredPosition = new Vector2(34f, -16f);
        speakerRect.sizeDelta = new Vector2(420f, 38f);

        speakerText = speakerObject.GetComponent<TextMeshProUGUI>();
        speakerText.font = fontAsset != null ? fontAsset : TMP_Settings.defaultFontAsset;
        speakerText.fontSize = 24f;
        speakerText.color = Color.white;
        speakerText.alignment = TextAlignmentOptions.TopLeft;
        speakerText.enableWordWrapping = false;
        speakerText.raycastTarget = false;

        GameObject messageObject = new GameObject("Message", typeof(RectTransform), typeof(TextMeshProUGUI));
        messageObject.transform.SetParent(panelTransform, false);

        RectTransform messageRect = messageObject.GetComponent<RectTransform>();
        messageRect.anchorMin = Vector2.zero;
        messageRect.anchorMax = Vector2.one;
        messageRect.offsetMin = new Vector2(34f, 22f);
        messageRect.offsetMax = new Vector2(-34f, -22f);

        messageText = messageObject.GetComponent<TextMeshProUGUI>();
        messageText.font = fontAsset != null ? fontAsset : TMP_Settings.defaultFontAsset;
        messageText.fontSize = 31f;
        messageText.color = DialogueTextColor;
        messageText.enableWordWrapping = true;
        messageText.overflowMode = TextOverflowModes.Overflow;
        messageText.alignment = TextAlignmentOptions.MidlineLeft;
        messageText.raycastTarget = false;
    }

    private void ApplyStandaloneStyle(Ep3LobbyIntroShotData shot)
    {
        RectTransform panelRect = panelImage.rectTransform;
        RectTransform messageRect = messageText.rectTransform;
        RectTransform speakerRect = speakerText.rectTransform;
        CutsceneBoxType boxType = ResolveCutsceneBoxType(shot);

        panelImage.enabled = true;
        messageText.enabled = true;

        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.sizeDelta = DialogueBoxSize;

        speakerText.enabled = false;
        speakerText.text = string.Empty;
        speakerText.color = ResolveSpeakerColor(shot);

        switch (boxType)
        {
            case CutsceneBoxType.System:
                panelRect.anchorMin = new Vector2(0.5f, 1f);
                panelRect.anchorMax = new Vector2(0.5f, 1f);
                panelRect.pivot = new Vector2(0.5f, 1f);
                panelRect.anchoredPosition = SystemBoxPosition;
                panelRect.sizeDelta = SystemBoxSize;
                panelImage.color = new Color(0.03f, 0.03f, 0.06f, 0.82f);
                messageRect.offsetMin = new Vector2(30f, 14f);
                messageRect.offsetMax = new Vector2(-30f, -14f);
                messageText.fontSize = 32f;
                messageText.alignment = TextAlignmentOptions.Center;
                messageText.text = shot.subtitleText;
                break;

            case CutsceneBoxType.Player:
                panelRect.anchoredPosition = PlayerBoxPosition;
                panelRect.sizeDelta = DialogueBoxSize;
                panelImage.color = PlayerPanelColor;
                messageRect.offsetMin = new Vector2(36f, 22f);
                messageRect.offsetMax = new Vector2(-36f, -22f);
                messageText.fontSize = 31f;
                messageText.alignment = TextAlignmentOptions.MidlineLeft;
                messageText.text = shot.subtitleText;
                break;

            case CutsceneBoxType.Voice:
            default:
                panelRect.anchoredPosition = VoiceBoxPosition;
                panelRect.sizeDelta = DialogueBoxSize;
                panelImage.color = VoicePanelColor;
                speakerText.enabled = shot.showSpeakerName && !string.IsNullOrWhiteSpace(ResolveSpeakerDisplayName(shot));
                speakerText.text = ResolveSpeakerDisplayName(shot);
                speakerRect.anchoredPosition = new Vector2(34f, -16f);
                speakerRect.sizeDelta = new Vector2(420f, 38f);
                messageRect.offsetMin = new Vector2(34f, 22f);
                messageRect.offsetMax = speakerText.enabled ? new Vector2(-34f, -48f) : new Vector2(-34f, -22f);
                messageText.fontSize = 31f;
                messageText.alignment = TextAlignmentOptions.MidlineLeft;
                messageText.text = shot.subtitleText;
                break;
        }
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
                if (textboxManager.text_player == null || textboxManager.box_player == null)
                {
                    return false;
                }

                textboxManager.text_player.text = shot.subtitleText;
                textboxManager.box_player.SetActive(true);
                return true;

            case CutsceneBoxType.Voice:
                if (textboxManager.text_voice == null || textboxManager.box_voice == null || textboxManager.voice_Name == null)
                {
                    return false;
                }

                textboxManager.text_voice.text = shot.subtitleText;
                textboxManager.voice_Name.text = ResolveSpeakerDisplayName(shot);
                textboxManager.voice_Name.color = ResolveSpeakerColor(shot);
                textboxManager.box_voice.SetActive(true);
                return true;

            default:
                if (textboxManager.text_system == null || textboxManager.box_system == null)
                {
                    return false;
                }

                textboxManager.text_system.text = shot.subtitleText;
                textboxManager.box_system.SetActive(true);
                return true;
        }
    }

    private bool HideCutsceneCanvas()
    {
        if (!HasBoundCutsceneCanvas())
        {
            return false;
        }

        if (textboxManager.box_system != null)
        {
            textboxManager.box_system.SetActive(false);
        }

        if (textboxManager.box_player != null)
        {
            textboxManager.box_player.SetActive(false);
        }

        if (textboxManager.box_voice != null)
        {
            textboxManager.box_voice.SetActive(false);
        }

        return true;
    }

    private bool HasBoundCutsceneCanvas()
    {
        if (textboxManager == null && !forceStandaloneOverlay)
        {
            textboxManager = GetComponentInParent<TextboxManager>();
        }

        return textboxManager != null &&
               textboxManager.box_system != null &&
               textboxManager.box_player != null &&
               textboxManager.box_voice != null &&
               textboxManager.text_system != null &&
               textboxManager.text_player != null &&
               textboxManager.text_voice != null &&
               textboxManager.voice_Name != null;
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
            return "Leon";
        }

        if (speakerType.Contains("painter"))
        {
            return "Elio";
        }

        if (speakerType.Contains("girl"))
        {
            return "Luna";
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

        if (speakerType.Contains("musician") || speakerName.Contains("leon"))
        {
            return new Color(0.48f, 0.76f, 1f, 1f);
        }

        if (speakerType.Contains("painter") || speakerName.Contains("elio"))
        {
            return new Color(0.72f, 1f, 0.62f, 1f);
        }

        if (speakerType.Contains("girl") || speakerName.Contains("luna"))
        {
            return new Color(1f, 0.68f, 0.78f, 1f);
        }

        if (speakerType.Contains("core") || speakerName == "???")
        {
            return new Color(0.86f, 0.86f, 0.9f, 1f);
        }

        return Color.white;
    }
}
