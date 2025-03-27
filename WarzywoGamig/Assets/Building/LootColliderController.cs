using UnityEngine;

public class LootColliderController : MonoBehaviour
{
    private Collider lootCollider;
    private GameObject player;
    private bool playerExited = false; // Flaga wykrywaj¹ca wyjœcie gracza

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
            Debug.LogError(" Nie znaleziono gracza! SprawdŸ, czy ma tag 'Player'.");
            return;
        }

        Debug.Log(" LootColliderController aktywowany!");
    }

    private void Update()
    {
        // Sprawdzenie, czy lootCollider i player s¹ poprawnie przypisane
        if (lootCollider == null || player == null)
        {
            Debug.Log("B³¹d: lootCollider lub player jest null w FixedUpdate!");
            return;
        }

        // Pobranie komponentu Collider tylko raz na pocz¹tku, zamiast za ka¿dym razem w FixedUpdate
        Collider playerCollider = player.GetComponent<Collider>();
        if (playerCollider == null)
        {
            Debug.LogError("B³¹d: Player nie posiada komponentu Collider!");
            return;
        }

        // Jeœli gracz jeszcze nie wyszed³, sprawdzamy, czy ju¿ nie jest w colliderze
        if (!playerExited && !lootCollider.bounds.Intersects(playerCollider.bounds))
        {
            Debug.Log("Gracz opuœci³ strefê lootu (wykryto w FixedUpdate)!");
            ActivateCollider();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player)
        {
            Debug.Log(" Gracz wyszed³ z colliderea lootu!");
            playerExited = true; // Ustawiamy flagê
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
