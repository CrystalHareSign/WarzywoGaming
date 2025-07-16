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

    [Header("Atak - sto¿ek przed potworem")]
    [Tooltip("Zasiêg ataku (jak daleko siêga potwór)")]
    public float attackRange = 2.4f;
    [Tooltip("K¹t ataku (w stopniach, np. 60 = tylko przed potworem)")]
    [Range(0, 180)] public float attackAngle = 60f;
    [Tooltip("Obra¿enia zadawane graczowi przez jeden atak")]
    public float attackDamage = 20f;
    [Tooltip("Czas oczekiwania przed atakiem (okienko na ucieczkê)")]
    public float attackWindupTime = 0.6f;
    [Tooltip("Cooldown pomiêdzy atakami")]
    public float attackCooldown = 1.5f;
    [Tooltip("Prêdkoœæ potwora gdy szykuje siê do ataku")]
    public float attackSlowSpeed = 0.25f;

    private float lastAttackTime = -999f;
    private bool isAttackWindup = false;
    private float attackWindupTimer = 0f;

    private Vector3 lastKnownPlayerPosition;
    private bool hasLastKnownPosition = false;
    private float searchTimer = 0f;
    private bool searching = false;

    private Vector3 searchTarget;
    private float searchWaitTimer = 0f;

    private Transform player;
    private PlayerStats playerStats;
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
        {
            player = playerObj.transform;
            playerStats = player.GetComponent<PlayerStats>();
        }

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

            // --- ATAKOWANIE ---
            bool canAttack = detected && CanAttackPlayer();

            // Zmienione: AI nie zatrzymuje siê w windupie, tylko zwalnia i goni gracza
            if (canAttack)
            {
                if (!isAttackWindup && Time.time >= lastAttackTime + attackCooldown)
                {
                    agent.isStopped = false;
                    agent.speed = attackSlowSpeed; // zwalnia, ale nie zatrzymuje siê
                    isAttackWindup = true;
                    attackWindupTimer = 0f;
                }
                if (isAttackWindup)
                {
                    attackWindupTimer += Time.deltaTime;
                    agent.SetDestination(player.position); // zawsze goni gracza
                    if (attackWindupTimer >= attackWindupTime)
                    {
                        // Na koñcu windupu sprawdzamy, czy gracz jest w zasiêgu
                        if (CanAttackPlayer())
                        {
                            PerformAttack();
                            lastAttackTime = Time.time;
                        }
                        isAttackWindup = false;
                        attackWindupTimer = 0f;
                    }
                }
                else
                {
                    // Jeœli skoñczy³ windup, wraca do poœcigu na szybkoœci AGRO
                    agent.speed = agroSpeed;
                    agent.isStopped = false;
                    agent.SetDestination(player.position);
                }
                return;
            }
            // Je¿eli nie mo¿na zaatakowaæ, resetuj windup
            isAttackWindup = false;
            attackWindupTimer = 0f;


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

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                searchWaitTimer += Time.deltaTime;

                if (searchWaitTimer >= searchWaitTime)
                {
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

    /// <summary>
    /// Czy gracz jest w zasiêgu ataku (sto¿ek przed potworem)?
    /// </summary>
    bool CanAttackPlayer()
    {
        if (player == null) return false;
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 target = player.position + Vector3.up * playerEyeHeight;
        Vector3 dirToPlayer = (target - origin).normalized;
        float distance = Vector3.Distance(origin, target);

        if (distance > attackRange)
            return false;

        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > attackAngle / 2f)
            return false;

        // Raycast: czy nie ma przeszkody?
        if (Physics.Raycast(origin, dirToPlayer, out RaycastHit hit, attackRange, detectionMask))
        {
            if (hit.transform.CompareTag("Player"))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Zadaj obra¿enia graczowi jeœli nadal jest w zasiêgu ataku!
    /// </summary>
    private void PerformAttack()
    {
        if (playerStats != null && CanAttackPlayer())
        {
            playerStats.TakeDamage(attackDamage);
            // Mo¿esz dodaæ animacje, dŸwiêki, efekty itp.
        }
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

        searching = false;
        searchTimer = 0f;
        searchWaitTimer = 0f;
        isAttackWindup = false;
        attackWindupTimer = 0f;
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

        // Sto¿ek ataku przed potworem
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.4f);
        Vector3 attackOrigin = transform.position + Vector3.up * eyeHeight;
        float halfAttackAngle = attackAngle * 0.5f;
        Vector3 attackForward = transform.forward;
        int attackSegments = 16;
        for (int i = 0; i <= attackSegments; i++)
        {
            float angle = Mathf.Lerp(-halfAttackAngle, halfAttackAngle, i / (float)attackSegments);
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 dir = rot * attackForward;
            Gizmos.DrawLine(attackOrigin, attackOrigin + dir * attackRange);
        }
    }
}