using UnityEngine;

public class MovingObject : MonoBehaviour
{
    public float speed { get; private set; } // Ustawiamy publiczny getter i prywatny setter
    public float lifetime { get; private set; }

    public void Initialize(float objectSpeed, float objectLifetime)
    {
        speed = objectSpeed;
        lifetime = objectLifetime;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }
}
