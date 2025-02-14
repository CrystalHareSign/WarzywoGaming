using UnityEngine;
using System.Collections;

public class InteractableItem : MonoBehaviour, IInteractable
{
    public System.Action onInteract;
    public HoverMessage hoverMessage;

    [Header("Nazwa | Opony MUSZ� sie tak nazywa�")]
    public string itemName; // Nazwa przedmiotu

    [Header("Zaczyna gr� jako Naprawione/Zepsute")]
    public bool startAsNonInteractive = false;

    [Header("System Item�w")]
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
    public int wheelIndex; // Indeks ko�a (0-3)

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
            Debug.LogWarning($"[COOLDOWN] Nie mo�na wej�� w interakcj� z {itemName}. Cooldown aktywny.");
            return;
        }

        if (hoverMessage.alwaysActive || InteractivityManager.Instance.IsInteractable(gameObject))
        {
            Debug.Log($"[INTERACT] {itemName} - Interakcja rozpocz�ta.");

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
            Debug.LogWarning($"[WARNING] Pr�ba interakcji z nieinteraktywnym przedmiotem: {itemName}");
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
                Debug.LogWarning($"[WARNING] Nieznana nazwa ko�a {itemName}, zwracam -1");
                return -1;
        }
    }

    public void TakeDamage(int amount)
    {
        if (usesHealthSystem)
        {
            currentHealth = Mathf.Max(currentHealth - amount, 0);
            Debug.Log($"[DAMAGE] {itemName} otrzyma� {amount} obra�e�. Aktualne zdrowie: {currentHealth}");

            // Znajd� UI i zaktualizuj stan zdrowia ko�a
            WheelHealthUI ui = FindFirstObjectByType<WheelHealthUI>();
            if (ui != null)
            {
                Debug.Log($"[UI UPDATE] Aktualizacja UI dla {itemName} (Ko�o {GetWheelIndex()}) na zdrowie {currentHealth}");
                ui.UpdateWheelHealth(GetWheelIndex(), currentHealth);
            }
            else
            {
                Debug.LogError("[ERROR] WheelHealthUI nie znaleziono w scenie!");
            }

            if (currentHealth == 0)
            {
                Debug.Log($"[BROKEN] {itemName} jest ca�kowicie zepsuty i wymaga naprawy.");
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

            // Znajd� UI i zaktualizuj stan zdrowia ko�a
            WheelHealthUI ui = FindFirstObjectByType<WheelHealthUI>();
            if (ui != null)
            {
                Debug.Log($"[UI UPDATE] Aktualizacja UI dla {itemName} (Ko�o {GetWheelIndex()}) na zdrowie {currentHealth}");
                ui.UpdateWheelHealth(GetWheelIndex(), currentHealth);
            }
            else
            {
                Debug.LogError("[ERROR] WheelHealthUI nie znaleziono w scenie!");
            }

            if (currentHealth == maxHealth)
            {
                Debug.Log($"[FULLY REPAIRED] {itemName} w pe�ni naprawione. Dezaktywacja interakcji.");
                InteractivityManager.Instance.UpdateInteractivityStatus(gameObject, false);
                hoverMessage.isInteracted = true;
            }
        }
        else
        {
            Debug.Log($"[INFO] {itemName} ju� w pe�ni naprawione.");
        }
    }

    private void UpdateUI()
    {
        if (wheelHealthUI != null)
        {
            Debug.Log($"[UI UPDATE] Aktualizacja UI dla ko�a {wheelIndex} na zdrowie {currentHealth}");
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
        Debug.Log($"[COOLDOWN] Rozpocz�cie cooldownu dla {itemName}");
        yield return new WaitForSeconds(cooldownTime);
        isCooldownActive = false;
        Debug.Log($"[COOLDOWN] Cooldown zako�czony dla {itemName}");
    }
}
