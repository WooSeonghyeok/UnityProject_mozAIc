using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static TextboxManager;
public class Puzzle4Manager : MonoBehaviour
{
    public static Puzzle4Manager instance;
    private PlayerMovement user;
    public GameObject cubes;
    private EP4_CubeSwitch[] cswitch;
    private EP4_Puzzle4_CubeCtrl[] acube;
    public EP4_Puzzle4_CubeCtrl lastCube;
    public Transform retryPos;
    private readonly string playerTag = "Player";
    public GameObject RetryBox;
    public Text RetryCnt;
    public Button retryButton;
    public GameObject retryPopup;
    public bool retryPopupOpen = false;
    public event Action retryEvent;
    public int retry_count = 0;
    [SerializeField] private float egoSync = 0f;
    public NPCData coreNPC;
    public TextboxCtrl_Ep4 cutscene;
    public bool isFirstContact = false;
    [SerializeField] private int puzzle4BaseMemoryRate = 5;  //퍼즐 클리어 시 기본으로 획득하는 기억 퍼즐 재구성 점수
    public int puzzle4MemoryRate = 0;
    private SoundTrigger clearSound;
    public GameObject interactionUI;   // "E" 상호작용 UI
    private bool isMidCutsceneOn = false;
    private bool isCompleteCutsceneOn = false;
    void Awake()
    {
        if (instance == null) instance = this;
        if (instance != this) Destroy(instance);
        user = GameObject.FindGameObjectWithTag(playerTag).GetComponent<PlayerMovement>();
        cswitch = cubes.GetComponentsInChildren<EP4_CubeSwitch>();
        acube = cubes.GetComponentsInChildren<EP4_Puzzle4_CubeCtrl>();
        RetryBox.SetActive(false);
        retryButton.gameObject.SetActive(false);
        retryPopup.SetActive(false);
        retryPopupOpen = false;
        clearSound = GetComponent<SoundTrigger>();
        interactionUI.SetActive(false);
    }
    private void OnEnable()  //이벤트 구독 실행
    {
        if (cswitch != null)
        {
            foreach (EP4_CubeSwitch c in cswitch)
            {
                c.SwitchClick += Switch_CountUp;
            }
        }
        if (user != null)
        {
            var ib = user.GetComponent<InteractManager>();
            if (ib != null) ib.puzzle4Goal += Puzzle4Complete;
        }
        SyncCheck();
        InteractManager.Instance.puzzle4Hint += HintMessage;
    }
    private void OnDisable()  //이벤트 구독 해제
    {
        if (cswitch != null)
        {
            foreach (EP4_CubeSwitch c in cswitch)
            {
                c.SwitchClick -= Switch_CountUp;
            }
        }
        if (user != null)
        {
            var ib = user.GetComponent<InteractManager>();
            if (ib != null) ib.puzzle4Goal -= Puzzle4Complete;
        }
        InteractManager.Instance.puzzle4Hint -= HintMessage;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            CountTxt();
            retryButton.gameObject.SetActive(true);
            RetryBox.SetActive(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            retryButton.gameObject.SetActive(false);
            retryPopup.SetActive(false);
            RetryBox.SetActive(false);
        }
    }
    public void Switch_CountUp()  //스위치 상호작용 횟수 누적 메소드
    {
        SyncCheck();
        if (egoSync >= 0.5f && !isMidCutsceneOn)
        {
            StartCoroutine(cutscene._manager.TalkSay(TalkType.player, "좋았던 것도, 아팠던 것도,\n끝내 미완성으로 남은 것도."));
            isMidCutsceneOn = true;
        }
        if (egoSync >= 1f && !isCompleteCutsceneOn)
        {
            StartCoroutine(cutscene._manager.TalkSay(TalkType.player, "전부 나로 받아들이겠다."));
            isCompleteCutsceneOn = true;
        }
    }
    public void OpenRetryPopup()  //다시하기 버튼 동작
    {
        retryPopupOpen = !retryPopupOpen;
        retryPopup.SetActive(retryPopupOpen);
    }
    public void ShootRetry()  //다시하기 실행 메소드
    {
        if (user != null && retryPos != null) user.transform.position = retryPos.position;
        retry_count++;
        retryEvent?.Invoke();  // 전체 PuzzleCubeCtrl에 다시 초기화 이벤트 전달
        SyncCheck();
        CloseRetryPopup();
    }
    public void CloseRetryPopup()  => retryPopup.SetActive(false);  //다시하기 팝업 닫기 동작
    void HintMessage()  //다시하기 지점 도착 시마다 힌트 메시지를 출력
    {
        if (!isFirstContact)  //처음 다시하기 지점 도착 시에는 컷신 대사를 대신 출력
        {
            StartCoroutine(cutscene._manager.TalkSay(TalkType.player, "도망치지 않겠다.")); 
            isFirstContact = true;
        }
        else
        {
            float x = UnityEngine.Random.Range(0, 9);
            string msg = x switch
            {
                1 => "기억 조각의 색을 바꿀 수 있는 스위치가 있는데?\n사용해보자.",
                2 => "스위치로 기억 조각의 색을 바꾸면\n연결된 다른 조각들도 함께 바뀌는 모양이네...",
                3 => "이 조각에 어느 조각이 연결되어 있는지는\n조각에 그려진 무늬를 보고 알 수 있겠지.",
                4 => "색을 바꾸는 스위치는 정해진 색을 켜거나 끄는 원리인가 봐.",
                5 => "화살표 무늬는 방향, 별 무늬는 색,\n 그렇다면 십자 무늬는 뭘 상징하지?",
                6 => "떠올랐어. 빛의 색은 빨강, 초록, 파랑을 섞어서 표현하는구나.",
                7 => "다른 색을 띠는 기억으로는 넘어갈 수 없는 것 같아.",
                _ => "여기서부터 기억의 색을 맞추어 길을 이어가야 해.",
            };
            StartCoroutine(cutscene._manager.TalkSay(TalkType.player, msg));
        }
    }
    public void Puzzle4Complete()  //퍼즐 완료 시 처리
    {
        SyncCheck();
        puzzle4MemoryRate = puzzle4BaseMemoryRate - Math.Clamp(retry_count, 0, puzzle4BaseMemoryRate);  //다시하기 횟수만큼 기억 퍼즐 재구성 점수 감점(최대 5점까지)
        SaveManager.instance.curData.memory_reconstruction_rate[11] = puzzle4MemoryRate;  //퍼즐 4 점수 획득
        SelfVoiceTag(egoSync >= 1f);  //자아 통합도 100% 달성 여부에 따라 "self_voice" 태그를 획득
    }
    private void SelfVoiceTag(bool b)
    {
        var tag = SaveManager.instance.curData.CoreTag.FirstOrDefault(t => t.TagName == "self_voice");
        if (tag != null) tag.tagGet = b;
        else
        {
            SaveManager.instance.curData.CoreTag.Add(new IsTagGet
            {
                TagName = "self_voice",
                tagGet = b
            });
        }
        if (b == true)
        {
            Debug.Log($"마지막 기억 획득!");
            clearSound.Play();
            coreNPC.revealStage = MemoryRevealStage.Full;
        }
    }
    public void SyncCheck()  //자아 통합도 계산
    {
        if (acube == null || acube.Length == 0 || lastCube == null)
        {
            egoSync = 0f;
            return;
        }
        int count = 0;
        foreach (EP4_Puzzle4_CubeCtrl c in acube)
        {
            if (c != null && c.cubeColor == lastCube.cubeColor) count++;
        }
        egoSync = (float)count / (float)acube.Length;
    }
    private void CountTxt()  //텍스트 갱신 메소드
    {
        if (RetryCnt != null) RetryCnt.text = $"Retry: {retry_count}";
    }
}