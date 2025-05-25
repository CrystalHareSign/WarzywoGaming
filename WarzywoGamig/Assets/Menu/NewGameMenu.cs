using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class NewGameMenu : MonoBehaviour
{
    public TMP_Text[] slotTexts;          // Teksty przycisków slotów
    public Button[] slotButtons;          // Referencje do przycisków slotów
    public Button newGameButton;          // Przycisk "Nowa Gra"
    public Button backButton;             // Przycisk "Wróæ"
    public GameObject startMenuUI;        // Referencja do Start Menu UI
    public GameObject newGameMenuUI;        // Referencja do Start Menu UI

    private int selectedSlotIndex = -1;

    // Lista wszystkich obiektów, które posiadaj¹ PlaySoundOnObject
    private List<PlaySoundOnObject> playSoundObjects = new List<PlaySoundOnObject>();

    private void Start()
    {
        selectedSlotIndex = -1;
        UpdateSlotTexts();
        UpdateButtonStates();

        // ZnajdŸ wszystkie obiekty posiadaj¹ce PlaySoundOnObject i dodaj do listy
        playSoundObjects.AddRange(Object.FindObjectsByType<PlaySoundOnObject>(FindObjectsSortMode.None));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnBackToStartMenu();

            // DŸwiêk wyjœcia z menu (opcjonalnie)
            foreach (var playSoundOnObject in playSoundObjects)
            {
                if (playSoundOnObject == null) continue;
                playSoundOnObject.PlaySound("MenuExit", 0.4f, false);
            }
        }
    }

    // Klikniêcie slotu
    public void OnSlotSelected(int index)
    {
        selectedSlotIndex = index;
        SaveManager.Instance.SetCurrentSlot(index);
        UpdateButtonStates();
    }

    // Klikniêcie "Nowa Gra"
    public void OnStartNewGame()
    {
        if (selectedSlotIndex < 0) return;

        SaveManager.Instance.ResetSaveSlot(selectedSlotIndex);
        SaveManager.Instance.ResetCurrency(); // opcjonalnie zresetuj walutê itp.
        startMenuUI.SetActive(false);
        SceneChanger.lastRelativePlayerPos = SceneChanger.defaultRelativePlayerPos;
        SceneManager.LoadScene("Main"); // <-- Zmieñ na swoj¹ scenê gry
    }

    // Klikniêcie "Wróæ"
    public void OnBackToStartMenu()
    {
        if (newGameMenuUI != null)
            newGameMenuUI.SetActive(false);
        if (startMenuUI != null)
            startMenuUI.SetActive(true);
    }

    private void UpdateSlotTexts()
    {
        for (int i = 0; i < slotTexts.Length; i++)
        {
            if (SaveManager.Instance == null) continue;  // Dodaj sprawdzenie, czy GameManager istnieje

            if (SaveManager.Instance.DoesSlotExist(i))
            {
                var data = SaveManager.Instance.LoadDataWithoutApplying(i);
                slotTexts[i].text = $"Slot {i + 1} - {data.lastSaveTime}";
            }
            else
            {
                slotTexts[i].text = $"Slot {i + 1} - Pusty";
            }
        }
    }

    private void UpdateButtonStates()
    {
        // Podœwietl wybrany slot
        for (int i = 0; i < slotButtons.Length; i++)
        {
            var colors = slotButtons[i].colors;
            colors.normalColor = (i == selectedSlotIndex) ? Color.yellow : Color.white;
            slotButtons[i].colors = colors;
        }

        // Aktywuj przycisk Nowa Gra tylko jeœli wybrano slot
        newGameButton.interactable = selectedSlotIndex >= 0;
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
