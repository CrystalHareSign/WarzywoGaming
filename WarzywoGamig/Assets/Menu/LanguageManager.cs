using UnityEngine;
using System;

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

[System.Serializable]
public class TerminalTranslations
{
    public string initializingTerminal; // ">>> Uruchamianie terminalu..."
    public string alreadyInScene;
    public string refiningBlocked;
    public string changingScene;
    public string terminalReady;
    public string commandCancelled;
    public string unknownCommand;
    public string commandMissing;
    public string confirmCommand;
    public string confirmYesKey;
    public string confirmNoKey;
    public string executingCommand;
    public string invalidResponse;
    public string command_home_desc;
    public string command_main_desc;
    public string command_help_key;
    public string terminalSecured;
    public string terminalExit;
    public string locations;
    public string hack;
    public string start;
    public string startHelp;
    public string startText;
    public string correctPassword;
    public string incorrectPassword;
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

    [Header("T³umaczenia terminala")]
    public TerminalTranslations englishTerminal;
    public TerminalTranslations polishTerminal;
    public TerminalTranslations germanTerminal;

    public UITranslations CurrentUITexts => currentLanguage switch
    {
        Language.Polski => polishTexts,
        Language.Deutsch => germanTexts,
        _ => englishTexts
    };

    public TerminalTranslations CurrentTerminalTexts => currentLanguage switch
    {
        Language.Polski => polishTerminal,
        Language.Deutsch => germanTerminal,
        _ => englishTerminal
    };

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
        foreach (var hover in UnityEngine.Object.FindObjectsByType<HoverMessage>(FindObjectsSortMode.None))
        {
            hover.UpdateLocalizedMessage();
        }
    }

    public string GetLocalizedMessage(string key)
    {
        var terminal = CurrentTerminalTexts;

        // U¿ycie refleksji do dynamicznego mapowania kluczy na pola
        var field = typeof(TerminalTranslations).GetField(key, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            return field.GetValue(terminal)?.ToString() ?? $"[[UNKNOWN KEY: {key}]]";
        }

        // Loguj brakuj¹cy klucz (pomocne w debugowaniu)
        //Debug.LogWarning($"[GetLocalizedMessage] Key '{key}' not found in TerminalTranslations.");
        return $"{key}";
    }
}
