using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public int damageAmount = 1;
    private int wheelCollisionCount = 0; // Licznik kolizji z Wheel

    private void OnTriggerEnter(Collider other)
    {
        // Sprawdü, czy obiekt ma komponent InteractableItem i uøywa systemu zdrowia
        var interactableItem = other.GetComponent<InteractableItem>();
        if (interactableItem != null && interactableItem.usesHealthSystem)
        {
            // Zadaj obraøenia obiektowi
            interactableItem.TakeDamage(damageAmount);
            // Debug.Log($"Dealt {damageAmount} damage to {interactableItem.gameObject.name}");
        }

        // Teraz sprawdzamy, czy obiekt, z ktÛrym dosz≥o do kolizji, ma tag "Wheel"
        if (other.CompareTag("Wheel"))
        {
            // ZwiÍksz licznik kolizji
            wheelCollisionCount++;

            // Sprawdzamy, czy kolizji by≥o juø 2
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
