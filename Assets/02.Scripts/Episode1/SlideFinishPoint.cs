using UnityEngine;
public class SlideFinishPoint : MonoBehaviour
{
    private readonly string playerTag = "Player";
    public BoxCollider slideFinishWall;
    public IceSlideRigidbody isr;
    private void OnTriggerEnter(Collider col)
    {
        if(col.CompareTag(playerTag))  // 충돌한 대상의 태그가 "Player" 라면,
        {
            // PlayerMovement 스크립트 가져오기
            PlayerMovement pm = col.GetComponent<PlayerMovement>();
            // IceSlideRigidbody 스크립트 가져오기
            IceSlideRigidbody isr = col.GetComponent<IceSlideRigidbody>();
            // 슬라이드 종료 순간의 현재 회전 저장
            Quaternion currentRotation = col.transform.rotation;
            if (isr != null)
            {
                // 슬라이딩 상태 완전히 초기화
                isr.ResetInputPhase();
                isr.enabled = false;
            }
            // PlayerMovement가 존재하면 활성화
            if (pm != null)
            {
                // PlayerMovement 내부 yaw 값을 현재 회전에 맞춰 동기화
                pm.SetLookRotation(currentRotation);
                // PlayerMovement를 다시 켜기 전에 Idle 상태로 복귀시킴
                pm.ExitSlideZoneAnimationMode();
                pm.enabled = true;
            }
            // slideFinishWall이 연결되어 있을 때만 활성화
            if (slideFinishWall != null)
            {
                slideFinishWall.enabled = true;
            }
            else
            {
                Debug.LogWarning("SlideFinishWall이 연결되지 않았습니다.");
            }
        }
    }
}
