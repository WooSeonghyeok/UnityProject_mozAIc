using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class Ep4_CutsceneManager : MonoBehaviour
{
    private readonly string playerTag = "Player";
    private PlayerInput user;
    private PlayerMovement userMove;
    public enum Talker { self, girl, painter, musician, core };
    public GameObject talkbox;
    public Text talkName;
    public Text talkText;
    private WaitForSecondsRealtime oneSec;
    private bool endNPCZoneArrived = false;
    public SaveDataObj curSaveData;
    public Checkpoint_Plane S3CP0;
    public CinemachineVirtualCamera coreCam;
    public CinemachineVirtualCamera gazeCam;
    public CutsceneImagePlayer Ep4_StartCutscene;
    public CutsceneImagePlayer Ep4_ClimaxCutscene;
    public CutsceneImagePlayer Ep4_EndCutscene1;
    public CutsceneImagePlayer Ep4_EndCutscene2;
    void Awake()
    {
        user = GameObject.FindGameObjectWithTag(playerTag).GetComponent<PlayerInput>();
        userMove = GameObject.FindGameObjectWithTag(playerTag).GetComponent<PlayerMovement>();
        talkbox.gameObject.SetActive(false);
        oneSec = new WaitForSecondsRealtime(1f);
        curSaveData = SaveManager.ReadCurJSON();
        coreCam.Priority = 1;
        gazeCam.Priority = 1;
        S3CP0.S3FirstCheck += Stage4FirstEnter;
    }
    private void OnEnable()
    {
        if (user != null)
        {
            user.gameObject.GetComponent<InteractManager>().stage4End += Stage4End;
            user.gameObject.GetComponent<InteractManager>().gameEnd += EndingCutscene;
        }
    }
    public void Stage4FirstEnter()
    {
        StartCoroutine(Stage4Start());
    }
    public void Stage4End()
    {
        StartCoroutine(Stage4Climax());
    }
    public void EndingCutscene()
    {
        UserCtrl(false);
        StartCoroutine(SyncEnding());
    }
    public IEnumerator Stage4Start()
    {
        if (curSaveData.isFirstEnterAtS3CP0) yield break;
        Ep4_StartCutscene.PlayCutscene();
        coreCam.Priority = 11;
        UserCtrl(false);
        yield return new WaitForSecondsRealtime(4f);
        StartCoroutine(TalkSay(Talker.core, "이제 거의 다 왔어"));
        yield return oneSec;
        StartCoroutine(TalkSay(Talker.core, "남은 건... 이어 붙이는 거야"));
        gazeCam.Priority = 12;
        yield return oneSec;
        StartCoroutine(TalkSay(Talker.core, "조각은 다 모였어.\n하지만 아직 하나의 이야기가 되지 못했지"));
        yield return oneSec;
        coreCam.Priority = 1;
        gazeCam.Priority = 1;
        curSaveData.isFirstEnterAtS3CP0 = true;
        SaveManager.instance.curData = curSaveData;
        SaveManager.instance.WriteCurJSON();
        StartCoroutine(TalkSay(Talker.core, "기억을 되찾는 건 끝났어.\n이제는 네가 그걸 네 삶으로 받아들일 차례야."));
        UserCtrl(true);
    }
    public IEnumerator Puzzle1Complete()
    {
        StartCoroutine(TalkSay(Talker.girl, "나, 너랑 놀았던 거 계속 기억하고 있었어."));
        yield return oneSec;
        StartCoroutine(TalkSay(Talker.girl, "그래서 다시 만날 수 있었던 거야."));
    }
    public IEnumerator Puzzle2Complete()
    {
        StartCoroutine(TalkSay(Talker.painter, "혼자였던 적은 없었어."));
        yield return oneSec;
        StartCoroutine(TalkSay(Talker.painter, "우린 같이 그렸고, 같이 고민했지."));
    }
    public IEnumerator Puzzle3Complete()
    {
        StartCoroutine(TalkSay(Talker.musician, "이 노래… 결국 들려줄 수 있어서 다행이야."));
        yield return oneSec;
        StartCoroutine(TalkSay(Talker.musician, "이제는… 네가 기억해줘."));
    }
    public IEnumerator Stage4Climax()
    {
        if (endNPCZoneArrived == true) yield break;
        UserCtrl(false);
        endNPCZoneArrived = true;
        Ep4_ClimaxCutscene.PlayCutscene();
        StartCoroutine(TalkSay(Talker.core, "넌 잊은 게 아니야"));
        yield return oneSec;
        StartCoroutine(TalkSay(Talker.core, "버티기 위해, 잠시 나눠 둔 거야"));
        yield return oneSec;
        StartCoroutine(TalkSay(Talker.core, "추억도, 꿈도, 사랑도...\n전부 네가 감당해야 했던 삶이었어"));
        yield return oneSec;
        StartCoroutine(TalkSay(Talker.core, "나는 네가 놓아둔 마지막 조각이야\n네가 다시 돌아올 때까지, 여기 남아 있었어"));
        yield return oneSec;
        UserCtrl(true);
    }
    public IEnumerator SyncEnding()
    {
        Ep4_EndCutscene1.PlayCutscene();
        UserCtrl(false);
        StartCoroutine(TalkSay(Talker.girl, "이제 괜찮아. 넌 계속 여기 있었으니까."));
        yield return oneSec;
        StartCoroutine(TalkSay(Talker.painter, "잊고 있던 게 아니라, 다시 그려야 했던 거야."));
        yield return oneSec;
        StartCoroutine(TalkSay(Talker.musician, "멈춘 게 아니라... 마지막 음을 기다리고 있었던 거야."));
        yield return oneSec;
        Ep4_EndCutscene2.PlayCutscene();
        yield return new WaitForSecondsRealtime(7f);
        SceneManager.LoadScene("EndingScene");
    }
    private void UserCtrl(bool b)  //유저 입력 적용 여부 컨트롤
    {
        user.enabled = b;
        userMove.enabled = b;
        userMove.SetMoveLock(!b);
    }
    public IEnumerator TalkSay(Talker talk, string say)
    {
        switch (talk)
        {
            case Talker.girl:       talkName.text = "luna";   talkName.color = Color.red;   break;
            case Talker.painter:    talkName.text = "elio";   talkName.color = Color.green; break;
            case Talker.musician:   talkName.text = "leon";   talkName.color = Color.blue;  break;
            case Talker.core:       talkName.text = "???";    talkName.color = Color.gray;  break;
            case Talker.self:       talkName.text = "";       talkName.color = Color.black; break;
        }
        talkText.text = say;
        talkText.enabled = true;
        talkbox.SetActive(true);
        yield return oneSec;
        talkbox.SetActive(false);
    }
}