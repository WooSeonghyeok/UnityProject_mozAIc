using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class PortalTeleport : MonoBehaviour
{
    [Header("포탈 설정")]
    public Transform destination;          // 이동할 목적지 포탈(또는 스폰 위치)
    public string playerTag = "Player";    // 플레이어 태그

    [Header("텔레포트 설정")]
    public float exitOffset = 5f;        // 목적지 도착 후 포탈 앞쪽으로 얼마나 띄울지
    public float reEnterDelay = 1f;      // 도착 직후 다시 포탈 타는 것 방지 시간

    [Header("카메라")]
    public CinemachineVirtualCamera virtualCam; // 따라오는 가상 카메라 연결

    [Header("컷씬 설정")]
    public bool useCutscene = false;            // 컷씬 사용 여부
    public bool requirePuzzleClear = false;     // 퍼즐 클리어 후에만 컷씬 실행할지
    public bool enterCaveCutscene = true;    // 동굴입장 컷씬인지
    public CutsceneImagePlayer cutscenePlayer;  // 컷씬 이미지 플레이어 연결

    private bool canTeleport = true;

    private void OnTriggerEnter(Collider other)
    {
        // 컷씬이 재생 중이면 순간이동 방지
        if (cutscenePlayer != null && cutscenePlayer.IsPlaying) return;

        // 현재 포탈이 텔레포트 가능한 상태가 아니면 무시
        if (!canTeleport) return;

        // 플레이어가 아니면 무시
        if (!other.CompareTag(playerTag)) return;

        // 목적지가 비어 있으면 무시
        if (destination == null) return;

        TeleportPlayer(other);
    }

    void TeleportPlayer(Collider player)
    {
        // 플레이어 위치 이동 전에 Rigidbody 가져오기
        Rigidbody rb = player.GetComponent<Rigidbody>();

        // Rigidbody가 있으면 순간이동 직전에 속도를 0으로 정리
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 목적지 포탈의 정면 방향으로 살짝 앞으로 떨어뜨려서
        // 바로 다시 Trigger에 닿지 않게 함
        Vector3 targetPos = destination.position + destination.forward * exitOffset;
        
        // 목적지 반대 방향을 보게 함
        Quaternion targetRot = destination.rotation;
        
        // 위치 이동
        player.transform.position = targetPos;

        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            // PlayerMovement의 yaw 값을 같이 바꿔야 카메라가 덮어쓰지 않음
            pm.SetLookRotation(targetRot);
        }
        else
        {
            player.transform.rotation = targetRot;
        }

        // Cinemachine 이전 상태 초기화
        if (virtualCam != null)
        {
            // 이전 프레임 기준 추적값을 버려서 순간이동 후 바로 새 위치를 따라가게 함
            virtualCam.PreviousStateIsValid = false;
        }

        // 목적지 포탈도 잠깐 비활성 상태로 만들어서
        // 도착 직후 다시 순간이동되는 현상을 막음
        PortalTeleport targetPortal = destination.GetComponent<PortalTeleport>();
        if (targetPortal != null)
        {
            targetPortal.StartCoroutine(targetPortal.DisableTeleportTemporarily());
        }

        // 현재 포탈도 잠깐 비활성 처리
        StartCoroutine(DisableTeleportTemporarily());

        // 컷씬 재생
        if (useCutscene && cutscenePlayer != null)
        {
            cutscenePlayer.PlayCutscene();
        }
        // 컷씬 조건 확인 후 재생
        TryPlayCutscene();
    }

    private void TryPlayCutscene()
    {
        if (!useCutscene || cutscenePlayer == null)
            return;

        // 퍼즐 클리어 후에만 재생해야 하는 포탈이면 검사
        if (requirePuzzleClear)
        {
            if (GameManager_Ep1.Instance == null)
                return;

            if (!GameManager_Ep1.Instance.isPuzzleCleared)
                return;
        }

        // 한 번만 재생하는 설정이면 중복 실행 방지
        if (enterCaveCutscene)
            return;

        cutscenePlayer.PlayCutscene();
    }
    IEnumerator DisableTeleportTemporarily()
    {
        canTeleport = false;
        yield return new WaitForSeconds(reEnterDelay);
        canTeleport = true;
    }
}
