using UnityEngine;
using UnityEngine.Rendering;

public class Ep3_2RhythmLaneNote : MonoBehaviour
{
    private static Mesh quadMesh;

    private Renderer[] cachedRenderers;
    private Transform cachedTransform;
    private Vector3 initialLocalScale = Vector3.one;
    private Vector3 spawnPosition;
    private Vector3 judgePosition;
    private float previewTime;
    private float judgeTime;
    private float judgeWindow;
    private bool isResolved;
    private Ep3_2LaneType laneType;
    private Quaternion noteRotation = Quaternion.identity;

    public Ep3_2LaneType LaneType => laneType;
    public float JudgeTime => judgeTime;
    public float JudgeWindow => judgeWindow;
    public bool IsResolved => isResolved;

    private void Awake()
    {
        cachedTransform = transform;
        ReplaceRuntimeNoteMeshWithQuad();
        cachedRenderers = GetComponentsInChildren<Renderer>(true);
        initialLocalScale = cachedTransform.localScale;
    }

    public void Initialize(
        Ep3_2LaneType lane,
        float notePreviewTime,
        float noteJudgeTime,
        float noteJudgeWindow,
        Vector3 noteSpawnPosition,
        Vector3 noteJudgePosition,
        Color noteColor,
        Quaternion visualRotation,
        Vector3 noteScale)
    {
        laneType = lane;
        previewTime = notePreviewTime;
        judgeTime = noteJudgeTime;
        judgeWindow = noteJudgeWindow;
        spawnPosition = noteSpawnPosition;
        judgePosition = noteJudgePosition;
        noteRotation = visualRotation;
        isResolved = false;

        cachedTransform.SetPositionAndRotation(noteSpawnPosition, noteRotation);
        cachedTransform.localScale = Vector3.Scale(initialLocalScale, noteScale);

        if (cachedRenderers != null)
        {
            for (int i = 0; i < cachedRenderers.Length; i++)
            {
                Renderer renderer = cachedRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.sortingOrder = 20;
                renderer.material.renderQueue = 3100;

                Texture mainTexture = null;
                if (renderer.material.HasProperty("_BaseMap"))
                {
                    mainTexture = renderer.material.GetTexture("_BaseMap");
                }
                else if (renderer.material.HasProperty("_MainTex"))
                {
                    mainTexture = renderer.material.mainTexture;
                }

                bool hasTexture = mainTexture != null;

                if (!hasTexture && renderer.material.HasProperty("_Color"))
                {
                    renderer.material.color = noteColor;
                }

                if (!hasTexture && renderer.material.HasProperty("_BaseColor"))
                {
                    renderer.material.SetColor("_BaseColor", noteColor);
                }

                if (renderer.material.HasProperty("_EmissionColor"))
                {
                    renderer.material.EnableKeyword("_EMISSION");
                    renderer.material.SetColor("_EmissionColor", hasTexture ? Color.white * 1.2f : noteColor * 1.6f);
                }
            }
        }
    }

    public void Tick(float currentTime)
    {
        if (isResolved)
        {
            return;
        }

        float travelDenominator = Mathf.Max(0.0001f, judgeTime - previewTime);
        float normalized = Mathf.Clamp01((currentTime - previewTime) / travelDenominator);
        cachedTransform.position = Vector3.Lerp(spawnPosition, judgePosition, normalized);
        cachedTransform.rotation = noteRotation;
    }

    public float GetTimingDelta(float currentTime)
    {
        return Mathf.Abs(currentTime - judgeTime);
    }

    public bool IsExpired(float currentTime)
    {
        return !isResolved && currentTime > judgeTime + judgeWindow;
    }

    public void Resolve()
    {
        isResolved = true;
    }

    private void ReplaceRuntimeNoteMeshWithQuad()
    {
        Mesh mesh = GetOrCreateQuadMesh();
        if (mesh == null)
        {
            return;
        }

        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>(true);
        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (meshFilters[i] == null)
            {
                continue;
            }

            meshFilters[i].sharedMesh = mesh;
        }
    }

    private static Mesh GetOrCreateQuadMesh()
    {
        if (quadMesh != null)
        {
            return quadMesh;
        }

        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        MeshFilter filter = quad.GetComponent<MeshFilter>();
        if (filter != null)
        {
            quadMesh = filter.sharedMesh;
        }

        Object.DestroyImmediate(quad);
        return quadMesh;
    }
}
