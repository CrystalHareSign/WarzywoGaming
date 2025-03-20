using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class HarpoonController : MonoBehaviour
{
    public GameObject harpoonPrefab;
    public Transform firePoint;
    public Transform treasureMountPoint;
    [Header("HARPUN PARAMETRY")]
    public float shootSpeed = 20f;
    public float returnSpeed = 10f;
    public float maxRange = 50f;
    public float reloadTimer = 0f; // Nowa zmienna do liczenia czasu przeładowania
    public float treasureLifetime = 1f;
    [Header("! DRGANIE !")]
    public float returnTolerance = 3.0f;
    [Header("WYKRYWANIE")]
    public float maxHorizontalAngleToTarget = 30f;
    public float maxVerticalAngleToTarget = 30f;
    public float detectionRange = 100f;
    public float minDetectionRange = 10f;
    public bool showRangesInScene = true;

    private GameObject currentHarpoon;
    private Rigidbody harpoonRb;
    public bool isReturning = false;
    public bool canShoot = true;
    private Vector3 initialScale = new Vector3(1, 1, 1);
    private float currentShootDistance = 0f;
    private Vector3 shootPosition;

    private TurretController turretController;

    void Start()
    {
        turretController = Object.FindFirstObjectByType<TurretController>();

        if (harpoonPrefab != null && firePoint != null)
        {
            currentHarpoon = Instantiate(harpoonPrefab, firePoint.position, firePoint.rotation);
            currentHarpoon.transform.SetParent(firePoint);
            harpoonRb = currentHarpoon.GetComponent<Rigidbody>();
            harpoonRb.isKinematic = true;
            currentHarpoon.transform.localScale = initialScale;

            Harpoon harpoonScript = currentHarpoon.GetComponent<Harpoon>();
            if (harpoonScript != null)
            {
                harpoonScript.treasureLifetime = treasureLifetime;
            }
        }
        else
        {
            Debug.LogError("harpoonPrefab lub firePoint nie jest przypisany.");
        }
    }

    void Update()
    {
        if (turretController != null && turretController.isUsingTurret && turretController.isRaised)
        {
            if (Input.GetMouseButtonDown(0) && canShoot && !(turretController.isLowering) && reloadTimer <= 0f) // Sprawdzamy, czy czas przeładowania minął
            {
                GameObject target = FindClosestTreasureInView();
                if (target != null)
                {
                    Vector3 predictedPosition = PredictTargetPosition(target);
                    ShootHarpoon(predictedPosition);
                }
                else
                {
                    Vector3 shootDirection = GetMouseWorldPosition();
                    ShootHarpoon(shootDirection);
                }
            }

            if (isReturning)
            {
                ReturnHarpoon();
            }

            if (currentShootDistance >= maxRange)
            {
                StartReturnHarpoon();
            }

            // Jeśli harpun wrócił, zaczynamy odliczać czas przeładowania
            if (reloadTimer > 0f)
            {
                reloadTimer -= Time.deltaTime;
            }
        }

        if (currentHarpoon != null && !harpoonRb.isKinematic)
        {
            currentShootDistance = Vector3.Distance(shootPosition, currentHarpoon.transform.position);
        }
    }

    void ShootHarpoon(Vector3 targetPosition)
    {
        if (currentHarpoon != null)
        {
            currentHarpoon.transform.SetParent(null);
            currentHarpoon.SetActive(true);

            Vector3 shootDirection = Camera.main.transform.forward;
            harpoonRb.isKinematic = false;
            harpoonRb.linearVelocity = shootDirection * shootSpeed;

            canShoot = false;
            shootPosition = firePoint.position;
            currentShootDistance = 0f;
        }
        else
        {
            Debug.LogError("currentHarpoon nie jest przypisany.");
        }
    }

    void ReturnHarpoon()
    {
        if (currentHarpoon != null)
        {
            Vector3 returnDirection = (firePoint.position - currentHarpoon.transform.position).normalized;
            harpoonRb.linearVelocity = returnDirection * returnSpeed;

            if (Vector3.Distance(currentHarpoon.transform.position, firePoint.position) < returnTolerance)
            {
                harpoonRb.linearVelocity = Vector3.zero;
                harpoonRb.isKinematic = true;
                currentHarpoon.transform.SetParent(firePoint);
                currentHarpoon.transform.localPosition = Vector3.zero;
                currentHarpoon.transform.localRotation = Quaternion.identity;
                currentHarpoon.transform.localScale = initialScale;

                foreach (Transform child in currentHarpoon.transform)
                {
                    child.localPosition = Vector3.zero;
                    child.localRotation = Quaternion.identity;
                }

                isReturning = false;

                // Po powrocie harpunu, zaczynamy czas przeładowania
                reloadTimer = reloadTime; // Zainicjuj czas przeładowania
                canShoot = true; // Pozwól na strzał po zakończeniu przeładowania
            }
        }
        else
        {
            Debug.LogError("currentHarpoon nie jest przypisany.");
        }
    }

    void StartReturnHarpoon()
    {
        if (harpoonRb != null)
        {
            harpoonRb.isKinematic = false;
            isReturning = true;
        }
        else
        {
            Debug.LogError("harpoonRb nie jest przypisany.");
        }
    }

    public void OnHarpoonCollision()
    {
        if (harpoonRb != null)
        {
            StartReturnHarpoon();
        }
        else
        {
            Debug.LogError("harpoonRb nie jest przypisany.");
        }
    }

    GameObject FindClosestTreasureInView()
    {
        GameObject[] treasures = GameObject.FindGameObjectsWithTag("Treasure");
        GameObject closestTreasure = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject treasure in treasures)
        {
            Vector3 directionToTreasure = (treasure.transform.position - firePoint.position).normalized;
            float angleToTreasureHorizontal = Vector3.Angle(firePoint.forward, new Vector3(directionToTreasure.x, 0, directionToTreasure.z));
            float angleToTreasureVertical = Vector3.Angle(firePoint.forward, new Vector3(0, directionToTreasure.y, directionToTreasure.z));
            float distanceToTreasure = Vector3.Distance(firePoint.position, treasure.transform.position);

            if (angleToTreasureHorizontal <= maxHorizontalAngleToTarget && angleToTreasureVertical <= maxVerticalAngleToTarget && distanceToTreasure <= detectionRange && distanceToTreasure >= minDetectionRange)
            {
                if (distanceToTreasure < closestDistance)
                {
                    closestDistance = distanceToTreasure;
                    closestTreasure = treasure;
                }
            }
        }

        return closestTreasure;
    }

    Vector3 PredictTargetPosition(GameObject target)
    {
        TreasureTracker treasureTracker = target.GetComponent<TreasureTracker>();

        if (treasureTracker != null)
        {
            Vector3 targetVelocity = treasureTracker.CurrentVelocity;
            float distanceToTarget = Vector3.Distance(firePoint.position, target.transform.position);
            float timeToTarget = distanceToTarget / shootSpeed;
            Vector3 predictedPosition = target.transform.position + targetVelocity * timeToTarget;

            return predictedPosition;
        }

        return target.transform.position;
    }

    Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            return hit.point;
        }

        return ray.origin + ray.direction * maxRange;
    }

    void OnDrawGizmos()
    {
        if (showRangesInScene)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePoint.position, detectionRange);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(firePoint.position, minDetectionRange);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(firePoint.position, maxRange);
            Gizmos.color = Color.yellow;
            DrawViewCone(firePoint.position, firePoint.forward, maxHorizontalAngleToTarget, maxVerticalAngleToTarget, detectionRange);
        }
    }

    void DrawViewCone(Vector3 position, Vector3 direction, float horizontalAngle, float verticalAngle, float range)
    {
        Vector3 up = Quaternion.Euler(verticalAngle, 0, 0) * direction * range;
        Vector3 down = Quaternion.Euler(-verticalAngle, 0, 0) * direction * range;
        Vector3 left = Quaternion.Euler(0, -horizontalAngle, 0) * direction * range;
        Vector3 right = Quaternion.Euler(0, horizontalAngle, 0) * direction * range;

        Gizmos.DrawLine(position, position + up);
        Gizmos.DrawLine(position, position + down);
        Gizmos.DrawLine(position, position + left);
        Gizmos.DrawLine(position, position + right);

        Gizmos.DrawLine(position + up, position + left);
        Gizmos.DrawLine(position + up, position + right);
        Gizmos.DrawLine(position + down, position + left);
        Gizmos.DrawLine(position + down, position + right);
    }
}
