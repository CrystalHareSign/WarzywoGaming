using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public Image weaponImage; // Ikona broni
    public Image itemImage; // Ikona innego przedmiotu (tylko wirtualne przechowywanie)

    public Sprite defaultWeaponSprite; // Domyœlny obrazek broni
    public Sprite defaultItemSprite; // Domyœlny obrazek przedmiotu

    public Dictionary<string, Sprite> weaponIcons = new Dictionary<string, Sprite>(); // Ikony broni
    public Dictionary<string, Sprite> itemIcons = new Dictionary<string, Sprite>(); // Ikony przedmiotów

    public List<GameObject> collectedItems = new List<GameObject>(); // Lista zebranych przedmiotów (mo¿esz dodaæ do niej obiekty zebrane w Inventory)

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
            // Poka¿ ikonê ostatniego zebranych przedmiotów w UI
            itemImage.sprite = itemIcons.ContainsKey(collectedItems[collectedItems.Count - 1].name) ? itemIcons[collectedItems[collectedItems.Count - 1].name] : defaultItemSprite;
            itemImage.enabled = true;
        }
        else
        {
            itemImage.enabled = false;
        }

        // Mo¿esz dodaæ wiêcej logiki do wyœwietlania innych przedmiotów na UI, jak np. broñ.
    }

    // Funkcja do aktualizacji broni, jeœli chcesz obs³ugiwaæ broñ w tym samym UI
    public void EquipWeapon(GameObject weapon)
    {
        // Przyk³adowa logika - zaimplementuj zgodnie z potrzebami
        string weaponName = weapon.GetComponent<InteractableItem>().itemName;
        weaponImage.sprite = weaponIcons.ContainsKey(weaponName) ? weaponIcons[weaponName] : defaultWeaponSprite;
        weaponImage.enabled = true;
    }
}
