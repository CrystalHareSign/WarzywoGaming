using UnityEngine;
using UnityEngine.AI;

public class MonsterSpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Collider spawnArea;
    public Transform targetArea;
    public int enemyCount = 5;

    void Start()
    {
        SpawnMonsters();
    }

    void SpawnMonsters()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 spawnPosition = GetValidSpawnPosition();
            if (spawnPosition == Vector3.zero)
            {
                Debug.LogWarning("Nie znaleziono poprawnej pozycji na NavMesh, używam domyślnej pozycji spawnu.");
                spawnPosition = transform.position; // Awaryjna pozycja
            }

            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            MonsterMovement movement = enemy.GetComponent<MonsterMovement>();
            if (movement != null)
            {
                movement.Initialize(targetArea);
            }
        }
    }

    Vector3 GetValidSpawnPosition()
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            Vector3 randomPoint = GetRandomPointInBounds(spawnArea.bounds);
            //Debug.Log($"Próbuję znaleźć pozycję na NavMesh: {randomPoint}");

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                return hit.position;
            }
            else
            {
                Debug.LogWarning($"Nie znaleziono pozycji na NavMesh dla punktu: {randomPoint}");
            }
        }
        return Vector3.zero; // Brak poprawnej pozycji
    }

    Vector3 GetRandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            bounds.center.y,
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }
}
