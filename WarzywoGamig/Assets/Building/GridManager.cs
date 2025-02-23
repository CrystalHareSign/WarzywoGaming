using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject objectPrefab; // Prefab do podgl�du i finalnego postawienia
    public float gridSize = 1f; // Rozmiar siatki
    public float tileSpacing = 0.1f; // Odst�p mi�dzy kafelkami
    public Transform gridArea; // Obszar siatki
    public GameObject gridTilePrefab; // Prefab kafelka siatki
    public Material whiteMaterial; // Materia� siatki

    private GameObject previewObject; // Obiekt podgl�du
    private bool isBuildingMode = false; // Tryb budowy w��czony/wy��czony
    private GameObject[] gridTiles; // Tablica kafelk�w
    private float gridAreaWidth;
    private float gridAreaHeight;

    void Start()
    {
        gridAreaWidth = gridArea.localScale.x;
        gridAreaHeight = gridArea.localScale.z;
        CreateGrid();
    }

    void Update()
    {
        // W��cz/wy��cz tryb budowy za pomoc� B
        if (Input.GetKeyDown(KeyCode.B))
        {
            isBuildingMode = !isBuildingMode;
            ToggleGridVisibility(isBuildingMode);

            if (isBuildingMode && previewObject == null)
            {
                CreatePreviewObject();
            }

            if (!isBuildingMode && previewObject != null)
            {
                Destroy(previewObject);
                previewObject = null; // Usuwamy obiekt podgl�du, gdy tryb budowy jest wy��czony
            }
        }

        // Tryb budowy
        if (isBuildingMode && previewObject != null)
        {
            MovePreviewObject();

            if (Input.GetMouseButtonDown(0)) // Klikni�cie lewego przycisku myszy
            {
                PlaceObject();
            }
        }
    }

    // Tworzy obiekt podgl�du do snappowania na siatce
    void MovePreviewObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 snappedPosition = SnapToGrid(hit.point);
            previewObject.transform.position = snappedPosition;
            previewObject.SetActive(true); // Aktywujemy obiekt podgl�du

            // Debugowanie
            Debug.Log("Snapped Position: " + snappedPosition);
        }
    }

    void CreatePreviewObject()
    {
        previewObject = Instantiate(objectPrefab);
        previewObject.SetActive(false); // Na pocz�tku obiekt jest niewidoczny

        // Wy��czamy fizyk� i collider
        Collider col = previewObject.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Rigidbody rb = previewObject.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true; // Sprawiamy, �e nie ma fizyki

        // Upewnij si�, �e obiekt podgl�du nie wp�ywa na gracza
        previewObject.GetComponent<Collider>().enabled = false;
    }

    // Umieszczenie finalnego obiektu w miejscu podgl�du
    void PlaceObject()
    {
        if (previewObject != null)
        {
            Vector3 placePosition = previewObject.transform.position;
            GameObject placedObject = Instantiate(objectPrefab, placePosition, Quaternion.identity);

            // W��czamy fizyk� w nowo postawionym obiekcie
            Collider col = placedObject.GetComponent<Collider>();
            if (col != null) col.enabled = true;

            Rigidbody rb = placedObject.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false; // Przywracamy fizyk� dla postawionego obiektu
        }
    }

    // Snappowanie pozycji do siatki
    Vector3 SnapToGrid(Vector3 position)
    {
        float snappedX = Mathf.Round(position.x / gridSize) * gridSize;
        float snappedZ = Mathf.Round(position.z / gridSize) * gridSize;
        float gridHeight = gridArea.position.y;

        return new Vector3(snappedX, gridHeight, snappedZ);
    }

    // Tworzenie siatki
    private void CreateGrid()
    {
        if (gridTilePrefab == null) return;

        int gridWidth = Mathf.FloorToInt(gridAreaWidth / (gridSize + tileSpacing));
        int gridHeight = Mathf.FloorToInt(gridAreaHeight / (gridSize + tileSpacing));

        gridTiles = new GameObject[gridWidth * gridHeight];

        Vector3 startPosition = gridArea.position - new Vector3(gridAreaWidth / 2, 0, gridAreaHeight / 2);

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector3 tilePosition = new Vector3(x * (gridSize + tileSpacing), 0, z * (gridSize + tileSpacing)) + startPosition;
                int index = x + z * gridWidth;
                gridTiles[index] = Instantiate(gridTilePrefab, tilePosition, Quaternion.identity);
                gridTiles[index].transform.parent = gridArea;
                gridTiles[index].SetActive(false);

                if (whiteMaterial != null)
                {
                    gridTiles[index].GetComponent<Renderer>().material = whiteMaterial;
                }
            }
        }
    }

    // W��czanie/wy��czanie widoczno�ci siatki
    private void ToggleGridVisibility(bool isVisible)
    {
        foreach (GameObject tile in gridTiles)
        {
            tile.SetActive(isVisible);
        }
    }

    // Przypisanie nowego obiektu do budowy
    public void SetObjectPrefab(GameObject newPrefab)
    {
        if (newPrefab != null)
        {
            objectPrefab = newPrefab;
        }
    }
}
