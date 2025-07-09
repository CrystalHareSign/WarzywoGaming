using UnityEngine;
public enum LootCategory { Currency, Ammunition, Meds, StaminaBooster, key, Special }

public enum ItemEffectType { Currency, Ammunition, Heal, StaminaBoost, Key, Custom, None}

public enum LootRarity { Common = 1, Uncommon = 2, Rare = 3, Epic = 4, Legendary = 5 }

[CreateAssetMenu(menuName = "Procedural/ItemPrefabData")]
public class ItemPrefabData : ScriptableObject
{
    public GameObject prefab;
    public string itemName;

    [Header("Rzadkoœæ lootu (Common, Uncommon, Rare, Epic, Legendary)")]
    [Range(1, 5)]
    public int lootRarity = 1;

    [Header("Kategoria lootu")]
    public LootCategory lootCategory;

    [Header("Efekt dzia³ania")]
    public ItemEffectType effectType = ItemEffectType.None;

    [Header("Wartoœæ efektu (np. ile leczy, ile daje boosta)")]
    public int effectValue = 0;

    [Header("Czas trwania efektu (w sekundach, dla efektów czasowych)")]
    public float effectDuration = 0f;

    [Header("Czy automatycznie u¿yæ po podniesieniu (nie trafia do ekwipunku)?")]
    public bool autoUseOnPickup = false;

    [Header("Custom effect (dla niestandardowych zachowañ)")]
    public string customEffectID; // np. "Teleport", "SummonBoss", itp.
}


