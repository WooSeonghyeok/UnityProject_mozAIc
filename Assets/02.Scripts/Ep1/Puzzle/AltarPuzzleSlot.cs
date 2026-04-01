using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AltarPuzzleSlot : MonoBehaviour
{
    [Header("UI 참조")]
    public Button button;
    public Image iconImage;         // 자식 IconImage
    public Image backgroundImage;   // 버튼 본체 Image(별 모양 배경)

    [Header("슬롯 데이터")]
    public int slotIndex;
    public StarData slotStarData;
    public bool isPressed;

    private AltarPuzzleManager puzzleManager;

    /// <summary>
    /// 슬롯 초기화
    /// </summary>
    public void Init(AltarPuzzleManager manager, int index, StarData starData, bool showIcon)
    {
        puzzleManager = manager;
        slotIndex = index;
        slotStarData = starData;
        isPressed = false;

        // 버튼 다시 활성화
        if (button != null)
        {
            button.interactable = true;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClickSlot);

            // 버튼의 Target Graphic이 비어 있으면 자기 Image를 사용
            if (button.targetGraphic == null && backgroundImage != null)
            {
                button.targetGraphic = backgroundImage;
            }
        }

        // 배경 이미지는 항상 유지
        if (backgroundImage != null && slotStarData != null)
        {
            // 별 모양 슬롯의 색상 변경
            backgroundImage.color = slotStarData.starColor;

            // 버튼 클릭을 받으려면 배경은 raycastTarget 켜둠
            backgroundImage.raycastTarget = true;
        }

        // 아이콘은 클릭 막지 않게 설정
        if (iconImage != null)
        {
            iconImage.raycastTarget = false;

            if (showIcon && slotStarData != null && slotStarData.icon != null)
            {
                // 아이콘이 있으면 표시
                iconImage.sprite = slotStarData.icon;
                iconImage.color = Color.white;
                iconImage.enabled = true;
            }
            else
            {
                // 아이콘이 없으면 자식 아이콘만 숨김
                iconImage.sprite = null;
                iconImage.enabled = false;
            }
        }

        transform.localScale = Vector3.one;
        gameObject.SetActive(true);

        // 슬롯 활성화
        gameObject.SetActive(true);
        transform.localScale = Vector3.one;
    }

    /// <summary>
    /// 슬롯 클릭 처리
    /// </summary>
    private void OnClickSlot()
    {
        // 이미 눌린 슬롯은 무시
        if (isPressed)
            return;

        isPressed = true;

        if (puzzleManager != null)
            puzzleManager.OnSlotClicked(this);
    }

    /// <summary>
    /// 정답 클릭 시 연출
    /// </summary>
    public void SetSuccessVisual()
    {
        transform.localScale = Vector3.one * 1.08f;

        if (button != null)
            button.interactable = false;
    }

    /// <summary>
    /// 오답 클릭 시 연출
    /// </summary>
    public void SetFailVisual()
    {
        transform.localScale = Vector3.one * 0.92f;
    }

    /// <summary>
    /// 버튼 활성/비활성
    /// </summary>
    public void SetInteractable(bool value)
    {
        if (button != null)
            button.interactable = value;
    }
}