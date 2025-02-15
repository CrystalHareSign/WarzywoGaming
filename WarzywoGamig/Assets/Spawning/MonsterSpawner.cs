using UnityEngine;
using System.Collections.Generic;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Ustawienia spawnera")]
    public GameObject monsterPrefab; // Prefab potwora
    public int maxMonstersAtOnce = 5; // Maksymalna liczba potworów w danym momencie
    public float spawnInterval = 2f; // Czêstotliwoœæ spawnowania (w sekundach)
    public Collider spawnArea; // Obszar, w którym potwory bêd¹ siê pojawiaæ
    public float minSpawnDistance = 1.5f; // Minimalna odleg³oœæ miêdzy potworami

    private List<GameObject> activeMonsters = new List<GameObject>();
    private float spawnTimer;

    void Update()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnMonsters();
            spawnTimer = spawnInterval;
        }
    }

    void SpawnMonsters()
    {
        if (activeMonsters.Count >= maxMonstersAtOnce) return; // Sprawdzenie limitu potworów

        int monstersToSpawn = Mathf.Min(maxMonstersAtOnce - activeMonsters.Count, maxMonstersAtOnce);
        for (int i = 0; i < monstersToSpawn; i++)
        {
            Vector3 spawnPosition = GetValidSpawnPosition();
            if (spawnPosition != Vector3.zero)
            {
                GameObject monster = Instantiate(monsterPrefab, spawnPosition, Quaternion.identity);
                activeMonsters.Add(monster);
            }
        }
    }

    Vector3 GetValidSpawnPosition()
    {
        int maxAttempts = 10; // Liczba prób znalezienia odpowiedniej pozycji
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector3 randomPosition = GetRandomPointInBounds(spawnArea.bounds);

            if (IsPositionValid(randomPosition))
            {
                return randomPosition;
            }
        }
        return Vector3.zero; // Nie znaleziono odpowiedniego miejsca
    }

    Vector3 GetRandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            bounds.min.y,
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }

    bool IsPositionValid(Vector3 position)
    {
        foreach (GameObject monster in activeMonsters)
        {
            if (Vector3.Distance(position, monster.transform.position) < minSpawnDistance)
            {
                return false; // Pozycja jest za blisko innego potwora
            }
        }
        return true;
    }

    public void RemoveMonster(GameObject monster)
    {
        if (activeMonsters.Contains(monster))
        {
            activeMonsters.Remove(monster);
            Destroy(monster);
        }
    }
}
