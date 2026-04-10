using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using static TextboxManager;

public class TextboxCtrl_Ep4 : MonoBehaviour
{
    public TextboxManager _manager;
    private PlayerInput user;
    public CinemachineVirtualCamera coreCam;
    public CinemachineVirtualCamera gazeCam;
    public Checkpoint_Plane S3CP0;
    public WaitForSecondsRealtime oneSec = new (1f);
    public WaitForSecondsRealtime onehalfSec = new(1.5f);
    public WaitForSecondsRealtime twoSec = new(2f);
    private bool endNPCZoneArrived = false;
    public CutsceneImagePlayer Ep4_StartCutscene;
    public CutsceneImagePlayer Ep4_ClimaxCutscene;
    public CutsceneImagePlayer Ep4_EndCutscene;
    public SoundTrigger startSound;
    public SoundTrigger endSound;
    void Awake()
    {
        user = _manager.user;
        coreCam.Priority = 1;
        gazeCam.Priority = 1;
    }
    private void OnEnable()
    {
        if (user != null)
        {
            S3CP0.S3FirstCheck += Stage4FirstEnter;
            user.gameObject.GetComponent<InteractManager>().stage4End += Stage4End;
            user.gameObject.GetComponent<InteractManager>().gameEnd += EndingCutscene;
        }
    }
    private void OnDisable()
    {
        if (user != null)
        {
            S3CP0.S3FirstCheck -= Stage4FirstEnter;
            user.gameObject.GetComponent<InteractManager>().stage4End -= Stage4End;
            user.gameObject.GetComponent<InteractManager>().gameEnd -= EndingCutscene;
        }
    }
    public void Stage4FirstEnter()
    {
        StartCoroutine(Stage4Start());
    }
    public IEnumerator Stage4Start()
    {
        if (SaveManager.instance.curData.isFirstEnterAtS3CP0) yield break;
        Ep4_StartCutscene.PlayCutscene();
        yield return null;
        StartCoroutine(_manager.TalkSay(TalkType.system, "지금까지 지나온 곳들이 전부 섞여 있다."));
        coreCam.Priority = 11;
        _manager.UserCtrl(false);
        yield return new WaitForSecondsRealtime(2f);
        StartCoroutine(_manager.TalkSay(TalkType.voice, "이제 거의 다 왔어.", Talker.core));
        yield return onehalfSec;
        StartCoroutine(_manager.TalkSay(TalkType.voice, "남은 건 하나뿐이야.", Talker.core));
        gazeCam.Priority = 12;
        coreCam.Priority = 1;
        yield return onehalfSec;
        StartCoroutine(_manager.TalkSay(TalkType.voice, "되찾는 건 거의 끝났어.\n이제는 이어 붙여야 해.", Talker.core));
        yield return onehalfSec;
        gazeCam.Priority = 1;
        SaveManager.instance.curData.isFirstEnterAtS3CP0 = true;
        endSound.Play();
        StartCoroutine(_manager.TalkSay(TalkType.voice, "추억도, 꿈도, 사랑도 따로 남아 있을 뿐이야.\n그걸 네 삶으로 받아들여야 해", Talker.core));
        _manager.UserCtrl(true);
    }
    public IEnumerator Puzzle1Start()
    {
        startSound.Play();
        StartCoroutine(_manager.TalkSay(TalkType.player, "이 길... 기억난다."));
        yield return oneSec;
        StartCoroutine(_manager.TalkSay(TalkType.voice, "추억은 장면으로만 남지 않아.\n몸이 먼저 기억하는 길도 있지.", Talker.core));
    }
    public IEnumerator Puzzle1Complete()
    {
        startSound.Play();
        StartCoroutine(_manager.TalkSay(TalkType.player, "혼자였던 적 없었구나."));
        yield return oneSec;
        StartCoroutine(_manager.TalkSay(TalkType.voice, "그 시간은 처음부터 네 안에 남아 있었어.", Talker.core));
    }
    public IEnumerator Puzzle2Start()
    {
        startSound.Play();
        StartCoroutine(_manager.TalkSay(TalkType.player, "아직 올바른 자리에서 보지 못한 것뿐이다."));
        yield return oneSec;
        StartCoroutine(_manager.TalkSay(TalkType.voice, "함께 꾸던 꿈은 없어지지 않아.\n다만, 네가 어디에 서서 바라보는지만 잊었을 뿐이지.", Talker.core));
    }
    public IEnumerator Puzzle2Complete()
    {
        startSound.Play();
        StartCoroutine(_manager.TalkSay(TalkType.voice, "혼자가 아니라, 함께 보고 있었구나."));
        yield return oneSec;
        StartCoroutine(_manager.TalkSay(TalkType.voice, "그래서 더 아팠고, 그래서 더 선명한 거야.", Talker.core));
    }
    public IEnumerator Puzzle3Start()
    {
        startSound.Play();
        StartCoroutine(_manager.TalkSay(TalkType.player, "이건… 끝내 완성하지 못했던 곡."));
        yield return oneSec;
        StartCoroutine(_manager.TalkSay(TalkType.voice, "사라진 게 아니야. 아직 다 모이지 못한 거지.", Talker.core));
    }
    public IEnumerator Puzzle3Complete()
    {
        startSound.Play();
        StartCoroutine(_manager.TalkSay(TalkType.voice, "끝난 게 아니었으니까, 여기 남아 있었겠지.", Talker.core));
        yield return oneSec;
        StartCoroutine(_manager.TalkSay(TalkType.player, "이건 잃어버린 음악이 아니라\n끝내 닿지 못했던 마음이었어."));
    }
    public IEnumerator Puzzle4Start()
    {
        _manager.UserCtrl(false);
        StartCoroutine(_manager.TalkSay(TalkType.player, "이건… 길 같기도 하고, 조각난 판 같기도 하다."));
        yield return oneSec;
        StartCoroutine(_manager.TalkSay(TalkType.voice, "하나의 길처럼 보이지만, 사실은 흩어진 조각들이지.", Talker.core));
        yield return oneSec;
        StartCoroutine(_manager.TalkSay(TalkType.player, "추억의 미끄러짐도, 꿈의 색도, 소리의 흔적도."));
        yield return oneSec;
        StartCoroutine(_manager.TalkSay(TalkType.voice, "이제는 네가 직접 그 위를 지나가야 해.", Talker.core));
        _manager.UserCtrl(true);
    }
    public void Stage4End()
    {
        StartCoroutine(Stage4Climax());
    }
    public IEnumerator Stage4Climax()
    {
        if (endNPCZoneArrived == true) yield break;
        _manager.UserCtrl(false);
        endNPCZoneArrived = true;
        Ep4_ClimaxCutscene.PlayCutscene();
        yield return oneSec;
        StartCoroutine(_manager.TalkSay(TalkType.voice, "넌 잊은 게 아니야.\n버티기 위해, 잠시 나눠 둔 거야.", Talker.core));
        yield return onehalfSec;
        StartCoroutine(_manager.TalkSay(TalkType.voice, "나는 네가 놓아둔 마지막 조각이야.", Talker.core));
        yield return onehalfSec;
        StartCoroutine(_manager.TalkSay(TalkType.player, "처음부터... 내 목소리였구나."));
        yield return new WaitForSecondsRealtime(0.5f);
        endSound.Play();
        _manager.UserCtrl(true);
    }
    public void EndingCutscene()
    {
        StartCoroutine(SyncEnding());
        _manager.UserCtrl(false);
    }
    public IEnumerator SyncEnding()
    {
        Ep4_EndCutscene.PlayCutscene();
        yield return null;
        StartCoroutine(_manager.TalkSay(TalkType.voice, "그 기억들은 널 무너뜨리기 위해 남아 있던 게 아니야.\n널 다시 네 자리로 돌려보내기 위해 남아 있던 거야.", Talker.core));
        yield return twoSec;
        StartCoroutine(_manager.TalkSay(TalkType.voice, "돌아가자. 이번엔 끝까지.", Talker.core));
        yield return twoSec;
        StartCoroutine(_manager.TalkSay(TalkType.player, "전부... 내 삶이었다."));
        yield return twoSec;
        SceneManager.LoadScene("EndingScene");
    }
}