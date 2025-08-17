using UnityEngine;

public class DialogueActionHandler : MonoBehaviour
{
    public static DialogueActionHandler Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetOtherDialogueSlot(string action)
    {
        // Przyk³ad akcji: SetDialogueSlot:NazwaItemu,2
        string[] parts = action.Substring("SetDialogueSlot:".Length).Split(',');
        if (parts.Length < 2)
        {
            Debug.LogWarning("Z³y format akcji SetDialogueSlot!");
            return;
        }
        string itemName = parts[0];
        if (!int.TryParse(parts[1], out int slotIndex))
        {
            Debug.LogWarning("Nieprawid³owy index slotu dialogu!");
            return;
        }

        foreach (var item in Object.FindObjectsByType<InteractableItem>(FindObjectsSortMode.None))
        {
            if (item.itemName == itemName)
            {
                item.SetDialogueIndex(slotIndex);
                Debug.Log($"Ustawiono slot dialogu {slotIndex} dla {itemName}");
                return;
            }
        }
        Debug.LogWarning($"Nie znaleziono rozmówcy o nazwie itemu: {itemName}");
    }

    //================================================================================================================//

    public void HandleAction(string action, InteractableItem context)
    {
        if (string.IsNullOrEmpty(action)) return;

        if (action.StartsWith("SetDialogueSlot:"))
        {
            SetOtherDialogueSlot(action);
            return;
        }

        // --- QUEST HANDLING ---
        if (action.StartsWith("unlock_quest:"))
        {
            string questId = action.Substring("unlock_quest:".Length).Trim();
            var notesManager = NotesManagerUI.Instance ?? Object.FindFirstObjectByType<NotesManagerUI>();
            if (notesManager != null)
            {
                notesManager.UnlockQuest(questId);
                Debug.Log($"Odblokowano questa: {questId}");
            }
            else
            {
                Debug.LogWarning("[DialogueActionHandler] Brak NotesManagerUI! Nie mo¿na odblokowaæ questa.");
            }
            return;
        }

        if (action.StartsWith("complete_quest:"))
        {
            string questId = action.Substring("complete_quest:".Length).Trim();
            var notesManager = NotesManagerUI.Instance ?? Object.FindFirstObjectByType<NotesManagerUI>();
            if (notesManager != null)
            {
                // ZnajdŸ quest i oznacz jako ukoñczony
                var quest = notesManager.quests.Find(q => q.Id == questId);
                if (quest != null)
                {
                    foreach (var goal in quest.Goals)
                    {
                        goal.IsCompleted = true;
                    }
                    notesManager.SortQuests();
                    notesManager.UpdateQuestButtons();
                    notesManager.UpdateQuestIndicators();
                    notesManager.UpdateQuestsTabIndicator();
                    Debug.Log($"Ukoñczono questa: {questId}");
                }
                else
                {
                    Debug.LogWarning($"Quest o ID '{questId}' nie znaleziony!");
                }
            }
            else
            {
                Debug.LogWarning("[DialogueActionHandler] Brak NotesManagerUI! Nie mo¿na ukoñczyæ questa.");
            }
            return;
        }

        if (action.StartsWith("read_quest:"))
        {
            string questId = action.Substring("read_quest:".Length).Trim();
            var notesManager = NotesManagerUI.Instance ?? Object.FindFirstObjectByType<NotesManagerUI>();
            if (notesManager != null)
            {
                var quest = notesManager.quests.Find(q => q.Id == questId);
                if (quest != null)
                {
                    quest.IsRead = true;
                    notesManager.SortQuests();
                    notesManager.UpdateQuestButtons();
                    notesManager.UpdateQuestIndicators();
                    notesManager.UpdateQuestsTabIndicator();
                    Debug.Log($"Oznaczono questa jako przeczytany: {questId}");
                }
                else
                {
                    Debug.LogWarning($"Quest o ID '{questId}' nie znaleziony!");
                }
            }
            else
            {
                Debug.LogWarning("[DialogueActionHandler] Brak NotesManagerUI! Nie mo¿na oznaczyæ questa jako przeczytany.");
            }
            return;
        }

        // --- DODAJ W£ASNE AKCJE QUESTOWE WED£UG POTRZEB ---
        // if (action.StartsWith("...")) { ... }

        // --- PRZYK£ADOWE AKCJE ---
        switch (action)
        {
            case "TalkToGuard":
                TalkToGuard();
                break;
            default:
                Debug.LogWarning("Nieznana akcja dialogowa: " + action);
                break;
        }
    }

    //================================================================================================================//

    private void TalkToGuard()
    {
        Debug.Log("Dodano kaske");
        SaveManager.Instance.AddCurrency(1000f);
        var shop = Object.FindFirstObjectByType<LootShop>();
        if (shop != null)
            shop.UpdatePlayerCurrencyUI();

        NotesManagerUI.Instance.CompleteQuestById("Kibel");
    }
}