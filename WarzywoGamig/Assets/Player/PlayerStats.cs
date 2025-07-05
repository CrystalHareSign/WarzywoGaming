using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth =100f;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaRegenPerSecond = 5f;
    public float staminaRegenDelay = 2f;
    private float staminaRegenTimer = 0f;
    [HideInInspector] public bool staminaExhausted = false;

    [Header("Bonus Stamina")]
    public float staminaBonus = 0f; // aktywny bonus (np. +50)

    [Header("Stamina Usage")]
    public float staminaUsage; // Mo¿esz ustawiæ w Inspectorze, np. 10f

    private Coroutine staminaBonusCoroutine;

    /// <summary>
    /// Maksymalna stamina, uwzglêdniaj¹c bonus.
    /// </summary>
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
        // SprawdŸ, czy stamina zosta³a wyczerpana
        if (currentStamina <= 0f)
        {
            currentStamina = 0f;
            staminaExhausted = true;
        }

        RegenStamina();
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

    /// <summary>
    /// U¿yj staminy (np. podczas sprintu). Zwraca true, jeœli stamina zosta³a u¿yta, false jeœli stamina wyczerpana.
    /// </summary>
    public bool UseStamina(float amount)
    {
        if (amount <= 0f || staminaExhausted) return false;
        if (currentStamina > 0f)
        {
            // Odejmujemy staminê wzglêdem aktualnego limitu (max + bonus jeœli aktywny)
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
                currentStamina = Mathf.Clamp(currentStamina + staminaRegenPerSecond * Time.deltaTime, 0f, TotalMaxStamina);
                // staminaExhausted resetowane w PlayerMovement po puszczeniu sprintu
            }
        }
    }

    /// <summary>
    /// U¿yj Energy Drink: daje bonus do max staminy na czas trwania efektu.
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
        // Jeœli stamina jest poni¿ej nowego limitu, nie zmieniamy jej;
        // Jeœli jest równa maxStamina, podbijamy do maxStamina+bonus
        if (currentStamina < maxStamina)
            currentStamina = Mathf.Min(currentStamina + bonus, TotalMaxStamina);
        // W przeciwnym razie staminaBaseBar i staminaBonusBar poka¿¹ odpowiednie wartoœci

        yield return new WaitForSeconds(duration);

        staminaBonus = 0f;
        // Przytnij currentStamina do maxStamina, jeœli trzeba
        currentStamina = Mathf.Min(currentStamina, maxStamina);
        staminaBonusCoroutine = null;
    }

    private void Die()
    {
        Debug.Log("Gracz zgin¹³!");
    }
}