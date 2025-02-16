using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header ("Bullet Settings")]
    public float speed = 20f;
    public int damage = 20; // Obrażenia zadawane przez pocisk
    public float lifeTime = 5f; // Czas życia pocisku w sekundach
    public Rigidbody rb;

    void Start()
    {
        rb.linearVelocity = transform.forward * speed;  // Pocisk porusza się w kierunku przodu

        // Zniszczenie pocisku po określonym czasie
        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter(Collision hit)
    {
        Zombie zombie = hit.collider.GetComponent<Zombie>(); // Sprawdzamy, czy pocisk trafił w zombie
        if (zombie != null)
        {
            zombie.TakeDamage(damage);  // Zadaj obrażenia zombie
        }
        Destroy(gameObject);  // Zniszczenie pocisku po kolizji
    }
}
