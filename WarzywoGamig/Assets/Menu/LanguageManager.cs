using UnityEngine;

[System.Serializable]
public class UITranslations
{
    public string resume;
    public string options;
    public string quit;
}

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance;

    public enum Language { English, Polski, Deutsch }
    public Language currentLanguage = Language.English;

    [Header("T³umaczenia UI")]
    public UITranslations englishTexts;
    public UITranslations polishTexts;
    public UITranslations germanTexts;

    public UITranslations CurrentUITexts
    {
        get
        {
            return currentLanguage switch
            {
                Language.Polski => polishTexts,
                Language.Deutsch => germanTexts,
                _ => englishTexts,
            };
        }
    }

    public delegate void LanguageChanged();
    public event LanguageChanged OnLanguageChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetLanguage(Language lang)
    {
        currentLanguage = lang;
        PlayerPrefs.SetInt("GameLanguage", (int)lang);
        PlayerPrefs.Save();
        OnLanguageChanged?.Invoke();

        // Aktualizacja HoverMessages w scenie
        foreach (var hover in FindObjectsOfType<HoverMessage>())
        {
            hover.UpdateLocalizedMessage();
        }
    }
}
