using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Security.Cryptography.X509Certificates;

public enum UsableType
{
    Quick,
    Context
}

public class InteractableItem : MonoBehaviour, IInteractable
{
    public System.Action onInteract;
    public HoverMessage hoverMessage;

    [Header("Nazwa | Opony MUSZ¥ siê tak nazywaæ")]
    public string itemName; // Nazwa przedmiotu

    [Header("Kategoria przedmiotu")]
    public string category;

    [Header("Iloœæ przedmiotu")]
    public int quantity = 0;

    [Header("Zaczyna grê jako Naprawione/Zepsute")]
    public bool startAsNonInteractive = false;

    [Header("System Itemów")]
    public bool canBePickedUp = false;
    public bool canBeDropped = false;
    public bool isWeapon;
    public bool isLoot = false;

    [Header("usable item")]
    public bool isUsableItem = false;
    public UsableType usableType = UsableType.Context;

    // --- NOWOŒÆ: referencja do ScriptableObject z baz¹ danych efektów ---
    [Header("Baza danych przedmiotu (ScriptableObject)")]
    public ItemPrefabData data;

    [Header("NPC Dialogue")]
    public bool isNPC = false;
    public List<DialogueData> npcDialogues;
    public int currentDialogueIndex = 0;

    [Header("Turret")]
    public bool isTurret = false;

    [Header("Refiner")]
    public bool isRefiner = false;

    [Header("Monitor")]
    public bool isMonitor = false;
    public bool busMonitor = false;

    [Header("Driver Seat")]
    public bool isDriverSeat = false;

    [Header("Mission Definer")]
    public bool isMissionDefiner = false;

    [Header("System kierowczy")]
    public bool alwaysInteractive = false;
    public bool hasCooldown = false;
    [SerializeField] private static float cooldownTime = 2f;
    private static bool isCooldownActive = false;

    [Header("System zdrowia BUSA")]
    public bool usesHealthSystem = false;
    public int maxHealth = 2;
    [SerializeField] private int currentHealth;
    public float requiredHoldTime = 0f;

    [Header("UI System Zdrowia")]
    public int wheelIndex;

    [Header("SCENY")]
    public bool UsingSceneSystem = false;
    public bool SceneMain = false;
    public bool SceneHome = false;

    public WheelHealthUI wheelHealthUI;
    public WheelManager wheelManager;
    public DialogueManager dialogueManager;

    private void Start()
    {
        hoverMessage = GetComponent<HoverMessage>();
        wheelHealthUI = Object.FindFirstObjectByType<WheelHealthUI>();
        dialogueManager = Object.FindFirstObjectByType<DialogueManager>();

        // --- Automatyczne podpinanie metody do onInteract na podstawie bazy danych ---
        if (isUsableItem && usableType == UsableType.Quick && data != null)
        {
            onInteract += UseDatabaseEffect;
        }

        if (dialogueManager == null)
        {
            Debug.LogError($"[ERROR] Brak komponentu DialogueManager na {gameObject.name}");
            return;
        }
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
            // Twój kod jeœli potrzebujesz
        }
        else if (isLoot)
        {
            Debug.LogWarning("GridManager.Instance is null when trying to add " + gameObject.name);
        }
    }

    // --- Nowa metoda: efekt na podstawie bazy danych ScriptableObject ---
    public void UseDatabaseEffect()
    {
        if (data == null) return;

        switch (data.effectType)
        {
            case ItemEffectType.Heal:
                Debug.Log($"Leczenie gracza o {data.effectValue} HP przez item: {itemName}");
                // PlayerStats.Instance.Heal(data.effectValue); // Odkomentuj jeœli masz PlayerStats
                break;
            case ItemEffectType.Boost:
                Debug.Log($"Dodano boost o wartoœci {data.effectValue} przez item: {itemName}");
                // PlayerStats.Instance.AddBuff("speed", data.effectValue);
                break;
            case ItemEffectType.Key:
                Debug.Log($"U¿yto klucza: {itemName}");
                // Obs³u¿ klucz w innym miejscu jeœli potrzeba
                break;
            case ItemEffectType.Custom:
                UseCustomEffect(data.customEffectID);
                break;
            case ItemEffectType.None:
            default:
                Debug.Log($"Przedmiot {itemName} nie ma przypisanego efektu.");
                break;
        }
    }

    // --- Customowe efekty obs³uguj tu ---
    public virtual void UseCustomEffect(string effectID)
    {
        switch (effectID)
        {
            case "Teleport":
                // np. Teleportuj gracza
                // PlayerController.Instance.TeleportTo(someLocation);
                Debug.Log("Teleportacja gracza!");
                break;
            case "SummonBoss":
                // np. Przywo³aj bossa
                // BossSpawner.Instance.SpawnBoss();
                Debug.Log("Przywo³ano bossa!");
                break;
            case "MySpecialEffect":
                // w³asna akcja
                Debug.Log("W³asny efekt specjalny!");
                break;
            default:
                Debug.LogWarning($"Nieznany custom effect: {effectID}");
                break;
        }
    }

    public void Interact()
    {
        if (hasCooldown && isCooldownActive)
        {
            return;
        }

        if (IsSceneActive())
        {
            if (isNPC && npcDialogues != null && npcDialogues.Count > 0 && dialogueManager != null)
            {
                dialogueManager.StartDialogue(npcDialogues[currentDialogueIndex], hoverMessage);
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
                    // Wywo³anie eventu lub dodatkowych interakcji
                    onInteract?.Invoke();
                    if (hoverMessage != null && !hoverMessage.alwaysActive)
                    {
                        hoverMessage.isInteracted = true;
                        InteractivityManager.Instance.UpdateInteractivityStatus(gameObject, false);
                    }
                }

                if (hasCooldown)
                {
                    SetAllInteractablesInteracted(true);
                    StartCoroutine(CooldownCoroutine());
                }
            }
        }
        else
        {
            Debug.LogWarning($"[WARNING] Obiekt {itemName} nie jest aktywny w tej scenie.");
        }
    }

    private bool IsSceneActive()
    {
        if (!UsingSceneSystem) return true;
        if (SceneMain && SceneManager.GetActiveScene().name == "Main") return true;
        if (SceneHome && SceneManager.GetActiveScene().name == "Home") return true;
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
            default: return -1;
        }
    }

    public void TakeDamage(int amount)
    {
        if (usesHealthSystem)
        {
            currentHealth = Mathf.Max(currentHealth - amount, 0);

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
            UpdateUI();

            if (currentHealth == maxHealth)
            {
                InteractivityManager.Instance.UpdateInteractivityStatus(gameObject, false);
                hoverMessage.isInteracted = true;
            }
        }
    }

    public void UpdateUI()
    {
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
        SetAllInteractablesInteracted(false);
    }

    private void SetAllInteractablesInteracted(bool state)
    {
        foreach (var item in Object.FindObjectsByType<InteractableItem>(FindObjectsSortMode.None))
        {
            if (item.hoverMessage != null)
                item.hoverMessage.isInteracted = state;
        }
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

    public void SetDialogueIndex(int newIndex)
    {
        if (npcDialogues != null && newIndex >= 0 && newIndex < npcDialogues.Count)
        {
            currentDialogueIndex = newIndex;
        }
    }

    public void NextDialogue()
    {
        if (npcDialogues != null && currentDialogueIndex < npcDialogues.Count - 1)
        {
            currentDialogueIndex++;
        }
    }
}