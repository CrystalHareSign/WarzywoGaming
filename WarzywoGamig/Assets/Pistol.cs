using UnityEngine;

public class Pistol : PlayerShooting
{
    public override void Shoot()
    {
        if (currentAmmo > 0)
        {
            currentAmmo--;

            // Tworzenie instancji pocisku w punkcie wystrzału
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            if (projectile != null)
            {
                // Nadanie pociskowi prędkości
                Rigidbody rb = projectile.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = firePoint.forward * projectileSpeed;
                }
            }

            Debug.Log("Shot fired, remaining ammo: " + currentAmmo);
        }
        else
        {
            Debug.Log("No ammo, reload required.");
        }
    }
}
