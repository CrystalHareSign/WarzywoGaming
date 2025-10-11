# FishNet Multiplayer Setup Guide

This document explains how to configure items and players for FishNet multiplayer support.

## Overview

The multiplayer system uses FishNet networking framework to synchronize item pickup, drop, and attachment between players. The main components are:

1. **PickupItemPhysics.cs** - Handles networked item physics and state synchronization
2. **PlayerInteraction.cs** - Manages player interaction with items using RPCs
3. **NetworkTransform** - Unity component for transform synchronization

## Required Components for Pickup Items

For each item prefab that can be picked up, dropped, or attached:

### 1. Add NetworkObject Component
- Select the item prefab
- Add Component → FishNet → Network Object
- Configure settings:
  - **Is Networked**: ✓ (checked)
  - **Is Global**: ✗ (unchecked for most items)
  - **Default Despawn Type**: Destroy

### 2. Add NetworkTransform Component
- Add Component → FishNet → Network Transform
- Configure settings:
  - **Synchronize Position**: ✓
  - **Synchronize Rotation**: ✓
  - **Synchronize Scale**: ✗ (usually not needed)
  - **Interpolation**: 0.15 (adjust for smoothness)
  - **Send Rate**: 0.05 (20 updates per second, adjust as needed)

### 3. Add PickupItemPhysics Component
- Add Component → Scripts → PickupItemPhysics
- Configure settings:
  - **Use Gravity When Dropped**: ✓
  - **Kinematic When Held**: ✓
  - **Use Joint When Attached**: ✓ (for backpack items)
  - **Joint Break Force**: 1000 (adjust based on gameplay)
  - **Joint Break Torque**: 1000 (adjust based on gameplay)

### 4. Ensure Rigidbody is Present
- The item must have a Rigidbody component
- Recommended settings:
  - **Mass**: 1 (adjust based on item)
  - **Use Gravity**: ✓ (will be controlled by PickupItemPhysics)
  - **Is Kinematic**: ✗ (will be controlled by PickupItemPhysics)

### 5. Ensure Collider is Present
- The item must have at least one Collider component
- The collider's **Is Trigger** property will be managed by PickupItemPhysics

## Player Setup

### 1. Add NetworkObject to Player
- Select the player prefab
- Add Component → FishNet → Network Object
- Configure:
  - **Is Networked**: ✓
  - **Is Global**: ✓ (players usually persist across scenes)

### 2. Add NetworkTransform to Player
- Add Component → FishNet → Network Transform
- Configure similar to items but with:
  - **Send Rate**: 0.02 (50 updates per second for smoother player movement)

### 3. Update PlayerInteraction Component
- The PlayerInteraction script now inherits from NetworkBehaviour
- Ensure it's properly configured in the Inspector
- The script will automatically check IsOwner before processing input

### 4. Create Hand and Backpack Transforms
Option A - Manual Setup (Recommended):
1. Create a child GameObject under the player named "PlayerRig"
2. Under PlayerRig, create:
   - "Hand" - Position at (0.5, -0.3, 1.0) relative to player
   - "Backpack" - Position at (0, 0.5, -0.5) relative to player
3. Add NetworkObject component to both Hand and Backpack

Option B - Automatic Setup:
- The PlayerInteraction script will automatically create these if they don't exist
- They will be positioned at default locations

## Network Manager Setup

### 1. Create Network Manager GameObject
1. Create empty GameObject named "NetworkManager"
2. Add Component → FishNet → Network Manager
3. Configure:
   - **Transport**: Tugboat (default) or your preferred transport

### 2. Configure Server Manager
- Under Server Manager:
  - **Start On Headless**: ✓ (for dedicated servers)
  - **Maximum Clients**: Set your desired max players

### 3. Configure Client Manager
- Under Client Manager:
  - **Auto Start Connection**: ✗ (usually handled by menu)

### 4. Add Player Spawner
1. Add Component → FishNet → Player Spawner
2. Configure:
   - **Player Prefab**: Drag your player prefab here
   - **Spawn Areas**: Define spawn points

## Testing Multiplayer

### Local Testing
1. Build the project
2. Run 2+ instances:
   - One as Host (Server + Client)
   - Others as Clients
3. Test pickup, drop, and attachment synchronization

### Key Things to Test
- ✓ Items appear at correct positions for all clients
- ✓ Physics synchronization works smoothly
- ✓ Joint attachments work correctly
- ✓ Items can be transferred between hand and backpack
- ✓ Dropped items fall with proper physics
- ✓ Multiple players can interact without conflicts

## Usage in Code

### Picking Up an Item
```csharp
// On the client
PlayerInteraction playerInteraction = GetComponent<PlayerInteraction>();
playerInteraction.RequestPickupItem(itemGameObject);
```

### Dropping an Item
```csharp
// On the client
Vector3 dropPosition = player.transform.position + player.transform.forward * 2f;
playerInteraction.RequestDropItem(itemGameObject, dropPosition);
```

### Moving to Backpack
```csharp
// On the client
playerInteraction.RequestMoveToBackpack(itemGameObject);
```

### Moving to Hand
```csharp
// On the client
playerInteraction.RequestMoveToHand(itemGameObject);
```

## Troubleshooting

### Items not synchronizing
- Ensure NetworkObject and NetworkTransform are on the item
- Check that the item prefab is registered with FishNet
- Verify Server Manager is running

### Physics glitches
- Adjust NetworkTransform interpolation value
- Increase Send Rate for more frequent updates
- Check that Rigidbody settings are appropriate

### Players can't pick up items
- Verify PlayerInteraction has NetworkBehaviour inheritance
- Check that IsOwner returns true for local player
- Ensure item has PickupItemPhysics component

### Joint breaking unexpectedly
- Increase Joint Break Force and Torque values
- Check for physics collisions causing force spikes
- Verify item mass is appropriate

## Performance Optimization

1. **Reduce Send Rate** for distant objects
2. **Use LOD** for item meshes
3. **Implement Culling** for items far from players
4. **Pool Objects** instead of instantiating/destroying
5. **Batch RPCs** when possible

## Advanced: Custom Joint Synchronization

For advanced use cases with complex joint setups:

```csharp
// Extend PickupItemPhysics
public class CustomPickupPhysics : PickupItemPhysics
{
    [SyncObject]
    private readonly SyncList<JointData> syncedJoints = new SyncList<JointData>();
    
    // Implement custom joint replication
}
```

## FishNet Version

This implementation is compatible with FishNet 4.4.7+. Update the package reference in `Packages/manifest.json` if needed:

```json
"com.firstgeargames.fishnet": "https://github.com/FirstGearGames/FishNet.git?path=/Assets/FishNet#4.4.7"
```

## Additional Resources

- [FishNet Documentation](https://fish-networking.gitbook.io/docs/)
- [FishNet Discord](https://discord.gg/Ta9HgDh4Hj)
- [Example Projects](https://github.com/FirstGearGames/FishNet/tree/main/Assets/FishNet/Example)

## License

This implementation follows the same license as your project.
