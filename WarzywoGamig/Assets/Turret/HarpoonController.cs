using UnityEngine;

public class HarpoonController : MonoBehaviour
{
    public GameObject harpoonPrefab;
    public Transform firePoint;
    public float shootSpeed = 20f;
    public float returnSpeed = 10f;
    public float maxRange = 50f; // Maksymalny zasięg harpunu

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
                ShootHarpoon();
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
        else
        {
            // Debug.LogError("turretController nie jest przypisany lub wieżyczka nie jest używana ani podniesiona.");
        }

        // Oblicz aktualną odległość przebyta przez harpun
        if (currentHarpoon != null && !harpoonRb.isKinematic)
        {
            currentShootDistance = Vector3.Distance(shootPosition, currentHarpoon.transform.position);
        }
    }

    void ShootHarpoon()
    {
        if (currentHarpoon != null)
        {
            // Odłącz harpun od firePoint i aktywuj go
            currentHarpoon.transform.SetParent(null);
            currentHarpoon.SetActive(true);

            Vector3 shootDirection = (GetMouseWorldPosition() - firePoint.position).normalized;
            harpoonRb.isKinematic = false;
            harpoonRb.velocity = shootDirection * shootSpeed;
            canShoot = false;

            // Zapisz pozycję z której harpun został wystrzelony
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

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = Camera.main.transform.position.y; // Dostosuj z do odległości od kamery
        return Camera.main.ScreenToWorldPoint(mouseScreenPosition);
    }
}