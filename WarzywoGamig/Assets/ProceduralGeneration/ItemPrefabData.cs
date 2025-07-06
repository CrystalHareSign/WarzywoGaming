using UnityEngine;

public enum ItemEffectType { None, Heal, Boost, Key, Custom }

[CreateAssetMenu(menuName = "Procedural/ItemPrefabData")]
public class ItemPrefabData : ScriptableObject
{
    public GameObject prefab;
    public string itemName;

    [Header("Efekt dzia³ania")]
    public ItemEffectType effectType = ItemEffectType.None;

    [Header("Wartoœæ efektu (np. ile leczy, ile daje boosta)")]
    public int effectValue = 0;

    [Header("Czas trwania efektu (w sekundach, dla efektów czasowych)")]
    public float effectDuration = 0f;

    [Header("Custom effect (dla niestandardowych zachowañ)")]
    public string customEffectID; // np. "Teleport", "SummonBoss", itp.
}