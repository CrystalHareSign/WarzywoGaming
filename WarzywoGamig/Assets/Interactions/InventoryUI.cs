using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public Image weaponImage; // Ikona broni
    public Image itemImage; // Ikona innego przedmiotu

    public Sprite defaultWeaponSprite; // Domyślny obrazek broni
    public Sprite defaultItemSprite; // Domyślny obrazek przedmiotu

    public Dictionary<string, Sprite> weaponIcons = new Dictionary<string, Sprite>(); // Ikony broni
    public Dictionary<string, Sprite> itemIcons = new Dictionary<string, Sprite>(); // Ikony przedmiotów

    // UI dla amunicji
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI totalAmmoText;
    public TextMeshProUGUI reloadingText;
    public TextMeshProUGUI slashText;

    private Gun currentWeapon; // Zmienna do przechowywania aktualnie wybranej broni

    void Start()
    {
        // Domyślnie ukrywamy wszystkie elementy UI
        weaponImage.enabled = false;
        ammoText.gameObject.SetActive(false);
        totalAmmoText.gameObject.SetActive(false);
        reloadingText.gameObject.SetActive(false);
        slashText.gameObject.SetActive(false);
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

                // Przypisanie broni do UI
                Gun gun = weapons[i].GetComponent<Gun>();
                if (gun != null)
                {
                    currentWeapon = gun;
                    UpdateWeaponUI(gun);
                }
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

        // Jeśli gracz nie ma broni, ukrywamy UI broni
        if (!weaponEquipped)
        {
            weaponImage.enabled = false;
            ammoText.gameObject.SetActive(false);
            totalAmmoText.gameObject.SetActive(false);
            reloadingText.gameObject.SetActive(false);
            slashText.gameObject.SetActive(false);
        }

        // Jeśli gracz nie ma żadnych przedmiotów, ukrywamy ikony przedmiotów
        if (!itemEquipped)
        {
            itemImage.enabled = false;
        }
    }

    // Nowa metoda do aktualizacji UI broni
    public void UpdateWeaponUI(Gun gun)
    {
        if (gun == null) return;

        // Aktywujemy UI broni tylko wtedy, gdy broń jest trzymana
        ammoText.gameObject.SetActive(true);
        totalAmmoText.gameObject.SetActive(true);
        slashText.gameObject.SetActive(true);
        reloadingText.gameObject.SetActive(gun.IsReloading());

        ammoText.text = gun.currentAmmo.ToString();
        totalAmmoText.text = gun.unlimitedAmmo ? "∞" : gun.totalAmmo.ToString();
    }
}
