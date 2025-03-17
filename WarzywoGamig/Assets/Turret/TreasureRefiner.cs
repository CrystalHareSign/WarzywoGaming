using TMPro;
using UnityEngine;

public class TreasureRefiner : MonoBehaviour
{
    public Inventory inventory; // Odniesienie do ekwipunku gracza
    public InventoryUI inventoryUI;

    private bool isRefining = false; // Flaga, która zapewnia, ¿e metoda jest wywo³ywana tylko raz

    // Publiczne referencje do TextMeshPro dla kategorii i iloœci
    public TextMeshProUGUI[] categoryTexts; // Tablica dla tekstów kategorii
    public TextMeshProUGUI[] countTexts;    // Tablica dla tekstów iloœci

    void Start()
    {
        InitializeSlots();
    }

    // Funkcja do usuwania przedmiotu z ekwipunku i aktualizowania slotów
    public void RemoveOldestItemFromInventory(string itemName)
    {
        if (isRefining)
        {
            return; // Jeœli metoda ju¿ zosta³a wywo³ana, nie rób nic
        }

        isRefining = true; // Ustawienie flagi na true, aby metoda dzia³a³a tylko raz

        // Szukamy przedmiotu w ekwipunku
        GameObject itemToRemove = null;
        int itemIndex = -1;

        for (int i = 0; i < inventory.items.Count; i++)
        {
            if (inventory.items[i].name == itemName)
            {
                itemToRemove = inventory.items[i];
                itemIndex = i;
                break;
            }
        }

        if (itemToRemove != null)
        {
            // Pobieramy dane przedmiotu z InventoryUI
            InteractableItem interactableItem = itemToRemove.GetComponent<InteractableItem>();
            if (interactableItem != null)
            {
                // Zaktualizowanie slotów w TreasureRefiner
                UpdateTreasureRefinerSlots(interactableItem);

                // Usuwamy przedmiot z ekwipunku
                inventory.items.RemoveAt(itemIndex);
                Destroy(itemToRemove); // Zniszczenie obiektu w grze

                Debug.Log($"Usuniêto przedmiot: {itemToRemove.name}");

                // Po usuniêciu przedmiotu zaktualizuj UI ekwipunku
                if (inventoryUI != null)
                {
                    inventoryUI.UpdateInventoryUI(inventory.weapons, inventory.items);
                }
            }
        }
        else
        {
            Debug.Log("Nie znaleziono przedmiotu o nazwie '" + itemName + "' w ekwipunku.");
        }

        // Po zakoñczeniu operacji ustawiamy flagê z powrotem na false
        isRefining = false;
    }

    // Funkcja aktualizuj¹ca sloty w TreasureRefiner
    private void UpdateTreasureRefinerSlots(InteractableItem item)
    {
        // Zak³adaj¹c, ¿e `item` zawiera dane o kategorii oraz iloœci
        TreasureResources treasureResources = item.GetComponent<TreasureResources>();

        if (treasureResources != null)
        {
            // Przejœcie przez sloty w TreasureRefiner
            for (int i = 0; i < categoryTexts.Length; i++)
            {
                // Sprawdzamy, czy slot jest ju¿ zajêty
                if (categoryTexts[i].text == "-" && countTexts[i].text == "0")
                {
                    // Slot jest pusty, wiêc wstawiamy dane
                    categoryTexts[i].text = treasureResources.resourceCategories[0].name;
                    countTexts[i].text = treasureResources.resourceCategories[0].resourceCount.ToString();
                    break; // Zakoñczono dodawanie zasobów
                }
            }

            // Jeœli wszystkie sloty s¹ ju¿ zajête, mo¿emy tutaj dodaæ logikê, np. pokazanie komunikatu o braku miejsca.
        }
    }

    // Funkcja inicjalizuj¹ca domyœlne wartoœci w slotach (np. "Brak" i 0)
    public void InitializeSlots()
    {
        for (int i = 0; i < categoryTexts.Length; i++)
        {
            categoryTexts[i].text = "-"; // Domyœlna kategoria
            countTexts[i].text = "0"; // Domyœlna iloœæ
        }
    }
}
