using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [Header("Sliders")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider ambientVolumeSlider;

    private void Start()
    {
        LoadCurrentSettings();

        // Dodaj s�uchaczy do suwak�w
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        ambientVolumeSlider.onValueChanged.AddListener(SetAmbientVolume);

        // Obs�uga braku AudioManager.Instance
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("AudioManager.Instance is null. Wy��czanie suwak�w.");
            musicVolumeSlider.interactable = false;
            sfxVolumeSlider.interactable = false;
            ambientVolumeSlider.interactable = false;
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(volume); // Dynamiczna aktualizacja
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(volume); // Dynamiczna aktualizacja
        }
    }

    public void SetAmbientVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetAmbientVolume(volume); // Dynamiczna aktualizacja
        }
    }

    public void ApplyChanges()
    {
        // Zapisanie zmian w AudioManagerze oraz PlayerPrefs
        if (AudioManager.Instance != null)
        {
            PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
            PlayerPrefs.SetFloat("AmbientVolume", ambientVolumeSlider.value);
            PlayerPrefs.Save();

            Debug.Log("Zmiany g�o�no�ci zosta�y zastosowane i zapisane!");
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
            // Pobranie warto�ci z PlayerPrefs lub ustawienie domy�lnych z AudioManager
            float savedMusicVolume = PlayerPrefs.HasKey("MusicVolume") ? PlayerPrefs.GetFloat("MusicVolume") : AudioManager.Instance.masterMusicVolume;
            float savedSFXVolume = PlayerPrefs.HasKey("SFXVolume") ? PlayerPrefs.GetFloat("SFXVolume") : AudioManager.Instance.masterSFXVolume;
            float savedAmbientVolume = PlayerPrefs.HasKey("AmbientVolume") ? PlayerPrefs.GetFloat("AmbientVolume") : AudioManager.Instance.masterAmbientVolume;

            musicVolumeSlider.value = savedMusicVolume;
            sfxVolumeSlider.value = savedSFXVolume;
            ambientVolumeSlider.value = savedAmbientVolume;

            // Aktualizacja AudioManagera
            AudioManager.Instance.SetMusicVolume(savedMusicVolume);
            AudioManager.Instance.SetSFXVolume(savedSFXVolume);
            AudioManager.Instance.SetAmbientVolume(savedAmbientVolume);
        }
        else
        {
            Debug.LogWarning("AudioManager.Instance is null");
        }
    }

    public void BackToPauseMenu()
    {
        PauseMenu pauseMenu = Object.FindFirstObjectByType<PauseMenu>();
        if (pauseMenu != null)
        {
            pauseMenu.BackToPauseMenu();
        }
        else
        {
            Debug.LogWarning("Nie znaleziono PauseMenu");
        }
    }
}