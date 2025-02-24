using UnityEngine;
using System.Collections.Generic;  // Dodajemy przestrzeñ nazw dla list

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
    private HashSet<Vector3> occupiedTiles; // Zbiór zajêtych kafelków (unikanie duplikatów)

    void Start()
    {
        gridAreaWidth = gridArea.localScale.x;
        gridAreaHeight = gridArea.localScale.z;
        occupiedTiles = new HashSet<Vector3>(); // Inicjalizujemy zbiór
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

    // Snappowanie pozycji do siatki uwzglêdniaj¹c tileSpacing, omijaj¹c zajête miejsca
    public Vector3 SnapToGrid(Vector3 position)
    {
        float snappedX = Mathf.Floor((position.x - gridArea.position.x + (gridSize / 2) + (tileSpacing / 2)) / (gridSize + tileSpacing)) * (gridSize + tileSpacing) + gridArea.position.x;
        float snappedZ = Mathf.Floor((position.z - gridArea.position.z + (gridSize / 2) + (tileSpacing / 2)) / (gridSize + tileSpacing)) * (gridSize + tileSpacing) + gridArea.position.z;

        Vector3 snappedPosition = new Vector3(snappedX, gridArea.position.y, snappedZ);

        // Sprawdzanie, czy kafelek jest zajêty
        if (occupiedTiles.Contains(snappedPosition))
        {
            // Jeœli zajêty, przesuñ obiekt w inne miejsce
            snappedPosition = GetNextAvailablePosition(snappedPosition);
        }

        return snappedPosition;
    }

    // Zwraca kolejn¹ dostêpn¹ pozycjê na siatce
    private Vector3 GetNextAvailablePosition(Vector3 position)
    {
        Vector3 offset = new Vector3(gridSize + tileSpacing, 0, 0); // Przesuniêcie do nastêpnej pozycji
        Vector3 newPosition = position;

        // Sprawdzamy kolejno ró¿ne pozycje wokó³ aktualnej
        for (int i = 0; i < 10; i++) // Ograniczamy liczbê prób
        {
            newPosition += offset;
            if (!occupiedTiles.Contains(newPosition))
            {
                break;
            }
        }

        return newPosition;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            return hit.point;
        }
        return Vector3.zero;
    }

    private void CreatePreviewObject()
    {
        if (objectPreviewPrefab == null) return;

        previewObject = Instantiate(objectPreviewPrefab);
        DisableColliders(previewObject);
    }

    private void DestroyPreviewObject()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
        }
    }

    private void PlaceObject()
    {
        if (previewObject != null)
        {
            Vector3 placementPosition = previewObject.transform.position;
            Instantiate(objectPreviewPrefab, placementPosition, Quaternion.identity);

            // Zajmujemy kafelek, na którym postawiono obiekt
            occupiedTiles.Add(placementPosition);
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

        Vector3 startPosition = gridArea.position - new Vector3(gridAreaWidth / 2, 0, gridAreaHeight / 2);

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
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
}
