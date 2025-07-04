using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GeneralOptionsMenu : MonoBehaviour
{
    [SerializeField] private PauseMenu pauseMenu;
    public GameObject generalOptionsMenuUI;
    public GameObject optionsMenuUI;

    public TMP_Dropdown languageDropdown;

    [Header("Domy�lny j�zyk")]
    public LanguageManager.Language defaultLanguage = LanguageManager.Language.English;

    private LanguageManager.Language tempSelectedLanguage;

    [Header("Teksty przycisk�w")]
    public TMP_Text apply1ButtonText;
    public TMP_Text cancel1ButtonText;
    public TMP_Text back1ButtonText;
    public TMP_Text reset1ButtonText;
    public TMP_Text languageText;

    // Lista wszystkich obiekt�w, kt�re posiadaj� PlaySoundOnObject
    private List<PlaySoundOnObject> playSoundObjects = new List<PlaySoundOnObject>();


    void Start()
    {
        InitializeDropdown();

        // Subskrybuj zmiany j�zyka
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged += UpdateButtonTexts;
        }

        // Zaktualizuj teksty przycisk�w
        UpdateButtonTexts();

        // Znajd� wszystkie obiekty posiadaj�ce PlaySoundOnObject i dodaj do listy
        playSoundObjects.AddRange(Object.FindObjectsByType<PlaySoundOnObject>(FindObjectsSortMode.None));
    }

    void InitializeDropdown()
    {
        if (languageDropdown == null)
        {
            Debug.LogError("Brak przypisanego TMP_Dropdown w Inspectorze!");
            return;
        }

        languageDropdown.ClearOptions();

        var languages = System.Enum.GetNames(typeof(LanguageManager.Language));
        languageDropdown.AddOptions(new System.Collections.Generic.List<string>(languages));

        int savedIndex = PlayerPrefs.HasKey("GameLanguage") ? PlayerPrefs.GetInt("GameLanguage") : (int)LanguageManager.Instance.currentLanguage;
        tempSelectedLanguage = (LanguageManager.Language)savedIndex;

        languageDropdown.value = savedIndex;
        languageDropdown.RefreshShownValue();

        languageDropdown.onValueChanged.AddListener(delegate { OnLanguageChanged(languageDropdown.value); });
    }

    public void OnLanguageChanged(int index)
    {
        tempSelectedLanguage = (LanguageManager.Language)index;
        Debug.Log("Tymczasowo wybrano j�zyk: " + tempSelectedLanguage);
    }

    public void ApplyChanges()
    {
        LanguageManager.Instance.SetLanguage(tempSelectedLanguage); // ustawia currentLanguage, zapisuje do PlayerPrefs, wywo�uje event
        PlayerPrefs.SetInt("GameLanguage", (int)tempSelectedLanguage);
        PlayerPrefs.Save();

        // Dodatkowo zapisujemy do JSON, by na starcie gry wczyta� ostatni wyb�r
        LanguageManager.Instance.SaveLanguageToJson(tempSelectedLanguage);

        Debug.Log("Zastosowano j�zyk: " + tempSelectedLanguage);

        if (pauseMenu != null)
            pauseMenu.UpdateButtonTexts();
    }

    public void CancelChanges()
    {
        int savedIndex = PlayerPrefs.HasKey("GameLanguage") ? PlayerPrefs.GetInt("GameLanguage") : (int)LanguageManager.Instance.currentLanguage;

        languageDropdown.value = savedIndex;
        tempSelectedLanguage = (LanguageManager.Language)savedIndex;
        languageDropdown.RefreshShownValue();

        //Debug.Log("Anulowano zmiany j�zyka");
    }

    public void ResetToDefaults()
    {
        languageDropdown.value = (int)defaultLanguage;
        tempSelectedLanguage = defaultLanguage;
        languageDropdown.RefreshShownValue();
        ApplyChanges();

        Debug.Log("Zresetowano j�zyk do domy�lnego: " + defaultLanguage);
    }

    public void BackToOptionsMenu()
    {
        CancelChanges();
        generalOptionsMenuUI.SetActive(false);
        optionsMenuUI.SetActive(true);
    }

    public void UpdateButtonTexts()
    {
        //Debug.Log("UpdateButtonTexts called");

        if (LanguageManager.Instance == null) return;
        var uiTexts = LanguageManager.Instance.CurrentUITexts;

        if (apply1ButtonText != null) apply1ButtonText.text = uiTexts.apply1;
        if (cancel1ButtonText != null) cancel1ButtonText.text = uiTexts.cancel1;
        if (back1ButtonText != null) back1ButtonText.text = uiTexts.back1;
        if (reset1ButtonText != null) reset1ButtonText.text = uiTexts.reset1;
        if (languageText != null) languageText.text = uiTexts.language;
    }

    void OnEnable()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged += UpdateButtonTexts;
            UpdateButtonTexts();
        }
    }

    void OnDisable()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged -= UpdateButtonTexts;
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
        foreach (var playSoundOnObject in playSoundObjects)
        {
            if (playSoundOnObject == null) continue;

            playSoundOnObject.PlaySound("MenuMouseOn", 0.8f, false);
        }
    }
}
