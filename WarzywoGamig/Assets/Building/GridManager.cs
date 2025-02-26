using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public float gridSize = 1f; // Rozmiar siatki
    public float tileSpacing = 0.1f; // Odst�p mi�dzy kafelkami
    public Transform gridArea; // Obszar siatki
    public GameObject gridTilePrefab; // Prefab kafelka siatki
    public List<GameObject> buildingPrefabs = new List<GameObject>(); // Lista dost�pnych prefab�w
    private int currentPrefabIndex = 0; // Aktualny indeks prefabrykatu

    private bool isBuildingMode = false; // Tryb budowy w��czony/wy��czony
    private GameObject previewObject; // Obiekt podgl�du
    private float gridAreaWidth;
    private float gridAreaHeight;
    private Dictionary<Vector3, GameObject> occupiedTiles = new Dictionary<Vector3, GameObject>(); // Zbi�r zaj�tych kafelk�w

    public static GridManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

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

        if (IsPositionAvailable(snappedPosition, prefabSize) && IsInsideGrid(snappedPosition, prefabSize))
            return snappedPosition;

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

    private bool IsPositionAvailable(Vector3 position, PrefabSize prefabSize)
    {
        for (int x = 0; x < prefabSize.widthInTiles; x++)
        {
            for (int z = 0; z < prefabSize.depthInTiles; z++)
            {
                Vector3 checkPosition = new Vector3(position.x + x * (gridSize + tileSpacing), position.y, position.z + z * (gridSize + tileSpacing));
                if (occupiedTiles.ContainsKey(checkPosition))
                    return false;
            }
        }
        return true;
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
            return hit.point;
        }
        return Vector3.zero;
    }

    public void PlaceObject()
    {
        if (previewObject != null)
        {
            Vector3 placementPosition = previewObject.transform.position;
            PrefabSize prefabSize = previewObject.GetComponent<PrefabSize>();

            if (IsPositionAvailable(placementPosition, prefabSize) && IsInsideGrid(placementPosition, prefabSize))
            {
                for (int x = 0; x < prefabSize.widthInTiles; x++)
                {
                    for (int z = 0; z < prefabSize.depthInTiles; z++)
                    {
                        Vector3 occupiedPosition = new Vector3(
                            placementPosition.x + x * (gridSize + tileSpacing),
                            placementPosition.y,
                            placementPosition.z + z * (gridSize + tileSpacing)
                        );
                        occupiedTiles[occupiedPosition] = previewObject;
                    }
                }

                Instantiate(buildingPrefabs[currentPrefabIndex], placementPosition, Quaternion.identity);

                //// Wy��czenie trybu budowania po postawieniu obiektu
                //isBuildingMode = false;
                //DestroyPreviewObject();
                //ToggleGridVisibility(false);
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

    public void AddToBuildingPrefabs(GameObject prefab)
    {
        if (prefab != null && !buildingPrefabs.Contains(prefab))
        {
            buildingPrefabs.Add(prefab);
        }
        else
        {
            Debug.LogWarning("Trying to add a null or duplicate prefab to buildingPrefabs.");
        }
    }
}
