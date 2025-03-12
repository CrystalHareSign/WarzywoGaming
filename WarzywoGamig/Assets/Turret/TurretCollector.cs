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
        public GameObject resourceVisual;
    }

    public List<ResourceSlot> resourceSlots = new List<ResourceSlot>();
    public int maxResourcePerSlot = 10;

    void Start()
    {
        // Initialize resource slots with empty categories, zero resources, and no visual representation
        foreach (var slot in resourceSlots)
        {
            slot.resourceCategory = "";
            slot.resourceCount = 0;
            slot.resourceVisual = null;
        }
    }

    public void CollectResource(TreasureResources treasureResources)
    {
        foreach (var category in treasureResources.GetResourceCategories())
        {
            int remainingResources = category.resourceCount;

            for (int i = 0; i < resourceSlots.Count; i++)
            {
                var slot = resourceSlots[i];
                if (slot.resourceCategory == "" || slot.resourceCategory == category.name)
                {
                    int availableSpace = maxResourcePerSlot - slot.resourceCount;
                    int resourcesToCollect = Mathf.Min(remainingResources, availableSpace);

                    if (resourcesToCollect > 0)
                    {
                        if (slot.resourceCategory == "")
                        {
                            slot.resourceCategory = category.name;
                        }
                        slot.resourceCount += resourcesToCollect;
                        remainingResources -= resourcesToCollect;

                        // Update or spawn a visual representation of the collected resources
                        UpdateResourceVisual(slot, treasureResources.gameObject, category.name, slot.resourceCount);

                        if (remainingResources == 0)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }

    private void UpdateResourceVisual(ResourceSlot slot, GameObject originalResource, string resourceCategory, int resourceCount)
    {
        if (slot.resourceVisual == null)
        {
            // Create a new visual representation if none exists
            slot.resourceVisual = Instantiate(originalResource, slot.slotTransform.position, slot.slotTransform.rotation);
            slot.resourceVisual.transform.SetParent(slot.slotTransform);
            slot.resourceVisual.transform.localPosition = Vector3.zero;
            slot.resourceVisual.transform.localScale = Vector3.one * 0.2f;

            // Remove all other scripts from the copied resource except TreasureResources
            foreach (var script in slot.resourceVisual.GetComponents<MonoBehaviour>())
            {
                if (!(script is TreasureResources))
                {
                    Destroy(script);
                }
            }
        }

        // Update the TreasureResources component of the copied resource
        TreasureResources copyResources = slot.resourceVisual.GetComponent<TreasureResources>();
        if (copyResources == null)
        {
            copyResources = slot.resourceVisual.AddComponent<TreasureResources>();
        }
        copyResources.resourceCategories = new List<ResourceCategory> { new ResourceCategory { name = resourceCategory, isActive = true, resourceCount = resourceCount } }; // Keep only the category of the collected resource
    }
}