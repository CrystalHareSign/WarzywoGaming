using UnityEngine;
using System.Collections;

public class ItemEffectManager : MonoBehaviour
{
    public static ItemEffectManager Instance;

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

    public bool UseItem(ItemPrefabData data, GameObject itemObj)
    {
        Debug.Log($"[ItemEffectManager] Próba u¿ycia przedmiotu: {data.itemName}, typ: {data.effectType}, value: {data.effectValue}, duration: {data.effectDuration}");

        switch (data.effectType)
        {
            case ItemEffectType.Heal:
                if (PlayerStats.Instance != null && PlayerStats.Instance.currentHealth >= PlayerStats.Instance.maxHealth)
                {
                    var hoverMsg = itemObj.GetComponent<HoverMessage>();
                    string popupText = hoverMsg ? hoverMsg.infoMessage : "Max health!"; // fallback
                    HoverMessageManager.Instance.ShowInfoPopup(popupText, 22, 2.5f);
                    Debug.Log("[ItemEffectManager] Leczenie zablokowane: pe³ne zdrowie!");
                    return false; // NIE zu¿ywaj itemu!
                }
                PlayerStats.Instance.Heal(data.effectValue);
                Debug.Log($"[ItemEffectManager] Heal: {data.effectValue}");
                return true;
            case ItemEffectType.StaminaBoost:
                PlayerStats.Instance.UseEnergyDrink(data.effectValue, data.effectDuration);
                Debug.Log($"[ItemEffectManager] Boost: +{data.effectValue} bonus stamina na {data.effectDuration} s");
                return true;
            case ItemEffectType.Key:
                Debug.Log($"[ItemEffectManager] U¿yto klucza: {data.itemName}");
                return true;
            case ItemEffectType.Currency:
                SaveManager.Instance.AddCurrency(data.effectValue);
                Debug.Log($"[ItemEffectManager] Dodano {data.effectValue} waluty graczowi!");
                return true;
            case ItemEffectType.Ammunition:
                Inventory.Instance.AddAmmo(data.ammoType, data.effectValue);
                return true;
            case ItemEffectType.Custom:
                var interactable = itemObj.GetComponent<InteractableItem>();
                if (interactable != null)
                {
                    Debug.Log($"[ItemEffectManager] Custom effect: {data.customEffectID}");
                    interactable.UseCustomEffect(data.customEffectID);
                }
                return true;
            default:
                return true;
        }
    }
}