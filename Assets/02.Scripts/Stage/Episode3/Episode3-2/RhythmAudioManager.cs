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
    [SerializeField]
    private List<Vector2> platformOffsets = new List<Vector2>
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

    [Header("탑다운 노트 생성")]
    [Tooltip("탑다운 리듬 퍼즐에서 한 박을 몇 개 스텝으로 나눌지 설정합니다.")]
    [Min(1)]
    [SerializeField] private int topDownSubdivision = 4;

    [Tooltip("반박 위치 노트를 생성할 확률입니다.")]
    [Range(0f, 1f)]
    [SerializeField] private float topDownMidBeatChance = 0.95f;

    [Tooltip("오프비트 노트를 생성할 확률입니다.")]
    [Range(0f, 1f)]
    [SerializeField] private float topDownSyncopationChance = 0.72f;

    [Tooltip("탑다운 퍼즐용 판정 허용 시간입니다. 0 이하이면 기본 judgeWindow를 사용합니다.")]
    [SerializeField] private float topDownJudgeWindow = 0.16f;

    [Tooltip("탑다운 노트가 판정선까지 내려오는 데 걸리는 시간입니다. 모든 노트가 같은 속도를 유지합니다.")]
    [Min(0.25f)]
    [SerializeField] private float topDownTravelTime = 2.4f;

    [Tooltip("탑다운 노트 타이밍을 랜덤 대신 고정 패턴으로 생성합니다.")]
    [SerializeField] private bool useDeterministicTopDownPattern = true;

    [Header("탑다운 수동 채보")]
    [Tooltip("연결된 채보 에셋에 노트가 있으면 자동 생성보다 우선해서 사용합니다.")]
    [SerializeField] private bool useTopDownBeatMapAssetFirst = true;

    [Tooltip("멜로디에 맞춰 직접 작성한 탑다운 채보 에셋입니다.")]
    [SerializeField] private BeatMapData topDownBeatMapAsset;

    [Tooltip("수동 채보 전체 판정 시점을 한 번에 앞/뒤로 미세 조정합니다. 양수면 더 늦게 도착합니다.")]
    [SerializeField] private float topDownChartGlobalJudgeOffset = 0f;

    [Tooltip("초반 몇 마디까지 별도 판정 오프셋을 적용할지 설정합니다. 0이면 사용하지 않습니다.")]
    [Min(0)]
    [SerializeField] private int introChartMeasureCount = 0;

    [Tooltip("초반 지정 마디 구간에만 추가로 적용할 판정 오프셋입니다. 양수면 더 늦게 도착합니다.")]
    [SerializeField] private float introChartJudgeOffset = 0f;

    [Tooltip("한 마디를 몇 박으로 볼지 설정합니다. 기본은 4박자입니다.")]
    [Min(1)]
    [SerializeField] private int beatsPerMeasure = 4;

    public float Bpm => bpm;
    public float StartOffset => startOffset;
    public float SecondsPerBeat => bpm > 0f ? 60f / bpm : 0f;
    public float SecondsPerStep => subdivision > 0 ? SecondsPerBeat / subdivision : 0f;
    public bool IsPlaying => audioSource != null && audioSource.isPlaying;
    public float ClipLength => audioClip != null ? audioClip.length : 0f;
    public AudioClip AudioClip => audioClip;
    public BeatMapData TopDownBeatMapAsset => topDownBeatMapAsset;

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
    /// 탑다운 WASD 리듬 퍼즐 전용 비트맵을 생성한다.
    /// 기존 발판 퍼즐과 분리해 더 촘촘하고 동시에 여러 노트가 나오는 패턴을 만든다.
    /// </summary>
    public BeatMapData CreateTopDownRuntimeBeatMap()
    {
        if (!HasValidSetup())
        {
            return null;
        }

        BeatMapData runtimeBeatMap = ScriptableObject.CreateInstance<BeatMapData>();
        if (TryFillTopDownBeatMapFromAsset(runtimeBeatMap))
        {
            return runtimeBeatMap;
        }

        FillTopDownBeatMap(runtimeBeatMap);
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
        int previousLaneIndex = -1;

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
            beatEvent.laneType = GetNextLane(previousLaneIndex);
            beatEvent.platformOffsets = ClonePlatformOffsets();
            beatEvent.randomOffsetX = randomOffsetX;
            beatEvent.randomOffsetY = randomOffsetY;

            targetBeatMap.beatEvents.Add(beatEvent);
            previousLaneIndex = (int)beatEvent.laneType;
        }

        Debug.Log($"[RhythmAudioManager] 비트맵 생성 완료 - beatCount={targetBeatMap.beatEvents.Count}, beatsPerPlatformSpawn={beatsPerPlatformSpawn}");
    }

    public void FillTopDownBeatMap(BeatMapData targetBeatMap)
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
        int stepsPerBeat = Mathf.Max(1, topDownSubdivision);
        float secondsPerStep = SecondsPerBeat / stepsPerBeat;

        if (secondsPerStep <= 0f)
        {
            Debug.LogWarning("[RhythmAudioManager] 탑다운 노트 생성 간격 계산에 실패했습니다.");
            return;
        }

        if (duration <= startOffset)
        {
            Debug.LogWarning($"[RhythmAudioManager] duration({duration})이 startOffset({startOffset})보다 작거나 같습니다.");
            return;
        }

        int totalStepCount = Mathf.FloorToInt((duration - startOffset) / secondsPerStep);
        totalStepCount = Mathf.Max(0, totalStepCount);

        float effectiveJudgeWindow = GetEffectiveTopDownJudgeWindow(secondsPerStep);
        float effectiveTravelTime = GetEffectiveTopDownTravelTime();
        float firstJudgeTime = Mathf.Max(startOffset, effectiveTravelTime);
        int firstStepIndex = Mathf.Max(0, Mathf.CeilToInt((firstJudgeTime - startOffset) / secondsPerStep));
        int previousPrimaryLaneIndex = -1;

        for (int stepIndex = firstStepIndex; stepIndex < totalStepCount; stepIndex++)
        {
            int stepInBeat = stepIndex % stepsPerBeat;

            if (!ShouldSpawnTopDownNote(stepIndex, stepInBeat, stepsPerBeat))
            {
                continue;
            }

            float judgeTime = startOffset + secondsPerStep * stepIndex;
            int noteCount = GetTopDownNoteCount();
            List<Ep3_2LaneType> lanes = BuildTopDownLaneSet(noteCount, ref previousPrimaryLaneIndex);

            for (int i = 0; i < lanes.Count; i++)
            {
                BeatEvent beatEvent = new BeatEvent
                {
                    previewTime = judgeTime - effectiveTravelTime,
                    judgeTime = judgeTime,
                    judgeWindow = effectiveJudgeWindow,
                    endTime = judgeTime + effectiveJudgeWindow,
                    minPlatformCount = 1,
                    maxPlatformCount = 1,
                    mustStep = true,
                    targetPlatformIndex = 0,
                    laneType = lanes[i],
                    isHoldNote = false,
                    holdDuration = 0f,
                    platformOffsets = ClonePlatformOffsets(),
                    randomOffsetX = randomOffsetX,
                    randomOffsetY = randomOffsetY
                };

                targetBeatMap.beatEvents.Add(beatEvent);
            }
        }

        targetBeatMap.beatEvents.Sort((left, right) =>
        {
            int timeCompare = left.judgeTime.CompareTo(right.judgeTime);
            if (timeCompare != 0)
            {
                return timeCompare;
            }

            return left.laneType.CompareTo(right.laneType);
        });

        Debug.Log(
            $"[RhythmAudioManager] 탑다운 비트맵 생성 완료 - noteCount={targetBeatMap.beatEvents.Count}, stepsPerBeat={stepsPerBeat}, judgeWindow={effectiveJudgeWindow:F3}");
    }

    private bool TryFillTopDownBeatMapFromAsset(BeatMapData targetBeatMap)
    {
        if (!useTopDownBeatMapAssetFirst || targetBeatMap == null || topDownBeatMapAsset == null)
        {
            return false;
        }

        if (topDownBeatMapAsset.topDownChartNotes == null || topDownBeatMapAsset.topDownChartNotes.Count == 0)
        {
            return false;
        }

        targetBeatMap.beatEvents.Clear();

        float duration = GetEffectiveDuration();
        float secondsPerStep = Mathf.Max(0.0001f, SecondsPerBeat / Mathf.Max(1, topDownSubdivision));
        float effectiveJudgeWindow = GetEffectiveTopDownJudgeWindow(secondsPerStep);
        float effectiveTravelTime = GetEffectiveTopDownTravelTime();

        List<TopDownChartNote> sortedNotes = new List<TopDownChartNote>(topDownBeatMapAsset.topDownChartNotes);
        sortedNotes.Sort((left, right) => left.judgeTimeSeconds.CompareTo(right.judgeTimeSeconds));

        for (int i = 0; i < sortedNotes.Count; i++)
        {
            TopDownChartNote chartNote = sortedNotes[i];
            if (chartNote == null)
            {
                continue;
            }

            float judgeTime = chartNote.judgeTimeSeconds + topDownChartGlobalJudgeOffset;
            if (ShouldApplyIntroChartOffset(chartNote.judgeTimeSeconds))
            {
                judgeTime += introChartJudgeOffset;
            }
            if (judgeTime < startOffset || judgeTime > duration)
            {
                continue;
            }

            float noteJudgeWindow = chartNote.judgeWindowOverride > 0f
                ? chartNote.judgeWindowOverride
                : effectiveJudgeWindow;

            BeatEvent beatEvent = new BeatEvent
            {
                previewTime = judgeTime - effectiveTravelTime,
                judgeTime = judgeTime,
                judgeWindow = noteJudgeWindow,
                endTime = judgeTime + Mathf.Max(noteJudgeWindow, chartNote.holdDurationSeconds),
                minPlatformCount = 1,
                maxPlatformCount = 1,
                targetPlatformIndex = 0,
                mustStep = true,
                laneType = chartNote.laneType,
                isHoldNote = chartNote.isHoldNote,
                holdDuration = Mathf.Max(0f, chartNote.holdDurationSeconds),
                platformOffsets = ClonePlatformOffsets(),
                randomOffsetX = randomOffsetX,
                randomOffsetY = randomOffsetY
            };

            targetBeatMap.beatEvents.Add(beatEvent);
        }

        Debug.Log(
            $"[RhythmAudioManager] 수동 채보 비트맵 로드 완료 - noteCount={targetBeatMap.beatEvents.Count}, asset={topDownBeatMapAsset.name}");
        return targetBeatMap.beatEvents.Count > 0;
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

    private Ep3_2LaneType GetNextLane(int previousLaneIndex)
    {
        int laneCount = System.Enum.GetValues(typeof(Ep3_2LaneType)).Length;
        int laneIndex = Random.Range(0, laneCount);

        if (laneCount > 1 && previousLaneIndex >= 0 && laneIndex == previousLaneIndex)
        {
            laneIndex = (laneIndex + Random.Range(1, laneCount)) % laneCount;
        }

        return (Ep3_2LaneType)laneIndex;
    }

    private bool ShouldSpawnTopDownNote(int stepIndex, int stepInBeat, int stepsPerBeat)
    {
        if (useDeterministicTopDownPattern)
        {
            return ShouldSpawnTopDownNoteDeterministically(stepIndex, stepInBeat, stepsPerBeat);
        }

        if (stepInBeat == 0)
        {
            return true;
        }

        bool isHalfBeat = stepsPerBeat > 1 && stepsPerBeat % 2 == 0 && stepInBeat == stepsPerBeat / 2;
        if (isHalfBeat)
        {
            return Random.value <= topDownMidBeatChance;
        }

        float chance = topDownSyncopationChance;
        bool isQuarterAccent = stepsPerBeat >= 4 && stepInBeat % Mathf.Max(1, stepsPerBeat / 2) == 0;
        if (isQuarterAccent)
        {
            chance = Mathf.Max(chance, topDownMidBeatChance * 0.8f);
        }

        return Random.value <= chance;
    }

    private bool ShouldSpawnTopDownNoteDeterministically(int stepIndex, int stepInBeat, int stepsPerBeat)
    {
        if (stepInBeat == 0)
        {
            return true;
        }

        int stepsPerMeasure = Mathf.Max(1, stepsPerBeat * 4);
        int stepInMeasure = stepIndex % stepsPerMeasure;
        int measureIndex = stepIndex / stepsPerMeasure;

        if (stepsPerBeat == 1)
        {
            return false;
        }

        if (stepsPerBeat == 2)
        {
            return (measureIndex % 4) switch
            {
                0 => stepInMeasure == 5,
                1 => stepInMeasure == 3 || stepInMeasure == 7,
                2 => stepInMeasure == 1 || stepInMeasure == 5,
                _ => stepInMeasure == 3
            };
        }

        if (stepsPerBeat >= 4)
        {
            int halfBeatStep = stepsPerBeat / 2;
            if (stepInBeat == halfBeatStep)
            {
                return true;
            }

            if (measureIndex % 2 == 1)
            {
                int upbeatStep = Mathf.Max(1, halfBeatStep / 2);
                return stepInBeat == upbeatStep;
            }

            return false;
        }

        return false;
    }

    private int GetTopDownNoteCount()
    {
        return 1;
    }

    private List<Ep3_2LaneType> BuildTopDownLaneSet(int noteCount, ref int previousPrimaryLaneIndex)
    {
        List<Ep3_2LaneType> lanes = new List<Ep3_2LaneType>(noteCount);
        Ep3_2LaneType primaryLane = GetNextLane(previousPrimaryLaneIndex);
        lanes.Add(primaryLane);
        previousPrimaryLaneIndex = (int)primaryLane;

        if (noteCount <= 1)
        {
            return lanes;
        }

        List<Ep3_2LaneType> candidates = new List<Ep3_2LaneType>
        {
            Ep3_2LaneType.D,
            Ep3_2LaneType.F,
            Ep3_2LaneType.Space,
            Ep3_2LaneType.J,
            Ep3_2LaneType.K
        };

        candidates.Remove(primaryLane);

        while (lanes.Count < noteCount && candidates.Count > 0)
        {
            int candidateIndex = Random.Range(0, candidates.Count);
            lanes.Add(candidates[candidateIndex]);
            candidates.RemoveAt(candidateIndex);
        }

        return lanes;
    }

    private float GetEffectiveTopDownJudgeWindow(float secondsPerStep)
    {
        float desiredWindow = topDownJudgeWindow > 0f ? topDownJudgeWindow : judgeWindow;
        float maxSafeWindow = secondsPerStep * 0.45f;
        return Mathf.Max(0.05f, Mathf.Min(desiredWindow, maxSafeWindow));
    }

    private float GetEffectiveTopDownTravelTime()
    {
        return Mathf.Max(0.25f, topDownTravelTime);
    }

    private bool ShouldApplyIntroChartOffset(float rawJudgeTimeSeconds)
    {
        if (introChartMeasureCount <= 0 || Mathf.Approximately(introChartJudgeOffset, 0f))
        {
            return false;
        }

        float secondsPerBeat = SecondsPerBeat;
        if (secondsPerBeat <= 0f)
        {
            return false;
        }

        float introDuration = secondsPerBeat * Mathf.Max(1, beatsPerMeasure) * introChartMeasureCount;
        float introEndTime = startOffset + introDuration;
        return rawJudgeTimeSeconds <= introEndTime;
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
