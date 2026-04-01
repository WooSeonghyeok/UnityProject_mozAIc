using UnityEngine;

public class MissCollider : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private RhythmPuzzleManager puzzleManager;

    [Header("낙하 페널티")]
    [SerializeField] private bool registerMissOnFall = true;
    [SerializeField] private bool registerWrongOnFall = false;

    [Header("중복 감지 방지")]
    [SerializeField] private float triggerCooldown = 0.2f;

    [Header("복구 횟수 소진 시 처리")]
    [SerializeField] private bool restartPuzzleWhenRecoveryExhausted = true;

    private float lastTriggerTime = -999f;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    // MissCollider에 플레이어가 닿았을 때 호출된다.
    //
    // 처리 순서:
    // 1. Player 태그인지 확인
    // 2. 짧은 시간 안의 중복 낙하 감지 차단
    // 3. 낙하 페널티(미스/오답) 적용
    // 4. 복구 가능하면 최근 안전 위치로 복귀
    // 5. 복구 횟수를 다 썼으면 시작 위치로 되돌린 뒤 퍼즐 재시작
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        // 아주 짧은 시간 안에 같은 낙하가 여러 번 감지되는 것을 막는다.
        if (Time.time < lastTriggerTime + triggerCooldown)
        {
            return;
        }

        lastTriggerTime = Time.time;

        PlayerFallRecovery recovery = other.GetComponent<PlayerFallRecovery>();
        if (recovery == null)
        {
            Debug.LogWarning("[MissCollider] PlayerFallRecovery가 플레이어에 없습니다.");
            return;
        }

        // 낙하 시 점수 처리
        // 현재 구조에서는 미스 처리 또는 오답 처리 중 하나를 선택해서 사용한다.
        if (puzzleManager != null)
        {
            if (registerMissOnFall)
            {
                puzzleManager.RegisterMiss();
            }
            else if (registerWrongOnFall)
            {
                puzzleManager.RegisterWrongStep();
            }
        }

        // 남은 구제 횟수가 있으면 최근 안전 위치로 복귀
        if (recovery.CanRecover())
        {
            recovery.RecoverToLastSafePoint();
            return;
        }

        // 복구 횟수를 모두 사용했다면:
        // 1. 안전 위치를 시작 위치로 되돌리고
        // 2. 플레이어를 시작 위치로 복귀시키고
        // 3. 퍼즐을 처음부터 다시 시작한다.
        if (restartPuzzleWhenRecoveryExhausted)
        {
            recovery.ResetRecoveryCount();
            recovery.ResetSafePointToFallback();
            recovery.RecoverToLastSafePoint();

            if (puzzleManager != null)
            {
                puzzleManager.RestartPuzzleFromStart();
            }

            return;
        }

        // 재시작 옵션을 끈 경우에는 기존처럼 퍼즐 실패 처리
        if (puzzleManager != null)
        {
            puzzleManager.FailPuzzle();
        }
    }
}