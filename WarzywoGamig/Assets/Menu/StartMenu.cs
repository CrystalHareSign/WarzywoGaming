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
    public GameObject optionsMenuUI; // Dodaj referencjê do menu opcji
    public GameObject generalOptionsMenuUI;
    public GameObject visualOptionsMenuUI;
    public GameObject soundOptionsMenuUI;

    [Header("Teksty przycisków")]
    public TMP_Text newGameButtonText;
    public TMP_Text continueButtonText;
    public TMP_Text loadGameButtonText;
    public TMP_Text optionsButtonText;
    public TMP_Text quitButtonText;

    // Lista wszystkich obiektów, które posiadaj¹ PlaySoundOnObject
    private List<PlaySoundOnObject> playSoundObjects = new List<PlaySoundOnObject>();

    void Start()
    {
        // Sprawdzenie, czy startMenuUI jest aktywne na pocz¹tku
        if (startMenuUI != null && !startMenuUI.activeSelf)
        {
            startMenuUI.SetActive(true); // Upewnij siê, ¿e menu startowe jest aktywne
        }

        Cursor.lockState = CursorLockMode.None; // Zmieniamy tryb kursora, by by³ widoczny
        Cursor.visible = true;

        // Subskrybuj zmiany jêzyka
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged += UpdateButtonTexts;
        }

        // Zaktualizuj teksty przycisków
        UpdateButtonTexts();

        if (continueButtonText != null && SaveManager.Instance.GetLastUsedSlotIndex() == -1)
        {
            continueButtonText.transform.parent.gameObject.SetActive(false);
        }

        // ZnajdŸ wszystkie obiekty posiadaj¹ce PlaySoundOnObject i dodaj do listy
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
            // Jeœli jesteœ w menu opcji – wracaj do menu pauzy
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

    // Metoda dla "Nowa Gra" - przejœcie do menu nowej gry
    public void OnNewGameButton()
    {
        Debug.Log("Klikniêto Now¹ Grê");  // Dodaj debugowanie
        startMenuUI.SetActive(false); // Ukryj startowe menu
        newGameMenuUI.SetActive(true); // Poka¿ menu nowej gry
    }

    // Metoda dla "Wczytaj grê" - przejœcie do menu wczytywania
    public void OnLoadGameButton()
    {
        startMenuUI.SetActive(false); // Ukryj startowe menu
        loadGameMenuUI.SetActive(true); // Poka¿ menu wczytywania gry
    }

    // Metoda dla "Opcje" - przejœcie do menu opcji
    public void OnOptionsButton()
    {
        startMenuUI.SetActive(false); // Ukryj startowe menu
        optionsMenuUI.SetActive(true); // Poka¿ menu opcji
    }

    public void OnContinueButton()
    {
        int lastSlot = SaveManager.Instance.GetLastUsedSlotIndex();
        if (lastSlot == -1)
        {
            Debug.LogWarning("Brak dostêpnego zapisu do kontynuacji!");
            return;
        }

        // Ustaw slot jako wybrany
        SaveManager.Instance.SetCurrentSlot(lastSlot);

        // Wczytaj dane gracza (DOK£ADNIE jak przy rêcznym wyborze slotu w LoadGameMenu)
        SaveManager.Instance.LoadPlayerData(lastSlot); // <-- To musi ³adowaæ WSZYSTKO: walutê, pozycjê, rotacjê, ekwipunek, itd.

        startMenuUI.SetActive(false);
        // Opcjonalnie: jeœli masz scenê do za³adowania, tu wywo³aj SceneManager.LoadScene(...)
    }

    // Metoda dla "Wyjœcie" - zakoñczenie gry
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

        // Sprawdzenie widocznoœci przycisku "Continue" zawsze przy aktywacji menu
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