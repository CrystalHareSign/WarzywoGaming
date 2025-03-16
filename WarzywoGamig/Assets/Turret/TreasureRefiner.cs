using UnityEngine;

public class TreasureRefiner : MonoBehaviour
{
    public Inventory inventory; // Odniesienie do ekwipunku gracza
    public InventoryUI inventoryUI;

    private bool isRefining = false; // Flaga, kt�ra zapewnia, �e metoda jest wywo�ywana tylko raz

    // Funkcja do usuwania przedmiotu o nazwie "item_1" z ekwipunku
    public void RemoveOldestItemFromInventory(string itemName)
    {
        if (isRefining)
        {
            return; // Je�li metoda ju� zosta�a wywo�ana, nie r�b nic
        }

        isRefining = true; // Ustawienie flagi na true, aby metoda dzia�a�a tylko raz

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

            // Usuwamy obiekt z gry (je�li chcesz go zniszczy�)
            Destroy(itemToRemove);

            Debug.Log($"Usuni�to przedmiot: {itemToRemove.name}");

            // Po usuni�ciu przedmiotu zaktualizuj UI ekwipunku
            if (inventoryUI != null)
            {
                inventoryUI.UpdateInventoryUI(inventory.weapons, inventory.items);
            }
        }
        else
        {
            Debug.Log("Nie znaleziono przedmiotu o nazwie 'item_1' w ekwipunku.");
        }

        // Po zako�czeniu operacji ustawiamy flag� z powrotem na false, aby metoda mog�a by� wywo�ana ponownie
        isRefining = false;
    }
}