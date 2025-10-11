using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;

/// <summary>
/// Handles networked pickup item physics and synchronization using FishNet.
/// This component should be added to any item that can be picked up, held, or attached in multiplayer.
/// </summary>
public class PickupItemPhysics : NetworkBehaviour
{
    [Header("Item State")]
    [SyncVar(OnChange = nameof(OnIsHeldChanged))]
    private bool isHeld = false;

    [SyncVar(OnChange = nameof(OnIsAttachedChanged))]
    private bool isAttached = false;

    [Header("References")]
    private Rigidbody rb;
    private Collider[] colliders;
    private ConfigurableJoint joint;
    private Transform holdParent;

    [Header("Physics Settings")]
    public bool useGravityWhenDropped = true;
    public bool kinematicWhenHeld = true;

    [Header("Joint Settings")]
    public bool useJointWhenAttached = true;
    public float jointBreakForce = 1000f;
    public float jointBreakTorque = 1000f;

    // Synchronized joint target for ConfigurableJoint
    [SyncVar(OnChange = nameof(OnJointTargetChanged))]
    private Vector3 syncedJointTargetPosition;
    
    [SyncVar(OnChange = nameof(OnJointTargetChanged))]
    private Quaternion syncedJointTargetRotation;

    // Network object reference for the holder
    [SyncVar]
    private int holderNetworkObjectId = -1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();
        
        if (rb == null)
        {
            Debug.LogWarning($"PickupItemPhysics on {gameObject.name} requires a Rigidbody component.");
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        // Apply initial state when joining
        ApplyHeldState(isHeld);
        ApplyAttachedState(isAttached);
    }

    #region Pickup and Drop Methods

    /// <summary>
    /// Request to pick up this item. Call this from the client.
    /// </summary>
    /// <param name="parent">The transform to parent this item to (e.g., player's hand)</param>
    public void RequestPickup(Transform parent)
    {
        if (base.IsOwner)
        {
            ServerPickup(parent.GetComponent<NetworkObject>().ObjectId);
        }
        else
        {
            ServerPickupRpc(parent.GetComponent<NetworkObject>().ObjectId);
        }
    }

    /// <summary>
    /// Request to drop this item. Call this from the client.
    /// </summary>
    /// <param name="dropPosition">World position to drop the item at</param>
    /// <param name="dropRotation">World rotation to drop the item with</param>
    public void RequestDrop(Vector3 dropPosition, Quaternion dropRotation)
    {
        if (base.IsOwner)
        {
            ServerDrop(dropPosition, dropRotation);
        }
        else
        {
            ServerDropRpc(dropPosition, dropRotation);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerPickupRpc(int parentNetworkObjectId)
    {
        ServerPickup(parentNetworkObjectId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerDropRpc(Vector3 dropPosition, Quaternion dropRotation)
    {
        ServerDrop(dropPosition, dropRotation);
    }

    [Server]
    private void ServerPickup(int parentNetworkObjectId)
    {
        isHeld = true;
        holderNetworkObjectId = parentNetworkObjectId;
        
        // Find the parent transform
        if (base.ServerManager.Objects.Spawned.TryGetValue(parentNetworkObjectId, out NetworkObject netObj))
        {
            holdParent = netObj.transform;
            transform.SetParent(holdParent);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        ApplyHeldState(true);
    }

    [Server]
    private void ServerDrop(Vector3 dropPosition, Quaternion dropRotation)
    {
        isHeld = false;
        holderNetworkObjectId = -1;
        holdParent = null;
        
        transform.SetParent(null);
        transform.position = dropPosition;
        transform.rotation = dropRotation;

        ApplyHeldState(false);
    }

    #endregion

    #region Attach and Detach Methods

    /// <summary>
    /// Request to attach this item to a specific point (e.g., backpack slot).
    /// </summary>
    /// <param name="attachPoint">The transform to attach to</param>
    /// <param name="useJoint">Whether to use a ConfigurableJoint</param>
    public void RequestAttach(Transform attachPoint, bool useJoint = true)
    {
        if (base.IsOwner)
        {
            ServerAttach(attachPoint.GetComponent<NetworkObject>()?.ObjectId ?? -1, useJoint);
        }
        else
        {
            ServerAttachRpc(attachPoint.GetComponent<NetworkObject>()?.ObjectId ?? -1, useJoint);
        }
    }

    /// <summary>
    /// Request to detach this item.
    /// </summary>
    public void RequestDetach()
    {
        if (base.IsOwner)
        {
            ServerDetach();
        }
        else
        {
            ServerDetachRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerAttachRpc(int attachPointNetworkObjectId, bool useJoint)
    {
        ServerAttach(attachPointNetworkObjectId, useJoint);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerDetachRpc()
    {
        ServerDetach();
    }

    [Server]
    private void ServerAttach(int attachPointNetworkObjectId, bool useJoint)
    {
        isAttached = true;

        // Find the attach point
        if (attachPointNetworkObjectId >= 0 && base.ServerManager.Objects.Spawned.TryGetValue(attachPointNetworkObjectId, out NetworkObject netObj))
        {
            Transform attachPoint = netObj.transform;
            transform.SetParent(attachPoint);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            if (useJoint && useJointWhenAttached && rb != null)
            {
                CreateJoint(attachPoint);
            }
        }

        ApplyAttachedState(true);
    }

    [Server]
    private void ServerDetach()
    {
        isAttached = false;
        
        if (joint != null)
        {
            Destroy(joint);
            joint = null;
        }

        transform.SetParent(null);
        
        ApplyAttachedState(false);
    }

    #endregion

    #region Joint Management

    private void CreateJoint(Transform attachTarget)
    {
        if (rb == null) return;

        // Remove existing joint
        if (joint != null)
        {
            Destroy(joint);
        }

        joint = gameObject.AddComponent<ConfigurableJoint>();
        joint.connectedBody = attachTarget.GetComponent<Rigidbody>();
        
        // Configure joint to be stiff but allow some flexibility
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
        joint.angularXMotion = ConfigurableJointMotion.Locked;
        joint.angularYMotion = ConfigurableJointMotion.Locked;
        joint.angularZMotion = ConfigurableJointMotion.Locked;

        joint.breakForce = jointBreakForce;
        joint.breakTorque = jointBreakTorque;

        // Sync initial joint target
        if (base.IsServerStarted)
        {
            syncedJointTargetPosition = transform.localPosition;
            syncedJointTargetRotation = transform.localRotation;
        }
    }

    private void OnJointBroken(float breakForce)
    {
        if (base.IsServerStarted)
        {
            ServerDetach();
        }
    }

    #endregion

    #region State Change Callbacks

    private void OnIsHeldChanged(bool prev, bool next, bool asServer)
    {
        if (!asServer)
        {
            ApplyHeldState(next);
            
            // Re-parent on clients
            if (next && holderNetworkObjectId >= 0)
            {
                if (base.ClientManager.Objects.Spawned.TryGetValue(holderNetworkObjectId, out NetworkObject netObj))
                {
                    transform.SetParent(netObj.transform);
                    transform.localPosition = Vector3.zero;
                    transform.localRotation = Quaternion.identity;
                }
            }
            else if (!next)
            {
                transform.SetParent(null);
            }
        }
    }

    private void OnIsAttachedChanged(bool prev, bool next, bool asServer)
    {
        if (!asServer)
        {
            ApplyAttachedState(next);
        }
    }

    private void OnJointTargetChanged(Vector3 prev, Vector3 next, bool asServer)
    {
        UpdateJointTarget();
    }

    private void OnJointTargetChanged(Quaternion prev, Quaternion next, bool asServer)
    {
        UpdateJointTarget();
    }

    private void UpdateJointTarget()
    {
        if (joint != null && !base.IsServerStarted)
        {
            joint.targetPosition = syncedJointTargetPosition;
            joint.targetRotation = syncedJointTargetRotation;
        }
    }

    #endregion

    #region Physics State Management

    private void ApplyHeldState(bool held)
    {
        if (rb != null)
        {
            if (held)
            {
                rb.isKinematic = kinematicWhenHeld;
                rb.useGravity = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            else
            {
                rb.isKinematic = false;
                rb.useGravity = useGravityWhenDropped;
            }
        }

        // Update colliders
        if (colliders != null)
        {
            foreach (var col in colliders)
            {
                if (col != null)
                {
                    col.isTrigger = held;
                }
            }
        }
    }

    private void ApplyAttachedState(bool attached)
    {
        if (rb != null)
        {
            if (attached)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            else
            {
                rb.isKinematic = false;
                rb.useGravity = useGravityWhenDropped;
            }
        }

        if (colliders != null)
        {
            foreach (var col in colliders)
            {
                if (col != null)
                {
                    col.isTrigger = attached;
                }
            }
        }
    }

    #endregion

    #region Public Getters

    public bool IsHeld => isHeld;
    public bool IsAttached => isAttached;
    public bool HasJoint => joint != null;

    #endregion

    #region Update Joint Position (called from server)

    /// <summary>
    /// Updates the joint target position and rotation. Call this on the server.
    /// </summary>
    [Server]
    public void UpdateJointTransform(Vector3 localPosition, Quaternion localRotation)
    {
        syncedJointTargetPosition = localPosition;
        syncedJointTargetRotation = localRotation;

        if (joint != null)
        {
            joint.targetPosition = localPosition;
            joint.targetRotation = localRotation;
        }
    }

    #endregion
}
