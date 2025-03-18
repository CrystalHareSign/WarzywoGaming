using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public int damageAmount = 1;
    private int wheelCollisionCount = 0; // Licznik kolizji z Wheel

    private void OnTriggerEnter(Collider other)
    {
        // Sprawd�, czy obiekt ma komponent InteractableItem i u�ywa systemu zdrowia
        var interactableItem = other.GetComponent<InteractableItem>();
        if (interactableItem != null && interactableItem.usesHealthSystem)
        {
            // Zadaj obra�enia obiektowi
            interactableItem.TakeDamage(damageAmount);
            // Debug.Log($"Dealt {damageAmount} damage to {interactableItem.gameObject.name}");
        }

        // Teraz sprawdzamy, czy obiekt, z kt�rym dosz�o do kolizji, ma tag "Wheel"
        if (other.CompareTag("Wheel"))
        {
            // Zwi�ksz licznik kolizji
            wheelCollisionCount++;

            // Sprawdzamy, czy kolizji by�o ju� 2
            if (wheelCollisionCount >= 2)
            {
                // Zniszcz obiekt po 2 kolizjach z Wheel
                Destroy(gameObject);
                Debug.Log("Obstacle destroyed after 2 collisions with 'Wheel'");
            }
            else
            {
                Debug.Log($"Collision with 'Wheel' detected. {2 - wheelCollisionCount} more to destroy.");
            }
        }
    }
}
