using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class NewGameMenu : MonoBehaviour
{
    public TMP_Text[] slotTexts;          // Teksty przycisk�w slot�w
    public Button[] slotButtons;          // Referencje do przycisk�w slot�w
    public Button newGameButton;          // Przycisk "Nowa Gra"
    public Button backButton;             // Przycisk "Wr��"
    public GameObject startMenuUI;        // Referencja do Start Menu UI
    public GameObject newGameMenuUI;        // Referencja do Start Menu UI

    private int selectedSlotIndex = -1;

    // Lista wszystkich obiekt�w, kt�re posiadaj� PlaySoundOnObject
    private List<PlaySoundOnObject> playSoundObjects = new List<PlaySoundOnObject>();

    private void Start()
    {
        selectedSlotIndex = -1;
        UpdateSlotTexts();
        UpdateButtonStates();

        // Znajd� wszystkie obiekty posiadaj�ce PlaySoundOnObject i dodaj do listy
        playSoundObjects.AddRange(Object.FindObjectsByType<PlaySoundOnObject>(FindObjectsSortMode.None));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnBackToStartMenu();

            // D�wi�k wyj�cia z menu (opcjonalnie)
            foreach (var playSoundOnObject in playSoundObjects)
            {
                if (playSoundOnObject == null) continue;
                playSoundOnObject.PlaySound("MenuExit", 0.4f, false);
            }
        }
    }

    // Klikni�cie slotu
    public void OnSlotSelected(int index)
    {
        selectedSlotIndex = index;
        SaveManager.Instance.SetCurrentSlot(index);
        UpdateButtonStates();
    }

    // Klikni�cie "Nowa Gra"
    public void OnStartNewGame()
    {
        if (selectedSlotIndex < 0) return;

        SaveManager.Instance.ResetSaveSlot(selectedSlotIndex);
        SaveManager.Instance.ResetCurrency(); // opcjonalnie zresetuj walut� itp.
        startMenuUI.SetActive(false);
        SceneChanger.lastRelativePlayerPos = SceneChanger.defaultRelativePlayerPos;
        SceneManager.LoadScene("Main"); // <-- Zmie� na swoj� scen� gry
    }

    // Klikni�cie "Wr��"
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
        // Pod�wietl wybrany slot
        for (int i = 0; i < slotButtons.Length; i++)
        {
            var colors = slotButtons[i].colors;
            colors.normalColor = (i == selectedSlotIndex) ? Color.yellow : Color.white;
            slotButtons[i].colors = colors;
        }

        // Aktywuj przycisk Nowa Gra tylko je�li wybrano slot
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
