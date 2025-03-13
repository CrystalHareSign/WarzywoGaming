using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public Image weaponImage; // Ikona broni
    public Image[] itemImages = new Image[4]; // Tablica obrazków dla przedmiotów
    public TextMeshProUGUI[] itemTexts = new TextMeshProUGUI[4]; // Tablica tekstów dla ilości przedmiotów

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
        foreach (var img in itemImages)
        {
            img.enabled = false;
        }
        foreach (var txt in itemTexts)
        {
            txt.gameObject.SetActive(false);
        }
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

        // Aktualizacja UI dla przedmiotów
        UpdateItemUI(items);
    }

    // Aktualizacja UI przedmiotów
    private void UpdateItemUI(List<GameObject> items)
    {
        // Ukrywamy wszystkie ikony i teksty przedmiotów
        for (int i = 0; i < itemImages.Length; i++)
        {
            itemImages[i].enabled = false;
            itemTexts[i].gameObject.SetActive(false);
        }

        // Aktualizujemy ikony i teksty dla podniesionych przedmiotów
        for (int i = 0; i < items.Count && i < 4; i++)
        {
            InteractableItem item = items[i].GetComponent<InteractableItem>();
            TreasureResources treasureResources = items[i].GetComponent<TreasureResources>();
            if (item != null && treasureResources != null)
            {
                itemImages[i].sprite = itemIcons.ContainsKey(item.itemName) ? itemIcons[item.itemName] : defaultItemSprite;
                itemImages[i].enabled = true;
                itemTexts[i].text = treasureResources.resourceCategories[0].resourceCount.ToString();
                itemTexts[i].gameObject.SetActive(true);
            }
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