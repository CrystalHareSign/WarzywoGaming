using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public float gridSize = 1f; // Rozmiar siatki
    public float tileSpacing = 0.1f; // Odstêp miêdzy kafelkami
    public Transform gridArea; // Obszar siatki
    public GameObject gridTilePrefab; // Prefab kafelka siatki
    public List<GameObject> buildingPrefabs; // Lista dostêpnych prefabów
    private int currentPrefabIndex = 0; // Aktualny indeks prefabrykatu

    private bool isBuildingMode = false; // Tryb budowy w³¹czony/wy³¹czony
    private GameObject previewObject; // Obiekt podgl¹du
    private float gridAreaWidth;
    private float gridAreaHeight;
    private HashSet<Vector3> occupiedTiles; // Zbiór zajêtych kafelków

    void Start()
    {
        gridAreaWidth = gridArea.localScale.x;
        gridAreaHeight = gridArea.localScale.z;
        occupiedTiles = new HashSet<Vector3>();
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

        if (isBuildingMode)
        {
            HandlePrefabSwitching();

            if (previewObject != null)
            {
                Vector3 mousePosition = GetMouseWorldPosition();
                previewObject.transform.position = SnapToGrid(mousePosition);

                if (Input.GetMouseButtonDown(0))
                    PlaceObject();
            }
        }
    }

    private void HandlePrefabSwitching()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            currentPrefabIndex += (scroll > 0) ? 1 : -1;

            if (currentPrefabIndex >= buildingPrefabs.Count)
                currentPrefabIndex = 0;
            else if (currentPrefabIndex < 0)
                currentPrefabIndex = buildingPrefabs.Count - 1;

            UpdatePreviewObject();
        }
    }

    private void UpdatePreviewObject()
    {
        if (previewObject != null)
            Destroy(previewObject);

        CreatePreviewObject();
    }

    private void CreatePreviewObject()
    {
        if (buildingPrefabs.Count == 0) return;

        previewObject = Instantiate(buildingPrefabs[currentPrefabIndex]);
        DisableColliders(previewObject);
    }

    private void DestroyPreviewObject()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
        }
    }

    public Vector3 SnapToGrid(Vector3 position)
    {
        if (previewObject == null) return position;

        PrefabSize prefabSize = previewObject.GetComponent<PrefabSize>();
        if (prefabSize == null)
        {
            prefabSize = previewObject.AddComponent<PrefabSize>();
            prefabSize.widthInTiles = 1;
            prefabSize.depthInTiles = 1;
        }

        float offsetX = (prefabSize.widthInTiles % 2 == 0) ? (gridSize + tileSpacing) / 2 : 0;
        float offsetZ = (prefabSize.depthInTiles % 2 == 0) ? (gridSize + tileSpacing) / 2 : 0;

        float snappedX = Mathf.Floor((position.x - gridArea.position.x + (gridSize / 2)) / (gridSize + tileSpacing)) * (gridSize + tileSpacing) + gridArea.position.x + offsetX;
        float snappedZ = Mathf.Floor((position.z - gridArea.position.z + (gridSize / 2)) / (gridSize + tileSpacing)) * (gridSize + tileSpacing) + gridArea.position.z + offsetZ;

        Vector3 snappedPosition = new Vector3(snappedX, gridArea.position.y, snappedZ);

        if (IsPositionAvailable(snappedPosition, prefabSize))
            return snappedPosition;

        return GetNextAvailablePosition(snappedPosition, prefabSize);
    }

    private bool IsPositionAvailable(Vector3 position, PrefabSize prefabSize)
    {
        for (int x = 0; x < prefabSize.widthInTiles; x++)
        {
            for (int z = 0; z < prefabSize.depthInTiles; z++)
            {
                Vector3 checkPosition = new Vector3(position.x + x * (gridSize + tileSpacing), position.y, position.z + z * (gridSize + tileSpacing));
                if (occupiedTiles.Contains(checkPosition))
                    return false;
            }
        }
        return true;
    }

    private Vector3 GetNextAvailablePosition(Vector3 startPosition, PrefabSize prefabSize)
    {
        for (int i = 0; i < 20; i++)
        {
            Vector3 newPosition = startPosition + new Vector3(i * (gridSize + tileSpacing), 0, 0);
            if (IsPositionAvailable(newPosition, prefabSize))
                return newPosition;
        }
        return startPosition;
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

    private void PlaceObject()
    {
        if (previewObject != null)
        {
            Vector3 placementPosition = previewObject.transform.position;
            PrefabSize prefabSize = previewObject.GetComponent<PrefabSize>();

            if (IsPositionAvailable(placementPosition, prefabSize))
            {
                for (int x = 0; x < prefabSize.widthInTiles; x++)
                {
                    for (int z = 0; z < prefabSize.depthInTiles; z++)
                    {
                        Vector3 occupiedPosition = new Vector3(placementPosition.x + x * (gridSize + tileSpacing), placementPosition.y, placementPosition.z + z * (gridSize + tileSpacing));
                        occupiedTiles.Add(occupiedPosition);
                    }
                }
                Instantiate(buildingPrefabs[currentPrefabIndex], placementPosition, Quaternion.identity);
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