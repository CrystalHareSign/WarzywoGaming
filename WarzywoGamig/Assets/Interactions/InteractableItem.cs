using UnityEngine;

public class InteractableItem : MonoBehaviour, IInteractable
{
    public string itemName; // Nazwa przedmiotu
    public float requiredHoldTime = 5f; // Czas trzymania przycisku interakcji w sekundach wymagany do aktywacji
    public HoverMessage hoverMessage; // Odniesienie do komponentu HoverMessage
    public bool alwaysInteractive = false; // Czy obiekt jest zawsze interaktywny
    private void Start()
    {
        // Pobierz komponent HoverMessage z obiektu
        hoverMessage = GetComponent<HoverMessage>();

        if (hoverMessage == null)
        {
            Debug.LogError("HoverMessage component not found on " + gameObject.name);
            return;
        }

        // Upewnij siê, ¿e InteractivityManager jest zainicjalizowany
        if (InteractivityManager.Instance == null)
        {
            Debug.LogError("InteractivityManager instance not found.");
            return;
        }

        // Zarejestruj obiekt w InteractivityManager
        InteractivityManager.Instance.RegisterInteractable(gameObject, alwaysInteractive);
        Debug.Log($"InteractableItem {itemName} registered.");
    }

    public void Interact()
    {
        // SprawdŸ, czy obiekt jest interaktywny przed interakcj¹ lub czy jest zawsze aktywny
        if (hoverMessage.alwaysActive || InteractivityManager.Instance.IsInteractable(gameObject))
        {
            //////////  FUNKCJA //////////

            // Oznacz obiekt jako interaktowany
            if (hoverMessage != null && !hoverMessage.alwaysActive)
            {
                hoverMessage.isInteracted = true;
                InteractivityManager.Instance.UpdateInteractivityStatus(gameObject, false);
            }
        }
        else
        {
            Debug.LogWarning("Attempted to interact with a non-interactive item: " + itemName);
        }
    }
}