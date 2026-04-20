using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class SoundManagerLoopExtensions
{
    private static readonly Dictionary<SoundManager, AudioSource> LoopSources = new Dictionary<SoundManager, AudioSource>();
    private static readonly FieldInfo SfxDictField = typeof(SoundManager).GetField("sfxDict", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo MasterVolumeField = typeof(SoundManager).GetField("masterVolume", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo SfxVolumeField = typeof(SoundManager).GetField("sfxVolume", BindingFlags.Instance | BindingFlags.NonPublic);

    public static void PlayLoopSFX(this SoundManager manager, SoundManager.SFXType type, float volumeScale = 1f)
    {
        if (manager == null)
        {
            return;
        }

        if (type == SoundManager.SFXType.None)
        {
            manager.StopLoopSFX();
            return;
        }

        if (!TryGetClip(manager, type, out AudioClip clip))
        {
            Debug.LogWarning($"[SoundManager] Loop SFX clip is not registered: {type}");
            manager.StopLoopSFX();
            return;
        }

        AudioSource loopSource = GetOrCreateLoopSource(manager);

        if (loopSource.clip == clip && loopSource.isPlaying)
        {
            return;
        }

        loopSource.Stop();
        loopSource.clip = clip;
        loopSource.loop = true;
        loopSource.pitch = 1f;
        loopSource.volume = GetSfxVolume(manager) * Mathf.Clamp01(volumeScale);
        loopSource.Play();
    }

    public static void StopLoopSFX(this SoundManager manager)
    {
        if (manager == null)
        {
            return;
        }

        if (!LoopSources.TryGetValue(manager, out AudioSource loopSource) || loopSource == null)
        {
            return;
        }

        loopSource.Stop();
        loopSource.clip = null;
        loopSource.pitch = 1f;
        loopSource.volume = GetSfxVolume(manager);
    }

    private static AudioSource GetOrCreateLoopSource(SoundManager manager)
    {
        if (LoopSources.TryGetValue(manager, out AudioSource existingSource) && existingSource != null)
        {
            return existingSource;
        }

        AudioSource loopSource = manager.gameObject.AddComponent<AudioSource>();
        loopSource.playOnAwake = false;
        loopSource.loop = true;
        loopSource.spatialBlend = 0f;
        loopSource.rolloffMode = AudioRolloffMode.Linear;
        loopSource.minDistance = 1f;
        loopSource.maxDistance = 500f;
        loopSource.priority = 128;
        loopSource.volume = GetSfxVolume(manager);

        LoopSources[manager] = loopSource;
        return loopSource;
    }

    private static bool TryGetClip(SoundManager manager, SoundManager.SFXType type, out AudioClip clip)
    {
        clip = null;

        if (SfxDictField == null)
        {
            return false;
        }

        var sfxDict = SfxDictField.GetValue(manager) as Dictionary<SoundManager.SFXType, AudioClip[]>;
        if (sfxDict == null || !sfxDict.TryGetValue(type, out AudioClip[] clips) || clips == null || clips.Length == 0)
        {
            return false;
        }

        int index = Random.Range(0, clips.Length);
        clip = clips[index];
        return clip != null;
    }

    private static float GetSfxVolume(SoundManager manager)
    {
        return GetFloatValue(manager, MasterVolumeField, 1f) * GetFloatValue(manager, SfxVolumeField, 1f);
    }

    private static float GetFloatValue(SoundManager manager, FieldInfo field, float fallbackValue)
    {
        if (field == null)
        {
            return fallbackValue;
        }

        object value = field.GetValue(manager);
        return value is float floatValue ? floatValue : fallbackValue;
    }
}
