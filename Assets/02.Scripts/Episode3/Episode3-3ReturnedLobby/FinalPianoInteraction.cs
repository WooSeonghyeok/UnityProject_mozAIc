using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [Header("디버그")]
    [SerializeField] private bool debugLog = true;
    // 상호작용 시 호출할 함수
    // 두 곡 중 하나를 랜덤으로 골라 재생한다.
    public void PlayRandomMusic()
    {
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
        // 이미 재생 중일 때 다시 재생을 막고 싶으면 여기서 종료
        if (!allowReplayWhilePlaying && audioSource.isPlaying)
        {
            if (debugLog)
            {
                Debug.Log("[FinalPianoInteraction] 이미 곡이 재생 중이므로 다시 재생하지 않습니다.");
            }
            return;
        }
        // 둘 중 하나만 있을 경우 그 곡을 재생
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
        if (debugLog)
        {
            Debug.Log($"[FinalPianoInteraction] 랜덤 곡 재생: {selectedClip.name}");
        }
    }
    // 나중에 컷신을 넣고 싶을 때 확장하기 쉬운 진입 함수
    // 지금은 컷신 없이 바로 랜덤 곡 재생만 수행한다.
    public void PlayCutsceneThenMusic()
    {
        PlayRandomMusic();
        SaveManager.instance.curData.ep4_open = true;
    }
    // 현재 재생 중인 곡을 멈추고 싶을 때 사용할 수 있는 함수
    public void StopMusic()
    {
        if (audioSource == null)
        {
            return;
        }
        audioSource.Stop();
        if (debugLog)
        {
            Debug.Log("[FinalPianoInteraction] 곡 재생 중지");
        }
    }
}