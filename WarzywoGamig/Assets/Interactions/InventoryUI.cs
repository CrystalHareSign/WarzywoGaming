using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public Image weaponImage; // Ikona broni
    public Image itemImage; // Ikona innego przedmiotu

    public Sprite defaultWeaponSprite; // Domyœlny obrazek broni
    public Sprite defaultItemSprite; // Domyœlny obrazek przedmiotu

    public Dictionary<string, Sprite> weaponIcons = new Dictionary<string, Sprite>(); // Ikony broni
    public Dictionary<string, Sprite> itemIcons = new Dictionary<string, Sprite>(); // Ikony przedmiotów

    void Start()
    {
        weaponImage.enabled = false;
        itemImage.enabled = false;
    }

    // Nowa metoda do aktualizacji UI
    public void UpdateInventoryUI(List<GameObject> weapons, List<GameObject> items)
    {
        bool weaponEquipped = false;
        bool itemEquipped = false;

        // Sprawdzamy, czy gracz ma jakiekolwiek bronie
        for (int i = 0; i < weapons.Count; i++)
        {
            InteractableItem weapon = weapons[i].GetComponent<InteractableItem>();
            if (weapon != null)
            {
                weaponImage.sprite = weaponIcons.ContainsKey(weapon.itemName) ? weaponIcons[weapon.itemName] : defaultWeaponSprite;
                weaponImage.enabled = true;
                weaponEquipped = true;
            }
        }

        // Sprawdzamy, czy gracz ma jakiekolwiek przedmioty
        for (int i = 0; i < items.Count; i++)
        {
            InteractableItem item = items[i].GetComponent<InteractableItem>();
            if (item != null)
            {
                itemImage.sprite = itemIcons.ContainsKey(item.itemName) ? itemIcons[item.itemName] : defaultItemSprite;
                itemImage.enabled = true;
                itemEquipped = true;
            }
        }

        // Jeœli gracz nie ma broni, ukrywamy ikony broni
        if (!weaponEquipped)
        {
            weaponImage.enabled = false;
        }

        // Jeœli gracz nie ma ¿adnych przedmiotów, ukrywamy ikony przedmiotów
        if (!itemEquipped)
        {
            itemImage.enabled = false;
        }
    }
}
