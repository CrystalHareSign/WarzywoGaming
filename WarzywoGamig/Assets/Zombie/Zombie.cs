using UnityEngine;
using UnityEngine.UI;

public class Zombie : MonoBehaviour
{
    public int maxHealth = 100;  // Maksymalne zdrowie
    public int currentHealth;  // Aktualne zdrowie
    public Text healthText;  // Odwo�anie do UI tekstu (przypisane w inspektorze)
    public Transform head;  // Referencja do g�owy Zombie

    private void Start()
    {
        currentHealth = maxHealth;  // Na pocz�tku Zombie ma pe�ne zdrowie
        UpdateHealthText();  // Zaktualizuj wy�wietlany tekst
    }

    private void Update()
    {
        // Upewnij si�, �e tekst jest zawsze skierowany w stron� kamery
        Vector3 lookAtPosition = Camera.main.transform.position;  // Pozycja kamery
        lookAtPosition.y = healthText.transform.position.y;  // Ustawiamy y na tej samej wysoko�ci co tekst (�eby nie patrzy� do g�ry/d�)

        // Ustawiamy tekst nad g�ow� Zombie (zmieniamy pozycj� tekstu)
        healthText.transform.position = head.position + new Vector3(0f, 2f, 0f); // Przesuwamy go nad g�ow�

        // Upewnij si�, �e tekst patrzy w stron� kamery
        healthText.transform.LookAt(lookAtPosition);  // Obr�� tekst, aby patrzy� w stron� kamery

        // Je�li tekst jest odwr�cony, wykonaj dodatkow� rotacj�:
        healthText.transform.Rotate(0f, 180f, 0f);  // Obracamy o 180 stopni wok� osi Y
    }

    // Funkcja do zadawania obra�e�
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;  // Odejmuje zadane obra�enia

        if (currentHealth <= 0)
        {
            Die();  // Zombie umiera, je�li zdrowie spadnie do zera
        }

        UpdateHealthText();  // Zaktualizuj wy�wietlany tekst
    }

    // Funkcja do aktualizacji tekstu zdrowia
    private void UpdateHealthText()
    {
        healthText.text = "HP: " + currentHealth.ToString();  // Zaktualizuj tekst, wy�wietlaj�c aktualne zdrowie
    }

    // Funkcja umierania Zombie
    private void Die()
    {
        Debug.Log("Zombie umiera!");
        Destroy(gameObject);  // Zniszczenie obiektu Zombie (mo�esz doda� animacj� umierania)
    }
}
