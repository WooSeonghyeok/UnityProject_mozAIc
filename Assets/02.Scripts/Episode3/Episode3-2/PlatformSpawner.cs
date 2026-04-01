using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 비트 이벤트 정보를 바탕으로 실제 발판 오브젝트를 생성/배치/반환하는 스포너.
/// 
/// 이 클래스의 핵심 책임:
/// 1. BeatEvent에 맞는 발판 개수와 정답 발판을 결정한다.
/// 2. 발판 프리팹을 풀에서 꺼내거나 새로 생성한다.
/// 3. 거리 검사와 콜라이더 겹침 검사를 통해 안전한 위치를 찾는다.
/// 4. 생성이 끝난 발판을 다시 풀로 반환한다.
/// 
/// 즉, "발판을 어디에 어떻게 만들 것인가"를 담당하는 런타임 스폰 시스템이다.
/// </summary>
public class PlatformSpawner : MonoBehaviour
{
    /// <summary>
    /// 한 번의 비트 스폰 결과를 외부 매니저에 전달하기 위한 묶음 데이터.
    /// 
    /// beatIndex:
    /// - 몇 번째 비트에 대한 스폰 결과인지 식별용
    /// 
    /// platforms:
    /// - 실제로 유효하게 생성된 RhythmPlatform 목록
    /// 
    /// spawnedObjects:
    /// - 풀 반환 시 다시 돌려줄 실제 GameObject 목록
    /// 
    /// targetSpawnedIndex:
    /// - 이번 스폰 결과 목록 안에서 정답 발판이 몇 번째인지
    /// </summary>
    [System.Serializable]
    public class SpawnResult
    {
        public int beatIndex = -1;
        public List<RhythmPlatform> platforms = new List<RhythmPlatform>();
        public List<GameObject> spawnedObjects = new List<GameObject>();
        public int targetSpawnedIndex = -1;
    }

    [Header("발판 프리팹 목록")]
    [SerializeField] private GameObject[] platformPrefabs;

    [Header("생성 부모")] // 발판 오브젝트들이 생성될 때 부모로 삼을 트랜스폼. null이면 이 스포너 오브젝트가 부모가 된다.
    [SerializeField] private Transform Platforms;

    [Header("기본 회전")]
    [SerializeField] private Vector3 platformRotationEuler = new Vector3(90f, 0f, 0f);

    [Header("비트 간격")]
    [SerializeField] private float beatSpacing = 4f;

    [Header("겹침 방지 설정")]
    [SerializeField] private float minPlatformDistance = 1.5f;
    [SerializeField] private int maxSpawnPositionRetryCount = 10;
    [SerializeField] private bool useColliderOverlapCheck = true;
    [SerializeField] private float colliderBoundsPadding = 0.05f;

    /// <summary>
    /// 프리팹 인덱스별 오브젝트 풀.
    /// 같은 프리팹을 반복 생성/삭제하지 않고 재사용하기 위해 사용한다.
    /// </summary>
    private readonly Dictionary<int, Queue<GameObject>> pooledObjectsByPrefabIndex = new Dictionary<int, Queue<GameObject>>();

    /// <summary>
    /// 현재 활성 오브젝트가 어떤 프리팹 인덱스에서 나왔는지 추적하기 위한 맵.
    /// 반환 시 어느 풀로 돌려보낼지 알기 위해 필요하다.
    /// </summary>
    private readonly Dictionary<GameObject, int> activeObjectPrefabIndexMap = new Dictionary<GameObject, int>();

    /// <summary>
    /// 현재 씬에서 살아 있는 발판 오브젝트 집합.
    /// 전체 활성 발판과의 콜라이더 겹침 검사를 할 때 사용한다.
    /// </summary>
    private readonly HashSet<GameObject> activeSpawnedObjects = new HashSet<GameObject>();

    /// <summary>
    /// 하나의 비트에 필요한 발판들을 실제로 생성한다.
    /// 
    /// clearPrevious:
    /// - true이면 기존 활성 발판을 전부 지우고 새로 시작
    /// - false이면 기존 활성 발판은 유지한 채 현재 비트 발판만 추가
    /// 
    /// 처리 흐름:
    /// 1. 유효성 검사
    /// 2. 정답 발판 우선 생성
    /// 3. 추가 발판을 남은 오프셋에서 랜덤 생성
    /// 4. 결과를 SpawnResult로 반환
    /// </summary>
    public SpawnResult SpawnPlatforms(BeatEvent beatEvent, int beatSequenceIndex, bool clearPrevious = false)
    {
        if (clearPrevious)
        {
            ClearSpawnedPlatforms();
        }

        SpawnResult result = new SpawnResult();
        result.beatIndex = beatSequenceIndex;

        if (beatEvent == null)
        {
            Debug.LogWarning("[PlatformSpawner] BeatEvent가 null입니다.");
            return result;
        }

        if (platformPrefabs == null || platformPrefabs.Length == 0)
        {
            Debug.LogWarning("[PlatformSpawner] platformPrefabs가 비어 있습니다.");
            return result;
        }

        if (beatEvent.platformOffsets == null || beatEvent.platformOffsets.Count == 0)
        {
            Debug.LogWarning("[PlatformSpawner] platformOffsets가 비어 있습니다.");
            return result;
        }

        if (beatEvent.targetPlatformIndex < 0 || beatEvent.targetPlatformIndex >= beatEvent.platformOffsets.Count)
        {
            Debug.LogWarning(
                $"[PlatformSpawner] targetPlatformIndex={beatEvent.targetPlatformIndex}, platformOffsets.Count={beatEvent.platformOffsets.Count}");
            return result;
        }

        // 비트 데이터가 요구하는 최소/최대 발판 수를 안전 범위로 보정한다.
        // 현재 구조는 최대 3개 후보 위치를 기본 전제로 두고 있어 1~3 범위로 클램프한다.
        int minCount = Mathf.Clamp(beatEvent.minPlatformCount, 1, 3);
        int maxCount = Mathf.Clamp(beatEvent.maxPlatformCount, minCount, 3);

        // 실제 이번 비트에서 몇 개를 만들지 랜덤으로 결정한다.
        // 단, 후보 위치 개수보다 더 많이 만들 수는 없으므로 다시 한 번 상한을 적용한다.
        int spawnCount = Random.Range(minCount, maxCount + 1);
        spawnCount = Mathf.Min(spawnCount, beatEvent.platformOffsets.Count);

        Transform spawnParent = Platforms != null ? Platforms : transform;
        Quaternion spawnRotation = transform.rotation * Quaternion.Euler(platformRotationEuler);

        // 비트 인덱스에 따라 전방으로 일정 간격씩 이동한 위치를 기본 시작점으로 삼는다.
        Vector3 beatBasePosition = transform.position + Vector3.forward * beatSequenceIndex * beatSpacing;

        // 같은 비트 안에서 이미 사용한 위치를 기록해 거리 중복을 줄인다.
        List<Vector3> usedPositions = new List<Vector3>();

        // 아직 사용하지 않은 후보 오프셋 인덱스 목록.
        // 정답 발판 생성 후에는 해당 인덱스를 제거하고 남은 위치만 사용한다.
        List<int> remainingOffsetIndices = new List<int>();
        for (int i = 0; i < beatEvent.platformOffsets.Count; i++)
        {
            remainingOffsetIndices.Add(i);
        }

        // 정답 발판은 beatEvent가 지정한 targetPlatformIndex를 우선 사용한다.
        // 다만 해당 위치가 겹침 때문에 실패할 수 있으므로, 실패 시 다른 오프셋도 fallback으로 시도한다.
        List<int> targetOffsetOrder = BuildTargetOffsetOrder(beatEvent.targetPlatformIndex, remainingOffsetIndices);
        bool targetSpawned = TrySpawnSinglePlatform(
            logicalPlatformIndex: 0,
            isTargetPlatform: true,
            beatEvent: beatEvent,
            beatBasePosition: beatBasePosition,
            spawnParent: spawnParent,
            spawnRotation: spawnRotation,
            candidateOffsetIndices: targetOffsetOrder,
            usedPositions: usedPositions,
            remainingOffsetIndices: remainingOffsetIndices,
            result: result);

        // 정답 발판이 끝내 생성되지 못하면 이 비트는 유효한 퍼즐 진행이 불가능하므로 실패로 간주한다.
        // 이미 일부 오브젝트가 생성되었다면 먼저 정리한 뒤 빈 결과를 반환한다.
        if (!targetSpawned)
        {
            ReleaseSpawnedObjects(result.spawnedObjects);
            result.spawnedObjects.Clear();
            result.platforms.Clear();
            result.targetSpawnedIndex = -1;

            Debug.LogWarning($"[PlatformSpawner] {beatSequenceIndex}번째 비트의 정답 발판 생성에 실패했습니다.");
            return result;
        }

        // 정답 발판 외의 추가 발판 생성.
        // 남은 오프셋 중 무작위 순서를 섞어서 시도한다.
        for (int i = 1; i < spawnCount; i++)
        {
            if (remainingOffsetIndices.Count == 0)
            {
                break;
            }

            List<int> offsetOrder = new List<int>(remainingOffsetIndices);
            Shuffle(offsetOrder);

            TrySpawnSinglePlatform(
                logicalPlatformIndex: i,
                isTargetPlatform: false,
                beatEvent: beatEvent,
                beatBasePosition: beatBasePosition,
                spawnParent: spawnParent,
                spawnRotation: spawnRotation,
                candidateOffsetIndices: offsetOrder,
                usedPositions: usedPositions,
                remainingOffsetIndices: remainingOffsetIndices,
                result: result);
        }

        return result;
    }

    /// <summary>
    /// 외부에서 전달받은 생성 오브젝트 목록을 전부 풀로 반환한다.
    /// </summary>
    public void ReleaseSpawnedObjects(List<GameObject> spawnedObjects)
    {
        if (spawnedObjects == null)
        {
            return;
        }

        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            ReleaseSpawnedObject(spawnedObjects[i]);
        }
    }

    /// <summary>
    /// 현재 활성 상태로 남아 있는 발판을 전부 풀로 반환한다.
    /// 
    /// 주의:
    /// activeSpawnedObjects를 직접 순회하면서 제거하면 컬렉션 수정 문제가 생길 수 있으므로
    /// 별도 리스트로 복사한 뒤 안전하게 반환한다.
    /// </summary>
    public void ClearSpawnedPlatforms()
    {
        if (activeSpawnedObjects.Count == 0)
        {
            return;
        }

        List<GameObject> releaseTargets = new List<GameObject>(activeSpawnedObjects);
        for (int i = 0; i < releaseTargets.Count; i++)
        {
            ReleaseSpawnedObject(releaseTargets[i]);
        }
    }

    /// <summary>
    /// 발판 하나를 생성/배치하는 내부 루틴.
    /// 
    /// logicalPlatformIndex:
    /// - 현재 비트 안에서 몇 번째 발판인지 논리 인덱스
    /// 
    /// isTargetPlatform:
    /// - 이 발판이 정답 발판인지 여부
    /// 
    /// candidateOffsetIndices:
    /// - 이번 발판이 시도할 수 있는 후보 오프셋 순서
    /// 
    /// 실패 시:
    /// - 생성 오브젝트를 즉시 풀로 반환하고 false 반환
    /// </summary>
    private bool TrySpawnSinglePlatform(
        int logicalPlatformIndex,
        bool isTargetPlatform,
        BeatEvent beatEvent,
        Vector3 beatBasePosition,
        Transform spawnParent,
        Quaternion spawnRotation,
        List<int> candidateOffsetIndices,
        List<Vector3> usedPositions,
        List<int> remainingOffsetIndices,
        SpawnResult result)
    {
        if (candidateOffsetIndices == null || candidateOffsetIndices.Count == 0)
        {
            return false;
        }

        int prefabIndex = Random.Range(0, platformPrefabs.Length);
        GameObject obj = GetOrCreatePlatformObject(prefabIndex);
        if (obj == null)
        {
            return false;
        }

        bool placedSuccessfully = TryPlacePlatformObject(
            platformObject: obj,
            spawnParent: spawnParent,
            spawnRotation: spawnRotation,
            beatEvent: beatEvent,
            beatBasePosition: beatBasePosition,
            candidateOffsetIndices: candidateOffsetIndices,
            usedPositions: usedPositions,
            out int selectedOffsetIndex,
            out Vector3 finalSpawnPosition);

        if (!placedSuccessfully)
        {
            ReleaseSpawnedObject(obj);
            return false;
        }

        RhythmPlatform platform = obj.GetComponent<RhythmPlatform>();
        if (platform == null)
        {
            platform = obj.GetComponentInChildren<RhythmPlatform>(true);
        }

        if (platform == null)
        {
            Debug.LogWarning($"[PlatformSpawner] '{platformPrefabs[prefabIndex].name}' 프리팹에서 RhythmPlatform 컴포넌트를 찾지 못했습니다.");
            ReleaseSpawnedObject(obj);
            return false;
        }

        platform.SetPlatformIndex(logicalPlatformIndex);
        platform.SetActiveTarget(false);

        // 위치/활성 상태 등록은 "실제로 유효한 플랫폼 컴포넌트를 확인한 뒤"에만 수행한다.
        // 그래야 실패한 오브젝트가 활성 목록에 잘못 남지 않는다.
        usedPositions.Add(finalSpawnPosition);
        activeSpawnedObjects.Add(obj);
        remainingOffsetIndices.Remove(selectedOffsetIndex);

        result.spawnedObjects.Add(obj);
        result.platforms.Add(platform);

        if (isTargetPlatform)
        {
            result.targetSpawnedIndex = result.platforms.Count - 1;
        }

        return true;
    }

    /// <summary>
    /// 정답 발판이 우선 시도할 오프셋 순서를 만든다.
    /// 
    /// 규칙:
    /// 1. 지정된 targetPlatformIndex를 가장 먼저 시도
    /// 2. 실패 시 나머지 오프셋을 랜덤 fallback 순서로 시도
    /// 
    /// 이 방식은 "원래 의도한 위치를 최대한 지키되, 생성 실패로 게임이 멈추는 상황은 피한다"는 의도다.
    /// </summary>
    private List<int> BuildTargetOffsetOrder(int preferredTargetOffsetIndex, List<int> remainingOffsetIndices)
    {
        List<int> offsetOrder = new List<int>();
        offsetOrder.Add(preferredTargetOffsetIndex);

        List<int> fallbackOffsets = new List<int>();
        for (int i = 0; i < remainingOffsetIndices.Count; i++)
        {
            int offsetIndex = remainingOffsetIndices[i];
            if (offsetIndex == preferredTargetOffsetIndex)
            {
                continue;
            }

            fallbackOffsets.Add(offsetIndex);
        }

        Shuffle(fallbackOffsets);
        offsetOrder.AddRange(fallbackOffsets);

        return offsetOrder;
    }

    /// <summary>
    /// 하나의 발판 오브젝트를 "어느 오프셋에 둘지" 결정하는 상위 배치 루틴.
    /// 
    /// 여러 후보 오프셋을 순서대로 시도하고,
    /// 성공한 첫 위치를 선택한다.
    /// </summary>
    private bool TryPlacePlatformObject(
        GameObject platformObject,
        Transform spawnParent,
        Quaternion spawnRotation,
        BeatEvent beatEvent,
        Vector3 beatBasePosition,
        List<int> candidateOffsetIndices,
        List<Vector3> usedPositions,
        out int selectedOffsetIndex,
        out Vector3 finalSpawnPosition)
    {
        selectedOffsetIndex = -1;
        finalSpawnPosition = beatBasePosition;

        for (int i = 0; i < candidateOffsetIndices.Count; i++)
        {
            int offsetIndex = candidateOffsetIndices[i];
            Vector2 baseOffset = beatEvent.platformOffsets[offsetIndex];

            if (TryPlacePlatformObjectAtOffset(
                platformObject,
                spawnParent,
                spawnRotation,
                beatEvent,
                beatBasePosition,
                baseOffset,
                usedPositions,
                out finalSpawnPosition))
            {
                selectedOffsetIndex = offsetIndex;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 특정 오프셋 위치 하나에 대해 실제 배치를 시도한다.
    /// 
    /// 처리 순서:
    /// 1. 랜덤 오프셋 포함 후보 위치 계산
    /// 2. 같은 비트 내부 최소 거리 검사
    /// 3. 전체 활성 발판과 콜라이더 겹침 검사
    /// 4. 성공하면 해당 위치 사용
    /// 5. 여러 번 실패하면 랜덤 없이 baseOffset 원위치도 마지막으로 시도
    /// 
    /// 즉, "최대한 자연스럽게 흩뿌리되, 끝까지 안 되면 기본 위치라도 써본다"는 정책이다.
    /// </summary>
    private bool TryPlacePlatformObjectAtOffset(
        GameObject platformObject,
        Transform spawnParent,
        Quaternion spawnRotation,
        BeatEvent beatEvent,
        Vector3 beatBasePosition,
        Vector2 baseOffset,
        List<Vector3> usedPositions,
        out Vector3 finalSpawnPosition)
    {
        finalSpawnPosition =
            beatBasePosition +
            Vector3.right * baseOffset.x +
            Vector3.up * baseOffset.y;

        int retryCount = Mathf.Max(1, maxSpawnPositionRetryCount);

        for (int attempt = 0; attempt < retryCount; attempt++)
        {
            float randomX = Random.Range(-beatEvent.randomOffsetX, beatEvent.randomOffsetX);
            float randomY = Random.Range(-beatEvent.randomOffsetY, beatEvent.randomOffsetY);

            Vector3 candidatePosition =
                beatBasePosition +
                Vector3.right * (baseOffset.x + randomX) +
                Vector3.up * (baseOffset.y + randomY);

            if (!IsFarEnough(candidatePosition, usedPositions))
            {
                continue;
            }

            // 배치 적용(활성화)
            // 겹침이 나더라도 같은 오브젝트를 다른 위치에 다시 시도할 수 있어야 하므로
            // 여기서는 풀 반환하지 않고 비활성화만 한다.
            ApplySpawnTransform(platformObject, spawnParent, candidatePosition, spawnRotation);

            if (useColliderOverlapCheck && HasAnyColliderOverlap(platformObject))
            {
                // 이번 후보 위치만 실패.
                // 같은 오브젝트로 다음 후보 위치를 다시 시도하기 위해 비활성화만 한다.
                platformObject.SetActive(false);
                continue;
            }

            finalSpawnPosition = candidatePosition;
            return true;
        }

        // 랜덤 시도에 모두 실패한 경우, 기본 오프셋 위치라도 한 번 더 써본다.
        Vector3 fallbackPosition =
            beatBasePosition +
            Vector3.right * baseOffset.x +
            Vector3.up * baseOffset.y;

        if (IsFarEnough(fallbackPosition, usedPositions))
        {
            ApplySpawnTransform(platformObject, spawnParent, fallbackPosition, spawnRotation);

            if (!useColliderOverlapCheck || !HasAnyColliderOverlap(platformObject))
            {
                finalSpawnPosition = fallbackPosition;
                return true;
            }

            // fallback도 실패하면 이번 오브젝트는 아직 결과에 등록되지 않은 상태이므로
            // 풀 반환하지 않고 비활성화만 한다.
            platformObject.SetActive(false);
        }

        return false;
    }

    /// <summary>
    /// 오브젝트를 실제 스폰 위치와 회전으로 배치하고 활성화한다.
    /// 
    /// Physics.SyncTransforms()를 호출하는 이유:
    /// - 직후 콜라이더 bounds 검사 시 최신 Transform 정보가 반영되도록 하기 위함
    /// </summary>
    private void ApplySpawnTransform(GameObject platformObject, Transform spawnParent, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        platformObject.transform.SetParent(spawnParent, false);
        platformObject.transform.position = spawnPosition;
        platformObject.transform.rotation = spawnRotation;
        platformObject.SetActive(true);
        Physics.SyncTransforms();
    }

    /// <summary>
    /// 후보 발판이 현재 활성 발판들과 콜라이더 bounds 기준으로 겹치는지 검사한다.
    /// 
    /// 주의:
    /// - 정밀한 MeshCollider 삼각형 충돌이 아니라 Bounds 기반 단순 교차 검사다.
    /// - 그러나 퍼즐 발판 겹침 방지 용도에는 충분히 실용적이고 성능 부담도 적다.
    /// </summary>
    private bool HasAnyColliderOverlap(GameObject candidateObject)
    {
        Collider[] candidateColliders = candidateObject.GetComponentsInChildren<Collider>(true);
        if (candidateColliders == null || candidateColliders.Length == 0)
        {
            return false;
        }

        foreach (GameObject activeObject in activeSpawnedObjects)
        {
            if (activeObject == null || activeObject == candidateObject)
            {
                continue;
            }

            Collider[] activeColliders = activeObject.GetComponentsInChildren<Collider>(true);
            if (activeColliders == null || activeColliders.Length == 0)
            {
                continue;
            }

            for (int i = 0; i < candidateColliders.Length; i++)
            {
                Collider candidateCollider = candidateColliders[i];
                if (candidateCollider == null || !candidateCollider.enabled)
                {
                    continue;
                }

                Bounds candidateBounds = candidateCollider.bounds;
                candidateBounds.Expand(colliderBoundsPadding);

                for (int j = 0; j < activeColliders.Length; j++)
                {
                    Collider activeCollider = activeColliders[j];
                    if (activeCollider == null || !activeCollider.enabled)
                    {
                        continue;
                    }

                    Bounds activeBounds = activeCollider.bounds;
                    activeBounds.Expand(colliderBoundsPadding);

                    if (candidateBounds.Intersects(activeBounds))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 같은 비트 내부에서 이미 사용한 위치와 최소 거리 이상 떨어져 있는지 검사한다.
    /// 
    /// 이 검사는 "같은 비트 안에서 발판끼리 너무 붙는 문제"를 빠르게 막는 1차 방어다.
    /// 이후 전체 활성 발판과의 콜라이더 검사로 2차 방어를 수행한다.
    /// </summary>
    private bool IsFarEnough(Vector3 candidatePosition, List<Vector3> usedPositions)
    {
        float minimumDistance = Mathf.Max(0f, minPlatformDistance);
        float minimumDistanceSqr = minimumDistance * minimumDistance;

        for (int i = 0; i < usedPositions.Count; i++)
        {
            Vector3 delta = candidatePosition - usedPositions[i];
            if (delta.sqrMagnitude < minimumDistanceSqr)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 풀에서 오브젝트를 가져오거나, 없으면 새로 생성한다.
    /// 
    /// 반환 시점에는 아직 실제 위치/부모 배치를 하지 않고 비활성 상태로 돌려준다.
    /// 이후 배치 성공 시 ApplySpawnTransform에서 활성화된다.
    /// </summary>
    private GameObject GetOrCreatePlatformObject(int prefabIndex)
    {
        Queue<GameObject> pool = GetPool(prefabIndex);
        GameObject platformObject = DequeueValidObject(pool);

        if (platformObject == null)
        {
            GameObject prefab = platformPrefabs[prefabIndex];
            platformObject = Instantiate(prefab, transform);
        }

        activeObjectPrefabIndexMap[platformObject] = prefabIndex;
        platformObject.SetActive(false);

        return platformObject;
    }

    /// <summary>
    /// 발판 오브젝트 하나를 비활성화하고 원래 풀로 반환한다.
    /// 
    /// 만약 어떤 이유로 prefabIndex 추적 정보가 없으면
    /// 일단 비활성화/부모 복귀만 하고 종료한다.
    /// </summary>
    private void ReleaseSpawnedObject(GameObject platformObject)
    {
        if (platformObject == null)
        {
            return;
        }

        activeSpawnedObjects.Remove(platformObject);

        if (!activeObjectPrefabIndexMap.TryGetValue(platformObject, out int prefabIndex))
        {
            platformObject.SetActive(false);
            platformObject.transform.SetParent(transform, false);
            return;
        }

        activeObjectPrefabIndexMap.Remove(platformObject);

        platformObject.SetActive(false);
        platformObject.transform.SetParent(transform, false);
        platformObject.transform.localPosition = Vector3.zero;
        platformObject.transform.localRotation = Quaternion.identity;

        Queue<GameObject> pool = GetPool(prefabIndex);
        pool.Enqueue(platformObject);
    }

    /// <summary>
    /// 특정 프리팹 인덱스용 풀을 가져오거나, 없으면 새로 만든다.
    /// </summary>
    private Queue<GameObject> GetPool(int prefabIndex)
    {
        if (!pooledObjectsByPrefabIndex.TryGetValue(prefabIndex, out Queue<GameObject> pool))
        {
            pool = new Queue<GameObject>();
            pooledObjectsByPrefabIndex.Add(prefabIndex, pool);
        }

        return pool;
    }

    /// <summary>
    /// 풀에서 null이 아닌 유효 오브젝트 하나를 꺼낸다.
    /// 풀 안에 파괴된 참조(null)가 섞여 있을 수 있어 while로 걸러낸다.
    /// </summary>
    private GameObject DequeueValidObject(Queue<GameObject> pool)
    {
        while (pool.Count > 0)
        {
            GameObject pooledObject = pool.Dequeue();
            if (pooledObject != null)
            {
                return pooledObject;
            }
        }

        return null;
    }

    /// <summary>
    /// 리스트를 제자리에서 셔플한다.
    /// 
    /// 후보 오프셋 순서를 랜덤화해
    /// 같은 패턴으로만 스폰되는 것을 줄이기 위한 유틸리티 메서드다.
    /// </summary>
    private void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            int temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}