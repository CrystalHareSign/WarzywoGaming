using UnityEngine;
using System.Collections;

public class PlayerShooting : MonoBehaviour
{
    public GameObject projectilePrefab;     // Prefab pocisku
    public Transform firePoint;             // Punkt wystrzału pocisku
    public Camera playerCamera;             // Odniesienie do kamery postaci
    public GameObject pistol;               // Model pistoletu
    [Header("Shooting Settings")]
    public float projectileSpeed = 20f;     // Prędkość pocisku
    public float fireRate = 0.2f;           // Częstotliwość strzałów
    public int maxAmmo = 10;                // Maksymalna pojemność magazynku
    public float reloadTime = 2f;           // Czas przeładowania

    protected int currentAmmo;              // Aktualna ilość amunicji w magazynku
    protected float nextFireTime = 0f;
    protected bool isReloading = false;

    void Start()
    {
        currentAmmo = maxAmmo;
    }

    void Update()
    {
        if (isReloading)
            return;

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
            return;
        }

        // Upewnij się, że model pistoletu podąża za kamerą (tylko rotacja)
        if (pistol != null && playerCamera != null)
        {
            pistol.transform.rotation = playerCamera.transform.rotation;
        }
    }

    void FixedUpdate()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime) // Sprawdzenie trzymanego lewego przycisku myszy
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
        Debug.Log("Reloaded");
    }

    public virtual void Shoot()
    {
        if (projectilePrefab != null && firePoint != null && playerCamera != null)
        {
            currentAmmo--;

            // Tworzenie instancji pocisku w punkcie wystrzału
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position + firePoint.forward * 0.5f, firePoint.rotation);
            Debug.Log("Pocisk został utworzony w pozycji: " + firePoint.position + " i kierunku: " + playerCamera.transform.forward);
            if (projectile != null)
            {
                // Nadanie pociskowi prędkości
                Rigidbody rb = projectile.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = playerCamera.transform.forward * projectileSpeed;
                    Debug.Log("Pocisk ma prędkość: " + rb.linearVelocity);
                }
                else
                {
                    Debug.LogWarning("Rigidbody nie został znaleziony na pocisku.");
                }
            }
            else
            {
                Debug.LogWarning("Pocisk nie został utworzony.");
            }
        }
        else
        {
            Debug.LogWarning("Bullet or fire point is not assigned.");
        }
    }
}
