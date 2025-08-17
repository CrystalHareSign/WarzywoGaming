using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

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

    [System.Serializable]
    public class QuestGoal
    {
        public LocalizedNoteText Name;
        public LocalizedNoteText Description;
        public bool IsOptional;
        public bool IsCompleted;
    }

    [System.Serializable]
    public class QuestEntry
    {
        public string Id;
        public LocalizedNoteText Name;
        public List<QuestGoal> Goals = new List<QuestGoal>();
        public bool IsUnlocked;
        public bool IsRead;
        public bool IsCompleted => Goals.Count > 0 && Goals.FindAll(g => !g.IsOptional).TrueForAll(g => g.IsCompleted);
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

    // === QUESTS UI ===
    [Header("Przyciski questów w UI (dodaj rêcznie z Hierarchii)")]
    public List<Button> questButtons = new List<Button>();
    [Header("Dane questów (parowane po indeksie z przyciskami)")]
    public List<QuestEntry> quests = new List<QuestEntry>();
    [Header("Lampka przy zak³adce QUESTS (dodaj Image z Hierarchii)")]
    public Image questsTabIndicator;

    // --- LAMPKI PRZY KA¯DYM QUEST BUTTONIE ---
    [Header("Lampki nieodczytanych questów (dodaj Image z Hierarchii, po jednej dla ka¿dego questa)")]
    public List<Image> questUnreadIndicators = new List<Image>();
    [Header("Lampki ukoñczonych questów (dodaj Image z Hierarchii, po jednej dla ka¿dego questa)")]
    public List<Image> questCompletedIndicators = new List<Image>();

    // --- UI do opisu i obrazka (OSOBNE dla ka¿dej zak³adki) ---
    [Header("UI do opisu i obrazka dla NOTES")]
    public Image noteImage;
    public TMP_Text noteDescriptionText;
    [Header("UI do opisu i obrazka dla INFO")]
    public Image infoImage;
    public TMP_Text infoDescriptionText;

    // === QUESTS UI - szczegó³y questa ===
    [Header("UI pole nazwy questa (TMP_Text)")]
    public TMP_Text questNameText;

    [Header("UI scrollowany obszar z celami questa (przypisz rêcznie, bez prefabów!)")]
    public List<TMP_Text> questGoalNameTexts;
    public List<TMP_Text> questGoalDescTexts;

    // --- Lampka globalna na HUDzie gracza ---
    [Header("Lampka na HUDzie gracza (dodaj Image z Canvasu gracza)")]
    public Image inventoryIndicator;
    public Image inventoryInventory;

    [Header("Quest Notification UI")]
    public TMP_Text questNotificationText; // ju¿ masz!
    public Image questNotificationImage;   // dodaj to pole!

    private Coroutine notificationFadeCoroutine;

    [Header("Quest Notifications Texts")]
    public string newQuestTextPolish = "Nowe zadanie";
    public string newQuestTextEnglish = "New quest";
    public string newQuestTextGerman = "Neue Aufgabe";

    public string completedQuestTextPolish = "Zadanie ukoñczone";
    public string completedQuestTextEnglish = "Quest completed";
    public string completedQuestTextGerman = "Aufgabe abgeschlossen";

    private enum TabType { None, Notes, Info, Quests }
    private TabType currentTab = TabType.None;

    public static NotesManagerUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
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
        // Quests
        for (int i = 0; i < questButtons.Count && i < quests.Count; i++)
        {
            int index = i;
            questButtons[index].onClick.AddListener(() => ShowQuest(index));
        }

        SortQuests();
        UpdateButtons();
        UpdateInfoButtons();
        UpdateNoteIndicators();
        UpdateInfoIndicators();
        UpdateInventoryIndicator();
        UpdateNotesTabIndicator();
        UpdateInfoTabIndicator();
        UpdateQuestButtons();
        UpdateQuestIndicators();
        UpdateQuestsTabIndicator();
        ClearNoteDisplay();
        ClearQuestDisplay();
    }

    // --- SORTOWANIE QUESTÓW: ukoñczone na koniec, potem przeczytane ---
    public void SortQuests()
    {
        quests.Sort((a, b) =>
        {
            // Najpierw nieukoñczone, potem ukoñczone
            if (a.IsCompleted != b.IsCompleted)
                return a.IsCompleted ? 1 : -1;
            // W grupie nieukoñczonych: najpierw nieprzeczytane, potem przeczytane
            if (!a.IsCompleted && !b.IsCompleted)
            {
                if (a.IsRead != b.IsRead)
                    return a.IsRead ? 1 : -1;
            }
            return 0;
        });
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
    public string GetQuestName(QuestEntry entry)
    {
        switch (LanguageManager.Instance.currentLanguage)
        {
            case LanguageManager.Language.Polski: return entry.Name.polish;
            case LanguageManager.Language.Deutsch: return entry.Name.german;
            default: return entry.Name.english;
        }
    }
    public string GetGoalName(QuestGoal goal)
    {
        switch (LanguageManager.Instance.currentLanguage)
        {
            case LanguageManager.Language.Polski: return goal.Name.polish;
            case LanguageManager.Language.Deutsch: return goal.Name.german;
            default: return goal.Name.english;
        }
    }
    public string GetGoalDescription(QuestGoal goal)
    {
        switch (LanguageManager.Instance.currentLanguage)
        {
            case LanguageManager.Language.Polski: return goal.Description.polish;
            case LanguageManager.Language.Deutsch: return goal.Description.german;
            default: return goal.Description.english;
        }
    }

    // --- Pokazywanie notatki ---
    public void ShowNote(int index)
    {
        if (index < 0 || index >= notes.Count) return;
        currentTab = TabType.Notes;

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
        ClearQuestDisplay();

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
        ClearQuestDisplay();

        if (!infos[index].IsRead)
        {
            infos[index].IsRead = true;
            UpdateInfoIndicators();
            UpdateInventoryIndicator();
            UpdateInfoTabIndicator();
        }
    }

    // --- Pokazywanie questa ---
    public void ShowQuest(int index)
    {
        if (index < 0 || index >= quests.Count) return;
        currentTab = TabType.Quests;

        if (noteImage != null) { noteImage.enabled = false; noteImage.sprite = null; }
        if (noteDescriptionText != null) { noteDescriptionText.text = ""; }
        if (infoImage != null) { infoImage.enabled = false; infoImage.sprite = null; }
        if (infoDescriptionText != null) { infoDescriptionText.text = ""; }

        if (questNameText != null)
            questNameText.text = GetQuestName(quests[index]);

        var goals = quests[index].Goals;
        for (int i = 0; i < questGoalNameTexts.Count; i++)
        {
            if (i < goals.Count)
            {
                questGoalNameTexts[i].gameObject.SetActive(true);
                questGoalNameTexts[i].text = GetGoalName(goals[i]);
                questGoalDescTexts[i].gameObject.SetActive(true);
                questGoalDescTexts[i].text = GetGoalDescription(goals[i]);
            }
            else
            {
                questGoalNameTexts[i].gameObject.SetActive(false);
                questGoalDescTexts[i].gameObject.SetActive(false);
            }
        }

        bool needSort = false;
        if (!quests[index].IsRead)
        {
            quests[index].IsRead = true;
            UpdateInventoryIndicator();
            needSort = true;
        }
        if (quests[index].IsCompleted)
        {
            needSort = true;
        }
        if (needSort)
        {
            SortQuests();
            UpdateQuestButtons();
        }
        UpdateQuestIndicators();
        UpdateQuestsTabIndicator();
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
    public void UpdateQuestButtons()
    {
        for (int i = 0; i < questButtons.Count && i < quests.Count; i++)
        {
            questButtons[i].gameObject.SetActive(quests[i].IsUnlocked);
            var txt = questButtons[i].GetComponentInChildren<TMP_Text>();
            if (txt != null)
                txt.text = GetQuestName(quests[i]);
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
    public void UpdateQuestIndicators()
    {
        for (int i = 0; i < quests.Count; i++)
        {
            var quest = quests[i];
            bool showCompleted = quest.IsUnlocked && quest.IsCompleted;
            bool showUnread = quest.IsUnlocked && !quest.IsRead && !quest.IsCompleted;
            if (i < questCompletedIndicators.Count && questCompletedIndicators[i] != null)
                questCompletedIndicators[i].enabled = showCompleted;
            if (i < questUnreadIndicators.Count && questUnreadIndicators[i] != null)
                questUnreadIndicators[i].enabled = showUnread;
        }
    }

    public void UpdateInventoryIndicator()
    {
        if (inventoryIndicator == null) return;
        bool anyUnread = false;
        // Notatki
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
        // QUESTY (nowe! dodaj to)
        if (!anyUnread)
        {
            for (int i = 0; i < quests.Count; i++)
            {
                if (quests[i].IsUnlocked && !quests[i].IsRead && !quests[i].IsCompleted)
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
    // --- Lampka nad zak³adk¹ Quests ---
    public void UpdateQuestsTabIndicator()
    {
        if (questsTabIndicator == null) return;
        bool anyUnreadActiveQuest = false;
        for (int i = 0; i < quests.Count; i++)
        {
            if (quests[i].IsUnlocked && !quests[i].IsRead && !quests[i].IsCompleted)
            {
                anyUnreadActiveQuest = true;
                break;
            }
        }
        questsTabIndicator.enabled = anyUnreadActiveQuest;
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

    public string GetQuestNotificationText(bool isCompleted)
    {
        switch (LanguageManager.Instance.currentLanguage)
        {
            case LanguageManager.Language.Polski:
                return isCompleted ? completedQuestTextPolish : newQuestTextPolish;
            case LanguageManager.Language.Deutsch:
                return isCompleted ? completedQuestTextGerman : newQuestTextGerman;
            default:
                return isCompleted ? completedQuestTextEnglish : newQuestTextEnglish;
        }
    }

    public void ShowQuestNotification(string message, float duration = 2f, float fadeDuration = 0.75f)
    {
        Debug.Log("ShowQuestNotification called with message: " + message);

        if (questNotificationText == null)
        {
            Debug.LogWarning("questNotificationText is null!");
            return;
        }
        if (questNotificationImage == null)
        {
            Debug.LogWarning("questNotificationImage is null!");
            return;
        }

        if (notificationFadeCoroutine != null)
        {
            Debug.Log("Stopping previous notificationFadeCoroutine.");
            StopCoroutine(notificationFadeCoroutine);
        }

        questNotificationText.text = message;
        questNotificationText.gameObject.SetActive(true);
        Debug.Log("questNotificationText set active.");

        // Aktywuj obrazek
        questNotificationImage.gameObject.SetActive(true);
        Debug.Log("questNotificationImage set active.");

        // Ustaw pe³n¹ widocznoœæ tekstu i obrazka
        var col = questNotificationText.color;
        col.a = 1f;
        questNotificationText.color = col;

        var imgCol = questNotificationImage.color;
        imgCol.a = 1f;
        questNotificationImage.color = imgCol;

        Debug.Log("Starting FadeOutQuestNotification coroutine.");
        notificationFadeCoroutine = StartCoroutine(FadeOutQuestNotification(fadeDuration, duration));
    }

    private IEnumerator FadeOutQuestNotification(float fadeDuration, float waitBeforeFade)
    {
        Debug.Log($"FadeOutQuestNotification will wait {waitBeforeFade} seconds before fading.");
        yield return new WaitForSeconds(waitBeforeFade);

        float elapsed = 0f;
        Color startTextColor = questNotificationText.color;
        Color startImageColor = questNotificationImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            // Fade tekstu
            var col = startTextColor;
            col.a = Mathf.Lerp(1f, 0f, t);
            questNotificationText.color = col;

            // Fade obrazka
            var imgCol = startImageColor;
            imgCol.a = Mathf.Lerp(1f, 0f, t);
            questNotificationImage.color = imgCol;

            yield return null;
        }
        Debug.Log("Fade out finished. Hiding questNotificationText and questNotificationImage.");
        questNotificationText.gameObject.SetActive(false);
        questNotificationImage.gameObject.SetActive(false);
    }

    public void UnlockQuest(string id)
    {
        for (int i = 0; i < quests.Count; i++)
        {
            if (quests[i].Id == id)
            {
                quests[i].IsUnlocked = true;
                quests[i].IsRead = false;
                SortQuests();
                UpdateQuestButtons();
                UpdateQuestIndicators();
                UpdateQuestsTabIndicator();
                UpdateInventoryIndicator();
                ShowQuestNotification(GetQuestNotificationText(false)); // <-- tylko tekst!
                break;
            }
        }
    }

    public void CompleteQuestById(string questId)
    {
        for (int i = 0; i < quests.Count; i++)
        {
            if (quests[i].Id == questId)
            {
                foreach (var goal in quests[i].Goals)
                    goal.IsCompleted = true;
                SortQuests();
                UpdateQuestButtons();
                UpdateQuestIndicators();
                UpdateQuestsTabIndicator();
                UpdateInventoryIndicator();
                ShowQuestNotification(GetQuestNotificationText(true)); // <-- tylko tekst!
                break;
            }
        }
    }

    // --- Metoda do obs³ugi ukoñczenia celu questa ---
    public void OnQuestGoalCompleted(int questIndex, int goalIndex)
    {
        if (questIndex < 0 || questIndex >= quests.Count) return;
        var quest = quests[questIndex];
        if (goalIndex < 0 || goalIndex >= quest.Goals.Count) return;

        quest.Goals[goalIndex].IsCompleted = true;
        SortQuests();
        UpdateQuestButtons();
        UpdateQuestIndicators();
        UpdateQuestsTabIndicator();
        UpdateInventoryIndicator();
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

    // --- Czyszczenie szczegó³ów questa ---
    public void ClearQuestDisplay()
    {
        if (questNameText != null)
            questNameText.text = "";
        for (int i = 0; i < questGoalNameTexts.Count; i++)
        {
            questGoalNameTexts[i].gameObject.SetActive(false);
            questGoalDescTexts[i].gameObject.SetActive(false);
        }
    }

    public void ShowNotesTab()
    {
        currentTab = TabType.Notes;
        UpdateButtons();
        UpdateNoteIndicators();
        UpdateInventoryIndicator();
        UpdateNotesTabIndicator();
        ClearNoteDisplay();
        ClearQuestDisplay();
    }
    public void ShowInfoTab()
    {
        currentTab = TabType.Info;
        UpdateInfoButtons();
        UpdateInfoIndicators();
        UpdateInventoryIndicator();
        UpdateInfoTabIndicator();
        ClearNoteDisplay();
        ClearQuestDisplay();
    }
    public void ShowQuestsTab()
    {
        currentTab = TabType.Quests;
        SortQuests();
        UpdateQuestButtons();
        UpdateQuestIndicators();
        UpdateInventoryIndicator();
        UpdateQuestsTabIndicator();
        ClearNoteDisplay();
        ClearQuestDisplay();
    }
}

// Przyk³ad u¿ycia:
// notesManagerUI.UnlockNote("strzelanie");
// notesManagerUI.UnlockInfo("strzelanie");
// notesManagerUI.UnlockQuest("quest_id");
// notesManagerUI.OnQuestGoalCompleted(questIndex, goalIndex);