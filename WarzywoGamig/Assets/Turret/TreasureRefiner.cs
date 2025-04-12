using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TreasureRefiner : MonoBehaviour
{
    public Inventory inventory;
    public InventoryUI inventoryUI;

    private bool isRefining = false;
    public bool toDestroy = false;

    public TextMeshProUGUI[] categoryTexts;
    public TextMeshProUGUI[] countTexts;
    public TextMeshProUGUI trashCategoryText; // Nowy tekst dla kategorii trash
    public TextMeshProUGUI trashCountText; // Nowy tekst dla iloœci trash
    public TextMeshProUGUI selectedCategoryText; // Nowy tekst UI pokazuj¹cy aktualnie wybran¹ kategoriê
    public TextMeshProUGUI selectedCountText;

    public GameObject prevCategoryButton;
    public GameObject nextCategoryButton;

    public GameObject refineButton; // 5-ty Cube
    public GameObject prefabToSpawn;
    public Transform spawnPoint;
    public float spawnYPosition = 2f; // ustawiasz dok³adne Y w inspektorze
    public float refineAmount = 10;
    public float maxResourcePerSlot = 50f;
    private float trashAmount = 0;
    public float trashResourceRequired = 10f; // Wymagana iloœæ zasobów na trash
    public float trashMaxAmount = 100f;

    // Nowe zmienne dla supplyTrash i refineTrash
    public GameObject supplyTrashButton;
    public GameObject refineTrashButton;

    private int selectedCategoryIndex;
    private Color defaultColor;
    public Color highlightColor = Color.green;

    private void Start()
    {
        InitializeSlots();

        selectedCategoryIndex = FindNextValidCategoryIndexLoop(0, 1);
        if (selectedCategoryIndex != -1)
        {
            selectedCategoryText.text = selectedCategoryIndex == categoryTexts.Length ? "Trash" : categoryTexts[selectedCategoryIndex].text;
        }
        else
        {
            selectedCategoryText.text = "- - -";
            selectedCountText.text = "0";
        }

        // Sprawdzanie, czy jesteœmy w scenie Home
        UpdateButtonStates();

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
                if (hit.collider.gameObject == prevCategoryButton)
                {
                    SwitchCategory(-1); // Prze³¹cz na poprzedni¹ kategoriê
                }

                if (hit.collider.gameObject == nextCategoryButton)
                {
                    SwitchCategory(1); // Prze³¹cz na nastêpn¹ kategoriê
                }

                if (hit.collider.gameObject == refineButton)
                {
                    RefineResources();
                }

                if (hit.collider.gameObject == supplyTrashButton)
                {
                    SupplyTrash();
                }

                if (hit.collider.gameObject == refineTrashButton)
                {
                    RefineTrash();
                }
            }
        }
    }

    private int FindNextValidCategoryIndexLoop(int startIndex, int direction)
    {
        int total = categoryTexts.Length + (IsHomeScene() ? 1 : 0); // dodajemy Trash jako dodatkowy slot
        int index = startIndex;

        for (int i = 0; i < total; i++)
        {
            index = (index + direction + total) % total;

            // Obs³uga trash slotu
            if (index == categoryTexts.Length && IsHomeScene())
            {
                if (int.Parse(trashCountText.text) > 0)
                    return index;
            }
            else if (index < categoryTexts.Length && categoryTexts[index].text != "-")
            {
                return index;
            }
        }

        return -1; // Nie znaleziono ¿adnej aktywnej kategorii
    }


    private void SwitchCategory(int direction)
    {
        List<int> activeIndexes = new List<int>();

        // Zbieramy indeksy slotów, które maj¹ kategoriê
        for (int i = 0; i < categoryTexts.Length; i++)
        {
            if (categoryTexts[i].text != "-")
                activeIndexes.Add(i);
        }

        // Jeœli jesteœmy w scenie Home – dodajemy Trash jako dodatkowy "slot"
        bool isHome = SceneManager.GetActiveScene().name == "Home";
        if (isHome)
        {
            activeIndexes.Add(categoryTexts.Length); // Trash jako ostatni indeks
        }

        if (activeIndexes.Count == 0)
        {
            selectedCategoryIndex = -1;
            selectedCategoryText.text = "- - -";
            selectedCountText.text = "0";
            return;
        }

        // Znajdujemy aktualny indeks w liœcie aktywnych
        int currentIndexInActive = activeIndexes.IndexOf(selectedCategoryIndex);

        // Jeœli obecny index nie jest aktywny (np. reset kategorii), zacznij od pocz¹tku
        if (currentIndexInActive == -1)
            currentIndexInActive = 0;

        // Przeskakujemy
        currentIndexInActive += direction;

        // Zapêtlenie
        if (currentIndexInActive < 0)
            currentIndexInActive = activeIndexes.Count - 1;
        else if (currentIndexInActive >= activeIndexes.Count)
            currentIndexInActive = 0;

        // Ustaw nowy index
        selectedCategoryIndex = activeIndexes[currentIndexInActive];

        // Ustaw teksty UI
        if (selectedCategoryIndex == categoryTexts.Length && isHome) // Trash
        {
            selectedCategoryText.text = "Trash";
            selectedCountText.text = trashAmount.ToString();
        }
        else
        {
            selectedCategoryText.text = categoryTexts[selectedCategoryIndex].text;
            selectedCountText.text = countTexts[selectedCategoryIndex].text;
        }

        Debug.Log($"Wybrano kategoriê: {selectedCategoryText.text}, Iloœæ: {selectedCountText.text}");
    }

    public void RefreshSelectedCategoryUI()
    {
        // Jeœli nic nie jest wybrane
        if (selectedCategoryIndex == -1)
        {
            selectedCategoryText.text = "- - -";
            selectedCountText.text = "0";
            return;
        }

        // Jeœli wybrany jest Trash
        if (selectedCategoryIndex == categoryTexts.Length && IsHomeScene())
        {
            selectedCategoryText.text = "Trash";
            selectedCountText.text = trashAmount.ToString();
            return;
        }

        // Sprawdzenie poprawnoœci indeksu
        if (selectedCategoryIndex >= 0 && selectedCategoryIndex < categoryTexts.Length)
        {
            selectedCategoryText.text = categoryTexts[selectedCategoryIndex].text;
            selectedCountText.text = countTexts[selectedCategoryIndex].text;
        }
        else
        {
            selectedCategoryText.text = "- - -";
            selectedCountText.text = "0";
        }
    }

    private bool IsHomeScene()
    {
        return SceneManager.GetActiveScene().name == "Home";
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Aktualizujemy stan przycisków po zmianie sceny
        UpdateButtonStates();

        // Sprawdzamy, czy kategoria zosta³a wybrana i czy jest dostêpna
        if (selectedCategoryIndex != -1 && selectedCategoryIndex < categoryTexts.Length)
        {
            selectedCategoryText.text = categoryTexts[selectedCategoryIndex].text;

            // Aktualizowanie iloœci wybranej kategorii
            int currentAmount = int.Parse(countTexts[selectedCategoryIndex].text);
            selectedCountText.text = currentAmount.ToString();
        }
        else
        {
            // Jeœli ¿adna kategoria nie zosta³a wybrana, ustawiamy "0"
            selectedCategoryText.text = "- - -";
            selectedCountText.text = "0";
        }
    }


    // Funkcja do aktualizacji stanu przycisków na podstawie sceny
    private void UpdateButtonStates()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Sprawdzenie czy referencje nie s¹ ju¿ zniszczone
        if (trashCategoryText == null || trashCountText == null)
        {
            Debug.LogWarning("trashCategoryText lub trashCountText s¹ null - prawdopodobnie scena siê zmieni³a.");
            return;
        }

        if (currentScene == "Home")
        {
            trashCategoryText.gameObject.SetActive(true);
            trashCountText.gameObject.SetActive(true);
        }
        else
        {
            trashCategoryText.gameObject.SetActive(false);
            trashCountText.gameObject.SetActive(false);
        }

        // W³¹cz/wy³¹cz przyciski prze³¹czania kategorii
        if (categoryTexts.Length > 0)
        {
            prevCategoryButton.SetActive(true);
            nextCategoryButton.SetActive(true);
        }
        else
        {
            prevCategoryButton.SetActive(false);
            nextCategoryButton.SetActive(false);
        }
    }

    private void RefineResources()
    {
        if (selectedCategoryIndex == -1)
        {
            Debug.Log("Wybierz kategoriê zanim przetworzysz zasoby!");
            return;
        }

        // Jeœli wybrany slot to trash
        if (selectedCategoryIndex == categoryTexts.Length && IsHomeScene())
        {
            RefineTrash();
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
                RefreshSelectedCategoryUI();
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
        if (selectedCategoryIndex < 0 || selectedCategoryIndex > categoryTexts.Length)
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

        // Dodanie LootColliderController
        Collider spawnedCollider = spawned.GetComponent<Collider>();
        if (spawnedCollider != null)
        {
            LootColliderController colliderController = spawned.AddComponent<LootColliderController>();
            colliderController.Initialize(spawnedCollider);
        }
        else
        {
            Debug.LogWarning(" Brak colliderea w zespawnowanym obiekcie!");
        }

        // Dodajemy skrypt TreasureValue do prefabrykatu
        TreasureValue treasureValue = spawned.AddComponent<TreasureValue>();

        // Przypisujemy kategoriê i iloœæ zasobów
        string resourceCategory;

        if (isTrash && selectedCategoryIndex == categoryTexts.Length)
        {
            resourceCategory = "Trash";
        }
        else
        {
            resourceCategory = categoryTexts[selectedCategoryIndex].text;
        }

        treasureValue.category = resourceCategory;
        treasureValue.amount = (int)resourceAmount;

        // Nadanie tagu "Loot"
        spawned.tag = "Loot";

        Debug.Log("Prefab zespawnowany na Y = " + spawnPos.y + " z kategori¹: " + resourceCategory + " i iloœci¹: " + resourceAmount);
    }

    public void SupplyTrash()
    {
        if (!IsHomeScene())
        {
            Debug.Log("SupplyTrash dostêpne tylko w scenie Home.");
            return;
        }

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
                categoryTexts[i].text = "-";
            }
        }

        if (totalTrashAmount > 0)
        {
            // Sprawdzamy, czy nie przekroczyliœmy maksymalnej iloœci Trash
            if (trashAmount + totalTrashAmount <= trashMaxAmount)
            {
                trashAmount += totalTrashAmount;
                trashCountText.text = trashAmount.ToString();
                Debug.Log($"Sumowano {totalTrashAmount} zasobów do Trash. Ca³kowita iloœæ Trash: {trashAmount}");
            }
            else
            {
                // Jeœli przekroczyliœmy limit, dodajemy tylko do maksymalnej wartoœci
                float excessTrash = (trashAmount + totalTrashAmount) - trashMaxAmount;
                trashAmount = trashMaxAmount;
                trashCountText.text = trashAmount.ToString();
                Debug.Log($"Przekroczono limit! Trash zosta³ ustawiony na maksymaln¹ wartoœæ: {trashAmount}. Nadmiar {excessTrash} zasobów zosta³ zignorowany.");
            }
        }
        else
        {
            Debug.Log("Brak zasobów do sumowania w slotach.");
        }
        RefreshSelectedCategoryUI();
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
                trashAmount = currentTrashAmount; // <- zaktualizuj wewnêtrzn¹ wartoœæ!
                trashCountText.text = currentTrashAmount.ToString();

                // Spawnowanie Trash
                SpawnPrefab(isTrash: true);

                // I DOPIERO TERAZ odœwie¿enie UI:
                RefreshSelectedCategoryUI();
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


    public void RemoveOldestItemFromInventory()
    {
        if (isRefining) return;
        isRefining = true;

        bool resourcesAdded = false;

        // Przechodzimy przez wszystkie przedmioty w kolejnoœci chronologicznej
        for (int i = 0; i < inventory.items.Count; i++)
        {
            GameObject itemToRemove = inventory.items[i];
            InteractableItem interactableItem = itemToRemove.GetComponent<InteractableItem>();

            if (interactableItem != null)
            {
                // Próbujemy dodaæ zasoby z tego przedmiotu
                UpdateTreasureRefinerSlots(interactableItem, ref resourcesAdded);

                if (resourcesAdded)
                {
                    // Jeœli uda³o siê dodaæ, usuwamy przedmiot i koñczymy pêtlê
                    inventory.items.RemoveAt(i);
                    Destroy(itemToRemove);

                    if (inventoryUI != null)
                    {
                        inventoryUI.UpdateInventoryUI(inventory.weapons, inventory.items);
                    }
                    break;
                }
            }
        }

        // Jeœli ¿aden przedmiot nie pasowa³
        if (!resourcesAdded)
        {
            Debug.Log("Nie mo¿na dodaæ zasobów. Wszystkie sloty przekroczy³yby max");
        }

        isRefining = false;
    }

    public void UpdateTreasureRefinerSlots(InteractableItem item, ref bool resourcesAdded)
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

                    // Jeœli suma zasobów przekroczy maksymalny limit, nie dodajemy nic
                    if (newCount <= (int)maxResourcePerSlot)
                    {
                        countTexts[i].text = newCount.ToString();
                        addedToExistingSlot = true;
                        resourcesAdded = true;

                        // Odejmujemy od ekwipunku gracza tylko tyle, ile brakowa³o do maksymalnej wartoœci w Refinerze
                        item.GetComponent<TreasureResources>().resourceCategories[0].resourceCount -= resourceCount;
                    }
                    else
                    {
                        // Jeœli suma zasobów przekroczy limit, po prostu nie dodajemy
                        countTexts[i].text = countTexts[i].text; // Nie zmienia tekstu w ogóle
                        addedToExistingSlot = false;

                        //GÓWNO
                        //// Nie dodajemy ¿adnych zasobów, tylko usuwamy nadmiar z ekwipunku gracza
                        //item.GetComponent<TreasureResources>().resourceCategories[0].resourceCount -= (newCount - (int)maxResourcePerSlot);
                        //GÓWNO
                        Debug.Log("Zasoby nie zosta³y dodane. Przekroczono maksymalny limit.");
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

                                // Flaga wskazuj¹ca, ¿e zasoby zosta³y dodane
                                resourcesAdded = true;
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

        //Debug.Log("Wszystkie sloty zosta³y zresetowane.");
    }
}
