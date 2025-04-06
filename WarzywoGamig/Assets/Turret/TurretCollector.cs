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

    public HarpoonController harpoonController;

    void Start()
    {
        //resourceSlots = new List<ResourceSlot>(GetComponentsInChildren<ResourceSlot>());

        // Initialize resource slots with empty categories, zero resources, and no visual representation
        foreach (var slot in resourceSlots)
        {
            slot.resourceCategory = "";
            slot.resourceCount = 0;
            slot.resourceVisual = null;
        }
    }
    void Update()
    {
        // Monitor the resource slots in every frame to reset them if needed
        MonitorSlots();
    }
    private void MonitorSlots()
    {
        // Monitor slots and reset them if the visual object is inactive (not active in hierarchy)
        foreach (var slot in resourceSlots)
        {
            if (slot.resourceVisual != null && !slot.resourceVisual.activeInHierarchy && slot.resourceCategory != "")
            {
                // Reset the slot when the visual object is inactive
                slot.resourceCategory = "";
                slot.resourceCount = 0;
                slot.resourceVisual = null;

                // Optionally, you can add a visual cue here for when the slot becomes empty
                // For example: Debug.Log("Slot " + slot.slotTransform.name + " is now empty.");
            }
        }
    }

    public void ResetSlotForItem(GameObject item)
    {
        foreach (var slot in resourceSlots)
        {
            if (slot.resourceVisual == item)
            {
                slot.resourceCategory = "";
                slot.resourceCount = 0;

                // Upewniamy siê, ¿e referencja nie bêdzie wskazywaæ na zniszczony obiekt
                if (slot.resourceVisual != null)
                {
                    slot.resourceVisual.SetActive(false);
                    slot.resourceVisual = null;
                }

                return;
            }
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

                // Sprawdzamy, czy resourceVisual nadal istnieje
                if (slot.resourceVisual != null && !slot.resourceVisual)
                {
                    slot.resourceVisual = null; // Zerujemy, jeœli obiekt zosta³ usuniêty
                }

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

                        // Aktualizacja lub stworzenie nowego wizualnego obiektu zasobu
                        UpdateResourceVisual(slot, treasureResources.gameObject, category.name, slot.resourceCount);

                        harpoonController = Object.FindFirstObjectByType<HarpoonController>();  // Pobieramy HarpoonController, jeœli nie jest przypisany w inspektorze

                        // Przesy³amy dane do HarpoonController
                        if (harpoonController != null)
                        {
                            harpoonController.UpdateResourceUI(resourceSlots);
                        }

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

        if (slot.resourceVisual.TryGetComponent(out TreasureResources resourceComponent))
        {
            resourceComponent.UpdateResourceCategoryCount(resourceCategory, resourceCount);
        }
    }
    public void ClearAllSlots()
    {
        foreach (var slot in resourceSlots)
        {
            // Resetowanie danych slotu
            slot.resourceCategory = "";
            slot.resourceCount = 0;

            // Dezaktywowanie i usuwanie wizualizacji zasobu, jeœli istnieje
            if (slot.resourceVisual != null)
            {
                slot.resourceVisual.SetActive(false);
                Destroy(slot.resourceVisual); // Usuwamy obiekt wizualizacji
                slot.resourceVisual = null;
            }
        }
    }

}