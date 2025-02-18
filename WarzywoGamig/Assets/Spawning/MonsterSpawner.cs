using UnityEngine;
using UnityEngine.AI;

public class MonsterSpawner : MonoBehaviour
{
    public Transform targetArea; // ✅ TargetArea teraz jest publiczne i dostępne w Inspectorze
    public Vector3 spawnAreaSize = new Vector3(10, 1, 10);

    public static Transform TargetArea;

    void Start()
    {
        if (targetArea == null)
        {
            //Debug.LogError("[MonsterSpawner] ❌ TargetArea nie jest przypisane w Inspectorze!");
        }
        else
        {
            TargetArea = targetArea;
            //Debug.Log("[MonsterSpawner] ✅ TargetArea ustawione na start!");
        }
    }

    public void SetTarget(Transform newTarget)
    {
        TargetArea = newTarget;
        //Debug.Log("[MonsterSpawner] ✅ TargetArea ustawione przez RoundManager!");
    }

    public void SpawnEnemyGroup(GameObject enemyPrefab, int count)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("[MonsterSpawner] ❌ enemyPrefab jest NULL! Nie można zespawnować wrogów.");
            return;
        }

        //Debug.Log($"[MonsterSpawner] 🧟 Otrzymano żądanie spawnu: {count}x {enemyPrefab.name}");

        for (int i = 0; i < count; i++)
        {
            SpawnEnemy(enemyPrefab);
        }
    }

    public void SpawnEnemy(GameObject enemyPrefab)
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();

        if (spawnPosition == Vector3.zero)
        {
            //Debug.LogWarning("[MonsterSpawner] ⚠️ Nie znaleziono odpowiedniego miejsca do spawnu!");
            return;
        }

        GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.Euler(0, 90, 0));

        //Debug.Log($"[MonsterSpawner] ✅ Zespawnowano {enemyPrefab.name} na pozycji {spawnPosition}");

        MonsterMovement movement = newEnemy.GetComponent<MonsterMovement>();
        if (movement != null)
        {
            movement.SetTarget(TargetArea);
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        Vector3 spawnCenter = transform.position;
        Vector3 randomPosition = spawnCenter + new Vector3(
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            0,
            Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
        );

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPosition, out hit, 5f, NavMesh.AllAreas))
        {
            //Debug.Log($"[MonsterSpawner] ✅ Spawnuję wroga na {hit.position} (NavMesh)");
            return hit.position;
        }

        //Debug.LogWarning("[MonsterSpawner] ⚠️ Nie znaleziono miejsca na NavMesh, spawnuję na pozycji spawnera!");
        return spawnCenter;
    }
}
