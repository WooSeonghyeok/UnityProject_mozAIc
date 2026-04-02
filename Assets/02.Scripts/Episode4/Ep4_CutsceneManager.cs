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
    public Image talkbox;
    public Text talkText;
    private WaitForSecondsRealtime oneSec;
    private bool endNPCZoneArrived = false;
    public SaveDataObj curSaveData;
    public Checkpoint_Plane S3CP0;
    public CinemachineVirtualCamera coreCam;
    public CinemachineVirtualCamera gazeCam;
    public GameObject climaxOrbit;
    public GameObject EndOrbit;
    void Awake()
    {
        user = GameObject.FindGameObjectWithTag(playerTag).GetComponent<PlayerInput>();
        userMove = GameObject.FindGameObjectWithTag(playerTag).GetComponent<PlayerMovement>();
        talkbox.enabled = false;
        talkText.enabled = false;
        oneSec = new WaitForSecondsRealtime(1f);
        curSaveData = SaveManager.ReadCurJSON();
        coreCam.Priority = 1;
        gazeCam.Priority = 1;
        S3CP0.S3CP0FirstCheck += Stage4FirstEnter;
        climaxOrbit.SetActive(false);
        EndOrbit.SetActive(false);
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
        coreCam.Priority = 11;
        UserCtrl(false);
        yield return oneSec;
        StartCoroutine(TalkSay("이제 거의 다 왔어", Color.white));
        yield return oneSec;
        yield return oneSec;
        StartCoroutine(TalkSay("남은 건... 이어 붙이는 거야", Color.white));
        gazeCam.Priority = 12;
        yield return oneSec;
        StartCoroutine(TalkSay("조각은 다 모였어.\n하지만 아직 하나의 이야기가 되지 못했지", Color.white));
        yield return oneSec;
        coreCam.Priority = 1;
        gazeCam.Priority = 1;
        curSaveData.isFirstEnterAtS3CP0 = true;
        SaveManager.instance.curData = curSaveData;
        SaveManager.instance.WriteCurJSON();
        StartCoroutine(TalkSay("기억을 되찾는 건 끝났어.\n이제는 네가 그걸 네 삶으로 받아들일 차례야.", Color.white));
        UserCtrl(true);
    }
    public IEnumerator Stage4Climax()
    {
        if (endNPCZoneArrived == true) yield break;
        UserCtrl(false);
        endNPCZoneArrived = true;
        climaxOrbit.SetActive(true);
        StartCoroutine(TalkSay("넌 잊은 게 아니야",Color.white));
        yield return oneSec;
        StartCoroutine(TalkSay("버티기 위해, 잠시 나눠 둔 거야", Color.white));
        yield return oneSec;
        StartCoroutine(TalkSay("추억도, 꿈도, 사랑도...\n전부 네가 감당해야 했던 삶이었어", Color.white));
        yield return oneSec;
        StartCoroutine(TalkSay("나는 네가 놓아둔 마지막 조각이야\n네가 다시 돌아올 때까지, 여기 남아 있었어", Color.white));
        yield return oneSec;
        UserCtrl(true);
        climaxOrbit.SetActive(false);
    }
    public IEnumerator SyncEnding()
    {
        EndOrbit.SetActive(true);
        StartCoroutine(TalkSay("이제 괜찮아. 넌 계속 여기 있었으니까.", Color.red));
        yield return oneSec;
        StartCoroutine(TalkSay("잊고 있던 게 아니라, 다시 그려야 했던 거야.", Color.green));
        yield return oneSec;
        StartCoroutine(TalkSay("멈춘 게 아니라... 마지막 음을 기다리고 있었던 거야.", Color.blue));
        yield return oneSec;
        EndOrbit.SetActive(false);
        yield return oneSec;
        SceneManager.LoadScene("EndingScene");
    }
    private void UserCtrl(bool b)  //유저 입력 적용 여부 컨트롤
    {
        user.enabled = b;
        userMove.enabled = b;
        userMove.SetMoveLock(!b);
    }
    public IEnumerator TalkSay(string say,Color col)
    {
        talkText.text = say;
        talkText.color = col;
        talkText.enabled = true;
        talkbox.enabled = true;
        yield return oneSec;
        talkText.enabled = false;
        talkbox.enabled = false;
    }
}