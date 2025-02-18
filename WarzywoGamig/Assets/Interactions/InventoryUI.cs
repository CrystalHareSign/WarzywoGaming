using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public Image weaponImage; // Ikona broni
    public Image itemImage; // Ikona innego przedmiotu (tylko wirtualne przechowywanie)

    public Sprite defaultWeaponSprite; // Domy�lny obrazek broni
    public Sprite defaultItemSprite; // Domy�lny obrazek przedmiotu

    public Dictionary<string, Sprite> weaponIcons = new Dictionary<string, Sprite>(); // Ikony broni
    public Dictionary<string, Sprite> itemIcons = new Dictionary<string, Sprite>(); // Ikony przedmiot�w

    public List<GameObject> collectedItems = new List<GameObject>(); // Lista zebranych przedmiot�w (mo�esz doda� do niej obiekty zebrane w Inventory)

    void Start()
    {
        weaponImage.enabled = false;
        itemImage.enabled = false;
    }

    // Nowa metoda do aktualizacji UI
    public void UpdateInventoryUI()
    {
        if (collectedItems.Count > 0)
        {
            // Poka� ikon� ostatniego zebranych przedmiot�w w UI
            itemImage.sprite = itemIcons.ContainsKey(collectedItems[collectedItems.Count - 1].name) ? itemIcons[collectedItems[collectedItems.Count - 1].name] : defaultItemSprite;
            itemImage.enabled = true;
        }
        else
        {
            itemImage.enabled = false;
        }

        // Mo�esz doda� wi�cej logiki do wy�wietlania innych przedmiot�w na UI, jak np. bro�.
    }

    // Funkcja do aktualizacji broni, je�li chcesz obs�ugiwa� bro� w tym samym UI
    public void EquipWeapon(GameObject weapon)
    {
        // Przyk�adowa logika - zaimplementuj zgodnie z potrzebami
        string weaponName = weapon.GetComponent<InteractableItem>().itemName;
        weaponImage.sprite = weaponIcons.ContainsKey(weaponName) ? weaponIcons[weaponName] : defaultWeaponSprite;
        weaponImage.enabled = true;
    }
}
