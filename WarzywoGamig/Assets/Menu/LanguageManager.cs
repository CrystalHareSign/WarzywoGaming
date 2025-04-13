using UnityEngine;

[System.Serializable]
public class UITranslations
{
    [Header("PauseMenu")]
    public string resume;
    public string options;
    public string quit;
    [Header("OptionsMenu")]
    public string generalSettings;
    public string soundSettings;
    public string visualSettings;
    public string back;
    [Header("GeneralMenu")]
    public string apply1;
    public string cancel1;
    public string back1;
    public string reset1;
    public string language;
    [Header("SoundMenu")]
    public string apply2;
    public string cancel2;
    public string back2;
    public string reset2;
    public string music;
    public string sfx;
    public string ambient;
    [Header("VisualMenu")]
    public string apply3;
    public string cancel3;
    public string back3;
    public string reset3;
    public string resolution;
    public string fullscreen;
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
