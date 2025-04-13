using TMPro;
using UnityEngine;

public class GeneralOptionsMenu : MonoBehaviour
{
    public GameObject generalOptionsMenuUI;
    public GameObject optionsMenuUI;

    public TMP_Dropdown languageDropdown;

    [Header("Domyœlny jêzyk")]
    public LanguageManager.Language defaultLanguage = LanguageManager.Language.English;

    public static GeneralOptionsMenu instance;

    private LanguageManager.Language tempSelectedLanguage;

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
        InitializeDropdown();
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
        Debug.Log("Tymczasowo wybrano jêzyk: " + tempSelectedLanguage);
    }

    public void ApplyChanges()
    {
        LanguageManager.Instance.SetLanguage(tempSelectedLanguage);
        PlayerPrefs.SetInt("GameLanguage", (int)tempSelectedLanguage);
        PlayerPrefs.Save();
        Debug.Log("Zastosowano jêzyk: " + tempSelectedLanguage);

        PauseMenu.instance?.UpdateButtonTexts();
    }

    public void CancelChanges()
    {
        int savedIndex = PlayerPrefs.HasKey("GameLanguage") ? PlayerPrefs.GetInt("GameLanguage") : (int)LanguageManager.Instance.currentLanguage;

        languageDropdown.value = savedIndex;
        tempSelectedLanguage = (LanguageManager.Language)savedIndex;
        languageDropdown.RefreshShownValue();

        Debug.Log("Anulowano zmiany jêzyka");
    }

    public void ResetToDefaults()
    {
        languageDropdown.value = (int)defaultLanguage;
        tempSelectedLanguage = defaultLanguage;
        languageDropdown.RefreshShownValue();

        Debug.Log("Zresetowano jêzyk do domyœlnego: " + defaultLanguage);
    }

    public void BackToOptionsMenu()
    {
        CancelChanges();
        generalOptionsMenuUI.SetActive(false);
        optionsMenuUI.SetActive(true);
    }
}
