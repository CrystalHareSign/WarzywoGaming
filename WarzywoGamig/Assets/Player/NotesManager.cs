using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class NotesManagerUI : MonoBehaviour
{
    [System.Serializable]
    public class LocalizedNoteText
    {
        // Tytu³y do wpisania – zostaw bez [TextArea]
        public string english;
        public string polish;
        public string german;
    }

    [System.Serializable]
    public class LocalizedNoteDescription
    {
        // OPISY – tu dajemy [TextArea], ¿eby by³y DU¯E OKNA w Inspectorze!
        [TextArea] public string english;
        [TextArea] public string polish;
        [TextArea] public string german;
    }

    [System.Serializable]
    public class NoteEntry
    {
        public string Id;
        public LocalizedNoteText Title;
        public LocalizedNoteDescription Description;
        public Sprite Image;
        public bool IsUnlocked;
        public bool IsRead;
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

    [Header("Lampka przy zak³adce NOTES (dodaj Image z Hierarchii)")]
    public Image notesTabIndicator;

    void Start()
    {
        for (int i = 0; i < noteButtons.Count && i < notes.Count; i++)
        {
            int index = i;
            noteButtons[index].onClick.AddListener(() => ShowNote(index));
        }
        UpdateButtons();
        UpdateNoteIndicators();
        UpdateInventoryIndicator();
        UpdateNotesTabIndicator();
        ClearNoteDisplay();
    }

    public string GetNoteTitle(NoteEntry entry)
    {
        switch (LanguageManager.Instance.currentLanguage)
        {
            case LanguageManager.Language.Polski: return entry.Title.polish;
            case LanguageManager.Language.Deutsch: return entry.Title.german;
            default: return entry.Title.english;
        }
    }

    public string GetNoteDescription(NoteEntry entry)
    {
        switch (LanguageManager.Instance.currentLanguage)
        {
            case LanguageManager.Language.Polski: return entry.Description.polish;
            case LanguageManager.Language.Deutsch: return entry.Description.german;
            default: return entry.Description.english;
        }
    }

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
            noteDescriptionText.text = GetNoteDescription(notes[index]);
        }
        if (!notes[index].IsRead)
        {
            notes[index].IsRead = true;
            UpdateNoteIndicators();
            UpdateInventoryIndicator();
            UpdateNotesTabIndicator();
        }
    }

    public void UpdateButtons()
    {
        for (int i = 0; i < noteButtons.Count && i < notes.Count; i++)
        {
            noteButtons[i].gameObject.SetActive(notes[i].IsUnlocked);
            var txt = noteButtons[i].GetComponentInChildren<TMP_Text>();
            if (txt != null)
                txt.text = GetNoteTitle(notes[i]);
        }
    }

    public void UpdateNoteIndicators()
    {
        for (int i = 0; i < noteIndicators.Count && i < notes.Count; i++)
        {
            noteIndicators[i].enabled = notes[i].IsUnlocked && !notes[i].IsRead;
        }
    }

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

    public void UpdateNotesTabIndicator()
    {
        if (notesTabIndicator == null) return;
        bool anyUnread = false;
        for (int i = 0; i < notes.Count; i++)
        {
            if (notes[i].IsUnlocked && !notes[i].IsRead)
            {
                anyUnread = true;
                break;
            }
        }
        notesTabIndicator.enabled = anyUnread;
    }

    public void UnlockNote(string id)
    {
        for (int i = 0; i < notes.Count; i++)
        {
            if (notes[i].Id == id)
            {
                notes[i].IsUnlocked = true;
                notes[i].IsRead = false;
                UpdateButtons();
                UpdateNoteIndicators();
                UpdateInventoryIndicator();
                UpdateNotesTabIndicator();
                break;
            }
        }
    }

    public void ClearNoteDisplay()
    {
        if (noteImage != null)
        {
            noteImage.sprite = null;
            noteImage.enabled = false;
        }
        if (noteDescriptionText != null)
        {
            noteDescriptionText.text = "";
        }
    }

    public void ShowNotesTab()
    {
        UpdateButtons();
        UpdateNoteIndicators();
        UpdateInventoryIndicator();
        UpdateNotesTabIndicator();
        ClearNoteDisplay();
    }
}

// np. w skrypcie InventoryUI, po zakoñczeniu tutoriala strzelania:
//         notesManagerUI.UnlockNote("strzelanie");