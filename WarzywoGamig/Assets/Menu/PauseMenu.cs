using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUI;
    public GameObject optionsMenuUI; // Dodaj referencjê do menu opcji

    void Start()
    {
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false); // Upewnij siê, ¿e menu pauzy jest niewidoczne na starcie
        optionsMenuUI.SetActive(false); // Upewnij siê, ¿e menu opcji jest niewidoczne na starcie
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false); // Ukryj menu opcji na wypadek, gdyby by³o otwarte
        Time.timeScale = 1f;
        GameIsPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        optionsMenuUI.SetActive(false); // Ukryj menu opcji na wypadek, gdyby by³o otwarte
        Time.timeScale = 0f;
        GameIsPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void LoadOptionsMenu()
    {
        pauseMenuUI.SetActive(false); // Ukryj menu pauzy
        optionsMenuUI.SetActive(true); // Poka¿ menu opcji
        Time.timeScale = 0.01f;
    }

    public void BackToPauseMenu()
    {
        optionsMenuUI.SetActive(false); // Ukryj menu opcji
        pauseMenuUI.SetActive(true); // Poka¿ menu pauzy
    }

    public void QuitGame()
    {
        Debug.Log("Quit game");
        Application.Quit();
    }
}