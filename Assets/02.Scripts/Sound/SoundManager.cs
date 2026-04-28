using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
/// <summary>
/// 프로젝트 전체의 사운드를 총괄하는 매니저
/// - BGM
/// - Ambient(환경음)
/// - UI
/// - SFX(상호작용 / 퍼즐 / 점프 / 착지 등)
/// 를 분리해서 관리한다.
/// 
/// 사용 방식:
/// SoundManager.Instance.PlayBGM(BGMType.Title);
/// SoundManager.Instance.PlayAmbient(AmbientType.Wind_Loop);
/// SoundManager.Instance.PlayUI(UIType.Click);
/// SoundManager.Instance.PlaySFX(SFXType.Pickup);
/// SoundManager.Instance.PlaySFX3D(SFXType.DoorOpen, transform.position);
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    public const string MasterVolumeKey = "Volume";
    public const string BGMVolumeKey = "BGM_Volume";
    public const string AmbientVolumeKey = "Ambient_Volume";
    public const string UIVolumeKey = "UI_Volume";
    public const string SFXVolumeKey = "SFX_Volume";
    public const string MasterMuteKey = "Volume_Mute";
    public const string BGMMuteKey = "BGM_Volume_Mute";
    public const string AmbientMuteKey = "Ambient_Volume_Mute";
    public const string UIMuteKey = "UI_Volume_Mute";
    public const string SFXMuteKey = "SFX_Volume_Mute";
    private const string AudioCustomizationKey = "AudioSettingsCustomized";

    #region Enum
    public enum BGMType
    {
        None,
        Title,
        Episode0,
        Episode1_Village,
        Episode1_1,
        Episode1_2,
        Episode2_Studio,
        Episode2_1,
        Episode2_2,
        Episode3_Lobby,
        Episode3_1,
        Episode3_2,
        Episode4,
        Episode4_Finale
    }
    public enum AmbientType
    {
        None,
        Ep0_Loop,
        Ep1_Village_Loop,
        Ep1_1_Loop,
        Ep1_2_Loop,
        Ep2_Studio_Loop,
        Ep2_1_Loop,
        Ep2_2_Loop,
        Ep3_Lobby_Loop,
        Ep3_1_Loop,
        Ep3_2_Loop,
        Ep4_Loop
    }
    public enum UIType
    {
        None,
        Click,
        Hover,
        Confirm,
        Cancel,
        Open,
        Close
    }
    public enum SFXType
    {
        None,
        //플레이어 액션
        Jump,
        Land,
        //플레이어 풋스탭
        Footstep_Ep_Opening,
        Footstep_Ep_0_Lobby,
        Footstep_Ep1_Village,
        Footstep_Ep1_1,
        Footstep_Ep1_2,
        Footstep_Ep2_Studio,
        Footstep_Ep2_1,
        Footstep_Ep2_2,
        Footstep_Ep3_Lobby,
        Footstep_Ep3_1,
        Footstep_Ep3_2,
        Footstep_Ep4,
        //에피소드 공용
        PortalPass,
        // 에피소드 1 전용
        Ep1_1Slide,
        Ep1_1SlideHit,
        Ep1_Village_RockOpen,
        Ep1_Village_StarPickup,
        Ep1_2_StarClear,
        // 에피소드 2 전용
        Ep2_Studio_EnterPicture,
        Ep2_1_PaintCorrect,
        Ep2_1_PaintWrong,
        Ep2_1_PaintFall,
        Ep2_2_ObjectGaze,
        Ep2_2ObjectAppear,
        // 에피소드 3 전용
        Ep3_1_DoorAppear,
        Ep3_1_DoorOpen,
        Ep3_1_DoorPass,
        Ep3_2_TileActive,
        Ep3_2_TileStepCorrect,
        Ep3_2_TileStepWrong,
        // 에피소드 4 전용
        Ep4_Last_LeverPull,
        Ep4_Last_PuzzleComplete,
        // 컷씬 연출음
        CutsceneStart,
        CutsceneEnd
    }
    #endregion
    #region Serializable Entry Classes
    [System.Serializable]
    public class BGMEntry
    {
        public BGMType type;
        public AudioClip clip;
    }
    [System.Serializable]
    public class AmbientEntry
    {
        public AmbientType type;
        public AudioClip clip;
    }
    [System.Serializable]
    public class UIEntry
    {
        public UIType type;
        public AudioClip[] clips;
    }
    [System.Serializable]
    public class SFXEntry
    {
        public SFXType type;
        public AudioClip[] clips;
    }

    [System.Serializable]
    public class SceneSoundEntry
    {
        public string sceneName;
        public SoundProfile profile;
    }
    #endregion
    #region Inspector Fields

    [Header("����� �ҽ�")]
    [SerializeField] private AudioSource bgmSource;      // ������� ����
    [SerializeField] private AudioSource ambientSource;  // ȯ���� ����
    [SerializeField] private AudioSource uiSource;       // UI ���� ����
    [SerializeField] private AudioSource sfxSource;      // �Ϲ� ȿ���� ����
    [SerializeField] private AudioSource loopSfxSource;   // ���� ȿ���� ����

    [Header("오디오 믹서")]
    [SerializeField] private AudioMixer sceneAudioMixer;
    [SerializeField] private AudioMixerGroup bgmMixerGroup;
    [SerializeField] private AudioMixerGroup ambientMixerGroup;
    [SerializeField] private AudioMixerGroup uiMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private AudioMixerGroup loopSfxMixerGroup;
    [SerializeField] private float acousticSnapshotTransitionTime = 0.75f;

    [Header("기억 왜곡 필터")]
    [SerializeField] private float distantBgmLowPassCutoff = 2600f;
    [SerializeField] private float distantAmbientLowPassCutoff = 2200f;
    [SerializeField] private float distantSfxLowPassCutoff = 3000f;
    [SerializeField] private float distantLoopSfxLowPassCutoff = 2800f;
    [SerializeField] private float distant3DSfxLowPassCutoff = 3000f;
    [SerializeField] private AudioReverbPreset distantReverbPreset = AudioReverbPreset.StoneCorridor;
    [SerializeField] private float distantReverbDryLevel = -2000f;
    [SerializeField] private float distantReverbRoom = -1000f;
    [SerializeField] private float distantReverbDecayTime = 1.45f;

    [Header("����")]
    [Range(0f, 1f)][SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float bgmVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float ambientVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float uiVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;
    [Header("기본 볼륨값")]
    [Range(0f, 1f)][SerializeField] private float defaultMasterVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float defaultBGMVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float defaultAmbientVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float defaultUIVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float defaultSFXVolume = 1f;
    [Header("랜덤 피치")]
    [SerializeField] private bool useRandomPitchForSFX = true;
    [SerializeField] private float sfxPitchMin = 0.95f;
    [SerializeField] private float sfxPitchMax = 1.05f;
    [Header("BGM 목록")]
    [SerializeField] private List<BGMEntry> bgmEntries = new List<BGMEntry>();
    [Header("Ambient 목록")]
    [SerializeField] private List<AmbientEntry> ambientEntries = new List<AmbientEntry>();
    [Header("UI 목록")]
    [SerializeField] private List<UIEntry> uiEntries = new List<UIEntry>();
    [Header("SFX 목록")]
    [SerializeField] private List<SFXEntry> sfxEntries = new List<SFXEntry>();
    [Header("씬별 사운드 프로필 목록")]
    [SerializeField] private List<SceneSoundEntry> sceneSoundEntries = new List<SceneSoundEntry>();
    [Header("씬 로드 시 자동 BGM 설정 여부")]
    [SerializeField] private bool useAutoSceneBGM = true;
    [Header("씬 로드 시 자동 Ambient 설정 여부")]
    [SerializeField] private bool useAutoSceneAmbient = true;
    #endregion
    #region Runtime Dictionaries
    private Dictionary<BGMType, AudioClip> bgmDict = new Dictionary<BGMType, AudioClip>();
    private Dictionary<AmbientType, AudioClip> ambientDict = new Dictionary<AmbientType, AudioClip>();
    private Dictionary<UIType, AudioClip[]> uiDict = new Dictionary<UIType, AudioClip[]>();
    private Dictionary<SFXType, AudioClip[]> sfxDict = new Dictionary<SFXType, AudioClip[]>();
    private Dictionary<string, SoundProfile> sceneSoundDict = new Dictionary<string, SoundProfile>();
    private float resetMasterVolume;
    private float resetBGMVolume;
    private float resetAmbientVolume;
    private float resetUIVolume;
    private float resetSFXVolume;
    public bool isMasterMute = false;
    public bool isBGMMute = false;
    public bool isAmbientMute = false;
    public bool isUIMute = false;
    public bool isSFXMute = false;
    private bool audioCustomizedByUser;
    private readonly Dictionary<AudioSource, AudioLowPassFilter> lowPassFilters = new Dictionary<AudioSource, AudioLowPassFilter>();
    private readonly Dictionary<AudioSource, AudioReverbFilter> reverbFilters = new Dictionary<AudioSource, AudioReverbFilter>();
    private SoundProfile.SceneAcousticPreset currentAcousticPreset = SoundProfile.SceneAcousticPreset.Normal;
    private float currentAcousticIntensity;
    private AudioMixerSnapshot normalSnapshot;
    private AudioMixerSnapshot distantSnapshot;
    #endregion
    #region Unity Life Cycle
    private void Awake()
    {
        // 싱글톤 중복 생성 방지
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        EnsureDedicatedAudioSources();
        AssignMixerOutputs();
        CacheMixerSnapshots();
        EnsureAcousticFilters();
        ApplySceneAcousticPreset(SoundProfile.SceneAcousticPreset.Normal, 0f, true);
        BuildDictionaries();
        BuildSceneSoundDictionary();
        SetResetTargetsToProjectDefaults();
        InitializeAudioSettingsIfNeeded();
        LoadAudioSettings();
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log("SoundManager Awake CALLED");
    }
    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
    #endregion
    #region Initialize
    /// <summary>
    /// Inspector에서 등록한 리스트를 Dictionary로 변환
    /// 런타임 재생 속도와 관리 편의를 위해 사용
    /// </summary>
    private void BuildDictionaries()
    {
        bgmDict.Clear();
        foreach (var entry in bgmEntries)
        {
            if (entry == null) continue;
            if (entry.clip == null) continue;
            if (bgmDict.ContainsKey(entry.type)) continue;
            bgmDict.Add(entry.type, entry.clip);
        }
        ambientDict.Clear();
        foreach (var entry in ambientEntries)
        {
            if (entry == null) continue;
            if (entry.clip == null) continue;
            if (ambientDict.ContainsKey(entry.type)) continue;
            ambientDict.Add(entry.type, entry.clip);
        }
        uiDict.Clear();
        foreach (var entry in uiEntries)
        {
            if (entry == null) continue;
            if (entry.clips == null || entry.clips.Length == 0) continue;
            if (uiDict.ContainsKey(entry.type)) continue;
            uiDict.Add(entry.type, entry.clips);
        }
        sfxDict.Clear();
        foreach (var entry in sfxEntries)
        {
            if (entry == null) continue;
            if (entry.clips == null || entry.clips.Length == 0) continue;
            if (sfxDict.ContainsKey(entry.type)) continue;
            sfxDict.Add(entry.type, entry.clips);
        }
    }

    private void BuildSceneSoundDictionary()
    {
        sceneSoundDict.Clear();

        foreach (var entry in sceneSoundEntries)
        {
            if (entry == null) continue;
            if (string.IsNullOrWhiteSpace(entry.sceneName)) continue;
            if (entry.profile == null) continue;
            if (sceneSoundDict.ContainsKey(entry.sceneName)) continue;

            sceneSoundDict.Add(entry.sceneName, entry.profile);
        }
    }

    private void EnsureDedicatedAudioSources()
    {
        bgmSource = EnsureDedicatedAudioSource(bgmSource, "BGMSource");
        ambientSource = EnsureDedicatedAudioSource(ambientSource, "AmbientSource");
        uiSource = EnsureDedicatedAudioSource(uiSource, "UISource");
        sfxSource = EnsureDedicatedAudioSource(sfxSource, "SFXSource");
        loopSfxSource = EnsureDedicatedAudioSource(loopSfxSource, "LoopSFXSource");
    }

    private AudioSource EnsureDedicatedAudioSource(AudioSource source, string childName)
    {
        if (source == null)
        {
            return null;
        }

        if (source.gameObject != gameObject)
        {
            return source;
        }

        Transform existingChild = transform.Find(childName);
        if (existingChild != null)
        {
            AudioSource existingSource = existingChild.GetComponent<AudioSource>();
            if (existingSource != null)
            {
                source.enabled = false;
                source.playOnAwake = false;
                return existingSource;
            }
        }

        GameObject child = new GameObject(childName);
        child.transform.SetParent(transform, false);

        AudioSource dedicatedSource = child.AddComponent<AudioSource>();
        CopyAudioSourceSettings(source, dedicatedSource);

        source.enabled = false;
        source.playOnAwake = false;
        source.clip = null;

        return dedicatedSource;
    }

    private void CopyAudioSourceSettings(AudioSource source, AudioSource target)
    {
        if (source == null || target == null)
        {
            return;
        }

        target.clip = source.clip;
        target.outputAudioMixerGroup = source.outputAudioMixerGroup;
        target.playOnAwake = source.playOnAwake;
        target.loop = source.loop;
        target.mute = source.mute;
        target.bypassEffects = source.bypassEffects;
        target.bypassListenerEffects = source.bypassListenerEffects;
        target.bypassReverbZones = source.bypassReverbZones;
        target.priority = source.priority;
        target.volume = source.volume;
        target.pitch = source.pitch;
        target.panStereo = source.panStereo;
        target.spatialBlend = source.spatialBlend;
        target.reverbZoneMix = source.reverbZoneMix;
        target.dopplerLevel = source.dopplerLevel;
        target.spread = source.spread;
        target.rolloffMode = source.rolloffMode;
        target.minDistance = source.minDistance;
        target.maxDistance = source.maxDistance;
        target.spatialize = source.spatialize;
        target.spatializePostEffects = source.spatializePostEffects;
        target.SetCustomCurve(AudioSourceCurveType.CustomRolloff, source.GetCustomCurve(AudioSourceCurveType.CustomRolloff));
        target.SetCustomCurve(AudioSourceCurveType.Spread, source.GetCustomCurve(AudioSourceCurveType.Spread));
        target.SetCustomCurve(AudioSourceCurveType.SpatialBlend, source.GetCustomCurve(AudioSourceCurveType.SpatialBlend));
        target.SetCustomCurve(AudioSourceCurveType.ReverbZoneMix, source.GetCustomCurve(AudioSourceCurveType.ReverbZoneMix));
    }

    private void AssignMixerOutputs()
    {
        if (bgmSource != null && bgmMixerGroup != null)
        {
            bgmSource.outputAudioMixerGroup = bgmMixerGroup;
        }

        if (ambientSource != null && ambientMixerGroup != null)
        {
            ambientSource.outputAudioMixerGroup = ambientMixerGroup;
        }

        if (uiSource != null && uiMixerGroup != null)
        {
            uiSource.outputAudioMixerGroup = uiMixerGroup;
        }

        if (sfxSource != null && sfxMixerGroup != null)
        {
            sfxSource.outputAudioMixerGroup = sfxMixerGroup;
        }

        if (loopSfxSource != null)
        {
            AudioMixerGroup group = loopSfxMixerGroup != null ? loopSfxMixerGroup : sfxMixerGroup;
            if (group != null)
            {
                loopSfxSource.outputAudioMixerGroup = group;
            }
        }
    }

    private void CacheMixerSnapshots()
    {
        if (sceneAudioMixer == null)
        {
            normalSnapshot = null;
            distantSnapshot = null;
            return;
        }

        normalSnapshot = sceneAudioMixer.FindSnapshot("Normal");
        distantSnapshot = sceneAudioMixer.FindSnapshot("Distant");
    }

    private void EnsureAcousticFilters()
    {
        EnsureAcousticFiltersForSource(bgmSource);
        EnsureAcousticFiltersForSource(ambientSource);
        EnsureAcousticFiltersForSource(sfxSource);
        EnsureAcousticFiltersForSource(loopSfxSource);
    }

    private void EnsureAcousticFiltersForSource(AudioSource source)
    {
        if (source == null)
        {
            return;
        }

        if (!lowPassFilters.TryGetValue(source, out AudioLowPassFilter lowPass) || lowPass == null)
        {
            lowPass = source.GetComponent<AudioLowPassFilter>();
            if (lowPass == null)
            {
                lowPass = source.gameObject.AddComponent<AudioLowPassFilter>();
            }

            lowPass.enabled = false;
            lowPassFilters[source] = lowPass;
        }

        if (!reverbFilters.TryGetValue(source, out AudioReverbFilter reverb) || reverb == null)
        {
            reverb = source.GetComponent<AudioReverbFilter>();
            if (reverb == null)
            {
                reverb = source.gameObject.AddComponent<AudioReverbFilter>();
            }

            reverb.enabled = false;
            reverbFilters[source] = reverb;
        }
    }

    private void ApplySceneAcousticPreset(SoundProfile.SceneAcousticPreset preset, float intensity, bool immediate = false)
    {
        currentAcousticPreset = preset;
        currentAcousticIntensity = Mathf.Clamp01(intensity);

        bool useDistantMemoryFilters = preset == SoundProfile.SceneAcousticPreset.DistantMemory && currentAcousticIntensity > 0.001f;

        AudioMixerSnapshot targetSnapshot = useDistantMemoryFilters && currentAcousticIntensity > 0.4f
            ? distantSnapshot
            : normalSnapshot;

        if (targetSnapshot != null)
        {
            targetSnapshot.TransitionTo(immediate ? 0f : acousticSnapshotTransitionTime);
        }

        ConfigureAcousticFilters(bgmSource, useDistantMemoryFilters, Mathf.Lerp(22000f, distantBgmLowPassCutoff, currentAcousticIntensity));
        ConfigureAcousticFilters(ambientSource, useDistantMemoryFilters, Mathf.Lerp(22000f, distantAmbientLowPassCutoff, currentAcousticIntensity));
        ConfigureAcousticFilters(sfxSource, useDistantMemoryFilters, Mathf.Lerp(22000f, distantSfxLowPassCutoff, currentAcousticIntensity));
        ConfigureAcousticFilters(loopSfxSource, useDistantMemoryFilters, Mathf.Lerp(22000f, distantLoopSfxLowPassCutoff, currentAcousticIntensity));
        ApplyVolumes();
    }

    private void ConfigureAcousticFilters(AudioSource source, bool enabled, float lowPassCutoff)
    {
        if (source == null)
        {
            return;
        }

        EnsureAcousticFiltersForSource(source);

        if (!lowPassFilters.TryGetValue(source, out AudioLowPassFilter lowPass) || lowPass == null)
        {
            return;
        }

        if (!reverbFilters.TryGetValue(source, out AudioReverbFilter reverb) || reverb == null)
        {
            return;
        }

        lowPass.enabled = enabled;
        lowPass.cutoffFrequency = lowPassCutoff;
        lowPass.lowpassResonanceQ = 1.1f;

        reverb.enabled = enabled;
        if (enabled)
        {
            reverb.reverbPreset = distantReverbPreset;
            reverb.dryLevel = Mathf.Lerp(0f, distantReverbDryLevel, currentAcousticIntensity);
            reverb.room = Mathf.RoundToInt(Mathf.Lerp(0f, distantReverbRoom, currentAcousticIntensity));
            reverb.decayTime = Mathf.Lerp(1f, distantReverbDecayTime, currentAcousticIntensity);
        }
    }

    private void ConfigureTempSourceAcousticFilters(AudioSource source)
    {
        if (source == null || currentAcousticPreset != SoundProfile.SceneAcousticPreset.DistantMemory || currentAcousticIntensity <= 0.001f)
        {
            return;
        }

        AudioLowPassFilter lowPass = source.gameObject.AddComponent<AudioLowPassFilter>();
        lowPass.cutoffFrequency = Mathf.Lerp(22000f, distant3DSfxLowPassCutoff, currentAcousticIntensity);
        lowPass.lowpassResonanceQ = 1.1f;

        AudioReverbFilter reverb = source.gameObject.AddComponent<AudioReverbFilter>();
        reverb.reverbPreset = distantReverbPreset;
        reverb.dryLevel = Mathf.Lerp(0f, distantReverbDryLevel, currentAcousticIntensity);
        reverb.room = Mathf.RoundToInt(Mathf.Lerp(0f, distantReverbRoom, currentAcousticIntensity));
        reverb.decayTime = Mathf.Lerp(1f, distantReverbDecayTime, currentAcousticIntensity);
    }
    /// <summary>
    /// 각 소스에 볼륨 적용
    /// Master Volume도 함께 곱해서 최종 볼륨을 만든다.
    /// </summary>
    public void InitializeAudioSettingsIfNeeded()
    {
        bool hasChanges = false;

        hasChanges |= InitializeVolumePrefIfMissing(MasterVolumeKey, defaultMasterVolume);
        hasChanges |= InitializeVolumePrefIfMissing(BGMVolumeKey, defaultBGMVolume);
        hasChanges |= InitializeVolumePrefIfMissing(AmbientVolumeKey, defaultAmbientVolume);
        hasChanges |= InitializeVolumePrefIfMissing(UIVolumeKey, defaultUIVolume);
        hasChanges |= InitializeVolumePrefIfMissing(SFXVolumeKey, defaultSFXVolume);

        if (hasChanges)
        {
            PlayerPrefs.Save();
        }
    }

    private bool InitializeVolumePrefIfMissing(string key, float defaultValue)
    {
        if (PlayerPrefs.HasKey(key))
        {
            return false;
        }

        PlayerPrefs.SetFloat(key, Mathf.Clamp01(defaultValue));
        return true;
    }

    public void LoadAudioSettings()
    {
        masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, defaultMasterVolume);
        bgmVolume = PlayerPrefs.GetFloat(BGMVolumeKey, defaultBGMVolume);
        ambientVolume = PlayerPrefs.GetFloat(AmbientVolumeKey, defaultAmbientVolume);
        uiVolume = PlayerPrefs.GetFloat(UIVolumeKey, defaultUIVolume);
        sfxVolume = PlayerPrefs.GetFloat(SFXVolumeKey, defaultSFXVolume);
        isMasterMute = PlayerPrefs.GetInt(MasterMuteKey, 0) == 1;
        isBGMMute = PlayerPrefs.GetInt(BGMMuteKey, 0) == 1;
        isAmbientMute = PlayerPrefs.GetInt(AmbientMuteKey, 0) == 1;
        isUIMute = PlayerPrefs.GetInt(UIMuteKey, 0) == 1;
        isSFXMute = PlayerPrefs.GetInt(SFXMuteKey, 0) == 1;
        audioCustomizedByUser = PlayerPrefs.GetInt(AudioCustomizationKey, 0) == 1;
        ApplyVolumes();
    }

    public void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat(MasterVolumeKey, masterVolume);
        PlayerPrefs.SetFloat(BGMVolumeKey, bgmVolume);
        PlayerPrefs.SetFloat(AmbientVolumeKey, ambientVolume);
        PlayerPrefs.SetFloat(UIVolumeKey, uiVolume);
        PlayerPrefs.SetFloat(SFXVolumeKey, sfxVolume);
        PlayerPrefs.SetInt(AudioCustomizationKey, audioCustomizedByUser ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ResetAudioSettingsToDefaults(bool saveImmediately = true, bool clearCustomization = true)
    {
        masterVolume = resetMasterVolume;
        bgmVolume = resetBGMVolume;
        ambientVolume = resetAmbientVolume;
        uiVolume = resetUIVolume;
        sfxVolume = resetSFXVolume;
        ApplyVolumes();

        if (clearCustomization)
        {
            audioCustomizedByUser = false;
        }

        if (saveImmediately)
        {
            SaveAudioSettings();
        }
    }

    public void ApplyVolumes()
    {
        AudioListener.volume = masterVolume * (isMasterMute ? 0 : 1);
        float bgmAcousticMultiplier = Mathf.Lerp(1f, 0.62f, currentAcousticIntensity);
        float ambientAcousticMultiplier = Mathf.Lerp(1f, 0.58f, currentAcousticIntensity);
        float sfxAcousticMultiplier = Mathf.Lerp(1f, 0.72f, currentAcousticIntensity);
        if (bgmSource != null) bgmSource.volume = masterVolume * bgmVolume * bgmAcousticMultiplier * (isBGMMute ? 0 : 1);
        if (ambientSource != null) ambientSource.volume = masterVolume * ambientVolume * ambientAcousticMultiplier * (isAmbientMute ? 0 : 1);
        if (uiSource != null) uiSource.volume = masterVolume * uiVolume * (isUIMute ? 0 : 1);
        if (sfxSource != null) sfxSource.volume = masterVolume * sfxVolume * sfxAcousticMultiplier * (isSFXMute ? 0 : 1);
        if (loopSfxSource != null) loopSfxSource.volume = masterVolume * sfxVolume * sfxAcousticMultiplier * (isSFXMute ? 0 : 1);
    }
    #endregion
    #region Scene Loaded
    /// <summary>
    /// 씬이 로드될 때 자동으로 BGM / Ambient를 바꾸고 싶을 때 사용
    /// 씬 이름으로 연결된 SoundProfile을 찾아서 적용한다.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (sceneSoundDict.TryGetValue(scene.name, out SoundProfile profile))
        {
            ApplySoundProfile(profile);
            return;
        }

        SetResetTargetsToProjectDefaults();
        ApplySceneAcousticPreset(SoundProfile.SceneAcousticPreset.Normal, 0f);

        if (useAutoSceneBGM)
        {
            StopBGM();
        }

        if (useAutoSceneAmbient)
        {
            StopAmbient();
        }
    }

    public void ApplySoundProfile(SoundProfile profile, bool applyPlayerSound = true)
    {
        if (profile == null)
        {
            return;
        }

        SetResetTargets(profile);

        if (profile.applyVolumeDefaultsOnEnter && !audioCustomizedByUser)
        {
            ApplySceneVolumeDefaults(profile);
        }

        ApplySceneAcousticPreset(profile.acousticPreset, profile.acousticIntensity);

        if (useAutoSceneBGM)
        {
            if (profile.playBGMOnEnter)
            {
                PlayBGM(profile.bgm, profile.bgmLoop);
            }
            else
            {
                StopBGM();
            }
        }

        if (useAutoSceneAmbient)
        {
            if (profile.playAmbientOnEnter)
            {
                PlayAmbient(profile.ambient, profile.ambientLoop);
            }
            else
            {
                StopAmbient();
            }
        }

        if (applyPlayerSound)
        {
            PlayerSound playerSound = FindFirstObjectByType<PlayerSound>();
            if (playerSound != null)
            {
                playerSound.ApplySceneSoundProfile(profile);
            }
        }
    }

    private void SetResetTargetsToProjectDefaults()
    {
        resetMasterVolume = Mathf.Clamp01(defaultMasterVolume);
        resetBGMVolume = Mathf.Clamp01(defaultBGMVolume);
        resetAmbientVolume = Mathf.Clamp01(defaultAmbientVolume);
        resetUIVolume = Mathf.Clamp01(defaultUIVolume);
        resetSFXVolume = Mathf.Clamp01(defaultSFXVolume);
    }

    private void SetResetTargets(SoundProfile profile)
    {
        if (profile == null)
        {
            SetResetTargetsToProjectDefaults();
            return;
        }

        resetMasterVolume = Mathf.Clamp01(profile.defaultMasterVolume);
        resetBGMVolume = Mathf.Clamp01(profile.defaultBGMVolume);
        resetAmbientVolume = Mathf.Clamp01(profile.defaultAmbientVolume);
        resetUIVolume = Mathf.Clamp01(profile.defaultUIVolume);
        resetSFXVolume = Mathf.Clamp01(profile.defaultSFXVolume);
    }

    public void ApplySceneVolumeDefaults(SoundProfile profile, bool saveImmediately = true)
    {
        SetResetTargets(profile);
        ResetAudioSettingsToDefaults(saveImmediately, clearCustomization: true);
    }
    #endregion
    #region BGM
    /// <summary>
    /// BGM 재생
    /// 이미 같은 곡이 재생 중이면 다시 재생하지 않는다.
    /// </summary>
    public void PlayBGM(BGMType type, bool loop = true)
    {
        if (bgmSource == null) return;
        if (type == BGMType.None)
        {
            StopBGM();
            return;
        }
        if (!bgmDict.TryGetValue(type, out AudioClip clip) || clip == null)
        {
            Debug.LogWarning($"[SoundManager] BGM 클립이 등록되지 않음: {type} -> 기존 BGM 정지");
            StopBGM();
            return;
        }
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }
    public void StopBGM()
    {
        if (bgmSource == null) return;
        bgmSource.Stop();
        bgmSource.clip = null;
    }
    #endregion
    #region Ambient
    /// <summary>
    /// 환경음 루프 재생
    public void PlayAmbient(AmbientType type, bool loop = true)
    {
        if (ambientSource == null) return;
        if (type == AmbientType.None)
        {
            StopAmbient();
            return;
        }
        if (!ambientDict.TryGetValue(type, out AudioClip clip) || clip == null)
        {
            Debug.LogWarning($"[SoundManager] Ambient 클립이 등록되지 않음: {type} -> 기존 Ambient 정지");
            StopAmbient();
            return;
        }
        if (ambientSource.clip == clip && ambientSource.isPlaying) return;
        ambientSource.clip = clip;
        ambientSource.loop = loop;
        ambientSource.Play();
    }
    public void StopAmbient()
    {
        if (ambientSource == null) return;
        ambientSource.Stop();
        ambientSource.clip = null;
    }
    #endregion
    #region UI
    /// <summary>
    /// UI 사운드 재생
    /// - 여러 클립이 있으면 랜덤으로 선택
    /// - PlayOneShot으로 다른 UI 효과와 자연스럽게 겹칠 수 있음
    /// </summary>
    public void PlayUI(UIType type, float volumeScale = 1f)
    {
        if (type == UIType.None) return;
        if (uiSource == null) return;
        if (!uiDict.TryGetValue(type, out AudioClip[] clips))
        {
            Debug.LogWarning($"[SoundManager] UI 클립이 등록되지 않음: {type}");
            return;
        }
        AudioClip clip = GetRandomClip(clips);
        if (clip == null) return;
        volumeScale = Mathf.Clamp01(volumeScale);
        uiSource.PlayOneShot(clip, masterVolume * uiVolume * volumeScale);
    }
    #endregion
    #region SFX
    /// <summary>
    /// 일반 2D 효과음 재생
    /// - 상호작용
    /// - 퍼즐
    /// - 플레이어 점프 / 착지
    /// 같은 전역/평면적인 효과음에 사용
    /// </summary>
    public void PlaySFX(SFXType type, float volumeScale = 1f)
    {
        if (type == SFXType.None) return;
        if (sfxSource == null) return;
        if (!sfxDict.TryGetValue(type, out AudioClip[] clips))
        {
            Debug.LogWarning($"[SoundManager] SFX 클립이 등록되지 않음: {type}");
            return;
        }
        AudioClip clip = GetRandomClip(clips);
        if (clip == null) return;
        float oldPitch = sfxSource.pitch;
        if (useRandomPitchForSFX)
            sfxSource.pitch = Random.Range(sfxPitchMin, sfxPitchMax);
        volumeScale = Mathf.Clamp01(volumeScale);
        sfxSource.PlayOneShot(clip, masterVolume * sfxVolume * volumeScale);
        sfxSource.pitch = oldPitch;
    }
    /// <summary>
    /// ������ SFX ���
    /// - �����̵�
    /// </summary>
    public void PlayLoopSFX(SFXType type, float volumeScale = 1f)
    {
        if (type == SFXType.None) return;
        if (loopSfxSource == null) return;

        if (!sfxDict.TryGetValue(type, out AudioClip[] clips))
        {
            Debug.LogWarning($"[SoundManager] Loop SFX Ŭ���� ��ϵ��� ����: {type}");
            return;
        }

        AudioClip clip = GetRandomClip(clips);
        if (clip == null) return;

        // �̹� ���� ���� SFX�� ��� ���̸� �ߺ� ��� �� ��
        if (loopSfxSource.isPlaying && loopSfxSource.clip == clip)
            return;

        loopSfxSource.Stop();
        loopSfxSource.clip = clip;
        loopSfxSource.loop = true;
        loopSfxSource.volume = masterVolume * sfxVolume * Mathf.Clamp01(volumeScale);
        loopSfxSource.Play();
    }

    /// <summary>
    /// ���� ��� ���� ������ SFX ����
    /// </summary>
    public void StopLoopSFX()
    {
        if (loopSfxSource == null) return;

        loopSfxSource.Stop();
        loopSfxSource.clip = null;
        loopSfxSource.loop = false;
    }
    /// <summary>
    /// 3D ��ġ ��� ȿ���� ���
    /// - �� ���� �Ҹ�
    /// - ��Ż ��� �Ҹ�
    /// - ������Ʈ ��ȣ�ۿ� �Ҹ�
    /// � ���
    /// </summary>
    public void PlaySFX3D(SFXType type, Vector3 position, float volumeScale = 1f)
    {
        if (type == SFXType.None) return;
        if (!sfxDict.TryGetValue(type, out AudioClip[] clips))
        {
            Debug.LogWarning($"[SoundManager] 3D SFX 클립이 등록되지 않음: {type}");
            return;
        }
        AudioClip clip = GetRandomClip(clips);
        if (clip == null) return;
        GameObject tempObj = new GameObject($"Temp3DSFX_{type}");
        tempObj.transform.position = position;
        AudioSource tempSource = tempObj.AddComponent<AudioSource>();
        tempSource.clip = clip;
        if (sfxMixerGroup != null)
        {
            tempSource.outputAudioMixerGroup = sfxMixerGroup;
        }
        tempSource.volume = masterVolume * sfxVolume * volumeScale;
        tempSource.spatialBlend = 1f; // 3D 사운드
        tempSource.rolloffMode = AudioRolloffMode.Linear;
        tempSource.minDistance = 1f;
        tempSource.maxDistance = 15f;
        if (useRandomPitchForSFX)
            tempSource.pitch = Random.Range(sfxPitchMin, sfxPitchMax);
        ConfigureTempSourceAcousticFilters(tempSource);
        tempSource.Play();
        Destroy(tempObj, clip.length + 0.1f);
    }
    #endregion
    #region Utility
    /// <summary>
    /// 배열 안에서 랜덤 클립 하나를 고른다.
    /// </summary>
    private AudioClip GetRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        int index = Random.Range(0, clips.Length);
        return clips[index];
    }
    #endregion
    #region Volume Control
    public float MasterVolume => masterVolume;
    public float BGMVolume => bgmVolume;
    public float AmbientVolume => ambientVolume;
    public float UIVolume => uiVolume;
    public float SFXVolume => sfxVolume;
    public float ResetMasterVolume => resetMasterVolume;
    public float ResetBGMVolume => resetBGMVolume;
    public float ResetAmbientVolume => resetAmbientVolume;
    public float ResetUIVolume => resetUIVolume;
    public float ResetSFXVolume => resetSFXVolume;
    public float DefaultMasterVolume => defaultMasterVolume;
    public float DefaultBGMVolume => defaultBGMVolume;
    public float DefaultAmbientVolume => defaultAmbientVolume;
    public float DefaultUIVolume => defaultUIVolume;
    public float DefaultSFXVolume => defaultSFXVolume;

    public void SetMasterVolume(float value, bool save = true)
    {
        masterVolume = Mathf.Clamp01(value);
        audioCustomizedByUser = true;
        ApplyVolumes();

        if (save)
        {
            SaveAudioSettings();
        }
    }
    public void SetBGMVolume(float value, bool save = true)
    {
        bgmVolume = Mathf.Clamp01(value);
        audioCustomizedByUser = true;
        ApplyVolumes();

        if (save)
        {
            SaveAudioSettings();
        }
    }
    public void SetAmbientVolume(float value, bool save = true)
    {
        ambientVolume = Mathf.Clamp01(value);
        audioCustomizedByUser = true;
        ApplyVolumes();

        if (save)
        {
            SaveAudioSettings();
        }
    }
    public void SetUIVolume(float value, bool save = true)
    {
        uiVolume = Mathf.Clamp01(value);
        audioCustomizedByUser = true;
        ApplyVolumes();

        if (save)
        {
            SaveAudioSettings();
        }
    }
    public void SetSFXVolume(float value, bool save = true)
    {
        sfxVolume = Mathf.Clamp01(value);
        audioCustomizedByUser = true;
        ApplyVolumes();

        if (save)
        {
            SaveAudioSettings();
        }
    }
    #endregion
}
