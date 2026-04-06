using UnityEngine;

[CreateAssetMenu(fileName = "NewSceneSoundProfile", menuName = "Sound/Scene Sound Profile")]
public class SoundProfile : ScriptableObject
{
    [Header("BGM 설정")]
    public SoundManager.BGMType bgm = SoundManager.BGMType.None;
    public bool bgmLoop = true;
    public bool playBGMOnEnter = true;

    [Header("Ambient 설정")]
    public SoundManager.AmbientType ambient = SoundManager.AmbientType.None;
    public bool ambientLoop = true;
    public bool playAmbientOnEnter = true;

    [Header("씬 볼륨 제어 여부")]
    public bool overrideBGMVolume = false;
    [Range(0f, 1f)] public float bgmVolume = 1f;

    public bool overrideAmbientVolume = false;
    [Range(0f, 1f)] public float ambientVolume = 1f;

    public bool overrideUIVolume = false;
    [Range(0f, 1f)] public float uiVolume = 1f;

    public bool overrideSFXVolume = false;
    [Range(0f, 1f)] public float sfxVolume = 1f;
}