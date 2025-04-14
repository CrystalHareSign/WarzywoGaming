using UnityEngine;
using UnityEngine.UI;
using TMPro; // Import TextMeshPro namespace
using System.Collections.Generic;

public class SoundOptionsMenu : MonoBehaviour
{

    public GameObject soundOptionsMenuUI;
    public GameObject optionsMenuUI;

    [Header("Domy�lne warto�ci (0-100)")]
    public float defaultMusicVolume = 70f;
    public float defaultSFXVolume = 70f;
    public float defaultAmbientVolume = 70f;

    [Header("Sliders")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider ambientVolumeSlider;

    [Header("Percentage Texts (TextMeshPro)")]
    public TMP_Text musicVolumeText;
    public TMP_Text sfxVolumeText;
    public TMP_Text ambientVolumeText;

    [Header("Teksty przycisk�w")]
    public TMP_Text apply2ButtonText;
    public TMP_Text cancel2ButtonText;
    public TMP_Text back2ButtonText;
    public TMP_Text reset2ButtonText;
    public TMP_Text musicText;
    public TMP_Text sfxText;
    public TMP_Text ambientText;

    public static SoundOptionsMenu instance;

    // Lista wszystkich obiekt�w, kt�re posiadaj� PlaySoundOnObject
    private List<PlaySoundOnObject> playSoundObjects = new List<PlaySoundOnObject>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

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

        // Subskrybuj zmiany j�zyka
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged += UpdateButtonTexts;
        }

        // Zaktualizuj teksty przycisk�w
        UpdateButtonTexts();

        // Znajd� wszystkie obiekty posiadaj�ce PlaySoundOnObject i dodaj do listy
        playSoundObjects.AddRange(Object.FindObjectsOfType<PlaySoundOnObject>());
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

    public void BackToOptionsMenu()
    {
        LoadCurrentSettings();
        soundOptionsMenuUI.SetActive(false); // Ukryj menu opcji
        optionsMenuUI.SetActive(true); // Poka� menu pauzy

        //OptionsMenu optionsMenu = Object.FindFirstObjectByType<OptionsMenu>();
        //if (optionsMenu != null)
        //{
        //    optionsMenu.BackToOptionsMenu();
        //}
        //else
        //{
        //    Debug.LogWarning("Nie znaleziono PauseMenu");
        //}
    }

    public void ResetToDefaults()
    {
        musicVolumeSlider.value = defaultMusicVolume;
        sfxVolumeSlider.value = defaultSFXVolume;
        ambientVolumeSlider.value = defaultAmbientVolume;

        UpdateSliderValue(musicVolumeSlider, musicVolumeText, SetMusicVolume);
        UpdateSliderValue(sfxVolumeSlider, sfxVolumeText, SetSFXVolume);
        UpdateSliderValue(ambientVolumeSlider, ambientVolumeText, SetAmbientVolume);

        ApplyChanges();
        Debug.Log("Ustawienia zresetowane do warto�ci domy�lnych.");
    }

    public void UpdateButtonTexts()
    {
        Debug.Log("UpdateButtonTexts called");

        if (LanguageManager.Instance == null) return;
        var uiTexts = LanguageManager.Instance.CurrentUITexts;

        if (apply2ButtonText != null) apply2ButtonText.text = uiTexts.apply1;
        if (cancel2ButtonText != null) cancel2ButtonText.text = uiTexts.cancel1;
        if (back2ButtonText != null) back2ButtonText.text = uiTexts.back1;
        if (reset2ButtonText != null) reset2ButtonText.text = uiTexts.reset1;
        if (musicText != null) musicText.text = uiTexts.music;
        if (sfxText != null) sfxText.text = uiTexts.sfx;
        if (ambientText != null) ambientText.text = uiTexts.ambient;
    }

    public void EnterButtonSound()
    {
        foreach (var playSoundOnObject in playSoundObjects)
        {
            if (playSoundOnObject == null) continue;

            playSoundOnObject.PlaySound("MenuEnter", 0.4f, false);
        }
    }

    public void ExitButtonSound()
    {
        foreach (var playSoundOnObject in playSoundObjects)
        {
            if (playSoundOnObject == null) continue;

            playSoundOnObject.PlaySound("MenuExit", 0.4f, false);
        }
    }

    public void HoverButtonSound()
    {
        Debug.Log("dzia�a przucisk");
        foreach (var playSoundOnObject in playSoundObjects)
        {
            if (playSoundOnObject == null) continue;

            playSoundOnObject.PlaySound("MenuMouseOn", 0.8f, false);
        }
    }
}