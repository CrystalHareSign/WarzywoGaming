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

    public void UseItem(ItemPrefabData data, GameObject itemObj)
    {
        Debug.Log($"[ItemEffectManager] Próba u¿ycia przedmiotu: {data.itemName}, typ: {data.effectType}, value: {data.effectValue}, duration: {data.effectDuration}");

        switch (data.effectType)
        {
            case ItemEffectType.Heal:
                PlayerStats.Instance.Heal(data.effectValue);
                Debug.Log($"[ItemEffectManager] Heal: {data.effectValue}");
                break;
            case ItemEffectType.StaminaBoost:
                PlayerStats.Instance.UseEnergyDrink(data.effectValue, data.effectDuration);
                Debug.Log($"[ItemEffectManager] Boost: +{data.effectValue} bonus stamina na {data.effectDuration} s");
                break;
            case ItemEffectType.Key:
                Debug.Log($"[ItemEffectManager] U¿yto klucza: {data.itemName}");
                break;
            case ItemEffectType.Currency:
                SaveManager.Instance.AddCurrency(data.effectValue);
                Debug.Log($"[ItemEffectManager] Dodano {data.effectValue} waluty graczowi!");
                break;
            case ItemEffectType.Ammunition:
                Inventory.Instance.AddAmmo(data.ammoType, data.effectValue);
                break;

            case ItemEffectType.Custom:
                var interactable = itemObj.GetComponent<InteractableItem>();
                if (interactable != null)
                {
                    Debug.Log($"[ItemEffectManager] Custom effect: {data.customEffectID}");
                    interactable.UseCustomEffect(data.customEffectID);
                }
                break;
        }
    }
}