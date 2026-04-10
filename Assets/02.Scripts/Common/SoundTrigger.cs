using UnityEngine;
public class SoundTrigger : MonoBehaviour
{
    [Header("재생할 효과음")]
    [SerializeField] private SoundManager.SFXType sfxType = SoundManager.SFXType.None;
    [Header("재생 방식")]
    [SerializeField] private bool playAs3D = true;
    [Header("볼륨 배율")]
    [SerializeField][Range(0f, 1f)] private float volumeScale = 1f;
    public void Play()
    {
        if (SoundManager.Instance == null) return;
        if (sfxType == SoundManager.SFXType.None) return;
        if (playAs3D) SoundManager.Instance.PlaySFX3D(sfxType, transform.position, volumeScale);
        else SoundManager.Instance.PlaySFX(sfxType, volumeScale);
    }
    public void PlaySound()
    {
        Play();
    }
}