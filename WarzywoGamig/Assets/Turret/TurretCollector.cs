using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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

                // Przesy³amy dane do HarpoonController
                if (harpoonController != null)
                {
                    harpoonController.UpdateResourceUI(resourceSlots);
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
            slot.resourceVisual.transform.localScale = Vector3.one * 1.0f;

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

    // ZAPIS
    public List<CollectorSlotSaveData> GetSlotsSaveData()
    {
        var list = new List<CollectorSlotSaveData>();
        foreach (var slot in resourceSlots)
        {
            CollectorSlotSaveData data = new CollectorSlotSaveData();
            data.resourceCategory = slot.resourceCategory;
            data.resourceCount = slot.resourceCount;
            data.resourcePrefabName = slot.resourceVisual != null ? slot.resourceVisual.name.Replace("(Clone)", "") : "";
            list.Add(data);
        }
        return list;
    }

    public void LoadSlotsFromSave(List<CollectorSlotSaveData> saveData, Dictionary<string, GameObject> resourcePrefabs)
    {
        for (int i = 0; i < resourceSlots.Count && i < saveData.Count; i++)
        {
            var slot = resourceSlots[i];
            var data = saveData[i];

            slot.resourceCategory = data.resourceCategory;
            slot.resourceCount = data.resourceCount;

            if (slot.resourceVisual != null)
            {
                Destroy(slot.resourceVisual);
                slot.resourceVisual = null;
            }

            if (!string.IsNullOrEmpty(data.resourcePrefabName) && resourcePrefabs.ContainsKey(data.resourcePrefabName))
            {
                slot.resourceVisual = Instantiate(resourcePrefabs[data.resourcePrefabName], slot.slotTransform.position, slot.slotTransform.rotation, slot.slotTransform);
                slot.resourceVisual.transform.localPosition = Vector3.zero;
                slot.resourceVisual.transform.localScale = Vector3.one;
                slot.resourceVisual.SetActive(true);

                // Ustawienia fizyki
                var rb = slot.resourceVisual.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.isKinematic = true;

                // Aktywuj wymagane skrypty:
                var interactable = slot.resourceVisual.GetComponent<InteractableItem>();
                if (interactable != null)
                    interactable.enabled = true;

                var hover = slot.resourceVisual.GetComponent<HoverMessage>();
                if (hover != null)
                    hover.enabled = true;

                // USUÑ TreasureDefiner przed nadpisaniem zasobów!
                var definer = slot.resourceVisual.GetComponent<TreasureDefiner>();
                if (definer != null)
                    Destroy(definer);

                if (slot.resourceVisual.TryGetComponent(out TreasureResources treasure))
                {
                    Debug.Log($"[Przed] {slot.resourceCategory} count: {treasure.resourceCategories.FirstOrDefault()?.resourceCount}");

                    treasure.resourceCategories.Clear();
                    treasure.resourceCategories.Add(new ResourceCategory
                    {
                        name = slot.resourceCategory,
                        resourceCount = slot.resourceCount
                    });

                    Debug.Log($"[Po] {slot.resourceCategory} count: {treasure.resourceCategories.FirstOrDefault()?.resourceCount}");
                }

                if (harpoonController == null)
                {
                    harpoonController = Object.FindFirstObjectByType<HarpoonController>();
                }

                // ODŒWIE¯ENIE UI PO WGRANIU SLOTÓW
                if (harpoonController != null)
                {
                    harpoonController.UpdateResourceUI(resourceSlots);
                }
            }
            else
            {
                slot.resourceVisual = null;
            }
        }
    }
}

[System.Serializable]
public class CollectorSlotSaveData
{
    public string resourceCategory;
    public int resourceCount;
    public string resourcePrefabName; // Nowa nazwa prefab resourceVisual
}