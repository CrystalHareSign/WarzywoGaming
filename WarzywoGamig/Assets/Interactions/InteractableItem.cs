using UnityEngine;

public class InteractableItem : MonoBehaviour, IInteractable
{
    public string itemName; // Nazwa przedmiotu
    public float requiredHoldTime = 5f; // Czas trzymania przycisku interakcji w sekundach wymagany do aktywacji
    public HoverMessage hoverMessage; // Odniesienie do komponentu HoverMessage
    public bool alwaysInteractive = false; // Czy obiekt jest zawsze interaktywny
    public bool canBePickedUp = true; // Czy przedmiot mo¿e byæ podnoszony
    public bool canBeDropped = true; // Czy przedmiot mo¿e byæ upuszczany
    public bool hasCooldown = false; // Czy przedmiot ma cooldown
    public System.Action onInteract; // Delegat przechowuj¹cy funkcjê do wykonania po interakcji
    private static bool isCooldownActive = false; // Czy cooldown jest aktywny
    private static float cooldownTime = 2f; // Czas trwania cooldown w sekundach

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
        // SprawdŸ, czy cooldown jest aktywny tylko dla przedmiotów z cooldown
        if (hasCooldown && isCooldownActive)
        {
            Debug.LogWarning($"Cannot interact with {itemName}. Cooldown is active.");
            return;
        }

        // SprawdŸ, czy obiekt jest interaktywny przed interakcj¹ lub czy jest zawsze aktywny
        if (hoverMessage.alwaysActive || InteractivityManager.Instance.IsInteractable(gameObject))
        {
            // Wykonaj przypisan¹ funkcjê interakcji, jeœli istnieje
            onInteract?.Invoke();

            // Oznacz obiekt jako interaktowany
            if (hoverMessage != null && !hoverMessage.alwaysActive)
            {
                hoverMessage.isInteracted = true;
                InteractivityManager.Instance.UpdateInteractivityStatus(gameObject, false);
            }

            // Aktywuj cooldown tylko dla przedmiotów z cooldown
            if (hasCooldown)
            {
                StartCoroutine(CooldownCoroutine());
            }
        }
        else
        {
            Debug.LogWarning("Attempted to interact with a non-interactive item: " + itemName);
        }
    }

    private System.Collections.IEnumerator CooldownCoroutine()
    {
        isCooldownActive = true;
        yield return new WaitForSeconds(cooldownTime);
        isCooldownActive = false;
    }
}