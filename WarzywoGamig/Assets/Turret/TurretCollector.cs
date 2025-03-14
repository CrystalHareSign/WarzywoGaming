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
        if (treasureResources == null || treasureResources.GetResourceCategories() == null)
        {
            Debug.LogError("TreasureResources is null or no resource categories found.");
            return;
        }

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
        if (slot.slotTransform == null)
        {
            Debug.LogError("slotTransform is null for ResourceSlot.");
            return;
        }

        // Tworzenie wizualizacji zasobu tylko jeœli nie istnieje
        if (slot.resourceVisual == null)
        {
            slot.resourceVisual = Instantiate(originalResource, slot.slotTransform.position, slot.slotTransform.rotation);
            slot.resourceVisual.transform.SetParent(slot.slotTransform);
            slot.resourceVisual.transform.localPosition = Vector3.zero;
            slot.resourceVisual.transform.localScale = Vector3.one * 0.2f;

            // Usuwanie niepotrzebnych komponentów
            foreach (var script in slot.resourceVisual.GetComponents<MonoBehaviour>())
            {
                if (!(script is TreasureResources) && !(script is InteractableItem) && !(script is HoverMessage))
                {
                    Destroy(script);
                }
            }

            // W³¹czenie komponentów kolizji oraz interakcji
            if (slot.resourceVisual.TryGetComponent(out Collider resourceCollider))
            {
                resourceCollider.enabled = true;
            }

            if (slot.resourceVisual.TryGetComponent(out InteractableItem interactableItem))
            {
                interactableItem.enabled = true;
            }

            if (slot.resourceVisual.TryGetComponent(out HoverMessage hoverMessage))
            {
                hoverMessage.enabled = true;
            }
        }

        // Zaktualizowanie komponentu TreasureResources
        TreasureResources copyResources = slot.resourceVisual.GetComponent<TreasureResources>();
        if (copyResources == null)
        {
            copyResources = slot.resourceVisual.AddComponent<TreasureResources>();
        }

        // Aktualizacja licznika zasobów
        ResourceCategory resourceCategoryToUpdate = copyResources.resourceCategories.Find(rc => rc.name == resourceCategory);
        if (resourceCategoryToUpdate != null)
        {
            resourceCategoryToUpdate.resourceCount = resourceCount;
        }
        else
        {
            copyResources.resourceCategories.Add(new ResourceCategory { name = resourceCategory, isActive = true, resourceCount = resourceCount });
        }
    }
}