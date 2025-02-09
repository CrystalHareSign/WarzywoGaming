using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // SprawdŸ, czy obiekt, który wszed³ w kolizjê, jest przedmiotem interaktywnym

        Debug.Log("Triggered by: " + other.gameObject.name);

        var interactableItem = other.GetComponent<InteractableItem>();
        if (interactableItem != null)
        {
            // Odnów interaktywnoœæ obiektu
            InteractivityManager.Instance.RestoreInteractivity(interactableItem.gameObject);
            Debug.Log($"Interactivity restored for {interactableItem.gameObject.name} by {gameObject.name}");

            // Ustaw isInteracted na false
            var hoverMessage = interactableItem.GetComponent<HoverMessage>();
            if (hoverMessage != null)
            {
                hoverMessage.isInteracted = false;
            }
        }
        else
        {
            Debug.Log($"Obstacle collided with non-interactable object: {other.gameObject.name}");
        }
    }
}