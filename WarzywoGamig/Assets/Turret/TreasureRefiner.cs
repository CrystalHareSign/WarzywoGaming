using TMPro;
using UnityEngine;

public class TreasureRefiner : MonoBehaviour
{
    public Inventory inventory; // Odniesienie do ekwipunku gracza
    public InventoryUI inventoryUI;

    private bool isRefining = false; // Flaga, kt�ra zapewnia, �e metoda jest wywo�ywana tylko raz

    // Publiczne referencje do TextMeshPro dla kategorii i ilo�ci
    public TextMeshProUGUI[] categoryTexts; // Tablica dla tekst�w kategorii
    public TextMeshProUGUI[] countTexts;    // Tablica dla tekst�w ilo�ci

    void Start()
    {
        InitializeSlots();
    }

    // Funkcja do usuwania przedmiotu z ekwipunku i aktualizowania slot�w
    public void RemoveOldestItemFromInventory(string itemName)
    {
        if (isRefining)
        {
            return; // Je�li metoda ju� zosta�a wywo�ana, nie r�b nic
        }

        isRefining = true; // Ustawienie flagi na true, aby metoda dzia�a�a tylko raz

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
                // Zaktualizowanie slot�w w TreasureRefiner
                UpdateTreasureRefinerSlots(interactableItem);

                // Usuwamy przedmiot z ekwipunku
                inventory.items.RemoveAt(itemIndex);
                Destroy(itemToRemove); // Zniszczenie obiektu w grze

                Debug.Log($"Usuni�to przedmiot: {itemToRemove.name}");

                // Po usuni�ciu przedmiotu zaktualizuj UI ekwipunku
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

        // Po zako�czeniu operacji ustawiamy flag� z powrotem na false
        isRefining = false;
    }

    // Funkcja aktualizuj�ca sloty w TreasureRefiner
    private void UpdateTreasureRefinerSlots(InteractableItem item)
    {
        // Zak�adaj�c, �e `item` zawiera dane o kategorii oraz ilo�ci
        TreasureResources treasureResources = item.GetComponent<TreasureResources>();

        if (treasureResources != null)
        {
            // Przej�cie przez sloty w TreasureRefiner
            for (int i = 0; i < categoryTexts.Length; i++)
            {
                // Sprawdzamy, czy slot jest ju� zaj�ty
                if (categoryTexts[i].text == "-" && countTexts[i].text == "0")
                {
                    // Slot jest pusty, wi�c wstawiamy dane
                    categoryTexts[i].text = treasureResources.resourceCategories[0].name;
                    countTexts[i].text = treasureResources.resourceCategories[0].resourceCount.ToString();
                    break; // Zako�czono dodawanie zasob�w
                }
            }

            // Je�li wszystkie sloty s� ju� zaj�te, mo�emy tutaj doda� logik�, np. pokazanie komunikatu o braku miejsca.
        }
    }

    // Funkcja inicjalizuj�ca domy�lne warto�ci w slotach (np. "Brak" i 0)
    public void InitializeSlots()
    {
        for (int i = 0; i < categoryTexts.Length; i++)
        {
            categoryTexts[i].text = "-"; // Domy�lna kategoria
            countTexts[i].text = "0"; // Domy�lna ilo��
        }
    }
}
