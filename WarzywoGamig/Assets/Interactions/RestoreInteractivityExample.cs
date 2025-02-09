using UnityEngine;

public class RestoreInteractivityExample : MonoBehaviour
{
    public GameObject interactableItem; // Przedmiot, kt�ry ma zosta� ponownie interaktywny
    public float restoreTime = 5f; // Czas po kt�rym przedmiot stanie si� ponownie interaktywny

    void Start()
    {
        if (interactableItem == null)
        {
            Debug.LogError("InteractableItem is not assigned.");
            return;
        }

        // Przywr�� interaktywno�� po okre�lonym czasie
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