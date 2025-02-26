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
    public TextMeshProUGUI weaponNameText; // Tekst wyświetlający nazwę broni
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI totalAmmoText;
    public TextMeshProUGUI reloadingText;
    public TextMeshProUGUI slashText;

    private Gun currentWeapon; // Aktualnie trzymana broń

    void Start()
    {
        // Domyślnie ukrywamy wszystkie elementy UI
        weaponImage.enabled = false;
        itemImage.enabled = false;
        weaponNameText.gameObject.SetActive(false);

        ammoText.gameObject.SetActive(false);
        totalAmmoText.gameObject.SetActive(false);
        reloadingText.gameObject.SetActive(false);
        slashText.gameObject.SetActive(false);
    }

    // Aktualizacja UI ekwipunku
    public void UpdateInventoryUI(List<GameObject> weapons, List<GameObject> items)
    {
        bool weaponEquipped = false;
        bool itemEquipped = false;

        // Sprawdzamy, czy gracz trzyma broń
        foreach (var weaponObj in weapons)
        {
            InteractableItem weapon = weaponObj.GetComponent<InteractableItem>();
            if (weapon != null)
            {
                weaponImage.sprite = weaponIcons.ContainsKey(weapon.itemName) ? weaponIcons[weapon.itemName] : defaultWeaponSprite;
                weaponImage.enabled = true;
                weaponEquipped = true;

                // Aktualizacja nazwy broni
                weaponNameText.text = weapon.itemName;
                weaponNameText.gameObject.SetActive(true);

                // Aktualizacja UI amunicji
                Gun gun = weaponObj.GetComponent<Gun>();
                if (gun != null)
                {
                    currentWeapon = gun;
                    UpdateWeaponUI(gun);
                }
            }
        }

        // Sprawdzamy, czy gracz trzyma przedmioty
        foreach (var itemObj in items)
        {
            InteractableItem item = itemObj.GetComponent<InteractableItem>();
            if (item != null)
            {
                itemImage.sprite = itemIcons.ContainsKey(item.itemName) ? itemIcons[item.itemName] : defaultItemSprite;
                itemImage.enabled = true;
                itemEquipped = true;
            }
        }

        // Ukrywanie UI, jeśli gracz nie trzyma broni
        if (!weaponEquipped)
        {
            weaponImage.enabled = false;
            weaponNameText.gameObject.SetActive(false);

            ammoText.gameObject.SetActive(false);
            totalAmmoText.gameObject.SetActive(false);
            reloadingText.gameObject.SetActive(false);
            slashText.gameObject.SetActive(false);
        }

        // Ukrywanie UI, jeśli gracz nie trzyma żadnych przedmiotów
        if (!itemEquipped)
        {
            itemImage.enabled = false;
        }
    }

    // Aktualizacja UI broni (amunicja + nazwa)
    public void UpdateWeaponUI(Gun gun)
    {
        if (gun == null) return;

        // Pokazanie UI amunicji
        ammoText.gameObject.SetActive(true);
        totalAmmoText.gameObject.SetActive(true);
        slashText.gameObject.SetActive(true);
        reloadingText.gameObject.SetActive(gun.IsReloading());

        // Aktualizacja wartości amunicji
        ammoText.text = gun.currentAmmo.ToString();
        totalAmmoText.text = gun.unlimitedAmmo ? "∞" : gun.totalAmmo.ToString();
    }
}
