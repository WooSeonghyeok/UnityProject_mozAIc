using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [HideInInspector] public Vector2 moveInput;  // WASD 입력값을 저장할 변수 (x: 좌우, y: 상하)
    [HideInInspector] public Vector2 lookInput;  // 마우스 입력값 저장
    [HideInInspector] public bool isSprint;      // 달리기 여부
    [HideInInspector] public bool jumpTriggered; // 점프 입력이 들어왔는지 저장
    public event Action Interact;                // 상호작용 입력이 들어오면 이벤트를 발동
    public bool isJumpLock = false;              // 특정 연출 중 점프 동작이 막힘
    public bool isLookLock = false;              // 특정 연출 중 시선 동작이 막힘
    public CameraSwitcher cameraSwitcher; // 🔥 추가

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        if (isLookLock) return;
        lookInput = context.ReadValue<Vector2>();
    }
    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.started)
            isSprint = true;
        else if (context.canceled)
            isSprint = false;
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (isJumpLock) return;
        // 스페이스바를 눌렀을 때 1회성 점프 입력 발생
        if (context.started)
        {
            jumpTriggered = true;
        }
    }
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Interact?.Invoke();
        }
    }
    public void OnRightClick(InputAction.CallbackContext context)
    {
        // 🔥 우클릭 입력을 CameraSwitcher로 전달
        if (context.started)
        {
            if (cameraSwitcher != null)
            {
                cameraSwitcher.ToggleCamera();
            }
            else
            {
                Debug.LogWarning("CameraSwitcher 연결 안됨!");
            }
        }
    }
}