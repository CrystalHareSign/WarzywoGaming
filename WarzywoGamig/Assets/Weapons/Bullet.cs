using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 20f;
    public int damage = 20; // Obrażenia zadawane przez pocisk
    public float lifeTime = 5f; // Czas życia pocisku w sekundach
    public Rigidbody rb;
    public GameObject impactEffect; // Efekt trafienia (przypisany w Inspectorze)

    void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>(); // Pobranie Rigidbody, jeśli nie jest przypisane
        }

        rb.linearVelocity = transform.forward * speed;  // ✅ Naprawione poruszanie się pocisku

        // Zniszczenie pocisku po określonym czasie
        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter(Collision hit)
    {
        if (hit.collider.CompareTag("Enemy")) // ✅ Upewniamy się, że trafiliśmy we wroga
        {
            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>(); // ✅ Zamieniamy `Zombie` na `EnemyHealth`
            if (enemy != null)
            {
                enemy.TakeDamage(damage);  // Zadaj obrażenia wrogowi
            }
        }

        if (impactEffect != null) // ✅ Dodajemy efekt trafienia (jeśli przypisany)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject); // ✅ Zniszczenie pocisku po trafieniu
    }
}
