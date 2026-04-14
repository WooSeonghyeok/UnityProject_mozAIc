using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class StageSelectButton : MonoBehaviour
{
    public SaveDataObj curData;
    public int StageNumber;
    public bool isLock = true;
    public TextMeshProUGUI stageName;
    public Image selectImage;
    public bool isSelect = false;
    public Image lockImage;
    private void OnEnable()
    {
        if (StageSelect.instance != null) StageSelect.instance.TouchStageButtonEvent += OnStageTouch;
        StartCoroutine(ButtonDelay());
    }
    private IEnumerator ButtonDelay()
    {
        yield return null;
        Refresh();
    }
    private void OnDisable()
    {
        if (StageSelect.instance != null) StageSelect.instance.TouchStageButtonEvent -= OnStageTouch;
    }
    private void OnStageTouch(int stageSelect) => OnTouchStageButton(stageSelect == StageNumber);
    public void OnClick()
    {
        if (StageSelect.instance != null) StageSelect.instance.SelectStage(StageNumber);
        else OnTouchStageButton(true);
    }
    public void OnTouchStageButton(bool b)
    {
        if (isLock) return;
        isSelect = b;
        SelectImgCheck();
    }
    public void Refresh()
    {
        if (SaveManager.instance == null) return;
        curData = SaveManager.instance.curData;
        switch (StageNumber)
        {
            case 0: isLock = !curData.ep1_open; break;
            case 1: isLock = !curData.ep2_open; break;
            case 2: isLock = !curData.ep3_open; break;
            case 3: isLock = !curData.ep4_open; break;
        }
        if (stageName != null) stageName.text = $"Stage {StageNumber + 1}";
        SelectImgCheck();
        LockImgCheck();
    }
    private void SelectImgCheck()
    {
        if (selectImage != null) selectImage.enabled = isSelect;
    }
    private void LockImgCheck()
    {
        if (lockImage != null) lockImage.enabled = isLock;
    }
}