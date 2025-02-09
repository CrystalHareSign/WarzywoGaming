using UnityEngine;

public class RestoreInteractivityExample : MonoBehaviour
{
    public GameObject interactableItem; // Przedmiot, który ma zostaæ ponownie interaktywny
    public float restoreTime = 5f; // Czas po którym przedmiot stanie siê ponownie interaktywny

    void Start()
    {
        if (interactableItem == null)
        {
            Debug.LogError("InteractableItem is not assigned.");
            return;
        }

        // Przywróæ interaktywnoœæ po okreœlonym czasie
        Invoke(nameof(RestoreInteractivity), restoreTime);
    }

    void RestoreInteractivity()
    {
        if (InteractivityManager.Instance == null)
        {
            Debug.LogError("InteractivityManager instance not found.");
            return;
        }

        InteractivityManager.Instance.RestoreInteractivity(interactableItem);
        Debug.Log("Interactivity restored for item: " + interactableItem.name);
    }
}