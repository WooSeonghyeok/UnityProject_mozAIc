using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageSelect : MonoBehaviour
{
    public static StageSelect instance;

    public SaveDataObj CurData;

    public VerticalLayoutGroup stageList;  //스테이지 리스트
    public StageSelectButton[] stageButtons;  //스테이지 리스트에 출력되는 스테이지
    public int stageSelect = 0;  //현재 선택한 스테이지 번호

    public LayoutGroup checkpointList;  //체크포인트 리스트 구역
    public CheckpointSelectButton[] cpButtons;  //체크포인트 버튼 리스트
    public int cpSelect = -1;  //현재 선택한 체크포인트 번호

    public Button EnterButton;  //입장 버튼
    public SoundTrigger EnterSound;  //입장 시 사운드 출력

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

        CurData = SaveManager.instance.curData;
        EnterSound = GetComponent<SoundTrigger>();
    }

    private void OnEnable()
    {
        if (StageSelectionData.SelectedStage == -1)
        {
            if (stageButtons != null && stageButtons.Length > 0 && !CurData.ep1_open)
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

        if (cpButtons != null)
            CPButtonsPrint(Mathf.Max(0, stageSelect));
    }
    private int GetCPCount(int stage)
    {
        return stage switch
        {
            1 => 3,
            3 => 4,
            _ => 1
        };
    }
    public void SelectStage(int index)
    {
        if (stageButtons == null || index < 0 || index >= stageButtons.Length) return;

        stageSelect = index;
        RefreshStageButtons();
        CPReset();
    }
    private void RefreshStageButtons()
    {
        foreach (var btn in stageButtons)
            btn.Refresh();
    }
    public void TouchStageButton()
    {
        if (stageButtons == null || stageButtons.Length <= 0 ||
            stageSelect < 0 || stageSelect >= stageButtons.Length) return;

        CPReset();
    }

    private void CPReset()
    {
        cpSelect = -1;
        CPButtonsPrint(stageSelect);
    }

    void CPButtonsPrint(int selectNum)
    {
        if (cpButtons == null || cpButtons.Length == 0) return;

        foreach (var btn in cpButtons)
        {
            btn.OnTouchCPButton(false);
            btn.gameObject.SetActive(false);
        }

        int cpCount = GetCPCount(selectNum);

        for (int i = 0; i < cpCount; i++)
        {
            cpButtons[i].StageNumber = selectNum;
            cpButtons[i].cpNum = i;
            cpButtons[i].gameObject.SetActive(true);
            cpButtons[i].Refresh();
        }

        if (cpCount == 1)
        {
            if (stageSelect != -1 && cpButtons.Length > 0 && !cpButtons[0].isLock)
                SelectCP(selectNum, 0);
            else
                cpSelect = -1;
        }
        else
        {
            if (cpSelect < 0 || cpSelect >= cpCount || cpButtons[cpSelect].isLock)
                cpSelect = -1;
        }

        EnterButton.interactable = EnterCheck();
    }

    public void SelectCP(int selectNum, int index)
    {
        if (cpButtons == null || index < 0 || index >= cpButtons.Length) return;

        cpSelect = index;
        NotifyCPSelected();
    }

    public void TouchCPButton()
    {
        if (cpButtons == null) return;
        if (cpSelect < 0 || cpSelect >= cpButtons.Length) return;

        NotifyCPSelected();
    }
    private void NotifyCPSelected()
    {
        EnterButton.interactable = EnterCheck();
    }
    private bool EnterCheck()  //입장 버튼 활성화 여부 체크
    {
        return
        stageSelect >= 0 && stageSelect < stageButtons.Length &&  //스테이지 범위 오류 시 false 반환
        cpSelect >= 0 && cpSelect < cpButtons.Length &&  //체크포인트 범위 오류 시 false 반환
        !stageButtons[stageSelect].isLock && !cpButtons[cpSelect].isLock;    //스테이지, 체크포인트 둘 다 잠금 해제여야 true
    }

    public void OnEnterStage()
    {
        StageSelectionData.SelectedStage = stageSelect;
        StageSelectionData.SelectedCP = cpSelect;

        if (cpButtons != null &&
            cpSelect >= 0 && cpSelect < cpButtons.Length &&
            cpButtons[cpSelect].isLock)
        {
            Debug.LogWarning("StageSelect: Cannot enter locked checkpoint");
            return;
        }

        EnterSound.Play();

        GameManager.Instance.openPopupCnt = 0;
        GameManager.Instance.lookLock = false;

        SceneManager.LoadScene($"Episode{stageSelect + 1}_Scene");
    }
}