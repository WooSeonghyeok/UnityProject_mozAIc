using System.Collections;
using UnityEngine;
using static TextboxManager;

public class TextboxCtrl_Ending : MonoBehaviour
{
    public TextboxManager _manager;
    private EndingManager ending;
    public WaitForSecondsRealtime oneSec = new(1f);
    public WaitForSecondsRealtime twoSec = new(2f);
    private void Start()
    {
        _manager.skipBtn.SetActive(false);
        ending = GetComponent<EndingManager>();
    }
    public IEnumerator TrueEndCutscene()
    {
        yield return twoSec;
        yield return _manager.TalkSay(TalkType.voice, "괜찮아. 이젠 전부 네 것이니까.", 2.5f, Talker.self);
        yield return oneSec;
        yield return _manager.TalkSay(TalkType.player, "기억은 사라진 게 아니었다.", 2.5f);
        yield return oneSec;
        yield return _manager.TalkSay(TalkType.player, "나는 그것을 견딜 수 있을 만큼 다시 돌아온 거다.", 3f);
        yield return twoSec;
        ending.EndingClear();
    }
    public IEnumerator NormalEndCutscene()
    {
        yield return twoSec;
        yield return _manager.TalkSay(TalkType.voice, "괜찮아. 다 돌아오지 못했어도, 넌 여기까지 왔어.", 2f, Talker.self);
        yield return oneSec;
        yield return _manager.TalkSay(TalkType.player, "완전하지 않아도… 돌아올 수는 있다.", 2f);
        yield return oneSec;
        yield return _manager.TalkSay(TalkType.player, "아직 남은 빈칸이 있더라도, 그 역시 내 일부다.", 2f);
        yield return twoSec;
        ending.EndingClear();
    }
}
