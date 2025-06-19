using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HoverMessage : MonoBehaviour
{
    [Header("Teksty po jêzykach")]
    [TextArea] public string messageEnglish;
    [TextArea] public string messagePolish;

    [Header("Teksty info (popup) po jêzykach")]
    [TextArea] public string infoEnglish;
    [TextArea] public string infoPolish;

    [Header("Tekst klawisza (wspólny)")]
    public string keyText = "[E]";

    public float interactionDistance = 5f;
    public bool isInteracted = false;
    public bool alwaysActive = false;

    [Header("SCENY")]
    public bool UsingSceneSystem = false;
    public bool SceneMain = false;
    public bool SceneHome = false;

    [Header("Wygl¹d")]
    public int messageFontSize = 20;
    public int keyFontSize = 30;

    [Header("Wygl¹d popupa")]
    public int infoFontSize = 20; // Rozmiar czcionki popupu
    public float duration = 3f;   // Czas wyœwietlania popupu w sekundach

    [HideInInspector] public string message;
    [HideInInspector] public string infoMessage;

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        UpdateLocalizedMessage();
        CheckSceneStatus();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateLocalizedMessage();
        CheckSceneStatus();
    }

    public void UpdateLocalizedMessage()
    {
        if (LanguageManager.Instance == null) return;

        switch (LanguageManager.Instance.currentLanguage)
        {
            case LanguageManager.Language.English:
                message = messageEnglish;
                infoMessage = infoEnglish;
                break;
            case LanguageManager.Language.Polski:
                message = messagePolish;
                infoMessage = infoPolish;
                break;
            default:
                message = messageEnglish;
                infoMessage = infoEnglish;
                break;
        }
    }

    private void CheckSceneStatus()
    {
        if (!UsingSceneSystem) return;

        string currentSceneName = SceneManager.GetActiveScene().name;

        if (SceneMain && currentSceneName != "Main")
        {
            isInteracted = true;
        }
        else if (SceneHome && currentSceneName != "Home")
        {
            isInteracted = true;
        }
        else
        {
            isInteracted = false;
        }
    }
}