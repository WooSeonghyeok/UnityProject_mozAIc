using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class VcmChat : MonoBehaviour
{
    [Header("대화 전용 Virtual Camera")]
    [SerializeField] private CinemachineVirtualCamera chatVirtualCamera;

    [Header("평소 플레이어 Virtual Camera")]
    [SerializeField] private CinemachineVirtualCamera playerVirtualCamera;

    [Header("Priority 설정")]
    [SerializeField] private int normalPriority = 10;
    [SerializeField] private int chatPriority = 50;

    [Header("플레이어 렌더링 제외 설정")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask playerLayer;
    private int originCullingMask;      // 대화 전 원래 Culling Mask 저장용

    private void Awake()
    {
        // Main Camera 자동 탐색
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null)
        {
            originCullingMask = mainCamera.cullingMask;
        }
    }

    // 대화 시작 시 ChatCamPos 위치로 대화 카메라 전환
    public void StartChatCamera(Transform chatCamPos)
    {
        if (chatVirtualCamera == null || playerVirtualCamera == null)
        {
            Debug.LogWarning("[ChatVirtualCameraController] Virtual Camera 참조가 비어 있습니다.");
            return;
        }

        if (chatCamPos == null)
        {
            Debug.LogWarning("[ChatVirtualCameraController] ChatCamPos가 비어 있습니다.");
            return;
        }

        // ChatCamPos 위치와 회전을 대화 전용 카메라에 적용
        chatVirtualCamera.transform.position = chatCamPos.position;
        chatVirtualCamera.transform.rotation = chatCamPos.rotation;

        // 대화 카메라 우선순위를 올려서 CinemachineBrain이 선택하게 함
        chatVirtualCamera.Priority = chatPriority;
        playerVirtualCamera.Priority = normalPriority;

        // 대화 중에는 Player 레이어를 카메라 렌더링에서 제외
        if (mainCamera != null)
        {
            originCullingMask = mainCamera.cullingMask;
            mainCamera.cullingMask &= ~playerLayer.value;
        }
    }

    // 대화 종료 시 플레이어 카메라로 복귀
    public void EndChatCamera()
    {
        if (chatVirtualCamera == null || playerVirtualCamera == null)
            return;

        // 플레이어 카메라 우선순위를 다시 올림
        playerVirtualCamera.Priority = chatPriority;
        chatVirtualCamera.Priority = normalPriority;

        // 대화 종료 후 원래 Culling Mask 복구
        if (mainCamera != null)
        {
            mainCamera.cullingMask = originCullingMask;
        }
    }
}
