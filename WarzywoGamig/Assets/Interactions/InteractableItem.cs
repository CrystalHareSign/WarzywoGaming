using UnityEngine;
using System.Collections;

public class InteractableItem : MonoBehaviour, IInteractable
{
    public System.Action onInteract;
    public HoverMessage hoverMessage;

    [Header("Nazwa | Opony MUSZ¥ sie tak nazywaæ")]
    public string itemName; // Nazwa przedmiotu

    [Header("Zaczyna grê jako Naprawione/Zepsute")]
    public bool startAsNonInteractive = false;

    [Header("System Itemów")]
    public bool canBePickedUp = true;
    public bool canBeDropped = true;

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

        Debug.Log($"[START] {itemName} - usesHealthSystem: {usesHealthSystem}, maxHealth: {maxHealth}");

        if (hoverMessage == null)
        {
            Debug.LogError($"[ERROR] HoverMessage component not found on {gameObject.name}");
            return;
        }

        if (InteractivityManager.Instance == null)
        {
            Debug.LogError($"[ERROR] InteractivityManager instance not found.");
            return;
        }

        if (usesHealthSystem)
        {
            currentHealth = maxHealth;
            UpdateUI();
        }

        if (startAsNonInteractive || (usesHealthSystem && currentHealth == maxHealth))
        {
            Debug.Log($"[INFO] {itemName} zarejestrowany jako NIE-interaktywny.");
            InteractivityManager.Instance.RegisterAsNonInteractive(gameObject);
        }
        else
        {
            Debug.Log($"[INFO] {itemName} zarejestrowany jako INTERAKTYWNY.");
            InteractivityManager.Instance.RegisterInteractable(gameObject, alwaysInteractive);
        }
    }

    public void Interact()
    {
        if (hasCooldown && isCooldownActive)
        {
            Debug.LogWarning($"[COOLDOWN] Nie mo¿na wejœæ w interakcjê z {itemName}. Cooldown aktywny.");
            return;
        }

        if (hoverMessage.alwaysActive || InteractivityManager.Instance.IsInteractable(gameObject))
        {
            Debug.Log($"[INTERACT] {itemName} - Interakcja rozpoczêta.");

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
            Debug.LogWarning($"[WARNING] Próba interakcji z nieinteraktywnym przedmiotem: {itemName}");
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
            default:
                Debug.LogWarning($"[WARNING] Nieznana nazwa ko³a {itemName}, zwracam -1");
                return -1;
        }
    }

    public void TakeDamage(int amount)
    {
        if (usesHealthSystem)
        {
            currentHealth = Mathf.Max(currentHealth - amount, 0);
            Debug.Log($"[DAMAGE] {itemName} otrzyma³ {amount} obra¿eñ. Aktualne zdrowie: {currentHealth}");

            // ZnajdŸ UI i zaktualizuj stan zdrowia ko³a
            WheelHealthUI ui = FindFirstObjectByType<WheelHealthUI>();
            if (ui != null)
            {
                Debug.Log($"[UI UPDATE] Aktualizacja UI dla {itemName} (Ko³o {GetWheelIndex()}) na zdrowie {currentHealth}");
                ui.UpdateWheelHealth(GetWheelIndex(), currentHealth);
            }
            else
            {
                Debug.LogError("[ERROR] WheelHealthUI nie znaleziono w scenie!");
            }

            if (currentHealth == 0)
            {
                Debug.Log($"[BROKEN] {itemName} jest ca³kowicie zepsuty i wymaga naprawy.");
                InteractivityManager.Instance.UpdateInteractivityStatus(gameObject, true);
                hoverMessage.isInteracted = false;
            }
        }
    }

    private void RepairItem()
    {
        if (usesHealthSystem && currentHealth < maxHealth)
        {
            currentHealth++;
            Debug.Log($"[REPAIR] {itemName} naprawione. Aktualne zdrowie: {currentHealth}");

            // ZnajdŸ UI i zaktualizuj stan zdrowia ko³a
            WheelHealthUI ui = FindFirstObjectByType<WheelHealthUI>();
            if (ui != null)
            {
                Debug.Log($"[UI UPDATE] Aktualizacja UI dla {itemName} (Ko³o {GetWheelIndex()}) na zdrowie {currentHealth}");
                ui.UpdateWheelHealth(GetWheelIndex(), currentHealth);
            }
            else
            {
                Debug.LogError("[ERROR] WheelHealthUI nie znaleziono w scenie!");
            }

            if (currentHealth == maxHealth)
            {
                Debug.Log($"[FULLY REPAIRED] {itemName} w pe³ni naprawione. Dezaktywacja interakcji.");
                InteractivityManager.Instance.UpdateInteractivityStatus(gameObject, false);
                hoverMessage.isInteracted = true;
            }
        }
        else
        {
            Debug.Log($"[INFO] {itemName} ju¿ w pe³ni naprawione.");
        }
    }

    private void UpdateUI()
    {
        if (wheelHealthUI != null)
        {
            Debug.Log($"[UI UPDATE] Aktualizacja UI dla ko³a {wheelIndex} na zdrowie {currentHealth}");
            wheelHealthUI.UpdateWheelHealth(wheelIndex, currentHealth);
        }
        else
        {
            Debug.LogError("[ERROR] wheelHealthUI nie istnieje! UI nie zostanie zaktualizowane.");
        }
    }

    private IEnumerator CooldownCoroutine()
    {
        isCooldownActive = true;
        Debug.Log($"[COOLDOWN] Rozpoczêcie cooldownu dla {itemName}");
        yield return new WaitForSeconds(cooldownTime);
        isCooldownActive = false;
        Debug.Log($"[COOLDOWN] Cooldown zakoñczony dla {itemName}");
    }
}
