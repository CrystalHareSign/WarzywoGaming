using UnityEngine;
using UnityEngine.AI;

public class ProceduralMonsterAI : MonoBehaviour
{
    [Header("Zmys³y")]
    public bool useSight = true;
    public bool useHearing = true;

    [Header("Wzrok - wysokoœæ oczu")]
    public float eyeHeight = 4.6f;
    [Header("2.5 +- wysokoœæ oczu gracza")]
    public float playerEyeHeight = 2.6f;

    [Header("Wzrok (Cone)")]
    public float sightRange = 12f;
    [Range(0, 360)] public float sightAngle = 80f;

    [Header("S³uch (Radius)")]
    public float hearingRange = 6f;

    [Header("Layer Mask")]
    public LayerMask detectionMask;

    [Header("Patrol")]
    public float patrolRadius = 5f;
    public float patrolWaitTime = 2f;

    [Header("Prêdkoœci")]
    [Tooltip("Prêdkoœæ patrolowania")]
    public float normalSpeed = 3.5f;
    [Tooltip("Prêdkoœæ w trybie AGRO (po wykryciu gracza)")]
    public float agroSpeed = 6f;
    [Tooltip("Prêdkoœæ podczas szukania w okolicy ostatniej pozycji gracza")]
    public float searchSpeed = 4.5f;

    [Header("Szukanie gracza po zgubieniu")]
    [Tooltip("Ile sekund potwór szuka gracza w okolicy ostatniej pozycji")]
    public float searchTime = 3f;
    [Tooltip("Promieñ obszaru, po którym potwór chodzi w trybie szukania")]
    public float searchRadius = 2f;
    [Tooltip("Ile sekund stoi w jednym punkcie podczas szukania")]
    public float searchWaitTime = 0.7f;

    private Vector3 lastKnownPlayerPosition;
    private bool hasLastKnownPosition = false;
    private float searchTimer = 0f;
    private bool searching = false;

    private Vector3 searchTarget;
    private float searchWaitTimer = 0f;

    private Transform player;
    private NavMeshAgent agent;
    private Vector3 patrolTarget;
    private float patrolTimer;
    private bool isPatrolling;

    // KOLOR AGRO
    private Renderer rend;
    private Color agroColor = Color.red;
    private Color calmColor = Color.white;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        rend = GetComponentInChildren<Renderer>();
        if (rend != null)
            rend.material.color = calmColor;

        StartPatrol();
    }

    void Update()
    {
        bool detected = false;

        if (player != null)
        {
            if (useSight && IsPlayerInSight())
                detected = true;

            if (!detected && useHearing && IsPlayerHeard())
                detected = true;

            // ZMIANA KOLORU
            if (rend != null)
                rend.material.color = detected ? agroColor : calmColor;

            if (detected)
            {
                lastKnownPlayerPosition = player.position;
                hasLastKnownPosition = true;
                searching = false;
                searchTimer = 0f;
                searchWaitTimer = 0f;
                agent.isStopped = false;
                agent.speed = agroSpeed; // SZYBKOŒÆ AGRO
                agent.SetDestination(player.position);
                isPatrolling = false;
                return;
            }
        }
        else
        {
            if (rend != null)
                rend.material.color = calmColor;
        }

        // ---- SZUKANIE OSTATNIEJ POZYCJI GRACZA ----
        if (!detected && hasLastKnownPosition)
        {
            searching = true;
            agent.isStopped = false;
            agent.speed = searchSpeed; // SZYBKOŒÆ SZUKANIA

            // Jeœli dotar³ do celu szukania lub jeszcze nie ma celu
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                searchWaitTimer += Time.deltaTime;
                if (searchWaitTimer >= searchWaitTime)
                {
                    // Losuj nowy punkt wokó³ ostatniej pozycji gracza
                    Vector2 circle = Random.insideUnitCircle * searchRadius;
                    searchTarget = lastKnownPlayerPosition + new Vector3(circle.x, 0, circle.y);
                    agent.SetDestination(searchTarget);
                    searchWaitTimer = 0f;
                }
            }
            searchTimer += Time.deltaTime;
            if (searchTimer >= searchTime)
            {
                hasLastKnownPosition = false;
                searching = false;
                searchTimer = 0f;
                searchWaitTimer = 0f;
                StartPatrol();
            }
            return;
        }

        PatrolBehaviour();
    }

    bool IsPlayerInSight()
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 target = player.position + Vector3.up * playerEyeHeight;
        Vector3 dirToPlayer = (target - origin).normalized;
        float distance = Vector3.Distance(origin, target);

        if (distance > sightRange)
            return false;

        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > sightAngle / 2f)
            return false;

        if (Physics.Raycast(origin, dirToPlayer, out RaycastHit hit, sightRange, detectionMask))
        {
            Debug.Log("Raycast trafi³ w: " + hit.transform.name + " | tag: " + hit.transform.tag + " | warstwa: " + LayerMask.LayerToName(hit.transform.gameObject.layer));
            if (hit.transform.CompareTag("Player"))
                return true;
        }
        return false;
    }

    bool IsPlayerHeard()
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 dirToPlayer = (player.position - origin).normalized;
        float distance = Vector3.Distance(origin, player.position);

        if (distance > hearingRange)
            return false;

        if (Physics.Raycast(origin, dirToPlayer, out RaycastHit hit, hearingRange, detectionMask))
        {
            if (hit.transform.CompareTag("Player"))
                return true;
        }
        return false;
    }

    void PatrolBehaviour()
    {
        if (searching) return; // nie patroluj podczas szukania gracza

        if (!isPatrolling)
            StartPatrol();

        agent.speed = normalSpeed; // SZYBKOŒÆ PATROLU

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            patrolTimer += Time.deltaTime;
            if (patrolTimer >= patrolWaitTime)
                StartPatrol();
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
        agent.speed = normalSpeed; // SZYBKOŒÆ PATROLU
        isPatrolling = true;
        patrolTimer = 0f;

        // resetowanie zmiennych szukania na wszelki wypadek
        searching = false;
        searchTimer = 0f;
        searchWaitTimer = 0f;
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

    void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;

        // S³uch – okr¹g
        if (useHearing)
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.3f);
            Gizmos.DrawWireSphere(origin, hearingRange);
        }

        // Wzrok – sto¿ek
        if (useSight)
        {
            Gizmos.color = new Color(1f, 1f, 0.2f, 0.6f);
            Vector3 forward = transform.forward;
            float halfAngle = sightAngle * 0.5f;
            Quaternion leftRay = Quaternion.AngleAxis(-halfAngle, Vector3.up);
            Quaternion rightRay = Quaternion.AngleAxis(halfAngle, Vector3.up);

            Vector3 leftDir = leftRay * forward;
            Vector3 rightDir = rightRay * forward;

            Gizmos.DrawLine(origin, origin + leftDir * sightRange);
            Gizmos.DrawLine(origin, origin + rightDir * sightRange);

            int segments = 24;
            for (int i = 0; i <= segments; i++)
            {
                float angle = Mathf.Lerp(-halfAngle, halfAngle, i / (float)segments);
                Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
                Vector3 dir = rot * forward;
                Gizmos.DrawLine(origin, origin + dir * sightRange);
            }
        }

        // Gizmo dla obszaru szukania gracza
        if (hasLastKnownPosition)
        {
            Gizmos.color = new Color(1f, 0.5f, 0.2f, 0.3f);
            Gizmos.DrawWireSphere(lastKnownPlayerPosition, searchRadius);
        }
    }
}