using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 비트 그룹 생성/제거에 맞춰 장식 오브젝트를 랜덤 스폰하는 스포너.
/// 
/// 설계 의도:
/// - 장식은 씬 전체에 한 번 깔리는 것이 아니라
///   "현재 살아 있는 발판 그룹 근처"에만 생성되어가지고
///   그 비트가 사라질 때 함께 제거된다.
/// 
/// 핵심 역할:
/// 1. Decos 자식 오브젝트를 장식 원본(prefab 후보)처럼 수집
/// 2. 특정 비트 그룹 기준으로 장식 생성
/// 3. 특정 비트 그룹 제거 시 장식도 함께 제거
/// 4. 전체 리셋 시 모든 장식 정리
/// </summary>
public class DecoRandomSpawn : MonoBehaviour
{
    [Header("스폰 부모")]
    [SerializeField] private Transform spawnedDecoParent;
    [SerializeField] private BoxCollider spawnAreaBox;

    [Header("비트별 장식 생성 설정")]
    [SerializeField] private int minDecoPerBeat = 2;
    [SerializeField] private int maxDecoPerBeat = 5;
    [SerializeField] private float spawnRadius = 6f;
    [SerializeField] private float forwardBias = 2f;

    [Header("위치 보정")]
    [SerializeField] private float verticalOffset = 0f;
    [SerializeField] private int maxSpawnPositionRetryCount = 20;
    [SerializeField] private float minDistanceBetweenDecos = 1.5f;

    [Header("지면 보정")]
    [SerializeField] private bool alignToGround = true;
    [SerializeField] private float raycastStartHeight = 10f;
    [SerializeField] private float raycastDistance = 50f;
    [SerializeField] private LayerMask groundLayerMask = Physics.DefaultRaycastLayers;

    [Header("회전/크기 랜덤")]
    [SerializeField] private bool randomYawRotation = true;
    [SerializeField] private Vector2 randomScaleRange = new Vector2(1f, 1f);

    /// <summary>
    /// Decos 자식 오브젝트에서 수집한 장식 원본 목록.
    /// 
    /// 실제 런타임에서는 이 오브젝트들을 직접 이동하지 않고,
    /// Instantiate의 원본처럼 사용한다.
    /// </summary>
    private readonly List<GameObject> decoPrefabs = new List<GameObject>();

    /// <summary>
    /// beatIndex별로 현재 생성되어 있는 장식 오브젝트 목록.
    /// 
    /// 어떤 비트 그룹이 사라질 때 해당 비트 장식만 제거하기 위해 필요하다.
    /// </summary>
    private readonly Dictionary<int, List<GameObject>> spawnedDecosByBeatIndex = new Dictionary<int, List<GameObject>>();

    /// <summary>
    /// 현재 살아 있는 장식들의 위치 목록.
    /// 
    /// 장식끼리 너무 가까이 붙지 않게 하기 위한 거리 검사에 사용한다.
    /// </summary>
    private readonly List<Vector3> activeSpawnedPositions = new List<Vector3>();

    /// <summary>
    /// 씬 시작 전에 원본 장식 후보를 수집한다.
    /// Decos 자식 오브젝트는 원본 역할만 하고, 직접 보이진 않도록 비활성화한다.
    /// </summary>
    private void Awake()
    {
        CacheDecoPrefabsFromChildren();
    }

    /// <summary>
    /// 특정 비트 그룹 발판들을 기준으로 장식을 생성한다.
    /// 
    /// 동작:
    /// 1. 같은 beatIndex 장식이 이미 있으면 먼저 제거
    /// 2. 비트당 생성 개수 랜덤 결정
    /// 3. 발판 중 하나를 앵커로 골라 주변에 장식 생성
    /// 4. 생성된 장식 목록을 beatIndex 기준으로 기록
    /// 
    /// 이 메서드는 RhythmBeatWindowManager가 비트 그룹 생성 직후 호출한다.
    /// </summary>
    public void SpawnForBeatGroup(int beatIndex, List<RhythmPlatform> platforms)
    {
        if (platforms == null || platforms.Count == 0)
        {
            return;
        }

        ReleaseForBeatGroup(beatIndex);

        if (decoPrefabs.Count == 0)
        {
            Debug.LogWarning("[DecoRandomSpawn] 스폰할 장식 프리팹이 없습니다.");
            return;
        }

        Transform parent = spawnedDecoParent != null ? spawnedDecoParent : transform;
        int spawnCount = Random.Range(minDecoPerBeat, maxDecoPerBeat + 1);
        List<GameObject> spawnedObjects = new List<GameObject>();

        for (int i = 0; i < spawnCount; i++)
        {
            // 현재 비트 그룹 발판들 중 하나를 기준점으로 삼아 주변 장식을 배치한다.
            // 이렇게 해야 장식이 퍼즐 진행 방향을 따라 함께 움직이는 것처럼 보인다.
            RhythmPlatform anchorPlatform = platforms[Random.Range(0, platforms.Count)];
            if (anchorPlatform == null)
            {
                continue;
            }

            if (!TryGetSpawnPosition(anchorPlatform.transform, out Vector3 spawnPosition))
            {
                continue;
            }

            GameObject prefab = decoPrefabs[Random.Range(0, decoPrefabs.Count)];
            if (prefab == null)
            {
                continue;
            }

            Quaternion spawnRotation = prefab.transform.rotation;
            if (randomYawRotation)
            {
                spawnRotation *= Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            }

            GameObject spawnedObject = Instantiate(prefab, spawnPosition, spawnRotation, parent);
            spawnedObject.SetActive(true);

            float randomScale = Random.Range(randomScaleRange.x, randomScaleRange.y);
            spawnedObject.transform.localScale = prefab.transform.localScale * randomScale;

            spawnedObjects.Add(spawnedObject);
            activeSpawnedPositions.Add(spawnPosition);
        }

        if (spawnedObjects.Count > 0)
        {
            spawnedDecosByBeatIndex[beatIndex] = spawnedObjects;
        }
    }

    /// <summary>
    /// 특정 비트 그룹에 속한 장식만 제거한다.
    /// 
    /// 비트 그룹이 화면에서 빠질 때 발판과 함께 장식도 정리하기 위해 사용한다.
    /// </summary>
    public void ReleaseForBeatGroup(int beatIndex)
    {
        if (!spawnedDecosByBeatIndex.TryGetValue(beatIndex, out List<GameObject> spawnedObjects))
        {
            return;
        }

        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            GameObject spawnedObject = spawnedObjects[i];
            if (spawnedObject != null)
            {
                RemoveSpawnedPosition(spawnedObject.transform.position);
                Destroy(spawnedObject);
            }
        }

        spawnedDecosByBeatIndex.Remove(beatIndex);
    }

    /// <summary>
    /// 현재 생성된 모든 장식을 제거한다.
    /// 퍼즐 종료/리셋 시 전체 초기화용으로 사용된다.
    /// </summary>
    public void ClearAllSpawnedObjects()
    {
        List<int> beatIndices = new List<int>(spawnedDecosByBeatIndex.Keys);
        for (int i = 0; i < beatIndices.Count; i++)
        {
            ReleaseForBeatGroup(beatIndices[i]);
        }

        activeSpawnedPositions.Clear();
    }

    /// <summary>
    /// Decos 자식들을 장식 원본 목록으로 수집한다.
    /// 
    /// 원본 자식은 씬에서 직접 보일 필요가 없으므로 비활성화한다.
    /// 이후 런타임에서는 이 목록에서 랜덤 원본을 골라 Instantiate한다.
    /// </summary>
    private void CacheDecoPrefabsFromChildren()
    {
        decoPrefabs.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child == null)
            {
                continue;
            }

            GameObject childObject = child.gameObject;
            decoPrefabs.Add(childObject);
            childObject.SetActive(false);
        }
    }

    /// <summary>
    /// 특정 발판을 기준으로 장식 스폰 위치를 찾는다.
    /// 
    /// 위치 생성 규칙:
    /// - 발판 주변 원형 범위 내에서 랜덤 위치 선택
    /// - 약간의 forwardBias를 줘서 진행 방향 전방 쪽에도 배치
    /// - spawnAreaBox가 있으면 그 박스 내부에서만 허용
    /// - 지면 보정이 켜져 있으면 Raycast로 바닥에 붙임
    /// - 기존 장식과 너무 가까우면 재시도
    /// </summary>
    private bool TryGetSpawnPosition(Transform anchorTransform, out Vector3 finalPosition)
    {
        finalPosition = anchorTransform.position;

        Vector3 forward = anchorTransform.forward;
        Vector3 right = anchorTransform.right;

        for (int i = 0; i < Mathf.Max(1, maxSpawnPositionRetryCount); i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;

            Vector3 candidatePosition =
                anchorTransform.position +
                forward * (forwardBias + randomCircle.y) +
                right * randomCircle.x;

            if (spawnAreaBox != null && !IsInsideSpawnArea(candidatePosition))
            {
                continue;
            }

            if (alignToGround)
            {
                candidatePosition = GetGroundAdjustedPosition(candidatePosition);
            }
            else
            {
                candidatePosition += Vector3.up * verticalOffset;
            }

            if (!IsFarEnough(candidatePosition))
            {
                continue;
            }

            finalPosition = candidatePosition;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 월드 좌표가 spawnAreaBox 내부인지 검사한다.
    /// 
    /// 박스 콜라이더의 로컬 좌표계로 변환한 뒤 halfSize 범위 안인지 확인한다.
    /// 이렇게 하면 박스가 이동/회전/스케일되어도 일관되게 검사할 수 있다.
    /// </summary>
    private bool IsInsideSpawnArea(Vector3 worldPosition)
    {
        if (spawnAreaBox == null)
        {
            return true;
        }

        Vector3 localPoint = spawnAreaBox.transform.InverseTransformPoint(worldPosition) - spawnAreaBox.center;
        Vector3 halfSize = spawnAreaBox.size * 0.5f;

        return
            Mathf.Abs(localPoint.x) <= halfSize.x &&
            Mathf.Abs(localPoint.y) <= halfSize.y &&
            Mathf.Abs(localPoint.z) <= halfSize.z;
    }

    /// <summary>
    /// 후보 위치 위에서 아래로 Raycast를 쏴 실제 지면 위치를 찾는다.
    /// 
    /// 지면을 맞추지 못하면 원래 위치 + verticalOffset을 사용한다.
    /// 즉, 지면이 반드시 있어야만 생성되는 강제 구조는 아니다.
    /// </summary>
    private Vector3 GetGroundAdjustedPosition(Vector3 targetPosition)
    {
        Vector3 rayOrigin = targetPosition + Vector3.up * raycastStartHeight;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastDistance, groundLayerMask))
        {
            return hit.point + Vector3.up * verticalOffset;
        }

        return targetPosition + Vector3.up * verticalOffset;
    }

    /// <summary>
    /// 새 장식 위치가 기존 장식들과 최소 거리 이상 떨어져 있는지 검사한다.
    /// 
    /// 장식끼리 과도하게 뭉치는 시각적 문제를 줄이기 위한 단순 거리 기반 방어다.
    /// </summary>
    private bool IsFarEnough(Vector3 candidatePosition)
    {
        float minimumDistance = Mathf.Max(0f, minDistanceBetweenDecos);
        float minimumDistanceSqr = minimumDistance * minimumDistance;

        for (int i = 0; i < activeSpawnedPositions.Count; i++)
        {
            Vector3 delta = candidatePosition - activeSpawnedPositions[i];
            if (delta.sqrMagnitude < minimumDistanceSqr)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 장식 제거 시 activeSpawnedPositions에서도 해당 위치를 제거한다.
    /// 
    /// 완전히 동일한 부동소수 좌표 비교는 불안정할 수 있으므로
    /// 아주 작은 오차 범위 안이면 같은 위치로 간주한다.
    /// </summary>
    private void RemoveSpawnedPosition(Vector3 worldPosition)
    {
        for (int i = activeSpawnedPositions.Count - 1; i >= 0; i--)
        {
            Vector3 delta = activeSpawnedPositions[i] - worldPosition;
            if (delta.sqrMagnitude <= 0.01f)
            {
                activeSpawnedPositions.RemoveAt(i);
                return;
            }
        }
    }
}
