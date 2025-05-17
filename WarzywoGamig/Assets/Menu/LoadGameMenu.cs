using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LoadGameMenu : MonoBehaviour
{
    public TMP_Text[] slotTexts;              // Teksty przycisków slotów
    public Button[] slotButtons;              // Przycisk UI dla ka¿dego slotu
    public Button loadGameButton;             // Przycisk "Wczytaj grê"
    public Button backButton;             // Przycisk "Wróæ"
    public GameObject startMenuUI;            // Referencja do Start Menu UI
    public GameObject loadGameMenuUI;

    private int selectedSlotIndex = -1;

    // Lista wszystkich obiektów, które posiadaj¹ PlaySoundOnObject
    private List<PlaySoundOnObject> playSoundObjects = new List<PlaySoundOnObject>();

    private void Start()
    {
        selectedSlotIndex = -1;
        UpdateSlotTexts();
        UpdateButtonStates();

        playSoundObjects.AddRange(Object.FindObjectsByType<PlaySoundOnObject>(FindObjectsSortMode.None));
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

    private void OnEnable()
    {
        UpdateSlotTexts();
        UpdateButtonStates();
    }

    public void SelectSlot(int index)
    {
        if (SaveManager.Instance.DoesSlotExist(index))
        {
            selectedSlotIndex = index;
            SaveManager.Instance.SetCurrentSlot(index);
            UpdateButtonStates();
        }
        else
        {
            Debug.LogWarning("Brak zapisu w tym slocie.");
        }
    }

    public void OnLoadGameButton()
    {
        if (selectedSlotIndex < 0) return;

        SaveManager.Instance.LoadPlayerData(selectedSlotIndex);
        startMenuUI.SetActive(false);
    }

    public void OnBackToStartMenu()
    {
        if (loadGameMenuUI != null)
            loadGameMenuUI.SetActive(false);
        if (startMenuUI != null)
            startMenuUI.SetActive(true);
    }

    private void UpdateSlotTexts()
    {
        for (int i = 0; i < slotTexts.Length; i++)
        {
            if (slotTexts[i] == null) continue; // <--- zapobiega b³êdowi

            if (SaveManager.Instance != null && SaveManager.Instance.DoesSlotExist(i))
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
        for (int i = 0; i < slotButtons.Length; i++)
        {
            var colors = slotButtons[i].colors;
            colors.normalColor = (i == selectedSlotIndex) ? Color.yellow : Color.white;
            slotButtons[i].colors = colors;
        }

        loadGameButton.interactable = selectedSlotIndex >= 0;
    }

    // DŸwiêki (opcjonalnie podpinane pod eventy w Inspectorze)
    public void EnterButtonSound()
    {
        foreach (var obj in playSoundObjects)
        {
            if (obj == null) continue;
            obj.PlaySound("MenuEnter", 0.4f, false);
        }
    }

    public void ExitButtonSound()
    {
        foreach (var obj in playSoundObjects)
        {
            if (obj == null) continue;
            obj.PlaySound("MenuExit", 0.4f, false);
        }
    }

    public void HoverButtonSound()
    {
        foreach (var obj in playSoundObjects)
        {
            if (obj == null) continue;
            obj.PlaySound("MenuMouseOn", 0.8f, false);
        }
    }
}
