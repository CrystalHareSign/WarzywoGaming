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
    public GameObject optionsMenuUI;
    public GameObject generalOptionsMenuUI;
    public GameObject visualOptionsMenuUI;
    public GameObject soundOptionsMenuUI;
    public MouseLook mouseLook;

    [Header("Teksty przycisków")]
    public TMP_Text resumeButtonText;
    public TMP_Text optionsButtonText;
    public TMP_Text mainMenuButtonText;
    public TMP_Text quitButtonText;

    private List<PlaySoundOnObject> playSoundObjects = new List<PlaySoundOnObject>();

    public static PauseMenu Instance;

    // --- DODANE: flaga, która blokuje menu pauzy po wyjœciu ESC ---
    private bool blockPauseOnEsc = false;
    private float blockPauseTimer = 0f;
    private float blockPauseDuration = 0.5f; // ile sekund ESC blokuje menu pauzy po zamkniêciu (dowolnie dostosuj)

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false);
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

        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged += UpdateButtonTexts;
        }

        UpdateButtonTexts();
        playSoundObjects.AddRange(Object.FindObjectsByType<PlaySoundOnObject>(FindObjectsSortMode.None));
    }

    void Update()
    {
        if (InventoryUI.Instance.isMenuActive)return;
        if (InputBlocker.Active) return;
        if (DialogueManager.DialogueActive) return;
        if (!CameraToMonitor.CanUseMenu) return;
        if (MissionDefiner.IsAnyDefinerActive) return;
        if (DriverSeatInteraction.IsAnyDriverSeatActive) return;

        // --- Blokada menu pauzy tu¿ po ESC --- (reset co klatkê, jeœli aktywna)
        if (blockPauseOnEsc)
        {
            blockPauseTimer += Time.unscaledDeltaTime;
            if (blockPauseTimer > blockPauseDuration)
            {
                blockPauseOnEsc = false;
                blockPauseTimer = 0f;
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Jeœli jesteœ w jednym z podmenu opcji – wracaj do optionsMenuUI
            if ((generalOptionsMenuUI != null && generalOptionsMenuUI.activeSelf) ||
                (visualOptionsMenuUI != null && visualOptionsMenuUI.activeSelf) ||
                (soundOptionsMenuUI != null && soundOptionsMenuUI.activeSelf))
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
            // Jeœli jesteœ w menu pauzy – wznow grê i ZABLOKUJ wywo³anie menu pauzy przez ESC na chwilê
            else if (pauseMenuUI.activeSelf)
            {
                Resume();

                foreach (var playSoundOnObject in playSoundObjects)
                {
                    if (playSoundOnObject == null) continue;
                    playSoundOnObject.PlaySound("MenuExit", 0.4f, false);
                }
                // --- DODANE: blokada menu pauzy po ESC ---
                blockPauseOnEsc = true;
                blockPauseTimer = 0f;
            }
            // W innym wypadku – zapauzuj grê, ale TYLKO jeœli NIE jest aktywna blokada po ESC
            else
            {
                if (!blockPauseOnEsc)
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
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);
        if (generalOptionsMenuUI != null) generalOptionsMenuUI.SetActive(false);
        if (visualOptionsMenuUI != null) visualOptionsMenuUI.SetActive(false);
        if (soundOptionsMenuUI != null) soundOptionsMenuUI.SetActive(false);

        Time.timeScale = 1f;
        GameIsPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        var mouseLook = Object.FindFirstObjectByType<MouseLook>();
        if (mouseLook != null)
            mouseLook.enabled = true;

        foreach (var playSoundOnObject in playSoundObjects)
        {
            if (playSoundOnObject == null) continue;
            playSoundOnObject.FadeOutSound("PauseMenuMusic", 1f);
            playSoundOnObject.ResumeAllSoundsExcept(new string[] { "PauseMenuMusic" }, 0.5f);
        }
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        optionsMenuUI.SetActive(false);
        Time.timeScale = 0f;
        GameIsPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (mouseLook != null)
            mouseLook.enabled = false;

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
        var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var obj in allObjects)
        {
            if (obj != null)
            {
                if (string.IsNullOrEmpty(obj.scene.name))
                {
                    Destroy(obj);
                }
            }
        }
        yield return null;
        SceneManager.LoadScene("StartMenu");
    }

    public void LoadOptionsMenu()
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(true);
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