using System.Collections.Generic;
using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    public GameObject startMenuUI;
    public GameObject newGameMenuUI;
    public GameObject continueMenuUI;
    public GameObject loadGameMenuUI;
    public GameObject optionsMenuUI; // Dodaj referencj� do menu opcji
    public GameObject generalOptionsMenuUI;
    public GameObject visualOptionsMenuUI;
    public GameObject soundOptionsMenuUI;

    [Header("Teksty przycisk�w")]
    public TMP_Text newGameButtonText;
    public TMP_Text continueButtonText;
    public TMP_Text loadGameButtonText;
    public TMP_Text optionsButtonText;
    public TMP_Text quitButtonText;

    // Lista wszystkich obiekt�w, kt�re posiadaj� PlaySoundOnObject
    private List<PlaySoundOnObject> playSoundObjects = new List<PlaySoundOnObject>();

    void Start()
    {
        // Sprawdzenie, czy startMenuUI jest aktywne na pocz�tku
        if (startMenuUI != null && !startMenuUI.activeSelf)
        {
            startMenuUI.SetActive(true); // Upewnij si�, �e menu startowe jest aktywne
        }

        Cursor.lockState = CursorLockMode.None; // Zmieniamy tryb kursora, by by� widoczny
        Cursor.visible = true;

        // Subskrybuj zmiany j�zyka
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged += UpdateButtonTexts;
        }

        // Zaktualizuj teksty przycisk�w
        UpdateButtonTexts();

        if (continueButtonText != null && SaveManager.Instance.GetLastUsedSlotIndex() == -1)
        {
            continueButtonText.transform.parent.gameObject.SetActive(false);
        }

        // Znajd� wszystkie obiekty posiadaj�ce PlaySoundOnObject i dodaj do listy
        playSoundObjects.AddRange(Object.FindObjectsByType<PlaySoundOnObject>(FindObjectsSortMode.None));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if ((generalOptionsMenuUI != null && generalOptionsMenuUI.activeSelf) || (visualOptionsMenuUI != null && visualOptionsMenuUI.activeSelf) || (soundOptionsMenuUI != null && soundOptionsMenuUI.activeSelf))
            {
                generalOptionsMenuUI.SetActive(false);
                visualOptionsMenuUI.SetActive(false);
                soundOptionsMenuUI.SetActive(false);

                optionsMenuUI.SetActive(true);

                foreach (var playSoundOnObject in playSoundObjects)
                {
                    if (playSoundOnObject == null) continue;

                    playSoundOnObject.PlaySound("MenuExit", 0.4f, false);
                }
            }
            // Je�li jeste� w menu opcji � wracaj do menu pauzy
            else if (optionsMenuUI.activeSelf)
            {
                optionsMenuUI.SetActive(false);
                startMenuUI.SetActive(true);

                foreach (var playSoundOnObject in playSoundObjects)
                {
                    if (playSoundOnObject == null) continue;

                    playSoundOnObject.PlaySound("MenuExit", 0.4f, false);
                }
            }
        }
    }

    // Metoda dla "Nowa Gra" - przej�cie do menu nowej gry
    public void OnNewGameButton()
    {
        Debug.Log("Klikni�to Now� Gr�");  // Dodaj debugowanie
        startMenuUI.SetActive(false); // Ukryj startowe menu
        newGameMenuUI.SetActive(true); // Poka� menu nowej gry
    }

    // Metoda dla "Wczytaj gr�" - przej�cie do menu wczytywania
    public void OnLoadGameButton()
    {
        startMenuUI.SetActive(false); // Ukryj startowe menu
        loadGameMenuUI.SetActive(true); // Poka� menu wczytywania gry
    }

    // Metoda dla "Opcje" - przej�cie do menu opcji
    public void OnOptionsButton()
    {
        startMenuUI.SetActive(false); // Ukryj startowe menu
        optionsMenuUI.SetActive(true); // Poka� menu opcji
    }

    public void OnContinueButton()
    {
        int lastSlot = SaveManager.Instance.GetLastUsedSlotIndex();
        if (lastSlot == -1)
        {
            Debug.LogWarning("Brak dost�pnego zapisu do kontynuacji!");
            return;
        }

        // Ustaw slot jako wybrany
        SaveManager.Instance.SetCurrentSlot(lastSlot);

        // Wczytaj dane gracza (DOK�ADNIE jak przy r�cznym wyborze slotu w LoadGameMenu)
        SaveManager.Instance.LoadPlayerData(lastSlot); // <-- To musi �adowa� WSZYSTKO: walut�, pozycj�, rotacj�, ekwipunek, itd.

        startMenuUI.SetActive(false);
        // Opcjonalnie: je�li masz scen� do za�adowania, tu wywo�aj SceneManager.LoadScene(...)
    }

    // Metoda dla "Wyj�cie" - zako�czenie gry
    public void OnQuitButton()
    {
        Application.Quit();
    }

    public void UpdateButtonTexts()
    {
        if (LanguageManager.Instance == null) return;

        var uiTexts = LanguageManager.Instance.CurrentUITexts;

        if (newGameButtonText != null) newGameButtonText.text = uiTexts.newGame;
        if (loadGameButtonText != null) loadGameButtonText.text = uiTexts.loadGame;
        if (continueButtonText != null) continueButtonText.text = uiTexts.continueGame;
        if (optionsButtonText != null) optionsButtonText.text = uiTexts.options;
        if (quitButtonText != null) quitButtonText.text = uiTexts.quit;
    }

    private void OnEnable()
    {

        if (SaveManager.Instance == null)
            Debug.LogError("SaveManager.Instance is NULL!");
        else
            Debug.Log("SaveManager.Instance found. LastUsedSlot: " + SaveManager.Instance.GetLastUsedSlotIndex());

        // Sprawdzenie widoczno�ci przycisku "Continue" zawsze przy aktywacji menu
        int lastSlot = SaveManager.Instance.GetLastUsedSlotIndex();
        Debug.Log("Last used slot in OnEnable: " + lastSlot);

        if (continueButtonText != null)
            continueButtonText.transform.parent.gameObject.SetActive(lastSlot != -1);
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