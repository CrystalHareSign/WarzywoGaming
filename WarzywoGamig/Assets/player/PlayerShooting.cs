using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject projectilePrefab; // Prefab pocisku
    public Transform firePoint; // Punkt wystrzału pocisku
    public Camera playerCamera;
    [Header("Shooting Settings")]
    public float projectileSpeed = 20f; // Prędkość pocisku
    public float fireRate = 0.2f; // Częstotliwość strzałów

    private float nextFireTime = 0f;

    void Update()
    {
        if (Input.GetMouseButton(0)) // Sprawdzenie trzymanego lewego przycisku myszy
        {
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = (Time.time * Time.deltaTime) + fireRate;
            }
        }
    }

    void Shoot()
    {
        if (projectilePrefab != null && firePoint != null && playerCamera != null)
        {
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
                    //rb.useGravity = false; // Wyłączenie grawitacji dla pocisku, jeśli nie jest potrzebna
                    Debug.Log("Pocisk ma prędkość: " + rb.linearVelocity);
                }
                else
                {
                    Debug.LogWarning("Rigidbody nie został znaleziony na pocisku.");
                }
            }
        }
        else
        {
            Debug.LogWarning("Bullet or fire point is not assigned.");
        }
    }
}
