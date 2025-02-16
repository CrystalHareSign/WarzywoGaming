using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyData
{
    public GameObject enemyPrefab;
    public int amount; // Liczba sztuk tego typu
}

public class MonsterSpawner : MonoBehaviour
{
    public List<EnemyData> enemies; // Lista prefabów + liczba sztuk
    public Transform targetArea;
    public Vector3 spawnAreaSize = new Vector3(10, 1, 10);

    public static Transform TargetArea;
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    void Start()
    {
        TargetArea = targetArea; // Przypisujemy globalne TargetArea
        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        if (enemies.Count == 0)
        {
            Debug.LogError("Brak prefabów potworów w MonsterSpawner!");
            return;
        }

        foreach (EnemyData enemyData in enemies)
        {
            for (int i = 0; i < enemyData.amount; i++)
            {
                SpawnEnemy(enemyData.enemyPrefab);
            }
        }
    }

    void SpawnEnemy(GameObject enemyPrefab)
    {
        Vector3 spawnPosition = GetSpawnPosition();

        GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.Euler(0, 90, 0));
        spawnedEnemies.Add(newEnemy);

        MonsterMovement movement = newEnemy.GetComponent<MonsterMovement>();
        if (movement != null)
        {
            movement.SetTarget(TargetArea);
        }
    }

    Vector3 GetSpawnPosition()
    {
        Vector3 position;
        int attempts = 10;
        do
        {
            position = new Vector3(
                transform.position.x + Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
                transform.position.y,
                transform.position.z + Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
            );
            attempts--;
        } while (Physics.CheckSphere(position, 1f) && attempts > 0);

        return position;
    }
}
