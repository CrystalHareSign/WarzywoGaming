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

        // Sprawdzamy, czy s¹ jakieœ przedmioty w ekwipunku
        if (playerInventory.items.Count > 0)
        {
            // Usuwamy najstarszy przedmiot (pierwszy w liœcie)
            GameObject oldestItem = playerInventory.items[0];

            // Debugowanie: SprawdŸmy, co jest w liœcie
            Debug.Log("Najstarszy przedmiot do usuniêcia: " + oldestItem.name);

            // Usuwamy go z listy
            playerInventory.items.RemoveAt(0); // Usuwamy go z listy

            // Dezaktywujemy obiekt w grze (aby znikn¹³ z widoku gracza)
            oldestItem.SetActive(false);

            // Mo¿esz dodaæ logikê, aby obiekt zosta³ usuniêty z poziomu œwiata (jeœli to wymagane)
            Destroy(oldestItem); // Usuñ obiekt z gry

            // Aktualizujemy UI po usuniêciu przedmiotu
            inventoryUI.UpdateInventoryUI(playerInventory.weapons, playerInventory.items);
        }
        else
        {
            Debug.Log("Brak przedmiotów do usuniêcia.");
        }
    }
}
