using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SpawnablePrefab
{
    public GameObject prefab; // Prefab obiektu
    [Range(0, 100)] public float spawnChance; // Szansa procentowa na spawnowanie
}

public class SpawnManager : MonoBehaviour
{
    [Header("Pierwsza grupa spawnerów")]
    public List<SpawnablePrefab> spawnablePrefabsGroup1;
    public GameObject[] spawnPointsGroup1;
    public float spawnIntervalGroup1 = 2f;
    public bool enableGroup1 = true;

    [Header("Druga grupa spawnerów")]
    public List<SpawnablePrefab> spawnablePrefabsGroup2;
    public GameObject[] spawnPointsGroup2;
    public float spawnIntervalGroup2 = 0.5f;
    public bool enableGroup2 = true;

    [Header("Trzecia grupa spawnerów")]
    public List<SpawnablePrefab> spawnablePrefabsGroup3;
    public GameObject[] spawnPointsGroup3;
    public float spawnIntervalGroup3 = 1f;
    public bool enableGroup3 = true;

    [Header("Czwarta grupa spawnerów (Plane)")]
    public List<SpawnablePrefab> spawnablePrefabsGroup4;
    public GameObject[] spawnPointsGroup4;
    public float spawnIntervalGroup4 = 5f;
    public float group4Lifetime;
    public bool enableGroup4 = true;

    [Header("Piąta grupa spawnerów (Trawa)")]
    public List<SpawnablePrefab> spawnablePrefabsGroup5;
    public GameObject[] spawnPointsGroup5;
    public float spawnIntervalGroup5 = 3f;
    public float group5Lifetime;
    public bool enableGroup5 = true;

    [Header("Prędkość obiektów")]
    public float spawnedObjectSpeed = 5f;

    [Header("Czas życia obiektów")]
    public float globalObjectLifetime = 10f;

    private float timerGroup1;
    private float timerGroup2;
    private float timerGroup3;
    private float timerGroup4;
    private float timerGroup5;
    private bool hasGroundBeenSetToZero = false;

    void Start()
    {
        timerGroup1 = spawnIntervalGroup1;
        timerGroup2 = spawnIntervalGroup2;
        timerGroup3 = spawnIntervalGroup3;
        timerGroup4 = spawnIntervalGroup4;
        timerGroup5 = spawnIntervalGroup5;


    }

    void Update()
    {
        if (enableGroup1)
        {
            timerGroup1 -= Time.deltaTime;
            if (timerGroup1 <= 0f)
            {
                SpawnRandomPrefab(spawnablePrefabsGroup1, spawnPointsGroup1, LayerMask.GetMask("Default"), globalObjectLifetime); // Dla grupy 1 używamy globalObjectLifetime
                timerGroup1 = spawnIntervalGroup1;
            }
        }

        if (enableGroup2)
        {
            timerGroup2 -= Time.deltaTime;
            if (timerGroup2 <= 0f)
            {
                SpawnRandomPrefab(spawnablePrefabsGroup2, spawnPointsGroup2, LayerMask.GetMask("Default"), globalObjectLifetime); // Dla grupy 2 używamy globalObjectLifetime
                timerGroup2 = spawnIntervalGroup2;
            }
        }

        if (enableGroup3)
        {
            timerGroup3 -= Time.deltaTime;
            if (timerGroup3 <= 0f)
            {
                SpawnRandomPrefab(spawnablePrefabsGroup3, spawnPointsGroup3, LayerMask.GetMask("Default"), globalObjectLifetime); // Dla grupy 3 używamy globalObjectLifetime
                timerGroup3 = spawnIntervalGroup3;
            }
        }

        if (enableGroup4)
        {
            timerGroup4 -= Time.deltaTime;
            if (timerGroup4 <= 0f)
            {
                // Użyj wartości z Inspektora dla czasu życia grupy 4
                SpawnRandomPrefab(spawnablePrefabsGroup4, spawnPointsGroup4, LayerMask.GetMask("Ground"), group4Lifetime); // Przekazujemy group4Lifetime dla grupy 4
                timerGroup4 = spawnIntervalGroup4;
            }
        }

        if (enableGroup5)
        {
            timerGroup5 -= Time.deltaTime;
            if (timerGroup5 <= 0f)
            {
                // Użyj wartości z Inspektora dla czasu życia grupy 5
                SpawnRandomPrefab(spawnablePrefabsGroup5, spawnPointsGroup5, LayerMask.GetMask("Grass"), group5Lifetime); // Przekazujemy group5Lifetime dla grupy 5
                timerGroup5 = spawnIntervalGroup5;
            }
        }
    }

    void SpawnRandomPrefab(List<SpawnablePrefab> spawnablePrefabs, GameObject[] spawnPoints, LayerMask mask, float objectLifetime)
    {
        if (spawnPoints.Length == 0 || spawnablePrefabs.Count == 0)
        {
            Debug.LogWarning("Brak przypisanych punktów spawnowania lub prefabów.");
            return;
        }

        GameObject selectedPrefab = ChoosePrefabByChance(spawnablePrefabs);
        if (selectedPrefab == null) return;

        int maxAttempts = spawnPoints.Length;
        for (int i = 0; i < maxAttempts; i++)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            GameObject spawnPoint = spawnPoints[randomIndex];

            Collider prefabCollider = selectedPrefab.GetComponent<Collider>();
            if (prefabCollider == null)
            {
                Debug.LogError($"Prefab {selectedPrefab.name} nie ma colliddera!");
                continue;
            }

            Vector3 spawnPosition = spawnPoint.transform.position;
            Vector3 halfExtents = prefabCollider.bounds.extents;

            // Użyj maski warstwy, aby wykryć inne obiekty
            Collider[] colliders = Physics.OverlapBox(spawnPosition, halfExtents, Quaternion.identity, mask);

            // Sprawdź, czy w danym miejscu jest już obiekt
            if (colliders.Length == 0)
            {
                GameObject spawnedObject = Instantiate(selectedPrefab, spawnPosition, spawnPoint.transform.rotation);

                // Jeśli spawnuje się Ground i jeszcze nie zostało ustawione
                if (selectedPrefab.CompareTag("Ground") && !hasGroundBeenSetToZero)
                {
                    Vector3 newPosition = new Vector3(0f, spawnedObject.transform.position.y, 0f); // Nowa pozycja (0, Y, 0)
                    spawnedObject.transform.position = newPosition; // Ustawiamy pozycję

                    hasGroundBeenSetToZero = true; // Ustawiamy flagę, aby nie ustawiać więcej
                }

                ApplyVelocity(spawnedObject, spawnPoint);

                // Ustawienie lifetime dla obiektu
                MovingObject movingObject = spawnedObject.GetComponent<MovingObject>();
                if (movingObject != null)
                {
                    movingObject.Initialize(spawnedObjectSpeed, objectLifetime); // Przekazujemy odpowiedni lifetime
                }

                Destroy(spawnedObject, objectLifetime); // Obiekt będzie zniszczony po upływie czasu
                return;
            }
        }

        //Debug.LogWarning("Nie znaleziono wolnego miejsca do spawnowania.");
    }

    GameObject ChoosePrefabByChance(List<SpawnablePrefab> spawnablePrefabs)
    {
        float totalChance = 0f;
        foreach (var spawnable in spawnablePrefabs)
        {
            totalChance += spawnable.spawnChance;
        }

        float randomValue = Random.Range(0, totalChance);
        float cumulativeChance = 0f;

        foreach (var spawnable in spawnablePrefabs)
        {
            cumulativeChance += spawnable.spawnChance;
            if (randomValue <= cumulativeChance)
            {
                return spawnable.prefab;
            }
        }

        return null;
    }

    void ApplyVelocity(GameObject spawnedObject, GameObject spawnPoint)
    {
        Rigidbody rb = spawnedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = -spawnPoint.transform.right * spawnedObjectSpeed;
        }
    }
}
