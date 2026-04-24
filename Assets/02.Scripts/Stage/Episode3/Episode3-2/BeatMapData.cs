using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime beat map data for the Episode 3-2 rhythm puzzle.
/// </summary>
[CreateAssetMenu(fileName = "BeatMapData", menuName = "Episode3/BeatMapData")]
public class BeatMapData : ScriptableObject
{
    public List<BeatEvent> beatEvents = new List<BeatEvent>();
}

public enum Ep3_2LaneType
{
    Left,
    Up,
    Down,
    Right
}

/// <summary>
/// One rhythm event entry consumed in time order.
/// Existing platform fields are preserved for backward compatibility.
/// </summary>
[System.Serializable]
public class BeatEvent
{
    public float previewTime;
    public float judgeTime;
    public float judgeWindow;
    public float endTime;

    [Header("발판 개수")]
    [Range(1, 3)] public int minPlatformCount = 1;
    [Range(1, 3)] public int maxPlatformCount = 3;

    [Header("정답 설정")]
    public int targetPlatformIndex;
    public bool mustStep = true;

    [Header("탑다운 리듬 입력")]
    public Ep3_2LaneType laneType = Ep3_2LaneType.Down;
    public bool isHoldNote = false;
    public float holdDuration = 0f;

    [Header("후보 위치(X=좌우, Y=높이)")]
    public List<Vector2> platformOffsets = new List<Vector2>();

    [Header("랜덤 위치 보정")]
    public float randomOffsetX = 0.5f;
    public float randomOffsetY = 0.25f;
}
