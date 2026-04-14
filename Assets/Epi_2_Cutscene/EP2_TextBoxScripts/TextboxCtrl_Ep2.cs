using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using static TextboxManager;

public class TextboxCtrl_Ep2 : MonoBehaviour
{
    public TextboxManager _manager;
    private PlayerInput user;

    public WaitForSecondsRealtime oneSec = new(1f);
    public WaitForSecondsRealtime onehalfSec = new(1.5f);
    public WaitForSecondsRealtime twoSec = new(2f);

    private bool introPlayed = false;

    void Awake()
    {
        user = _manager.user;
    }

    // ===============================
    // 🎬 Episode2 시작 컷씬
    // ===============================
    public void Episode2Start()
    {
        StartCoroutine(Episode2Intro());
    }

    IEnumerator Episode2Intro()
    {
        if (introPlayed) yield break;
        introPlayed = true;

        _manager.UserCtrl(false);

        yield return StartCoroutine(_manager.TalkSay(TalkType.system,
            "이곳은 기억과 조각들이 뒤섞인 공간이다."));

        yield return oneSec;

        yield return StartCoroutine(_manager.TalkSay(TalkType.player,
            "여긴... 어디지?"));

        yield return oneSec;

        yield return StartCoroutine(_manager.TalkSay(TalkType.voice,
            "흩어진 조각들을 찾아야 해.", Talker.core));

        _manager.UserCtrl(true);
    }

    // ===============================
    // 🌌 Space 퍼즐
    // ===============================

    public IEnumerator SpacePuzzleStart()
    {
        _manager.UserCtrl(false);

        yield return StartCoroutine(_manager.TalkSay(TalkType.player,
            "이 공간... 낯설지 않아."));

        yield return oneSec;

        yield return StartCoroutine(_manager.TalkSay(TalkType.voice,
            "기억은 항상 네 안에 있었어.", Talker.core));

        _manager.UserCtrl(true);
    }

    public IEnumerator SpacePuzzleComplete()
    {
        _manager.UserCtrl(false);

        yield return StartCoroutine(_manager.TalkSay(TalkType.player,
            "조각이... 맞춰지고 있어."));

        yield return oneSec;

        yield return StartCoroutine(_manager.TalkSay(TalkType.voice,
            "첫 번째 기억이 돌아왔어.", Talker.core));

        _manager.UserCtrl(true);
    }

    // ===============================
    // 🎨 Paint 퍼즐
    // ===============================

    public IEnumerator PaintPuzzleStart()
    {
        _manager.UserCtrl(false);

        yield return StartCoroutine(_manager.TalkSay(TalkType.player,
            "이 그림... 어딘가 익숙한데"));

        yield return oneSec;

        yield return StartCoroutine(_manager.TalkSay(TalkType.voice,
            "네가 잊고 있던 장면이야.", Talker.core));

        _manager.UserCtrl(true);
    }

    public IEnumerator PaintPuzzleComplete()
    {
        _manager.UserCtrl(false);

        yield return StartCoroutine(_manager.TalkSay(TalkType.player,
            "이건... 내가 잃어버린 기억이야"));

        yield return oneSec;

        yield return StartCoroutine(_manager.TalkSay(TalkType.voice,
            "이제 거의 다 왔어.", Talker.core));

        _manager.UserCtrl(true);
    }

    // ===============================
    // 🏁 엔딩 컷씬
    // ===============================

    public void Episode2Ending()
    {
        StartCoroutine(EndingSequence());
    }

    IEnumerator EndingSequence()
    {
        _manager.UserCtrl(false);

        yield return StartCoroutine(_manager.TalkSay(TalkType.voice,
            "이제 모든 조각이 돌아왔어.", Talker.core));

        yield return onehalfSec;

        yield return StartCoroutine(_manager.TalkSay(TalkType.player,
            "이게... 나였던 거구나."));

        yield return twoSec;

        SceneManager.LoadScene("NextScene"); // 필요시 변경

        _manager.UserCtrl(true);
    }
}