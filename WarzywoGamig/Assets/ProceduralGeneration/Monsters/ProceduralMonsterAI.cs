using UnityEngine;
using UnityEngine.AI;

public class ProceduralMonsterAI : MonoBehaviour
{
    public float followRange = 10f;
    public float patrolRadius = 5f;
    public float patrolWaitTime = 2f;

    private Transform player;
    private NavMeshAgent agent;
    private Vector3 patrolTarget;
    private float patrolTimer;
    private bool isPatrolling;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        StartPatrol();
    }

    void Update()
    {
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance <= followRange)
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);
                isPatrolling = false;
                return;
            }
        }

        PatrolBehaviour();
    }

    void PatrolBehaviour()
    {
        if (!isPatrolling)
        {
            StartPatrol();
        }

        // Jeœli dotar³ do celu patrolu, poczekaj chwilê i wybierz nowy punkt
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            patrolTimer += Time.deltaTime;
            if (patrolTimer >= patrolWaitTime)
            {
                StartPatrol();
            }
        }
        else
        {
            patrolTimer = 0f;
        }
    }

    void StartPatrol()
    {
        patrolTarget = GetRandomPatrolPoint();
        agent.SetDestination(patrolTarget);
        agent.isStopped = false;
        isPatrolling = true;
        patrolTimer = 0f;
    }

    Vector3 GetRandomPatrolPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
            return hit.position;
        else
            return transform.position;
    }
}