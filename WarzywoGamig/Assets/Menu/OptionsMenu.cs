using UnityEngine;
using UnityEngine.UI;
using TMPro; // Import TextMeshPro namespace

public class OptionsMenu : MonoBehaviour
{
    [Header("Sliders")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider ambientVolumeSlider;

    [Header("Percentage Texts (TextMeshPro)")]
    public TMP_Text musicVolumeText;
    public TMP_Text sfxVolumeText;
    public TMP_Text ambientVolumeText;

    private void Start()
    {
        // Ustaw zakres warto�ci slider�w na 0-100
        musicVolumeSlider.minValue = 0;
        musicVolumeSlider.maxValue = 100;
        sfxVolumeSlider.minValue = 0;
        sfxVolumeSlider.maxValue = 100;
        ambientVolumeSlider.minValue = 0;
        ambientVolumeSlider.maxValue = 100;

        LoadCurrentSettings();

        // Dodaj s�uchaczy do suwak�w
        musicVolumeSlider.onValueChanged.AddListener(delegate { UpdateSliderValue(musicVolumeSlider, musicVolumeText, SetMusicVolume); });
        sfxVolumeSlider.onValueChanged.AddListener(delegate { UpdateSliderValue(sfxVolumeSlider, sfxVolumeText, SetSFXVolume); });
        ambientVolumeSlider.onValueChanged.AddListener(delegate { UpdateSliderValue(ambientVolumeSlider, ambientVolumeText, SetAmbientVolume); });

        // Obs�uga braku AudioManager.Instance
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("AudioManager.Instance is null. Wy��czanie suwak�w.");
            musicVolumeSlider.interactable = false;
            sfxVolumeSlider.interactable = false;
            ambientVolumeSlider.interactable = false;
        }
    }

    private void UpdateSliderValue(Slider slider, TMP_Text text, System.Action<float> updateAction)
    {
        // Zaokr�glenie warto�ci suwaka do liczby ca�kowitej
        float roundedValue = Mathf.Round(slider.value);
        slider.value = roundedValue;

        // Aktualizacja tekstu procentowego
        text.text = $"{(int)roundedValue}%";

        // Wywo�anie odpowiedniej akcji (np. ustawienie g�o�no�ci)
        updateAction?.Invoke(roundedValue / 100f); // Przekazujemy warto�� w zakresie 0-1 do AudioManager
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
            PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value / 100f);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value / 100f);
            PlayerPrefs.SetFloat("AmbientVolume", ambientVolumeSlider.value / 100f);
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

            // Przekszta�cenie warto�ci 0-1 na 0-100
            musicVolumeSlider.value = savedMusicVolume * 100f;
            sfxVolumeSlider.value = savedSFXVolume * 100f;
            ambientVolumeSlider.value = savedAmbientVolume * 100f;

            UpdateSliderValue(musicVolumeSlider, musicVolumeText, SetMusicVolume);
            UpdateSliderValue(sfxVolumeSlider, sfxVolumeText, SetSFXVolume);
            UpdateSliderValue(ambientVolumeSlider, ambientVolumeText, SetAmbientVolume);
        }
        else
        {
            Debug.LogWarning("AudioManager.Instance is null");
        }
    }

    public void BackToPauseMenu()
    {
        LoadCurrentSettings();
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