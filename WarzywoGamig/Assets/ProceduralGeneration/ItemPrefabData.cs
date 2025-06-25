using UnityEngine;

[CreateAssetMenu(menuName = "Procedural/ItemPrefabData")]
public class ItemPrefabData : ScriptableObject
{
    public GameObject prefab;
    public string itemName;
}