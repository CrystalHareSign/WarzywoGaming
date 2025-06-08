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

        foreach (var item in FindObjectsOfType<InteractableItem>())
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
        if (action.StartsWith("SetDialogueSlot:"))
        {
            SetOtherDialogueSlot(action);
            return;
        }

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
        var shop = FindFirstObjectByType<LootShop>();
        if (shop != null)
            shop.UpdatePlayerCurrencyUI();
    }
}