using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static TextboxManager;
public class Puzzle4Manager : MonoBehaviour
{
    public static Puzzle4Manager instance;
    public SaveDataObj CurData;
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
    public event Action RetryEvent;
    public int retry_count = 0;
    [SerializeField] private float egoSync = 0f;
    public NPCData coreNPC;
    public TextboxCtrl_Ep4 cutscene;
    public bool isFirstContact = false;
    public bool isClear = false;
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
        CurData = SaveManager.instance.curData;
    }
    private void OnEnable()  //이벤트 구독 실행
    {
        if (cswitch != null)
        {
            foreach (EP4_CubeSwitch c in cswitch)
            {
                c.SwitchClick += Switch_Click;
            }
        }
        if (user != null)
        {
            var ib = user.GetComponent<InteractManager>();
            if (ib != null) ib.puzzle4Goal += Puzzle4Complete;
        }
        SyncCheck();
        InteractManager.Instance.puzzle4Hint += RetryHint;
    }
    private void OnDisable()  //이벤트 구독 해제
    {
        if (cswitch != null)
        {
            foreach (EP4_CubeSwitch c in cswitch)
            {
                c.SwitchClick -= Switch_Click;
            }
        }
        if (user != null)
        {
            var ib = user.GetComponent<InteractManager>();
            if (ib != null) ib.puzzle4Goal -= Puzzle4Complete;
        }
        InteractManager.Instance.puzzle4Hint -= RetryHint;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            RetryTxt();
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
    public void Switch_Click()
    {
        SyncCheck();
        if (egoSync >= 0.5f && !isMidCutsceneOn)  //절반 이상 같은 색으로 통일되면 중간 메시지 출력
        {
            StartCoroutine(cutscene._manager.TalkSay(TalkType.player, "좋았던 것도, 아팠던 것도,\n끝내 미완성으로 남은 것도."));
            isMidCutsceneOn = true;
        }
        else if (egoSync >= 1f && !isCompleteCutsceneOn)  //모든 칸을 같은 색으로 통일 시 완성 메시지 출력
        {
            StartCoroutine(cutscene._manager.TalkSay(TalkType.player, "전부 나로 받아들이겠다."));
            isCompleteCutsceneOn = true;
        }
        else  //그 외의 경우 힌트 메시지 출력
        {
            HintText();
        }
    }
    public void OpenRetryPopup()  //다시하기 버튼 동작
    {
        retryPopupOpen = !retryPopupOpen;
        retryPopup.SetActive(retryPopupOpen);
    }
    public void ShootRetry()  //다시하기 실행 메소드
    {
        user.GetComponent<PlayerInput>().ResetInputState();
        if (user != null && retryPos != null) user.transform.position = retryPos.position;
        retry_count++;
        RetryEvent?.Invoke();  // 전체 PuzzleCubeCtrl에 다시 초기화 이벤트 전달
        SyncCheck();
        CloseRetryPopup();
        RetryTxt();
    }
    public void CloseRetryPopup()  => retryPopup.SetActive(false);  //다시하기 팝업 닫기 동작
    void RetryHint()  //다시하기 지점 도착 시마다 메시지 출력
    {
        if (!isFirstContact)  //처음 다시하기 지점 도착 시에는 컷신 대사 출력
        {
            StartCoroutine(cutscene._manager.TalkSay(TalkType.player, "도망치지 않겠다.")); 
            isFirstContact = true;
        }
        else
        {
            HintText();  //이후 다시하기 지점 도착 시마다 힌트 대사 출력
        }
    }
    private void HintText()  //실제로 힌트 메시지를 출력하는 메소드
    {
        int x = UnityEngine.Random.Range(0, 10);
        string msg = x switch
        {
            1 => "이웃한 기억 조각끼리 색이 같아야 저편으로 넘어갈 수 있구나.",
            2 => "조각에 그려져 있는 그림이 색을 바꾸는 힌트인가 본데...",
            3 => "기억 조각의 색을 바꾸면, 다른 조각들의 색도 한꺼번에 바뀌네.",
            4 => "그림의 바탕 색은 기억 조각의 색이 어떻게 바뀌는지 알려주는 것 같아.",
            5 => "기억 조각의 색을 바꾸는 원리는 정해진 색을 켜고 끄는 식인 것 같아.",
            6 => "빛의 색은 빨강, 초록, 파랑을 겹쳐서 표현하지. 그래서 색이 여덟 종류인 거야.",
            7 => "화살표 그림은 화살표 방향을 따라 늘어선 조각들을 바꾼다는 뜻이었어.",
            8 => "별 그림은 지금 보이는 색이 같은 조각들을 한꺼번에 바꾼다는 뜻이네.",
            9 => "십자 그림은 바로 옆의 조각들만 색을 바꿔준다는 거구나.",
            _ => "기억 조각들의 색을 맞추어 도착 지점까지 길을 이어가야 해.",
        };
        StartCoroutine(cutscene._manager.TalkSay(TalkType.player, msg));
    }
    public void Puzzle4Complete()  //퍼즐 완료 시 처리
    {
        if (isClear) return;  //클리어 처리는 1번만
        SyncCheck();
        puzzle4MemoryRate = puzzle4BaseMemoryRate - Math.Clamp(retry_count, 0, puzzle4BaseMemoryRate);  //다시하기 횟수만큼 기억 퍼즐 재구성 점수 감점(최대 5점까지)
        CurData.memory_reconstruction_rate[11] = puzzle4MemoryRate;  //퍼즐 4 점수 획득
        SelfVoiceTag(egoSync >= 1f);  //자아 통합도 100% 달성 여부에 따라 "self_voice" 태그를 획득
        isClear = true;
    }
    private void SelfVoiceTag(bool b)
    {
        var tag = CurData.CoreTag.FirstOrDefault(t => t.TagName == "self_voice");
        if (tag != null) tag.tagGet = b;
        else
        {
            CurData.CoreTag.Add(new IsTagGet
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
    private void RetryTxt()  //다시하기 횟수 텍스트 갱신
    {
        if (RetryCnt != null) RetryCnt.text = $"Retry: {retry_count}";
    }
}