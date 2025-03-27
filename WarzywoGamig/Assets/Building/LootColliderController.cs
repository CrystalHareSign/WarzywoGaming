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

    private void FixedUpdate()
    {
        // Jeœli gracz jeszcze nie wyszed³, sprawdzamy, czy ju¿ nie jest w colliderze
        if (!playerExited && !lootCollider.bounds.Intersects(player.GetComponent<Collider>().bounds))
        {
            Debug.Log(" Gracz opuœci³ strefê lootu (wykryto w FixedUpdate)!");
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
