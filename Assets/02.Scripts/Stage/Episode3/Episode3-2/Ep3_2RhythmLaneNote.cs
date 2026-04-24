using UnityEngine;

public class Ep3_2RhythmLaneNote : MonoBehaviour
{
    private MeshRenderer cachedRenderer;
    private Transform cachedTransform;
    private Vector3 spawnPosition;
    private Vector3 judgePosition;
    private float previewTime;
    private float judgeTime;
    private float judgeWindow;
    private bool isResolved;
    private Ep3_2LaneType laneType;

    public Ep3_2LaneType LaneType => laneType;
    public float JudgeTime => judgeTime;
    public float JudgeWindow => judgeWindow;
    public bool IsResolved => isResolved;

    private void Awake()
    {
        cachedTransform = transform;
        cachedRenderer = GetComponentInChildren<MeshRenderer>(true);
    }

    public void Initialize(
        Ep3_2LaneType lane,
        float notePreviewTime,
        float noteJudgeTime,
        float noteJudgeWindow,
        Vector3 noteSpawnPosition,
        Vector3 noteJudgePosition,
        Color noteColor,
        Vector3 noteScale)
    {
        laneType = lane;
        previewTime = notePreviewTime;
        judgeTime = noteJudgeTime;
        judgeWindow = noteJudgeWindow;
        spawnPosition = noteSpawnPosition;
        judgePosition = noteJudgePosition;
        isResolved = false;

        cachedTransform.position = noteSpawnPosition;
        cachedTransform.localScale = noteScale;

        if (cachedRenderer != null)
        {
            cachedRenderer.material.color = noteColor;
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
}
