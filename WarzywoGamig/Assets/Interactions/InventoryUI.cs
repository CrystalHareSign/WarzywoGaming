using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public Image weaponImage; // Ikona broni
    public Image[] itemImages = new Image[4]; // Tablica obrazków dla przedmiotów
    public TextMeshProUGUI[] itemTexts = new TextMeshProUGUI[4]; // Tablica tekstów dla ilości przedmiotów
    public TextMeshProUGUI[] itemCategoryTexts = new TextMeshProUGUI[4]; // Tablica tekstów dla kategorii zasobów

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

    public WeaponDatabase weaponDatabase;

    public static InventoryUI Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        if (currentWeapon != null)
        {
            UpdateWeaponUI(currentWeapon); // Zaktualizuj UI broni
        }

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

    // Zmieniono nazwę parametru z weaponNames na weapons dla spójności
    public void UpdateInventoryUI(List<string> weapons, List<GameObject> items)
    {
        bool weaponEquipped = false;

        foreach (var weaponName in weapons)
        {
            if (string.IsNullOrEmpty(weaponName)) continue;

            // Pobierz prefab/dane broni z WeaponDatabase (przypisz w Inspectorze!)
            WeaponPrefabEntry found = weaponDatabase.weaponPrefabsList.Find(w => w.weaponName == weaponName);

            if (found != null && found.weaponPrefab != null)
            {
                Debug.Log("Weapon found: " + weaponName);

                // Ikona
                weaponImage.sprite = weaponIcons.ContainsKey(weaponName) ? weaponIcons[weaponName] : defaultWeaponSprite;
                weaponImage.enabled = true;
                weaponEquipped = true;

                // Nazwa broni
                weaponNameText.text = weaponName;
                weaponNameText.gameObject.SetActive(true);

                Gun gun = found.weaponPrefab.GetComponent<Gun>();
                if (gun != null)
                {
                    Debug.Log("Updating weapon UI for: " + gun.name);
                    currentWeapon = gun;
                    UpdateWeaponUI(gun);
                }
                else
                {
                    Debug.LogWarning("Gun component not found on prefab for: " + weaponName);
                }
            }
            else
            {
                Debug.LogWarning("WeaponPrefabEntry or prefab not found for: " + weaponName);
            }
        }

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

    private void UpdateItemUI(List<GameObject> items)
    {
        // Ukryj wszystkie ikony, teksty ilości oraz kategorie przedmiotów
        for (int i = 0; i < itemImages.Length; i++)
        {
            itemImages[i].enabled = false;
            itemTexts[i].gameObject.SetActive(false);
            itemCategoryTexts[i].gameObject.SetActive(false);
        }

        // Aktualizuj ikony, teksty ilości oraz kategorie dla podniesionych przedmiotów
        for (int i = 0; i < items.Count && i < 4; i++)
        {
            if (items[i] == null) continue;

            InteractableItem item = items[i].GetComponent<InteractableItem>();
            TreasureResources treasureResources = items[i].GetComponent<TreasureResources>();

            if (item != null && treasureResources != null)
            {
                itemImages[i].sprite = itemIcons.ContainsKey(item.itemName) ? itemIcons[item.itemName] : defaultItemSprite;
                itemImages[i].enabled = true;

                itemTexts[i].text = treasureResources.resourceCategories[0].resourceCount.ToString();
                itemTexts[i].gameObject.SetActive(true);

                string categoryName = treasureResources.resourceCategories[0].name;
                itemCategoryTexts[i].text = categoryName;
                itemCategoryTexts[i].gameObject.SetActive(true);
            }
        }
    }

    public void UpdateWeaponUI(Gun gun)
    {
        if (gun == null)
        {
            return;
        }

        ammoText.gameObject.SetActive(true);
        totalAmmoText.gameObject.SetActive(true);
        slashText.gameObject.SetActive(true);
        reloadingText.gameObject.SetActive(gun.IsReloading());

        ammoText.text = gun.currentAmmo.ToString();
        totalAmmoText.text = gun.unlimitedAmmo ? "∞" : gun.totalAmmo.ToString();
    }

    public void SetWeaponUI(GameObject weaponPrefab)
    {
        if (weaponPrefab == null)
        {
            weaponImage.sprite = defaultWeaponSprite;
            weaponImage.enabled = false;
            weaponNameText.text = "";
            weaponNameText.gameObject.SetActive(false);
            ammoText.gameObject.SetActive(false);
            totalAmmoText.gameObject.SetActive(false);
            slashText.gameObject.SetActive(false);
            reloadingText.gameObject.SetActive(false);
            return;
        }

        InteractableItem weapon = weaponPrefab.GetComponent<InteractableItem>();
        Gun gun = weaponPrefab.GetComponent<Gun>();

        if (weapon != null)
        {
            weaponImage.sprite = weaponIcons.ContainsKey(weapon.itemName) ? weaponIcons[weapon.itemName] : defaultWeaponSprite;
            weaponImage.enabled = true;
            weaponNameText.text = weapon.itemName;
            weaponNameText.gameObject.SetActive(true);
        }

        if (gun != null)
        {
            UpdateWeaponUI(gun);
        }
        else
        {
            ammoText.gameObject.SetActive(false);
            totalAmmoText.gameObject.SetActive(false);
            slashText.gameObject.SetActive(false);
            reloadingText.gameObject.SetActive(false);
        }
    }
}