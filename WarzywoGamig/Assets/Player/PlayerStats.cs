using UnityEngine;
using System.Collections;
using TMPro; // U¿ywaj TMP_Text zamiast Text

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("Health")]
    public float maxHealth = 1000f;
    public float currentHealth = 1000f;

    [Header("Health Regen")]
    public bool enableHealthRegen = true;
    public float healthRegenDelay = 2f; // czas oczekiwania po otrzymaniu obra¿eñ
    public float healthRegenPerSecond = 2f; // sta³a prêdkoœæ regeneracji HP
    private float healthRegenTimer = 0f;

    [Header("HP UI (Canvas)")]
    public TMP_Text hpText; // Przypisz w Inspectorze TMP_Text z Canvasu

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
    public float staminaUsage; // Mo¿esz ustawiæ w Inspectorze, np. 10f

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

        UpdateHpUI();
    }

    void Update()
    {
        // --- HP REGEN ---
        if (enableHealthRegen)
            HealthRegenTick();

        // --- STAMINA ---
        if (currentStamina <= 0f)
        {
            currentStamina = 0f;
            staminaExhausted = true;
        }

        RegenStamina();

        // --- NEW: Update boost timer if active ---
        if (staminaBonus > 0f && staminaBonusTimeLeft > 0f)
            staminaBonusTimeLeft = Mathf.Max(0f, staminaBonusTimeLeft - Time.deltaTime);

        // --- Aktualizuj licznik HP na UI ---
        UpdateHpUI();
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
        UpdateHpUI();
    }

    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
        healthRegenTimer = healthRegenDelay;
        UpdateHpUI();
        if (currentHealth == 0)
        {
            Die();
        }
    }

    private void HealthRegenTick()
    {
        if (currentHealth >= maxHealth || currentHealth <= 0f)
            return;

        if (healthRegenTimer > 0f)
        {
            healthRegenTimer -= Time.deltaTime;
            return;
        }

        float regenLimit = GetRegenLimit();
        currentHealth = Mathf.Clamp(currentHealth + healthRegenPerSecond * Time.deltaTime, 0f, regenLimit);
    }

    // Regen nie przekracza progów: 200, 400, 600, 800, 1000 (lub maxHealth)
    private float GetRegenLimit()
    {
        if (currentHealth <= 200f)
            return 200f;
        else if (currentHealth <= 400f)
            return 400f;
        else if (currentHealth <= 600f)
            return 600f;
        else if (currentHealth <= 800f)
            return 800f;
        else
            return maxHealth;
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
    /// Np. +25% max stamina => +25% regen (bazuj¹c na staminaRegenPerSecond)
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
            timer += Time.deltaTime;
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
        Debug.Log("Gracz zgin¹³!");
        // Mo¿esz tu dodaæ dodatkow¹ logikê œmierci gracza, np. animacje, UI itp.
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
        // Dodaj tu inne efekty jeœli masz (np. speedBoost, shield, itp.)
    }

    private void UpdateHpUI()
    {
        if (hpText != null)
        {
            hpText.text = Mathf.RoundToInt(currentHealth).ToString();
        }
    }
}