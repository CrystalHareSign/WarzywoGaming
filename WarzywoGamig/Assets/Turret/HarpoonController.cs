using UnityEngine;

public class HarpoonController : MonoBehaviour
{
    public GameObject harpoonPrefab;
    public Transform firePoint;
    public float shootSpeed = 20f;
    public float returnSpeed = 10f;

    private GameObject currentHarpoon;
    private Rigidbody harpoonRb;
    private bool isReturning = false;
    private bool canShoot = true;
    private Vector3 initialScale = new Vector3(1, 1, 1);

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
        }
        else
        {
            //Debug.LogError("turretController nie jest przypisany lub wieżyczka nie jest używana ani podniesiona.");
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

    public void OnHarpoonCollision()
    {
        if (harpoonRb != null)
        {
            isReturning = true;
            harpoonRb.isKinematic = false;
        }
        else
        {
            Debug.LogError("harpoonRb nie jest przypisany.");
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = Camera.main.transform.position.y; // Adjust z to be the distance from the camera
        return Camera.main.ScreenToWorldPoint(mouseScreenPosition);
    }
}