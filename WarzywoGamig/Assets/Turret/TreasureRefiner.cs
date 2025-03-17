using TMPro;
using UnityEngine;

public class TreasureRefiner : MonoBehaviour
{
    public Inventory inventory;
    public InventoryUI inventoryUI;

    private bool isRefining = false;

    public TextMeshProUGUI[] categoryTexts;
    public TextMeshProUGUI[] countTexts;

    // Nowa zmienna dla limitu zasobów w jednym slocie
    public float maxResourcePerSlot = 50f;

    void Start()
    {
        InitializeSlots();
    }

    public void RemoveOldestItemFromInventory(string itemName)
    {
        if (isRefining) return;

        isRefining = true;

        GameObject itemToRemove = null;
        int itemIndex = -1;

        for (int i = 0; i < inventory.items.Count; i++)
        {
            if (inventory.items[i].name == itemName)
            {
                itemToRemove = inventory.items[i];
                itemIndex = i;
                break;
            }
        }

        if (itemToRemove != null)
        {
            InteractableItem interactableItem = itemToRemove.GetComponent<InteractableItem>();
            if (interactableItem != null)
            {
                UpdateTreasureRefinerSlots(interactableItem);

                inventory.items.RemoveAt(itemIndex);
                Destroy(itemToRemove);

                if (inventoryUI != null)
                {
                    inventoryUI.UpdateInventoryUI(inventory.weapons, inventory.items);
                }
            }
        }

        isRefining = false;
    }

    private void UpdateTreasureRefinerSlots(InteractableItem item)
    {
        TreasureResources treasureResources = item.GetComponent<TreasureResources>();

        if (treasureResources != null)
        {
            string resourceCategory = treasureResources.resourceCategories[0].name;
            int resourceCount = treasureResources.resourceCategories[0].resourceCount;

            bool addedToExistingSlot = false;

            // Najpierw sprawdzamy, czy dana kategoria ju¿ istnieje w slotach
            for (int i = 0; i < categoryTexts.Length; i++)
            {
                if (categoryTexts[i].text == resourceCategory)
                {
                    int currentCount = int.Parse(countTexts[i].text);
                    int newCount = Mathf.Min(currentCount + resourceCount, (int)maxResourcePerSlot);
                    countTexts[i].text = newCount.ToString();
                    addedToExistingSlot = true;
                    break;
                }
            }

            // Jeœli kategoria nie istnieje jeszcze w slotach, szukamy pustego slotu
            if (!addedToExistingSlot)
            {
                for (int i = 0; i < categoryTexts.Length; i++)
                {
                    if (categoryTexts[i].text == "-" && countTexts[i].text == "0")
                    {
                        categoryTexts[i].text = resourceCategory;
                        countTexts[i].text = Mathf.Min(resourceCount, (int)maxResourcePerSlot).ToString();
                        break;
                    }
                }
            }
        }
    }

    public void InitializeSlots()
    {
        for (int i = 0; i < categoryTexts.Length; i++)
        {
            categoryTexts[i].text = "-";
            countTexts[i].text = "0";
        }
    }
}
