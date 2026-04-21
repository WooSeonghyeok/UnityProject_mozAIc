using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 비트 그룹 생성/제거에 맞춰 장식 오브젝트를 랜덤 스폰하는 스포너.
///
/// 설계 의도:
/// - 장식은 씬 전체에 한 번 깔리는 것이 아니라
///   "현재 살아 있는 발판 그룹 근처"에만 생성된다.
/// - 필요에 따라 원본 장식 오브젝트를 씬에 그대로 보이게 둘 수도 있다.
/// - 전진형 퍼즐의 발밑 분위기를 위해 비트 그룹 아래 구름 레이어도 함께 생성할 수 있다.
/// </summary>
public class DecoRandomSpawn : MonoBehaviour
{
    private const string CloudTextureResourcePath = "Ep.3/Stage3_Skybox/Cloud";

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

    [Header("원본 표시")]
    [SerializeField] private bool hideSourceObjectsOnAwake = true;

    [Header("비트별 구름 레이어")]
    [SerializeField] private bool spawnCloudLayersPerBeat = true;
    [SerializeField] private Transform spawnedCloudParent;
    [SerializeField] private float cloudBaseHeightOffset = -6.5f;
    [SerializeField] private float cloudForwardOffset = 4f;
    [SerializeField] private float cloudSidePadding = 18f;
    [SerializeField] private float cloudForwardPadding = 26f;
    [SerializeField] private float minimumCloudWidth = 64f;
    [SerializeField] private float minimumCloudLength = 96f;
    [SerializeField] private float lowerCloudHeightOffset = -3.2f;
    [SerializeField] private float lowerCloudWidthMultiplier = 1.4f;
    [SerializeField] private float lowerCloudLengthMultiplier = 1.8f;
    [SerializeField] private Vector2 cloudYawRandomRange = new Vector2(-18f, 18f);
    [SerializeField] private Color upperCloudColor = new Color(1f, 0.7f, 0.58f, 0.34f);
    [SerializeField] private Color lowerCloudColor = new Color(0.33f, 0.28f, 0.46f, 0.24f);
    [SerializeField] private Vector2 upperCloudTilingDivisor = new Vector2(22f, 44f);
    [SerializeField] private Vector2 lowerCloudTilingDivisor = new Vector2(30f, 56f);

    private readonly List<GameObject> decoPrefabs = new List<GameObject>();
    private readonly Dictionary<int, List<GameObject>> spawnedDecosByBeatIndex = new Dictionary<int, List<GameObject>>();
    private readonly Dictionary<int, List<GameObject>> spawnedCloudsByBeatIndex = new Dictionary<int, List<GameObject>>();
    private readonly List<Vector3> activeSpawnedPositions = new List<Vector3>();

    private Mesh sharedCloudQuadMesh;
    private Texture2D cloudTexture;

    private void Awake()
    {
        CacheDecoPrefabsFromChildren();
    }

    private void OnDestroy()
    {
        if (sharedCloudQuadMesh != null)
        {
            Destroy(sharedCloudQuadMesh);
            sharedCloudQuadMesh = null;
        }
    }

    public void SpawnForBeatGroup(int beatIndex, List<RhythmPlatform> platforms)
    {
        if (platforms == null || platforms.Count == 0)
        {
            return;
        }

        ReleaseForBeatGroup(beatIndex);

        if (decoPrefabs.Count > 0)
        {
            Transform parent = spawnedDecoParent != null ? spawnedDecoParent : transform;
            int spawnCount = Random.Range(minDecoPerBeat, maxDecoPerBeat + 1);
            List<GameObject> spawnedObjects = new List<GameObject>();

            for (int i = 0; i < spawnCount; i++)
            {
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
        else
        {
            Debug.LogWarning("[DecoRandomSpawn] 스폰할 장식 프리팹이 없습니다.");
        }

        if (spawnCloudLayersPerBeat)
        {
            SpawnCloudLayersForBeatGroup(beatIndex, platforms);
        }
    }

    public void ReleaseForBeatGroup(int beatIndex)
    {
        bool hadSpawnedObjects = false;

        if (spawnedDecosByBeatIndex.TryGetValue(beatIndex, out List<GameObject> spawnedObjects))
        {
            hadSpawnedObjects = true;

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

        if (spawnedCloudsByBeatIndex.TryGetValue(beatIndex, out List<GameObject> spawnedClouds))
        {
            hadSpawnedObjects = true;

            for (int i = 0; i < spawnedClouds.Count; i++)
            {
                DestroyCloudObject(spawnedClouds[i]);
            }

            spawnedCloudsByBeatIndex.Remove(beatIndex);
        }

        if (!hadSpawnedObjects)
        {
            return;
        }
    }

    public void ClearAllSpawnedObjects()
    {
        HashSet<int> beatIndices = new HashSet<int>(spawnedDecosByBeatIndex.Keys);
        beatIndices.UnionWith(spawnedCloudsByBeatIndex.Keys);

        foreach (int beatIndex in beatIndices)
        {
            ReleaseForBeatGroup(beatIndex);
        }

        activeSpawnedPositions.Clear();
    }

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

            if (hideSourceObjectsOnAwake)
            {
                childObject.SetActive(false);
            }
        }
    }

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

    private Vector3 GetGroundAdjustedPosition(Vector3 targetPosition)
    {
        Vector3 rayOrigin = targetPosition + Vector3.up * raycastStartHeight;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastDistance, groundLayerMask))
        {
            return hit.point + Vector3.up * verticalOffset;
        }

        return targetPosition + Vector3.up * verticalOffset;
    }

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

    private void SpawnCloudLayersForBeatGroup(int beatIndex, List<RhythmPlatform> platforms)
    {
        if (!TryCalculateBeatBounds(platforms, out Bounds beatBounds))
        {
            return;
        }

        Texture2D loadedCloudTexture = GetOrLoadCloudTexture();
        if (loadedCloudTexture == null)
        {
            Debug.LogWarning("[DecoRandomSpawn] 구름 텍스처를 Resources에서 찾을 수 없습니다.");
            return;
        }

        if (sharedCloudQuadMesh == null)
        {
            sharedCloudQuadMesh = CreateQuadMesh();
        }

        Transform parent = spawnedCloudParent != null ? spawnedCloudParent : (spawnedDecoParent != null ? spawnedDecoParent : transform);
        List<GameObject> spawnedClouds = new List<GameObject>(2);

        Vector3 beatCenter = beatBounds.center;
        float baseWidth = Mathf.Max(minimumCloudWidth, beatBounds.size.x + cloudSidePadding * 2f);
        float baseLength = Mathf.Max(minimumCloudLength, beatBounds.size.z + cloudForwardPadding * 2f);
        float baseY = beatBounds.min.y + cloudBaseHeightOffset;
        float yaw = Random.Range(cloudYawRandomRange.x, cloudYawRandomRange.y);

        Vector3 upperPosition = new Vector3(
            beatCenter.x,
            baseY,
            beatCenter.z + cloudForwardOffset);

        Vector3 lowerPosition = new Vector3(
            beatCenter.x,
            baseY + lowerCloudHeightOffset,
            beatCenter.z + cloudForwardOffset * 0.8f);

        spawnedClouds.Add(CreateCloudObject(
            $"BeatCloud_{beatIndex}_Upper",
            parent,
            upperPosition,
            new Vector2(baseWidth, baseLength),
            yaw,
            upperCloudColor,
            upperCloudTilingDivisor));

        spawnedClouds.Add(CreateCloudObject(
            $"BeatCloud_{beatIndex}_Lower",
            parent,
            lowerPosition,
            new Vector2(baseWidth * lowerCloudWidthMultiplier, baseLength * lowerCloudLengthMultiplier),
            -yaw * 0.45f,
            lowerCloudColor,
            lowerCloudTilingDivisor));

        spawnedCloudsByBeatIndex[beatIndex] = spawnedClouds;
    }

    private bool TryCalculateBeatBounds(List<RhythmPlatform> platforms, out Bounds bounds)
    {
        bounds = default;
        bool initialized = false;

        for (int i = 0; i < platforms.Count; i++)
        {
            RhythmPlatform platform = platforms[i];
            if (platform == null)
            {
                continue;
            }

            if (TryGetPlatformBounds(platform.gameObject, out Bounds platformBounds))
            {
                if (!initialized)
                {
                    bounds = platformBounds;
                    initialized = true;
                }
                else
                {
                    bounds.Encapsulate(platformBounds);
                }
            }
            else
            {
                Vector3 position = platform.transform.position;
                if (!initialized)
                {
                    bounds = new Bounds(position, Vector3.zero);
                    initialized = true;
                }
                else
                {
                    bounds.Encapsulate(position);
                }
            }
        }

        return initialized;
    }

    private bool TryGetPlatformBounds(GameObject platformObject, out Bounds bounds)
    {
        bounds = default;
        bool initialized = false;

        Collider[] colliders = platformObject.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (collider == null || !collider.enabled)
            {
                continue;
            }

            if (!initialized)
            {
                bounds = collider.bounds;
                initialized = true;
            }
            else
            {
                bounds.Encapsulate(collider.bounds);
            }
        }

        if (initialized)
        {
            return true;
        }

        Renderer[] renderers = platformObject.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null || !renderer.enabled)
            {
                continue;
            }

            if (!initialized)
            {
                bounds = renderer.bounds;
                initialized = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        return initialized;
    }

    private Texture2D GetOrLoadCloudTexture()
    {
        if (cloudTexture == null)
        {
            cloudTexture = Resources.Load<Texture2D>(CloudTextureResourcePath);
        }

        return cloudTexture;
    }

    private GameObject CreateCloudObject(
        string objectName,
        Transform parent,
        Vector3 worldPosition,
        Vector2 worldSize,
        float yaw,
        Color color,
        Vector2 tilingDivisor)
    {
        GameObject cloudObject = new GameObject(objectName);
        cloudObject.transform.SetParent(parent, false);
        cloudObject.transform.position = worldPosition;
        cloudObject.transform.rotation = Quaternion.Euler(90f, yaw, 0f);
        cloudObject.transform.localScale = new Vector3(worldSize.x, worldSize.y, 1f);

        MeshFilter meshFilter = cloudObject.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = sharedCloudQuadMesh;

        MeshRenderer meshRenderer = cloudObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = CreateCloudMaterial(color, worldSize, tilingDivisor);
        meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        meshRenderer.lightProbeUsage = LightProbeUsage.Off;
        meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;

        return cloudObject;
    }

    private Material CreateCloudMaterial(Color color, Vector2 worldSize, Vector2 tilingDivisor)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Transparent");
        }

        Material material = new Material(shader);
        material.name = "Runtime_BeatCloudMat";

        Vector2 tiling = GetCloudTiling(worldSize, tilingDivisor);
        Vector2 offset = GetRandomCloudOffset();

        if (material.HasProperty("_BaseMap"))
        {
            material.SetTexture("_BaseMap", cloudTexture);
            material.SetTextureScale("_BaseMap", tiling);
            material.SetTextureOffset("_BaseMap", offset);
        }
        else if (material.HasProperty("_MainTex"))
        {
            material.SetTexture("_MainTex", cloudTexture);
            material.SetTextureScale("_MainTex", tiling);
            material.SetTextureOffset("_MainTex", offset);
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        else if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }

        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 1f);
        }

        if (material.HasProperty("_Blend"))
        {
            material.SetFloat("_Blend", 0f);
        }

        if (material.HasProperty("_AlphaClip"))
        {
            material.SetFloat("_AlphaClip", 0f);
        }

        if (material.HasProperty("_Cull"))
        {
            material.SetFloat("_Cull", 0f);
        }

        if (material.HasProperty("_ZWrite"))
        {
            material.SetFloat("_ZWrite", 0f);
        }

        if (material.HasProperty("_SrcBlend"))
        {
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        }

        if (material.HasProperty("_DstBlend"))
        {
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        }

        material.renderQueue = (int)RenderQueue.Transparent;
        return material;
    }

    private Vector2 GetCloudTiling(Vector2 worldSize, Vector2 tilingDivisor)
    {
        float widthDivisor = Mathf.Max(0.01f, tilingDivisor.x);
        float lengthDivisor = Mathf.Max(0.01f, tilingDivisor.y);

        return new Vector2(
            Mathf.Max(1f, worldSize.x / widthDivisor),
            Mathf.Max(1f, worldSize.y / lengthDivisor));
    }

    private Vector2 GetRandomCloudOffset()
    {
        return new Vector2(Random.value, Random.value);
    }

    private void DestroyCloudObject(GameObject cloudObject)
    {
        if (cloudObject == null)
        {
            return;
        }

        MeshRenderer meshRenderer = cloudObject.GetComponent<MeshRenderer>();
        if (meshRenderer != null && meshRenderer.sharedMaterial != null)
        {
            Destroy(meshRenderer.sharedMaterial);
        }

        Destroy(cloudObject);
    }

    private Mesh CreateQuadMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Ep3_2BeatCloudQuad";
        mesh.vertices = new[]
        {
            new Vector3(-0.5f, -0.5f, 0f),
            new Vector3(0.5f, -0.5f, 0f),
            new Vector3(-0.5f, 0.5f, 0f),
            new Vector3(0.5f, 0.5f, 0f),
        };
        mesh.uv = new[]
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
        };
        mesh.triangles = new[]
        {
            0, 2, 1,
            2, 3, 1
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
