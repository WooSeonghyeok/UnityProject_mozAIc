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

    // ⭐ 추가: Panel 이미지 (Source Image 바꿀 대상)
    public Image panelImage;

    // ⭐ 추가: Stage별 Sprite 데이터
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

        // ⭐⭐⭐ 핵심: Stage + CP 기준으로 Panel 이미지 변경 ⭐⭐⭐
        if (panelImage != null)
        {
            Sprite selectedSprite = null;

            switch (StageNumber)
            {
                case 0:
                    if (stage1Sprites != null && stage1Sprites.Length > 0)
                        selectedSprite = stage1Sprites[0];
                    break;

                case 1:
                    if (stage2Sprites != null && stage2Sprites.Length > 0)
                        selectedSprite = stage2Sprites[0];
                    break;

                case 2:
                    if (stage3Sprites != null && stage3Sprites.Length > 0)
                        selectedSprite = stage3Sprites[0];
                    break;

                case 3:
                    if (stage4Sprites != null && stage4Sprites.Length > cpNum)
                        selectedSprite = stage4Sprites[cpNum];
                    break;
            }

            if (selectedSprite != null)
                panelImage.sprite = selectedSprite;
        }

        // 🔽 기존 로직 그대로 유지
        switch (StageNumber)
        {
            case 0: isLock = !CurData.ep1_open; break;
            case 1:
                switch (cpNum)
                {
                    case 0: isLock = !CurData.ep2_open; break;          // 시작
                    case 1: isLock = !CurData.ep2_spaceClear; break;    // Space
                    case 2: isLock = !CurData.ep2_paintClear; break;    // Paint
                }
                break;
            case 2: isLock = !CurData.ep3_open; break;

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