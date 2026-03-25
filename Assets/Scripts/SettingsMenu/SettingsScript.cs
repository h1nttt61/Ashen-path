using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class SettingsScript : MonoBehaviour
{
    public Dropdown resolutionDropdown;
    public AudioMixer audioMixer;
    public Slider musicSlider;

    Resolution[] resolutions;

    void Start()
    {
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        resolutions = Screen.resolutions;
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height;
            options.Add(option);
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
                currentResolutionIndex = i;
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.RefreshShownValue();

        LoadSettings(currentResolutionIndex);
    }

    public void OnSliderChanged(float value)
    {
        SetVolume(value);
    }

    public void SetVolume(float volume)
    {
        if (audioMixer == null) return;
        float clampedVolume = Mathf.Clamp(volume, 0.0001f, 1f);
        float dB = Mathf.Log10(clampedVolume) * 20;
        audioMixer.SetFloat("MusicVol", dB);
        PlayerPrefs.SetFloat("MusicVolumePreference", clampedVolume);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("FullscreenPreference", isFullscreen ? 1 : 0);
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt("ResolutionPreference", resolutionIndex);
    }

    public void ExitSettings()
    {
        PlayerPrefs.Save();
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadSettings(int currentResIndex)
    {
        float savedVol = PlayerPrefs.GetFloat("MusicVolumePreference", 0.75f);
        if (musicSlider != null) musicSlider.value = savedVol;
        SetVolume(savedVol);

        resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionPreference", currentResIndex);

        Screen.fullScreen = PlayerPrefs.GetInt("FullscreenPreference", 1) == 1;
    }
}