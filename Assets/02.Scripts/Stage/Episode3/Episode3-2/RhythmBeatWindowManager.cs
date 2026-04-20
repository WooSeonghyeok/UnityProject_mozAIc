using System.Collections.Generic;
using UnityEngine;

public class RhythmBeatWindowManager : MonoBehaviour
{
    // 하나의 비트에 대응하는 발판 그룹 정보
    //
    // beatIndex:
    // - 이 그룹이 비트맵 상 몇 번째 비트인지 식별하는 값
    //
    // platforms:
    // - 실제 플레이어가 밟을 수 있는 발판 컴포넌트 목록
    //
    // spawnedObjects:
    // - 풀링 반환 시 실제 오브젝트를 되돌리기 위해 보관하는 GameObject 목록
    //
    // targetPlatform:
    // - 이 비트에서 정답 처리되는 발판
    private class BeatPlatformGroup
    {
        public int beatIndex;
        public List<RhythmPlatform> platforms = new List<RhythmPlatform>();
        public List<GameObject> spawnedObjects = new List<GameObject>();
        public RhythmPlatform targetPlatform;
    }

    [Header("발판 스포너")]
    [SerializeField] private PlatformSpawner platformSpawner;

    [Header("장식 스포너")]
    [SerializeField] private DecoRandomSpawn decoRandomSpawn;

    [Header("순환 생성 설정")]
    [SerializeField] private int initialSpawnBeatCount = 3;
    [SerializeField] private int maximumSpawnBeatCount = 5;

    // 현재 살아 있는 비트 그룹들
    // 리스트 앞쪽일수록 오래된 그룹이며,
    // maximumSpawnBeatCount를 넘으면 앞에서부터 제거된다.
    private readonly List<BeatPlatformGroup> activeBeatPlatformGroups = new List<BeatPlatformGroup>();

    // 생성된 발판이 플레이어 입력을 다시 전달할 퍼즐 매니저
    // RhythmPlatform.Initialize(owner, beatIndex) 호출에 사용된다.
    private RhythmPuzzleManager ownerPuzzleManager;

    // 현재 퍼즐에서 사용 중인 비트맵 데이터
    private BeatMapData beatMapData;

    // 다음에 생성해야 할 비트 인덱스
    // 이미 스폰 시도를 끝낸 비트를 다시 만들지 않기 위해 증가시키며 관리한다.
    private int nextBeatIndexToSpawn = 0;

    // 현재 살아 있는 그룹이 하나라도 있는지 빠르게 확인하기 위한 프로퍼티
    // 퍼즐 시작 직후 초기 스폰 성공 여부 판단에 사용한다.
    public bool HasAnyActiveGroup => activeBeatPlatformGroups.Count > 0;

    private void Reset()
    {
        if (platformSpawner == null)
        {
            platformSpawner = GetComponent<PlatformSpawner>();
        }

        if (decoRandomSpawn == null)
        {
            decoRandomSpawn = GetComponentInChildren<DecoRandomSpawn>(true);
        }
    }

    // 퍼즐 시작 시 윈도우 매니저의 소유자와 데이터 소스를 연결한다.
    public void Initialize(RhythmPuzzleManager owner, BeatMapData sourceBeatMap)
    {
        ownerPuzzleManager = owner;
        beatMapData = sourceBeatMap;
        nextBeatIndexToSpawn = 0;
    }

    // 현재 윈도우 상태를 완전히 리셋한다.
    //
    // - 살아 있는 발판 그룹 제거
    // - 장식 제거
    // - owner/beatMap 참조 해제
    // - 다음 스폰 인덱스 초기화
    public void ResetState()
    {
        ClearAllSpawnedBeatGroups();
        ownerPuzzleManager = null;
        beatMapData = null;
        nextBeatIndexToSpawn = 0;
    }

    // 퍼즐 시작 시 필요한 초기 비트 그룹을 생성한다.
    //
    // 예:
    // - initialSpawnBeatCount = 3 이면 시작 시 3개의 비트 그룹을 먼저 준비한다.
    //
    // 주의:
    // - 생성 실패가 나더라도 전체 퍼즐이 멈추지 않도록 다음 비트로 넘어간다.
    // - successCount는 "실제로 생성 성공한 그룹 수"를 의미한다.
    public void SpawnInitialBeatGroups()
    {
        int totalBeatCount = GetTotalBeatCount();
        int targetSpawnCount = Mathf.Clamp(initialSpawnBeatCount, 1, Mathf.Max(1, maximumSpawnBeatCount));
        targetSpawnCount = Mathf.Min(targetSpawnCount, totalBeatCount);

        int successCount = 0;

        while (successCount < targetSpawnCount && nextBeatIndexToSpawn < totalBeatCount)
        {
            if (SpawnBeatGroup(nextBeatIndexToSpawn))
            {
                successCount++;
            }

            // 성공/실패와 관계없이 이 비트는 한 번 스폰 시도를 끝냈으므로 다음 인덱스로 넘긴다.
            nextBeatIndexToSpawn++;
        }
    }

    // 다음 비트 그룹이 필요할 때 하나를 추가로 생성한다.
    //
    // 동작 순서:
    // 1. 현재 유지 개수가 최대치 이상이면 가장 오래된 그룹부터 제거
    // 2. 아직 생성하지 않은 비트 인덱스 중 하나를 시도
    // 3. 생성 실패 시 경고를 남기고 다음 비트로 넘어감
    public void SpawnNextBeatGroupIfNeeded()
    {
        int maxKeepCount = Mathf.Max(1, maximumSpawnBeatCount);

        while (activeBeatPlatformGroups.Count >= maxKeepCount)
        {
            ReleaseOldestBeatGroup();
        }

        while (nextBeatIndexToSpawn < GetTotalBeatCount())
        {
            if (SpawnBeatGroup(nextBeatIndexToSpawn))
            {
                nextBeatIndexToSpawn++;
                return;
            }

            Debug.LogWarning($"[RhythmBeatWindowManager] {nextBeatIndexToSpawn}번째 비트는 스폰 실패로 건너뜁니다.");
            nextBeatIndexToSpawn++;
        }
    }

    // 특정 비트 인덱스의 그룹이 현재 윈도우 안에 존재하는지 확인한다.
    // 퍼즐 매니저가 "활성화 가능한 비트인가?"를 검사할 때 사용한다.
    public bool HasBeatGroup(int beatIndex)
    {
        return GetBeatPlatformGroup(beatIndex) != null;
    }

    // 특정 비트 그룹이 지금 꼭 필요할 때 직접 생성 여부를 보장하려고 시도한다.
    //
    // 사용 목적:
    // - 순차 스폰 과정에서 어떤 비트가 생성 실패로 건너뛰어졌더라도
    //   실제 활성화 시점에 한 번 더 생성 기회를 주기 위함이다.
    public bool EnsureBeatGroupExists(int beatIndex)
    {
        if (HasBeatGroup(beatIndex))
        {
            return true;
        }

        if (beatIndex < 0 || beatIndex >= GetTotalBeatCount())
        {
            return false;
        }

        int maxKeepCount = Mathf.Max(1, maximumSpawnBeatCount);

        while (activeBeatPlatformGroups.Count >= maxKeepCount)
        {
            ReleaseOldestBeatGroup();
        }

        bool success = SpawnBeatGroup(beatIndex);

        if (success)
        {
            // 직접 생성한 비트가 순차 스폰 인덱스보다 앞서 있으면,
            // 다음 순차 생성이 같은 비트를 다시 시도하지 않도록 보정한다.
            if (beatIndex >= nextBeatIndexToSpawn)
            {
                nextBeatIndexToSpawn = beatIndex + 1;
            }

            Debug.Log($"[RhythmBeatWindowManager] {beatIndex}번째 비트 직접 생성 성공");
            return true;
        }

        Debug.LogWarning($"[RhythmBeatWindowManager] {beatIndex}번째 비트 직접 생성 실패");
        return false;
    }

    // 특정 비트 그룹 안에 주어진 발판이 포함되는지 확인한다.
    //
    // 왜 필요한가?
    // - 플레이어가 현재 비트와 상관없는 다른 발판을 밟았을 수 있기 때문
    // - 현재 활성 비트 그룹에 속한 발판인지 먼저 확인한 뒤 판정해야 한다
    public bool ContainsPlatform(int beatIndex, RhythmPlatform platform)
    {
        if (platform == null)
        {
            return false;
        }

        BeatPlatformGroup group = GetBeatPlatformGroup(beatIndex);
        if (group == null)
        {
            return false;
        }

        return group.platforms.Contains(platform);
    }

    // 특정 비트의 정답 발판을 반환한다.
    //
    // 반환값:
    // - true: 정상적으로 정답 발판을 찾음
    // - false: 그룹이 없거나 targetPlatform이 비어 있음
    public bool TryGetTargetPlatform(int beatIndex, out RhythmPlatform targetPlatform)
    {
        targetPlatform = null;

        BeatPlatformGroup group = GetBeatPlatformGroup(beatIndex);
        if (group == null || group.targetPlatform == null)
        {
            return false;
        }

        targetPlatform = group.targetPlatform;
        return true;
    }

    // 현재 살아 있는 모든 비트 그룹의 정답 활성 상태를 끈다.
    //
    // 퍼즐 종료나 전체 초기화 시 발판 자체의 정답 활성 상태를 정리할 때 사용한다.
    public void ClearAllTargetStates()
    {
        for (int i = 0; i < activeBeatPlatformGroups.Count; i++)
        {
            BeatPlatformGroup group = activeBeatPlatformGroups[i];
            if (group.targetPlatform != null)
            {
                group.targetPlatform.SetActiveTarget(false);
            }
        }
    }

    // 현재 살아 있는 모든 비트 그룹의 정답 발판을 활성화한다.
    //
    // 사용 목적:
    // - 비트를 한 개씩 순차 생성하더라도,
    //   현재 윈도우에 살아 있는 각 비트는 자기 정답 발판을 계속 유지하게 하기 위함이다.
    public void ActivateAllCurrentTargets()
    {
        for (int i = 0; i < activeBeatPlatformGroups.Count; i++)
        {
            BeatPlatformGroup group = activeBeatPlatformGroups[i];
            if (group == null || group.targetPlatform == null)
            {
                continue;
            }

            group.targetPlatform.SetActiveTarget(true);
        }
    }

    // 현재 살아 있는 모든 정답 발판의 Transform 목록을 반환한다.
    //
    // 사용 목적:
    // - RhythmPuzzleManager가 현재 화면에 살아 있는 정답 발판 전체에
    //   정답 표시 이펙트를 다시 붙일 때 사용한다.
    public List<Transform> GetAllCurrentTargetPlatforms()
    {
        List<Transform> result = new List<Transform>();

        for (int i = 0; i < activeBeatPlatformGroups.Count; i++)
        {
            BeatPlatformGroup group = activeBeatPlatformGroups[i];
            if (group == null || group.targetPlatform == null)
            {
                continue;
            }

            result.Add(group.targetPlatform.transform);
        }

        return result;
    }

    // 현재 살아 있는 모든 비트 그룹을 제거한다.
    //
    // 제거 대상:
    // - 장식 스포너가 만든 장식들
    // - 플랫폼 스포너가 만든 발판들
    // - 내부 그룹 목록
    public void ClearAllSpawnedBeatGroups()
    {
        if (decoRandomSpawn != null)
        {
            decoRandomSpawn.ClearAllSpawnedObjects();
        }

        if (platformSpawner != null)
        {
            platformSpawner.ClearSpawnedPlatforms();
        }

        activeBeatPlatformGroups.Clear();
    }

    // 현재 비트맵에 들어 있는 전체 비트 수를 반환한다.
    // 데이터가 비어 있으면 0을 반환한다.
    private int GetTotalBeatCount()
    {
        if (beatMapData == null || beatMapData.beatEvents == null)
        {
            return 0;
        }

        return beatMapData.beatEvents.Count;
    }

    // 특정 비트 인덱스에 해당하는 발판 그룹을 실제로 생성한다.
    //
    // 생성 절차:
    // 1. BeatEvent를 읽는다.
    // 2. PlatformSpawner에 발판 생성 요청을 보낸다.
    // 3. 생성된 발판에 owner/beatIndex를 주입한다.
    // 4. 정답 발판을 결정한다.
    // 5. 필요하면 장식도 함께 스폰한다.
    private bool SpawnBeatGroup(int beatIndex)
    {
        if (platformSpawner == null || beatMapData == null || beatMapData.beatEvents == null)
        {
            return false;
        }

        BeatEvent beatEvent = beatMapData.beatEvents[beatIndex];
        PlatformSpawner.SpawnResult spawnResult = platformSpawner.SpawnPlatforms(beatEvent, beatIndex, false);

        if (spawnResult == null || spawnResult.platforms == null || spawnResult.platforms.Count == 0)
        {
            Debug.LogWarning($"[RhythmBeatWindowManager] {beatIndex}번째 비트 생성 실패");
            return false;
        }

        BeatPlatformGroup group = new BeatPlatformGroup();
        group.beatIndex = beatIndex;

        for (int i = 0; i < spawnResult.platforms.Count; i++)
        {
            RhythmPlatform platform = spawnResult.platforms[i];
            if (platform == null)
            {
                continue;
            }

            // 생성된 발판이 이후 밟힘 이벤트를 올바른 퍼즐 매니저로 전달할 수 있도록 owner를 주입한다.
            platform.Initialize(ownerPuzzleManager, beatIndex);
            platform.SetActiveTarget(false);
            group.platforms.Add(platform);
        }

        for (int i = 0; i < spawnResult.spawnedObjects.Count; i++)
        {
            GameObject spawnedObject = spawnResult.spawnedObjects[i];
            if (spawnedObject == null)
            {
                continue;
            }

            group.spawnedObjects.Add(spawnedObject);
        }

        // 발판 오브젝트는 생성됐더라도 RhythmPlatform 컴포넌트를 가진 유효 발판이 하나도 없으면 실패로 간주한다.
        if (group.platforms.Count == 0)
        {
            platformSpawner.ReleaseSpawnedObjects(group.spawnedObjects);
            return false;
        }

        // 스포너가 알려준 targetSpawnedIndex를 우선 사용한다.
        // 만약 어떤 이유로 인덱스가 유효하지 않다면 첫 번째 발판을 정답으로 사용해
        // "정답 발판이 없는 비트"가 생기지 않도록 방어한다.
        if (spawnResult.targetSpawnedIndex >= 0 && spawnResult.targetSpawnedIndex < group.platforms.Count)
        {
            group.targetPlatform = group.platforms[spawnResult.targetSpawnedIndex];
        }
        else
        {
            group.targetPlatform = group.platforms[0];
        }

        group.targetPlatform.SetActiveTarget(false);
        activeBeatPlatformGroups.Add(group);

        // 장식 스포너는 현재 비트 그룹 발판 위치를 기준으로 장식 연출을 붙인다.
        if (decoRandomSpawn != null)
        {
            decoRandomSpawn.SpawnForBeatGroup(beatIndex, group.platforms);
        }

        Debug.Log($"[RhythmBeatWindowManager] {beatIndex}번째 비트 생성 완료 / 발판 수: {group.platforms.Count}");
        return true;
    }

    // 현재 살아 있는 그룹들 중 특정 비트 인덱스에 해당하는 그룹을 찾는다.
    // 없으면 null 반환.
    private BeatPlatformGroup GetBeatPlatformGroup(int beatIndex)
    {
        for (int i = 0; i < activeBeatPlatformGroups.Count; i++)
        {
            BeatPlatformGroup group = activeBeatPlatformGroups[i];
            if (group.beatIndex == beatIndex)
            {
                return group;
            }
        }

        return null;
    }

    // 가장 오래된 비트 그룹 하나를 제거한다.
    //
    // 제거 대상:
    // - 정답 상태
    // - 해당 비트 장식
    // - 해당 비트 발판 오브젝트
    // - 내부 그룹 목록
    private void ReleaseOldestBeatGroup()
    {
        if (activeBeatPlatformGroups.Count == 0)
        {
            return;
        }

        BeatPlatformGroup oldestGroup = activeBeatPlatformGroups[0];

        if (oldestGroup.targetPlatform != null)
        {
            oldestGroup.targetPlatform.SetActiveTarget(false);
        }

        if (decoRandomSpawn != null)
        {
            decoRandomSpawn.ReleaseForBeatGroup(oldestGroup.beatIndex);
        }

        if (platformSpawner != null)
        {
            platformSpawner.ReleaseSpawnedObjects(oldestGroup.spawnedObjects);
        }

        activeBeatPlatformGroups.RemoveAt(0);

        Debug.Log($"[RhythmBeatWindowManager] {oldestGroup.beatIndex}번째 비트 풀 반환");
    }
}