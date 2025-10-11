# FishNet Multiplayer Quick Reference

Quick reference for common networking tasks in WarzywoGaming.

## 🚀 Getting Started (3 Steps)

### 1. Item Prefab Setup
```
Add to each pickup item:
☐ NetworkObject
☐ NetworkTransform
☐ PickupItemPhysics
```

### 2. Player Prefab Setup
```
Add to player:
☐ NetworkObject (Is Global ✓)
☐ NetworkTransform
Update existing:
☐ PlayerInteraction (already updated)
```

### 3. Scene Setup
```
Create GameObject with:
☐ Network Manager
☐ Server Manager
☐ Client Manager
☐ Player Spawner
```

## 📋 Common Code Patterns

### Pickup Item
```csharp
PickupItemPhysics physics = item.GetComponent<PickupItemPhysics>();
if (physics != null)
{
    physics.RequestPickup(handTransform);
}
```

### Drop Item
```csharp
Vector3 dropPos = transform.position + Vector3.forward * 2f;
physics.RequestDrop(dropPos, Quaternion.identity);
```

### Attach to Backpack
```csharp
physics.RequestAttach(backpackTransform, useJoint: true);
```

### Detach from Backpack
```csharp
physics.RequestDetach();
```

### Check if Held/Attached
```csharp
if (physics.IsHeld) { /* ... */ }
if (physics.IsAttached) { /* ... */ }
```

## 🎮 Player Input Pattern

```csharp
void Update()
{
    // ALWAYS add this check first!
    if (!base.IsOwner)
        return;
    
    // Your input code here...
}
```

## 🌐 Network Methods

### Client → Server
```csharp
[ServerRpc]
private void ServerDoSomethingRpc(GameObject obj)
{
    // Runs on server only
    // Validate and process
}
```

### Server → All Clients
```csharp
[ObserversRpc]
private void ObserversUpdateStateRpc(Vector3 pos)
{
    // Runs on all clients
    // Update visuals/state
}
```

### Server → Specific Client
```csharp
[TargetRpc]
private void TargetShowMessageRpc(NetworkConnection conn, string msg)
{
    // Runs on specific client
    // Show UI notification
}
```

## 📦 Synchronized Data

### Simple Value
```csharp
[SyncVar(OnChange = nameof(OnValueChanged))]
private int myValue;

private void OnValueChanged(int old, int @new, bool asServer)
{
    // React to change
}
```

### List
```csharp
[SyncObject]
private readonly SyncList<string> myList = new SyncList<string>();

void Start()
{
    myList.OnChange += OnListChanged;
}

private void OnListChanged(SyncListOperation op, int index, string old, string @new, bool asServer)
{
    // React to list change
}
```

## 🔧 Ownership Checks

```csharp
// For input handling
if (!base.IsOwner) return;

// For server-only logic
if (!base.IsServerStarted) return;

// For client-only logic
if (!base.IsClientStarted) return;

// For owner on server
if (base.IsOwner && base.IsServerStarted) { }
```

## ⚡ Performance Tips

### ❌ Don't Do This
```csharp
void Update()
{
    // Sends 60 RPCs per second!
    ServerUpdatePositionRpc(transform.position);
}
```

### ✅ Do This Instead
```csharp
// Let NetworkTransform handle position
// Or batch updates:
float timer = 0;
void Update()
{
    timer += Time.deltaTime;
    if (timer >= 0.1f)
    {
        ServerUpdateRpc();
        timer = 0;
    }
}
```

## 🐛 Debugging

### Check Network Status
```csharp
Debug.Log($"IsServer: {base.IsServerStarted}");
Debug.Log($"IsClient: {base.IsClientStarted}");
Debug.Log($"IsOwner: {base.IsOwner}");
Debug.Log($"IsHost: {base.IsHostStarted}");
```

### Network Object Info
```csharp
NetworkObject netObj = GetComponent<NetworkObject>();
Debug.Log($"ObjectId: {netObj.ObjectId}");
Debug.Log($"Owner: {netObj.Owner.ClientId}");
```

### FishNet Debug Window
```
Window → Analysis → Fish-Networking → Network
Shows active connections, spawned objects, etc.
```

## 📱 Common Mistakes

### ❌ Mistake #1: Missing Ownership Check
```csharp
void Update()
{
    if (Input.GetKeyDown(KeyCode.E))
        Interact(); // Runs on ALL clients!
}
```
✅ **Fix:**
```csharp
void Update()
{
    if (!base.IsOwner) return; // Only owner processes input
    if (Input.GetKeyDown(KeyCode.E))
        Interact();
}
```

### ❌ Mistake #2: Direct State Change
```csharp
void PickupItem()
{
    item.transform.SetParent(hand); // Only changes locally!
}
```
✅ **Fix:**
```csharp
void PickupItem()
{
    ServerPickupItemRpc(item); // Syncs to all clients
}
```

### ❌ Mistake #3: Instantiating Network Objects
```csharp
GameObject obj = Instantiate(networkPrefab); // Wrong!
```
✅ **Fix:**
```csharp
// On server only:
NetworkObject obj = base.ServerManager.Spawn(networkPrefab);
```

## 🎯 Testing Checklist

Local Multiplayer Test:
```
☐ Build project
☐ Run build as Host
☐ Run Editor as Client
☐ Test all item interactions
☐ Check console for errors
☐ Verify smooth physics
☐ Test disconnection/reconnection
```

## 📚 Documentation Links

- Full Setup: [FISHNET_MULTIPLAYER_SETUP.md](./Assets/Interactions/FISHNET_MULTIPLAYER_SETUP.md)
- Migration: [MIGRATION_GUIDE.md](./Assets/MIGRATION_GUIDE.md)
- Example: [NetworkedInventoryExample.cs](./Assets/Player/NetworkedInventoryExample.cs)
- FishNet Docs: https://fish-networking.gitbook.io/docs/

## 🆘 Help

### Item Not Syncing?
1. ✓ Has NetworkObject?
2. ✓ Has NetworkTransform?
3. ✓ Has PickupItemPhysics?
4. ✓ Network Manager running?

### Input Working on All Players?
1. ✓ Check for `if (!base.IsOwner) return;`
2. ✓ Verify player has NetworkObject
3. ✓ Check ownership is assigned correctly

### Physics Glitching?
1. ✓ Increase NetworkTransform interpolation
2. ✓ Adjust send rate (lower = smoother but more bandwidth)
3. ✓ Check for collision issues
4. ✓ Verify Rigidbody settings

---

**Pro Tip:** Always test with 2+ clients to catch synchronization issues early!
