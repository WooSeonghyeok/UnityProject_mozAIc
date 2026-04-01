using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerInput : MonoBehaviour
{
    [HideInInspector] public Vector2 moveInput;  // WASD 입력값을 저장할 변수 (x: 좌우, y: 상하)
    [HideInInspector] public Vector2 lookInput;  // 마우스 입력값 저장
    [HideInInspector] public bool isSprint;      // 달리기 여부
    [HideInInspector] public bool jumpTriggered; // 점프 입력이 들어왔는지 저장
    [HideInInspector] public event Action Interact;
    [HideInInspector] public bool isLookLock = false;  //시야 회전 잠금
    [HideInInspector] public bool isJumpLock = false;  //점프 잠금
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
}