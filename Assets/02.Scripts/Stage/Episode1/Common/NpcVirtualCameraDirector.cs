using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Cinemachine Virtual Camera를 전환해서
// 잠깐 NPC 쪽을 비추고 다시 플레이어 카메라로 복귀시키는 스크립트
public class NpcVirtualCameraDirector : MonoBehaviour
{
    [Header("Virtual Camera")]
    [SerializeField] private GameObject playerVirtualCamera; // 평소 플레이어를 따라다니는 vcam
    [SerializeField] private GameObject npcVirtualCamera;    // NPC 연출용 vcam

    [Header("연출 설정")]
    [SerializeField] private float defaultFocusDuration = 3f;

    private Coroutine focusCoroutine;

    private void Start()
    {
        // 시작 시에는 플레이어 카메라만 활성화
        SetPlayerCameraActive();
    }

    // NPC 연출 카메라를 켜고 일정 시간 후 자동 복귀
    public void FocusOnNpc()
    {
        FocusOnNpc(defaultFocusDuration);
    }

    // NPC 연출 카메라를 켜고 지정 시간 후 자동 복귀
    public void FocusOnNpc(float duration)
    {
        if (focusCoroutine != null)
        {
            StopCoroutine(focusCoroutine);
        }

        focusCoroutine = StartCoroutine(FocusRoutine(duration));
    }

    // NPC 연출 카메라 즉시 활성화
    public void EnableNpcCamera()
    {
        // 플레이어용 vcam 비활성화
        if (playerVirtualCamera != null)
        {
            playerVirtualCamera.SetActive(false);
        }

        // NPC용 vcam 활성화
        if (npcVirtualCamera != null)
        {
            npcVirtualCamera.SetActive(true);
        }
    }

    // 플레이어 카메라 즉시 복귀
    public void ReturnToPlayerCamera()
    {
        if (focusCoroutine != null)
        {
            StopCoroutine(focusCoroutine);
            focusCoroutine = null;
        }

        SetPlayerCameraActive();
    }

    private IEnumerator FocusRoutine(float duration)
    {
        EnableNpcCamera();

        yield return new WaitForSeconds(duration);

        SetPlayerCameraActive();
        focusCoroutine = null;
    }

    private void SetPlayerCameraActive()
    {
        // NPC용 vcam 비활성화
        if (npcVirtualCamera != null)
        {
            npcVirtualCamera.SetActive(false);
        }

        // 플레이어용 vcam 활성화
        if (playerVirtualCamera != null)
        {
            playerVirtualCamera.SetActive(true);
        }
    }
}
