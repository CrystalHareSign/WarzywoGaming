using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 20f;
    public int damage = 20;
    public float lifeTime = 5f;
    public Rigidbody rb;
    public GameObject impactEffect;

    void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        rb.linearVelocity = transform.forward * speed;

        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Bullet hit: " + collision.collider.name);

        if (collision.collider.CompareTag("Enemy"))
        {
            // Szukaj EnemyHealth na root obiekcie wroga
            EnemyHealth enemy = collision.collider.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
            {
                Debug.Log("Dealing damage to enemy!");
                enemy.TakeDamage(damage);
            }
        }

        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}