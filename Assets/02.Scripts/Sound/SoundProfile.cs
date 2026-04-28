using UnityEngine;

[CreateAssetMenu(fileName = "NewSceneSoundProfile", menuName = "Sound/Scene Sound Profile")]
public class SoundProfile : ScriptableObject
{
    public enum SceneAcousticPreset
    {
        [InspectorName("일반")]
        Normal,
        [InspectorName("먼 기억")]
        DistantMemory
    }

    [Header("BGM 설정")]
    public SoundManager.BGMType bgm = SoundManager.BGMType.None;
    public bool bgmLoop = true;

    [Header("Ambient 설정")]
    public SoundManager.AmbientType ambient = SoundManager.AmbientType.None;
    public bool ambientLoop = true;

    [Header("자동 재생 여부")]
    public bool playBGMOnEnter = true;
    public bool playAmbientOnEnter = true;

    [Header("씬 기본 볼륨")]
    public bool applyVolumeDefaultsOnEnter = true;
    [Range(0f, 1f)] public float defaultMasterVolume = 1f;
    [Range(0f, 1f)] public float defaultBGMVolume = 1f;
    [Range(0f, 1f)] public float defaultAmbientVolume = 1f;
    [Range(0f, 1f)] public float defaultUIVolume = 1f;
    [Range(0f, 1f)] public float defaultSFXVolume = 1f;

    [Header("공간 음향 프리셋")]
    [InspectorName("음향 프리셋")]
    public SceneAcousticPreset acousticPreset = SceneAcousticPreset.Normal;
    [InspectorName("기억 왜곡 강도")]
    [Tooltip("값이 클수록 더 멀고 웅웅대는 느낌이 강해집니다.")]
    [Range(0f, 1f)] public float acousticIntensity = 0f;

    [Header("플레이어 사운드 설정")]
    public SoundManager.SFXType playerFootstep = SoundManager.SFXType.None;
    public SoundManager.SFXType playerJump = SoundManager.SFXType.Jump;
    public SoundManager.SFXType playerLand = SoundManager.SFXType.Land;

    public bool playerFootstepAs3D = true;

    [Range(0f, 1f)] public float playerFootstepVolume = 0.7f;
    [Range(0f, 1f)] public float playerJumpVolume = 1f;
    [Range(0f, 1f)] public float playerLandVolume = 1f;
}
