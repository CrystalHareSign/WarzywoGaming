using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Sprawd�, czy obiekt, kt�ry wszed� w kolizj�, jest przedmiotem interaktywnym

        Debug.Log("Triggered by: " + other.gameObject.name);

        var interactableItem = other.GetComponent<InteractableItem>();
        if (interactableItem != null)
        {
            // Odn�w interaktywno�� obiektu
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