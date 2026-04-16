using UnityEngine;
using UnityEngine.Rendering;

public class Ep3_2CloudLayerSystem : MonoBehaviour
{
    private const string UpperLayerName = "CloudLayer_Upper";
    private const string LowerLayerName = "CloudLayer_Lower";
    private const string UpperCoverLayerName = "CloudLayer_UpperCover";
    private const string LowerCoverLayerName = "CloudLayer_LowerCover";
    private const string CloudTextureResourcePath = "Ep.3/Stage3_Skybox/Cloud";
    private const string DefaultFollowTargetTag = "Player";
    private const string DefaultFollowTargetName = "Player";

    [Header("Placement")]
    [SerializeField] private Vector3 upperLayerLocalPosition = new Vector3(0f, -3.5f, 320f);
    [Tooltip("X = width, Y = forward length after the quad is rotated")]
    [SerializeField] private Vector2 upperLayerSize = new Vector2(750f, 920f);
    [SerializeField] private Vector3 lowerLayerLocalPosition = new Vector3(0f, -10.5f, 320f);
    [Tooltip("X = width, Y = forward length after the quad is rotated")]
    [SerializeField] private Vector2 lowerLayerSize = new Vector2(750f, 1040f);

    [Header("Coverage Follow")]
    [SerializeField] private bool followPlayerProgress = true;
    [SerializeField] private Transform followTarget;
    [SerializeField] private int tilesPerLayer = 3;
    [SerializeField] private float tileSpacingMultiplier = 0.82f;
    [SerializeField] private bool freezeWhenPuzzlePlatformsDisappear = true;
    [SerializeField] private float platformCheckInterval = 0.15f;

    [Header("Color")]
    [SerializeField] private Color upperLayerColor = new Color(1f, 0.68f, 0.56f, 0.34f);
    [SerializeField] private Color lowerLayerColor = new Color(0.33f, 0.28f, 0.46f, 0.22f);

    [Header("Texture Motion")]
    [SerializeField] private Vector2 upperLayerTiling = new Vector2(1.15f, 5.2f);
    [SerializeField] private Vector2 lowerLayerTiling = new Vector2(1.05f, 4.3f);
    [SerializeField] private Vector2 upperLayerScrollSpeed = new Vector2(0.0025f, 0.0015f);
    [SerializeField] private Vector2 lowerLayerScrollSpeed = new Vector2(-0.0015f, 0.001f);

    [Header("Gap Cover Layer")]
    [SerializeField] private bool addGapCoverLayer = true;
    [SerializeField] private Vector3 upperCoverLayerLocalOffset = new Vector3(0f, 0.35f, 0f);
    [SerializeField] private Vector2 upperCoverLayerSizeMultiplier = new Vector2(1.08f, 1.04f);
    [SerializeField] private Color upperCoverLayerColor = new Color(1f, 0.74f, 0.63f, 0.17f);
    [SerializeField] private Vector2 upperCoverLayerTiling = new Vector2(1.38f, 5.8f);
    [SerializeField] private Vector2 upperCoverLayerScrollSpeed = new Vector2(-0.0018f, 0.0022f);
    [SerializeField] private Vector2 upperCoverLayerTextureOffset = new Vector2(0.37f, 0.16f);
    [SerializeField] private Vector3 lowerCoverLayerLocalOffset = new Vector3(0f, 0.55f, 0f);
    [SerializeField] private Vector2 lowerCoverLayerSizeMultiplier = new Vector2(1.1f, 1.06f);
    [SerializeField] private Color lowerCoverLayerColor = new Color(0.42f, 0.35f, 0.52f, 0.14f);
    [SerializeField] private Vector2 lowerCoverLayerTiling = new Vector2(1.28f, 4.9f);
    [SerializeField] private Vector2 lowerCoverLayerScrollSpeed = new Vector2(0.0014f, 0.0017f);
    [SerializeField] private Vector2 lowerCoverLayerTextureOffset = new Vector2(0.61f, 0.29f);

    private GameObject[] upperLayerObjects = new GameObject[0];
    private GameObject[] lowerLayerObjects = new GameObject[0];
    private GameObject[] upperCoverLayerObjects = new GameObject[0];
    private GameObject[] lowerCoverLayerObjects = new GameObject[0];
    private Material upperLayerMaterial;
    private Material lowerLayerMaterial;
    private Material upperCoverLayerMaterial;
    private Material lowerCoverLayerMaterial;
    private Texture2D cloudTexture;
    private Mesh sharedQuadMesh;
    private float cachedFollowLocalZ;
    private bool hasCachedFollowLocalZ;
    private bool hasActivePuzzlePlatforms;
    private float nextPlatformCheckTime;

    private void Start()
    {
        BuildLayersIfNeeded();
        cachedFollowLocalZ = upperLayerLocalPosition.z;
        hasCachedFollowLocalZ = true;
    }

    private void Update()
    {
        UpdateMaterialScroll(upperLayerMaterial, upperLayerTiling, upperLayerScrollSpeed, Vector2.zero);
        UpdateMaterialScroll(lowerLayerMaterial, lowerLayerTiling, lowerLayerScrollSpeed, Vector2.zero);
        UpdateMaterialScroll(upperCoverLayerMaterial, upperCoverLayerTiling, upperCoverLayerScrollSpeed, upperCoverLayerTextureOffset);
        UpdateMaterialScroll(lowerCoverLayerMaterial, lowerCoverLayerTiling, lowerCoverLayerScrollSpeed, lowerCoverLayerTextureOffset);
    }

    private void LateUpdate()
    {
        UpdateLayerCoverage(upperLayerObjects, upperLayerLocalPosition, upperLayerSize);
        UpdateLayerCoverage(lowerLayerObjects, lowerLayerLocalPosition, lowerLayerSize);
        UpdateLayerCoverage(
            upperCoverLayerObjects,
            upperLayerLocalPosition + upperCoverLayerLocalOffset,
            Vector2.Scale(upperLayerSize, upperCoverLayerSizeMultiplier));
        UpdateLayerCoverage(
            lowerCoverLayerObjects,
            lowerLayerLocalPosition + lowerCoverLayerLocalOffset,
            Vector2.Scale(lowerLayerSize, lowerCoverLayerSizeMultiplier));
    }

    private void OnDestroy()
    {
        DestroyLayer(ref upperLayerObjects, ref upperLayerMaterial);
        DestroyLayer(ref lowerLayerObjects, ref lowerLayerMaterial);
        DestroyLayer(ref upperCoverLayerObjects, ref upperCoverLayerMaterial);
        DestroyLayer(ref lowerCoverLayerObjects, ref lowerCoverLayerMaterial);

        if (sharedQuadMesh != null)
        {
            Destroy(sharedQuadMesh);
            sharedQuadMesh = null;
        }
    }

    [ContextMenu("Rebuild Cloud Layers")]
    public void RebuildCloudLayers()
    {
        DestroyLayer(ref upperLayerObjects, ref upperLayerMaterial);
        DestroyLayer(ref lowerLayerObjects, ref lowerLayerMaterial);
        DestroyLayer(ref upperCoverLayerObjects, ref upperCoverLayerMaterial);
        DestroyLayer(ref lowerCoverLayerObjects, ref lowerCoverLayerMaterial);
        BuildLayersIfNeeded();
    }

    private void BuildLayersIfNeeded()
    {
        cloudTexture = Resources.Load<Texture2D>(CloudTextureResourcePath);
        if (cloudTexture == null)
        {
            Debug.LogWarning("[Ep3_2CloudLayerSystem] Cloud texture could not be loaded from Resources.");
            return;
        }

        EnsureCloudTextureWrapMode();

        if (sharedQuadMesh == null)
        {
            sharedQuadMesh = CreateQuadMesh();
        }

        CleanupLayerChildren(UpperLayerName);
        CleanupLayerChildren(LowerLayerName);
        CleanupLayerChildren(UpperCoverLayerName);
        CleanupLayerChildren(LowerCoverLayerName);

        upperLayerObjects = CreateLayerObjects(
            UpperLayerName,
            upperLayerLocalPosition,
            upperLayerSize,
            upperLayerColor,
            ref upperLayerMaterial);

        lowerLayerObjects = CreateLayerObjects(
            LowerLayerName,
            lowerLayerLocalPosition,
            lowerLayerSize,
            lowerLayerColor,
            ref lowerLayerMaterial);

        if (!addGapCoverLayer)
        {
            upperCoverLayerObjects = new GameObject[0];
            lowerCoverLayerObjects = new GameObject[0];
            upperCoverLayerMaterial = null;
            lowerCoverLayerMaterial = null;
            return;
        }

        upperCoverLayerObjects = CreateLayerObjects(
            UpperCoverLayerName,
            upperLayerLocalPosition + upperCoverLayerLocalOffset,
            Vector2.Scale(upperLayerSize, upperCoverLayerSizeMultiplier),
            upperCoverLayerColor,
            ref upperCoverLayerMaterial);

        lowerCoverLayerObjects = CreateLayerObjects(
            LowerCoverLayerName,
            lowerLayerLocalPosition + lowerCoverLayerLocalOffset,
            Vector2.Scale(lowerLayerSize, lowerCoverLayerSizeMultiplier),
            lowerCoverLayerColor,
            ref lowerCoverLayerMaterial);
    }

    private GameObject[] CreateLayerObjects(
        string layerName,
        Vector3 localPosition,
        Vector2 layerSize,
        Color layerColor,
        ref Material layerMaterial)
    {
        int safeTileCount = Mathf.Max(1, tilesPerLayer);
        float stride = GetTileStride(layerSize);
        float centerOffset = (safeTileCount - 1) * 0.5f;

        layerMaterial = CreateCloudMaterial(layerColor);

        GameObject[] layerObjects = new GameObject[safeTileCount];
        for (int i = 0; i < safeTileCount; i++)
        {
            Vector3 tileLocalPosition = localPosition;
            tileLocalPosition.z += (i - centerOffset) * stride;

            layerObjects[i] = CreateLayerObject(
                safeTileCount == 1 ? layerName : $"{layerName}_{i}",
                tileLocalPosition,
                layerSize,
                layerMaterial);
        }

        return layerObjects;
    }

    private GameObject CreateLayerObject(
        string layerName,
        Vector3 localPosition,
        Vector2 layerSize,
        Material layerMaterial)
    {
        GameObject layerObject = new GameObject(layerName);
        layerObject.transform.SetParent(transform, false);
        layerObject.transform.localPosition = localPosition;
        layerObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        layerObject.transform.localScale = new Vector3(layerSize.x, layerSize.y, 1f);

        MeshFilter meshFilter = layerObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = layerObject.AddComponent<MeshFilter>();
        }

        meshFilter.sharedMesh = sharedQuadMesh;

        MeshRenderer meshRenderer = layerObject.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = layerObject.AddComponent<MeshRenderer>();
        }

        meshRenderer.sharedMaterial = layerMaterial;
        meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        meshRenderer.lightProbeUsage = LightProbeUsage.Off;
        meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;

        Collider collider = layerObject.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        return layerObject;
    }

    private Material CreateCloudMaterial(Color baseColor)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Transparent");
        }

        Material material = new Material(shader);
        material.name = "Runtime_CloudLayerMat";

        if (material.HasProperty("_BaseMap"))
        {
            material.SetTexture("_BaseMap", cloudTexture);
        }
        else if (material.HasProperty("_MainTex"))
        {
            material.SetTexture("_MainTex", cloudTexture);
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", baseColor);
        }
        else if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", baseColor);
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

    private void UpdateMaterialScroll(Material material, Vector2 tiling, Vector2 scrollSpeed, Vector2 baseOffset)
    {
        if (material == null)
        {
            return;
        }

        Vector2 resolvedTiling = new Vector2(
            Mathf.Max(0.01f, tiling.x),
            Mathf.Max(0.01f, tiling.y));
        Vector2 offset = new Vector2(
            Mathf.Repeat(baseOffset.x + scrollSpeed.x * Time.time, 1f),
            Mathf.Repeat(baseOffset.y + scrollSpeed.y * Time.time, 1f));

        if (material.HasProperty("_BaseMap"))
        {
            material.SetTextureScale("_BaseMap", resolvedTiling);
            material.SetTextureOffset("_BaseMap", offset);
        }

        if (material.HasProperty("_MainTex"))
        {
            material.SetTextureScale("_MainTex", resolvedTiling);
            material.SetTextureOffset("_MainTex", offset);
        }
    }

    private void EnsureCloudTextureWrapMode()
    {
        if (cloudTexture == null)
        {
            return;
        }

        cloudTexture.wrapMode = TextureWrapMode.Repeat;
        cloudTexture.wrapModeU = TextureWrapMode.Repeat;
        cloudTexture.wrapModeV = TextureWrapMode.Repeat;
    }

    private void UpdateLayerCoverage(GameObject[] layerObjects, Vector3 baseLocalPosition, Vector2 layerSize)
    {
        if (!followPlayerProgress || layerObjects == null || layerObjects.Length == 0)
        {
            return;
        }

        Transform resolvedFollowTarget = ResolveFollowTarget();
        float stride = GetTileStride(layerSize);
        float centerOffset = (layerObjects.Length - 1) * 0.5f;
        float centerZ = GetTrackedCenterZ(baseLocalPosition, resolvedFollowTarget);

        for (int i = 0; i < layerObjects.Length; i++)
        {
            GameObject layerObject = layerObjects[i];
            if (layerObject == null)
            {
                continue;
            }

            Vector3 tileLocalPosition = baseLocalPosition;
            tileLocalPosition.z = centerZ + (i - centerOffset) * stride;
            layerObject.transform.localPosition = tileLocalPosition;
        }
    }

    private float GetTrackedCenterZ(Vector3 baseLocalPosition, Transform resolvedFollowTarget)
    {
        if (resolvedFollowTarget == null)
        {
            return hasCachedFollowLocalZ ? cachedFollowLocalZ + baseLocalPosition.z : baseLocalPosition.z;
        }

        if (!freezeWhenPuzzlePlatformsDisappear || HasActivePuzzlePlatforms())
        {
            cachedFollowLocalZ = transform.InverseTransformPoint(resolvedFollowTarget.position).z;
            hasCachedFollowLocalZ = true;
        }

        return hasCachedFollowLocalZ ? cachedFollowLocalZ + baseLocalPosition.z : baseLocalPosition.z;
    }

    private Transform ResolveFollowTarget()
    {
        if (followTarget != null)
        {
            return followTarget;
        }

        GameObject taggedPlayer = null;
        try
        {
            taggedPlayer = GameObject.FindGameObjectWithTag(DefaultFollowTargetTag);
        }
        catch (UnityException)
        {
            taggedPlayer = null;
        }

        if (taggedPlayer != null)
        {
            followTarget = taggedPlayer.transform;
        }
        else
        {
            GameObject namedPlayer = GameObject.Find(DefaultFollowTargetName);
            if (namedPlayer != null)
            {
                followTarget = namedPlayer.transform;
            }
        }

        return followTarget;
    }

    private float GetTileStride(Vector2 layerSize)
    {
        // Let 0 mean full overlap so inspector tuning matches what we see in the scene.
        float safeMultiplier = Mathf.Clamp01(tileSpacingMultiplier);
        return Mathf.Max(0f, layerSize.y * safeMultiplier);
    }

    private bool HasActivePuzzlePlatforms()
    {
        if (!freezeWhenPuzzlePlatformsDisappear)
        {
            return true;
        }

        if (Time.time < nextPlatformCheckTime)
        {
            return hasActivePuzzlePlatforms;
        }

        nextPlatformCheckTime = Time.time + Mathf.Max(0.05f, platformCheckInterval);
        hasActivePuzzlePlatforms = FindObjectsOfType<RhythmPlatform>().Length > 0;
        return hasActivePuzzlePlatforms;
    }

    private void CleanupLayerChildren(string layerName)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child == null)
            {
                continue;
            }

            if (child.name == layerName || child.name.StartsWith(layerName + "_"))
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void DestroyLayer(ref GameObject[] layerObjects, ref Material layerMaterial)
    {
        if (layerObjects != null)
        {
            for (int i = 0; i < layerObjects.Length; i++)
            {
                if (layerObjects[i] != null)
                {
                    Destroy(layerObjects[i]);
                }
            }

            layerObjects = new GameObject[0];
        }

        if (layerMaterial != null)
        {
            Destroy(layerMaterial);
            layerMaterial = null;
        }
    }

    private Mesh CreateQuadMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Ep3_2CloudQuad";

        mesh.vertices = new[]
        {
            new Vector3(-0.5f, -0.5f, 0f),
            new Vector3( 0.5f, -0.5f, 0f),
            new Vector3(-0.5f,  0.5f, 0f),
            new Vector3( 0.5f,  0.5f, 0f),
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
