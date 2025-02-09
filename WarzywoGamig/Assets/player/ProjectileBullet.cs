using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] public int damage = 20;
    [SerializeField] public float lifeTime = 3.0f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // Sprawd�, czy obiekt, z kt�rym koliduje pocisk, istnieje i ma komponent EnemyHealth
        if (other != null)
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            // Sprawd�, czy obiekt pocisku istnieje, zanim go zniszczysz
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }
    }
}
