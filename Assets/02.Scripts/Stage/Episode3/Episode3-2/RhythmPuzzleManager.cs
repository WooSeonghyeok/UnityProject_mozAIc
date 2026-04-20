using System.Collections.Generic;
using UnityEngine;

public class RhythmPuzzleManager : MonoBehaviour
{
    [Header("비트맵 데이터")]
    [SerializeField] private BeatMapData beatMapData;
    [Header("오디오 매니저")]
    [SerializeField] private RhythmAudioManager audioManager;
    [SerializeField] private bool useGeneratedBeatMapFromAudio = true;
    [Header("리듬 이펙트 매니저")]
    [SerializeField] private RhythmEffectManager effectManager;
    [Header("비트 윈도우 매니저")]
    [SerializeField] private RhythmBeatWindowManager beatWindowManager;
    [Header("점수 매니저")]
    [SerializeField] private RhythmScoreManager scoreManager;
    [Header("플레이어 복귀")]
    [SerializeField] private PlayerFallRecovery playerFallRecovery;

    private Ep3_2Manager stageManager;
    // 퍼즐이 현재 진행 중인지 여부
    // false이면 Update 루프에서 아무 로직도 실행하지 않는다.
    private bool isRunning = false;
    // 한 번의 발판 밟기 이벤트를 처리하는 도중인지 여부
    // 플레이어 콜라이더가 여러 개이거나, 한 프레임에 중복 진입이 발생하더라도
    // 동일 입력이 중복 처리되지 않도록 막는 잠금 역할을 한다.
    private bool isResolvingStep = false;
    // 현재 판정 대상으로 보고 있는 비트 인덱스
    // -1이면 현재 판정 대기 중인 비트가 없다는 뜻이다.
    private int currentBeatIndex = -1;
    // 다음에 활성화 시도를 해야 할 비트 인덱스
    // 시간 흐름에 따라 이 값을 증가시키며 비트를 순서대로 소비한다.
    private int nextBeatIndexToActivate = 0;
    // 현재 활성 비트가 이미 정답/오답 판정을 한 번 끝냈는지 여부
    // 비트 진행 자체는 음악 시간이 결정하고,
    // 플레이어 입력은 점수 처리만 담당하도록 하기 위해 사용한다.
    private bool isCurrentBeatResolved = false;

    private void Reset()
    {
        if (beatWindowManager == null)
        {
            beatWindowManager = GetComponent<RhythmBeatWindowManager>();
        }

        if (scoreManager == null)
        {
            scoreManager = GetComponent<RhythmScoreManager>();
        }

        if (effectManager == null)
        {
            effectManager = GetComponent<RhythmEffectManager>();
        }

        if (audioManager == null)
        {
            audioManager = GetComponent<RhythmAudioManager>();
        }

        if (playerFallRecovery == null)
        {
            playerFallRecovery = FindFirstObjectByType<PlayerFallRecovery>();
        }
    }

    private void Update()
    {
        if (!isRunning)
        {
            return;
        }

        float currentTime = GetCurrentPlaybackTime();
        // 비트 활성화/종료는 플레이어 입력이 아니라 음악 시간 기준으로만 처리한다.
        TryActivatePendingBeat(currentTime);
        TryHandleBeatTimeout(currentTime);
    }

    public void Initialize(Ep3_2Manager manager)
    {
        stageManager = manager;
    }
    // 시작 절차:
    // 1. 이전 퍼즐 상태 초기화
    // 2. 비트맵 준비
    // 3. 비트 윈도우 매니저 초기화 및 초기 발판 생성
    // 4. 플레이어 복구 횟수/기본 안전 위치 초기화
    // 5. 오디오 재생 시작
    // 6. 현재 시간 기준으로 활성 가능한 첫 비트 검사
    public void StartPuzzle()
    {
        ResetPuzzleState();

        if (!PrepareBeatMapData())
        {
            return;
        }

        if (beatWindowManager == null)
        {
            Debug.LogWarning("[RhythmPuzzleManager] RhythmBeatWindowManager가 연결되지 않았습니다.");
            return;
        }

        if (scoreManager == null)
        {
            Debug.LogWarning("[RhythmPuzzleManager] RhythmScoreManager가 연결되지 않았습니다.");
            return;
        }

        if (GetTotalBeatCount() <= 0)
        {
            Debug.LogWarning("[RhythmPuzzleManager] 생성할 비트가 없습니다.");
            return;
        }

        isRunning = true;
        isResolvingStep = false;
        currentBeatIndex = -1;
        nextBeatIndexToActivate = 0;
        isCurrentBeatResolved = false;

        beatWindowManager.Initialize(this, beatMapData);
        beatWindowManager.SpawnInitialBeatGroups();

        if (!beatWindowManager.HasAnyActiveGroup)
        {
            Debug.LogWarning("[RhythmPuzzleManager] 초기 발판 생성에 실패했습니다.");
            isRunning = false;
            return;
        }
        // 낙하 복구 관련 상태 초기화
        // 시작 직후에는 fallbackSpawnPoint를 기본 안전 위치로 사용하고,
        // 이후 정답 발판을 밟을 때마다 최근 안전 위치를 갱신한다.
        if (playerFallRecovery != null)
        {
            playerFallRecovery.ResetRecoveryCount();
            playerFallRecovery.ResetSafePointToFallback();
        }

        if (audioManager != null)
        {
            audioManager.Play();
        }

        TryActivatePendingBeat(GetCurrentPlaybackTime());

        Debug.Log($"[RhythmPuzzleManager] 퍼즐 시작 - 총 비트 수: {GetTotalBeatCount()}");
    }
    // 점수, 활성 비트, 오디오, 이펙트, 생성된 발판 그룹까지 모두 정리한다.
    // 재시작/씬 재진입 시 가장 먼저 호출되는 리셋 루틴이다.
    private void ResetPuzzleState()
    {
        currentBeatIndex = -1;
        nextBeatIndexToActivate = 0;
        isResolvingStep = false;
        isRunning = false;
        isCurrentBeatResolved = false;

        if (audioManager != null)
        {
            audioManager.Stop();
        }

        if (effectManager != null)
        {
            // 현재 구조에서는 여러 정답 발판에 표시 이펙트가 동시에 붙을 수 있으므로
            // 리셋 시에는 전체 표시 이펙트를 한 번에 숨긴다.
            effectManager.HideAllTargetIndicators();
        }

        if (scoreManager != null)
        {
            scoreManager.ResetState();
        }

        if (beatWindowManager != null)
        {
            beatWindowManager.ResetState();
        }
    }

    // useGeneratedBeatMapFromAudio가 켜져 있으면
    // 오디오 설정을 기반으로 런타임 비트맵을 생성해 사용한다.
    // 그렇지 않으면 인스펙터에 연결된 beatMapData를 그대로 사용한다.
    private bool PrepareBeatMapData()
    {
        if (useGeneratedBeatMapFromAudio && audioManager != null)
        {
            BeatMapData generatedBeatMap = audioManager.CreateRuntimeBeatMap();
            if (generatedBeatMap != null)
            {
                beatMapData = generatedBeatMap;
            }
        }

        if (beatMapData == null || beatMapData.beatEvents == null || beatMapData.beatEvents.Count == 0)
        {
            Debug.LogWarning("[RhythmPuzzleManager] 사용할 BeatMapData가 없습니다.");
            return false;
        }

        return true;
    }

    private int GetTotalBeatCount()
    {
        if (beatMapData == null || beatMapData.beatEvents == null)
        {
            return 0;
        }

        return beatMapData.beatEvents.Count;
    }
    // 현재 오디오 재생 시간을 반환한다.
    // 오디오 매니저가 없으면 0초로 간주한다.
    private float GetCurrentPlaybackTime()
    {
        if (audioManager == null)
        {
            return 0f;
        }

        return audioManager.GetPlaybackTime();
    }
    // 핵심 규칙:
    // - previewTime 이전이면 아직 기다린다.
    // - 그룹이 없으면 즉시 스킵하지 않고 직접 생성 시도를 한 번 더 한다.
    // - 직접 생성에도 실패한 비트만 최종적으로 스킵한다.
    // - 활성화 직후 이미 판정 시간이 지났다면 미스로 처리하고 다음 비트로 넘긴다.
    private void TryActivatePendingBeat(float currentTime)
    {
        if (currentBeatIndex >= 0)
        {
            return;
        }

        while (nextBeatIndexToActivate < GetTotalBeatCount())
        {
            BeatEvent nextBeatEvent = beatMapData.beatEvents[nextBeatIndexToActivate];
            if (currentTime < nextBeatEvent.previewTime)
            {
                return;
            }

            if (!beatWindowManager.HasBeatGroup(nextBeatIndexToActivate))
            {
                // 순차 생성에서 빠졌더라도 실제로 필요한 순간 한 번 더 생성 기회를 준다.
                bool created = beatWindowManager.EnsureBeatGroupExists(nextBeatIndexToActivate);

                if (!created)
                {
                    Debug.LogWarning($"[RhythmPuzzleManager] {nextBeatIndexToActivate}번째 비트 그룹이 없어 건너뜁니다.");
                    nextBeatIndexToActivate++;
                    continue;
                }
            }

            int beatIndexToActivate = nextBeatIndexToActivate;
            nextBeatIndexToActivate++;

            ActivateBeatTarget(beatIndexToActivate);

            if (currentTime > nextBeatEvent.judgeTime + nextBeatEvent.judgeWindow)
            {
                RegisterMiss();
                AdvanceToNextBeat();
                continue;
            }

            return;
        }

        if (currentBeatIndex < 0 && nextBeatIndexToActivate >= GetTotalBeatCount())
        {
            CompletePuzzle();
        }
    }

    // 현재 구조에서는 비트 진행을 음악 시간이 결정한다.
    // 따라서 플레이어 입력과 관계없이 judgeTime + judgeWindow를 넘기면
    // 그 시점에 현재 비트를 종료하고 다음 비트로 진행한다.
    // 단, 이미 정답/오답 판정이 끝난 비트라면 미스를 주지 않고
    // 조용히 다음 비트로 넘어간다.
    private void TryHandleBeatTimeout(float currentTime)
    {
        if (currentBeatIndex < 0)
        {
            return;
        }

        BeatEvent currentBeatEvent = beatMapData.beatEvents[currentBeatIndex];
        if (currentTime > currentBeatEvent.judgeTime + currentBeatEvent.judgeWindow)
        {
            if (!isCurrentBeatResolved)
            {
                RegisterMiss();
            }

            AdvanceToNextBeat();
        }
    }
    // 동작:
    // - 현재 윈도우에 살아 있는 비트 그룹들의 정답 발판은 모두 유지한다.
    // - 새 비트가 활성화될 때도 기존 정답 표시를 지우지 않는다.
    // - 현재 비트 인덱스를 갱신하고, 해당 비트를 아직 미해결 상태로 만든다.
    // - 정답 표시 이펙트는 현재 화면에 살아 있는 정답 발판 전체에 다시 동기화한다.
    public void ActivateBeatTarget(int beatIndex)
    {
        if (!beatWindowManager.TryGetTargetPlatform(beatIndex, out RhythmPlatform targetPlatform))
        {
            return;
        }
        // 발판 자체의 정답 상태는 현재 살아 있는 비트 그룹 전체에 대해 유지한다.
        beatWindowManager.ActivateAllCurrentTargets();

        currentBeatIndex = beatIndex;
        isCurrentBeatResolved = false;
        // 정답 표시 이펙트도 단일 타겟이 아니라
        // 현재 살아 있는 정답 발판 전체를 대상으로 다시 동기화한다.
        if (effectManager != null && beatWindowManager != null)
        {
            effectManager.HideAllTargetIndicators();

            List<Transform> currentTargetPlatforms = beatWindowManager.GetAllCurrentTargetPlatforms();
            effectManager.ShowTargetIndicatorsForPlatforms(currentTargetPlatforms);
        }
    }
    // 현재 구조에서는 비트 하나가 판정 완료되더라도
    // 그 비트 그룹이 아직 윈도우 안에 살아 있는 동안은 정답 표시를 유지한다.
    // 따라서 여기서는 현재 활성 비트 인덱스와 판정 상태만 정리하고,
    // 정답 표시 이펙트는 남아 있는 정답 발판들을 기준으로 다시 동기화한다.
    // 실제 정답 발판/이펙트 제거는 그룹이 풀 반환될 때 함께 정리된다.
    public void DeactivateBeatTarget(int beatIndex)
    {
        if (currentBeatIndex == beatIndex)
        {
            currentBeatIndex = -1;
        }

        isCurrentBeatResolved = false;

        if (effectManager != null && beatWindowManager != null)
        {
            effectManager.HideAllTargetIndicators();

            List<Transform> currentTargetPlatforms = beatWindowManager.GetAllCurrentTargetPlatforms();
            effectManager.ShowTargetIndicatorsForPlatforms(currentTargetPlatforms);
        }
    }
    // 현재 살아 있는 모든 비트 그룹의 타겟 상태를 비활성화한다.
    // 퍼즐 종료 또는 전체 상태 초기화 시 사용한다.
    private void ClearActiveTargets()
    {
        if (beatWindowManager != null)
        {
            beatWindowManager.ClearAllTargetStates();
        }

        if (effectManager != null)
        {
            effectManager.HideAllTargetIndicators();
        }

        currentBeatIndex = -1;
        isCurrentBeatResolved = false;
    }
    // 판정 순서:
    // 1. 현재 퍼즐 진행 중인지 확인
    // 2. 밟은 발판이 유효한지 확인
    // 3. 현재 비트가 아직 미해결 상태인지 확인
    // 4. 지금 정답 상태로 활성화된 발판인지 확인
    // 5. 점수만 처리하고, 비트 진행은 음악 시간에 맡긴다.
    // 현재 구조에서는 비트 진행을 플레이어 입력이 아니라 음악 시간 기준으로 처리한다.
    // 따라서 발판을 밟았다고 즉시 다음 비트로 넘기지 않는다.
    public void OnPlatformStepped(RhythmPlatform steppedPlatform)
    {
        if (!isRunning) return;
        if (isResolvingStep) return;
        if (steppedPlatform == null) return;
        if (currentBeatIndex < 0) return;
        if (isCurrentBeatResolved) return;

        isResolvingStep = true;
        // 현재 구조에서는 currentBeatIndex의 targetPlatform인지까지 보지 않고
        // 지금 정답 상태로 켜져 있는 발판인지 여부만으로 판정한다.
        if (steppedPlatform.IsActiveTarget)
        {
            if (effectManager != null)
            {
                effectManager.PlaySuccessEffect(steppedPlatform.transform.position);
            }
            // 정답 발판을 밟았으면 이 위치를 최근 안전 위치로 저장한다.
            // 이후 낙하 시 처음 스폰 위치가 아니라 마지막으로 성공한 발판 근처로 복귀하게 된다.
            if (playerFallRecovery != null)
            {
                playerFallRecovery.SaveSafePoint(steppedPlatform.transform);
            }

            RegisterCorrectStep();
            isCurrentBeatResolved = true;
        }
        else
        {
            RegisterWrongStep();
            isCurrentBeatResolved = true;
        }

        isResolvingStep = false;
    }
    // 처리 순서:
    // 1. 현재 비트 판정 상태 정리
    // 2. 다음 비트 그룹 필요 시 생성
    // 3. 현재 시간 기준으로 다음 활성 비트 검사
    // 4. 더 이상 진행할 비트가 없으면 퍼즐 완료
    private void AdvanceToNextBeat()
    {
        int resolvedBeatIndex = currentBeatIndex;

        if (resolvedBeatIndex >= 0)
        {
            DeactivateBeatTarget(resolvedBeatIndex);
        }

        beatWindowManager.SpawnNextBeatGroupIfNeeded();

        if (currentBeatIndex < 0)
        {
            TryActivatePendingBeat(GetCurrentPlaybackTime());
        }

        if (currentBeatIndex < 0 && nextBeatIndexToActivate >= GetTotalBeatCount())
        {
            CompletePuzzle();
        }
    }
    // 정답 처리 위임
    // 실제 점수 수치 계산은 RhythmScoreManager가 담당한다.
    public void RegisterCorrectStep()
    {
        if (!isRunning || scoreManager == null) return;
        scoreManager.RegisterCorrectStep();
    }
    // 오답 처리 위임
    public void RegisterWrongStep()
    {
        if (!isRunning || scoreManager == null) return;
        scoreManager.RegisterWrongStep();
    }
    // 미스 처리 위임
    public void RegisterMiss()
    {
        if (!isRunning || scoreManager == null) return;
        scoreManager.RegisterMiss();
    }
    // 퍼즐 성공 종료 처리
    // - 진행 중지
    // - 활성 타겟 정리
    // - 오디오 정지
    // - 생성된 발판/장식 정리
    // - 스테이지 매니저에 최종 점수 보고
    public void CompletePuzzle()
    {
        if (!isRunning) return;

        isRunning = false;
        ClearActiveTargets();

        if (audioManager != null)
        {
            audioManager.Stop();
        }

        if (beatWindowManager != null)
        {
            beatWindowManager.ClearAllSpawnedBeatGroups();
        }

        if (stageManager != null)
        {
            int finalScore = scoreManager != null ? scoreManager.Score : 0;
            stageManager.OnRhythmPuzzleCompleted(finalScore);
        }
    }
    // 퍼즐 실패 종료 처리
    // 성공과 마찬가지로 상태를 정리한 뒤 실패를 보고한다.
    public void FailPuzzle()
    {
        if (!isRunning) return;

        isRunning = false;
        ClearActiveTargets();

        if (audioManager != null)
        {
            audioManager.Stop();
        }

        if (beatWindowManager != null)
        {
            beatWindowManager.ClearAllSpawnedBeatGroups();
        }

        if (stageManager != null)
        {
            stageManager.OnRhythmPuzzleFailed();
        }
    }
    // 퍼즐을 시작 상태로 다시 되돌린다.
    // 현재 구조에서는 StartPuzzle()가 내부적으로 상태 초기화부터 다시 수행하므로
    // 외부에서는 이 메서드만 호출해도 재시작이 가능하다.
    public void RestartPuzzleFromStart()
    {
        StartPuzzle();
    }
}