using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public List<GameObject> enemyPrefabs;
    public Transform targetArea; // Teraz to będzie globalne dla wszystkich potworów
    public int enemyCount = 10;
    public Vector3 spawnAreaSize = new Vector3(10, 1, 10);

    public static Transform TargetArea; // Statyczne pole dla potworów

    private List<GameObject> spawnedEnemies = new List<GameObject>();

    void Start()
    {
        TargetArea = targetArea; // Przypisujemy target globalnie
        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        if (enemyPrefabs.Count == 0)
        {
            Debug.LogError("Brak prefabów potworów w MonsterSpawner!");
            return;
        }

        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
        Vector3 spawnPosition = new Vector3(
            transform.position.x + Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            transform.position.y,
            transform.position.z + Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
        );

        GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.Euler(0, 90, 0));
        spawnedEnemies.Add(newEnemy);
    }
}
