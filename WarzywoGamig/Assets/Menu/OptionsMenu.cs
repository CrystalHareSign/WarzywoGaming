using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class OptionsMenu : MonoBehaviour
{
    public GameObject optionsMenuUI;
    public GameObject pauseMenuUI;
    public GameObject generalOptionsPanel;
    public GameObject soundOptionsPanel;
    public GameObject visualOptionsPanel;
    public GameObject startMenuUI;

    [Header("Teksty przycisk�w")]
    public TMP_Text generalSettingsText;
    public TMP_Text soundSettingsText;
    public TMP_Text visualSettingsText;
    public TMP_Text backButtonText;

    // Lista wszystkich obiekt�w, kt�re posiadaj� PlaySoundOnObject
    private List<PlaySoundOnObject> playSoundObjects = new List<PlaySoundOnObject>();

    void Start()
    {
        optionsMenuUI.SetActive(false); // Upewnij si�, �e menu opcji jest niewidoczne na starcie

        UpdateButtonTexts();

        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged += UpdateButtonTexts;
        }

        // Znajd� wszystkie obiekty posiadaj�ce PlaySoundOnObject i dodaj do listy
        playSoundObjects.AddRange(Object.FindObjectsByType<PlaySoundOnObject>(FindObjectsSortMode.None));
    }

    private void OnEnable()
    {
        UpdateButtonTexts();
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

        string currentScene = SceneManager.GetActiveScene().name;
        
        if (currentScene == "StartMenu")
        {
            if (startMenuUI != null)
            {
                startMenuUI.SetActive(true); // Poka� menu pauzy
            }
        }
        else
        {
            if (pauseMenuUI != null)
            {
                pauseMenuUI.SetActive(true); // Poka� menu pauzy
            }
        }
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
        foreach (var playSoundOnObject in playSoundObjects)
        {
            if (playSoundOnObject == null) continue;

            playSoundOnObject.PlaySound("MenuMouseOn", 0.8f, false);
        }
    }
}
