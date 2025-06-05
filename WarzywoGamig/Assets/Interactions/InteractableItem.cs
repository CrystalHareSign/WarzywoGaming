using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class InteractableItem : MonoBehaviour, IInteractable
{
    public System.Action onInteract;
    public HoverMessage hoverMessage;

    [Header("Nazwa | Opony MUSZ� si� tak nazywa�")]
    public string itemName; // Nazwa przedmiotu

    [Header("Kategoria przedmiotu")]
    public string category; // <-- DODAJ TO POLE

    [Header("Ilo�� przedmiotu")]
    public int quantity = 0; // <-- DODAJ TO POLE

    [Header("Zaczyna gr� jako Naprawione/Zepsute")]
    public bool startAsNonInteractive = false;

    [Header("System Item�w")]
    public bool canBePickedUp = false;
    public bool canBeDropped = false;
    public bool isWeapon;    // Okre�la, czy przedmiot jest broni�
    public bool isLoot = false;  // Flaga, kt�ra decyduje, czy przedmiot jest lootem

    [Header("Turret")]
    public bool isTurret = false; // Okre�la, czy przedmiot jest wie�yczk�

    [Header("Refiner")]
    public bool isRefiner = false; //  czy jest Refiner

    [Header("Monitor")]
    public bool isMonitor = false; //  czy jest Refiner
    public bool busMonitor = false; //  czy jest Refiner

    [Header("System kierowczy")]
    public bool alwaysInteractive = false;
    public bool hasCooldown = false;
    [SerializeField] private static float cooldownTime = 2f;
    private static bool isCooldownActive = false;

    [Header("System zdrowia BUSA")]
    public bool usesHealthSystem = false; // Czy ten przedmiot korzysta z systemu zdrowia?
    public int maxHealth = 2; // Maksymalne zdrowie to 2
    [SerializeField] private int currentHealth; // Aktualne zdrowie
    public float requiredHoldTime = 0f; // Czas trzymania przycisku interakcji

    [Header("UI System Zdrowia")]
    public int wheelIndex; // Indeks ko�a (0-3)

    [Header("SCENY")]
    // Dodane boole do sprawdzania aktywnej sceny
    public bool UsingSceneSystem = false; // Nowy prze��cznik
    public bool SceneMain = false;
    public bool SceneHome = false;

    public WheelHealthUI wheelHealthUI;
    public WheelManager wheelManager;

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
            // Ustaw currentHealth tylko, je�li NIE trwa �adowanie save
            if (SaveManager.Instance == null || !SaveManager.Instance.isLoading)
            {
                currentHealth = maxHealth;
            }

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

        if (isLoot && GridManager.Instance != null)
        {

        }
        else if (isLoot)
        {
            Debug.LogWarning("GridManager.Instance is null when trying to add " + gameObject.name);
        }
    }

    public void Interact()
    {
        if (hasCooldown && isCooldownActive)
        {
            return;
        }

        // Sprawdzanie aktywnej sceny
        if (IsSceneActive())
        {
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
                //Debug.LogWarning($"[WARNING] Pr�ba interakcji z nieinteraktywnym obiektem: {itemName}");
            }
        }
        else
        {
            Debug.LogWarning($"[WARNING] Obiekt {itemName} nie jest aktywny w tej scenie.");
        }
    }
    private bool IsSceneActive()
    {
        if (!UsingSceneSystem)
        {
            return true; // Je�li nie korzystamy z systemu scen, zawsze zwracamy true
        }

        if (SceneMain && SceneManager.GetActiveScene().name == "Main")
        {
            return true;
        }

        if (SceneHome && SceneManager.GetActiveScene().name == "Home")
        {
            return true;
        }

        return false;
    }

    private int GetWheelIndex()
    {
        switch (itemName)
        {
            case "OponaLP": return 0;
            case "OponaPP": return 1;
            case "OponaLT": return 2;
            case "OponaPT": return 3;
            default: return -1; // Je�li nazwa nie pasuje
        }
    }

    public void TakeDamage(int amount)
    {
        if (usesHealthSystem)
        {
            currentHealth = Mathf.Max(currentHealth - amount, 0);
            //Debug.Log($"[LOG] {itemName} otrzyma� {amount} obra�e�. Aktualne zdrowie: {currentHealth}/{maxHealth}");

            // Upewnij si�, �e mo�na naprawia� od razu po obra�eniach
            if (currentHealth < maxHealth)
            {
                InteractivityManager.Instance.UpdateInteractivityStatus(gameObject, true);
                hoverMessage.isInteracted = false;
            }

            UpdateUI();
        }
    }

    public void SetCurrentHealth(int value)
    {
        currentHealth = Mathf.Clamp(value, 0, maxHealth);
        UpdateUI();
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
                //Debug.Log($"[LOG] {itemName} w pe�ni naprawiony. Wy��czanie interaktywno�ci.");
                InteractivityManager.Instance.UpdateInteractivityStatus(gameObject, false);
                hoverMessage.isInteracted = true;
            }
        }
    }

    public void UpdateUI()
    {
        // Za ka�dym razem pobierz aktualny WheelHealthUI:
        wheelHealthUI = Object.FindFirstObjectByType<WheelHealthUI>();
        if (wheelHealthUI != null)
        {
            wheelHealthUI.UpdateWheelHealth(GetWheelIndex(), currentHealth);
        }
    }

    private IEnumerator CooldownCoroutine()
    {
        isCooldownActive = true;
        yield return new WaitForSeconds(cooldownTime);
        isCooldownActive = false;
    }

    public void RefreshInteractivity()
    {
        if (usesHealthSystem)
        {
            if (currentHealth < maxHealth)
            {
                InteractivityManager.Instance.UpdateInteractivityStatus(gameObject, true);
                if (hoverMessage != null) hoverMessage.isInteracted = false;
            }
            else
            {
                InteractivityManager.Instance.UpdateInteractivityStatus(gameObject, false);
                if (hoverMessage != null) hoverMessage.isInteracted = true;
            }
        }
    }
}
