using UnityEngine;

public class SoundController : MonoBehaviour
{
    [SerializeField] private SoundProfile profile;

    private void Start()
    {
        if (profile == null) return;
        if (SoundManager.Instance == null) return;

        ApplySceneSound();
        ApplyPlayerSound();
    }

    private void ApplySceneSound()
    {
        if (profile.playBGMOnEnter)
        {
            SoundManager.Instance.PlayBGM(profile.bgm, profile.bgmLoop);
        }

        if (profile.playAmbientOnEnter)
        {
            SoundManager.Instance.PlayAmbient(profile.ambient, profile.ambientLoop);
        }
    }

    private void ApplyPlayerSound()
    {
        PlayerSound playerSound = FindFirstObjectByType<PlayerSound>();
        if (playerSound == null) return;

        playerSound.ApplySceneSoundProfile(profile);
    }
}