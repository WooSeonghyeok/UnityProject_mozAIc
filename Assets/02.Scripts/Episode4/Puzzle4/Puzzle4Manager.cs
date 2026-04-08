using System;
using UnityEngine;
using UnityEngine.UI;
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
    public Text switchText;
    public Button retryButton;
    public GameObject retryPopup;
    public bool retryPopupOpen = false;
    public event Action retryEvent;
    private bool scoreFinished = false;
    private int switch_this = 0;
    private int switch_total = 0;
    public int retry_count = 0;
    [SerializeField] private float egoSync = 0f;
    public Ep4_CutsceneManager cutsceneManager;
    public float puzzle4MemoryRate = 0f;
    private SoundTrigger clearSound;
    public GameObject interactionUI;   // "E" 상호작용 UI
    void Awake()
    {
        if (instance == null) instance = this;
        if (instance != this) Destroy(instance);
        user = GameObject.FindGameObjectWithTag(playerTag).GetComponent<PlayerMovement>();
        cswitch = cubes.GetComponentsInChildren<EP4_CubeSwitch>();
        acube = cubes.GetComponentsInChildren<EP4_Puzzle4_CubeCtrl>();
        switchText.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);
        retryPopup.SetActive(false);
        retryPopupOpen = false;
        clearSound = GetComponent<SoundTrigger>();
        interactionUI.SetActive(false);
    }
    private void OnEnable()
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
    public void Switch_CountUp()  //스위치 상호작용 횟수 누적 메소드
    {
        switch_this++;
        SyncCheck();
    }
    public void OpenRetryPopup()  //다시하기 버튼 동작
    {
        retryPopupOpen = !retryPopupOpen;
        retryPopup.SetActive(retryPopupOpen);
    }
    public void ShootRetry()  //다시하기 실행 메소드
    {
        if (user != null && retryPos != null) user.transform.position = retryPos.position;
        switch_total += switch_this;
        switch_this = 0;
        retry_count++;
        retryEvent?.Invoke();  // 전체 PuzzleCubeCtrl에 다시 초기화 이벤트 전달
        SyncCheck();
        CloseRetryPopup();
    }
    public void CloseRetryPopup()  //다시하기 팝업 닫기 동작
    {
        retryPopup.SetActive(false);
    }
    void HintMessage()  //다시하기 지점 도착 시마다 힌트 메시지를 출력
    {
        string msg = "";
        float x = UnityEngine.Random.Range(0, 9);
        msg = x switch
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
        StartCoroutine(cutsceneManager.TalkSay(Ep4_CutsceneManager.Talker.self, msg));
    }
    public void Puzzle4Complete()  //퍼즐 완료 시 처리
    {
        if (scoreFinished == true) return;  //마지막 점수 계산 종료 확인
        switch_total += switch_this;  //최종 상호작용 횟수 확정
        SyncCheck();
        FinalScore();
        if (egoSync == 1f) SelfVoiceTag();  //자아 통합도 100% 달성해야 퍼즐4의 기억 조각을 획득 → 진엔딩 루트 진입
    }
    private void FinalScore()
    {
        puzzle4MemoryRate += Mathf.RoundToInt(switch_total * 0.1f);  //상호작용 횟수에 따른 기억 재구성률 점수 계산
        puzzle4MemoryRate += retry_count;  //다시하기 횟수에 따른 기억 재구성률 점수 계산
        puzzle4MemoryRate = Math.Clamp(puzzle4MemoryRate, 0, 5f);  //각 퍼즐당 최대 5점까지
        EndingConditionData.memory_reconstruction_rate -= (int)(puzzle4MemoryRate);  //이전까지 총 점수에서 감점
        SaveManager.instance.curData.memory_reconstruction_rate = EndingConditionData.memory_reconstruction_rate;
        Debug.Log($"최종점수: {EndingConditionData.memory_reconstruction_rate}");
        scoreFinished = true;  //마지막 점수 계산 종료 확인
    }
    private void SelfVoiceTag()
    {
        EndingConditionData.self_voice = true;
        foreach (IsTagGet lastTag in SaveManager.instance.curData.MemoryTag)
        {
            if (lastTag.TagName == "self_voice") lastTag.tagGet = true;
        }
        Debug.Log($"마지막 기억 획득!");
        clearSound.Play();
    }
    public void SyncCheck()  //자아 통합도 계산
    {
        if (acube == null || acube.Length == 0 || lastCube == null)
        {
            egoSync = 0f;
            CountTxt();
            return;
        }
        int count = 0;
        foreach (EP4_Puzzle4_CubeCtrl c in acube)
        {
            if (c != null && c.cubeColor == lastCube.cubeColor) count++;
        }
        egoSync = (float)count / (float)acube.Length;
        CountTxt();
    }
    private void CountTxt()  //텍스트 갱신 메소드
    {
        if (switchText != null)
            switchText.text = $"이번 스위치 횟수 = {switch_this}\n총 스위치 횟수 = {switch_total}\n다시하기 횟수 = {retry_count}\n자아 통합도 = {egoSync * 100}%";
    }
}