using System.Collections.Generic;
using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUI;
    public GameObject optionsMenuUI; // Dodaj referencj� do menu opcji
    public GameObject generalOptionsMenuUI;
    public GameObject visualOptionsMenuUI;
    public GameObject soundOptionsMenuUI;
    public MouseLook mouseLook;

    [Header("Teksty przycisk�w")]
    public TMP_Text resumeButtonText;
    public TMP_Text optionsButtonText;
    public TMP_Text quitButtonText;


    public static PauseMenu instance;

    // Lista wszystkich obiekt�w, kt�re posiadaj� PlaySoundOnObject
    private List<PlaySoundOnObject> playSoundObjects = new List<PlaySoundOnObject>();

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
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false); // Upewnij si�, �e menu pauzy jest niewidoczne na starcie
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

        // Subskrybuj zmiany j�zyka
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged += UpdateButtonTexts;
        }

        // Zaktualizuj teksty przycisk�w
        UpdateButtonTexts();

        // Znajd� wszystkie obiekty posiadaj�ce PlaySoundOnObject i dodaj do listy
        playSoundObjects.AddRange(Object.FindObjectsOfType<PlaySoundOnObject>());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Je�li jeste� w jednym z podmenu opcji � wracaj do optionsMenuUI
            if (generalOptionsMenuUI.activeSelf || visualOptionsMenuUI.activeSelf || soundOptionsMenuUI.activeSelf)
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
                pauseMenuUI.SetActive(true);

                foreach (var playSoundOnObject in playSoundObjects)
                {
                    if (playSoundOnObject == null) continue;

                    playSoundOnObject.PlaySound("MenuExit", 0.4f, false);
                }
            }
            // Je�li jeste� w menu pauzy � wznow gr�
            else if (pauseMenuUI.activeSelf)
            {
                Resume();

                foreach (var playSoundOnObject in playSoundObjects)
                {
                    if (playSoundOnObject == null) continue;

                    playSoundOnObject.PlaySound("MenuExit", 0.4f, false);
                }
            }
            // W innym wypadku � zapauzuj gr�
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
        optionsMenuUI.SetActive(false); // Ukryj menu opcji na wypadek, gdyby by�o otwarte
        Time.timeScale = 1f;
        GameIsPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        mouseLook.enabled = true; // w��cz kamer�

        foreach (var playSoundOnObject in playSoundObjects)
        {
            if (playSoundOnObject == null) continue;

            playSoundOnObject.StopSound("PauseMenuMusic");
        }
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        optionsMenuUI.SetActive(false); // Ukryj menu opcji na wypadek, gdyby by�o otwarte
        Time.timeScale = 0f;
        GameIsPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        mouseLook.enabled = false; // wy��cz kamer�

        foreach (var playSoundOnObject in playSoundObjects)
        {
            if (playSoundOnObject == null) continue;

            playSoundOnObject.PlaySound("PauseMenuMusic", 1.0f, true);
        }
    }

    public void LoadOptionsMenu()
    {
        pauseMenuUI.SetActive(false); // Ukryj menu pauzy
        optionsMenuUI.SetActive(true); // Poka� menu opcji
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