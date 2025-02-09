using UnityEngine;

public class MovingObject : MonoBehaviour
{
    public float speed = 5f; // Pr�dko�� poruszania si� obiektu
    public Vector3 direction = Vector3.forward; // Kierunek poruszania si� obiektu
    public float lifetime = 15f; // Czas �ycia obiektu w sekundach

    void Start()
    {
        // Zniszczenie obiektu po okre�lonym czasie
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Przesuwanie obiektu w okre�lonym kierunku i pr�dko�ci
        transform.Translate(direction * speed * Time.deltaTime);
    }
}