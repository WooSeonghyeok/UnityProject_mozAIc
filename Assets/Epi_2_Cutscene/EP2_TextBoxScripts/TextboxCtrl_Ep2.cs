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
    public Cinemachine.CinemachineVirtualCamera playerCam;
    public Cinemachine.CinemachineVirtualCamera introCam1;

    private bool introPlayed = false;
    void SwitchToCam(Cinemachine.CinemachineVirtualCamera cam)
    {
        playerCam.Priority = 0;
        cam.Priority = 20;
    }

    void BackToPlayerCam()
    {
        playerCam.Priority = 20;
        introCam1.Priority = 0;
    }
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
        if (PlayerPrefs.GetInt("Played_EP2_Text_Intro", 0) == 1)
            yield break;

        PlayerPrefs.SetInt("Played_EP2_Text_Intro", 1);
        PlayerPrefs.Save();

        if (introPlayed) yield break;
        introPlayed = true;

        _manager.UserCtrl(false);

        yield return _manager.TalkSay(TalkType.system,
            "이곳에는 멈춰 버린 열정이 남아 있다.", 2f);

        yield return _manager.TalkSay(TalkType.system,
            "장면은 보이지만, 아직 색은 살아 있지 않다.", 2f);

        yield return _manager.TalkSay(TalkType.player,
            "여긴... 어디지?", 2f, Talker.self, true);

        // 🎬 여기서 카메라 전환
        SwitchToCam(introCam1);
        yield return new WaitForSeconds(2f);

        yield return _manager.TalkSay(TalkType.voice,
            "…누구지..", 2f, Talker.core, true);

        yield return _manager.TalkSay(TalkType.voice,
            "처음 보는 것 같은데.", 2f, Talker.core, false);

        yield return _manager.TalkSay(TalkType.voice,
            "왜 네가 여기 있는 게 이상하지 않지?", 2f, Talker.core, false);

        // 🎬 여기서 다시 플레이어 카메라
        BackToPlayerCam();

        _manager.UserCtrl(true);
    }

    // ===============================
    // 🌌 Space 퍼즐 시작
    // ===============================
    public IEnumerator SpacePuzzleStart()
    {
        _manager.UserCtrl(false);

        yield return _manager.TalkSay(TalkType.player,
            "여기는 어디지..??", 2f, Talker.self, true);

        yield return _manager.TalkSay(TalkType.voice,
            "너는 옛날부터 다양한 각도로 그림을 바라봤어.", 2f, Talker.core, false);

        yield return _manager.TalkSay(TalkType.voice,
            "네가 구도를 잡을 때 기억을 떠올려봐.", 2f, Talker.core, false);

        _manager.UserCtrl(true);
    }

    // ===============================
    // 🌌 Space 퍼즐 단계별
    // ===============================
    public IEnumerator SpacePuzzleStep1()
    {
        _manager.UserCtrl(false);

        yield return _manager.TalkSay(TalkType.player,
            "방금… 흩어져 있던 것들이 하나로 보였어", 2f, Talker.self, true);

        _manager.UserCtrl(true);
    }

    public IEnumerator SpacePuzzleStep2()
    {
        _manager.UserCtrl(false);

        yield return _manager.TalkSay(TalkType.voice,
            "같은 장면도 어디에서 보느냐에 따라 완전히 달라지지.", 2f, Talker.core, true);

        yield return _manager.TalkSay(TalkType.voice,
            "우린 그런 이야기를 자주 했던 것 같아.", 2f, Talker.core, true);

        _manager.UserCtrl(true);
    }

    //public IEnumerator SpacePuzzleStep3()
    //{
    //    _manager.UserCtrl(false);

        

    //    _manager.UserCtrl(true);
    //}

    // ===============================
    // 🌌 Space 완료
    // ===============================
    public IEnumerator SpacePuzzleComplete()
    {
        _manager.UserCtrl(false);

        yield return _manager.TalkSay(TalkType.player,
            "…기억난다.", 2f, Talker.self, true);

        yield return _manager.TalkSay(TalkType.voice,
            "너는 늘 나랑 다른 곳을 봤어.", 2f, Talker.core, false);

        yield return _manager.TalkSay(TalkType.voice,
            "그래서 내가 못 보던 모양을 먼저 찾아냈지.", 2f, Talker.core, false);

        _manager.UserCtrl(true);
    }

    // ===============================
    // 🎨 Paint 퍼즐
    // ===============================
    public IEnumerator PaintPuzzleStart()
    {
        _manager.UserCtrl(false);

        yield return _manager.TalkSay(TalkType.player,
            "이 그림... 어딘가 익숙한데", 2f, Talker.self, true);

        yield return _manager.TalkSay(TalkType.voice,
            "네가 잊고 있던 장면이야.", 2f, Talker.core, false);

        _manager.UserCtrl(true);
    }

    public IEnumerator PaintStep1()
    {
        yield return _manager.TalkSay(TalkType.player,
            "색을 섞는 방식이… 익숙해", 2f, Talker.self, true);

        yield return _manager.TalkSay(TalkType.player,
            "머리보다 손이 먼저 기억하는 것 같아", 2f, Talker.self, true);
    }

    public IEnumerator PaintStep2()
    {
        yield return _manager.TalkSay(TalkType.voice,
            "너는 색을 고를 때 망설이지 않았어.", 2f, Talker.core, false);

        yield return _manager.TalkSay(TalkType.voice,
            "나는 형태를 먼저 봤고, 너는 분위기를 먼저 봤지.", 2f, Talker.core, false);
    }

    public IEnumerator PaintStep3()
    {
        yield return _manager.TalkSay(TalkType.player,
            "…맞다", 2f, Talker.self, true);

        yield return _manager.TalkSay(TalkType.voice,
            "우린 같은 그림을 봐도 서로 다른 색을 떠올렸어.", 2f, Talker.core, true);

        yield return _manager.TalkSay(TalkType.voice,
            "그리고 이상하게, 그게 늘 더 좋았어.", 2f, Talker.core, true);
    }

    public IEnumerator PaintPuzzleComplete()
    {
        yield return _manager.TalkSay(TalkType.player,
            "이건... 내가 잃어버린 기억이야", 2f, Talker.self, true);

        yield return _manager.TalkSay(TalkType.voice,
            "이제 거의 다 왔어.", 2f, Talker.core, true);
    }

    // ===============================
    // 🏁 엔딩
    // ===============================
    public void Episode2Ending()
    {
        StartCoroutine(EndingSequence());
    }

    IEnumerator EndingSequence()
    {
        _manager.UserCtrl(false);

        yield return _manager.TalkSay(TalkType.voice,
            "이제 모든 조각이 돌아왔어.", 2f, Talker.core, true);

        yield return _manager.TalkSay(TalkType.player,
            "이게... 나였던 거구나.", 2f, Talker.self, true);

        SceneManager.LoadScene("NextScene");

        _manager.UserCtrl(true);
    }
}