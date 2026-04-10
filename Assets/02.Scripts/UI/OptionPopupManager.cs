using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class OptionPopupManager : MonoBehaviour
{
    public GameObject OptionPopup;
    public Slider volumeSlider;
    public Slider mouseSlider;
    public TMP_Text volumeValue;
    public TMP_Text mouseValue;
    const string mouseKey = "Sensitivity";
    const string volumeKey = "Volume";
    void Start()
    {
        if (!PlayerPrefs.HasKey(volumeKey)) PlayerPrefs.SetFloat(volumeKey, 1f);
        if (!PlayerPrefs.HasKey(mouseKey)) PlayerPrefs.SetFloat(mouseKey, 0.05f);
        float vol = PlayerPrefs.GetFloat(volumeKey);
        float sens = PlayerPrefs.GetFloat(mouseKey);
        mouseSlider.value = sens;
        volumeSlider.value = vol;
        AudioListener.volume = vol;
        UpdateMouseText(sens);
        UpdateVolText(vol);
    }
    public void OpenOptionPopup() => OptionPopup.SetActive(true);
    public void CloseOptionPopup() => OptionPopup.SetActive(false);
    public void OnSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat(mouseKey, value);
        UpdateMouseText(value);
    }
    void UpdateMouseText(float value)
    {
        mouseValue.text = $"{Mathf.RoundToInt(value * 100)}";
    }
    public void OnVolumeChanged(float volume)
    {
        PlayerPrefs.SetFloat(volumeKey, volume);
        AudioListener.volume = volume;
        UpdateVolText(volume);
    }
    void UpdateVolText(float volume)
    {
        int txt = Mathf.RoundToInt(volume*100f);
        volumeValue.text = $"{txt}%";
    }
}
