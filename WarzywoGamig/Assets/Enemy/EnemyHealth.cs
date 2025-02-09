using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy Health")]
    [SerializeField]public int maxHealth = 100;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Dodaj tu kod, kt�ry uruchomi si�, gdy wr�g zginie (np. zniszczenie obiektu)
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Projectile")) // Upewnij si�, �e pociski maj� tag "Projectile"
        {
            TakeDamage(other.GetComponent<Projectile>().damage);
            Destroy(other.gameObject); // Zniszcz pocisk po trafieniu
        }
    }
}
