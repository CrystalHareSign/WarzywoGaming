using UnityEngine;

public class OptionsMenu : MonoBehaviour
{
    public GameObject optionsMenuUI;
    public GameObject pauseMenuUI;
    public GameObject generalOptionsPanel;
    public GameObject soundOptionsPanel;
    public GameObject visualOptionsPanel;

    public static OptionsMenu instance;

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
        optionsMenuUI.SetActive(false); // Upewnij siê, ¿e menu opcji jest niewidoczne na starcie
    }

    public void ShowGeneralSettings()
    {
        generalOptionsPanel.SetActive(true);
        soundOptionsPanel.SetActive(false);
        //visualOptionsPanel.SetActive(false);
        optionsMenuUI.SetActive(false); // Ukryj menu opcji
    }

    public void ShowSoundSettings()
    {
        generalOptionsPanel.SetActive(false);
        soundOptionsPanel.SetActive(true);
        //visualSettingsPanel.SetActive(false);
        optionsMenuUI.SetActive(false); // Ukryj menu opcji
    }

    public void ShowVisualSettings()
    {
        generalOptionsPanel.SetActive(false);
        soundOptionsPanel.SetActive(false);
        //visualOptionsPanel.SetActive(true);
        optionsMenuUI.SetActive(false); // Ukryj menu opcji
    }
    public void BackToPauseMenu()
    {
        optionsMenuUI.SetActive(false); // Ukryj menu opcji
        pauseMenuUI.SetActive(true); // Poka¿ menu pauzy
    }
}
