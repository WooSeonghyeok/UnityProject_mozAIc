using UnityEngine;
using UnityEngine.Events;

public class FinalPianoInteraction : MonoBehaviour
{
    [Header("오디오 재생기")]
    [SerializeField] private AudioSource audioSource;

    [Header("재생할 곡 2개")]
    [SerializeField] private AudioClip musicA;
    [SerializeField] private AudioClip musicB;

    [Header("재생 설정")]
    [SerializeField] private bool stopCurrentMusicBeforePlay = true;
    [SerializeField] private bool allowReplayWhilePlaying = false;
    [SerializeField] private bool playOnlyOnce = true;

    [Header("재생 종료 후 호출할 이벤트")]
    [SerializeField] private UnityEvent onMusicFinished;

    [Header("디버그")]
    [SerializeField] private bool debugLog = true;

    private bool hasPlayed = false;
    private Coroutine musicWaitRoutine;

    /// <summary>
    /// 상호작용 시 호출할 함수
    /// 두 곡 중 하나를 랜덤으로 골라 재생한다.
    /// </summary>
    public void PlayRandomMusic()
    {
        if (playOnlyOnce && hasPlayed)
        {
            if (debugLog)
            {
                Debug.Log("[FinalPianoInteraction] 이미 최종 연주가 실행되어 다시 재생하지 않습니다.");
            }
            return;
        }

        if (audioSource == null)
        {
            Debug.LogWarning("[FinalPianoInteraction] AudioSource가 연결되지 않았습니다.");
            return;
        }

        if (musicA == null && musicB == null)
        {
            Debug.LogWarning("[FinalPianoInteraction] 재생할 AudioClip이 없습니다.");
            return;
        }

        if (!allowReplayWhilePlaying && audioSource.isPlaying)
        {
            if (debugLog)
            {
                Debug.Log("[FinalPianoInteraction] 이미 곡이 재생 중이므로 다시 재생하지 않습니다.");
            }
            return;
        }

        AudioClip selectedClip = null;

        if (musicA != null && musicB != null)
        {
            selectedClip = Random.value < 0.5f ? musicA : musicB;
        }
        else if (musicA != null)
        {
            selectedClip = musicA;
        }
        else
        {
            selectedClip = musicB;
        }

        if (stopCurrentMusicBeforePlay)
        {
            audioSource.Stop();
        }

        audioSource.clip = selectedClip;
        audioSource.Play();
        hasPlayed = true;

        if (musicWaitRoutine != null)
        {
            StopCoroutine(musicWaitRoutine);
        }
        musicWaitRoutine = StartCoroutine(CoWaitMusicEnd(selectedClip.length));

        if (debugLog)
        {
            Debug.Log($"[FinalPianoInteraction] 랜덤 곡 재생: {selectedClip.name}");
        }
    }

    /// <summary>
    /// 컷씬 없이 바로 음악 재생 + 저장 처리
    /// </summary>
    public void PlayCutsceneThenMusic()
    {
        PlayRandomMusic();

        if (SaveManager.instance != null && SaveManager.instance.curData != null)
        {
            SaveManager.instance.curData.ep4_open = true;

            if (debugLog)
            {
                Debug.Log("[FinalPianoInteraction] ep4_open 저장 완료");
            }
        }
        else
        {
            Debug.LogWarning("[FinalPianoInteraction] SaveManager 또는 curData가 없습니다.");
        }
    }

    /// <summary>
    /// 현재 재생 중인 곡을 멈춘다.
    /// </summary>
    public void StopMusic()
    {
        if (audioSource == null)
        {
            return;
        }

        audioSource.Stop();

        if (musicWaitRoutine != null)
        {
            StopCoroutine(musicWaitRoutine);
            musicWaitRoutine = null;
        }

        if (debugLog)
        {
            Debug.Log("[FinalPianoInteraction] 곡 재생 중지");
        }
    }

    private System.Collections.IEnumerator CoWaitMusicEnd(float clipLength)
    {
        yield return new WaitForSeconds(clipLength);

        if (debugLog)
        {
            Debug.Log("[FinalPianoInteraction] 곡 재생 종료");
        }

        onMusicFinished?.Invoke();
    }
}