using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class NotesManagerUI : MonoBehaviour
{
    [System.Serializable]
    public class NoteEntry
    {
        public string Id;
        public string Title;
        [TextArea] public string Description;
        public Sprite Image;
        public bool IsUnlocked;
        public bool IsRead; // Czy notatka zosta³a przeczytana
    }

    [Header("Przyciski notatek w UI (dodaj rêcznie z Hierarchii)")]
    public List<Button> noteButtons = new List<Button>();

    [Header("Lampki przy nieprzeczytanych notatkach (dodaj Image z Hierarchii)")]
    public List<Image> noteIndicators = new List<Image>();

    [Header("Dane notatek (parowane po indeksie z przyciskami)")]
    public List<NoteEntry> notes = new List<NoteEntry>();

    [Header("UI do opisu i obrazka")]
    public Image noteImage;
    public TMP_Text noteDescriptionText;

    [Header("Lampka na HUDzie gracza (dodaj Image z Canvasu gracza)")]
    public Image inventoryIndicator;

    void Start()
    {
        // Podpinamy obs³ugê klikniêæ do przycisków
        for (int i = 0; i < noteButtons.Count && i < notes.Count; i++)
        {
            int index = i; // wa¿ne!
            noteButtons[index].onClick.AddListener(() => ShowNote(index));
        }
        UpdateButtons();
        UpdateNoteIndicators();
        UpdateInventoryIndicator();
        ClearNoteDisplay(); // Po starcie nie pokazuj ¿adnej notatki
    }

    // Pokazuje opis i obrazek dla notatki o danym indeksie, oraz odhacza jako przeczytan¹
    public void ShowNote(int index)
    {
        if (index < 0 || index >= notes.Count) return;
        if (noteImage != null)
        {
            noteImage.enabled = true;
            noteImage.sprite = notes[index].Image;
        }
        if (noteDescriptionText != null)
        {
            noteDescriptionText.text = notes[index].Description;
        }
        // Odhacz jako przeczytan¹, jeœli nie by³a
        if (!notes[index].IsRead)
        {
            notes[index].IsRead = true;
            UpdateNoteIndicators();
            UpdateInventoryIndicator();
        }
    }

    // Pokazuje lub ukrywa przyciski na podstawie IsUnlocked
    public void UpdateButtons()
    {
        for (int i = 0; i < noteButtons.Count && i < notes.Count; i++)
        {
            noteButtons[i].gameObject.SetActive(notes[i].IsUnlocked);
        }
    }

    // Pokazuje lub ukrywa lampki przy nieprzeczytanych notatkach
    public void UpdateNoteIndicators()
    {
        for (int i = 0; i < noteIndicators.Count && i < notes.Count; i++)
        {
            // Lampka widoczna tylko jeœli notatka jest odblokowana i nieprzeczytana
            noteIndicators[i].enabled = notes[i].IsUnlocked && !notes[i].IsRead;
        }
    }

    // Pokazuje/ukrywa lampkê na HUDzie gracza jeœli s¹ nieprzeczytane notatki
    public void UpdateInventoryIndicator()
    {
        if (inventoryIndicator == null) return;
        bool anyUnread = false;
        for (int i = 0; i < notes.Count; i++)
        {
            if (notes[i].IsUnlocked && !notes[i].IsRead)
            {
                anyUnread = true;
                break;
            }
        }
        inventoryIndicator.enabled = anyUnread;
    }

    // Odblokowanie notatki po Id
    public void UnlockNote(string id)
    {
        for (int i = 0; i < notes.Count; i++)
        {
            if (notes[i].Id == id)
            {
                notes[i].IsUnlocked = true;
                // Po odblokowaniu notatka jest nieprzeczytana!
                notes[i].IsRead = false;
                UpdateButtons();
                UpdateNoteIndicators();
                UpdateInventoryIndicator();
                break;
            }
        }
    }

    // Czyszczenie wyœwietlania notatki (nic nie wybrane)
    public void ClearNoteDisplay()
    {
        if (noteImage != null)
        {
            noteImage.sprite = null;
            noteImage.enabled = false; // opcjonalnie, jeœli chcesz ukryæ ca³kowicie obrazek
        }
        if (noteDescriptionText != null)
        {
            noteDescriptionText.text = "";
        }
    }

    // Wywo³aj to z InventoryUI po prze³¹czeniu zak³adki
    public void ShowNotesTab()
    {
        UpdateButtons();
        UpdateNoteIndicators();
        UpdateInventoryIndicator();
        ClearNoteDisplay(); // Po wejœciu w Notes nic nie wybrane
    }
}

// np. w skrypcie InventoryUI, po zakoñczeniu tutoriala strzelania:
//         notesManagerUI.UnlockNote("strzelanie");