using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Ep3CutsceneSubtitlePresenter : MonoBehaviour
{
    private static readonly Vector2 DialogueBoxSize = new Vector2(1000f, 200f);
    private static readonly Vector2 SystemBoxSize = new Vector2(1000f, 200f);
    private static readonly Vector2 SystemTextSize = new Vector2(1180f, 120f);
    private static readonly Vector2 VoiceNameSize = new Vector2(260f, 44f);
    private static readonly Vector4 PlayerPadding = new Vector4(56f, 16f, 56f, 32f);
    private static readonly Vector4 VoicePaddingWithName = new Vector4(52f, 16f, 52f, 76f);
    private static readonly Vector4 VoicePaddingNoName = new Vector4(52f, 16f, 52f, 22f);
    private static readonly Vector2 SystemBoxPosition = new Vector2(0f, 200f);
    private static readonly Vector2 PlayerBoxPosition = new Vector2(0f, 0f);
    private static readonly Vector2 VoiceBoxPosition = new Vector2(0f, -300f);
    private static readonly Color PlayerPanelColor = new Color(0.25f, 0.25f, 0.25f, 0.75f);
    private static readonly Color VoicePanelColor = new Color(0.15f, 0.15f, 0.18f, 0.78f);
    private static readonly Color DialogueTextColor = new Color(0.75f, 0.88f, 1f, 1f);

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
    private RectTransform panelRect;
    private TextMeshProUGUI messageText;
    private RectTransform messageRect;
    private TextMeshProUGUI speakerNameText;

    public void Configure(TMP_FontAsset fontAsset, TextboxManager existingTextboxManager = null, bool forceStandaloneOverlay = false)
    {
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

        ApplyStandaloneStyle(shot);

        canvasGroup.alpha = 1f;
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

        if (panelImage != null)
        {
            panelImage.enabled = false;
        }

        if (messageText != null)
        {
            messageText.text = string.Empty;
            messageText.enabled = false;
        }

        if (speakerNameText != null)
        {
            speakerNameText.text = string.Empty;
            speakerNameText.enabled = false;
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

            if (fontAsset != null && speakerNameText != null)
            {
                speakerNameText.font = fontAsset;
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
        panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = VoiceBoxPosition;
        panelRect.sizeDelta = DialogueBoxSize;

        panelImage = panelObject.GetComponent<Image>();
        panelImage.color = VoicePanelColor;
        panelImage.enabled = false;

        GameObject speakerObject = new GameObject("SpeakerName", typeof(RectTransform), typeof(TextMeshProUGUI));
        speakerObject.transform.SetParent(panelRect, false);
        RectTransform speakerRect = speakerObject.GetComponent<RectTransform>();
        speakerRect.anchorMin = new Vector2(0f, 1f);
        speakerRect.anchorMax = new Vector2(0f, 1f);
        speakerRect.pivot = new Vector2(0f, 1f);
        speakerRect.anchoredPosition = new Vector2(30f, -16f);
        speakerRect.sizeDelta = VoiceNameSize;

        speakerNameText = speakerObject.GetComponent<TextMeshProUGUI>();
        speakerNameText.font = fontAsset != null ? fontAsset : TMP_Settings.defaultFontAsset;
        speakerNameText.fontSize = 30f;
        speakerNameText.color = Color.white;
        speakerNameText.enableWordWrapping = false;
        speakerNameText.overflowMode = TextOverflowModes.Ellipsis;
        speakerNameText.alignment = TextAlignmentOptions.MidlineLeft;
        speakerNameText.enabled = false;

        GameObject textObject = new GameObject("Message", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(panelRect, false);
        messageRect = textObject.GetComponent<RectTransform>();
        messageRect.anchorMin = Vector2.zero;
        messageRect.anchorMax = Vector2.one;
        messageRect.offsetMin = new Vector2(PlayerPadding.x, PlayerPadding.y);
        messageRect.offsetMax = new Vector2(-PlayerPadding.z, -PlayerPadding.w);

        messageText = textObject.GetComponent<TextMeshProUGUI>();
        messageText.font = fontAsset != null ? fontAsset : TMP_Settings.defaultFontAsset;
        messageText.fontSize = 40f;
        messageText.color = Color.white;
        messageText.enableWordWrapping = true;
        messageText.overflowMode = TextOverflowModes.Overflow;
        messageText.alignment = TextAlignmentOptions.MidlineLeft;
        messageText.enabled = false;
    }

    private void ApplyStandaloneStyle(Ep3LobbyIntroShotData shot)
    {
        CutsceneBoxType boxType = ResolveCutsceneBoxType(shot);
        bool showSpeakerName = shot.showSpeakerName && !string.IsNullOrWhiteSpace(shot.speakerName);

        if (panelRect == null || panelImage == null || messageText == null || messageRect == null)
        {
            return;
        }

        panelImage.enabled = boxType != CutsceneBoxType.System;
        if (speakerNameText != null)
        {
            speakerNameText.enabled = false;
            speakerNameText.text = string.Empty;
        }

        switch (boxType)
        {
            case CutsceneBoxType.Player:
                panelRect.anchoredPosition = PlayerBoxPosition;
                panelRect.sizeDelta = DialogueBoxSize;
                panelImage.color = PlayerPanelColor;
                messageRect.anchorMin = Vector2.zero;
                messageRect.anchorMax = Vector2.one;
                messageRect.pivot = new Vector2(0.5f, 0.5f);
                messageRect.anchoredPosition = Vector2.zero;
                messageRect.sizeDelta = Vector2.zero;
                messageRect.offsetMin = new Vector2(PlayerPadding.x, PlayerPadding.y);
                messageRect.offsetMax = new Vector2(-PlayerPadding.z, -PlayerPadding.w);
                messageText.fontSize = 40f;
                messageText.color = DialogueTextColor;
                messageText.alignment = TextAlignmentOptions.MidlineLeft;
                messageText.text = shot.subtitleText;
                break;

            case CutsceneBoxType.Voice:
                panelRect.anchoredPosition = VoiceBoxPosition;
                panelRect.sizeDelta = DialogueBoxSize;
                panelImage.color = VoicePanelColor;
                messageRect.anchorMin = Vector2.zero;
                messageRect.anchorMax = Vector2.one;
                messageRect.pivot = new Vector2(0.5f, 0.5f);
                messageRect.anchoredPosition = Vector2.zero;
                messageRect.sizeDelta = Vector2.zero;
                Vector4 voicePadding = showSpeakerName ? VoicePaddingWithName : VoicePaddingNoName;
                messageRect.offsetMin = new Vector2(voicePadding.x, voicePadding.y);
                messageRect.offsetMax = new Vector2(-voicePadding.z, -voicePadding.w);
                messageText.fontSize = 40f;
                messageText.color = DialogueTextColor;
                messageText.alignment = showSpeakerName ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.MidlineLeft;
                messageText.text = shot.subtitleText;

                if (speakerNameText != null && showSpeakerName)
                {
                    speakerNameText.text = ResolveSpeakerDisplayName(shot);
                    speakerNameText.color = ResolveSpeakerColor(shot);
                    speakerNameText.enabled = true;
                }
                break;

            default:
                panelRect.anchoredPosition = SystemBoxPosition;
                panelRect.sizeDelta = SystemBoxSize;
                messageRect.anchorMin = new Vector2(0.5f, 0.5f);
                messageRect.anchorMax = new Vector2(0.5f, 0.5f);
                messageRect.pivot = new Vector2(0.5f, 0.5f);
                messageRect.anchoredPosition = new Vector2(0f, -24f);
                messageRect.sizeDelta = SystemTextSize;
                messageText.fontSize = 40f;
                messageText.color = Color.white;
                messageText.alignment = TextAlignmentOptions.Midline;
                messageText.text = shot.subtitleText;
                break;
        }

        messageText.enabled = true;
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
        if (textboxManager == null)
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
