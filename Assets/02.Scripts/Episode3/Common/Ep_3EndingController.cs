using UnityEngine;

/// <summary>
/// 최종 엔딩 타입에 따라 어떤 엔딩 오브젝트를 보여줄지 제어하는 컨트롤러.
/// 
/// 현재 구조는 단순하다:
/// - True 엔딩이면 trueEndingObject 활성화
/// - Normal 엔딩이면 normalEndingObject 활성화
/// 
/// 이후 컷신, 타임라인, 대사 시스템과 연결할 경우
/// 이 클래스가 엔딩 진입 진입점 역할을 유지하면 확장하기 쉽다.
/// </summary>
public class Ep_3EndingController : MonoBehaviour
{
    [Header("엔딩 오브젝트")]
    [SerializeField] private GameObject trueEndingObject;
    [SerializeField] private GameObject normalEndingObject;

    /// <summary>
    /// 전달받은 엔딩 데이터에 맞춰 엔딩 오브젝트를 활성화한다.
    /// 
    /// 먼저 모든 엔딩 오브젝트를 끈 뒤,
    /// 현재 판정 결과에 해당하는 것만 켜는 방식으로 상태 꼬임을 방지한다.
    /// </summary>
    public void PlayEnding(Ep3EndingStateData endingData)
    {
        if (trueEndingObject != null)
        {
            trueEndingObject.SetActive(false);
        }

        if (normalEndingObject != null)
        {
            normalEndingObject.SetActive(false);
        }

        switch (endingData.endingType)
        {
            case Ep3EndingType.True:
                if (trueEndingObject != null)
                {
                    trueEndingObject.SetActive(true);
                }
                break;

            case Ep3EndingType.Normal:
                if (normalEndingObject != null)
                {
                    normalEndingObject.SetActive(true);
                }
                break;
        }

        Debug.Log($"[Ep_3EndingController] 엔딩 재생: {endingData.endingType}");
    }
}