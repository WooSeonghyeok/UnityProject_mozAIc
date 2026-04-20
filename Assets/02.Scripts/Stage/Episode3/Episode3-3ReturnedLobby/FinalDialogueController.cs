using UnityEngine;

public class FinalDialogueController : MonoBehaviour
{
    [Header("출구 오브젝트")]
    [SerializeField] private GameObject exitObject;

    [Header("출구 콜라이더")]
    [SerializeField] private Collider exitCollider;

    [Header("시작할 때 출구 숨기기 여부")]
    [SerializeField] private bool hideExitObjectAtStart = false;

    // 컷씬/최종 이벤트 종료 여부
    public bool IsFinished { get; private set; } = false;

    private void Start()
    {
        // 시작 시 출구 오브젝트 자체를 숨기고 싶을 때
        if (hideExitObjectAtStart && exitObject != null)
        {
            exitObject.SetActive(false);
        }

        // 시작 시 출구 콜라이더 비활성화
        if (exitCollider != null)
        {
            exitCollider.enabled = false;
        }
    }

    /// <summary>
    /// 컷씬 또는 최종 이벤트가 끝났을 때 호출
    /// </summary>
    public void OnFinalDialogueEnd()
    {
        if (IsFinished) return;
        IsFinished = true;

        // 출구 오브젝트 다시 보이기
        if (hideExitObjectAtStart && exitObject != null)
        {
            exitObject.SetActive(true);
        }

        // 출구 콜라이더 열기
        if (exitCollider != null)
        {
            exitCollider.enabled = true;
        }

        Debug.Log("[FinalDialogueController] 최종 이벤트 종료. 출구가 열렸습니다.");
    }

    /// <summary>
    /// 필요 시 다시 잠그기
    /// </summary>
    public void LockExit()
    {
        IsFinished = false;

        if (hideExitObjectAtStart && exitObject != null)
        {
            exitObject.SetActive(false);
        }

        if (exitCollider != null)
        {
            exitCollider.enabled = false;
        }

        Debug.Log("[FinalDialogueController] 출구가 다시 잠겼습니다.");
    }
}