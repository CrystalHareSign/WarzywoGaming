using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealth : MonoBehaviour
{
    public int baseHealth = 100;  // Podstawowe zdrowie
    private int currentHealth;  // Aktualne zdrowie
    public TextMeshProUGUI healthText;  // UI tekstu HP (przypisane w inspektorze)
    public Transform head;  // Referencja do głowy Enemy
    private Transform mainCamera; // Referencja do kamery

    private void Start()
    {
        mainCamera = Camera.main.transform; // Pobranie głównej kamery
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

    // Funkcja do zadawania obrażeń
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // AGRO NA GRACZA PO OTRZYMANIU OBRAŻEŃ
        var ai = GetComponent<ProceduralMonsterAI>();
        if (ai != null)
            ai.AgroOnPlayer();

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
