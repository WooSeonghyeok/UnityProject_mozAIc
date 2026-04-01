using UnityEngine;

public class PlayerFallRecovery : MonoBehaviour
{
    [Header("초기 복귀 위치")]
    [SerializeField] private Transform fallbackSpawnPoint;

    [Header("복귀 위치 보정")]
    [SerializeField] private float recoverHeightOffset = 0.5f;

    [Header("실수 방지 설정")]
    [SerializeField] private int maxRecoveryCount = 1;

    // 현재 실제로 사용할 최근 안전 위치
    // 퍼즐 시작 직후에는 fallbackSpawnPoint를 기본값으로 사용하고,
    // 이후 정답 발판을 밟을 때마다 최신 안전 위치로 갱신된다.
    private Vector3 lastSafePosition;

    // 최근 안전 위치 복귀 시 함께 사용할 회전값
    private Quaternion lastSafeRotation;

    // 현재까지 사용한 복구 횟수
    private int usedRecoveryCount = 0;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // 시작 시에는 "초기 기본 복귀 위치"를 안전 위치로 사용한다.
        // 아직 정답 발판을 하나도 밟지 않았다면 이 위치로 복귀하게 된다.
        if (fallbackSpawnPoint != null)
        {
            lastSafePosition = fallbackSpawnPoint.position + Vector3.up * recoverHeightOffset;
            lastSafeRotation = fallbackSpawnPoint.rotation;
        }
        else
        {
            lastSafePosition = transform.position + Vector3.up * recoverHeightOffset;
            lastSafeRotation = transform.rotation;
        }
    }

    // 최근 안전 위치를 저장한다.
    // 정답 발판을 밟았을 때 호출해서,
    // 이후 낙하 시 "처음 위치"가 아니라 "가장 최근에 성공한 발판 근처"로 복귀하게 만든다.
    public void SaveSafePoint(Transform safePoint)
    {
        if (safePoint == null)
        {
            return;
        }

        lastSafePosition = safePoint.position + Vector3.up * recoverHeightOffset;
        lastSafeRotation = safePoint.rotation;
    }

    // 남은 구제 횟수가 있는지 확인한다.
    public bool CanRecover()
    {
        return usedRecoveryCount < maxRecoveryCount;
    }

    // 최근 안전 위치로 플레이어를 복귀시킨다.
    // 복귀 시 물리 속도도 함께 초기화해서 튕김이나 미끄러짐을 줄인다.
    public void RecoverToLastSafePoint()
    {
        usedRecoveryCount++;

        transform.position = lastSafePosition;
        transform.rotation = lastSafeRotation;

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    // 퍼즐 시작/재시작 시 복구 횟수를 초기화한다.
    // 최근 안전 위치 자체는 유지할 수도 있지만,
    // 현재 구조에서는 퍼즐 시작 시점에 다시 초기 위치가 기본값이 된다.
    public void ResetRecoveryCount()
    {
        usedRecoveryCount = 0;
    }

    // 외부에서 현재까지 사용한 구제 횟수를 보고 싶을 때 사용한다.
    public int GetUsedRecoveryCount()
    {
        return usedRecoveryCount;
    }

    // 현재 저장된 최근 안전 위치를 수동으로 초기 복귀 위치로 되돌리고 싶을 때 사용한다.
    // 퍼즐 시작 시점이나 강제 리셋 시 호출할 수 있다.
    public void ResetSafePointToFallback()
    {
        if (fallbackSpawnPoint != null)
        {
            lastSafePosition = fallbackSpawnPoint.position + Vector3.up * recoverHeightOffset;
            lastSafeRotation = fallbackSpawnPoint.rotation;
        }
        else
        {
            lastSafePosition = transform.position + Vector3.up * recoverHeightOffset;
            lastSafeRotation = transform.rotation;
        }
    }
}