using UnityEngine;
using Cinemachine;

public class CameraSwitcher : MonoBehaviour
{
    public CinemachineVirtualCamera thirdPersonCam; // 3인칭 카메라
    public CinemachineVirtualCamera firstPersonCam; // 1인칭 카메라

    public Transform playerCameraRoot; // 플레이어 카메라 기준 축

    public bool isFirstPerson = false; // 현재 카메라 상태


    public void ToggleCamera() // 🔥 public으로 변경
    {
        if (GameManager.Instance.isCutsceneMode) return;
        isFirstPerson = !isFirstPerson;

        if (isFirstPerson)
        {
            // 현재 카메라 방향 가져오기
            Vector3 forward = Camera.main.transform.forward;

            // 플레이어 방향을 카메라 방향으로 맞춤 (Y축 고정)
            playerCameraRoot.forward = new Vector3(forward.x, 0, forward.z);

            // 1인칭 카메라 활성화
            firstPersonCam.Priority = 20;
            thirdPersonCam.Priority = 10;
        }
        else
        {
            // 3인칭 카메라 활성화
            firstPersonCam.Priority = 10;
            thirdPersonCam.Priority = 20;
        }
    }
}