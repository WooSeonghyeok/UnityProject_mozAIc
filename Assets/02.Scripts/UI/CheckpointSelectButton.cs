using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CheckpointSelectButton : MonoBehaviour
{
    public SaveDataObj CurData;

    public int StageNumber;
    public int cpNum;

    public bool isLock = true;

    public TextMeshProUGUI CheckpointNumber;

    public Image selectImage;
    public bool isSelect = false;

    public Image lockImage;

    // ⭐ Panel 이미지 (배경)
    public Image panelImage;

    // ⭐ Stage별 Sprite 배열
    public Sprite[] stage1Sprites;
    public Sprite[] stage2Sprites;
    public Sprite[] stage3Sprites;
    public Sprite[] stage4Sprites;

    private void OnEnable()
    {
        UpdateLockState();

        if (StageSelect.instance != null)
            StageSelect.instance.TouchCPButtonEvent += OnCPTouch;

        StartCoroutine(ButtonDelay());
    }

    private IEnumerator ButtonDelay()
    {
        yield return null;
        Refresh();
    }

    private void OnDisable()
    {
        if (StageSelect.instance != null)
            StageSelect.instance.TouchCPButtonEvent -= OnCPTouch;
    }

    private void UpdateLockState()
    {
        bool isCleared = Checkpoint_Plane.IsCheckpointCleared(cpNum);
        isLock = !isCleared;

#if UNITY_EDITOR
        AssetDatabase.SaveAssets();
#endif
    }

    private void OnCPTouch(int cpSelect)
    {
        if (isLock) OnTouchCPButton(false);
        else OnTouchCPButton(cpSelect == cpNum);
    }

    public void OnClick()
    {
        if (StageSelect.instance != null)
            StageSelect.instance.SelectCP(StageNumber, cpNum);
        else
            OnTouchCPButton(true);
    }

    public void OnTouchCPButton(bool b)
    {
        if (isLock) return;

        isSelect = b;
        SelectImgCheck();
    }

    public void Refresh()
    {
        CurData = SaveManager.instance.curData;

        // ⭐⭐⭐ Panel 이미지 설정 (핵심 개선)
        if (panelImage != null)
        {
            Sprite[] targetArray = null;

            switch (StageNumber)
            {
                case 0: targetArray = stage1Sprites; break;
                case 1: targetArray = stage2Sprites; break;
                case 2: targetArray = stage3Sprites; break;
                case 3: targetArray = stage4Sprites; break;
            }

            if (targetArray != null && targetArray.Length > cpNum)
                panelImage.sprite = targetArray[cpNum];
        }

        // 🔽 Lock 상태 체크
        switch (StageNumber)
        {
            case 0:
                isLock = !CurData.ep1_open;
                break;

            case 1:
                switch (cpNum)
                {
                    case 0: isLock = !CurData.ep2_open; break;
                    case 1: isLock = !CurData.ep2_spaceClear; break;
                    case 2: isLock = !CurData.ep2_paintClear; break;
                }
                break;

            case 2:
                isLock = !CurData.ep3_open;
                break;

            case 3:
                switch (cpNum)
                {
                    case 0: isLock = !CurData.ep4_open; break;
                    case 1: isLock = !CurData.ep4_puzzle1Clear; break;
                    case 2: isLock = !CurData.ep4_puzzle2Clear; break;
                    case 3: isLock = !CurData.ep4_puzzle3Clear; break;
                }
                break;
        }

        if (CheckpointNumber != null)
            CheckpointNumber.text = $"{cpNum}";

        SelectImgCheck();
        LockImgCheck();
    }

    private void SelectImgCheck()
    {
        if (selectImage != null)
            selectImage.enabled = isSelect;
    }

    private void LockImgCheck()
    {
        if (lockImage != null)
            lockImage.enabled = isLock;
    }
}