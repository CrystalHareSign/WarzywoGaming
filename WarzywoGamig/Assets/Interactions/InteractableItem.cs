using UnityEngine;
using System.Collections;

public class InteractableItem : MonoBehaviour, IInteractable
{
    public System.Action onInteract;
    public HoverMessage hoverMessage;

    [Header("Nazwa | Opony MUSZ¥ siê tak nazywaæ")]
    public string itemName; // Nazwa przedmiotu

    [Header("Zaczyna grê jako Naprawione/Zepsute")]
    public bool startAsNonInteractive = false;

    [Header("System Itemów")]
    public bool canBePickedUp = true;
    public bool canBeDropped = true;
    public bool isWeapon;    // Okreœla, czy przedmiot jest broni¹

    [Header("System kierowczy")]
    public bool alwaysInteractive = false;
    public bool hasCooldown = false;
    [SerializeField] private static float cooldownTime = 2f;
    private static bool isCooldownActive = false;

    [Header("System zdrowia BUSA")]
    public bool usesHealthSystem = false; // Czy ten przedmiot korzysta z systemu zdrowia?
    public int maxHealth = 2; // Maksymalne zdrowie to 2
    [SerializeField] private int currentHealth; // Aktualne zdrowie
    public float requiredHoldTime = 5f; // Czas trzymania przycisku interakcji

    [Header("UI System Zdrowia")]
    public int wheelIndex; // Indeks ko³a (0-3)

    private WheelHealthUI wheelHealthUI;

    private void Start()
    {
        hoverMessage = GetComponent<HoverMessage>();
        wheelHealthUI = Object.FindFirstObjectByType<WheelHealthUI>();

        if (hoverMessage == null)
        {
            Debug.LogError($"[ERROR] Brak komponentu HoverMessage na {gameObject.name}");
            return;
        }

        if (InteractivityManager.Instance == null)
        {
            Debug.LogError("[ERROR] Brak instancji InteractivityManager.");
            return;
        }

        if (usesHealthSystem)
        {
            currentHealth = maxHealth;
            InteractivityManager.Instance.RegisterInteractable(gameObject, alwaysInteractive);
            UpdateUI();
        }
        else if (startAsNonInteractive)
        {
            InteractivityManager.Instance.RegisterAsNonInteractive(gameObject);
        }
        else
        {
            InteractivityManager.Instance.RegisterInteractable(gameObject, alwaysInteractive);
        }

        //Debug.Log($"[LOG] {itemName} zarejestrowany. Zdrowie: {currentHealth}/{maxHealth}");
    }

    public void Interact()
    {
        if (hasCooldown && isCooldownActive)
        {
            Debug.LogWarning($"[WARNING] Nie mo¿na wejœæ w interakcjê z {itemName}. Cooldown aktywny.");
            return;
        }

        if (hoverMessage.alwaysActive || InteractivityManager.Instance.IsInteractable(gameObject))
        {
            if (usesHealthSystem)
            {
                RepairItem();
            }
            else
            {
                onInteract?.Invoke();
                if (hoverMessage != null && !hoverMessage.alwaysActive)
                {
                    hoverMessage.isInteracted = true;
                    InteractivityManager.Instance.UpdateInteractivityStatus(gameObject, false);
                }
            }

            if (hasCooldown)
            {
                StartCoroutine(CooldownCoroutine());
            }
        }
        else
        {
            Debug.LogWarning($"[WARNING] Próba interakcji z nieinteraktywnym obiektem: {itemName}");
        }
    }

    private int GetWheelIndex()
    {
        switch (itemName)
        {
            case "OponaLP": return 0;
            case "OponaPP": return 1;
            case "OponaLT": return 2;
            case "OponaPT": return 3;
            default: return -1; // Jeœli nazwa nie pasuje
        }
    }

    public void TakeDamage(int amount)
    {
        if (usesHealthSystem)
        {
            currentHealth = Mathf.Max(currentHealth - amount, 0);
            Debug.Log($"[LOG] {itemName} otrzyma³ {amount} obra¿eñ. Aktualne zdrowie: {currentHealth}/{maxHealth}");

            // Upewnij siê, ¿e mo¿na naprawiaæ od razu po obra¿eniach
            if (currentHealth < maxHealth)
            {
                InteractivityManager.Instance.UpdateInteractivityStatus(gameObject, true);
                hoverMessage.isInteracted = false;
            }

            UpdateUI();
        }
    }

    private void RepairItem()
    {
        if (usesHealthSystem && currentHealth < maxHealth)
        {
            currentHealth++;
            //Debug.Log($"[LOG] {itemName} naprawiony. Aktualne zdrowie: {currentHealth}/{maxHealth}");

            UpdateUI();

            if (currentHealth == maxHealth)
            {
                //Debug.Log($"[LOG] {itemName} w pe³ni naprawiony. Wy³¹czanie interaktywnoœci.");
                InteractivityManager.Instance.UpdateInteractivityStatus(gameObject, false);
                hoverMessage.isInteracted = true;
            }
        }
    }

    private void UpdateUI()
    {
        if (wheelHealthUI != null)
        {
            wheelHealthUI.UpdateWheelHealth(GetWheelIndex(), currentHealth);
        }
        else
        {
            //Debug.LogWarning($"[WARNING] WheelHealthUI nie znaleziony dla {itemName}");
        }
    }

    private IEnumerator CooldownCoroutine()
    {
        isCooldownActive = true;
        yield return new WaitForSeconds(cooldownTime);
        isCooldownActive = false;
    }
}
