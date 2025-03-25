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
    public TextMeshProUGUI trashCountText; // Nowy tekst dla iloœci trash
    public float maxResourcePerSlot = 50f;
    public GameObject[] categoryButtons; // 4 Cubes
    public GameObject refineButton; // 5-ty Cube
    public GameObject prefabToSpawn;
    public Transform spawnPoint;
    public float spawnYPosition = 2f; // ustawiasz dok³adne Y w inspektorze
    public float refineAmount = 10;
    public float trashAmount = 0; 

    // Nowe zmienne dla supplyTrash i refineTrash
    public GameObject supplyTrashButton;
    public GameObject refineTrashButton;
    public float trashResourceRequired = 100f; // Wymagana iloœæ zasobów na trash

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

        // Sprawdzanie, czy jesteœmy w scenie Home
        UpdateButtonStates();

        // Rejestracja nas³uchiwania na zmianê sceny
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

                // Dodajemy obs³ugê nowych przycisków
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
        // Aktualizujemy stan przycisków po zmianie sceny
        UpdateButtonStates();

        // Zresetowanie liczby trash po za³adowaniu sceny Home
        if (scene.name == "Main")
        {
            trashAmount = 0;
            trashCountText.text = "0";  // Zaktualizowanie UI, ¿eby pokazaæ 0
            Debug.Log("Licznik Trash zresetowany do 0 na scenie Main.");
        }
    }

    // Funkcja do aktualizacji stanu przycisków na podstawie sceny
    private void UpdateButtonStates()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Sprawdzamy, czy supplyTrashButton i refineTrashButton s¹ ró¿ne od null
        if (supplyTrashButton != null && refineTrashButton != null)
        {
            // Jeœli jesteœmy w scenie Home, przyciski s¹ aktywne
            if (currentScene == "Home")
            {
                supplyTrashButton.SetActive(true);
                refineTrashButton.SetActive(true);
            }
            else
            {
                // W innych scenach przyciski s¹ nieaktywne
                supplyTrashButton.SetActive(false);
                refineTrashButton.SetActive(false);
            }
        }
        else
        {
            // Je¿eli któryœ z przycisków jest null, wyœwietlamy komunikat w logu
            if (supplyTrashButton == null)
            {
                Debug.LogWarning("supplyTrashButton jest null.");
            }

            if (refineTrashButton == null)
            {
                Debug.LogWarning("refineTrashButton jest null.");
            }
        }
    }

    private void SelectCategory(int index)
    {
        selectedCategoryIndex = index;
        Debug.Log($"Wybrano kategoriê w slocie {index + 1}: {categoryTexts[index].text}");

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
            Debug.Log("Wybierz kategoriê zanim przetworzysz zasoby!");
            return;
        }

        float currentAmount = int.Parse(countTexts[selectedCategoryIndex].text);

        if (currentAmount >= refineAmount)
        {
            // Sprawdzamy, czy miejsce na spawnowanie nie jest zablokowane
            if (!IsSpawnPointBlocked())
            {
                // Odejmujemy zasoby, tylko gdy spawnowanie jest mo¿liwe
                currentAmount -= refineAmount;
                countTexts[selectedCategoryIndex].text = currentAmount.ToString();

                // Spawnowanie zwyk³ego zasobu
                SpawnPrefab(isTrash: false);
            }
            else
            {
                Debug.Log("Nie mo¿na zespawnowaæ – kolizja z obiektem!");
            }
        }
        else
        {
            Debug.Log("Za ma³o zasobów w wybranej kategorii!");
        }
    }

    private void SpawnPrefab(bool isTrash = false)
    {
        // Sprawdzenie, czy wybrany indeks jest poprawny
        if (selectedCategoryIndex < 0 || selectedCategoryIndex >= categoryTexts.Length)
        {
            Debug.LogError("Nieprawid³owy indeks kategorii: " + selectedCategoryIndex);
            return;
        }

        float resourceAmount = isTrash ? trashResourceRequired : refineAmount;

        // 1. SprawdŸ, czy spawnPoint ma dzieci
        if (spawnPoint.childCount > 0)
        {
            foreach (Transform child in spawnPoint)
            {
                Debug.Log("Spawn point zablokowany przez dziecko: " + child.gameObject.name);
            }
            return;
        }

        // 2. SprawdŸ kolizje w obszarze Collidera spawnPointa
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
                    Debug.Log("Nie mo¿na zespawnowaæ – kolizja z obiektem: " + col.gameObject.name);
                    return;
                }
            }
        }

        // 3. Spawnowanie – ustaw Y manualnie
        Vector3 spawnPos = new Vector3(spawnPoint.position.x, spawnYPosition, spawnPoint.position.z);
        GameObject spawned = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        spawned.transform.SetParent(spawnPoint); // opcjonalnie jako dziecko

        // Dodajemy skrypt TreasureValue do prefabrykatu
        TreasureValue treasureValue = spawned.AddComponent<TreasureValue>();

        // Przypisujemy kategoriê i iloœæ zasobów
        string resourceCategory = categoryTexts[selectedCategoryIndex].text;
        treasureValue.category = resourceCategory;
        treasureValue.amount = (int)resourceAmount;

        Debug.Log("Prefab zespawnowany na Y = " + spawnPos.y + " z kategori¹: " + resourceCategory + " i iloœci¹: " + resourceAmount);
    }

    public void SupplyTrash()
    {
        Debug.Log($"[START] Aktualna iloœæ Trash przed sumowaniem: {trashAmount}");

        int totalTrashAmount = 0;

        for (int i = 0; i < categoryTexts.Length; i++)
        {
            int currentAmount = int.Parse(countTexts[i].text);
            Debug.Log($"[Slot {i + 1}] Kategoria: {categoryTexts[i].text}, Iloœæ: {currentAmount}");

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
            Debug.Log($"Sumowano {totalTrashAmount} zasobów do Trash. Ca³kowita iloœæ Trash: {trashAmount}");
        }
        else
        {
            Debug.Log("Brak zasobów do sumowania w slotach.");
        }
    }

    private void RefineTrash()
    {
        float currentTrashAmount = int.Parse(trashCountText.text);

        if (currentTrashAmount >= trashResourceRequired)
        {
            // Sprawdzamy, czy mo¿emy zespawnowaæ obiekt (czy nie ma kolizji)
            if (!IsSpawnPointBlocked())
            {
                currentTrashAmount -= trashResourceRequired;
                trashCountText.text = currentTrashAmount.ToString();

                // Spawnowanie Trash
                SpawnPrefab(isTrash: true);
            }
            else
            {
                Debug.Log("Nie mo¿na zespawnowaæ – kolizja z obiektem.");
            }
        }
        else
        {
            Debug.Log("Za ma³o zasobów do przetworzenia trash! Masz tylko " + currentTrashAmount + ", a potrzeba " + trashResourceRequired);
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

            // Sprawdzamy, czy mo¿na dodaæ zasób do istniej¹cego slotu
            for (int i = 0; i < categoryTexts.Length; i++)
            {
                if (categoryTexts[i].text == resourceCategory)
                {
                    int currentCount = int.Parse(countTexts[i].text);

                    // Obliczamy now¹ sumê zasobów, ale upewniamy siê, ¿e nie przekroczy maksymalnej dopuszczalnej wartoœci
                    int newCount = currentCount + resourceCount;

                    // Obliczamy, ile zasobów jeszcze mo¿na dodaæ do tego slotu
                    int maxAddable = (int)maxResourcePerSlot - currentCount;

                    // Jeœli suma zasobów przekroczy maksymalny limit, dodajemy tylko brakuj¹c¹ iloœæ
                    if (newCount <= (int)maxResourcePerSlot)
                    {
                        countTexts[i].text = newCount.ToString();
                        addedToExistingSlot = true;

                        // Odejmujemy od ekwipunku gracza tylko tyle, ile brakowa³o do maksymalnej wartoœci w Refinerze
                        item.GetComponent<TreasureResources>().resourceCategories[0].resourceCount -= resourceCount;
                    }
                    else
                    {
                        countTexts[i].text = maxResourcePerSlot.ToString();
                        addedToExistingSlot = true;

                        // Odejmujemy tylko brakuj¹c¹ iloœæ zasobów z ekwipunku
                        item.GetComponent<TreasureResources>().resourceCategories[0].resourceCount -= maxAddable;
                        Debug.Log("Dodano maksymaln¹ iloœæ zasobów do slotu. Nadmiar przedmiotu zosta³ usuniêty z ekwipunku.");
                    }

                    break;
                }
            }

            // Jeœli nie znaleziono istniej¹cego slotu, spróbuj dodaæ nowy slot tylko, gdy nie ma ju¿ slotu dla tej kategorii
            if (!addedToExistingSlot)
            {
                bool categoryExists = false;

                // Sprawdzamy, czy ju¿ istnieje slot z t¹ kategori¹
                for (int i = 0; i < categoryTexts.Length; i++)
                {
                    if (categoryTexts[i].text == resourceCategory)
                    {
                        categoryExists = true;
                        break;
                    }
                }

                // Jeœli kategoria nie istnieje, spróbuj dodaæ nowy slot
                if (!categoryExists)
                {
                    for (int i = 0; i < categoryTexts.Length; i++)
                    {
                        if (categoryTexts[i].text == "-" && countTexts[i].text == "0")
                        {
                            // Sprawdzamy, czy przedmiot nie przekroczy dopuszczalnej wartoœci w nowym slocie
                            int newCount = Mathf.Min(resourceCount, (int)maxResourcePerSlot);
                            if (newCount <= (int)maxResourcePerSlot)
                            {
                                categoryTexts[i].text = resourceCategory;
                                countTexts[i].text = newCount.ToString();

                                // Odejmujemy tyle zasobów z ekwipunku, ile zosta³o dodane do nowego slotu
                                item.GetComponent<TreasureResources>().resourceCategories[0].resourceCount -= newCount;
                            }
                            else
                            {
                                Debug.Log("Przekroczono dozwolony limit zasobów w nowym slocie. Przedmiot nie zosta³ dodany.");
                            }
                            break;
                        }
                    }
                }
                else
                {
                    Debug.Log("Slot z t¹ kategori¹ zasobów ju¿ istnieje. Nowy slot nie zostanie dodany.");
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

    // Funkcja sprawdzaj¹ca, czy spawn point jest zablokowany
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
            categoryTexts[i].text = "-";  // Resetujemy kategoriê
            countTexts[i].text = "0";     // Resetujemy iloœæ
        }

        Debug.Log("Wszystkie sloty zosta³y zresetowane.");
    }
}
