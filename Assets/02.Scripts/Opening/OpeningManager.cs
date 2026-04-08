using Cinemachine;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class OpeningManager : MonoBehaviour
{
    public static OpeningManager Instance;
    public Volume pp_Volume;
    public VolumeProfile pp_Volume_black;
    public VolumeProfile pp_Volume_flash;
    private readonly string playerTag = "Player";
    private PlayerInput user;
    private PlayerMovement userMove;
    public Image blackboard;
    private WaitForSecondsRealtime twoSec;
    private WaitForSecondsRealtime oneSec;
    private WaitForSecondsRealtime halfSec;
    public RawImage openLight;
    private enum BoxType { system, talk, voice };
    public GameObject systemBox;
    public Text systemText;
    public GameObject talkBox;
    public Text talkText;
    public GameObject voiceBox;
    public Text voiceText;
    public CutsceneImagePlayer OpeningMidCutscene;
    public GameObject openingMidGate;
    private bool isMidtalkOn = false;
    public CutsceneImagePlayer OpeningEndCutscene;
    private void Awake()
    {
        user = GameObject.FindGameObjectWithTag(playerTag).GetComponent<PlayerInput>();
        userMove = GameObject.FindGameObjectWithTag(playerTag).GetComponent<PlayerMovement>();
        twoSec = new WaitForSecondsRealtime(2f);
        oneSec = new WaitForSecondsRealtime(1f);
        halfSec = new WaitForSecondsRealtime(0.5f);
        blackboard.enabled = true;
        openLight.enabled = false;
        systemBox.SetActive(false);
        talkBox.SetActive(false);
        voiceBox.SetActive(false);
        openingMidGate.SetActive(false);
        isMidtalkOn = false;
    }
    void OnEnable()
    {
        InteractManager.Instance.OpeningMid += OpeningMid;
        InteractManager.Instance.OpeningGoal += OpeningEnd;
    }
    void Start()
    {
        user.isJumpLock = true;
        user.isLookLock = true;
        UserCtrl(false);
        pp_Volume.profile = pp_Volume_black;
        StartCoroutine(DarkEnter());
    }
    private void FixedUpdate()
    {
        user.isSprint = false;  //달리기 불가
    }
    void OnDisable()
    {
        InteractManager.Instance.OpeningMid -= OpeningMid;
        InteractManager.Instance.OpeningGoal -= OpeningEnd;
    }
    IEnumerator DarkEnter()
    {
        yield return twoSec;
        blackboard.enabled = false;
        StartCoroutine(TalkSay(BoxType.system, "당신은 어디에 있는가"));
        yield return oneSec;
        StartCoroutine(TalkSay(BoxType.talk, "찾아야 해"));
        yield return oneSec;
        StartCoroutine(TalkSay(BoxType.talk, "사라지기 전에"));
        yield return oneSec;
        StartCoroutine(TalkSay(BoxType.system, "당신은 누구였는가"));
        yield return oneSec;
        StartCoroutine(TalkSay(BoxType.talk, "내가... 왜 여기에 있지?"));
        UserCtrl(true);
        StartCoroutine(TalkSay(BoxType.system, "아무것도 선명하지 않다"));
    }
    public void OpeningMid()
    {
        StartCoroutine(MidVoice());
    }
    public IEnumerator MidVoice()
    {
        if (isMidtalkOn) yield break;
        isMidtalkOn = true;
        UserCtrl(false);
        pp_Volume.profile = pp_Volume_flash;
        yield return new WaitForSecondsRealtime(0.2f);
        pp_Volume.profile = pp_Volume_black;
        yield return new WaitForSecondsRealtime(0.2f);
        OpeningMidCutscene.PlayCutscene();
        yield return new WaitForSecondsRealtime(3f);
        StartCoroutine(TalkSay(BoxType.voice, "흩어진 거야"));
        yield return twoSec;
        StartCoroutine(TalkSay(BoxType.voice, "더 늦기 전에 찾아야 해."));
        yield return oneSec;
        StartCoroutine(TalkSay(BoxType.voice, "길을 따라가."));
        openingMidGate.SetActive(true);
        user.isLookLock = false;
        yield return halfSec;
        StartCoroutine(TalkSay(BoxType.system, "길의 끝에 문이 모습을 드러낸다."));
        yield return oneSec;
        StartCoroutine(TalkSay(BoxType.voice, "꼭 찾아야 해."));
        yield return oneSec;
        StartCoroutine(TalkSay(BoxType.talk, "너는 누구야?"));
        UserCtrl(true);
        yield return twoSec;
        StartCoroutine(TalkSay(BoxType.voice, "문 너머로 가면 알 수 있을 거야."));
        yield return halfSec;
        StartCoroutine(TalkSay(BoxType.system, "희미한 빛이 다음 기억으로 이어진다."));
    }
    public void OpeningEnd()
    {
        StartCoroutine(EnterLobby());
    }
    public IEnumerator EnterLobby()
    {
        UserCtrl(false);
        OpeningEndCutscene.PlayCutscene();
        yield return new WaitForSecondsRealtime(2f);
        StartCoroutine(TalkSay(BoxType.talk, "가장 먼저 남아 있던 것은 추억이었다"));
        yield return halfSec;
        StartCoroutine(TalkSay(BoxType.system, "첫 번째 기억으로 향하는 길이 열린다."));
        user.isJumpLock = false;
        if (SaveManager.instance != null) SaveManager.instance.curData.ep1_open = true;  //Start 신을 거치지 않은 경우 SaveManager가 null이므로 유효성 체크
        yield return halfSec;
        SceneManager.LoadScene("Episode1_Scene");
    }
    private void UserCtrl(bool b)  //유저 입력 적용 여부 컨트롤
    {
        user.enabled = b;
        userMove.enabled = b;
        userMove.SetMoveLock(!b);
    }
    private IEnumerator TalkSay(BoxType box, string say)
    {
        Text txt = box switch
        {
            BoxType.system => systemText,
            BoxType.talk => talkText,
            BoxType.voice => voiceText,
            _ => throw new ArgumentOutOfRangeException()
        };
        GameObject obj = box switch
        {
            BoxType.system => systemBox,
            BoxType.talk => talkBox,
            BoxType.voice => voiceBox,
            _ => throw new ArgumentOutOfRangeException()
        };
        txt.text = say;
        txt.enabled = true;
        obj.SetActive(true);
        yield return oneSec;
        txt.enabled = false;
        obj.SetActive(false);
    }
}