using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionPopupManager : MonoBehaviour
{
    public GameObject OptionPopup;
    public Slider volumeSlider;
    public Slider mouseSlider;
    public Slider bgmSlider;
    public Slider ambSlider;
    public Slider uiSlider;
    public Slider sfxSlider;
    public Button resetAudioButton;
    public TMP_Text volumeValue;
    public TMP_Text mouseValue;
    public Image masterMuteImg;
    public Image bgmMuteImg;
    public Image ambientMuteImg;
    public Image uiMuteImg;
    public Image sfxMuteImg;
    private const string MouseKey = "Sensitivity";
    private const float DefaultMouseSensitivity = 0.5f;

    private void Start()
    {
        BindResetButton();
        InitializeOptionSettingsIfNeeded();
        RefreshUIFromSavedSettings();
    }

    private void BindResetButton()
    {
        Button discoveredButton = FindResetButton();
        if (discoveredButton != null)
        {
            resetAudioButton = discoveredButton;
        }

        if (resetAudioButton == null)
        {
            return;
        }

        resetAudioButton.transform.SetAsLastSibling();
        EnsureResetButtonVisuals(resetAudioButton);

        if (!HasResetButtonBinding(resetAudioButton))
        {
            resetAudioButton.onClick.AddListener(OnResetAudioButtonClicked);
        }

        Debug.Log($"[OptionPopupManager] Bound reset button: {GetHierarchyPath(resetAudioButton.transform)}");
    }

    private Button FindResetButton()
    {
        if (OptionPopup != null)
        {
            Transform exactMatch = OptionPopup.transform.Find("Reset_Button");
            if (exactMatch != null && exactMatch.TryGetComponent(out Button exactButton))
            {
                return exactButton;
            }

            Button[] popupButtons = OptionPopup.GetComponentsInChildren<Button>(true);
            foreach (Button button in popupButtons)
            {
                if (button.name.Contains("Reset"))
                {
                    return button;
                }
            }
        }

        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            if (button.name.Contains("Reset"))
            {
                return button;
            }
        }

        return null;
    }

    private bool HasResetButtonBinding(Button button)
    {
        int persistentCount = button.onClick.GetPersistentEventCount();

        for (int i = 0; i < persistentCount; i++)
        {
            if (button.onClick.GetPersistentTarget(i) == this &&
                button.onClick.GetPersistentMethodName(i) == nameof(OnResetAudioButtonClicked))
            {
                return true;
            }
        }

        return false;
    }

    private string GetHierarchyPath(Transform target)
    {
        if (target == null)
        {
            return string.Empty;
        }

        string path = target.name;
        Transform current = target.parent;

        while (current != null)
        {
            path = $"{current.name}/{path}";
            current = current.parent;
        }

        return path;
    }

    private void EnsureResetButtonVisuals(Button button)
    {
        Canvas buttonCanvas = button.GetComponent<Canvas>();
        if (buttonCanvas == null)
        {
            buttonCanvas = button.gameObject.AddComponent<Canvas>();
        }

        buttonCanvas.overrideSorting = true;
        buttonCanvas.sortingOrder = 500;

        GraphicRaycaster raycaster = button.GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            raycaster = button.gameObject.AddComponent<GraphicRaycaster>();
        }

        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>(true);
        if (buttonText != null)
        {
            buttonText.text = "Reset";
            buttonText.raycastTarget = false;
        }

        Image[] childImages = button.GetComponentsInChildren<Image>(true);
        foreach (Image image in childImages)
        {
            if (image.gameObject == button.gameObject)
            {
                image.raycastTarget = true;
                continue;
            }

            image.raycastTarget = false;
        }
    }

    private void InitializeOptionSettingsIfNeeded()
    {
        if (!PlayerPrefs.HasKey(MouseKey))
        {
            PlayerPrefs.SetFloat(MouseKey, DefaultMouseSensitivity);
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.InitializeAudioSettingsIfNeeded();
        }
        else
        {
            InitializeAudioFallbackIfNeeded(SoundManager.MasterVolumeKey, 1f);
            InitializeAudioFallbackIfNeeded(SoundManager.BGMVolumeKey, 1f);
            InitializeAudioFallbackIfNeeded(SoundManager.AmbientVolumeKey, 1f);
            InitializeAudioFallbackIfNeeded(SoundManager.UIVolumeKey, 1f);
            InitializeAudioFallbackIfNeeded(SoundManager.SFXVolumeKey, 1f);
        }

        PlayerPrefs.Save();
    }

    private void InitializeAudioFallbackIfNeeded(string key, float defaultValue)
    {
        if (!PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.SetFloat(key, defaultValue);
        }
    }

    private void RefreshUIFromSavedSettings()
    {
        float sensitivity = PlayerPrefs.GetFloat(MouseKey, DefaultMouseSensitivity);
        SetSliderValue(mouseSlider, sensitivity);
        UpdateMouseText(sensitivity);
        RefreshAudioUI();
    }

    private void RefreshAudioUI()
    {
        float master;
        float bgm;
        float ambient;
        float ui;
        float sfx;

        if (SoundManager.Instance != null)
        {
            master = SoundManager.Instance.MasterVolume;
            bgm = SoundManager.Instance.BGMVolume;
            ambient = SoundManager.Instance.AmbientVolume;
            ui = SoundManager.Instance.UIVolume;
            sfx = SoundManager.Instance.SFXVolume;
        }
        else
        {
            master = PlayerPrefs.GetFloat(SoundManager.MasterVolumeKey, 1f);
            bgm = PlayerPrefs.GetFloat(SoundManager.BGMVolumeKey, 1f);
            ambient = PlayerPrefs.GetFloat(SoundManager.AmbientVolumeKey, 1f);
            ui = PlayerPrefs.GetFloat(SoundManager.UIVolumeKey, 1f);
            sfx = PlayerPrefs.GetFloat(SoundManager.SFXVolumeKey, 1f);
            AudioListener.volume = master;
        }

        SetSliderValue(volumeSlider, master);
        SetSliderValue(bgmSlider, bgm);
        SetSliderValue(ambSlider, ambient);
        SetSliderValue(uiSlider, ui);
        SetSliderValue(sfxSlider, sfx);
        UpdateVolText(master);
        masterMuteImg.enabled = SoundManager.Instance.isMasterMute;
        bgmMuteImg.enabled = SoundManager.Instance.isBGMMute;
        ambientMuteImg.enabled = SoundManager.Instance.isAmbientMute;
        uiMuteImg.enabled = SoundManager.Instance.isUIMute;
        sfxMuteImg.enabled = SoundManager.Instance.isSFXMute;
    }

    private void SetSliderValue(Slider slider, float value)
    {
        if (slider == null)
        {
            return;
        }

        slider.SetValueWithoutNotify(value);
    }

    public void OnSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat(MouseKey, value);
        PlayerPrefs.Save();
        UpdateMouseText(value);
    }

    private void UpdateMouseText(float value)
    {
        if (mouseValue != null)
        {
            mouseValue.text = $"{Mathf.RoundToInt(value * 10)}";
        }
    }

    public void OnVolumeChanged(float volume)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMasterVolume(volume);
        }
        else
        {
            PlayerPrefs.SetFloat(SoundManager.MasterVolumeKey, volume);
            PlayerPrefs.Save();
            AudioListener.volume = volume;
        }

        UpdateVolText(volume);
    }

    private void UpdateVolText(float volume)
    {
        if (volumeValue != null)
        {
            int txt = Mathf.RoundToInt(volume * 100f);
            volumeValue.text = $"{txt}%";
        }
    }

    public void OnBGMSliderChanged(float volume)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetBGMVolume(volume);
        }
        else
        {
            PlayerPrefs.SetFloat(SoundManager.BGMVolumeKey, volume);
            PlayerPrefs.Save();
        }
    }

    public void OnAmbientSliderChanged(float volume)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetAmbientVolume(volume);
        }
        else
        {
            PlayerPrefs.SetFloat(SoundManager.AmbientVolumeKey, volume);
            PlayerPrefs.Save();
        }
    }

    public void OnUISliderChanged(float volume)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetUIVolume(volume);
        }
        else
        {
            PlayerPrefs.SetFloat(SoundManager.UIVolumeKey, volume);
            PlayerPrefs.Save();
        }
    }

    public void OnSFXliderChanged(float volume)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetSFXVolume(volume);
        }
        else
        {
            PlayerPrefs.SetFloat(SoundManager.SFXVolumeKey, volume);
            PlayerPrefs.Save();
        }
    }

    public void OnResetAudioButtonClicked()
    {
        Debug.Log("[OptionPopupManager] Reset button clicked");

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.ResetAudioSettingsToDefaults();
            RefreshAudioUI();
        }
        else
        {
            ApplyAudioValuesToSliders(1f, 1f, 1f, 1f, 1f);
        }
    }

    public void OnMasterMuteButtonClicked()
    {
        if (SoundManager.Instance == null) return;
        SoundManager.Instance.isMasterMute = !SoundManager.Instance.isMasterMute;
        PlayerPrefs.SetInt(SoundManager.MasterMuteKey, SoundManager.Instance.isMasterMute ? 1 : 0);
        PlayerPrefs.Save();
        SoundManager.Instance.ApplyVolumes();
        masterMuteImg.enabled = SoundManager.Instance.isMasterMute;
    }
    public void OnBGMMuteButtonClicked()
    {
        if (SoundManager.Instance == null) return;
        SoundManager.Instance.isBGMMute = !SoundManager.Instance.isBGMMute;
        PlayerPrefs.SetInt(SoundManager.BGMMuteKey, SoundManager.Instance.isBGMMute ? 1 : 0);
        PlayerPrefs.Save();
        SoundManager.Instance.ApplyVolumes();
        bgmMuteImg.enabled = SoundManager.Instance.isBGMMute;
    }
    public void OnAmbientMuteButtonClicked()
    {
        if (SoundManager.Instance == null) return;
        SoundManager.Instance.isAmbientMute = !SoundManager.Instance.isAmbientMute;
        PlayerPrefs.SetInt(SoundManager.AmbientMuteKey, SoundManager.Instance.isAmbientMute ? 1 : 0);
        PlayerPrefs.Save();
        SoundManager.Instance.ApplyVolumes();
        ambientMuteImg.enabled = SoundManager.Instance.isAmbientMute;
    }
    public void OnUIMuteButtonClicked()
    {
        if (SoundManager.Instance == null) return;
        SoundManager.Instance.isUIMute = !SoundManager.Instance.isUIMute;
        PlayerPrefs.SetInt(SoundManager.UIMuteKey, SoundManager.Instance.isUIMute ? 1 : 0);
        PlayerPrefs.Save();
        SoundManager.Instance.ApplyVolumes();
        uiMuteImg.enabled = SoundManager.Instance.isUIMute;
    }
    public void OnSFXMuteButtonClicked()
    {
        if (SoundManager.Instance == null) return;
        SoundManager.Instance.isSFXMute = !SoundManager.Instance.isSFXMute;
        PlayerPrefs.SetInt(SoundManager.SFXMuteKey, SoundManager.Instance.isSFXMute ? 1 : 0);
        PlayerPrefs.Save();
        SoundManager.Instance.ApplyVolumes();
        sfxMuteImg.enabled = SoundManager.Instance.isSFXMute;
    }

    private void ApplyAudioValuesToSliders(float master, float bgm, float ambient, float ui, float sfx)
    {
        ApplySliderOrFallback(volumeSlider, master, OnVolumeChanged, SoundManager.MasterVolumeKey);
        ApplySliderOrFallback(bgmSlider, bgm, OnBGMSliderChanged, SoundManager.BGMVolumeKey);
        ApplySliderOrFallback(ambSlider, ambient, OnAmbientSliderChanged, SoundManager.AmbientVolumeKey);
        ApplySliderOrFallback(uiSlider, ui, OnUISliderChanged, SoundManager.UIVolumeKey);
        ApplySliderOrFallback(sfxSlider, sfx, OnSFXliderChanged, SoundManager.SFXVolumeKey);
    }

    private void ApplySliderOrFallback(Slider slider, float value, UnityEngine.Events.UnityAction<float> onChanged, string prefsKey)
    {
        if (slider != null)
        {
            slider.value = value;
            return;
        }

        PlayerPrefs.SetFloat(prefsKey, value);
        PlayerPrefs.Save();
        onChanged?.Invoke(value);
    }
}
