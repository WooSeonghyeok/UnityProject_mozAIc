using UnityEngine;

/// <summary>
/// 리듬 퍼즐에서 플레이어가 실제로 밟게 되는 개별 발판 컴포넌트.
/// 
/// 이 클래스의 책임:
/// 1. 자신이 몇 번째 비트/몇 번째 발판인지 상태를 보관한다.
/// 2. 현재 정답 발판인지 여부를 기록한다.
/// 3. 플레이어가 밟았을 때 퍼즐 매니저에 이벤트를 전달한다.
/// 
/// 즉, 판정 자체를 수행하는 클래스가 아니라
/// "발판 단위 상태 보관 + 밟힘 알림" 역할에 가깝다.
/// </summary>
public class RhythmPlatform : MonoBehaviour
{
    [Header("기본 상태")]
    [SerializeField] private int platformIndex = -1;
    [SerializeField] private int beatIndex = -1;
    [SerializeField] private bool isActiveTarget = false;

    /// <summary>
    /// 이 발판이 속한 퍼즐 매니저.
    /// 플레이어가 발판을 밟았을 때 다시 퍼즐 쪽으로 이벤트를 전달하기 위해 보관한다.
    /// </summary>
    private RhythmPuzzleManager puzzleManager;

    /// <summary>
    /// 현재 비트 그룹 안에서 몇 번째 발판인지 나타내는 인덱스.
    /// </summary>
    public int PlatformIndex => platformIndex;

    /// <summary>
    /// 이 발판이 몇 번째 비트에 속해 있는지 나타내는 인덱스.
    /// </summary>
    public int BeatIndex => beatIndex;

    /// <summary>
    /// 현재 이 발판이 정답 발판으로 활성화되어 있는지 여부.
    /// </summary>
    public bool IsActiveTarget => isActiveTarget;

    /// <summary>
    /// 발판 초기화.
    /// 
    /// manager:
    /// - 밟힘 이벤트를 다시 전달할 퍼즐 매니저
    /// 
    /// ownerBeatIndex:
    /// - 이 발판이 속한 비트 인덱스
    /// 
    /// 주의:
    /// - 생성 직후에는 항상 정답 활성 상태를 false로 리셋한다.
    /// - 정답 여부는 이후 퍼즐 진행 타이밍에 맞춰 별도로 켜진다.
    /// </summary>
    public void Initialize(RhythmPuzzleManager manager, int ownerBeatIndex)
    {
        puzzleManager = manager;
        beatIndex = ownerBeatIndex;
        isActiveTarget = false;
    }

    /// <summary>
    /// 스포너가 생성 순서에 맞춰 발판 인덱스를 설정한다.
    /// 
    /// 이 인덱스는 비트 그룹 내부에서의 논리적 순서를 의미하며,
    /// 실제 위치와 반드시 일치하는 것은 아니다.
    /// </summary>
    public void SetPlatformIndex(int index)
    {
        platformIndex = index;
    }

    /// <summary>
    /// 현재 발판이 정답 발판인지 여부를 기록한다.
    /// 
    /// 여기서는 상태만 바꾸고,
    /// 실제 정답 표시 이펙트 노출/숨김은 RhythmEffectManager가 담당한다.
    /// 역할을 분리해 두어 발판 클래스가 연출까지 직접 맡지 않게 한다.
    /// </summary>
    public void SetActiveTarget(bool value)
    {
        isActiveTarget = value;
    }

    /// <summary>
    /// 플레이어가 발판 트리거에 진입했을 때 호출된다.
    /// 
    /// 처리 규칙:
    /// - Player 태그가 아닌 오브젝트는 무시
    /// - 퍼즐 매니저가 연결되지 않았으면 무시
    /// - 조건을 만족하면 현재 발판 자신을 퍼즐 매니저에 전달
    /// 
    /// 실제 정답/오답/타이밍 판정은 여기서 하지 않고
    /// RhythmPuzzleManager가 현재 비트 상태를 기준으로 판단한다.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (puzzleManager == null) return;

        puzzleManager.OnPlatformStepped(this);
    }
}