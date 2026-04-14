using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static TextboxManager;
public class TextboxCtrl_Opening : MonoBehaviour
{
    public static TextboxCtrl_Opening Instance;
    public TextboxManager _manager;
    public Volume pp_Volume;
    public VolumeProfile pp_Volume_black;
    public VolumeProfile pp_Volume_flash;
    private PlayerInput user;
    public Image blackboard;
    private WaitForSecondsRealtime oneSec = new WaitForSecondsRealtime(1f);
    public RawImage openLight;
    public CutsceneImagePlayer OpeningMidCutscene;
    public GameObject openingMidGate;
    private bool isMidtalkOn = false;
    public CutsceneImagePlayer OpeningEndCutscene;
    private void Awake()
    {
        user = _manager.user;
        blackboard.enabled = true;
        openLight.enabled = false;
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
        _manager.UserCtrl(false);
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
        yield return oneSec;
        blackboard.enabled = false;
        yield return StartCoroutine(_manager.TalkSay(TalkType.system, "아무것도 완전히 존재하지 않는 공간.\n어두운 허공 사이로,\n겨우 한 줄기의 길만이 이어져 있다.", 2f));
        yield return StartCoroutine(_manager.TalkSay(TalkType.player, "찾아야 해", 1f, Talker.self, true));
        yield return StartCoroutine(_manager.TalkSay(TalkType.player, "사라지기 전에", 2f, Talker.self, true));
        _manager.UserCtrl(true);
        yield return StartCoroutine(_manager.TalkSay(TalkType.player, "내가... 왜 여기에 있지?"));
    }
    public void OpeningMid()
    {
        StartCoroutine(MidVoice());
    }
    public IEnumerator MidVoice()
    {
        if (isMidtalkOn) yield break;
        isMidtalkOn = true;
        _manager.UserCtrl(false);
        pp_Volume.profile = pp_Volume_flash;
        yield return new WaitForSecondsRealtime(0.1f);
        OpeningMidCutscene.PlayCutscene();
        yield return oneSec;
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "흩어진 거야.", 1f, Talker.core));
        pp_Volume.profile = pp_Volume_black;
        yield return oneSec;
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "더 늦기 전에 찾아야 해.", 1f, Talker.core));
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "길을 따라가.", 2f, Talker.core, true));
        openingMidGate.SetActive(true);
        user.isLookLock = false;
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "꼭 찾아야 해.", 2f, Talker.core, true));
        _manager.UserCtrl(true);
        yield return StartCoroutine(_manager.TalkSay(TalkType.player, "너는 누구야?", 2f));
        StartCoroutine(_manager.TalkSay(TalkType.voice, "문 너머로 가면 알 수 있을 거야.", 1f, Talker.core));
    }
    public void OpeningEnd()
    {
        StartCoroutine(EnterLobby());
    }
    public IEnumerator EnterLobby()
    {
        _manager.UserCtrl(false);
        OpeningEndCutscene.PlayCutscene();
        yield return new WaitForSecondsRealtime(1.5f);
        yield return StartCoroutine(_manager.TalkSay(TalkType.system, "첫 번째 기억으로 향하는 길이 열린다."));
        yield return StartCoroutine(_manager.TalkSay(TalkType.system, "잠시 머뭇거리지만, 결국 걸음을 옮긴다."));
        user.isJumpLock = false;
        if (SaveManager.instance != null) SaveManager.instance.curData.ep1_open = true;  //Start 신을 거치지 않은 경우 SaveManager가 null이므로 유효성 체크
        SceneManager.LoadScene("Episode1_Scene");
    }
}