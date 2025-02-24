using UnityEngine;

public class GridManager : MonoBehaviour
{
    public float gridSize = 1f; // Rozmiar siatki
    public float tileSpacing = 0.1f; // Odstêp miêdzy kafelkami
    public Transform gridArea; // Obszar siatki
    public GameObject gridTilePrefab; // Prefab kafelka siatki
    public GameObject objectPreviewPrefab; // Prefab podgl¹du budowy

    private bool isBuildingMode = false; // Tryb budowy w³¹czony/wy³¹czony
    private GameObject previewObject; // Obiekt podgl¹du
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
        if (Input.GetKeyDown(KeyCode.B))
        {
            isBuildingMode = !isBuildingMode;
            ToggleGridVisibility(isBuildingMode);

            if (isBuildingMode)
                CreatePreviewObject();
            else
                DestroyPreviewObject();
        }

        if (isBuildingMode && previewObject != null)
        {
            Vector3 mousePosition = GetMouseWorldPosition();
            previewObject.transform.position = SnapToGrid(mousePosition);

            if (Input.GetMouseButtonDown(0))
                PlaceObject();
        }
    }
    // Snappowanie pozycji do siatki uwzglêdniaj¹c tileSpacing
    public Vector3 SnapToGrid(Vector3 position)
    {
        // Uwzglêdniamy tileSpacing przy obliczeniach pozycji
        float snappedX = Mathf.Floor((position.x - gridArea.position.x + (gridSize / 2) + (tileSpacing / 2)) / (gridSize + tileSpacing)) * (gridSize + tileSpacing) + gridArea.position.x;
        float snappedZ = Mathf.Floor((position.z - gridArea.position.z + (gridSize / 2) + (tileSpacing / 2)) / (gridSize + tileSpacing)) * (gridSize + tileSpacing) + gridArea.position.z;

        // Ustawiamy wysokoœæ na poziomie siatki
        float gridHeight = gridArea.position.y;

        // Zwracamy now¹ skorygowan¹ pozycjê
        return new Vector3(snappedX, gridHeight, snappedZ);
    }

    // Pobiera pozycjê myszy w œwiecie
    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            return hit.point;
        }
        return Vector3.zero;
    }

    // Tworzy podgl¹d budowanego obiektu
    private void CreatePreviewObject()
    {
        if (objectPreviewPrefab == null) return;

        previewObject = Instantiate(objectPreviewPrefab);
        DisableColliders(previewObject);
    }

    // Usuwa podgl¹d obiektu
    private void DestroyPreviewObject()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
        }
    }

    // Umieszcza obiekt na siatce
    private void PlaceObject()
    {
        if (previewObject != null)
        {
            Instantiate(objectPreviewPrefab, previewObject.transform.position, Quaternion.identity);
        }
    }

    // Wy³¹cza wszystkie collidery w obiekcie i jego childach
    private void DisableColliders(GameObject obj)
    {
        Collider[] colliders = obj.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }
    }

    // Tworzenie siatki z bia³ymi kafelkami
    private void CreateGrid()
    {
        if (gridTilePrefab == null) return;

        int gridWidth = Mathf.FloorToInt(gridAreaWidth / (gridSize + tileSpacing));
        int gridHeight = Mathf.FloorToInt(gridAreaHeight / (gridSize + tileSpacing));

        Vector3 startPosition = gridArea.position - new Vector3(gridAreaWidth / 2, 0, gridAreaHeight / 2);

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector3 tilePosition = new Vector3(x * (gridSize + tileSpacing), 0, z * (gridSize + tileSpacing)) + startPosition;
                GameObject tile = Instantiate(gridTilePrefab, tilePosition, Quaternion.identity);
                tile.transform.parent = gridArea;
                tile.SetActive(false);

                // Ustawienie koloru kafelka na bia³y
                Renderer tileRenderer = tile.GetComponent<Renderer>();
                if (tileRenderer != null)
                {
                    tileRenderer.material.color = Color.white;
                }
            }
        }
    }

    // W³¹czanie/wy³¹czanie widocznoœci siatki
    private void ToggleGridVisibility(bool isVisible)
    {
        foreach (Transform child in gridArea)
        {
            child.gameObject.SetActive(isVisible);
        }
    }
}