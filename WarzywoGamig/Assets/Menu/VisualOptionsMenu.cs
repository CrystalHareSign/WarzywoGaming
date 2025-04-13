using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class VisualOptionsMenu : MonoBehaviour
{
    public GameObject visualOptionsMenuUI;
    public GameObject optionsMenuUI;

    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    private Resolution[] availableResolutions;
    private int currentResolutionIndex;

    private int tempResolutionIndex;
    private bool tempFullscreen;

    [Header("Teksty przycisków")]
    public TMP_Text apply3ButtonText;
    public TMP_Text cancel3ButtonText;
    public TMP_Text back3ButtonText;
    public TMP_Text reset3ButtonText;
    public TMP_Text resolutionText;
    public TMP_Text fullscreenText;

    void Start()
    {
        InitializeResolutions();

        // Wczytaj ustawienia z PlayerPrefs
        LoadSettings();

        // Update tekstów UI
        UpdateButtonTexts();

        // Subskrybuj zmiany jêzyka
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged += UpdateButtonTexts;
        }
    }

    void InitializeResolutions()
    {
        availableResolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        currentResolutionIndex = 0;

        for (int i = 0; i < availableResolutions.Length; i++)
        {
            string option = availableResolutions[i].width + " x " + availableResolutions[i].height;
            if (!options.Contains(option))
            {
                options.Add(option);
            }

            if (availableResolutions[i].width == Screen.currentResolution.width &&
                availableResolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.onValueChanged.AddListener(delegate { OnResolutionChanged(resolutionDropdown.value); });
    }

    void OnResolutionChanged(int index)
    {
        tempResolutionIndex = index;
        Debug.Log("Tymczasowo wybrano rozdzielczoœæ: " + availableResolutions[index].ToString());
    }

    public void OnFullscreenToggle(bool isFullscreen)
    {
        tempFullscreen = isFullscreen;
        Debug.Log("Tymczasowo ustawiono fullscreen: " + tempFullscreen);
    }

    public void ApplyChanges()
    {
        Resolution res = availableResolutions[tempResolutionIndex];
        Screen.SetResolution(res.width, res.height, tempFullscreen);

        PlayerPrefs.SetInt("ResolutionIndex", tempResolutionIndex);
        PlayerPrefs.SetInt("Fullscreen", tempFullscreen ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log("Zastosowano ustawienia graficzne.");
    }

    public void CancelChanges()
    {
        resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex", currentResolutionIndex);
        tempResolutionIndex = resolutionDropdown.value;

        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        tempFullscreen = fullscreenToggle.isOn;

        resolutionDropdown.RefreshShownValue();
        Debug.Log("Anulowano zmiany graficzne.");
    }

    public void ResetToDefaults()
    {
        resolutionDropdown.value = currentResolutionIndex;
        tempResolutionIndex = currentResolutionIndex;

        fullscreenToggle.isOn = true;
        tempFullscreen = true;

        resolutionDropdown.RefreshShownValue();
        Debug.Log("Zresetowano ustawienia graficzne do domyœlnych.");
    }

    public void BackToOptionsMenu()
    {
        CancelChanges();
        visualOptionsMenuUI.SetActive(false);
        optionsMenuUI.SetActive(true);
    }

    public void UpdateButtonTexts()
    {
        if (LanguageManager.Instance == null) return;
        var uiTexts = LanguageManager.Instance.CurrentUITexts;

        if (apply3ButtonText != null) apply3ButtonText.text = uiTexts.apply1;
        if (cancel3ButtonText != null) cancel3ButtonText.text = uiTexts.cancel1;
        if (back3ButtonText != null) back3ButtonText.text = uiTexts.back1;
        if (reset3ButtonText != null) reset3ButtonText.text = uiTexts.reset1;
        if (resolutionText != null) resolutionText.text = uiTexts.resolution;
        if (fullscreenText != null) fullscreenText.text = uiTexts.fullscreen;
    }

    void LoadSettings()
    {
        tempResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", currentResolutionIndex);
        tempFullscreen = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;

        resolutionDropdown.value = tempResolutionIndex;
        fullscreenToggle.isOn = tempFullscreen;

        resolutionDropdown.RefreshShownValue();
    }
}
