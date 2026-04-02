using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 오디오 설정을 기반으로 리듬 비트맵을 런타임 생성하고,
/// 실제 오디오 재생 시간도 제공하는 매니저.
/// 
/// 이 클래스의 핵심 역할:
/// 1. AudioClip / BPM / 생성 주기 설정을 바탕으로 BeatMapData 생성
/// 2. 퍼즐이 사용할 오디오 재생 시작/정지
/// 3. 현재 재생 시간 제공
/// 
/// 즉, "오디오와 비트 데이터의 연결점" 역할을 한다.
/// </summary>
public class RhythmAudioManager : MonoBehaviour
{
    [Header("오디오 설정")]
    [Tooltip("재생에 사용할 AudioSource입니다.")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("리듬 생성 기준이 되는 음악 클립입니다.")]
    [SerializeField] private AudioClip audioClip;

    [Header("리듬 설정")]
    [Tooltip("곡의 BPM입니다.")]
    [SerializeField] private float bpm = 120f;

    [Tooltip("비트 생성 시작 시간입니다.")]
    [SerializeField] private float startOffset = 0f;

    [Tooltip("비트맵을 생성할 총 길이입니다. 0이면 오디오 전체 길이를 사용합니다.")]
    [SerializeField] private float generateDuration = 0f;

    [Tooltip("한 박을 몇 개로 나눌지 설정합니다. 1이면 기본 박, 2면 반박입니다.")]
    [SerializeField] private int subdivision = 1;

    [Header("발판 생성 주기 설정")]
    [Tooltip("몇 박마다 발판을 생성할지 설정합니다. 1이면 매 박, 2이면 2박마다 생성됩니다.")]
    [Min(1)]
    [SerializeField] private int beatsPerPlatformSpawn = 2;

    [Tooltip("정답 발판을 미리 보여줄 시간입니다.")]
    [SerializeField] private float previewLeadTime = 1f;

    [Tooltip("판정 허용 시간입니다.")]
    [SerializeField] private float judgeWindow = 0.3f;

    [Header("발판 생성 설정")]
    [Tooltip("한 비트에서 최소 몇 개의 발판을 생성할지 설정합니다.")]
    [SerializeField] private int minPlatformCount = 2;

    [Tooltip("한 비트에서 최대 몇 개의 발판을 생성할지 설정합니다.")]
    [SerializeField] private int maxPlatformCount = 3;

    [Tooltip("발판 후보 위치 목록입니다.")]
    [SerializeField] private List<Vector2> platformOffsets = new List<Vector2>
    {
        new Vector2(-2f, 0f),
        new Vector2(0f, 0f),
        new Vector2(2f, 0f)
    };

    [Tooltip("발판 X축 랜덤 오프셋입니다.")]
    [SerializeField] private float randomOffsetX = 0.5f;

    [Tooltip("발판 Y축 랜덤 오프셋입니다.")]
    [SerializeField] private float randomOffsetY = 0.25f;

    [Tooltip("해당 비트에서 반드시 밟아야 하는지 여부입니다.")]
    [SerializeField] private bool mustStep = true;

    public float Bpm => bpm;
    public float StartOffset => startOffset;
    public float SecondsPerBeat => bpm > 0f ? 60f / bpm : 0f;
    public float SecondsPerStep => subdivision > 0 ? SecondsPerBeat / subdivision : 0f;

    /// <summary>
    /// 실제 발판 그룹 생성 간격.
    /// 예:
    /// - beatsPerPlatformSpawn = 1 -> 매 박 생성
    /// - beatsPerPlatformSpawn = 2 -> 2박마다 생성
    /// </summary>
    public float SecondsPerPlatformSpawn => SecondsPerBeat * Mathf.Max(1, beatsPerPlatformSpawn);

    /// <summary>
    /// 컴포넌트 부착 시 AudioSource 자동 참조.
    /// </summary>
    private void Reset()
    {
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// 현재 설정값이 런타임 비트맵 생성/재생에 적합한지 검증한다.
    /// 설정이 잘못되면 로그를 남기고 false를 반환한다.
    /// </summary>
    public bool HasValidSetup()
    {
        if (audioSource == null)
        {
            Debug.LogWarning("[RhythmAudioManager] AudioSource가 비어 있습니다.");
            return false;
        }

        if (audioClip == null)
        {
            Debug.LogWarning("[RhythmAudioManager] AudioClip이 비어 있습니다.");
            return false;
        }

        if (bpm <= 0f)
        {
            Debug.LogWarning("[RhythmAudioManager] BPM은 0보다 커야 합니다.");
            return false;
        }

        if (subdivision <= 0)
        {
            Debug.LogWarning("[RhythmAudioManager] subdivision은 1 이상이어야 합니다.");
            return false;
        }

        if (beatsPerPlatformSpawn <= 0)
        {
            Debug.LogWarning("[RhythmAudioManager] beatsPerPlatformSpawn은 1 이상이어야 합니다.");
            return false;
        }

        if (platformOffsets == null || platformOffsets.Count == 0)
        {
            Debug.LogWarning("[RhythmAudioManager] platformOffsets가 비어 있습니다.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 현재 인스펙터 설정을 바탕으로 런타임용 BeatMapData를 새로 생성한다.
    /// ScriptableObject 에셋 파일이 아니라 실행 중 임시 데이터다.
    /// </summary>
    public BeatMapData CreateRuntimeBeatMap()
    {
        if (!HasValidSetup())
        {
            return null;
        }

        BeatMapData runtimeBeatMap = ScriptableObject.CreateInstance<BeatMapData>();
        FillBeatMap(runtimeBeatMap);
        return runtimeBeatMap;
    }

    /// <summary>
    /// 전달받은 BeatMapData에 실제 비트 이벤트를 채운다.
    /// 
    /// 생성 기준:
    /// - duration
    /// - startOffset
    /// - beatsPerPlatformSpawn
    /// - 판정 시간/미리보기 시간
    /// - 발판 후보 위치
    /// </summary>
    public void FillBeatMap(BeatMapData targetBeatMap)
    {
        if (targetBeatMap == null)
        {
            Debug.LogWarning("[RhythmAudioManager] targetBeatMap이 null입니다.");
            return;
        }

        if (!HasValidSetup())
        {
            return;
        }

        targetBeatMap.beatEvents.Clear();

        float duration = GetEffectiveDuration();
        float secondsPerSpawn = SecondsPerPlatformSpawn;

        if (secondsPerSpawn <= 0f)
        {
            Debug.LogWarning("[RhythmAudioManager] 발판 생성 간격 계산에 실패했습니다.");
            return;
        }

        if (duration <= startOffset)
        {
            Debug.LogWarning($"[RhythmAudioManager] duration({duration})이 startOffset({startOffset})보다 작거나 같습니다.");
            return;
        }

        int totalBeatCount = Mathf.FloorToInt((duration - startOffset) / secondsPerSpawn);
        totalBeatCount = Mathf.Max(0, totalBeatCount);

        int clampedMinPlatformCount = Mathf.Clamp(minPlatformCount, 1, 3);
        int clampedMaxPlatformCount = Mathf.Clamp(maxPlatformCount, clampedMinPlatformCount, 3);

        for (int i = 0; i < totalBeatCount; i++)
        {
            float judgeTime = startOffset + secondsPerSpawn * i;

            BeatEvent beatEvent = new BeatEvent();
            beatEvent.previewTime = Mathf.Max(0f, judgeTime - previewLeadTime);
            beatEvent.judgeTime = judgeTime;
            beatEvent.judgeWindow = judgeWindow;
            beatEvent.endTime = judgeTime + judgeWindow;
            beatEvent.minPlatformCount = clampedMinPlatformCount;
            beatEvent.maxPlatformCount = clampedMaxPlatformCount;
            beatEvent.mustStep = mustStep;
            beatEvent.targetPlatformIndex = Random.Range(0, platformOffsets.Count);
            beatEvent.platformOffsets = ClonePlatformOffsets();
            beatEvent.randomOffsetX = randomOffsetX;
            beatEvent.randomOffsetY = randomOffsetY;

            targetBeatMap.beatEvents.Add(beatEvent);
        }

        Debug.Log($"[RhythmAudioManager] 비트맵 생성 완료 - beatCount={targetBeatMap.beatEvents.Count}, beatsPerPlatformSpawn={beatsPerPlatformSpawn}");
    }

    /// <summary>
    /// 퍼즐용 오디오 재생 시작.
    /// 항상 처음부터 재생되도록 Stop -> time=0 -> Play 순서로 초기화한다.
    /// </summary>
    public void Play()
    {
        if (!HasValidSetup())
        {
            return;
        }

        audioSource.clip = audioClip;
        audioSource.playOnAwake = false;
        audioSource.Stop();
        audioSource.time = 0f;
        audioSource.Play();
    }

    /// <summary>
    /// 현재 오디오 재생 정지.
    /// </summary>
    public void Stop()
    {
        if (audioSource == null)
        {
            return;
        }

        audioSource.Stop();
    }

    /// <summary>
    /// 현재 재생 시간을 반환한다.
    /// 퍼즐 판정은 이 시간을 기준으로 수행된다.
    /// </summary>
    public float GetPlaybackTime()
    {
        if (audioSource == null)
        {
            return 0f;
        }

        return audioSource.time;
    }

    /// <summary>
    /// 실제 비트맵 생성에 사용할 길이를 계산한다.
    /// - generateDuration > 0 이면 그 값을 우선 사용
    /// - 단, 오디오 길이를 초과하면 오디오 길이로 보정
    /// - 0이면 오디오 전체 길이 사용
    /// </summary>
    private float GetEffectiveDuration()
    {
        if (generateDuration > 0f)
        {
            if (audioClip != null)
            {
                return Mathf.Min(generateDuration, audioClip.length);
            }

            return generateDuration;
        }

        if (audioClip == null)
        {
            return 0f;
        }

        return audioClip.length;
    }

    /// <summary>
    /// 플랫폼 후보 위치 리스트를 복사해 BeatEvent에 전달한다.
    /// 원본 참조를 직접 넘기지 않는 이유는 런타임 수정이 원본 설정에 영향을 주지 않게 하기 위함이다.
    /// </summary>
    private List<Vector2> ClonePlatformOffsets()
    {
        List<Vector2> clonedOffsets = new List<Vector2>(platformOffsets.Count);

        for (int i = 0; i < platformOffsets.Count; i++)
        {
            clonedOffsets.Add(platformOffsets[i]);
        }

        return clonedOffsets;
    }
}