using System.Collections;
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
    public TextMeshProUGUI trashCountText; // Nowy tekst dla ilo�ci trash
    public TextMeshProUGUI selectedCategoryText; // Nowy tekst UI pokazuj�cy aktualnie wybran� kategori�
    public TextMeshProUGUI selectedCountText;
    public TextMeshProUGUI refineCostText;        // tekst obok kategorii
    public TextMeshProUGUI trashRefineCostText;   // tekst obok Trash

    public GameObject prevCategoryButton;
    public GameObject nextCategoryButton;

    public GameObject refineButton;
    public float maxInteractionDistance = 5f; // Maksymalny zasi�g interakcji z przyciskami

    public GameObject prefabToSpawn;
    public Transform spawnPoint;
    public GameObject door;
    public float doorDistance = 3;
    public float doorMoveDuration = 1f;  // Pr�dko�� ruchu drzwi
    public Vector3 doorStartingPosition; // R�cznie ustawiona pocz�tkowa pozycja drzwi

    public float spawnYPosition = 2f; // ustawiasz dok�adne Y w inspektorze
    public float refineAmount = 10;
    public float maxResourcePerSlot = 50f;
    private float trashAmount = 0;
    public float trashResourceRequired = 10f; // Wymagana ilo�� zasob�w na trash
    public float trashMaxAmount = 100f;
    public float spawnDelay = 5;

    // Nowe zmienne dla supplyTrash i refineTrash
    public GameObject supplyTrashButton;
    public GameObject refineTrashButton;

    private bool isProcessing;

    private int selectedCategoryIndex;

    // Lista wszystkich obiekt�w, kt�re posiadaj� PlaySoundOnObject
    private List<PlaySoundOnObject> playSoundObjects = new List<PlaySoundOnObject>();

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

        // Sprawdzanie, czy jeste�my w scenie Home
        UpdateButtonStates();

        SceneManager.sceneLoaded += OnSceneLoaded;

        trashAmount = 0;
        trashCountText.text = "0";

        if (refineCostText != null)
            refineCostText.text = "/" + refineAmount.ToString();

        if (trashRefineCostText != null)
            trashRefineCostText.text = "/" + trashResourceRequired.ToString();

        // Znajd� wszystkie obiekty posiadaj�ce PlaySoundOnObject i dodaj do listy
        playSoundObjects.AddRange(Object.FindObjectsOfType<PlaySoundOnObject>());
    }

    void Update()
    {
        HandleMouseClick();

        // Sprawdzenie, czy spawnPoint nie ma ju� dzieci
        if (spawnPoint.childCount == 0)
        {
            MoveBackDoor(); // Je�li brak dzieci, przesuwamy drzwi w lewo
        }
    }

    private void HandleMouseClick()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, maxInteractionDistance))
            {
                string[] highSounds = { "RefinerHigh1", "RefinerHigh2" };
                string[] lowSounds = { "RefinerLow1", "RefinerLow2" };
                string[] refineSounds = { "Refine1", "Refine2", "Refine3", "Refine4" };

                if (hit.collider.gameObject == prevCategoryButton)
                {
                    SwitchCategory(-1);

                    foreach (var playSoundOnObject in playSoundObjects)
                    {
                        if (playSoundOnObject == null) continue;

                        string chosen = highSounds[Random.Range(0, highSounds.Length)];
                        playSoundOnObject.PlaySound(chosen, 0.5f, false);

                        playSoundOnObject.PlaySound("RefinerBeep", 0.3f, false);
                    }
                }

                if (hit.collider.gameObject == nextCategoryButton)
                {
                    SwitchCategory(1);

                    foreach (var playSoundOnObject in playSoundObjects)
                    {
                        if (playSoundOnObject == null) continue;

                        string chosen = highSounds[Random.Range(0, highSounds.Length)];
                        playSoundOnObject.PlaySound(chosen, 0.5f, false);

                        playSoundOnObject.PlaySound("RefinerBeep", 0.3f, false);
                    }
                }

                if (hit.collider.gameObject == refineButton)
                {
                    if (!IsSpawnPointBlocked() && !isProcessing)
                    {
                        RefineResources();

                        foreach (var playSoundOnObject in playSoundObjects)
                        {
                            if (playSoundOnObject == null) continue;

                            string chosen = lowSounds[Random.Range(0, lowSounds.Length)];
                            playSoundOnObject.PlaySound(chosen, 0.5f, false);
                        }

                        foreach (var playSoundOnObject in playSoundObjects)
                        {
                            if (playSoundOnObject == null) continue;

                            string chosen = refineSounds[Random.Range(0, refineSounds.Length)];
                            playSoundOnObject.PlaySound(chosen, 0.5f, false);
                        }
                    }
                    else
                    {
                        foreach (var playSoundOnObject in playSoundObjects)
                        {
                            if (playSoundOnObject == null) continue;

                            playSoundOnObject.PlaySound("RefinerError", 0.2f, false);
                        }
                    }
                }

                if (hit.collider.gameObject == supplyTrashButton)
                {
                    if (IsHomeScene())
                    {    
                        SupplyTrash();

                        foreach (var playSoundOnObject in playSoundObjects)
                        {
                            if (playSoundOnObject == null) continue;

                            string chosen = lowSounds[Random.Range(0, lowSounds.Length)];
                            playSoundOnObject.PlaySound(chosen, 0.5f, false);
                        }
                    }
                    else
                    {
                        foreach (var playSoundOnObject in playSoundObjects)
                        {
                            if (playSoundOnObject == null) continue;

                            playSoundOnObject.PlaySound("RefinerError", 0.2f, false);
                        }
                    }
                }

                if (hit.collider.gameObject == refineTrashButton)
                {
                    RefineTrash();

                    //foreach (var playSoundOnObject in playSoundObjects)
                    //{
                    //    if (playSoundOnObject == null) continue;

                    //    string chosen = lowSounds[Random.Range(0, lowSounds.Length)];
                    //    playSoundOnObject.PlaySound(chosen, 0.5f, false);
                    //}
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

            // Obs�uga trash slotu
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

        return -1; // Nie znaleziono �adnej aktywnej kategorii
    }


    private void SwitchCategory(int direction)
    {
        List<int> activeIndexes = new List<int>();

        // Zbieramy indeksy slot�w, kt�re maj� kategori�
        for (int i = 0; i < categoryTexts.Length; i++)
        {
            if (categoryTexts[i].text != "-")
                activeIndexes.Add(i);
        }

        // Je�li jeste�my w scenie Home � dodajemy Trash jako dodatkowy "slot"
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

        // Znajdujemy aktualny indeks w li�cie aktywnych
        int currentIndexInActive = activeIndexes.IndexOf(selectedCategoryIndex);

        // Je�li obecny index nie jest aktywny (np. reset kategorii), zacznij od pocz�tku
        if (currentIndexInActive == -1)
            currentIndexInActive = 0;

        // Przeskakujemy
        currentIndexInActive += direction;

        // Zap�tlenie
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

            if (refineCostText != null)
                refineCostText.text = "/" + trashResourceRequired.ToString();
        }
        else
        {
            selectedCategoryText.text = categoryTexts[selectedCategoryIndex].text;
            selectedCountText.text = countTexts[selectedCategoryIndex].text;

            if (refineCostText != null)
                refineCostText.text = "/" + refineAmount.ToString();
        }

        Debug.Log($"Wybrano kategori�: {selectedCategoryText.text}, Ilo��: {selectedCountText.text}");
    }

    public void RefreshSelectedCategoryUI()
    {
        // Je�li nic nie jest wybrane
        if (selectedCategoryIndex == -1)
        {
            selectedCategoryText.text = "- - -";
            selectedCountText.text = "0";

            if (refineCostText != null)
                refineCostText.text = "/-";

            return;
        }

        // Je�li wybrany jest Trash
        if (selectedCategoryIndex == categoryTexts.Length && IsHomeScene())
        {
            selectedCategoryText.text = "Trash";
            selectedCountText.text = trashAmount.ToString();

            if (refineCostText != null)
                refineCostText.text = "/" + trashResourceRequired.ToString();

            return;
        }

        // Sprawdzenie poprawno�ci indeksu
        if (selectedCategoryIndex >= 0 && selectedCategoryIndex < categoryTexts.Length)
        {
            selectedCategoryText.text = categoryTexts[selectedCategoryIndex].text;
            selectedCountText.text = countTexts[selectedCategoryIndex].text;

            if (refineCostText != null)
                refineCostText.text = "/" + refineAmount.ToString();
        }
        else
        {
            selectedCategoryText.text = "- - -";
            selectedCountText.text = "0";

            if (refineCostText != null)
                refineCostText.text = "/-";
        }
    }

    private bool IsHomeScene()
    {
        return SceneManager.GetActiveScene().name == "Home";
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Aktualizujemy stan przycisk�w po zmianie sceny
        UpdateButtonStates();

        // Sprawdzamy, czy kategoria zosta�a wybrana i czy jest dost�pna
        if (selectedCategoryIndex != -1 && selectedCategoryIndex < categoryTexts.Length)
        {
            selectedCategoryText.text = categoryTexts[selectedCategoryIndex].text;

            // Aktualizowanie ilo�ci wybranej kategorii
            int currentAmount = int.Parse(countTexts[selectedCategoryIndex].text);
            selectedCountText.text = currentAmount.ToString();
        }
        else
        {
            // Je�li �adna kategoria nie zosta�a wybrana, ustawiamy "0"
            selectedCategoryText.text = "- - -";
            selectedCountText.text = "0";
        }
    }

    private void UpdateButtonStates()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        bool isHome = currentScene == "Home";

        // Sprawdzamy, czy obiekt trashCategoryText istnieje, zanim ustawimy jego widoczno��
        if (trashCategoryText != null && trashCategoryText.gameObject != null)
            trashCategoryText.gameObject.SetActive(isHome);

        // Sprawdzamy, czy obiekt trashCountText istnieje, zanim ustawimy jego widoczno��
        if (trashCountText != null && trashCountText.gameObject != null)
            trashCountText.gameObject.SetActive(isHome);

        // Sprawdzamy, czy obiekt trashRefineCostText istnieje, zanim ustawimy jego widoczno��
        if (trashRefineCostText != null && trashRefineCostText.gameObject != null)
            trashRefineCostText.gameObject.SetActive(isHome);

        // Ustawienie tekstu dla trashRefineCostText, sprawdzaj�c, czy obiekt jest dost�pny
        if (trashRefineCostText != null)
            trashRefineCostText.text = "/" + trashResourceRequired.ToString();

        // Sprawdzamy, czy lista categoryTexts zawiera jakie� elementy przed manipulowaniem przyciskami
        if (categoryTexts.Length > 0)
        {
            if (prevCategoryButton != null && prevCategoryButton.gameObject != null)
                prevCategoryButton.SetActive(true);

            if (nextCategoryButton != null && nextCategoryButton.gameObject != null)
                nextCategoryButton.SetActive(true);
        }
        else
        {
            // Je�li lista categoryTexts jest pusta, wy��czamy przyciski
            if (prevCategoryButton != null && prevCategoryButton.gameObject != null)
                prevCategoryButton.SetActive(false);

            if (nextCategoryButton != null && nextCategoryButton.gameObject != null)
                nextCategoryButton.SetActive(false);
        }
    }

    private void RefineResources()
    {
        if (selectedCategoryIndex == -1)
        {
            Debug.Log("Wybierz kategori� zanim przetworzysz zasoby!");
            return;
        }

        // Je�li wybrany slot to trash
        if (selectedCategoryIndex == categoryTexts.Length && IsHomeScene())
        {
            RefineTrash();
            return;
        }

        float currentAmount = int.Parse(countTexts[selectedCategoryIndex].text);

        if (currentAmount >= refineAmount)
        {
            if (!IsSpawnPointBlocked())
            {
                currentAmount -= refineAmount;
                countTexts[selectedCategoryIndex].text = currentAmount.ToString();

                // Reset slotu je�li pusto
                if (currentAmount == 0)
                {
                    categoryTexts[selectedCategoryIndex].text = "-";
                    countTexts[selectedCategoryIndex].text = "0";
                }

                // Wywo�anie SpawnPrefab z op�nieniem
                StartCoroutine(DelayedSpawn(false));
                RefreshSelectedCategoryUI();
            }
            else
            {
                Debug.Log("Nie mo�na zespawnowa� � kolizja z obiektem!");
            }
        }
        else
        {
            Debug.Log("Za ma�o zasob�w w wybranej kategorii!");
        }
    }

    // Metoda do przesuwania drzwi w prawo
    private void MoveRightDoor()
    {
        // Rozpoczynamy przesuwanie drzwi w prawo, podaj�c czas trwania
        StartCoroutine(MoveDoorCoroutine(doorStartingPosition.x + doorDistance, doorMoveDuration));
    }

    // Metoda do przesuwania drzwi w lewo
    private void MoveBackDoor()
    {
        // Rozpoczynamy przesuwanie drzwi w lewo, podaj�c czas trwania
        StartCoroutine(MoveDoorCoroutine(doorStartingPosition.x, doorMoveDuration));
    }

    // Coroutine do animacji przesuwania drzwi
    private IEnumerator MoveDoorCoroutine(float targetLocalXPosition, float duration)
    {
        float elapsedTime = 0f;
        float startingLocalXPosition = door.transform.localPosition.x;

        // Dop�ki drzwi nie osi�gn� docelowej pozycji lokalnej, kontynuujemy ich przesuwanie
        while (elapsedTime < duration)
        {
            // Obliczamy, jak� cz�� ruchu nale�y wykona� w danym czasie
            float t = elapsedTime / duration;

            // Interpolujemy mi�dzy pocz�tkow� pozycj� a docelow� pozycj� w lokalnych wsp�rz�dnych
            float newX = Mathf.Lerp(startingLocalXPosition, targetLocalXPosition, t);
            door.transform.localPosition = new Vector3(newX, door.transform.localPosition.y, door.transform.localPosition.z);

            elapsedTime += Time.deltaTime; // Aktualizujemy czas, kt�ry up�yn��
            yield return null;
        }

        // Zapewniamy, �e drzwi sko�cz� dok�adnie w docelowej pozycji lokalnej
        door.transform.localPosition = new Vector3(targetLocalXPosition, door.transform.localPosition.y, door.transform.localPosition.z);
    }

    private IEnumerator DelayedSpawn(bool isTrash)
    {
        isProcessing = true;
        yield return new WaitForSeconds(spawnDelay); // Czekaj okre�lony czas
        SpawnPrefab(isTrash); // Wywo�anie metody SpawnPrefab
        isProcessing = false;

        foreach (var playSoundOnObject in playSoundObjects)
        {
            if (playSoundOnObject == null) continue;

            playSoundOnObject.StopSound("Refine1");
            playSoundOnObject.StopSound("Refine2");
            playSoundOnObject.StopSound("Refine3");
            playSoundOnObject.StopSound("Refine4");
        }
    }

    private void SpawnPrefab(bool isTrash = false)
    {
        if (selectedCategoryIndex < 0 || selectedCategoryIndex > categoryTexts.Length)
        {
            Debug.LogError("Nieprawid�owy indeks kategorii: " + selectedCategoryIndex);
            return;
        }

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

        // Przypisujemy kategori� i ilo�� zasob�w
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

        Debug.Log("Prefab zespawnowany na Y = " + spawnPos.y + " z kategori�: " + resourceCategory + " i ilo�ci�: " + resourceAmount);

        MoveRightDoor();
    }

    public void SupplyTrash()
    {
        if (!IsHomeScene())
        {
            Debug.Log("SupplyTrash dost�pne tylko w scenie Home.");
            return;
        }

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
                categoryTexts[i].text = "-";
            }
        }

        if (totalTrashAmount > 0)
        {
            // Sprawdzamy, czy nie przekroczyli�my maksymalnej ilo�ci Trash
            if (trashAmount + totalTrashAmount <= trashMaxAmount)
            {
                trashAmount += totalTrashAmount;
                trashCountText.text = trashAmount.ToString();
                Debug.Log($"Sumowano {totalTrashAmount} zasob�w do Trash. Ca�kowita ilo�� Trash: {trashAmount}");
            }
            else
            {
                // Je�li przekroczyli�my limit, dodajemy tylko do maksymalnej warto�ci
                float excessTrash = (trashAmount + totalTrashAmount) - trashMaxAmount;
                trashAmount = trashMaxAmount;
                trashCountText.text = trashAmount.ToString();
                Debug.Log($"Przekroczono limit! Trash zosta� ustawiony na maksymaln� warto��: {trashAmount}. Nadmiar {excessTrash} zasob�w zosta� zignorowany.");
            }

            SwitchCategory(1); // Prze��cz na nast�pn� kategori�

        }
        else
        {
            Debug.Log("Brak zasob�w do sumowania w slotach.");
        }
        RefreshSelectedCategoryUI();
    }

    private void RefineTrash()
    {
        float currentTrashAmount = int.Parse(trashCountText.text);

        if (currentTrashAmount >= trashResourceRequired)
        {
            // Sprawdzamy, czy mo�emy zespawnowa� obiekt (czy nie ma kolizji)
            if (!IsSpawnPointBlocked())
            {
                currentTrashAmount -= trashResourceRequired;
                trashAmount = currentTrashAmount; // <- zaktualizuj wewn�trzn� warto��!
                trashCountText.text = currentTrashAmount.ToString();

                // Wywo�anie SpawnPrefab z op�nieniem
                StartCoroutine(DelayedSpawn(true));

                // I DOPIERO TERAZ od�wie�enie UI:
                RefreshSelectedCategoryUI();
            }
            else
            {
                Debug.Log("Nie mo�na zespawnowa� � kolizja z obiektem.");
            }
        }
        else
        {
            Debug.Log("Za ma�o zasob�w do przetworzenia trash! Masz tylko " + currentTrashAmount + ", a potrzeba " + trashResourceRequired);
        }
    }


    public void RemoveOldestItemFromInventory()
    {
        if (isRefining) return;
        isRefining = true;

        bool resourcesAdded = false;

        // Przechodzimy przez wszystkie przedmioty w kolejno�ci chronologicznej
        for (int i = 0; i < inventory.items.Count; i++)
        {
            GameObject itemToRemove = inventory.items[i];
            InteractableItem interactableItem = itemToRemove.GetComponent<InteractableItem>();

            if (interactableItem != null)
            {
                // Pr�bujemy doda� zasoby z tego przedmiotu
                UpdateTreasureRefinerSlots(interactableItem, ref resourcesAdded);

                if (resourcesAdded)
                {
                    // Je�li uda�o si� doda�, usuwamy przedmiot i ko�czymy p�tl�
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

        // Je�li �aden przedmiot nie pasowa�
        if (!resourcesAdded)
        {
            Debug.Log("Nie mo�na doda� zasob�w. Wszystkie sloty przekroczy�yby max");
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

            // Sprawdzamy, czy mo�na doda� zas�b do istniej�cego slotu
            for (int i = 0; i < categoryTexts.Length; i++)
            {
                if (categoryTexts[i].text == resourceCategory)
                {
                    int currentCount = int.Parse(countTexts[i].text);

                    // Obliczamy now� sum� zasob�w, ale upewniamy si�, �e nie przekroczy maksymalnej dopuszczalnej warto�ci
                    int newCount = currentCount + resourceCount;

                    // Obliczamy, ile zasob�w jeszcze mo�na doda� do tego slotu
                    int maxAddable = (int)maxResourcePerSlot - currentCount;

                    // Je�li suma zasob�w przekroczy maksymalny limit, nie dodajemy nic
                    if (newCount <= (int)maxResourcePerSlot)
                    {
                        countTexts[i].text = newCount.ToString();
                        addedToExistingSlot = true;
                        resourcesAdded = true;

                        // Odejmujemy od ekwipunku gracza tylko tyle, ile brakowa�o do maksymalnej warto�ci w Refinerze
                        treasureResources.resourceCategories[0].resourceCount -= resourceCount;

                        // Od�wie�amy UI wybranej kategorii
                        if (selectedCategoryIndex == i)
                        {
                            RefreshSelectedCategoryUI();
                        }
                    }
                    else
                    {
                        Debug.Log("Zasoby nie zosta�y dodane. Przekroczono maksymalny limit.");
                    }

                    break;
                }
            }

            // Je�li nie znaleziono istniej�cego slotu, spr�buj doda� nowy slot
            if (!addedToExistingSlot)
            {
                bool categoryExists = false;

                // Sprawdzamy, czy ju� istnieje slot z t� kategori�
                for (int i = 0; i < categoryTexts.Length; i++)
                {
                    if (categoryTexts[i].text == resourceCategory)
                    {
                        categoryExists = true;
                        break;
                    }
                }

                // Je�li kategoria nie istnieje, spr�buj doda� nowy slot
                if (!categoryExists)
                {
                    for (int i = 0; i < categoryTexts.Length; i++)
                    {
                        if (categoryTexts[i].text == "-" && countTexts[i].text == "0")
                        {
                            // Sprawdzamy, czy przedmiot nie przekroczy dopuszczalnej warto�ci w nowym slocie
                            int newCount = Mathf.Min(resourceCount, (int)maxResourcePerSlot);
                            if (newCount <= (int)maxResourcePerSlot)
                            {
                                categoryTexts[i].text = resourceCategory;
                                countTexts[i].text = newCount.ToString();

                                // Odejmujemy tyle zasob�w z ekwipunku, ile zosta�o dodane do nowego slotu
                                treasureResources.resourceCategories[0].resourceCount -= newCount;

                                // Flaga wskazuj�ca, �e zasoby zosta�y dodane
                                resourcesAdded = true;

                                // Je�eli nie by�o jeszcze wybranej kategorii, ustawiamy j�
                                if (selectedCategoryIndex == -1)
                                {
                                    selectedCategoryIndex = i;
                                    RefreshSelectedCategoryUI();
                                }

                                break;
                            }
                            else
                            {
                                Debug.Log("Przekroczono dozwolony limit zasob�w w nowym slocie. Przedmiot nie zosta� dodany.");
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log("Slot z t� kategori� zasob�w ju� istnieje. Nowy slot nie zostanie dodany.");
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

        //Debug.Log("Wszystkie sloty zosta�y zresetowane.");
    }
}
