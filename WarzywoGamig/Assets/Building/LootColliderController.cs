using UnityEngine;

public class LootColliderController : MonoBehaviour
{
    private Collider lootCollider;
    private GameObject player;
    private bool playerExited = false; // Flaga wykrywaj�ca wyj�cie gracza

    public void Initialize(Collider collider)
    {
        lootCollider = collider;
    }

    private void Start()
    {
        // Znalezienie gracza
        player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError(" Nie znaleziono gracza! Sprawd�, czy ma tag 'Player'.");
            return;
        }

        Debug.Log(" LootColliderController aktywowany!");
    }

    private void Update()
    {
        // Sprawdzenie, czy lootCollider i player s� poprawnie przypisane
        if (lootCollider == null || player == null)
        {
            Debug.Log("B��d: lootCollider lub player jest null w FixedUpdate!");
            return;
        }

        // Pobranie komponentu Collider tylko raz na pocz�tku, zamiast za ka�dym razem w FixedUpdate
        Collider playerCollider = player.GetComponent<Collider>();
        if (playerCollider == null)
        {
            Debug.LogError("B��d: Player nie posiada komponentu Collider!");
            return;
        }

        // Je�li gracz jeszcze nie wyszed�, sprawdzamy, czy ju� nie jest w colliderze
        if (!playerExited && !lootCollider.bounds.Intersects(playerCollider.bounds))
        {
            Debug.Log("Gracz opu�ci� stref� lootu (wykryto w FixedUpdate)!");
            ActivateCollider();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player)
        {
            Debug.Log(" Gracz wyszed� z colliderea lootu!");
            playerExited = true; // Ustawiamy flag�
            ActivateCollider();
        }
    }

    private void ActivateCollider()
    {
        lootCollider.isTrigger = false;
        Debug.Log(" Collider lootu aktywowany!");
        Destroy(this); // Usuwamy skrypt po aktywacji
    }
}
