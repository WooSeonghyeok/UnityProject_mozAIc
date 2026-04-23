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
    private WaitForSecondsRealtime oneSec = new(1f);
    public CutsceneImagePlayer OpeningMidCutscene;
    public GameObject openingMidGate;
    private bool isMidtalkOn = false;
    public CutsceneImagePlayer OpeningEndCutscene;
    private void Awake()
    {
        user = _manager.user;
        blackboard.enabled = true;
        openingMidGate.SetActive(false);
        isMidtalkOn = false;
    }
    void Start()
    {
        user.isJumpLock = true;
        user.isLookLock = true;
        _manager.UserCtrl(false);
        pp_Volume.profile = pp_Volume_black;
        StartCoroutine(DarkEnter());
        InteractManager.Instance.OpeningMid += OpeningMid;
        InteractManager.Instance.OpeningGoal += OpeningEnd;
    }
    private void FixedUpdate()
    {
        user.isSprint = false;  //달리기 불가
    }
    IEnumerator DarkEnter()
    {
        GameManager.Instance.CutsceneMode(true);
        yield return oneSec;
        blackboard.enabled = false;
        yield return _manager.TalkSay(TalkType.system, "아무것도 완전히 존재하지 않는 공간.\n어두운 허공 사이로, 겨우 한 줄기의 길만이 이어져 있다.", 2f);
        yield return oneSec;
        yield return _manager.TalkSay(TalkType.system, "길 주변에는 무언가의 조각들이 떠다닌다.\n모든 것이 흩어진 채 멈춰 있다.", 2f);
        yield return oneSec;
        yield return _manager.TalkSay(TalkType.player, "여기는 어디지..?", 2f, Talker.self, true);
        yield return oneSec;
        yield return _manager.TalkSay(TalkType.player, "내가... 왜 여기에 있지?", 2f, Talker.self, true);
        yield return oneSec;
        _manager.UserCtrl(true);
        yield return _manager.TalkSay(TalkType.player, "일단 앞으로 나아가봐야겠다", 2f);
    }
    public void OpeningMid()
    {
        Debug.Log("OpeningMid() CALLED");
        StartCoroutine(MidVoice());
    }
    public IEnumerator MidVoice()
    {
        if (isMidtalkOn) yield break;
        isMidtalkOn = true;
        _manager.UserCtrl(false);
        pp_Volume.profile = pp_Volume_flash;
        yield return new WaitForSecondsRealtime(0.1f);
        OpeningMidCutscene.PlayCutscene(false);
        yield return oneSec;
        yield return _manager.TalkSay(TalkType.voice, "늦기 전에 찾아야 해.", 2f, Talker.core);
        pp_Volume.profile = pp_Volume_black;
        yield return oneSec;
        yield return _manager.TalkSay(TalkType.player, "...?", 2f);
        yield return _manager.TalkSay(TalkType.player, "무슨 소리지?", 2f);
        yield return oneSec;
        openingMidGate.SetActive(true);
        yield return _manager.TalkSay(TalkType.voice, "꼭 찾아야 해.", 2f, Talker.core, true);
        GameManager.Instance.lookLock = false;
        user.isLookLock = false;
        _manager.UserCtrl(true);
        yield return _manager.TalkSay(TalkType.player, "너는 누구야?", 2f);
        yield return _manager.TalkSay(TalkType.player, "뭘, 뭘 찾아야 하는데?", 2f);
        yield return oneSec;
        StartCoroutine(_manager.TalkSay(TalkType.voice, "잊으면 안돼.", 2f, Talker.core));
    }
    public void OpeningEnd() => StartCoroutine(EnterLobby());
    public IEnumerator EnterLobby()
    {
        _manager.UserCtrl(false);
        OpeningEndCutscene.PlayCutscene(false);
        yield return oneSec;
        yield return _manager.TalkSay(TalkType.system, "어딘가로 향하는 길이 열렸다.", 2f);
        yield return oneSec;
        yield return _manager.TalkSay(TalkType.system, "이유를 알 수 없지만 왜인지.", 2f);
        yield return oneSec;
        yield return _manager.TalkSay(TalkType.system, "잊으면 안 된다는 말이… 발걸음을 움직이게 했다.", 2f);
        user.isJumpLock = false;
        if (SaveManager.instance != null) SaveManager.instance.curData.ep1_open = true;  //Start 신을 거치지 않은 경우 SaveManager가 null이므로 유효성 체크
        GameManager.Instance.isCutsceneMode = false;
        SceneManager.LoadScene("Episode1_Scene");
    }
}