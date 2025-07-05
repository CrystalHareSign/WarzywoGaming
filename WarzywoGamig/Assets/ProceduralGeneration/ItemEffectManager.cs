using UnityEngine;

public class ItemEffectManager : MonoBehaviour
{
    public static ItemEffectManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UseItem(ItemPrefabData data, GameObject itemObj)
    {
        switch (data.effectType)
        {
            case ItemEffectType.Heal:
                // Przyk³ad: PlayerStats.Instance.Heal(data.effectValue);
                Debug.Log("Heal: " + data.effectValue);
                break;
            case ItemEffectType.Boost:
                Debug.Log("Boost: " + data.effectValue);
                break;
            case ItemEffectType.Key:
                Debug.Log("U¿yto klucza: " + data.itemName);
                break;
            case ItemEffectType.Custom:
                // Przyk³ad: wywo³anie metody na InteractableItem
                var interactable = itemObj.GetComponent<InteractableItem>();
                if (interactable != null)
                    interactable.UseCustomEffect(data.customEffectID);
                break;
        }
    }
}