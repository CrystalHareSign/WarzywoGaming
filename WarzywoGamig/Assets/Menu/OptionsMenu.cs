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

        // Dodaj s�uchaczy do suwak�w
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
        // Ustawienie warto�ci w AudioManagerze i zapisanie ich
        if (AudioManager.Instance != null)
        {
            // Zak�adaj�c, �e masz odpowiednie suwaki na g�o�no��:
            AudioManager.Instance.SetMusicVolume(musicVolumeSlider.value);
            AudioManager.Instance.SetSFXVolume(sfxVolumeSlider.value);
            AudioManager.Instance.SetAmbientVolume(ambientVolumeSlider.value);

            Debug.Log("Zmiany g�o�no�ci zosta�y zastosowane!");
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
        FindFirstObjectByType<PauseMenu>().BackToPauseMenu(); // Wywo�aj metod� w PauseMenu
    }
}