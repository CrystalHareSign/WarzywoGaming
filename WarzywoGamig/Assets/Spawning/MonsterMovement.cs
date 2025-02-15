using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class MonsterMovement : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    private Transform targetArea;

    [Header("Unikanie przeszkód")]
    public float detectionRadius = 5.0f;  // Promień wykrywania przeszkód
    public float reactionDistance = 2.0f; // Dystans natychmiastowej reakcji
    public float avoidanceStrength = 4.0f; // Jak mocno skręca przy unikaniu
    public float avoidanceSpeedFactor = 1.5f; // Jak bardzo przyspiesza podczas uniku
    public float avoidanceTime = 0.2f; // Czas trwania uniku

    private bool isAvoiding = false;

    public void Initialize(Transform target)
    {
        targetArea = target;
    }

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            Debug.LogError(gameObject.name + " nie ma NavMeshAgent!");
        }
    }

    void Update()
    {
        if (targetArea != null)
        {
            navMeshAgent.SetDestination(targetArea.position);
            if (!isAvoiding)
            {
                DetectAndAvoidObstacles();
            }
        }
    }

    void DetectAndAvoidObstacles()
    {
        Collider[] obstacles = Physics.OverlapSphere(transform.position, detectionRadius);

        foreach (Collider obstacle in obstacles)
        {
            if (obstacle.CompareTag("Obstacle") || obstacle.CompareTag("Enemy"))
            {
                float distanceToObstacle = Vector3.Distance(transform.position, obstacle.transform.position);
                if (distanceToObstacle < reactionDistance)
                {
                    StartCoroutine(AvoidObstacle(obstacle.transform.position));
                    return;
                }
            }
        }
    }

    IEnumerator AvoidObstacle(Vector3 obstaclePosition)
    {
        isAvoiding = true;

        // Oblicz wektor uniku w stronę przeciwną do przeszkody
        Vector3 avoidanceDirection = (transform.position - obstaclePosition).normalized;
        avoidanceDirection.y = 0; // Nie zmieniamy wysokości

        // Dodaj losowy czynnik, aby unik był bardziej dynamiczny
        float randomOffset = Random.Range(-0.5f, 0.5f);
        avoidanceDirection += new Vector3(randomOffset, 0, randomOffset);

        Vector3 newTarget = transform.position + avoidanceDirection * avoidanceStrength;

        navMeshAgent.speed *= avoidanceSpeedFactor; // Tymczasowe zwiększenie prędkości
        navMeshAgent.SetDestination(newTarget);

        yield return new WaitForSeconds(avoidanceTime);

        navMeshAgent.speed /= avoidanceSpeedFactor; // Powrót do normalnej prędkości
        isAvoiding = false;
    }
}
