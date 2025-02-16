using UnityEngine;
using UnityEngine.AI;

public class MonsterMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    private Vector3 targetOffset;
    public float randomOffsetRange = 2f;
    private Transform targetArea;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        transform.rotation = Quaternion.Euler(0, 90, 0);
        SetRandomOffset();
        InvokeRepeating(nameof(SetRandomOffset), 3f, 3f);
    }

    void Update()
    {
        if (targetArea == null) return;

        Vector3 targetPosition = targetArea.position + targetOffset;

        // Sprawdzamy, czy w pobliżu są inni wrogowie i omijamy ich
        Vector3 avoidanceVector = GetSeparationVector();
        targetPosition += avoidanceVector;

        agent.SetDestination(targetPosition);
    }

    void SetRandomOffset()
    {
        targetOffset = new Vector3(
            Random.Range(-randomOffsetRange, randomOffsetRange),
            0,
            Random.Range(-randomOffsetRange, randomOffsetRange)
        );
    }

    public void SetTarget(Transform target)
    {
        targetArea = target;
    }

    Vector3 GetSeparationVector()
    {
        Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, 1f);
        Vector3 separationVector = Vector3.zero;
        int count = 0;

        foreach (Collider col in nearbyEnemies)
        {
            if (col.gameObject != gameObject && col.CompareTag("Enemy"))
            {
                separationVector += (transform.position - col.transform.position).normalized;
                count++;
            }
        }

        if (count > 0)
        {
            separationVector /= count;
            separationVector *= 2f; // Jak mocno omijają innych
        }

        return separationVector;
    }
}
