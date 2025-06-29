using System.Collections;
using System.Collections.Generic;
using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUI;
    public GameObject optionsMenuUI; // Dodaj referencjê do menu opcji
    public GameObject generalOptionsMenuUI;
    public GameObject visualOptionsMenuUI;
    public GameObject soundOptionsMenuUI;
    public MouseLook mouseLook;

    [Header("Teksty przycisków")]
    public TMP_Text resumeButtonText;
    public TMP_Text optionsButtonText;
    public TMP_Text mainMenuButtonText;
    public TMP_Text quitButtonText;

    // Lista wszystkich obiektów, które posiadaj¹ PlaySoundOnObject
    private List<PlaySoundOnObject> playSoundObjects = new List<PlaySoundOnObject>();

    public static PauseMenu Instance;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // NIE zostawiaj dwóch!
            return;
        }
        Instance = this;
    }
    void Start()
    {
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false); // Upewnij siê, ¿e menu pauzy jest niewidoczne na starcie
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (mouseLook == null)
        {
            mouseLook = Object.FindFirstObjectByType<MouseLook>();
            if (mouseLook == null)
            {
                Debug.LogWarning("Nie znaleziono MouseLook w scenie!");
            }
        }

        // Subskrybuj zmiany jêzyka
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged += UpdateButtonTexts;
        }

        // Zaktualizuj teksty przycisków
        UpdateButtonTexts();

        // ZnajdŸ wszystkie obiekty posiadaj¹ce PlaySoundOnObject i dodaj do listy
        playSoundObjects.AddRange(Object.FindObjectsByType<PlaySoundOnObject>(FindObjectsSortMode.None));
    }

    void Update()
    {
        if (InputBlocker.Active) return;
        // Blokada menu pauzy gdy inventory otwarte
        if (InventoryGridManager.InventoryOpen) return;
        // Blokuj pauzê jeœli trwa dialog
        if (DialogueManager.DialogueActive) return;
        // Dodanie warunku, który sprawdza, czy menu jest dostêpne
        if (!CameraToMonitor.CanUseMenu) return; // Jeœli flaga jest ustawiona na false, zablokuj dostêp do menu

        if (MissionDefiner.IsAnyDefinerActive) return;

        if (DriverSeatInteraction.IsAnyDriverSeatActive) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Jeœli jesteœ w jednym z podmenu opcji – wracaj do optionsMenuUI
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
                pauseMenuUI.SetActive(true);

                foreach (var playSoundOnObject in playSoundObjects)
                {
                    if (playSoundOnObject == null) continue;

                    playSoundOnObject.PlaySound("MenuExit", 0.4f, false);
                }
            }
            // Jeœli jesteœ w menu pauzy – wznow grê
            else if (pauseMenuUI.activeSelf)
            {
                Resume();

                foreach (var playSoundOnObject in playSoundObjects)
                {
                    if (playSoundOnObject == null) continue;

                    playSoundOnObject.PlaySound("MenuExit", 0.4f, false);
                }
            }
            // W innym wypadku – zapauzuj grê
            else
            {
                Pause();

                foreach (var playSoundOnObject in playSoundObjects)
                {
                    if (playSoundOnObject == null) continue;

                    playSoundOnObject.PlaySound("MenuEnter", 0.4f, false);
                }
            }
        }
    }


    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false); // Ukryj menu opcji na wypadek, gdyby by³o otwarte
        if (generalOptionsMenuUI != null) generalOptionsMenuUI.SetActive(false);
        if (visualOptionsMenuUI != null) visualOptionsMenuUI.SetActive(false);
        if (soundOptionsMenuUI != null) soundOptionsMenuUI.SetActive(false);

        Time.timeScale = 1f;
        GameIsPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // <--- KLUCZ: Za ka¿dym razem szukaj aktywnego MouseLook
        var mouseLook = Object.FindFirstObjectByType<MouseLook>();
        if (mouseLook != null)
            mouseLook.enabled = true;

        foreach (var playSoundOnObject in playSoundObjects)
        {
            if (playSoundOnObject == null) continue;

            playSoundOnObject.FadeOutSound("PauseMenuMusic", 1f);
            playSoundOnObject.ResumeAllSoundsExcept(new string[]{"PauseMenuMusic"},0.5f);
        }
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        optionsMenuUI.SetActive(false); // Ukryj menu opcji na wypadek, gdyby by³o otwarte
        Time.timeScale = 0f;
        GameIsPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (mouseLook != null) // <-- DODAJ TO!
            mouseLook.enabled = false; // w³¹cz kamerê

        foreach (var playSoundOnObject in playSoundObjects)
        {
            if (playSoundOnObject == null) continue;

            playSoundOnObject.PlaySound("PauseMenuMusic", 1.0f, true);
            playSoundOnObject.PauseAllSoundsExcept(new string[] { "PauseMenuMusic" }, 0.5f);
        }
    }

    public void ReturnToMainMenu()
    {
        StartCoroutine(ReturnAndClean());
    }

    private IEnumerator ReturnAndClean()
    {
        // Zbieramy wszystkie obiekty w scenie, ³¹cznie z obiektami, które s¹ ustawione na Don't Destroy On Load.
        var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        // Przechodzimy przez wszystkie obiekty i usuwamy je
        foreach (var obj in allObjects)
        {
            if (obj != null)
            {
                // Jeœli obiekt jest oznaczony jako DontDestroyOnLoad (czyli ma pust¹ nazwê sceny)
                if (string.IsNullOrEmpty(obj.scene.name))
                {
                    Destroy(obj); // Usuwamy obiekt
                }
            }
        }

        // Poczekaj na zakoñczenie usuwania obiektów (1 klatka)
        yield return null;

        // £adujemy now¹ scenê
        SceneManager.LoadScene("StartMenu");
    }

    public void LoadOptionsMenu()
    {
        pauseMenuUI.SetActive(false); // Ukryj menu pauzy
        optionsMenuUI.SetActive(true); // Poka¿ menu opcji
        Time.timeScale = 0.01f;
    }

    public void QuitGame()
    {
        Debug.Log("Quit game");
        Application.Quit();
    }

    public void UpdateButtonTexts()
    {
        if (LanguageManager.Instance == null) return;

        var uiTexts = LanguageManager.Instance.CurrentUITexts;

        if (resumeButtonText != null) resumeButtonText.text = uiTexts.resume;
        if (optionsButtonText != null) optionsButtonText.text = uiTexts.options;
        if (mainMenuButtonText != null) mainMenuButtonText.text = uiTexts.mainMenu;
        if (quitButtonText != null) quitButtonText.text = uiTexts.quit;
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