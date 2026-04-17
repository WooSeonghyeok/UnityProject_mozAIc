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
    [SerializeField] private bool returnPlayerToFallbackBeforeFail = true;

    private float lastTriggerTime = -999f;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

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

        if (recovery.CanRecover())
        {
            recovery.RecoverToLastSafePoint();
            return;
        }

        recovery.ResetRecoveryCount();
        recovery.ResetSafePointToFallback();

        if (returnPlayerToFallbackBeforeFail)
        {
            recovery.RecoverToLastSafePoint();
            recovery.ResetRecoveryCount();
        }

        if (restartPuzzleWhenRecoveryExhausted)
        {
            if (puzzleManager != null)
            {
                puzzleManager.RestartPuzzleFromStart();
            }

            return;
        }

        if (puzzleManager != null)
        {
            puzzleManager.FailPuzzle();
        }
    }
}
