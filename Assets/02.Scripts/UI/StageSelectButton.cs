using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class StageSelectButton : MonoBehaviour
{
    public SaveDataObj CurData;
    public int StageNumber;
    public bool isLock = true;
    public TextMeshProUGUI stageName;
    public Image selectImage;
    public bool isSelect = false;
    public Image lockImage;
    private void OnEnable()
    {
        StartCoroutine(ButtonDelay());
    }
    private IEnumerator ButtonDelay()
    {
        yield return null;
        Refresh();
    }
    public void OnClick()
    {
        if (StageSelect.instance != null)
        {
            StageSelect.instance.SelectStage(StageNumber);
            RefreshAll();
        }
    }
    public void OnTouchStageButton(bool b)
    {
        if (isLock) return;
        SelectImgCheck(b);
    }
    private void SelectImgCheck(bool isSelect)
    {
        if (selectImage != null) selectImage.enabled = isSelect;
    }
    private void LockImgCheck()
    {
        if (lockImage != null) lockImage.enabled = isLock;
    }
    private void RefreshAll()
    {
        foreach (var btn in StageSelect.instance.stageButtons)
            btn.Refresh();
    }
    public void Refresh()
    {
        if (SaveManager.instance == null) return;
        CurData = SaveManager.instance.curData;
        switch (StageNumber)
        {
            case 0: isLock = !CurData.ep1_open; break;
            case 1: isLock = !CurData.ep2_open; break;
            case 2: isLock = !CurData.ep3_open; break;
            case 3: isLock = !CurData.ep4_open; break;
        }
        if (stageName != null) stageName.text = $"Stage {StageNumber + 1}";
        LockImgCheck();
        bool isSelected = (StageSelect.instance != null &&
                   StageSelect.instance.stageSelect == StageNumber &&
                   !isLock);
        SelectImgCheck(isSelected);
    }
}