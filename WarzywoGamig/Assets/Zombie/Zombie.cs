using UnityEngine;
using UnityEngine.UI;

public class Zombie : MonoBehaviour
{
    public int maxHealth = 100;  // Maksymalne zdrowie
    public int currentHealth;  // Aktualne zdrowie
    public Text healthText;  // Odwo³anie do UI tekstu (przypisane w inspektorze)
    public Transform head;  // Referencja do g³owy Zombie

    private void Start()
    {
        currentHealth = maxHealth;  // Na pocz¹tku Zombie ma pe³ne zdrowie
        UpdateHealthText();  // Zaktualizuj wyœwietlany tekst
    }

    private void Update()
    {
        // Upewnij siê, ¿e tekst jest zawsze skierowany w stronê kamery
        Vector3 lookAtPosition = Camera.main.transform.position;  // Pozycja kamery
        lookAtPosition.y = healthText.transform.position.y;  // Ustawiamy y na tej samej wysokoœci co tekst (¿eby nie patrzy³ do góry/dó³)

        // Ustawiamy tekst nad g³ow¹ Zombie (zmieniamy pozycjê tekstu)
        healthText.transform.position = head.position + new Vector3(0f, 2f, 0f); // Przesuwamy go nad g³owê

        // Upewnij siê, ¿e tekst patrzy w stronê kamery
        healthText.transform.LookAt(lookAtPosition);  // Obróæ tekst, aby patrzy³ w stronê kamery

        // Jeœli tekst jest odwrócony, wykonaj dodatkow¹ rotacjê:
        healthText.transform.Rotate(0f, 180f, 0f);  // Obracamy o 180 stopni wokó³ osi Y
    }

    // Funkcja do zadawania obra¿eñ
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;  // Odejmuje zadane obra¿enia

        if (currentHealth <= 0)
        {
            Die();  // Zombie umiera, jeœli zdrowie spadnie do zera
        }

        UpdateHealthText();  // Zaktualizuj wyœwietlany tekst
    }

    // Funkcja do aktualizacji tekstu zdrowia
    private void UpdateHealthText()
    {
        healthText.text = "HP: " + currentHealth.ToString();  // Zaktualizuj tekst, wyœwietlaj¹c aktualne zdrowie
    }

    // Funkcja umierania Zombie
    private void Die()
    {
        Debug.Log("Zombie umiera!");
        Destroy(gameObject);  // Zniszczenie obiektu Zombie (mo¿esz dodaæ animacjê umierania)
    }
}
