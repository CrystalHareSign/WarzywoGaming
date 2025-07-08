using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaRegenPerSecond = 5f;
    public float staminaRegenDelay = 2f;
    private float staminaRegenTimer = 0f;
    [HideInInspector] public bool staminaExhausted = false;

    [Header("Bonus Stamina")]
    public float staminaBonus = 0f; // aktywny bonus (np. +50)
    [Header("Bonus Stamina Regen")]
    public float staminaRegenBonus = 0f; // tymczasowy bonus do regen (np. +5)

    [Header("Stamina Usage")]
    public float staminaUsage; // Moøesz ustawiÊ w Inspectorze, np. 10f

    private Coroutine staminaBonusCoroutine;

    // --- NEW: Boost timer for UI sync ---
    public float staminaBonusTimeLeft { get; private set; }
    public float staminaBonusDuration { get; private set; }

    public float TotalMaxStamina => maxStamina + staminaBonus;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
    }

    void Update()
    {
        // Sprawdü, czy stamina zosta≥a wyczerpana
        if (currentStamina <= 0f)
        {
            currentStamina = 0f;
            staminaExhausted = true;
        }

        RegenStamina();

        // --- NEW: Update boost timer if active ---
        if (staminaBonus > 0f && staminaBonusTimeLeft > 0f)
            staminaBonusTimeLeft = Mathf.Max(0f, staminaBonusTimeLeft - Time.unscaledDeltaTime);
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
        if (currentHealth == 0)
        {
            Die();
        }
    }

    public bool UseStamina(float amount)
    {
        if (amount <= 0f || staminaExhausted) return false;
        if (currentStamina > 0f)
        {
            currentStamina = Mathf.Clamp(currentStamina - amount, 0f, TotalMaxStamina);
            staminaRegenTimer = staminaRegenDelay;
            if (currentStamina == 0f)
                staminaExhausted = true;
            return true;
        }
        return false;
    }

    private void RegenStamina()
    {
        if (currentStamina < TotalMaxStamina)
        {
            if (staminaRegenTimer > 0f)
            {
                staminaRegenTimer -= Time.deltaTime;
            }
            else
            {
                float regen = staminaRegenPerSecond + staminaRegenBonus;
                currentStamina = Mathf.Clamp(currentStamina + regen * Time.deltaTime, 0f, TotalMaxStamina);
                // staminaExhausted resetowane w PlayerMovement po puszczeniu sprintu
            }
        }
    }

    /// <summary>
    /// Dodaje bonus do max staminy ORAZ taki sam procentowy bonus do regen staminy na czas trwania boosta.
    /// Np. +25% max stamina => +25% regen (bazujπc na staminaRegenPerSecond)
    /// </summary>
    public void UseEnergyDrink(float bonusAmount, float duration)
    {
        if (staminaBonusCoroutine != null)
            StopCoroutine(staminaBonusCoroutine);
        staminaBonusCoroutine = StartCoroutine(StaminaBonusRoutine(bonusAmount, duration));
    }

    private IEnumerator StaminaBonusRoutine(float bonus, float duration)
    {
        staminaBonus = bonus;
        staminaBonusDuration = duration;
        staminaBonusTimeLeft = duration;

        float percent = (maxStamina > 0f) ? (bonus / maxStamina) : 0f;
        staminaRegenBonus = staminaRegenPerSecond * percent;

        // NIE zmieniaj currentStamina przy starcie bonusu!

        float timer = 0f;
        while (timer < duration)
        {
            yield return null;
            timer += Time.unscaledDeltaTime;
            staminaBonusTimeLeft = Mathf.Max(0, duration - timer);
        }

        staminaBonus = 0f;
        staminaRegenBonus = 0f;
        currentStamina = Mathf.Min(currentStamina, maxStamina);

        staminaBonusTimeLeft = 0f;
        staminaBonusDuration = 0f;
        staminaBonusCoroutine = null;
    }

    private void Die()
    {
        Debug.Log("Gracz zginπ≥!");
        // Moøesz tu dodaÊ dodatkowπ logikÍ úmierci gracza, np. animacje, UI itp.
    }

    public void ResetStaminaExhaustion()
    {
        if (currentStamina > 0f)
            staminaExhausted = false;
    }

    public void ClearAllBonuses()
    {
        staminaBonus = 0f;
        staminaBonusDuration = 0f;
        staminaBonusTimeLeft = 0f;
        // Dodaj tu inne efekty jeúli masz (np. speedBoost, shield, itp.)
    }
}