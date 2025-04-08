using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public float buildRange = 5f; // Maksymalny zasi�g budowania
    public float gridSize = 1f; // Rozmiar siatki
    public float tileSpacing = 0.1f; // Odst�p mi�dzy kafelkami
    public Transform gridArea; // Obszar siatki
    public Transform player; // Referencja do gracza
    public Transform LootParent; // Przypisz do niego transform zawieraj�cy obiekty w r�ce gracza
    public GameObject gridTilePrefab; // Prefab kafelka siatki
    public List<GameObject> buildingPrefabs = new List<GameObject>(); // Lista dost�pnych prefab�w
    public bool isBuildingMode = false; // Tryb budowy w��czony/wy��czony
    public float dropHeight = 7f; // Wysoko��, na jakiej loot ma upa��
    public float checkInterval = 2f; // Czas (w sekundach) po kt�rym sprawdzamy kafelki
    private float timeSinceLastCheck = 0f; // Zmienna do liczenia czasu

    private int currentPrefabIndex = 0; // Aktualny indeks prefabrykatu
    private GameObject previewObject; // Obiekt podgl�du
    private float gridAreaWidth;
    private float gridAreaHeight;
    private Dictionary<Vector3, GameObject> occupiedTiles = new Dictionary<Vector3, GameObject>(); // Zbi�r zaj�tych kafelk�w
    public InventoryUI inventoryUI;

    public static GridManager Instance { get; private set; }

    // Lista wszystkich obiekt�w, kt�re posiadaj� PlaySoundOnObject
    private List<PlaySoundOnObject> playSoundObjects = new List<PlaySoundOnObject>();


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Debug.LogWarning("GridManager is a child GameObject. DontDestroyOnLoad will not work.");
            }
        }
    }


    void Start()
    {

        // Znajd� wszystkie obiekty posiadaj�ce PlaySoundOnObject i dodaj do listy
        playSoundObjects.AddRange(Object.FindObjectsOfType<PlaySoundOnObject>());

        gridAreaWidth = gridArea.localScale.x;
        gridAreaHeight = gridArea.localScale.z;
        CreateGrid();
    }

    void Update()
    {
        if (isBuildingMode && previewObject != null)
        {
            Vector3 mousePosition = GetMouseWorldPosition();
            Vector3 snappedPosition = SnapToGrid(mousePosition);

            // Sprawdzenie, czy obiekt snapuje do gridArea
            bool isWithinBounds = IsWithinBounds(snappedPosition);
            previewObject.SetActive(isWithinBounds);

            if (isWithinBounds)
            {
                previewObject.transform.position = snappedPosition;
            }

            if (Input.GetKeyDown(KeyCode.E) && previewObject.activeSelf)
            {
                //Debug.Log("Placing object");
                PlaceObject();
            }
        }

        // Obs�uguje naci�ni�cie Q do wyj�cia z trybu budowania i upuszczenia przedmiotu
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // Je�li gracz ma podniesiony przedmiot Loot
            if (LootParent.childCount > 0)
            {
                GameObject lootItem = LootParent.GetChild(0).gameObject; // Zak�adamy, �e gracz ma tylko jeden przedmiot w r�ce
                //Debug.Log("Dropping loot item");
                DropLootItem(lootItem); // Upuszczamy przedmiot
            }
        }

        timeSinceLastCheck += Time.deltaTime;

        // Sprawdzamy co okre�lony czas
        if (timeSinceLastCheck >= checkInterval)
        {
            CheckTiles();
            timeSinceLastCheck = 0f; // Resetujemy licznik
        }
    }

    private bool IsWithinBounds(Vector3 position)
    {
        float minX = gridArea.position.x - gridArea.localScale.x / 2;
        float maxX = gridArea.position.x + gridArea.localScale.x / 2;
        float minZ = gridArea.position.z - gridArea.localScale.z / 2;
        float maxZ = gridArea.position.z + gridArea.localScale.z / 2;

        bool isWithinBounds = position.x >= minX && position.x <= maxX && position.z >= minZ && position.z <= maxZ;

        //Debug.Log($"Position: {position}, MinX: {minX}, MaxX: {maxX}, MinZ: {minZ}, MaxZ: {maxZ}, IsWithinBounds: {isWithinBounds}");

        return isWithinBounds;
    }

    private void DropLootItem(GameObject lootItem)
    {
        // Pobieramy Collider obiektu Loot
        Collider lootCollider = lootItem.GetComponent<Collider>();
        if (lootCollider == null)
        {
            //Debug.LogWarning("Loot nie ma komponentu Collider.");
            return;
        }

        // Pobieramy Collider obszaru siatki
        Collider gridAreaCollider = gridArea.GetComponent<Collider>();
        if (gridAreaCollider == null)
        {
            //Debug.LogWarning("Siatka nie ma komponentu Collider.");
            return;
        }

        // Ustawiamy docelow� pozycj� przedmiotu na pozycji gracza, ale z okre�lon� wysoko�ci� 'dropHeight' (na osi Y)
        Vector3 dropPosition = new Vector3
        (
            player.transform.position.x, // U�ywamy pozycji gracza w X
            dropHeight, // Ustawiamy Y na dropHeight
            player.transform.position.z  // U�ywamy pozycji gracza w Z
        );

        // Sprawdzamy, czy kolidery si� przecinaj� przy docelowej pozycji
        Vector3 lootSize = lootCollider.bounds.size;
        Collider[] colliders = Physics.OverlapBox(dropPosition, lootSize / 2, lootItem.transform.rotation, LayerMask.GetMask("Default","InteractableItem")); // Zak�adamy, �e u�ywasz warstwy "Default"

        foreach (var collider in colliders)
        {
            if (collider == gridAreaCollider)
            {
                Debug.LogWarning("Nie mo�na upu�ci� przedmiotu wewn�trz obszaru siatki.");
                return;
            }
            if (collider == lootCollider)
            {
                Debug.LogWarning("Nie mo�na upu�ci� przedmiotu wewn�trz innego przedmiotu.");
                return;
            }
        }

        Inventory inventory = Object.FindFirstObjectByType<Inventory>(); // Znajd� skrypt Inventory

        if (inventory != null)
        {
            inventory.isLootBeingDropped = true; // Ustaw flag�, �e loot jest w trakcie upuszczania
        }

        // Usuwamy przedmiot z LootParent, aby go "upu�ci�"
        lootItem.transform.SetParent(null); // Przenosimy przedmiot na �wiat

        // Ustawiamy pozycj� przedmiotu na docelow� pozycj�
        lootItem.transform.position = dropPosition;

        // Ustawiamy pocz�tkow� rotacj� wzgl�dem �wiata (np. ustawiamy na 0, 0, 0)
        lootItem.transform.rotation = Quaternion.identity;

        // Ustawiamy przedmiot na Loot
        lootItem.GetComponent<InteractableItem>().isLoot = true;

        // Usuwamy przedmiot z listy BuildingPrefabs, co automatycznie wy��cza tryb budowania
        buildingPrefabs.Remove(lootItem); // Usuwamy obiekt z listy prefabrykowanych obiekt�w budowy

        // Wy��czamy tryb budowania
        isBuildingMode = false;

        // Usuwamy podgl�d prefabrykowanego obiektu, je�li istnieje
        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
        }

        foreach (var playSoundOnObject in playSoundObjects)
        {
            if (playSoundOnObject == null) continue;

            playSoundOnObject.PlaySound("LootDrop", 0.6f, false);
        }

        // Ukrywamy kafelki (lub inne obiekty zwi�zane z trybem budowania)
        ToggleGridVisibility(false);

        // Sprawdzamy, czy istnieje nieaktywna bro� w Inventory, a je�li tak, to j� aktywujemy
        if (inventory != null && inventory.currentWeaponPrefab != null)
        {
            inventory.currentWeaponPrefab.SetActive(true);
            inventoryUI.UpdateWeaponUI(inventory.currentWeaponPrefab.GetComponent<Gun>());
        }

        // Aktywujemy kafelki pod lootem
        PrefabSize prefabSize = lootItem.GetComponent<PrefabSize>();
        UnmarkTilesAsOccupied(lootItem.transform.position, prefabSize);

        LootColliderController colliderController = lootItem.AddComponent<LootColliderController>();
        colliderController.Initialize(lootCollider);

        //Debug.Log("Przedmiot upuszczony, tryb budowania wy��czony i kafelki ukryte.");
    }
    private void CheckTiles()
    {
        List<Vector3> tilesToActivate = new List<Vector3>();

        // Sprawdzamy ka�dy kafelek w zaj�tych kafelkach
        foreach (var tile in occupiedTiles)
        {
            if (tile.Value == null || !tile.Value.activeSelf)  // Je�li obiekt jest nieaktywny
            {
                tilesToActivate.Add(tile.Key); // Dodajemy kafelek do listy do aktywacji
            }
        }

        // Aktywujemy kafelki, kt�re s� teraz wolne
        foreach (var tilePosition in tilesToActivate)
        {
            occupiedTiles.Remove(tilePosition); // Usuwamy kafelek z zaj�tych
            SetTileActive(tilePosition, true); // Aktywujemy kafelek
        }
    }

    private void SetTileActive(Vector3 position, bool isActive)
    {
        // Logika aktywacji/desaktywacji kafelka, np. zmiana jego widoczno�ci
        GameObject tile = GetTileAtPosition(position);
        if (tile != null)
        {
            tile.SetActive(isActive);
        }
    }

    private GameObject GetTileAtPosition(Vector3 position)
    {
        // Funkcja pomocnicza do znalezienia kafelka na podstawie pozycji
        foreach (Transform child in gridArea)
        {
            if (Vector3.Distance(child.position, position) < gridSize) // Tolerancja odleg�o�ci
            {
                return child.gameObject;
            }
        }
        return null; // Je�li nie znaleziono
    }

    private void CreatePreviewObject()
    {
        if (buildingPrefabs.Count == 0) return;

        previewObject = Instantiate(buildingPrefabs[currentPrefabIndex]);
        previewObject.SetActive(true);  // Ustawienie obiektu jako aktywnego
        DisableColliders(previewObject);  // Wy��czenie kolider�w po ustawieniu na aktywny
    }

    private Vector3 SnapToGrid(Vector3 position)
    {
        if (previewObject == null) return position;

        // Pobieramy rozmiar prefabrykat�w, aby obliczy�, ile kafelk�w zajmuje obiekt
        PrefabSize prefabSize = previewObject.GetComponent<PrefabSize>();
        if (prefabSize == null)
        {
            prefabSize = previewObject.AddComponent<PrefabSize>();
            prefabSize.widthInTiles = 1; // Domy�lna wielko�� (1x1)
            prefabSize.depthInTiles = 1; // Domy�lna wielko�� (1x1)
        }

        // Uwzgl�dniamy odst�py i rozmiar kafelk�w
        float gridSizeWithSpacing = gridSize + tileSpacing;

        // Obliczanie pozycji snapowania
        float snappedX = Mathf.Round((position.x - gridArea.position.x) / gridSizeWithSpacing) * gridSizeWithSpacing + gridArea.position.x;
        float snappedZ = Mathf.Round((position.z - gridArea.position.z) / gridSizeWithSpacing) * gridSizeWithSpacing + gridArea.position.z;

        // Korekta pozycji dla wi�kszych obiekt�w
        if (prefabSize.widthInTiles > 1 || prefabSize.depthInTiles > 1)
        {
            snappedX += (gridSizeWithSpacing * (prefabSize.widthInTiles - 1)) / 2;
            snappedZ += (gridSizeWithSpacing * (prefabSize.depthInTiles - 1)) / 2;
        }

        // Ustawienie pozycji Y na podstawie siatki (je�li masz jak�� wysoko�� na siatce)
        float snappedY = gridArea.position.y;

        // Nowa pozycja obiektu
        Vector3 snappedPosition = new Vector3(snappedX, snappedY, snappedZ);

        // Sprawdzamy, czy ta pozycja jest dost�pna
        if (IsPositionAvailable(snappedPosition, prefabSize) && IsInsideGrid(snappedPosition, prefabSize))
        {
            return snappedPosition;
        }

        // Je�li miejsce jest zaj�te, szukamy kolejnej dost�pnej pozycji
        return GetNextAvailablePosition(snappedPosition, prefabSize);
    }

    private bool IsInsideGrid(Vector3 position, PrefabSize prefabSize)
    {
        // Sprawdzenie, czy obiekt znajduje si� w obr�bie siatki
        float gridAreaMinX = gridArea.position.x - gridAreaWidth / 2;
        float gridAreaMaxX = gridArea.position.x + gridAreaWidth / 2;
        float gridAreaMinZ = gridArea.position.z - gridAreaHeight / 2;
        float gridAreaMaxZ = gridArea.position.z + gridAreaHeight / 2;

        float objectMinX = position.x - prefabSize.widthInTiles * (gridSize + tileSpacing) / 2;
        float objectMaxX = position.x + prefabSize.widthInTiles * (gridSize + tileSpacing) / 2;
        float objectMinZ = position.z - prefabSize.depthInTiles * (gridSize + tileSpacing) / 2;
        float objectMaxZ = position.z + prefabSize.depthInTiles * (gridSize + tileSpacing) / 2;

        return objectMinX >= gridAreaMinX && objectMaxX <= gridAreaMaxX && objectMinZ >= gridAreaMinZ && objectMaxZ <= gridAreaMaxZ;
    }

    private Vector3 GetNextAvailablePosition(Vector3 startPosition, PrefabSize prefabSize)
    {
        // Implementacja spiralnego wyszukiwania dost�pnej pozycji
        int radius = 1;
        while (true)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int z = -radius; z <= radius; z++)
                {
                    Vector3 checkPosition = new Vector3(startPosition.x + x * (gridSize + tileSpacing), startPosition.y, startPosition.z + z * (gridSize + tileSpacing));
                    if (IsPositionAvailable(checkPosition, prefabSize))
                        return checkPosition;
                }
            }
            radius++;
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            // Przypisanie wysoko�ci do pozycji podgl�du
            Vector3 hitPoint = hit.point;
            hitPoint.y = gridArea.position.y;  // Mo�esz dostosowa�, je�li chcesz, aby obiekt by� na innej wysoko�ci
            return hitPoint;
        }
        return Vector3.zero;
    }

    public void PlaceObject()
    {
        // Sprawdzamy, czy obiekt do podgl�du istnieje
        if (previewObject != null)
        {
            // Pobieramy pozycj�, gdzie obiekt ma zosta� postawiony
            Vector3 placementPosition = previewObject.transform.position;

            // Pobieramy rozmiar prefabrykatu
            PrefabSize prefabSize = previewObject.GetComponent<PrefabSize>();

            // Sprawdzamy, czy odleg�o�� od gracza do miejsca budowy jest wystarczaj�ca
            if (Vector3.Distance(player.position, placementPosition) > buildRange)
            {
                Debug.Log("Zbyt daleko od gracza, nie mo�na postawi� obiektu.");
                return;
            }

            // Sprawdzamy, czy miejsce jest dost�pne i czy obiekt zmie�ci si� w obr�bie siatki
            if (IsPositionAvailable(placementPosition, prefabSize) && IsInsideGrid(placementPosition, prefabSize))
            {
                // Tworzymy obiekt w miejscu docelowym
                GameObject buildedObject = Instantiate(buildingPrefabs[currentPrefabIndex], placementPosition, previewObject.transform.rotation);
                buildedObject.SetActive(true);

                foreach (var playSoundOnObject in playSoundObjects)
                {
                    if (playSoundOnObject == null) continue;

                    playSoundOnObject.PlaySound("LootPlace", 0.7f, false);
                }

                // Dodanie LootColliderController do nowego obiektu
                Collider buildedCollider = buildedObject.GetComponent<Collider>();
                if (buildedCollider != null)
                {
                    LootColliderController colliderController = buildedObject.AddComponent<LootColliderController>();
                    colliderController.Initialize(buildedCollider);
                }
                else
                {
                    Debug.LogWarning(" Brak colliderea w postawionym obiekcie!");
                }

                // Usuwamy obiekt podgl�du z listy LootParent w Inventory
                Inventory inventory = Object.FindFirstObjectByType<Inventory>();
                if (inventory != null)
                {
                    inventory.RemoveObjectFromLootParent(previewObject);  // Zmieniono na metod�, kt�ra faktycznie usuwa obiekt z rodzica loot
                }

                // Usuwamy obiekt podgl�du z ziemi (po jego postawieniu)
                Destroy(previewObject);
                previewObject = null;

                // Aktualizujemy s�ownik, oznaczamy obszar zaj�ty przez obiekt
                MarkTilesAsOccupied(placementPosition, prefabSize, buildedObject);

                // Ko�czymy tryb budowania
                isBuildingMode = false;
                ToggleGridVisibility(false);  // Wy��czenie siatki, je�li to konieczne

                // Sprawdzamy, czy istnieje aktywna bro� w Inventory, a je�li tak, to j� dezaktywujemy
                if (inventory != null && inventory.currentWeaponPrefab != null)
                {
                    inventory.currentWeaponPrefab.SetActive(true);
                    inventoryUI.UpdateWeaponUI(inventory.currentWeaponPrefab.GetComponent<Gun>());
                }
            }
            else
            {
                Debug.Log("Miejsce jest zaj�te lub obiekt nie mie�ci si� w obr�bie siatki.");
            }
        }
        else
        {
            Debug.LogWarning("Brak obiektu podgl�du do postawienia.");
        }
    }

    private bool IsPositionAvailable(Vector3 position, PrefabSize prefabSize)
    {
        // Pobieramy pozycj� pocz�tkow� (dolny lewy r�g)
        int startX = Mathf.FloorToInt(position.x);
        int startZ = Mathf.FloorToInt(position.z);

        // Sprawdzamy, czy wszystkie kafelki, kt�re obiekt ma zaj��, s� dost�pne
        for (int x = startX; x < startX + prefabSize.widthInTiles; x++)
        {
            for (int z = startZ; z < startZ + prefabSize.depthInTiles; z++)
            {
                Vector3 tilePosition = new Vector3(x, 0, z);
                if (occupiedTiles.ContainsKey(tilePosition)) // Je�li kafelek jest ju� zaj�ty
                {
                    return false;
                }
            }
        }
        return true; // Miejsce dost�pne
    }

    private void MarkTilesAsOccupied(Vector3 position, PrefabSize prefabSize, GameObject buildedObject)
    {
        // Pobieramy pozycj� pocz�tkow� (dolny lewy r�g)
        int startX = Mathf.FloorToInt(position.x);
        int startZ = Mathf.FloorToInt(position.z);

        // Oznaczamy wszystkie kafelki, kt�re obiekt zajmuje
        for (int x = startX; x < startX + prefabSize.widthInTiles; x++)
        {
            for (int z = startZ; z < startZ + prefabSize.depthInTiles; z++)
            {
                Vector3 tilePosition = new Vector3(x, 0, z);
                if (!occupiedTiles.ContainsKey(tilePosition))  // Sprawdzamy, czy kafelek nie jest ju� zaj�ty
                {
                    occupiedTiles[tilePosition] = buildedObject; // Zapisujemy, �e kafelek jest zaj�ty przez ten obiekt
                    SetTileActive(tilePosition, false); // Deaktywujemy kafelek
                }
            }
        }
    }

    public void UnmarkTilesAsOccupied(Vector3 position, PrefabSize prefabSize)
    {
        // Pobieramy pozycj� pocz�tkow� (dolny lewy r�g)
        int startX = Mathf.FloorToInt(position.x);
        int startZ = Mathf.FloorToInt(position.z);

        // Oznaczamy wszystkie kafelki, kt�re obiekt zajmuje jako wolne
        for (int x = startX; x < startX + prefabSize.widthInTiles; x++)
        {
            for (int z = startZ; z < startZ + prefabSize.depthInTiles; z++)
            {
                Vector3 tilePosition = new Vector3(x, 0, z);
                if (occupiedTiles.ContainsKey(tilePosition))  // Sprawdzamy, czy kafelek jest zaj�ty
                {
                    occupiedTiles.Remove(tilePosition); // Usuwamy kafelek ze zbioru zaj�tych
                    SetTileActive(tilePosition, true); // Aktywujemy kafelek
                }
            }
        }
    }

    private void DisableColliders(GameObject obj)
    {
        Collider[] colliders = obj.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }
    }

    private void CreateGrid()
    {
        if (gridTilePrefab == null) return;

        int gridWidth = Mathf.FloorToInt(gridAreaWidth / (gridSize + tileSpacing));
        int gridHeight = Mathf.FloorToInt(gridAreaHeight / (gridSize + tileSpacing));

        // Startowa pozycja to lewy dolny r�g siatki, ale przesuni�ta o po�ow� d�ugo�ci kafelka w prawo i w g�r�
        Vector3 startPosition = gridArea.position - new Vector3(gridAreaWidth / 2, 0, gridAreaHeight / 2);
        startPosition += new Vector3((gridSize + tileSpacing) / 2, 0, (gridSize + tileSpacing) / 2);

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                // Pozycja kafelka w siatce
                Vector3 tilePosition = new Vector3(x * (gridSize + tileSpacing), 0, z * (gridSize + tileSpacing)) + startPosition;
                GameObject tile = Instantiate(gridTilePrefab, tilePosition, Quaternion.identity);
                tile.transform.parent = gridArea;
                tile.SetActive(false);

                Renderer tileRenderer = tile.GetComponent<Renderer>();
                if (tileRenderer != null)
                {
                    tileRenderer.material.color = Color.white;
                }
            }
        }
    }

    private void ToggleGridVisibility(bool isVisible)
    {
        foreach (Transform child in gridArea)
        {
            child.gameObject.SetActive(isVisible);
        }
    }
    private void CleanupBuildingPrefabs()
    {
        buildingPrefabs.RemoveAll(item => item == null);
    }

    public void AddToBuildingPrefabs(GameObject prefab)
    {
        if (prefab != null && !buildingPrefabs.Contains(prefab))
        {
            buildingPrefabs.Add(prefab);
            CleanupBuildingPrefabs(); // Usuni�cie pustych referencji

            isBuildingMode = true;
            ToggleGridVisibility(isBuildingMode);
            currentPrefabIndex = buildingPrefabs.Count - 1;

            CreatePreviewObject();
        }
    }

    public void RemoveFromBuildingPrefabs(GameObject lootItem)
    {
        if (buildingPrefabs.Contains(lootItem))
        {
            buildingPrefabs.Remove(lootItem);
            CleanupBuildingPrefabs(); // Usuni�cie pustych referencji
            Debug.Log("Usuni�to loot z BuildingPrefabs: " + lootItem.name);
        }
        else
        {
            Debug.LogWarning("Loot nie znajduje si� na li�cie BuildingPrefabs: " + lootItem.name);
        }
    }
}
