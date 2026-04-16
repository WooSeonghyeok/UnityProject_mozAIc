using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;
public class OptionPopupManager : MonoBehaviour
{
    public GameObject OptionPopup;
    public Slider volumeSlider;
    public Slider mouseSlider;
    public Slider bgmSlider;
    public Slider ambSlider;
    public Slider uiSlider;
    public Slider sfxSlider;
    public TMP_Text volumeValue;
    public TMP_Text mouseValue;
    const string mouseKey = "Sensitivity";
    const string volumeKey = "Volume";
    const string bgmVolKey = "BGM_Volume";
    const string ambVolKey = "Ambient_Volume";
    const string uiVolKey = "UI_Volume";
    const string sfxVolKey = "SFX_Volume";
    void Start()
    {
        if (!PlayerPrefs.HasKey(volumeKey)) PlayerPrefs.SetFloat(volumeKey, 1f);
        if (!PlayerPrefs.HasKey(mouseKey)) PlayerPrefs.SetFloat(mouseKey, 0.5f);
        if (!PlayerPrefs.HasKey(bgmVolKey)) PlayerPrefs.SetFloat(bgmVolKey, 1f);
        if (!PlayerPrefs.HasKey(ambVolKey)) PlayerPrefs.SetFloat(ambVolKey, 1f);
        if (!PlayerPrefs.HasKey(uiVolKey)) PlayerPrefs.SetFloat(uiVolKey, 1f);
        if (!PlayerPrefs.HasKey(sfxVolKey)) PlayerPrefs.SetFloat(sfxVolKey, 1f);
        float vol = PlayerPrefs.GetFloat(volumeKey);
        float sens = PlayerPrefs.GetFloat(mouseKey);
        float bgm = PlayerPrefs.GetFloat(bgmVolKey);
        float amb = PlayerPrefs.GetFloat(ambVolKey);
        float ui = PlayerPrefs.GetFloat(uiVolKey);
        float sfx = PlayerPrefs.GetFloat(sfxVolKey);
        mouseSlider.value = sens;
        volumeSlider.value = vol;
        bgmSlider.value = bgm;
        ambSlider.value = amb;
        uiSlider.value = ui;
        sfxSlider.value = sfx;
        AudioListener.volume = vol;
        UpdateMouseText(sens);
        UpdateVolText(vol);
    }
    public void OnSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat(mouseKey, value);
        UpdateMouseText(value);
    }
    void UpdateMouseText(float value)
    {
        mouseValue.text = $"{Mathf.RoundToInt(value * 10)}";
    }
    public void OnVolumeChanged(float volume)
    {
        PlayerPrefs.SetFloat(volumeKey, volume);
        AudioListener.volume = volume;
        if (SoundManager.Instance != null) SoundManager.Instance.SetMasterVolume(volume);
        UpdateVolText(volume);
    }
    void UpdateVolText(float volume)
    {
        int txt = Mathf.RoundToInt(volume*100f);
        volumeValue.text = $"{txt}%";
    }
    public void OnBGMSliderChanged(float volume)
    {
        PlayerPrefs.SetFloat(bgmVolKey, volume);
        if(SoundManager.Instance != null) SoundManager.Instance.SetBGMVolume(volume);
    }
    public void OnAmbientSliderChanged(float volume)
    {
        PlayerPrefs.SetFloat(ambVolKey, volume);
        if (SoundManager.Instance != null) SoundManager.Instance.SetAmbientVolume(volume);
    }
    public void OnUISliderChanged(float volume)
    {
        PlayerPrefs.SetFloat(uiVolKey, volume);
        if (SoundManager.Instance != null) SoundManager.Instance.SetUIVolume(volume);
    }
    public void OnSFXliderChanged(float volume)
    {
        PlayerPrefs.SetFloat(sfxVolKey, volume);
        if (SoundManager.Instance != null) SoundManager.Instance.SetSFXVolume(volume);
    }
}
