using Cinemachine;
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
    public CinemachineVirtualCamera playerCam;
    public CinemachineVirtualCamera[] introCams;

    private bool introPlayed = false;
    void SwitchToCam(CinemachineVirtualCamera cam)
    {
        if (cam == null) return;

        playerCam.Priority = 0;

        foreach (var c in introCams)
        {
            if (c != null)
                c.Priority = 0;
        }

        cam.Priority = 20;
    }
    // 🎬 공용 카메라 시퀀스 (추가된 부분)
    IEnumerator PlayIntroCams()
    {
        if (introCams == null || introCams.Length == 0)
            yield break;

        foreach (var cam in introCams)
        {
            if (cam == null) continue;

            SwitchToCam(cam);
            yield return new WaitForSecondsRealtime(2f);
        }
    }
    void BackToPlayerCam()
    {
        playerCam.Priority = 20;

        foreach (var c in introCams)
        {
            if (c != null)
                c.Priority = 0;
        }
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
        if (SaveManager.instance.curData.Played_EP2_Text_Intro)  yield break;
        SaveManager.instance.curData.Played_EP2_Text_Intro = true;
        SaveManager.WriteCurJSON(SaveManager.instance.curData);
        if (introPlayed) yield break;
        introPlayed = true;
        GameManager.Instance.CutsceneMode(true);
        _manager.UserCtrl(false);

        yield return _manager.TalkSay(TalkType.system,
            "이곳에는 멈춰 버린 열정이 남아 있다.", 2f);

        yield return _manager.TalkSay(TalkType.system,
            "장면은 보이지만, 아직 색은 살아 있지 않다.", 2f);

        yield return _manager.TalkSay(TalkType.player,
            "여긴... 어디지?", 2f, Talker.self, true);

        if (introCams.Length > 0 && introCams[0] != null)
        {
            SwitchToCam(introCams[0]);
            yield return new WaitForSecondsRealtime(2f);
        }

        yield return _manager.TalkSay(TalkType.voice,
            "…누구지..", 2f, Talker.core, true);

        yield return _manager.TalkSay(TalkType.voice,
            "처음 보는 것 같은데.", 2f, Talker.core, true);

        yield return _manager.TalkSay(TalkType.voice,
            "왜 낯이 익은 것 같지?", 2f, Talker.core, true);

        BackToPlayerCam();

        _manager.UserCtrl(true);
        GameManager.Instance.lookLock = false;
        GameManager.Instance.CutsceneMode(false);
    }

    // ===============================
    // 🌌 Space 퍼즐 시작
    // ===============================
    public IEnumerator SpacePuzzleStart()
    {
        _manager.UserCtrl(false);
        GameManager.Instance.CutsceneMode(true);

        yield return _manager.TalkSay(TalkType.player,
            "여기는 어디지..??", 2f, Talker.self, true);

        // 🔥 여기다가 넣으면 됨
        for (int i = 0; i < introCams.Length; i++)
        {
            if (introCams[i] == null) continue;

            // 🎬 카메라 이동
            SwitchToCam(introCams[i]);
            yield return new WaitForSecondsRealtime(1.5f);

            // 💬 텍스트
            if (i == 0)
            {
                yield return _manager.TalkSay(TalkType.voice,
                    "너는 옛날부터 다양한 각도로 그림을 바라봤어.", 2f, Talker.core, true);
            }
            else if (i == 1)
            {
                yield return _manager.TalkSay(TalkType.voice,
                    "네가 구도를 잡을 때 기억을 떠올려봐.", 2f, Talker.core, true);
            }
            else if (i == 2)
            {
                yield return _manager.TalkSay(TalkType.voice,
                    "하늘을 유심히 보면 \n예전처럼 찾아낼 수 있을거야.", 2.5f, Talker.core, true);
            }
        }

        // 🎬 다시 플레이어 카메라
        BackToPlayerCam();

        _manager.UserCtrl(true);
        GameManager.Instance.CutsceneMode(false);
    }

    // ===============================
    // 🌌 Space 퍼즐 단계별
    // ===============================
    public IEnumerator SpacePuzzleStep1()
    {
        _manager.UserCtrl(false);
        GameManager.Instance.CutsceneMode(true);

        yield return _manager.TalkSay(TalkType.player,
            "방금… 흩어져 있던 것들이 하나로 보였어", 2f, Talker.self, true);

        _manager.UserCtrl(true);
        GameManager.Instance.CutsceneMode(false);
    }

    public IEnumerator SpacePuzzleStep2()
    {
        _manager.UserCtrl(false);
        GameManager.Instance.CutsceneMode(true);

        yield return _manager.TalkSay(TalkType.voice,
            "같은 장면도 어디에서 보느냐에 따라 완전히 달라지지.", 2f, Talker.core, true);

        yield return _manager.TalkSay(TalkType.voice,
            "우린 그런 이야기를 자주 했던 것 같아.", 2f, Talker.core, true);

        _manager.UserCtrl(true);
        GameManager.Instance.CutsceneMode(false);
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
        GameManager.Instance.CutsceneMode(true);

        yield return _manager.TalkSay(TalkType.player,
            "…기억난다.", 2f, Talker.self, true);

        yield return _manager.TalkSay(TalkType.voice,
            "너는 늘 나랑 다른 곳을 봤어.", 2f, Talker.core, true);

        yield return _manager.TalkSay(TalkType.voice,
            "그래서 내가 못 보던 모양을 먼저 찾아냈지.", 2f, Talker.core, true);

        _manager.UserCtrl(true);
        GameManager.Instance.CutsceneMode(false);
    }

    // ===============================
    // 🎨 Paint 퍼즐
    // ===============================
    public IEnumerator PaintPuzzleStart()
    {

        _manager.UserCtrl(false);
        GameManager.Instance.CutsceneMode(true);

        yield return _manager.TalkSay(TalkType.player,
                   "이 공간... 어딘가 익숙한데", 2f, Talker.self, true);

        for (int i = 0; i < introCams.Length; i++)
        {
            if (introCams[i] == null) continue;

            // 🎬 카메라 이동
            SwitchToCam(introCams[i]);
            yield return new WaitForSecondsRealtime(1.5f);

            // 💬 텍스트
            if (i == 0)
            {
                yield return _manager.TalkSay(TalkType.voice,
                      "너는 줄곧 색을 잘 조합하곤 했지", 2f, Talker.core, true);
            }
            else if (i == 1)
            {
                yield return _manager.TalkSay(TalkType.voice,
                   "네가 잊고 있던 그림이야.", 2f, Talker.core, true);
            }

        }
        BackToPlayerCam();
        _manager.UserCtrl(true);
        GameManager.Instance.CutsceneMode(false);
    }

    public IEnumerator PaintStep1()
    {
        GameManager.Instance.CutsceneMode(true);
        yield return _manager.TalkSay(TalkType.player,
            "색을 섞는 방식이… 익숙해", 2f, Talker.self, true);

        yield return _manager.TalkSay(TalkType.player,
            "머리보다 손이 먼저 기억하는 것 같아", 2f, Talker.self, true);
        GameManager.Instance.CutsceneMode(false);
    }

    public IEnumerator PaintStep2()
    {
        GameManager.Instance.CutsceneMode(true);
        yield return _manager.TalkSay(TalkType.voice,
            "너는 색을 고를 때 망설이지 않았어.", 2f, Talker.core, true);

        yield return _manager.TalkSay(TalkType.voice,
            "나는 구도를 먼저 잡았고, 너는 색을 먼저 골랐지.", 2f, Talker.core, true);
        GameManager.Instance.CutsceneMode(false);
    }

    public IEnumerator PaintStep3()
    {
        GameManager.Instance.CutsceneMode(true);
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
        GameManager.Instance.CutsceneMode(false);
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
        GameManager.Instance.CutsceneMode(true);
        for (int i = 1; i < introCams.Length; i++)
        {
            if (introCams[i] == null) continue;

            // 🎬 카메라 이동
            SwitchToCam(introCams[i]);
            yield return new WaitForSecondsRealtime(2f);

            // 💬 텍스트
            if (i == 1)
            {
                yield return _manager.TalkSay(TalkType.player,
                    "이제 모든 기억이 돌아왔어.", 4f, Talker.self, true);

                yield return _manager.TalkSay(TalkType.player,
                    "이게... 나였던 거구나.", 4f, Talker.self, true);
            }
            else if (i == 2)
            {
                yield return _manager.TalkSay(TalkType.voice,
                 "너는 그냥 여기 들른 사람이 아니야.", 4f, Talker.core, true);

                yield return _manager.TalkSay(TalkType.voice,
                   "내 동료였어.", 4f, Talker.core, true);
                yield return _manager.TalkSay(TalkType.voice,
                  "같이 그리고, 같이 고민하고….", 4f, Talker.core, true);

                yield return _manager.TalkSay(TalkType.voice,
                  "같이 끝까지 가려고 했던 사람.", 4f, Talker.core, true);

                yield return _manager.TalkSay(TalkType.voice,
                  "왜 멈췄는지 이제 조금 알 것 같아.", 4f, Talker.core, true);

                yield return _manager.TalkSay(TalkType.voice,
                  "이건 너무 중요해서… 쉽게 끝낼 수 없었던 거야.", 4f, Talker.core, true);

                yield return _manager.TalkSay(TalkType.voice,
                  "이건 내 그림이 아니라, 우리 그림이었으니까.", 4f, Talker.core, true);
            }
            else if (i == 3)
            {
                var manager = FindObjectOfType<Episode2Manager>();
                if (manager != null && manager.finalObject != null)
                {
                    manager.finalObject.SetActive(true);
                }
                yield return new WaitForSecondsRealtime(2f);

                yield return _manager.TalkSay(TalkType.voice,
                 "네가 꿈꾸던 장면은 돌아왔어.", 4f, Talker.core, true);

                yield return _manager.TalkSay(TalkType.voice,
                 "이제 그 장면에 남아 있던 마지막 울림을 찾으러 가.", 4f, Talker.core, true);
            }

        }

        BackToPlayerCam();

        _manager.UserCtrl(true);
        GameManager.Instance.CutsceneMode(false);
    }
}