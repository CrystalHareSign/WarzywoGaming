using UnityEngine;
using UnityEngine.AI;

public class MonsterMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    private Vector3 targetOffset;
    public float randomOffsetRange = 2f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false; // Nie obracaj automatycznie
        transform.rotation = Quaternion.Euler(0, 90, 0); // Naprawiony obrót
        SetRandomOffset();
        InvokeRepeating(nameof(SetRandomOffset), 3f, 3f);
    }

    void Update()
    {
        if (MonsterSpawner.TargetArea == null) return; // Sprawdzamy tylko raz

        // Cel + losowe przesunięcie
        Vector3 targetPosition = MonsterSpawner.TargetArea.position + targetOffset;
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
}
