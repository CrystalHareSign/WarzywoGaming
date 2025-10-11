# Migration Guide: Adding FishNet Multiplayer Support

This guide explains how to integrate the new FishNet multiplayer system with your existing single-player code.

## Overview

The multiplayer system adds network synchronization to:
- Item pickup, drop, and physics
- Player interactions
- Inventory management
- Item attachments (hand, backpack)

## Step 1: Install FishNet

The FishNet package has been added to `Packages/manifest.json`. Unity will automatically download it when you open the project.

**Verification:**
1. Open Unity Editor
2. Wait for package resolution to complete
3. Check that `FishNet` folder appears in `Assets/Plugins/` or `Packages/`
4. Verify no console errors related to FishNet

## Step 2: Update Existing Scripts

### Option A: Gradual Migration (Recommended)

Keep your existing scripts working while adding network support alongside them.

#### For Inventory.cs:

**Don't modify** your existing `Inventory.cs` yet. Instead:

1. Add `NetworkedInventoryExample.cs` to your player
2. Both scripts can coexist temporarily
3. Gradually move functionality from `Inventory.cs` to `NetworkedInventoryExample.cs`
4. Once fully migrated, disable or remove old `Inventory.cs`

**Migration Checklist:**
```
[ ] Test NetworkedInventoryExample in single-player first
[ ] Verify all weapon management works
[ ] Verify all item management works
[ ] Test in multiplayer with 2+ clients
[ ] Switch over UI references from old to new script
[ ] Remove or disable old Inventory.cs
```

#### For PlayerInteraction.cs:

The file has already been updated with NetworkBehaviour. However, if you have custom modifications:

**Before (Old Code):**
```csharp
public class PlayerInteraction : MonoBehaviour
{
    void Update()
    {
        // Your interaction code
    }
}
```

**After (New Code):**
```csharp
public class PlayerInteraction : NetworkBehaviour
{
    void Update()
    {
        // Only process input for local player
        if (!base.IsOwner)
            return;
            
        // Your interaction code
    }
}
```

### Option B: Direct Integration

Modify your existing `Inventory.cs` directly:

#### 1. Change Base Class

```csharp
// OLD
public class Inventory : MonoBehaviour

// NEW
using FishNet.Object;
public class Inventory : NetworkBehaviour
```

#### 2. Add Owner Checks to Input Methods

```csharp
void Update()
{
    // Add this check at the start of Update
    if (!base.IsOwner)
        return;
        
    // Rest of your update code...
}
```

#### 3. Replace Direct State Changes with RPCs

**OLD - Direct Change:**
```csharp
void CollectItem()
{
    items.Add(item);
    item.SetActive(false);
}
```

**NEW - Network Synchronized:**
```csharp
void CollectItem()
{
    if (!base.IsOwner) return;
    
    // Request server to add item
    ServerCollectItemRpc(item);
}

[ServerRpc]
private void ServerCollectItemRpc(GameObject item)
{
    items.Add(item);
    ObserversCollectItem(item);
}

[ObserversRpc]
private void ObserversCollectItem(GameObject item)
{
    if (!base.IsOwner)
    {
        // Update for other clients
        item.SetActive(false);
    }
}
```

#### 4. Add SyncVars for Critical State

```csharp
using FishNet.Object.Synchronizing;

// For simple types
[SyncVar(OnChange = nameof(OnWeaponChanged))]
private string currentWeaponName;

// For lists
[SyncObject]
private readonly SyncList<string> weapons = new SyncList<string>();

private void OnWeaponChanged(string oldValue, string newValue, bool asServer)
{
    // Update UI or visuals
    UpdateWeaponVisual(newValue);
}
```

## Step 3: Update Item Prefabs

For every item that can be picked up:

### Required Components:

1. **NetworkObject**
   - Menu: Add Component → FishNet → Network Object
   - Settings: Is Networked ✓

2. **NetworkTransform**
   - Menu: Add Component → FishNet → Network Transform
   - Settings: Synchronize Position ✓, Synchronize Rotation ✓

3. **PickupItemPhysics**
   - Menu: Add Component → Scripts → PickupItemPhysics
   - This is the new network-aware physics handler

4. **Rigidbody** (existing)
   - Ensure it exists on the item

5. **Collider** (existing)
   - Ensure it exists on the item

### Example Prefab Setup:

```
ItemPrefab
├── NetworkObject
├── NetworkTransform
├── PickupItemPhysics (NEW)
├── Rigidbody
├── BoxCollider (or other collider)
├── InteractableItem (existing)
└── MeshRenderer
```

## Step 4: Update Player Prefab

### Required Components:

1. **NetworkObject**
   - Settings: Is Networked ✓, Is Global ✓

2. **NetworkTransform**
   - For smooth position synchronization

3. **PlayerInteraction** (updated)
   - Now inherits from NetworkBehaviour

4. **Inventory or NetworkedInventoryExample**
   - Choose your migration approach

### Player Hierarchy:

```
Player
├── NetworkObject
├── NetworkTransform
├── PlayerInteraction (NetworkBehaviour)
├── Inventory (old) or NetworkedInventoryExample (new)
├── PlayerMovement
├── MouseLook
└── PlayerRig (NEW - recommended)
    ├── Hand (NetworkObject)
    └── Backpack (NetworkObject)
```

## Step 5: Scene Setup

### Add Network Manager:

1. Create empty GameObject: "NetworkManager"
2. Add Component → FishNet → Network Manager
3. Add Component → FishNet → Server Manager
4. Add Component → FishNet → Client Manager
5. Add Component → FishNet → Player Spawner
   - Assign your player prefab

### Configuration:

```
NetworkManager
├── Transport: Tugboat (default)
├── Server Manager
│   ├── Start On Headless: ✓
│   └── Max Clients: 10 (or your preference)
├── Client Manager
│   └── Auto Connect: ✗ (use menu instead)
└── Player Spawner
    └── Player Prefab: Drag your player here
```

## Step 6: Test Migration

### Single Player Test:
1. Open scene with NetworkManager
2. Play in Editor
3. Verify everything works as before
4. Check console for network warnings (expected in single-player)

### Multiplayer Test:
1. Build your project (File → Build Settings → Build)
2. Run the build as Server/Host
3. Run Unity Editor as Client
4. Test:
   - [ ] Both players spawn correctly
   - [ ] Items can be picked up
   - [ ] Items synchronize between players
   - [ ] Physics works smoothly
   - [ ] UI updates correctly

## Step 7: Update Existing Item Interactions

### OLD Way (Direct GameObject manipulation):
```csharp
void PickupItem(GameObject item)
{
    item.transform.SetParent(handTransform);
    item.GetComponent<Rigidbody>().isKinematic = true;
}
```

### NEW Way (Using PickupItemPhysics):
```csharp
void PickupItem(GameObject item)
{
    PickupItemPhysics physics = item.GetComponent<PickupItemPhysics>();
    if (physics != null)
    {
        physics.RequestPickup(handTransform);
        // Physics state is now handled automatically
    }
}
```

### Integration with Existing InteractableItem:

```csharp
// In your existing interaction code:
InteractableItem interactable = hit.collider.GetComponent<InteractableItem>();
PickupItemPhysics physics = hit.collider.GetComponent<PickupItemPhysics>();

if (interactable != null && interactable.canBePickedUp)
{
    if (physics != null)
    {
        // Use network-aware pickup
        physics.RequestPickup(handTransform);
    }
    else
    {
        // Fallback to old method (for non-networked items)
        // Your existing code here
    }
}
```

## Common Issues and Solutions

### Issue: "IsOwner is always false"
**Solution:** Ensure the player has NetworkObject with proper ownership. The PlayerSpawner should automatically assign ownership to connecting clients.

### Issue: "Items not syncing between clients"
**Solution:** 
1. Verify item has NetworkObject and NetworkTransform
2. Check that ServerRpc methods are being called
3. Ensure Network Manager is running on server

### Issue: "Input working on all players"
**Solution:** Add `if (!base.IsOwner) return;` at the start of input-handling methods.

### Issue: "Physics glitching or jittering"
**Solution:**
1. Increase NetworkTransform interpolation (try 0.15-0.25)
2. Reduce network send rate if bandwidth limited
3. Check for physics collisions causing force spikes

### Issue: "Can't find FishNet namespaces"
**Solution:**
1. Check Packages/manifest.json has FishNet entry
2. Reimport Assets (right-click Assets folder → Reimport)
3. Restart Unity Editor
4. Check Package Manager for errors

### Issue: "Items duplicate or disappear"
**Solution:**
1. Ensure only server spawns/despawns networked objects
2. Use ServerRpc for state changes
3. Don't manually Instantiate networked prefabs (use ServerManager.Spawn)

## Best Practices

### 1. Always Check Ownership
```csharp
if (!base.IsOwner) return; // For input
if (!base.IsServerStarted) return; // For server-only logic
```

### 2. Use Appropriate RPC Types
- **ServerRpc**: Client → Server communication
- **ObserversRpc**: Server → All Clients broadcast
- **TargetRpc**: Server → Specific Client communication

### 3. Minimize Network Traffic
```csharp
// BAD - sends every frame
void Update() {
    ServerUpdatePositionRpc(transform.position);
}

// GOOD - let NetworkTransform handle it
// or batch updates:
float timer = 0;
void Update() {
    timer += Time.deltaTime;
    if (timer >= 0.1f) { // Only every 100ms
        ServerUpdatePositionRpc(transform.position);
        timer = 0;
    }
}
```

### 4. Validate on Server
```csharp
[ServerRpc(RequireOwnership = false)]
private void ServerPickupItemRpc(GameObject item)
{
    // Validate request
    if (item == null) return;
    if (Vector3.Distance(transform.position, item.transform.position) > maxPickupRange)
        return; // Too far away
    
    // Process valid request
    ProcessPickup(item);
}
```

### 5. Handle Late Joiners
Use `BufferLast = true` for important state:
```csharp
[ObserversRpc(BufferLast = true)]
private void ObserversSetWeapon(string weaponName)
{
    // This will be sent to late joiners automatically
}
```

## Performance Optimization

1. **Reduce NetworkTransform Send Rate** for distant objects
2. **Use Area of Interest** to limit what each client receives
3. **Pool NetworkObjects** instead of spawning/despawning
4. **Compress data** in RPCs when sending large amounts
5. **Batch RPCs** when making multiple related changes

## Further Reading

- [FishNet Official Documentation](https://fish-networking.gitbook.io/docs/)
- [FISHNET_MULTIPLAYER_SETUP.md](./Assets/Interactions/FISHNET_MULTIPLAYER_SETUP.md) - Component setup guide
- [NetworkedInventoryExample.cs](./Assets/Player/NetworkedInventoryExample.cs) - Example implementation

## Support

If you encounter issues during migration:
1. Check Unity Console for specific errors
2. Review FishNet documentation
3. Join FishNet Discord community
4. Check this repository's Issues section

## Rollback Plan

If you need to rollback to single-player:

1. Remove FishNet from manifest.json
2. Revert PlayerInteraction.cs to MonoBehaviour
3. Remove PickupItemPhysics components from items
4. Remove NetworkObject and NetworkTransform from prefabs
5. Remove NetworkManager from scenes

Keep backups before major changes!
