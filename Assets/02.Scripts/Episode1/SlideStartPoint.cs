using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideStartPoint : MonoBehaviour
{
    private readonly string playerTag = "Player";
    public enum SlideStartDirection
    {
        UsePointForward // 이 오브젝트의 forward 사용
    }
    [Header("슬라이드 시작 방향")]
    [SerializeField] private SlideStartDirection startDirection = SlideStartDirection.UsePointForward;

    [Header("NPC 퇴장 처리")]
    [SerializeField] private NPCFollower npcFollower;
    [SerializeField] private Transform npcExitWarpPoint;
    [SerializeField] private bool hideNpcOnSlideStart = true;

    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag(playerTag))  // 충돌한 대상의 태그가 "Player" 라면,
        {
            // PlayerMovement 스크립트 가져오기
            PlayerMovement pm = col.GetComponent<PlayerMovement>();

            // IceSlideRigidbody 스크립트 가져오기
            IceSlideRigidbody isr = col.GetComponent<IceSlideRigidbody>();

            // PlayerMovement가 존재하면 비활성화
            if (pm != null)
            {
                // 슬라이드 존 진입 순간 애니메이션을 Idle 전용 모드로 맞춤
                pm.EnterSlideZoneAnimationMode();
                pm.enabled = false;
            }

            // IceSlideRigidbody가 존재하면 활성화
            if (isr != null)
            {
                isr.enabled = true;
                isr.ResetInputPhase();

                // 플레이어 방향이 아니라 시작점이 정한 월드 방향으로 강제 시작
                Vector3 worldDir = GetWorldStartDirection();
                isr.StartInitialSlideInWorldDirection(worldDir);
            }

            // 슬라이딩 시작 순간 NPC 퇴장, 추적 중단
            if (hideNpcOnSlideStart && npcFollower != null && npcExitWarpPoint != null)
            {
                npcFollower.WarpTo(npcExitWarpPoint.position, npcExitWarpPoint.rotation);
                npcFollower.SetFollow(false);
            }
        }
    }
    /// 시작점이 사용할 월드 방향 반환
    private Vector3 GetWorldStartDirection()
    {
        switch (startDirection)
        {
            case SlideStartDirection.UsePointForward:
                return transform.forward;
        }

        return transform.forward;
    }
}
