using UnityEngine;

public class HarpoonController : MonoBehaviour
{
    public GameObject harpoonPrefab;
    public Transform firePoint;
    public float shootSpeed = 20f;
    public float returnSpeed = 10f;
    public float maxRange = 50f; // Maksymalny zasięg harpunu
    public float maxAngleToTarget = 30f; // Maksymalny kąt, w którym harpun naprowadza się na cel
    public float detectionRange = 100f; // Maksymalny zasięg wykrywania celu

    private GameObject currentHarpoon;
    private Rigidbody harpoonRb;
    private bool isReturning = false;
    private bool canShoot = true;
    private Vector3 initialScale = new Vector3(1, 1, 1);
    private float currentShootDistance = 0f; // Aktualna odległość przebyta przez harpun
    private Vector3 shootPosition; // Pozycja z której został wystrzelony harpun

    private TurretController turretController; // Referencja do TurretController

    void Start()
    {
        // Automatyczne przypisanie TurretController
        turretController = FindObjectOfType<TurretController>();

        if (harpoonPrefab != null && firePoint != null)
        {
            // Inicjalizuj harpun i ustaw jako child firePoint
            currentHarpoon = Instantiate(harpoonPrefab, firePoint.position, firePoint.rotation);
            currentHarpoon.transform.SetParent(firePoint);
            harpoonRb = currentHarpoon.GetComponent<Rigidbody>();
            harpoonRb.isKinematic = true;
            currentHarpoon.transform.localScale = initialScale; // Ustaw skalę na 1x1x1
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
            if (Input.GetMouseButtonDown(0) && canShoot)
            {
                GameObject target = FindClosestTreasureInView();
                if (target != null)
                {
                    //Debug.Log("Found target in view: " + target.name);
                    Vector3 predictedPosition = PredictTargetPosition(target);
                    //Debug.Log("Predicted position: " + predictedPosition);
                    ShootHarpoon(predictedPosition);
                }
                else
                {
                    Vector3 shootDirection = GetMouseWorldPosition();
                    //Debug.Log("Shooting in mouse direction: " + shootDirection);
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
            //Debug.Log("Current shoot distance: " + currentShootDistance);
        }
    }

    void ShootHarpoon(Vector3 targetPosition)
    {
        if (currentHarpoon != null)
        {
            // Odłącz harpun od firePoint i aktywuj go
            currentHarpoon.transform.SetParent(null);
            currentHarpoon.SetActive(true);

            Vector3 shootDirection = (targetPosition - firePoint.position).normalized;
            harpoonRb.isKinematic = false;
            harpoonRb.velocity = shootDirection * shootSpeed;
            canShoot = false;

            // Zapisz pozycję z której harpun został wystrzelony
            shootPosition = firePoint.position;
            currentShootDistance = 0f;

            //Debug.Log("Harpoon shot towards: " + targetPosition + " with direction: " + shootDirection);
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
            harpoonRb.velocity = returnDirection * returnSpeed;

            if (Vector3.Distance(currentHarpoon.transform.position, firePoint.position) < 0.5f)
            {
                // Zatrzymaj harpun i ustaw jako child firePoint
                harpoonRb.velocity = Vector3.zero;
                harpoonRb.isKinematic = true;
                currentHarpoon.transform.SetParent(firePoint);
                currentHarpoon.transform.localPosition = Vector3.zero;
                currentHarpoon.transform.localRotation = Quaternion.identity;
                currentHarpoon.transform.localScale = initialScale; // Ustaw skalę na 1x1x1
                isReturning = false;
                canShoot = true;
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
            float angleToTreasure = Vector3.Angle(firePoint.forward, directionToTreasure);
            float distanceToTreasure = Vector3.Distance(firePoint.position, treasure.transform.position);

            if (angleToTreasure <= maxAngleToTarget && distanceToTreasure <= detectionRange)
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
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = Camera.main.transform.position.y; // Dostosuj z do odległości od kamery
        return Camera.main.ScreenToWorldPoint(mouseScreenPosition);
    }
}