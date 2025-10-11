using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;

/// <summary>
/// Example implementation showing how to integrate FishNet networking with the Inventory system.
/// This script demonstrates best practices for networked inventory management.
/// 
/// IMPORTANT: This is an EXAMPLE file. You can use this as a reference to update your existing
/// Inventory.cs script, or use it as-is by replacing the old Inventory component.
/// </summary>
public class NetworkedInventoryExample : NetworkBehaviour
{
    [Header("Inventory Configuration")]
    public int maxWeapons = 3;
    public int maxItems = 5;
    public int maxLoot = 5;
    public float dropHeight = 1f;
    public LayerMask interactableLayer;

    [Header("References")]
    public Transform weaponParent;
    public Transform itemParent;
    public Transform lootParent;
    public InventoryUI inventoryUI;

    // Synchronized lists for multiplayer
    [SyncObject]
    private readonly SyncList<string> syncedWeapons = new SyncList<string>();

    // Local lists (not directly synced, managed via RPCs)
    private List<GameObject> items = new List<GameObject>();
    private List<GameObject> loot = new List<GameObject>();

    public GameObject currentWeaponPrefab;
    public string currentWeaponName = null;

    private void Awake()
    {
        // Initialize sync callbacks
        syncedWeapons.OnChange += OnWeaponsChanged;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        // Update UI when joining as client
        if (!base.IsServerStarted)
        {
            UpdateInventoryUI();
        }
    }

    #region Weapon Management

    /// <summary>
    /// Add a weapon to inventory. Call this on the client.
    /// </summary>
    public void AddWeapon(string weaponName)
    {
        if (!base.IsOwner) return;

        if (syncedWeapons.Count >= maxWeapons)
        {
            Debug.LogWarning("Weapon inventory is full!");
            return;
        }

        ServerAddWeaponRpc(weaponName);
    }

    [ServerRpc]
    private void ServerAddWeaponRpc(string weaponName)
    {
        if (syncedWeapons.Count < maxWeapons && !syncedWeapons.Contains(weaponName))
        {
            syncedWeapons.Add(weaponName);
        }
    }

    /// <summary>
    /// Remove a weapon from inventory.
    /// </summary>
    public void RemoveWeapon(string weaponName)
    {
        if (!base.IsOwner) return;
        ServerRemoveWeaponRpc(weaponName);
    }

    [ServerRpc]
    private void ServerRemoveWeaponRpc(string weaponName)
    {
        syncedWeapons.Remove(weaponName);
    }

    /// <summary>
    /// Equip a weapon by name.
    /// </summary>
    public void EquipWeapon(string weaponName)
    {
        if (!base.IsOwner) return;

        if (!syncedWeapons.Contains(weaponName))
        {
            Debug.LogWarning($"Weapon {weaponName} not in inventory!");
            return;
        }

        ServerEquipWeaponRpc(weaponName);
    }

    [ServerRpc]
    private void ServerEquipWeaponRpc(string weaponName)
    {
        // Despawn current weapon if any
        if (currentWeaponPrefab != null)
        {
            Destroy(currentWeaponPrefab);
        }

        // Spawn new weapon (this would need to load from WeaponDatabase)
        // For now, this is a placeholder
        currentWeaponName = weaponName;
        
        // Notify all clients to update their visual
        ObserversEquipWeapon(weaponName);
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversEquipWeapon(string weaponName)
    {
        // Update weapon visual for all clients
        // Load weapon prefab from database
        // Instantiate and parent to weaponParent
        currentWeaponName = weaponName;
        UpdateInventoryUI();
    }

    #endregion

    #region Item Management

    /// <summary>
    /// Add an item to inventory using networked pickup.
    /// </summary>
    public void AddItem(GameObject itemObject)
    {
        if (!base.IsOwner) return;

        if (items.Count >= maxItems)
        {
            Debug.LogWarning("Item inventory is full!");
            return;
        }

        // Use PickupItemPhysics for networked pickup
        PickupItemPhysics itemPhysics = itemObject.GetComponent<PickupItemPhysics>();
        if (itemPhysics != null && itemParent != null)
        {
            itemPhysics.RequestAttach(itemParent, useJoint: false);
        }

        // Add to local list and notify server
        items.Add(itemObject);
        ServerAddItemRpc(itemObject);
    }

    [ServerRpc]
    private void ServerAddItemRpc(GameObject itemObject)
    {
        // Server validates and broadcasts to observers
        ObserversAddItem(itemObject);
    }

    [ObserversRpc(BufferLast = true, ExcludeOwner = true)]
    private void ObserversAddItem(GameObject itemObject)
    {
        // Other clients add to their local list
        if (!items.Contains(itemObject))
        {
            items.Add(itemObject);
        }
        UpdateInventoryUI();
    }

    /// <summary>
    /// Remove and drop an item.
    /// </summary>
    public void DropItem(GameObject itemObject)
    {
        if (!base.IsOwner) return;

        if (!items.Contains(itemObject)) return;

        Vector3 dropPosition = transform.position;
        dropPosition.y = dropHeight;

        // Use PickupItemPhysics for networked drop
        PickupItemPhysics itemPhysics = itemObject.GetComponent<PickupItemPhysics>();
        if (itemPhysics != null)
        {
            itemPhysics.RequestDetach();
            itemPhysics.RequestDrop(dropPosition, Quaternion.identity);
        }

        items.Remove(itemObject);
        ServerDropItemRpc(itemObject, dropPosition);
    }

    [ServerRpc]
    private void ServerDropItemRpc(GameObject itemObject, Vector3 dropPosition)
    {
        ObserversDropItem(itemObject, dropPosition);
    }

    [ObserversRpc(ExcludeOwner = true)]
    private void ObserversDropItem(GameObject itemObject, Vector3 dropPosition)
    {
        items.Remove(itemObject);
        UpdateInventoryUI();
    }

    #endregion

    #region Loot Management

    /// <summary>
    /// Add loot to inventory.
    /// </summary>
    public void AddLoot(GameObject lootObject)
    {
        if (!base.IsOwner) return;

        if (loot.Count >= maxLoot)
        {
            Debug.LogWarning("Loot inventory is full!");
            return;
        }

        // Use PickupItemPhysics for networked pickup
        PickupItemPhysics lootPhysics = lootObject.GetComponent<PickupItemPhysics>();
        if (lootPhysics != null && lootParent != null)
        {
            lootPhysics.RequestPickup(lootParent);
        }

        loot.Add(lootObject);
        ServerAddLootRpc(lootObject);
    }

    [ServerRpc]
    private void ServerAddLootRpc(GameObject lootObject)
    {
        ObserversAddLoot(lootObject);
    }

    [ObserversRpc(BufferLast = true, ExcludeOwner = true)]
    private void ObserversAddLoot(GameObject lootObject)
    {
        if (!loot.Contains(lootObject))
        {
            loot.Add(lootObject);
        }
        UpdateInventoryUI();
    }

    /// <summary>
    /// Equip loot in hand.
    /// </summary>
    public void EquipLoot(GameObject lootObject)
    {
        if (!base.IsOwner) return;

        PickupItemPhysics lootPhysics = lootObject.GetComponent<PickupItemPhysics>();
        if (lootPhysics != null && lootParent != null)
        {
            lootPhysics.RequestPickup(lootParent);
            
            // Disable physics when held
            Rigidbody rb = lootObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }

        ServerEquipLootRpc(lootObject);
    }

    [ServerRpc]
    private void ServerEquipLootRpc(GameObject lootObject)
    {
        ObserversEquipLoot(lootObject);
    }

    [ObserversRpc(ExcludeOwner = true)]
    private void ObserversEquipLoot(GameObject lootObject)
    {
        // Update visual for other clients
        UpdateInventoryUI();
    }

    #endregion

    #region UI Updates

    private void OnWeaponsChanged(SyncListOperation op, int index, string oldItem, string newItem, bool asServer)
    {
        // Update UI whenever weapons list changes
        UpdateInventoryUI();
    }

    private void UpdateInventoryUI()
    {
        if (inventoryUI == null) return;

        // Convert syncedWeapons to List<string> for UI
        List<string> weaponList = new List<string>();
        foreach (string weapon in syncedWeapons)
        {
            weaponList.Add(weapon);
        }

        inventoryUI.UpdateInventoryUI(weaponList, items, currentWeaponName);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Get the current weapon list (for external access).
    /// </summary>
    public List<string> GetWeapons()
    {
        List<string> weaponList = new List<string>();
        foreach (string weapon in syncedWeapons)
        {
            weaponList.Add(weapon);
        }
        return weaponList;
    }

    /// <summary>
    /// Get the current items list.
    /// </summary>
    public List<GameObject> GetItems()
    {
        return new List<GameObject>(items);
    }

    /// <summary>
    /// Get the current loot list.
    /// </summary>
    public List<GameObject> GetLoot()
    {
        return new List<GameObject>(loot);
    }

    #endregion
}
