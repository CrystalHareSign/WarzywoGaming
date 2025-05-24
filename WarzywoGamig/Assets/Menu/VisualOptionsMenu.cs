using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class VisualOptionsMenu : MonoBehaviour
{
    public GameObject visualOptionsMenuUI;
    public GameObject optionsMenuUI;

    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown screenModeDropdown; // NOWY dropdown dla trybu ekranu

    private Resolution[] availableResolutions;
    private List<Resolution> filteredResolutions = new List<Resolution>();
    private int currentResolutionIndex;

    private int tempResolutionIndex;
    private int tempScreenModeIndex; // 0 = Windowed, 1 = Fullscreen, 2 = Borderless

    [Header("Teksty przycisków")]
    public TMP_Text apply3ButtonText;
    public TMP_Text cancel3ButtonText;
    public TMP_Text back3ButtonText;
    public TMP_Text reset3ButtonText;
    public TMP_Text resolutionText;
    public TMP_Text screenModeText; // Zamiast fullscreenText

    // Lista wszystkich obiektów, które posiadaj¹ PlaySoundOnObject
    private List<PlaySoundOnObject> playSoundObjects = new List<PlaySoundOnObject>();

    void Start()
    {
        InitializeResolutions();

        InitializeScreenModes();

        UpdateButtonTexts();

        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged += UpdateButtonTexts;
        }

        playSoundObjects.AddRange(Object.FindObjectsByType<PlaySoundOnObject>(FindObjectsSortMode.None));

        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        screenModeDropdown.onValueChanged.AddListener(OnScreenModeChanged);
    }

    void InitializeResolutions()
    {
        availableResolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        filteredResolutions.Clear();

        List<string> options = new List<string>();

        // Zbierz unikalne rozdzielczoœci
        List<Resolution> uniqueRes = new List<Resolution>();
        HashSet<string> seen = new HashSet<string>();

        for (int i = 0; i < availableResolutions.Length; i++)
        {
            string option = availableResolutions[i].width + " x " + availableResolutions[i].height;
            if (!seen.Contains(option))
            {
                uniqueRes.Add(availableResolutions[i]);
                seen.Add(option);
            }
        }

        // Sortuj malej¹co (najwiêksza na górze)
        uniqueRes.Sort((a, b) =>
        {
            int cmp = b.width.CompareTo(a.width);
            if (cmp == 0) cmp = b.height.CompareTo(a.height);
            return cmp;
        });

        // Dodaj do dropdowna
        foreach (var res in uniqueRes)
        {
            string option = res.width + " x " + res.height;
            filteredResolutions.Add(res);
            options.Add(option);
        }

        // Ustaw domyœln¹ (najwiêksza, czyli pierwsza)
        currentResolutionIndex = 0;

        resolutionDropdown.AddOptions(options);
    }

    void InitializeScreenModes()
    {
        screenModeDropdown.ClearOptions();
        List<string> modeOptions = new List<string> { "Windowed", "Fullscreen", "Borderless" };
        screenModeDropdown.AddOptions(modeOptions);
    }

    void OnResolutionChanged(int index)
    {
        tempResolutionIndex = index;
        Debug.Log("Tymczasowo wybrano rozdzielczoœæ: " + filteredResolutions[index].ToString());
    }

    void OnScreenModeChanged(int modeIndex)
    {
        tempScreenModeIndex = modeIndex;
        Debug.Log("Tymczasowo wybrano tryb ekranu: " + screenModeDropdown.options[modeIndex].text);
    }

    public void ApplyChanges()
    {
        Resolution res = filteredResolutions[tempResolutionIndex];
        FullScreenMode mode = GetFullScreenModeFromIndex(tempScreenModeIndex);

        Screen.SetResolution(res.width, res.height, mode);

        PlayerPrefs.SetInt("ResolutionIndex", tempResolutionIndex);
        PlayerPrefs.SetInt("ScreenMode", tempScreenModeIndex);
        PlayerPrefs.Save();

        Debug.Log("Zastosowano ustawienia graficzne: " + res.width + "x" + res.height + " " + mode.ToString());
    }

    public void CancelChanges()
    {
        int savedResIndex = PlayerPrefs.GetInt("ResolutionIndex", currentResolutionIndex);
        int savedModeIndex = PlayerPrefs.GetInt("ScreenMode", 0);

        resolutionDropdown.value = savedResIndex;
        tempResolutionIndex = savedResIndex;

        screenModeDropdown.value = savedModeIndex;
        tempScreenModeIndex = savedModeIndex;

        ApplyScreenSettings(tempResolutionIndex, tempScreenModeIndex);

        resolutionDropdown.RefreshShownValue();
        screenModeDropdown.RefreshShownValue();
        Debug.Log("Anulowano zmiany graficzne.");
    }

    public void ResetToDefaults()
    {
        resolutionDropdown.value = currentResolutionIndex;
        tempResolutionIndex = currentResolutionIndex;

        screenModeDropdown.value = 1; // Fullscreen jako domyœlny
        tempScreenModeIndex = 1;

        ApplyScreenSettings(tempResolutionIndex, tempScreenModeIndex);

        resolutionDropdown.RefreshShownValue();
        screenModeDropdown.RefreshShownValue();
        ApplyChanges();
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
        if (screenModeText != null) screenModeText.text = "Screen mode"; // Lub z uiTexts, jeœli jest
    }

    void LoadSettings()
    {
        tempResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", currentResolutionIndex);
        tempScreenModeIndex = PlayerPrefs.GetInt("ScreenMode", 1); // Fullscreen jako domyœlny

        resolutionDropdown.value = tempResolutionIndex;
        screenModeDropdown.value = tempScreenModeIndex;

        ApplyScreenSettings(tempResolutionIndex, tempScreenModeIndex);

        resolutionDropdown.RefreshShownValue();
        screenModeDropdown.RefreshShownValue();
    }

    void ApplyScreenSettings(int resIndex, int modeIndex)
    {
        Resolution res = filteredResolutions[resIndex];
        FullScreenMode mode = GetFullScreenModeFromIndex(modeIndex);

        Screen.SetResolution(res.width, res.height, mode);
    }

    FullScreenMode GetFullScreenModeFromIndex(int idx)
    {
        switch (idx)
        {
            case 0: return FullScreenMode.FullScreenWindow;
            case 1: return FullScreenMode.Windowed;
            case 2: return FullScreenMode.ExclusiveFullScreen;
            default: return FullScreenMode.FullScreenWindow;
        }
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
        Debug.Log("dzia³a przucisk");
        foreach (var playSoundOnObject in playSoundObjects)
        {
            if (playSoundOnObject == null) continue;

            playSoundOnObject.PlaySound("MenuMouseOn", 0.8f, false);
        }
    }
}