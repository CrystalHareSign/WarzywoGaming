using UnityEngine;

public class LootColliderController : MonoBehaviour
{
    private Collider lootCollider;
    private Collider playerCollider;
    private bool playerExited = false;

    public void Initialize(Collider lootCol)
    {
        lootCollider = lootCol;
        // playerCollider zostanie znaleziony w Start()
    }

    private void Start()
    {
        if (playerCollider == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null)
            {
                Debug.LogError("Nie znaleziono gracza! Sprawdü, czy ma tag 'Player'.");
                return;
            }
            playerCollider = playerObj.GetComponent<Collider>();
            if (playerCollider == null)
            {
                Debug.LogError("Player nie posiada komponentu Collider!");
                return;
            }
        }
    }

    private void Update()
    {
        if (lootCollider == null || playerCollider == null)
            return;

        if (!playerExited && !lootCollider.bounds.Intersects(playerCollider.bounds))
        {
            ActivateCollider();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == playerCollider)
        {
            playerExited = true;
            ActivateCollider();
        }
    }

    private void ActivateCollider()
    {
        if (lootCollider == null || playerCollider == null)
            return;

        lootCollider.isTrigger = false;
        Physics.IgnoreCollision(lootCollider, playerCollider, false);
        Destroy(this);
    }
}