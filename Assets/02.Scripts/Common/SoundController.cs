using UnityEngine;

public class SoundController : MonoBehaviour
{
    [SerializeField] private SoundProfile profile;

    private void Start()
    {
        if (profile == null) return;
        if (SoundManager.Instance == null) return;

        ApplyProfile();
    }

    private void ApplyProfile()
    {
        // 볼륨 먼저 적용
        if (profile.overrideBGMVolume)
            SoundManager.Instance.SetBGMVolume(profile.bgmVolume);

        if (profile.overrideAmbientVolume)
            SoundManager.Instance.SetAmbientVolume(profile.ambientVolume);

        if (profile.overrideUIVolume)
            SoundManager.Instance.SetUIVolume(profile.uiVolume);

        if (profile.overrideSFXVolume)
            SoundManager.Instance.SetSFXVolume(profile.sfxVolume);

        // BGM 적용
        if (profile.playBGMOnEnter)
        {
            SoundManager.Instance.PlayBGM(profile.bgm, profile.bgmLoop);
        }

        // Ambient 적용
        if (profile.playAmbientOnEnter)
        {
            SoundManager.Instance.PlayAmbient(profile.ambient, profile.ambientLoop);
        }
    }
}