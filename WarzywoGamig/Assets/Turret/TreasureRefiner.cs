using UnityEngine;

public class TreasureRefiner : MonoBehaviour
{
    public Inventory playerInventory; // Odniesienie do ekwipunku gracza
    public InventoryUI inventoryUI;

    // Funkcja do usuwania najstarszego przedmiotu z ekwipunku
    public void RemoveOldestItemFromInventory()
    {
        if (playerInventory == null)
        {
            Debug.LogError("Player Inventory is not assigned!");
            return;
        }

        // Sprawdzamy, czy s� jakie� przedmioty w ekwipunku
        if (playerInventory.items.Count > 0)
        {
            // Usuwamy najstarszy przedmiot (pierwszy w li�cie)
            GameObject oldestItem = playerInventory.items[0];

            // Debugowanie: Sprawd�my, co jest w li�cie
            Debug.Log("Najstarszy przedmiot do usuni�cia: " + oldestItem.name);

            // Usuwamy go z listy
            playerInventory.items.RemoveAt(0); // Usuwamy go z listy

            // Dezaktywujemy obiekt w grze (aby znikn�� z widoku gracza)
            oldestItem.SetActive(false);

            // Mo�esz doda� logik�, aby obiekt zosta� usuni�ty z poziomu �wiata (je�li to wymagane)
            Destroy(oldestItem); // Usu� obiekt z gry

            // Aktualizujemy UI po usuni�ciu przedmiotu
            inventoryUI.UpdateInventoryUI(playerInventory.weapons, playerInventory.items);
        }
        else
        {
            Debug.Log("Brak przedmiot�w do usuni�cia.");
        }
    }
}
