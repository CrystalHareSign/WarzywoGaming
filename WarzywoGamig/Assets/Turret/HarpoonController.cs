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
    private Vector3 initialScale;

    void Start()
    {
        // Inicjalizuj harpun i ustaw jako child firePoint
        currentHarpoon = Instantiate(harpoonPrefab, firePoint.position, firePoint.rotation, firePoint);
        harpoonRb = currentHarpoon.GetComponent<Rigidbody>();
        harpoonRb.isKinematic = true;
        initialScale = currentHarpoon.transform.localScale;
    }

    void Update()
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

    void ShootHarpoon()
    {
        // Odłącz harpun od firePoint i aktywuj go
        currentHarpoon.transform.SetParent(null);
        currentHarpoon.SetActive(true);

        Vector3 shootDirection = (GetMouseWorldPosition() - firePoint.position).normalized;
        harpoonRb.isKinematic = false;
        harpoonRb.velocity = shootDirection * shootSpeed;
        canShoot = false;
    }

    void ReturnHarpoon()
    {
        Vector3 returnDirection = (firePoint.position - currentHarpoon.transform.position).normalized;
        harpoonRb.velocity = returnDirection * returnSpeed;

        if (Vector3.Distance(currentHarpoon.transform.position, firePoint.position) < 0.5f)
        {
            // Zatrzymaj harpun i ustaw jako child firePoint
            harpoonRb.velocity = Vector3.zero;
            harpoonRb.isKinematic = true;
            currentHarpoon.transform.SetParent(firePoint);
            currentHarpoon.transform.position = firePoint.position;
            currentHarpoon.transform.rotation = firePoint.rotation;
            currentHarpoon.transform.localScale = initialScale;
            isReturning = false;
            canShoot = true;
        }
    }

    public void OnHarpoonCollision()
    {
        isReturning = true;
        harpoonRb.isKinematic = false;
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = Camera.main.transform.position.y; // Adjust z to be the distance from the camera
        return Camera.main.ScreenToWorldPoint(mouseScreenPosition);
    }
}