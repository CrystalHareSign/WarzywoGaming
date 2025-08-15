using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class NotesManagerUI : MonoBehaviour
{
    [System.Serializable]
    public class LocalizedNoteText
    {
        public string english;
        public string polish;
        public string german;
    }

    [System.Serializable]
    public class NoteEntry
    {
        public string Id;
        public LocalizedNoteText Title;
        public LocalizedNoteText Description;
        public Sprite Image;
        public bool IsUnlocked;
        public bool IsRead;
    }

    // ---- NOWE DLA INFO ----
    [System.Serializable]
    public class InfoEntry
    {
        public string Id;
        public LocalizedNoteText Title;
        public LocalizedNoteText Description;
        public Sprite Image;
        public bool IsUnlocked;
        public bool IsRead;
    }

    // --- UI dla zak³adki Notes ---
    [Header("Przyciski notatek w UI (dodaj rêcznie z Hierarchii)")]
    public List<Button> noteButtons = new List<Button>();

    [Header("Lampki przy nieprzeczytanych notatkach (dodaj Image z Hierarchii)")]
    public List<Image> noteIndicators = new List<Image>();

    [Header("Dane notatek (parowane po indeksie z przyciskami)")]
    public List<NoteEntry> notes = new List<NoteEntry>();

    [Header("Lampka przy zak³adce NOTES (dodaj Image z Hierarchii)")]
    public Image notesTabIndicator;

    // --- UI dla zak³adki Info ---
    [Header("Przyciski info w UI (dodaj rêcznie z Hierarchii)")]
    public List<Button> infoButtons = new List<Button>();

    [Header("Lampki przy nieprzeczytanych info (dodaj Image z Hierarchii)")]
    public List<Image> infoIndicators = new List<Image>();

    [Header("Dane info (parowane po indeksie z przyciskami)")]
    public List<InfoEntry> infos = new List<InfoEntry>();

    [Header("Lampka przy zak³adce INFO (dodaj Image z Hierarchii)")]
    public Image infoTabIndicator;

    // --- UI do opisu i obrazka (OSOBNE dla ka¿dej zak³adki) ---
    [Header("UI do opisu i obrazka dla NOTES")]
    public Image noteImage;
    public TMP_Text noteDescriptionText;

    [Header("UI do opisu i obrazka dla INFO")]
    public Image infoImage;
    public TMP_Text infoDescriptionText;

    // --- Lampka globalna na HUDzie gracza ---
    [Header("Lampka na HUDzie gracza (dodaj Image z Canvasu gracza)")]
    public Image inventoryIndicator;

    // --- Tryb zak³adki (1 = Notes, 2 = Info) ---
    private enum TabType { None, Notes, Info }
    private TabType currentTab = TabType.None;

    void Start()
    {
        // Notes
        for (int i = 0; i < noteButtons.Count && i < notes.Count; i++)
        {
            int index = i;
            noteButtons[index].onClick.AddListener(() => ShowNote(index));
        }

        // Info
        for (int i = 0; i < infoButtons.Count && i < infos.Count; i++)
        {
            int index = i;
            infoButtons[index].onClick.AddListener(() => ShowInfo(index));
        }

        UpdateButtons();
        UpdateInfoButtons();
        UpdateNoteIndicators();
        UpdateInfoIndicators();
        UpdateInventoryIndicator();
        UpdateNotesTabIndicator();
        UpdateInfoTabIndicator();
        ClearNoteDisplay();
    }

    // -- Pobieranie tekstów w odpowiednim jêzyku --
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
    public string GetInfoTitle(InfoEntry entry)
    {
        switch (LanguageManager.Instance.currentLanguage)
        {
            case LanguageManager.Language.Polski: return entry.Title.polish;
            case LanguageManager.Language.Deutsch: return entry.Title.german;
            default: return entry.Title.english;
        }
    }
    public string GetInfoDescription(InfoEntry entry)
    {
        switch (LanguageManager.Instance.currentLanguage)
        {
            case LanguageManager.Language.Polski: return entry.Description.polish;
            case LanguageManager.Language.Deutsch: return entry.Description.german;
            default: return entry.Description.english;
        }
    }

    // --- Pokazywanie notatki ---
    public void ShowNote(int index)
    {
        if (index < 0 || index >= notes.Count) return;
        currentTab = TabType.Notes;

        // Poka¿ tylko UI dla NOTES, ukryj dla INFO
        if (noteImage != null)
        {
            noteImage.enabled = true;
            noteImage.sprite = notes[index].Image;
        }
        if (noteDescriptionText != null)
        {
            noteDescriptionText.text = GetNoteDescription(notes[index]);
        }
        if (infoImage != null)
        {
            infoImage.enabled = false;
            infoImage.sprite = null;
        }
        if (infoDescriptionText != null)
        {
            infoDescriptionText.text = "";
        }

        // Odhacz jako przeczytan¹
        if (!notes[index].IsRead)
        {
            notes[index].IsRead = true;
            UpdateNoteIndicators();
            UpdateInventoryIndicator();
            UpdateNotesTabIndicator();
        }
    }

    // --- Pokazywanie info ---
    public void ShowInfo(int index)
    {
        if (index < 0 || index >= infos.Count) return;
        currentTab = TabType.Info;

        // Poka¿ tylko UI dla INFO, ukryj dla NOTES
        if (infoImage != null)
        {
            infoImage.enabled = true;
            infoImage.sprite = infos[index].Image;
        }
        if (infoDescriptionText != null)
        {
            infoDescriptionText.text = GetInfoDescription(infos[index]);
        }
        if (noteImage != null)
        {
            noteImage.enabled = false;
            noteImage.sprite = null;
        }
        if (noteDescriptionText != null)
        {
            noteDescriptionText.text = "";
        }

        // Odhacz jako przeczytan¹
        if (!infos[index].IsRead)
        {
            infos[index].IsRead = true;
            UpdateInfoIndicators();
            UpdateInventoryIndicator();
            UpdateInfoTabIndicator();
        }
    }

    // --- Aktualizacja przycisków ---
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
    public void UpdateInfoButtons()
    {
        for (int i = 0; i < infoButtons.Count && i < infos.Count; i++)
        {
            infoButtons[i].gameObject.SetActive(infos[i].IsUnlocked);
            var txt = infoButtons[i].GetComponentInChildren<TMP_Text>();
            if (txt != null)
                txt.text = GetInfoTitle(infos[i]);
        }
    }

    // --- Kropki przy nieprzeczytanych ---
    public void UpdateNoteIndicators()
    {
        for (int i = 0; i < noteIndicators.Count && i < notes.Count; i++)
        {
            noteIndicators[i].enabled = notes[i].IsUnlocked && !notes[i].IsRead;
        }
    }
    public void UpdateInfoIndicators()
    {
        for (int i = 0; i < infoIndicators.Count && i < infos.Count; i++)
        {
            infoIndicators[i].enabled = infos[i].IsUnlocked && !infos[i].IsRead;
        }
    }

    // --- Lampka globalna na HUDzie gracza ---
    public void UpdateInventoryIndicator()
    {
        if (inventoryIndicator == null) return;
        bool anyUnread = false;
        // Notes
        for (int i = 0; i < notes.Count; i++)
        {
            if (notes[i].IsUnlocked && !notes[i].IsRead)
            {
                anyUnread = true;
                break;
            }
        }
        // Info
        if (!anyUnread)
        {
            for (int i = 0; i < infos.Count; i++)
            {
                if (infos[i].IsUnlocked && !infos[i].IsRead)
                {
                    anyUnread = true;
                    break;
                }
            }
        }
        inventoryIndicator.enabled = anyUnread;
    }

    // --- Lampka nad zak³adk¹ Notes ---
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
    // --- Lampka nad zak³adk¹ Info ---
    public void UpdateInfoTabIndicator()
    {
        if (infoTabIndicator == null) return;
        bool anyUnread = false;
        for (int i = 0; i < infos.Count; i++)
        {
            if (infos[i].IsUnlocked && !infos[i].IsRead)
            {
                anyUnread = true;
                break;
            }
        }
        infoTabIndicator.enabled = anyUnread;
    }

    // --- Odblokowanie notatki po Id ---
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
    // --- Odblokowanie info po Id ---
    public void UnlockInfo(string id)
    {
        for (int i = 0; i < infos.Count; i++)
        {
            if (infos[i].Id == id)
            {
                infos[i].IsUnlocked = true;
                infos[i].IsRead = false;
                UpdateInfoButtons();
                UpdateInfoIndicators();
                UpdateInventoryIndicator();
                UpdateInfoTabIndicator();
                break;
            }
        }
    }

    // --- Czyszczenie wyœwietlania ---
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
        if (infoImage != null)
        {
            infoImage.sprite = null;
            infoImage.enabled = false;
        }
        if (infoDescriptionText != null)
        {
            infoDescriptionText.text = "";
        }
    }

    // --- Prze³¹czanie zak³adki Notes ---
    public void ShowNotesTab()
    {
        currentTab = TabType.Notes;
        UpdateButtons();
        UpdateNoteIndicators();
        UpdateInventoryIndicator();
        UpdateNotesTabIndicator();
        ClearNoteDisplay();
    }
    // --- Prze³¹czanie zak³adki Info ---
    public void ShowInfoTab()
    {
        currentTab = TabType.Info;
        UpdateInfoButtons();
        UpdateInfoIndicators();
        UpdateInventoryIndicator();
        UpdateInfoTabIndicator();
        ClearNoteDisplay();
    }
}

// np. w skrypcie InventoryUI, po zakoñczeniu tutoriala strzelania:
//         notesManagerUI.UnlockNote("strzelanie");