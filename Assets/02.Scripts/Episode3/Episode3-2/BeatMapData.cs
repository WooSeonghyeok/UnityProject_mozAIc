using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 리듬 퍼즐에서 사용할 비트 이벤트 목록을 보관하는 ScriptableObject.
/// 
/// 에디터에서 미리 만들어둘 수도 있고,
/// RhythmAudioManager가 런타임에 임시로 생성해서 사용할 수도 있다.
/// </summary>
[CreateAssetMenu(fileName = "BeatMapData", menuName = "Episode3/BeatMapData")]
public class BeatMapData : ScriptableObject
{
    /// <summary>
    /// 시간 순서대로 소비되는 비트 이벤트 목록.
    /// </summary>
    public List<BeatEvent> beatEvents = new List<BeatEvent>();
}

/// <summary>
/// 하나의 비트에서 필요한 판정/스폰 정보를 담는 데이터 단위.
/// 
/// previewTime:
/// - 정답 발판을 미리 보여주기 시작하는 시간
/// 
/// judgeTime:
/// - 실제 정답 판정 기준 시간
/// 
/// judgeWindow:
/// - 판정 허용 오차 범위
/// 
/// endTime:
/// - 필요 시 이 비트의 종료 시간으로 사용할 수 있는 값
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

    [Header("후보 위치(X=좌우, Y=높이)")]
    public List<Vector2> platformOffsets = new List<Vector2>();

    [Header("랜덤 위치 보정")]
    public float randomOffsetX = 0.5f;
    public float randomOffsetY = 0.25f;
}
