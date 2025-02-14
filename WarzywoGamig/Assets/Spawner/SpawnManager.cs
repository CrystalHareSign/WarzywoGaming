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
    public List<SpawnablePrefab> spawnablePrefabsGroup1; // Lista prefabów i ich szans dla grupy 1
    public GameObject[] spawnPointsGroup1;
    public float spawnIntervalGroup1 = 2f;
    public bool enableGroup1 = true;

    [Header("Druga grupa spawnerów")]
    public List<SpawnablePrefab> spawnablePrefabsGroup2; // Lista prefabów i ich szans dla grupy 2
    public GameObject[] spawnPointsGroup2;
    public float spawnIntervalGroup2 = 0.5f;
    public bool enableGroup2 = true;

    [Header("Trzecia grupa spawnerów")]
    public List<SpawnablePrefab> spawnablePrefabsGroup3; // Lista prefabów i ich szans dla grupy 3
    public GameObject[] spawnPointsGroup3;
    public float spawnIntervalGroup3 = 1f;
    public bool enableGroup3 = true;

    [Header("Prędkość obiektów")]
    public float spawnedObjectSpeed = 5f;

    [Header("Czas życia obiektów")]
    public float globalObjectLifetime = 10f;

    private float timerGroup1;
    private float timerGroup2;
    private float timerGroup3;

    void Start()
    {
        timerGroup1 = spawnIntervalGroup1;
        timerGroup2 = spawnIntervalGroup2;
        timerGroup3 = spawnIntervalGroup3;
    }

    void Update()
    {
        if (enableGroup1)
        {
            timerGroup1 -= Time.deltaTime;
            if (timerGroup1 <= 0f)
            {
                SpawnRandomPrefab(spawnablePrefabsGroup1, spawnPointsGroup1);
                timerGroup1 = spawnIntervalGroup1;
            }
        }

        if (enableGroup2)
        {
            timerGroup2 -= Time.deltaTime;
            if (timerGroup2 <= 0f)
            {
                SpawnRandomPrefab(spawnablePrefabsGroup2, spawnPointsGroup2);
                timerGroup2 = spawnIntervalGroup2;
            }
        }

        if (enableGroup3)
        {
            timerGroup3 -= Time.deltaTime;
            if (timerGroup3 <= 0f)
            {
                SpawnRandomPrefab(spawnablePrefabsGroup3, spawnPointsGroup3);
                timerGroup3 = spawnIntervalGroup3;
            }
        }
    }

    void SpawnRandomPrefab(List<SpawnablePrefab> spawnablePrefabs, GameObject[] spawnPoints)
    {
        if (spawnPoints.Length == 0 || spawnablePrefabs.Count == 0)
        {
            Debug.LogWarning("Brak przypisanych punktów spawnowania lub prefabów.");
            return;
        }

        GameObject selectedPrefab = ChoosePrefabByChance(spawnablePrefabs);
        if (selectedPrefab == null) return;

        int randomIndex = Random.Range(0, spawnPoints.Length);
        GameObject spawnPoint = spawnPoints[randomIndex];

        GameObject spawnedObject = Instantiate(selectedPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);
        ApplyVelocity(spawnedObject, spawnPoint);
        Destroy(spawnedObject, globalObjectLifetime);
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
