using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [Header("Sliders")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider ambientVolumeSlider;

    private float tempMusicVolume;
    private float tempSFXVolume;
    private float tempAmbientVolume;

    private void Start()
    {
        LoadCurrentSettings();

        // Dodaj s³uchaczy do suwaków
        musicVolumeSlider.onValueChanged.AddListener(SetTempMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetTempSFXVolume);
        ambientVolumeSlider.onValueChanged.AddListener(SetTempAmbientVolume);
    }

    public void SetTempMusicVolume(float volume)
    {
        tempMusicVolume = volume;
    }

    public void SetTempSFXVolume(float volume)
    {
        tempSFXVolume = volume;
    }

    public void SetTempAmbientVolume(float volume)
    {
        tempAmbientVolume = volume;
    }

    public void ApplyChanges()
    {
        // Ustawienie wartoœci w AudioManagerze i zapisanie ich
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(tempMusicVolume);
            AudioManager.Instance.SetSFXVolume(tempSFXVolume);
            AudioManager.Instance.SetAmbientVolume(tempAmbientVolume);
            Debug.LogWarning("Zapisano zmiany");
        }
        else
        {
            Debug.LogWarning("AudioManager.Instance is null");
        }
    }

    public void CancelChanges()
    {
        LoadCurrentSettings();
        Debug.LogWarning("Anulowano zmiany");
    }

    private void LoadCurrentSettings()
    {
        if (AudioManager.Instance != null)
        {
            tempMusicVolume = AudioManager.Instance.masterMusicVolume;
            tempSFXVolume = AudioManager.Instance.masterSFXVolume;
            tempAmbientVolume = AudioManager.Instance.masterAmbientVolume;

            musicVolumeSlider.value = tempMusicVolume;
            sfxVolumeSlider.value = tempSFXVolume;
            ambientVolumeSlider.value = tempAmbientVolume;
        }
        else
        {
            Debug.LogWarning("AudioManager.Instance is null");
        }
    }

    public void BackToPauseMenu()
    {
        FindFirstObjectByType<PauseMenu>().BackToPauseMenu(); // Wywo³aj metodê w PauseMenu
    }
}