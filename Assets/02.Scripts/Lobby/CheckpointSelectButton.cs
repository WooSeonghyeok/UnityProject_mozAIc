using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
public class CheckpointSelectButton : MonoBehaviour
{
    public SaveDataObj curData;
    public int StageNumber;
    public int cpNum;
    public bool isLock = true;
    public TextMeshProUGUI CheckpointNumber;
    public Image selectImage;
    public bool isSelect = false;
    public Image lockImage;
private void OnEnable()
    {
        UpdateLockState();
        if (StageSelect.instance != null) StageSelect.instance.TouchCPButtonEvent += OnCPTouch;
        StartCoroutine(ButtonDelay());
    }
    private IEnumerator ButtonDelay()
    {
        yield return null;
        Refresh();
    }
    private void OnDisable()
    {
        if (StageSelect.instance != null) StageSelect.instance.TouchCPButtonEvent -= OnCPTouch;
    }
    private void UpdateLockState()
    {
        bool isCleared = Checkpoint_Plane.IsCheckpointCleared(cpNum);
        isLock = !isCleared;
        AssetDatabase.SaveAssets();
    }
    private void OnCPTouch(int cpSelect)
    {
        if (isLock) OnTouchCPButton(false);
        else OnTouchCPButton(cpSelect == cpNum);
    }
    public void OnClick()
    {
        if (StageSelect.instance != null)  StageSelect.instance.SelectCP(StageNumber, cpNum);
        else OnTouchCPButton(true);
    }
    public void OnTouchCPButton(bool b)
    {
        if (isLock) return;
        isSelect = b;
        SelectImgCheck();
    }
    public void Refresh()
    {
        curData = SaveManager.instance.curData;
        switch (StageNumber)
        {
            case 0: isLock = !curData.ep1_open; break;
            case 1: isLock = !curData.ep2_open; break;
            case 2: isLock = !curData.ep3_open; break;
            case 3:
                {
                    switch (cpNum)
                    {
                        case 0: isLock = !curData.ep4_open; break;
                        case 1: isLock = !curData.ep4_puzzle1Clear; break;
                        case 2: isLock = !curData.ep4_puzzle2Clear; break;
                        case 3: isLock = !curData.ep4_puzzle3Clear; break;
                    }
                }
                break;
        }
        if (CheckpointNumber != null) CheckpointNumber.text = $"{cpNum}";
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