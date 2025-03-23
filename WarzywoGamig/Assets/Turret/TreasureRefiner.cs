using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TreasureRefiner : MonoBehaviour
{
    public Inventory inventory;
    public InventoryUI inventoryUI;

    private bool isRefining = false;

    public TextMeshProUGUI[] categoryTexts;
    public TextMeshProUGUI[] countTexts;
    public TextMeshProUGUI trashCategoryText; // Nowy tekst dla kategorii trash
    public TextMeshProUGUI trashCountText; // Nowy tekst dla ilo�ci trash
    public float maxResourcePerSlot = 50f;
    public GameObject[] categoryButtons; // 4 Cubes
    public GameObject refineButton; // 5-ty Cube
    public GameObject prefabToSpawn;
    public Transform spawnPoint;
    public float spawnYPosition = 2f; // ustawiasz dok�adne Y w inspektorze
    public float refineAmount = 10;
    public float trashAmount = 0; 

    // Nowe zmienne dla supplyTrash i refineTrash
    public GameObject supplyTrashButton;
    public GameObject refineTrashButton;
    public float trashResourceRequired = 100f; // Wymagana ilo�� zasob�w na trash

    private int selectedCategoryIndex = -1;
    private Color defaultColor;
    public Color highlightColor = Color.green;

    private void Start()
    {
        InitializeSlots();
        if (categoryButtons.Length > 0)
        {
            defaultColor = categoryButtons[0].GetComponent<Renderer>().material.color;
        }

        // Sprawdzanie, czy jeste�my w scenie Home
        UpdateButtonStates();

        // Rejestracja nas�uchiwania na zmian� sceny
        SceneManager.sceneLoaded += OnSceneLoaded;

        trashAmount = 0;
        trashCountText.text = "0";
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

                // Dodajemy obs�ug� nowych przycisk�w
                if (hit.collider.gameObject == supplyTrashButton)
                {
                    SupplyTrash();
                    return;
                }

                if (hit.collider.gameObject == refineTrashButton)
                {
                    RefineTrash();
                    return;
                }
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Aktualizujemy stan przycisk�w po zmianie sceny
        UpdateButtonStates();

        // Zresetowanie liczby trash po za�adowaniu sceny Home
        if (scene.name == "Main")
        {
            trashAmount = 0;
            trashCountText.text = "0";  // Zaktualizowanie UI, �eby pokaza� 0
            Debug.Log("Licznik Trash zresetowany do 0 na scenie Main.");
        }
    }

    // Funkcja do aktualizacji stanu przycisk�w na podstawie sceny
    private void UpdateButtonStates()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Je�li jeste�my w scenie Home, przyciski s� aktywne
        if (currentScene == "Home")
        {
            supplyTrashButton.SetActive(true);
            refineTrashButton.SetActive(true);
        }
        else
        {
            // W innych scenach przyciski s� nieaktywne
            supplyTrashButton.SetActive(false);
            refineTrashButton.SetActive(false);
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

        float currentAmount = int.Parse(countTexts[selectedCategoryIndex].text);

        if (currentAmount >= refineAmount)
        {
            currentAmount -= refineAmount;
            countTexts[selectedCategoryIndex].text = currentAmount.ToString();

            // Spawnowanie zwyk�ego zasobu
            SpawnPrefab(isTrash: false);
        }
        else
        {
            Debug.Log("Za ma�o zasob�w w wybranej kategorii!");
        }
    }

    private void SpawnPrefab(bool isTrash = false)
    {
        float resourceAmount = isTrash ? trashResourceRequired : refineAmount;

        // 1. Sprawd�, czy spawnPoint ma dzieci
        if (spawnPoint.childCount > 0)
        {
            foreach (Transform child in spawnPoint)
            {
                Debug.Log("Spawn point zablokowany przez dziecko: " + child.gameObject.name);
            }
            return;
        }

        // 2. Sprawd� kolizje w obszarze Collidera spawnPointa
        Collider spawnCollider = spawnPoint.GetComponent<Collider>();
        if (spawnCollider != null)
        {
            Collider[] overlaps = Physics.OverlapBox(
                spawnCollider.bounds.center,
                spawnCollider.bounds.extents,
                spawnPoint.rotation
            );

            foreach (Collider col in overlaps)
            {
                if (col.transform != spawnPoint) // Ignorujemy collider spawnPointa
                {
                    Debug.Log("Nie mo�na zespawnowa� � kolizja z obiektem: " + col.gameObject.name);
                    return;
                }
            }
        }

        // 3. Spawnowanie � ustaw Y manualnie
        Vector3 spawnPos = new Vector3(spawnPoint.position.x, spawnYPosition, spawnPoint.position.z);
        GameObject spawned = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        spawned.transform.SetParent(spawnPoint); // opcjonalnie jako dziecko

        // Dodajemy skrypt TreasureValue do prefabrykatu
        TreasureValue treasureValue = spawned.AddComponent<TreasureValue>();

        // Przypisujemy kategori� i ilo�� zasob�w
        string resourceCategory = categoryTexts[selectedCategoryIndex].text;
        treasureValue.category = resourceCategory;
        treasureValue.amount = (int)resourceAmount;

        Debug.Log("Prefab zespawnowany na Y = " + spawnPos.y + " z kategori�: " + resourceCategory + " i ilo�ci�: " + resourceAmount);
    }

    public void SupplyTrash()
    {
        Debug.Log($"[START] Aktualna ilo�� Trash przed sumowaniem: {trashAmount}");

        int totalTrashAmount = 0;

        for (int i = 0; i < categoryTexts.Length; i++)
        {
            int currentAmount = int.Parse(countTexts[i].text);
            Debug.Log($"[Slot {i + 1}] Kategoria: {categoryTexts[i].text}, Ilo��: {currentAmount}");

            totalTrashAmount += currentAmount;

            if (currentAmount > 0)
            {
                countTexts[i].text = "0";
                categoryTexts[i].text = "Trash";
            }
        }

        if (totalTrashAmount > 0)
        {
            trashAmount += totalTrashAmount;
            trashCountText.text = trashAmount.ToString();
            Debug.Log($"Sumowano {totalTrashAmount} zasob�w do Trash. Ca�kowita ilo�� Trash: {trashAmount}");
        }
        else
        {
            Debug.Log("Brak zasob�w do sumowania w slotach.");
        }
    }

    private void RefineTrash()
    {
        float currentTrashAmount = int.Parse(trashCountText.text);

        if (currentTrashAmount >= trashResourceRequired)
        {
            currentTrashAmount -= trashResourceRequired;
            trashCountText.text = currentTrashAmount.ToString();

            // Spawnowanie Trash
            SpawnPrefab(isTrash: true);
        }
        else
        {
            Debug.Log("Za ma�o zasob�w do przetworzenia trash! Masz tylko " + currentTrashAmount + ", a potrzeba " + trashResourceRequired);
        }
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

    // Funkcja sprawdzaj�ca, czy spawn point jest zablokowany
    private bool IsSpawnPointBlocked()
    {
        if (spawnPoint.childCount > 0)
            return true;

        Collider spawnCollider = spawnPoint.GetComponent<Collider>();
        if (spawnCollider != null)
        {
            Collider[] overlaps = Physics.OverlapBox(
                spawnCollider.bounds.center,
                spawnCollider.bounds.extents,
                spawnPoint.rotation
            );

            foreach (Collider col in overlaps)
            {
                if (col.transform != spawnPoint)
                    return true;
            }
        }

        return false;
    }

    public void ResetSlots()
    {
        for (int i = 0; i < categoryTexts.Length; i++)
        {
            categoryTexts[i].text = "-";  // Resetujemy kategori�
            countTexts[i].text = "0";     // Resetujemy ilo��
        }

        Debug.Log("Wszystkie sloty zosta�y zresetowane.");
    }
}
