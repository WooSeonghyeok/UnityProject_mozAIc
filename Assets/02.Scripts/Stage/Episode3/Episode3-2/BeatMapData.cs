using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime beat map data for the Episode 3-2 rhythm puzzle.
/// </summary>
[CreateAssetMenu(fileName = "BeatMapData", menuName = "Episode3/BeatMapData")]
public class BeatMapData : ScriptableObject
{
    [Header("기존 런타임 비트 이벤트")]
    public List<BeatEvent> beatEvents = new List<BeatEvent>();

    [Header("탑다운 수동 채보")]
    public List<TopDownChartNote> topDownChartNotes = new List<TopDownChartNote>();
}

public enum Ep3_2LaneType
{
    D,
    F,
    Space,
    J,
    K
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
    public Ep3_2LaneType laneType = Ep3_2LaneType.Space;
    public bool isHoldNote = false;
    public float holdDuration = 0f;

    [Header("후보 위치(X=좌우, Y=높이)")]
    public List<Vector2> platformOffsets = new List<Vector2>();

    [Header("랜덤 위치 보정")]
    public float randomOffsetX = 0.5f;
    public float randomOffsetY = 0.25f;
}

[System.Serializable]
public class TopDownChartNote
{
    [Header("멜로디 타이밍")]
    [Min(0f)] public float judgeTimeSeconds = 1f;
    [Min(0f)] public float judgeWindowOverride = 0f;

    [Header("입력 레인")]
    public Ep3_2LaneType laneType = Ep3_2LaneType.Space;

    [Header("홀드 노트")]
    public bool isHoldNote = false;
    [Min(0f)] public float holdDurationSeconds = 0f;

    [Header("메모")]
    [TextArea(1, 3)] public string memo;
}
