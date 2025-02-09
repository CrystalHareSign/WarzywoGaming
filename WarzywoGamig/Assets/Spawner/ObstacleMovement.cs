using UnityEngine;

public class MovingObject : MonoBehaviour
{
    public float speed = 5f; // Prêdkoœæ poruszania siê obiektu
    public Vector3 direction = Vector3.forward; // Kierunek poruszania siê obiektu
    public float lifetime = 15f; // Czas ¿ycia obiektu w sekundach

    void Start()
    {
        // Zniszczenie obiektu po okreœlonym czasie
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Przesuwanie obiektu w okreœlonym kierunku i prêdkoœci
        transform.Translate(direction * speed * Time.deltaTime);
    }
}