using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Inventory/WeaponDatabase")]
public class WeaponDatabase : ScriptableObject
{
    public List<WeaponPrefabEntry> weaponPrefabsList;
}