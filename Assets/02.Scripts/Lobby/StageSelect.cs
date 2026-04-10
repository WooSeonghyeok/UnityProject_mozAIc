using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class StageSelect : MonoBehaviour
{
    public static StageSelect instance;
    public SaveDataObj curData;
    public VerticalLayoutGroup stageList;  //스테이지 리스트
    public StageSelectButton[] stageButtons;  //스테이지 리스트에 출력되는 스테이지
    private bool isStageListEnable;
    public int stageSelect = 0;  //현재 선택한 스테이지 번호
    public event Action<int> TouchStageButtonEvent;  //스테이지 선택 이벤트
    public LayoutGroup checkpointList;  //체크포인트 리스트 구역
    public CheckpointSelectButton[] cpButtons;  //체크포인트 버튼 리스트
    public int cpSelect = -1;  //현재 선택한 체크포인트 번호
    public event Action<int> TouchCPButtonEvent;  //체크포인트 선택 이벤트
    public Button EnterButton;  //입장 버튼
    private SoundTrigger EnterSound;
    private void Awake()
    {
        if (instance == null) instance = this;
        if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        if (stageList != null)
            stageButtons = stageList.GetComponentsInChildren<StageSelectButton>();
        else
            stageButtons = GetComponentsInChildren<StageSelectButton>();
        if (StageSelectionData.SelectedStage >= 0 && StageSelectionData.SelectedStage < stageButtons.Length)
        {
            stageSelect = StageSelectionData.SelectedStage;
            cpSelect = StageSelectionData.SelectedCP;
        }
        if (checkpointList != null)
            cpButtons = checkpointList.GetComponentsInChildren<CheckpointSelectButton>();
        else
            cpButtons = GetComponentsInChildren<CheckpointSelectButton>();
        curData = SaveManager.instance.curData;
        EnterSound = GetComponent<SoundTrigger>();
    }
    private void OnEnable()
    {
        if (StageSelectionData.SelectedStage == -1)
        {
            if (stageButtons != null && stageButtons.Length > 0 && !curData.ep1_open)
            {
                stageSelect = 0;
                cpSelect = 0;
            }
            else
            {
                stageSelect = -1;
                cpSelect = -1;
            }
        }
        StartCoroutine(ButtonDelay());
    }
    private IEnumerator ButtonDelay()
    {
        yield return null; // 한 프레임 대기해서 모든 버튼의 OnEnable 구독이 끝나도록 함
        if (stageButtons != null)
        {
            for (int i = 0; i < stageButtons.Length; i++)
            {
                stageButtons[i].StageNumber = i;
                stageButtons[i].Refresh();
            }
        }
        if (stageButtons != null && stageButtons.Length > 0 && stageSelect >= 0 && stageSelect < stageButtons.Length)
            TouchStageButtonEvent?.Invoke(stageSelect);
        if (cpButtons != null)
            CPButtonsPrint(Mathf.Max(0, stageSelect));
    }
    public void SelectStage(int index)
    {
        if (stageButtons == null || index < 0 || index >= stageButtons.Length) return;
        stageSelect = index;
        TouchStageButtonEvent?.Invoke(stageSelect);
        cpSelect = -1;
        CPButtonsPrint(stageSelect);
    }
    public void TouchStageButton()
    { 
        if (stageButtons == null || stageButtons.Length <= 0 || stageSelect < 0 || stageSelect >= stageButtons.Length) return;
        TouchStageButtonEvent?.Invoke(stageSelect);
        cpSelect = -1;
        CPButtonsPrint(stageSelect);
    }
    void CPButtonsPrint(int selectNum)
    {
        if (cpButtons == null || cpButtons.Length == 0) return;
        TouchCPButtonEvent?.Invoke(-1);
        for (int i = 0; i < cpButtons.Length; i++)
        {
            cpButtons[i].OnTouchCPButton(false);
            cpButtons[i].gameObject.SetActive(false);
        }
        int cpCount;
        switch (selectNum)
        {
            case 3: cpCount = 4; break;
            default: cpCount = 1; break;
        }
        for (int i = 0; i < cpCount; i++)
        {
            cpButtons[i].StageNumber = selectNum;
            cpButtons[i].cpNum = i;
            cpButtons[i].gameObject.SetActive(true);
            cpButtons[i].Refresh();
        }
        if (cpSelect >= 0 && cpSelect < cpButtons.Length)
        {
            TouchCPButtonEvent?.Invoke(cpSelect);
        }
        else
        {
            TouchCPButtonEvent?.Invoke(-1);
        }
        EnterCheck();
    }
    public void SelectCP(int selectNum, int index)
    {
        if (cpButtons == null || index < 0 || index >= cpButtons.Length) return;
        cpSelect = index;
        TouchCPButtonEvent?.Invoke(cpSelect);
        EnterCheck();
    }
    public void TouchCPButton()
    {
        if (cpButtons == null) return;
        if (cpSelect < 0 || cpSelect >= cpButtons.Length) return;
        TouchCPButtonEvent?.Invoke(cpSelect);
        EnterCheck();
    }
    private void EnterCheck()  //입장 버튼 활성화 여부 체크
    {
        if (stageSelect < 0 || stageSelect >= stageButtons.Length) //스테이지 범위 오류 시 기본값 false 반환 후 종료
        {
            EnterButton.interactable = false;
            return;
        }
        if (cpSelect < 0 || cpSelect >= cpButtons.Length)  //체크포인트 범위 오류 시 기본값 false로 후 종료
        {
            EnterButton.interactable = false;
            return;
        }
        if (EnterButton != null)
            EnterButton.interactable = !(stageButtons[stageSelect].isLock || cpButtons[cpSelect].isLock);
    }
    public void OnEnterStage()
    {
        StageSelectionData.SelectedStage = stageSelect;
        StageSelectionData.SelectedCP = cpSelect;
        if (cpButtons != null && cpSelect >= 0 && cpSelect < cpButtons.Length && cpButtons[cpSelect].isLock)
        {
            Debug.LogWarning("StageSelect: Cannot enter locked checkpoint");
            return;
        }
        EnterSound.Play();
        SceneManager.LoadScene($"Episode{stageSelect + 1}_Scene");
    }
}
