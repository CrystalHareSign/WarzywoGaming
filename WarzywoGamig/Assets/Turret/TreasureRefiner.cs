using TMPro;
using UnityEngine;

public class TreasureRefiner : MonoBehaviour
{
    public Inventory inventory;
    public InventoryUI inventoryUI;

    private bool isRefining = false;

    public TextMeshProUGUI[] categoryTexts;
    public TextMeshProUGUI[] countTexts;

    public float maxResourcePerSlot = 50f;

    public GameObject[] categoryButtons; // 4 Cubes
    public GameObject refineButton; // 5-ty Cube

    public GameObject prefabToSpawn;
    public Transform spawnPoint;
    public float spawnHeightOffset = 2f; // ustalasz sobie w inspektorze

    public int refineAmount = 10;

    private int selectedCategoryIndex = -1;

    private Color defaultColor;
    public Color highlightColor = Color.green;

    void Start()
    {
        InitializeSlots();
        if (categoryButtons.Length > 0)
        {
            defaultColor = categoryButtons[0].GetComponent<Renderer>().material.color;
        }
    }

    void Update()
    {
        HandleMouseClick();
    }

    private void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                for (int i = 0; i < categoryButtons.Length; i++)
                {
                    if (hit.collider.gameObject == categoryButtons[i])
                    {
                        SelectCategory(i);
                        return;
                    }
                }

                if (hit.collider.gameObject == refineButton)
                {
                    RefineResources();
                    return;
                }
            }
        }
    }

    private void SelectCategory(int index)
    {
        selectedCategoryIndex = index;
        Debug.Log($"Wybrano kategori� w slocie {index + 1}: {categoryTexts[index].text}");

        for (int i = 0; i < categoryButtons.Length; i++)
        {
            Renderer rend = categoryButtons[i].GetComponent<Renderer>();
            if (i == index)
                rend.material.color = highlightColor;
            else
                rend.material.color = defaultColor;
        }
    }

    private void RefineResources()
    {
        if (selectedCategoryIndex == -1)
        {
            Debug.Log("Wybierz kategori� zanim przetworzysz zasoby!");
            return;
        }

        int currentAmount = int.Parse(countTexts[selectedCategoryIndex].text);

        if (currentAmount >= refineAmount)
        {
            currentAmount -= refineAmount;
            countTexts[selectedCategoryIndex].text = currentAmount.ToString();
            SpawnPrefab();
        }
        else
        {
            Debug.Log("Za ma�o zasob�w w wybranej kategorii!");
        }
    }

    private void SpawnPrefab()
    {
        Vector3 spawnPos = spawnPoint.position + new Vector3(0, spawnHeightOffset, 0);
        Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        Debug.Log("Prefab zosta� zespawnowany na wysoko�ci " + spawnPos.y);
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
