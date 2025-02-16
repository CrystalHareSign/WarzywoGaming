using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public int baseHealth = 100;  // Podstawowe zdrowie
    private int currentHealth;  // Aktualne zdrowie
    public Text healthText;  // UI tekstu HP (przypisane w inspektorze)
    public Transform head;  // Referencja do głowy Enemy
    private Transform mainCamera; // Referencja do kamery

    private bool hasHealthBeenIncreased = false; // ✅ Flaga zapobiegająca wielokrotnemu zwiększaniu HP

    private void Start()
    {
        mainCamera = Camera.main.transform; // Pobranie głównej kamery
        ApplyRoundHealthMultiplier(); // Skalowanie HP na podstawie rundy (tylko raz!)
        currentHealth = baseHealth;  // Ustawiamy aktualne zdrowie na nowe bazowe
        UpdateHealthText();  // Aktualizacja wyświetlanego HP
    }

    private void Update()
    {
        if (healthText != null)
        {
            UpdateHealthTextPosition();  // Utrzymanie pozycji i rotacji UI HP
        }
    }

    private void UpdateHealthTextPosition()
    {
        if (mainCamera == null) return;

        // Przesunięcie tekstu nad głowę
        healthText.transform.position = head.position + new Vector3(0f, 2f, 0f);

        // Ustawienie tekstu w stronę kamery
        healthText.transform.LookAt(mainCamera.position);
        healthText.transform.Rotate(0f, 180f, 0f); // Obrót, aby tekst nie był odwrócony
    }

    // ✅ Zwiększanie zdrowia przez RoundManager, ale tylko dla nowych wrogów
    public void IncreaseHealth(float multiplier)
    {
        if (!hasHealthBeenIncreased) // Sprawdzamy, czy już zwiększaliśmy zdrowie
        {
            baseHealth = Mathf.RoundToInt(baseHealth * multiplier);
            currentHealth = baseHealth; // Aktualizujemy bieżące zdrowie
            hasHealthBeenIncreased = true; // ✅ Zaznaczamy, że ten wróg już dostał bonus HP
            UpdateHealthText();
        }
    }

    // ✅ Skalowanie zdrowia tylko dla nowych wrogów (na starcie)
    private void ApplyRoundHealthMultiplier()
    {
        RoundManager roundManager = FindFirstObjectByType<RoundManager>();
        if (roundManager != null)
        {
            float healthMultiplier = Mathf.Pow(roundManager.healthMultiplierPerRound, roundManager.currentRound - 1);
            baseHealth = Mathf.RoundToInt(baseHealth * healthMultiplier);
            currentHealth = baseHealth;
            hasHealthBeenIncreased = true; // ✅ Oznaczamy, że ten wróg dostał już zdrowie na start
        }
    }

    // Funkcja do zadawania obrażeń
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            UpdateHealthText();  // Aktualizacja HP
        }
    }

    // Funkcja do aktualizacji tekstu zdrowia
    private void UpdateHealthText()
    {
        if (healthText != null)
        {
            healthText.text = "HP: " + currentHealth.ToString();
        }
    }

    // Funkcja umierania Enemy
    private void Die()
    {
        Debug.Log(gameObject.name + " umiera!");

        if (healthText != null)
        {
            Destroy(healthText.gameObject);  // Usunięcie UI HP
        }

        Destroy(gameObject);  // Zniszczenie Enemy
    }
}
