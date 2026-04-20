using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static Cinemachine.CinemachineTrackedDolly;
public class TextboxManager : MonoBehaviour
{
    private readonly string playerTag = "Player";
    private static readonly Vector2 SystemTextSize = new(1180f, 120f);
    private static readonly Vector2 VoiceNameSize = new(260f, 44f);
    private const float SystemTextOffsetY = -24f;
    public PlayerInput user;
    public PlayerMovement userMove;
    public enum TalkType { system, player, voice}
    public enum Talker { self, girl, painter, musician, core };
    public GameObject box_system;
    public GameObject box_player;
    public GameObject box_voice;
    public Text text_system;
    public Text text_player;
    public Text text_voice;
    public Text voice_Name;
    public GameObject nextBtn;
    public GameObject skipBtn;
    private bool nextPressed = false;
    public WaitForSecondsRealtime oneSec = new(1f);
    int curTalkID = 0;  //대사 스킵 시 현재 대사만 스킵하고 다음 대사로 넘어가기 위해 각 대사에 현재 대사 ID를 부여
    void Awake()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            user = player.GetComponent<PlayerInput>();
            if (user == null) Debug.LogWarning("PlayerInput 컴포넌트를 찾을 수 없습니다.");
            userMove = player.GetComponent<PlayerMovement>();
            if (userMove == null) Debug.LogWarning("PlayerMovement 컴포넌트를 찾을 수 없습니다.");
        }
        else
        {
            Debug.Log("Player 오브젝트가 존재하지 않는 신입니다.");
        }
        ApplyTextboxLayout();
        CloseBox();
    }

    public void UserCtrl(bool b)  //유저 입력 적용 여부 컨트롤
    {
        if (user != null) user.enabled = b;
        if (userMove != null)
        {
            userMove.enabled = b;
            userMove.SetMoveLock(!b);
        }
    }
    public void OnNextButton() => nextPressed = true;
    public IEnumerator TalkSay(TalkType type, string say, float time = 1f, Talker talk = Talker.self, bool canSkip = false)
    {
        int talkID = ++curTalkID;
        nextPressed = false;
        if(nextBtn != null) nextBtn.SetActive(canSkip);
        switch (talk)
        {
            case Talker.girl:       voice_Name.text = "luna";   voice_Name.color = Color.red;   break;
            case Talker.painter:    voice_Name.text = "elio";   voice_Name.color = Color.green; break;
            case Talker.musician:   voice_Name.text = "leon";   voice_Name.color = Color.blue;  break;
            case Talker.core:       voice_Name.text = "???";    voice_Name.color = Color.gray;  break;
            case Talker.self:       voice_Name.text = "YOU";    voice_Name.color = Color.black; break;
        }
        switch (type)
        {
            case TalkType.system: text_system.text = say; box_system.SetActive(true); break;
            case TalkType.player: text_player.text = say; box_player.SetActive(true); break;
            case TalkType.voice:  text_voice.text = say;  box_voice.SetActive(true);  break;
        }
        bool noAuto = (time <= 0f);  //수동 넘기기만 사용할 대사에는 time 값을 0 또는 음수(-)로 입력
        if (noAuto)
        {
            if (nextBtn != null) nextBtn.SetActive(true);
            while (!(nextPressed && talkID == curTalkID)) yield return null;
        }
        else
        {
            float timer = 0f;
            while (timer < time)  //time 시간 이내에 반복
            {
                if (canSkip && nextPressed && talkID == curTalkID) break;  // 스킵 가능 & 버튼 눌렀을 시 현재 컷신 텍스트를 종료
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
        }
        CloseBox();
    }
    public void CloseBox()
    {
        box_system.SetActive(false);
        box_player.SetActive(false);
        box_voice.SetActive(false);
        if (nextBtn != null) nextBtn.SetActive(false);
    }
    private void ApplyTextboxLayout()
    {
        ConfigureSystemText();
        ConfigureDialogueText(text_player, new Vector4(56f, 16f, 56f, 32f), TextAnchor.MiddleLeft, 40, 24);
        ConfigureDialogueText(text_voice, new Vector4(52f, 16f, 52f, 76f), TextAnchor.MiddleLeft, 40, 22);
        ConfigureVoiceNameText();
    }

    private void ConfigureSystemText()
    {
        if (text_system == null)
        {
            return;
        }

        RectTransform textRect = text_system.rectTransform;
        if (textRect != null)
        {
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = new Vector2(0f, SystemTextOffsetY);
            textRect.sizeDelta = SystemTextSize;
        }

        ConfigureTextStyle(text_system, TextAnchor.MiddleCenter, 40, 24);
    }

    private void ConfigureDialogueText(Text target, Vector4 padding, TextAnchor alignment, int maxFontSize, int minFontSize)
    {
        if (target == null)
        {
            return;
        }

        RectTransform textRect = target.rectTransform;
        if (textRect != null)
        {
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.offsetMin = new Vector2(padding.x, padding.y);
            textRect.offsetMax = new Vector2(-padding.z, -padding.w);
        }

        ConfigureTextStyle(target, alignment, maxFontSize, minFontSize);
    }

    private void ConfigureVoiceNameText()
    {
        if (voice_Name == null)
        {
            return;
        }

        RectTransform nameRect = voice_Name.rectTransform;
        if (nameRect != null)
        {
            nameRect.anchorMin = new Vector2(0f, 1f);
            nameRect.anchorMax = new Vector2(0f, 1f);
            nameRect.pivot = new Vector2(0f, 1f);
            nameRect.anchoredPosition = new Vector2(30f, -16f);
            nameRect.sizeDelta = VoiceNameSize;
        }

        ConfigureTextStyle(voice_Name, TextAnchor.MiddleLeft, 44, 18);
    }

    private static void ConfigureTextStyle(Text target, TextAnchor alignment, int maxFontSize, int minFontSize)
    {
        if (target == null)
        {
            return;
        }

        target.alignment = alignment;
        target.horizontalOverflow = HorizontalWrapMode.Wrap;
        target.verticalOverflow = VerticalWrapMode.Overflow;
        target.resizeTextForBestFit = true;
        target.resizeTextMaxSize = Mathf.Max(target.fontSize, maxFontSize);
        target.resizeTextMinSize = Mathf.Clamp(minFontSize, 1, target.resizeTextMaxSize);
        target.supportRichText = true;
        target.alignByGeometry = false;
    }
}
