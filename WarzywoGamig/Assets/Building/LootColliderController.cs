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

    private void FixedUpdate()
    {
        // Je�li gracz jeszcze nie wyszed�, sprawdzamy, czy ju� nie jest w colliderze
        if (!playerExited && !lootCollider.bounds.Intersects(player.GetComponent<Collider>().bounds))
        {
            Debug.Log(" Gracz opu�ci� stref� lootu (wykryto w FixedUpdate)!");
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
