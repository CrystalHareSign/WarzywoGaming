using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject objectPrefab; // Prefab do podgl¹du i finalnego postawienia
    public float gridSize = 1f; // Rozmiar siatki
    public float tileSpacing = 0.1f; // Odstêp miêdzy kafelkami
    public Transform gridArea; // Obszar siatki
    public GameObject gridTilePrefab; // Prefab kafelka siatki
    public Material whiteMaterial; // Materia³ siatki

    private GameObject previewObject; // Obiekt podgl¹du
    private bool isBuildingMode = false; // Tryb budowy w³¹czony/wy³¹czony
    private GameObject[] gridTiles; // Tablica kafelków
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
        // W³¹cz/wy³¹cz tryb budowy za pomoc¹ B
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
                previewObject = null; // Usuwamy obiekt podgl¹du, gdy tryb budowy jest wy³¹czony
            }
        }

        // Tryb budowy
        if (isBuildingMode && previewObject != null)
        {
            MovePreviewObject();

            if (Input.GetMouseButtonDown(0)) // Klikniêcie lewego przycisku myszy
            {
                PlaceObject();
            }
        }
    }

    // Tworzy obiekt podgl¹du do snappowania na siatce
    void MovePreviewObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 snappedPosition = SnapToGrid(hit.point);
            previewObject.transform.position = snappedPosition;
            previewObject.SetActive(true); // Aktywujemy obiekt podgl¹du

            // Debugowanie
            Debug.Log("Snapped Position: " + snappedPosition);
        }
    }

    void CreatePreviewObject()
    {
        previewObject = Instantiate(objectPrefab);
        previewObject.SetActive(false); // Na pocz¹tku obiekt jest niewidoczny

        // Wy³¹czamy fizykê i collider
        Collider col = previewObject.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Rigidbody rb = previewObject.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true; // Sprawiamy, ¿e nie ma fizyki

        // Upewnij siê, ¿e obiekt podgl¹du nie wp³ywa na gracza
        previewObject.GetComponent<Collider>().enabled = false;
    }

    // Umieszczenie finalnego obiektu w miejscu podgl¹du
    void PlaceObject()
    {
        if (previewObject != null)
        {
            Vector3 placePosition = previewObject.transform.position;
            GameObject placedObject = Instantiate(objectPrefab, placePosition, Quaternion.identity);

            // W³¹czamy fizykê w nowo postawionym obiekcie
            Collider col = placedObject.GetComponent<Collider>();
            if (col != null) col.enabled = true;

            Rigidbody rb = placedObject.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false; // Przywracamy fizykê dla postawionego obiektu
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

    // W³¹czanie/wy³¹czanie widocznoœci siatki
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
