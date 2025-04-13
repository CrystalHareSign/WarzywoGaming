using TMPro;
using UnityEngine;

public class OptionsMenu : MonoBehaviour
{
    public GameObject optionsMenuUI;
    public GameObject pauseMenuUI;
    public GameObject generalOptionsPanel;
    public GameObject soundOptionsPanel;
    public GameObject visualOptionsPanel;

    [Header("Teksty przycisków")]
    public TMP_Text generalSettingsText;
    public TMP_Text soundSettingsText;
    public TMP_Text visualSettingsText;
    public TMP_Text backButtonText;

    public static OptionsMenu instance;

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
    void Start()
    {
        optionsMenuUI.SetActive(false); // Upewnij siê, ¿e menu opcji jest niewidoczne na starcie

        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged += UpdateButtonTexts;
        }
    }

    public void ShowGeneralSettings()
    {
        generalOptionsPanel.SetActive(true);
        soundOptionsPanel.SetActive(false);
        //visualOptionsPanel.SetActive(false);
        optionsMenuUI.SetActive(false); // Ukryj menu opcji
    }

    public void ShowSoundSettings()
    {
        generalOptionsPanel.SetActive(false);
        soundOptionsPanel.SetActive(true);
        //visualSettingsPanel.SetActive(false);
        optionsMenuUI.SetActive(false); // Ukryj menu opcji
    }

    public void ShowVisualSettings()
    {
        generalOptionsPanel.SetActive(false);
        soundOptionsPanel.SetActive(false);
        visualOptionsPanel.SetActive(true);
        optionsMenuUI.SetActive(false); // Ukryj menu opcji
    }
    public void BackToPauseMenu()
    {
        optionsMenuUI.SetActive(false); // Ukryj menu opcji
        pauseMenuUI.SetActive(true); // Poka¿ menu pauzy
    }

    public void UpdateButtonTexts()
    {
        if (LanguageManager.Instance == null) return;

        var texts = LanguageManager.Instance.CurrentUITexts;

        if (generalSettingsText != null) generalSettingsText.text = texts.generalSettings;
        if (soundSettingsText != null) soundSettingsText.text = texts.soundSettings;
        if (visualSettingsText != null) visualSettingsText.text = texts.visualSettings;
        if (backButtonText != null) backButtonText.text = texts.back;
    }
}
