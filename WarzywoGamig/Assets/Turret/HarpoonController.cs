using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class HarpoonController : MonoBehaviour
{
    public GameObject harpoonPrefab;
    public Transform firePoint;
    public Transform treasureMountPoint; // Nowy punkt montażu dla Treasure
    [Header("HARPUN PARAMETRY")]
    public float shootSpeed = 20f;
    public float returnSpeed = 10f;
    public float maxRange = 50f; // Maksymalny zasięg harpunu
    public float reloadTime = 2f; // Czas przeładowania harpunu w sekundach
    public float treasureLifetime = 1f; // Czas, po którym Treasure zostaje zniszczone po powrocie harpunu
    [Header("! DRGANIE !")]
    public float returnTolerance = 3.0f;
    [Header("WYKRYWANIE")]
    public float maxHorizontalAngleToTarget = 30f; // Maksymalny poziomy kąt, w którym harpun naprowadza się na cel
    public float maxVerticalAngleToTarget = 30f; // Maksymalny pionowy kąt, w którym harpun naprowadza się na cel
    public float detectionRange = 100f; // Maksymalny zasięg wykrywania celu
    public float minDetectionRange = 10f; // Minimalny zasięg wykrywania celu
    public bool showRangesInScene = true; // Czy pokazywać zakresy w scenie

    private GameObject currentHarpoon;
    private Rigidbody harpoonRb;
    public bool isReturning = false;
    public bool canShoot = true;
    private Vector3 initialScale = new Vector3(1, 1, 1);
    private float currentShootDistance = 0f; // Aktualna odległość przebyta przez harpun
    private Vector3 shootPosition; // Pozycja z której został wystrzelony harpun

    private TurretController turretController; // Referencja do TurretController

    void Start()
    {
        // Automatyczne przypisanie TurretController
        turretController = Object.FindFirstObjectByType<TurretController>();

        if (harpoonPrefab != null && firePoint != null)
        {
            // Inicjalizuj harpun i ustaw jako child firePoint
            currentHarpoon = Instantiate(harpoonPrefab, firePoint.position, firePoint.rotation);
            currentHarpoon.transform.SetParent(firePoint);
            harpoonRb = currentHarpoon.GetComponent<Rigidbody>();
            harpoonRb.isKinematic = true;
            currentHarpoon.transform.localScale = initialScale; // Ustaw skalę na 1x1x1

            // Ustaw treasureLifetime w skrypcie Harpoon
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
            if (Input.GetMouseButtonDown(0) && canShoot && !(turretController.isLowering))
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

            // Sprawdź czy harpun przekroczył maksymalny zasięg
            if (currentShootDistance >= maxRange)
            {
                StartReturnHarpoon();
            }
        }

        // Oblicz aktualną odległość przebyta przez harpun
        if (currentHarpoon != null && !harpoonRb.isKinematic)
        {
            currentShootDistance = Vector3.Distance(shootPosition, currentHarpoon.transform.position);
        }
    }

    void ShootHarpoon(Vector3 targetPosition)
    {
        if (currentHarpoon != null)
        {
            // Odłącz harpun od firePoint i aktywuj go
            currentHarpoon.transform.SetParent(null);
            currentHarpoon.SetActive(true);

            // Deklarujemy i obliczamy kierunek na podstawie kamery
            Vector3 shootDirection = Camera.main.transform.forward;  // Tutaj deklarujemy shootDirection

            // Ustawiamy prędkość harpunu w tym kierunku
            harpoonRb.isKinematic = false;
            harpoonRb.linearVelocity = shootDirection * shootSpeed;

            canShoot = false;

            // Zapisz pozycję z której harpun został wystrzelony
            shootPosition = firePoint.position;
            currentShootDistance = 0f;

            // Rozpocznij proces powrotu harpunu po określonym czasie
            StartCoroutine(ReturnHarpoonAfterDelay());
        }
        else
        {
            Debug.LogError("currentHarpoon nie jest przypisany.");
        }
    }


    IEnumerator ReturnHarpoonAfterDelay()
    {
        yield return new WaitForSeconds(reloadTime);
        StartReturnHarpoon();
    }

    void ReturnHarpoon()
    {
        if (currentHarpoon != null)
        {
            Vector3 returnDirection = (firePoint.position - currentHarpoon.transform.position).normalized;
            harpoonRb.linearVelocity = returnDirection * returnSpeed;

            if (Vector3.Distance(currentHarpoon.transform.position, firePoint.position) < returnTolerance) // Zwiększony margines tolerancji
            {
                // Zatrzymaj harpun i ustaw jako child firePoint
                harpoonRb.linearVelocity = Vector3.zero;
                harpoonRb.isKinematic = true;
                currentHarpoon.transform.SetParent(firePoint);
                currentHarpoon.transform.localPosition = Vector3.zero;
                currentHarpoon.transform.localRotation = Quaternion.identity;
                currentHarpoon.transform.localScale = initialScale; // Ustaw skalę na 1x1x1

                // Zresetuj pozycję i rotację przyczepionego obiektu
                foreach (Transform child in currentHarpoon.transform)
                {
                    child.localPosition = Vector3.zero;
                    child.localRotation = Quaternion.identity;
                }

                isReturning = false;
                StartCoroutine(ReloadHarpoon());
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
            harpoonRb.isKinematic = false; // Upewnij się, że harpun nie jest kinematyczny przed powrotem
            isReturning = true;
        }
        else
        {
            Debug.LogError("harpoonRb nie jest przypisany.");
        }
    }

    IEnumerator ReloadHarpoon()
    {
        yield return new WaitForSeconds(reloadTime);
        canShoot = true;
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
            Vector3 targetVelocity = treasureTracker.CurrentVelocity;  // Prędkość celu
            float distanceToTarget = Vector3.Distance(firePoint.position, target.transform.position);

            // Czas, w którym harpun dotrze do celu, zakładając, że harpun porusza się z prędkością shootSpeed
            float timeToTarget = distanceToTarget / shootSpeed;

            // Predykcja nowej pozycji celu, uwzględniając jego prędkość
            Vector3 predictedPosition = target.transform.position + targetVelocity * timeToTarget;

            return predictedPosition;
        }

        // Jeżeli nie znaleziono skryptu TreasureTracker, po prostu zwróć obecną pozycję celu
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
            // Rysowanie maksymalnego zasięgu wykrywania celu
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePoint.position, detectionRange);

            // Rysowanie minimalnego zasięgu wykrywania celu
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(firePoint.position, minDetectionRange);

            // Rysowanie maksymalnego zasięgu harpunu
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(firePoint.position, maxRange);

            // Rysowanie stożka widzenia
            Gizmos.color = Color.yellow;
            DrawViewCone(firePoint.position, firePoint.forward, maxHorizontalAngleToTarget, maxVerticalAngleToTarget, detectionRange);
        }
    }

    void DrawViewCone(Vector3 position, Vector3 direction, float horizontalAngle, float verticalAngle, float range)
    {
        // Rysowanie stożka widzenia w obu płaszczyznach
        Vector3 up = Quaternion.Euler(verticalAngle, 0, 0) * direction * range;
        Vector3 down = Quaternion.Euler(-verticalAngle, 0, 0) * direction * range;
        Vector3 left = Quaternion.Euler(0, -horizontalAngle, 0) * direction * range;
        Vector3 right = Quaternion.Euler(0, horizontalAngle, 0) * direction * range;

        Gizmos.DrawLine(position, position + up);
        Gizmos.DrawLine(position, position + down);
        Gizmos.DrawLine(position, position + left);
        Gizmos.DrawLine(position, position + right);

        // Połączenie krawędzi stożka
        Gizmos.DrawLine(position + up, position + left);
        Gizmos.DrawLine(position + up, position + right);
        Gizmos.DrawLine(position + down, position + left);
        Gizmos.DrawLine(position + down, position + right);
    }
}