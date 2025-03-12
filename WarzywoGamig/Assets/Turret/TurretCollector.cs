using UnityEngine;
using System.Collections.Generic;

public class TurretCollector : MonoBehaviour
{
    [System.Serializable]
    public class ResourceSlot
    {
        public Transform slotTransform;
        public string resourceCategory = "";
        public int resourceCount = 0;
    }

    public List<ResourceSlot> resourceSlots = new List<ResourceSlot>();
    public int maxResourcePerSlot = 10;

    void Start()
    {
        // Initialize resource slots with empty categories and zero resources
        foreach (var slot in resourceSlots)
        {
            slot.resourceCategory = "";
            slot.resourceCount = 0;
        }
    }

    public void CollectResource(TreasureResources treasureResources)
    {
        foreach (var category in treasureResources.resourceCategories)
        {
            int remainingResources = treasureResources.resourceCount;

            for (int i = 0; i < resourceSlots.Count; i++)
            {
                var slot = resourceSlots[i];
                if (slot.resourceCategory == "" || slot.resourceCategory == category)
                {
                    int availableSpace = maxResourcePerSlot - slot.resourceCount;
                    int resourcesToCollect = Mathf.Min(remainingResources, availableSpace);

                    if (resourcesToCollect > 0)
                    {
                        if (slot.resourceCategory == "")
                        {
                            slot.resourceCategory = category;
                        }
                        slot.resourceCount += resourcesToCollect;
                        remainingResources -= resourcesToCollect;

                        // Spawn a visual representation of the collected resources
                        SpawnResourceVisual(slot.slotTransform, treasureResources.gameObject, resourcesToCollect);

                        if (remainingResources == 0)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }

    private void SpawnResourceVisual(Transform slotTransform, GameObject originalResource, int resourceCount)
    {
        // Create a copy of the original resource with a smaller scale
        GameObject resourceCopy = Instantiate(originalResource, slotTransform.position, slotTransform.rotation);
        resourceCopy.transform.SetParent(slotTransform);
        resourceCopy.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

        // Update the TreasureResources component of the copied resource
        TreasureResources copyResources = resourceCopy.GetComponent<TreasureResources>();
        if (copyResources == null)
        {
            copyResources = resourceCopy.AddComponent<TreasureResources>();
        }
        copyResources.resourceCount = resourceCount;
        copyResources.resourceCategories = new List<string> { copyResources.resourceCategories[0] }; // Keep only the category of the collected resource

        // Remove all other scripts from the copied resource
        foreach (var script in resourceCopy.GetComponents<MonoBehaviour>())
        {
            if (!(script is TreasureResources))
            {
                Destroy(script);
            }
        }

        // Reset collider to the new position and size
        Collider resourceCollider = resourceCopy.GetComponent<Collider>();
        if (resourceCollider != null)
        {
            resourceCollider.enabled = false; // Disable collider to reset its bounds
            resourceCollider.enabled = true;  // Enable collider to apply the new bounds
        }
        Rigidbody resourceRb = resourceCopy.GetComponent<Rigidbody>();
        if (resourceRb != null)
        {
            resourceRb.isKinematic = true;
        }
    }
}