using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class NewGameMenu : MonoBehaviour
{
    public TMP_Text[] slotTexts;
    public Button[] slotButtons;
    public Button newGameButton;
    public Button backButton;
    public GameObject startMenuUI;
    public GameObject newGameMenuUI;
    public TMP_Text newGameButtonText;
    public TMP_Text backButtonText;


    private int selectedSlotIndex = -1;

    // Panel potwierdzenia nadpisania
    public GameObject overwriteConfirmUI;
    public TMP_Text overwriteQuestionText;
    public Button overwriteYesButton;
    public Button overwriteNoButton;

    // Panel z polem tekstowym dla nazwy
    public GameObject nameConfirmUI;
    public TMP_InputField slotNameInputField;
    public TMP_Text slotNameLabelText; // JEDYNY label nad inputem
    public Button nameYesButton;
    public Button nameNoButton;
    public TMP_Text nameYesButtonText;
    public TMP_Text nameNoButtonText;

    // Lista wszystkich obiektów, które posiadaj¹ PlaySoundOnObject
    private List<PlaySoundOnObject> playSoundObjects = new List<PlaySoundOnObject>();

    // Do przechowywania w³asnych nazw slotów (mo¿esz to trzymaæ w SaveManagerze!)
    private string[] customSlotNames = new string[10]; // Dostosuj do liczby slotów

    private void Start()
    {
        selectedSlotIndex = -1;
        UpdateSlotTexts();
        UpdateButtonStates();
        UpdateButtonTexts();
        UpdateNamePanelYesNoTexts();

        playSoundObjects.AddRange(Object.FindObjectsByType<PlaySoundOnObject>(FindObjectsSortMode.None));

        if (overwriteConfirmUI != null) overwriteConfirmUI.SetActive(false);
        if (nameConfirmUI != null) nameConfirmUI.SetActive(false);

        UpdateOverwritePanelTexts();

        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged += UpdateOverwritePanelTexts;
            LanguageManager.Instance.OnLanguageChanged += UpdateButtonTexts;   // <-- NOWE
            LanguageManager.Instance.OnLanguageChanged += UpdateSlotTexts;     // <-- (opcjonalnie)
            LanguageManager.Instance.OnLanguageChanged += UpdateNamePanelYesNoTexts;
        }
    }

    private void OnEnable() 
    {
        UpdateSlotTexts();
        UpdateButtonStates();
        UpdateButtonTexts();
        UpdateNamePanelYesNoTexts();
    }

    private void OnDestroy()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged -= UpdateOverwritePanelTexts;
            LanguageManager.Instance.OnLanguageChanged -= UpdateButtonTexts;   // <-- NOWE
            LanguageManager.Instance.OnLanguageChanged -= UpdateSlotTexts;     // <-- (opcjonalnie)
            LanguageManager.Instance.OnLanguageChanged += UpdateNamePanelYesNoTexts;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnBackToStartMenu();
            foreach (var playSoundOnObject in playSoundObjects)
            {
                if (playSoundOnObject == null) continue;
                playSoundOnObject.PlaySound("MenuExit", 0.4f, false);
            }
        }
    }

    public void OnSlotSelected(int index)
    {
        selectedSlotIndex = index;
        SaveManager.Instance.SetCurrentSlot(index);
        UpdateButtonStates();
    }

    public void OnStartNewGame()
    {
        if (selectedSlotIndex < 0) return;

        UpdateOverwritePanelTexts();
        overwriteConfirmUI.SetActive(true);
    }

    // Potwierdzenie pierwszego okna (czy nadpisaæ)
    public void OnConfirmOverwriteClicked()
    {
        overwriteConfirmUI.SetActive(false);

        // Poka¿ panel z polem na nazwê
        nameConfirmUI.SetActive(true);
        slotNameInputField.text = ""; // wyczyœæ pole

        // Ustaw zawsze jeden tekst nad polem input
        var uiTexts = LanguageManager.Instance.CurrentUITexts;
        if (slotNameLabelText != null)
            slotNameLabelText.text = uiTexts.slotNameLabel; // "Nazwa zapisu:" / "Save name:"
    }

    public void OnCancelOverwriteClicked()
    {
        overwriteConfirmUI.SetActive(false);
    }

    public void OnConfirmNameClicked()
    {
        string slotName = slotNameInputField.text.Trim();
        var uiTexts = LanguageManager.Instance.CurrentUITexts;

        if (string.IsNullOrEmpty(slotName))
        {
            slotNameInputField.text = "";
            slotNameInputField.placeholder.GetComponent<TMP_Text>().text = uiTexts.slotNameLabel;
            return;
        }

        // Ustaw nazwê slotu w SaveManagerze (to zapisze j¹ do pliku!)
        SaveManager.Instance.SetCurrentSlotName(slotName);

        nameConfirmUI.SetActive(false);
        StartNewGame();
    }

    public void OnCancelNameClicked()
    {
        nameConfirmUI.SetActive(false);
    }

    private void StartNewGame()
    {
        SaveManager.Instance.ResetSaveSlot(selectedSlotIndex);
        SaveManager.Instance.ResetCurrency();
        startMenuUI.SetActive(false);
        SceneChanger.lastRelativePlayerPos = SceneChanger.defaultRelativePlayerPos;
        SceneManager.LoadScene("Main");
    }

    public void OnBackToStartMenu()
    {
        if (newGameMenuUI != null)
            newGameMenuUI.SetActive(false);
        if (startMenuUI != null)
            startMenuUI.SetActive(true);
    }

    private void UpdateSlotTexts()
    {
        var uiTexts = LanguageManager.Instance.CurrentUITexts;
        for (int i = 0; i < slotTexts.Length; i++)
        {
            if (SaveManager.Instance == null) continue;

            if (SaveManager.Instance.DoesSlotExist(i))
            {
                var data = SaveManager.Instance.LoadDataWithoutApplying(i);
                string name = !string.IsNullOrEmpty(data.slotName) ? data.slotName : uiTexts.unnamedSlot;
                slotTexts[i].text = $"{name} - {data.lastSaveTime}";
            }
            else
            {
                slotTexts[i].text = uiTexts.emptySlot;
            }
        }
    }

    private void UpdateButtonStates()
    {
        for (int i = 0; i < slotButtons.Length; i++)
        {
            var colors = slotButtons[i].colors;
            colors.normalColor = (i == selectedSlotIndex) ? Color.yellow : Color.white;
            slotButtons[i].colors = colors;
        }
        newGameButton.interactable = selectedSlotIndex >= 0;
    }

    private void UpdateButtonTexts()
    {
        if (LanguageManager.Instance == null) return;
        var uiTexts = LanguageManager.Instance.CurrentUITexts;
        if (newGameButtonText != null) newGameButtonText.text = uiTexts.newGame;
        if (backButtonText != null) backButtonText.text = uiTexts.back4;
    }

    private void UpdateNamePanelYesNoTexts()
    {
        if (LanguageManager.Instance == null) return;
        var uiTexts = LanguageManager.Instance.CurrentUITexts;
        if (nameYesButtonText != null) nameYesButtonText.text = uiTexts.newGame;
        if (nameNoButtonText != null) nameNoButtonText.text = uiTexts.no;
    }

    private void UpdateOverwritePanelTexts()
    {
        if (overwriteYesButton != null && overwriteNoButton != null && LanguageManager.Instance != null)
        {
            var uiTexts = LanguageManager.Instance.CurrentUITexts;
            var yesText = overwriteYesButton.GetComponentInChildren<TMP_Text>();
            var noText = overwriteNoButton.GetComponentInChildren<TMP_Text>();
            if (yesText != null) yesText.text = uiTexts.yes;
            if (noText != null) noText.text = uiTexts.no;

            if (overwriteQuestionText != null)
            {
                if (SaveManager.Instance != null && selectedSlotIndex >= 0)
                {
                    if (SaveManager.Instance.DoesSlotExist(selectedSlotIndex))
                        overwriteQuestionText.text = uiTexts.confirmOverwrite;
                    else
                        overwriteQuestionText.text = uiTexts.confirmNewGame;
                }
                else
                {
                    overwriteQuestionText.text = "";
                }
            }
        }
        // Analogicznie mo¿esz ustawiæ teksty w panelu nameConfirmUI (przy zmianie jêzyka)
        // Tu ju¿ nie ma potrzeby obs³ugi osobnych labeli dla panelu z nazw¹
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