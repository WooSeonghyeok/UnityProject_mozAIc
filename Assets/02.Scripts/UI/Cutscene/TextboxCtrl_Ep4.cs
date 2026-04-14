using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using static TextboxManager;

public class TextboxCtrl_Ep4 : MonoBehaviour
{
    public TextboxManager _manager;
    private PlayerInput user;
    public CinemachineVirtualCamera gazeCam;
    public CinemachineVirtualCamera climaxCam;
    public Checkpoint_Plane S3CP0;
    private bool endNPCZoneArrived = false;
    public CutsceneImagePlayer Ep4_StartCutscene;
    public CutsceneImagePlayer Ep4_ClimaxCutscene;
    public CutsceneImagePlayer Ep4_EndCutscene;
    public SoundTrigger startSound;
    public SoundTrigger endSound;
    void Awake()
    {
        user = _manager.user;
        gazeCam.Priority = 1;
        climaxCam.Priority = 1;
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
        yield return StartCoroutine(_manager.TalkSay(TalkType.system, "지금까지 지나온 곳들이 전부 섞여 있다.", 1.5f));
        _manager.UserCtrl(false);
        yield return StartCoroutine(_manager.TalkSay(TalkType.system, "이제는 하나의 기억만 보이는 게 아니다.",1.5f));
        gazeCam.Priority = 12;
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "이제 거의 다 왔어.", 2f, Talker.core));
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "남은 건 하나뿐이야.", 2f, Talker.core));
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "되찾는 건 거의 끝났어.\n이제는 이어 붙여야 해.", 1f, Talker.core));
        SaveManager.instance.curData.isFirstEnterAtS3CP0 = true;
        yield return StartCoroutine(_manager.TalkSay(TalkType.player, "이어 붙인다고? 무엇을?\n기억은 이미 돌아오고 있는 것 같은데..."));
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "조각은 모였어.\n하지만 아직 하나의 이야기가 되지 못했지.", 1.5f, Talker.core));
        gazeCam.Priority = 1;
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "추억도, 꿈도, 사랑도 따로 남아 있을 뿐이야.", 1.5f, Talker.core));
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "그걸 네 삶으로 받아들여야 해", 1f, Talker.core));
        _manager.UserCtrl(true);
        yield return StartCoroutine(_manager.TalkSay(TalkType.player, "내 삶.\n정말... 전부 내 이야기였던 걸까."));
        endSound.Play();
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "그래. 처음부터 전부 네 이야기였어.", 1f, Talker.core));
    }
    public IEnumerator Puzzle1Start()
    {
        startSound.Play();
        yield return StartCoroutine(_manager.TalkSay(TalkType.player, "이 길... 기억난다."));
        yield return StartCoroutine(_manager.TalkSay(TalkType.player, "차갑고 미끄러웠는데,\n이상하게도 그 끝에는 늘 빛이 있었어."));
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "추억은 장면으로만 남지 않아.\n몸이 먼저 기억하는 길도 있지.", 1f, Talker.core));
        yield return StartCoroutine(_manager.TalkSay(TalkType.player, "다시 지나가야 한다.\n그때의 나처럼."));
    }
    public IEnumerator Puzzle1Complete()
    {
        startSound.Play();
        yield return StartCoroutine(_manager.TalkSay(TalkType.player, "혼자였던 적 없었구나."));
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "그 시간은 처음부터 네 안에 남아 있었어.", 1f, Talker.core));
    }
    public IEnumerator Puzzle2Start()
    {
        startSound.Play();
        yield return StartCoroutine(_manager.TalkSay(TalkType.player, "이건... 사라진 게 아니다."));
        yield return StartCoroutine(_manager.TalkSay(TalkType.player, "아직 올바른 자리에서 보지 못한 것뿐이다."));
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "함께 꾸던 꿈은 없어지지 않아.\n다만, 네가 어디에 서서 바라보는지만 잊었을 뿐이지.", 1f, Talker.core));
    }
    public IEnumerator Ep4_Puzzle2_Mid()
    {
        yield return StartCoroutine(_manager.TalkSay(TalkType.player, "그때의 나는… 무엇을 보고 있었지.", 1.5f));
        yield return StartCoroutine(_manager.TalkSay(TalkType.player, "그리고 누구와 같은 방향을 바라보고 있었지."));
    }
    public IEnumerator Puzzle2Complete()
    {
        startSound.Play();
        yield return StartCoroutine(_manager.TalkSay(TalkType.player, "그래. 혼자가 아니라, 함께 보고 있었구나."));
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "그래서 더 아팠고, 그래서 더 선명한 거야.", 1f, Talker.core));
    }
    public IEnumerator Puzzle3Start()
    {
        startSound.Play();
        yield return StartCoroutine(_manager.TalkSay(TalkType.player, "이건… 끝내 완성하지 못했던 곡."));
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "사라진 게 아니야. 아직 다 모이지 못한 거지.", 1f, Talker.core));
    }
    public IEnumerator Puzzle3Complete()
    {
        startSound.Play();
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "끝난 게 아니었으니까, 여기 남아 있었겠지.",1f, Talker.core));
        yield return StartCoroutine(_manager.TalkSay(TalkType.player, "이제 들린다."));
        yield return StartCoroutine(_manager.TalkSay(TalkType.player, "이건 잃어버린 음악이 아니라\n끝내 닿지 못했던 마음이었어."));
    }
    public IEnumerator Puzzle4Start()
    {
        _manager.UserCtrl(false);
        yield return StartCoroutine(_manager.TalkSay(TalkType.player, "이 길은... 아직 나를 받아들이지 못하고 있다.", 1f, Talker.self, true));
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "마지막은 네가 정해야 해.", 1f, Talker.core, true));
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "되찾은 것들을 그냥 바라보는 것과,\n네 것으로 받아들이는 건 다르니까.", 1f, Talker.core, true));
        yield return StartCoroutine(_manager.TalkSay(TalkType.player, "받아들인다. 이 기억들이 아프더라도.\n이 관계들이 끝내 완전하지 않았더라도.", 1f, Talker.self, true));
        yield return StartCoroutine(_manager.TalkSay(TalkType.player, "전부... 내 삶이었다는 걸.", 1f, Talker.self, true));
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "그래. 좋았던 것만 네가 아니야.", 1f, Talker.core, true));
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "놓친 것도, 아팠던 것도, 끝내 다하지 못한 것도 전부 너야.", 1f, Talker.core, true));
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
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "넌 잊은 게 아니야.", 2f, Talker.core));
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "버티기 위해, 잠시 나눠 둔 거야.", 1.5f, Talker.core));
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "추억도, 꿈도, 사랑도… 전부 네가 감당해야 했던 삶이었어.", 1.5f, Talker.core));
        climaxCam.Priority = 15;
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "하지만 그때의 너는 너무 무너져 있었지.", 1.5f, Talker.core));
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "그래서 나를 남겨 둔 거야.", 1.5f, Talker.core));
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "조각들을 붙잡고 있을 마지막 자리로.", 1f, Talker.core));
        yield return StartCoroutine(_manager.TalkSay(TalkType.player, "그럼 이 사람은..."));
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "그래. 나는 네가 놓아둔 마지막 조각이야.", 1.5f,Talker.core));
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "네가 다시 돌아올 때까지 여기 남아 있었어.", 1f, Talker.core));
        climaxCam.Priority = 1;
        yield return StartCoroutine(_manager.TalkSay(TalkType.player, "처음부터... 내 목소리였구나."));
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
        yield return  StartCoroutine(_manager.TalkSay(TalkType.player, "같이 웃었던 시간도."));
        yield return  StartCoroutine(_manager.TalkSay(TalkType.player, "같은 곳을 바라보던 꿈도."));
        yield return  StartCoroutine(_manager.TalkSay(TalkType.player, "끝내 다 하지 못한 사랑도."));
        yield return  StartCoroutine(_manager.TalkSay(TalkType.player, "전부... 내 삶이었다.",1.5f));
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "이제는 알겠지.", 1.5f, Talker.self));
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "그 기억들은 널 무너뜨리기 위해 남아 있던 게 아니야.", 1.5f, Talker.self));
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "널 다시 네 자리로 돌려보내기 위해 남아 있던 거야.", 1.5f, Talker.self));
        yield return StartCoroutine(_manager.TalkSay(TalkType.voice, "돌아가자.\n이번엔 끝까지.", 2f, Talker.self));
        yield return StartCoroutine(_manager.TalkSay(TalkType.player, "이 손을 잡으면 전부 돌아온다.",1.5f));
        yield return  StartCoroutine(_manager.TalkSay(TalkType.player, "좋았던 것도.\n아팠던 것도.", 1.5f));
        yield return  StartCoroutine(_manager.TalkSay(TalkType.player, "끝내 미완성으로 남았던 것까지.", 2f));
        SceneManager.LoadScene("EndingScene");
    }
}