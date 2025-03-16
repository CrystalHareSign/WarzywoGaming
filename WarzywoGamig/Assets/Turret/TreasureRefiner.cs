using UnityEngine;

public class TreasureRefiner : MonoBehaviour
{
    public Inventory inventory; // Odniesienie do ekwipunku gracza
    public InventoryUI inventoryUI;

    private bool isRefining = false; // Flaga, która zapewnia, ¿e metoda jest wywo³ywana tylko raz

    // Funkcja do usuwania przedmiotu o nazwie "item_1" z ekwipunku
    public void RemoveOldestItemFromInventory(string itemName)
    {
        if (isRefining)
        {
            return; // Jeœli metoda ju¿ zosta³a wywo³ana, nie rób nic
        }

        isRefining = true; // Ustawienie flagi na true, aby metoda dzia³a³a tylko raz

        // Szukamy przedmiotu o nazwie "item_1" w ekwipunku
        GameObject itemToRemove = null;

        foreach (GameObject item in inventory.items)
        {
            if (item.name == itemName)
            {
                itemToRemove = item;
                break;
            }
        }

        if (itemToRemove != null)
        {
            // Usuwamy przedmiot o nazwie "item_1" z listy
            inventory.items.Remove(itemToRemove);

            // Usuwamy obiekt z gry (jeœli chcesz go zniszczyæ)
            Destroy(itemToRemove);

            Debug.Log($"Usuniêto przedmiot: {itemToRemove.name}");

            // Po usuniêciu przedmiotu zaktualizuj UI ekwipunku
            if (inventoryUI != null)
            {
                inventoryUI.UpdateInventoryUI(inventory.weapons, inventory.items);
            }
        }
        else
        {
            Debug.Log("Nie znaleziono przedmiotu o nazwie 'item_1' w ekwipunku.");
        }

        // Po zakoñczeniu operacji ustawiamy flagê z powrotem na false, aby metoda mog³a byæ wywo³ana ponownie
        isRefining = false;
    }
}