using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class OpeningManager : MonoBehaviour
{
    public static OpeningManager Instance;
    private AudioSource source;
    public AudioClip openClip;
    public Volume pp_Volume;
    public VolumeProfile pp_Volume_black;
    public VolumeProfile pp_Volume_flash;
    public VolumeProfile pp_Volume_gray;
    private readonly string playerTag = "Player";
    private PlayerInput user;
    private PlayerMovement userMove;
    public Light midFlash;
    public Image blackboard;
    private WaitForSecondsRealtime oneSec;
    private WaitForSecondsRealtime halfSec;
    public RawImage openLight;
    public Text openingText;
    public Text talkText;
    public CutsceneImagePlayer OpeningMidCutscene;
    public GameObject openingMidGate;
    private bool isMidtalkOn = false;
    public CutsceneImagePlayer OpeningEndCutscene;
    private void Awake()
    {
        source = GetComponent<AudioSource>();
        user = GameObject.FindGameObjectWithTag(playerTag).GetComponent<PlayerInput>();
        userMove = GameObject.FindGameObjectWithTag(playerTag).GetComponent<PlayerMovement>();
        oneSec = new WaitForSecondsRealtime(1f);
        halfSec = new WaitForSecondsRealtime(0.5f);
        midFlash.enabled = false;
        blackboard.enabled = true;
        openLight.enabled = false;
        openingText.enabled = false;
        talkText.enabled = false;
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
        yield return new WaitForSecondsRealtime(2f);
        source.PlayOneShot(openClip,0.9f);
        yield return new WaitForSecondsRealtime(2f);
        openLight.enabled = true;
        yield return oneSec;
        openLight.enabled = false;
        yield return oneSec;
        StartCoroutine(TalkSay(openingText, "누구였더라"));
        yield return oneSec;
        StartCoroutine(TalkSay(openingText, "기억나지 않아"));
        yield return oneSec;
        StartCoroutine(TalkSay(openingText, "사라지기 전에"));
        yield return oneSec;
        StartCoroutine(TalkSay(openingText, "찾아야 해"));
        yield return oneSec;
        blackboard.enabled = false;
        yield return oneSec;
        StartCoroutine(TalkSay(openingText, "당신은 어디에 있는가"));
        yield return oneSec;
        StartCoroutine(TalkSay(openingText, "당신은 누구였는가"));
        yield return oneSec;
        StartCoroutine(TalkSay(openingText, "아무것도 선명하지 않다"));
        UserCtrl(true);
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
        midFlash.enabled = true;
        yield return new WaitForSecondsRealtime(0.3f);
        pp_Volume.profile = pp_Volume_black;
        midFlash.enabled = false;
        OpeningMidCutscene.PlayCutscene();
        yield return new WaitForSecondsRealtime(3f);
        StartCoroutine(TalkSay(talkText, "잊은 게 아니야"));
        yield return oneSec;
        StartCoroutine(TalkSay(talkText, "흩어진 거야"));
        user.isLookLock = false;
        yield return oneSec;
        StartCoroutine(TalkSay(talkText, "사라지기 전에...\n이어 붙여"));
        UserCtrl(true);
        StartCoroutine(TalkSay(openingText, "첫번째 이야기의 조각이 반응한다"));
        yield return oneSec;
        openingMidGate.SetActive(true);
        pp_Volume.profile = pp_Volume_gray;
        StartCoroutine(TalkSay(openingText, "문이 열리고 있다."));
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
        StartCoroutine(TalkSay(openingText,"가장 먼저 남아 있던 것은 추억이었다"));
        yield return oneSec;
        StartCoroutine(TalkSay(openingText, "첫번째 기억이 당신을 부르고 있다"));
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
    private IEnumerator TalkSay(Text txt, string say)
    {
        txt.text = say;
        txt.enabled = true;
        yield return oneSec;
        txt.enabled = false;
    }
    private IEnumerator ImageFlash(Image img,float time)
    {
        img.enabled = true;
        yield return new WaitForSecondsRealtime(time);
        img.enabled = false;
    }
}