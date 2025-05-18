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

    public static InventoryUI Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            //Debug.Log("InventoryUI initialized.");
        }
        else
        {
            //Debug.LogWarning("Another instance of InventoryUI found. Destroying this instance.");
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        //Debug.Log("Inventory UI Start");

        // Sprawdź, czy broń jest przypisana po załadowaniu sceny
        if (currentWeapon != null)
        {
            //Debug.Log("Current weapon exists: " + currentWeapon.name);
            UpdateWeaponUI(currentWeapon); // Zaktualizuj UI broni
        }
        else
        {
            //Debug.LogWarning("No current weapon assigned after scene change.");
        }

        // Inicjalizacja UI
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

    public void UpdateInventoryUI(List<GameObject> weapons, List<GameObject> items)
    {
        //Debug.Log("Weapons count: " + weapons.Count);

        bool weaponEquipped = false;

        // Sprawdzamy, czy gracz trzyma broń
        foreach (var weaponObj in weapons)
        {
            if (weaponObj == null) continue;

            // Sprawdź, czy 'weaponObj' ma komponent InteractableItem
            InteractableItem weapon = weaponObj.GetComponent<InteractableItem>();

            if (weapon != null)
            {
                Debug.Log("Weapon found: " + weapon.itemName); // Logujemy nazwę broni

                weaponImage.sprite = weaponIcons.ContainsKey(weapon.itemName) ? weaponIcons[weapon.itemName] : defaultWeaponSprite;
                weaponImage.enabled = true;
                weaponEquipped = true;

                // Aktualizacja nazwy broni
                weaponNameText.text = weapon.itemName;
                weaponNameText.gameObject.SetActive(true);

                // Sprawdzamy, czy broń ma komponent Gun
                Gun gun = weaponObj.GetComponent<Gun>();
                if (gun != null)
                {
                    Debug.Log("Updating weapon UI for: " + gun.name); // Logujemy nazwę broni
                    currentWeapon = gun;
                    UpdateWeaponUI(gun);
                }
                else
                {
                    Debug.LogWarning("Gun component not found on: " + weaponObj.name);
                }
            }
            else
            {
                Debug.LogWarning("InteractableItem component not found on: " + weaponObj.name);
            }
        }

        if (!weaponEquipped)
        {
            //Debug.Log("No weapon equipped, hiding weapon UI.");
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
        // Ukrywamy wszystkie ikony, teksty ilości oraz kategorie przedmiotów
        for (int i = 0; i < itemImages.Length; i++)
        {
            itemImages[i].enabled = false;
            itemTexts[i].gameObject.SetActive(false);
            itemCategoryTexts[i].gameObject.SetActive(false);  // Ukryj tekst kategorii
        }

        // Aktualizujemy ikony, teksty ilości oraz kategorie dla podniesionych przedmiotów
        for (int i = 0; i < items.Count && i < 4; i++)
        {
            if (items[i] == null) continue; // Pomija usunięte obiekty

            InteractableItem item = items[i].GetComponent<InteractableItem>();
            TreasureResources treasureResources = items[i].GetComponent<TreasureResources>();

            if (item != null && treasureResources != null)
            {
                // Wyświetlanie ikony przedmiotu
                itemImages[i].sprite = itemIcons.ContainsKey(item.itemName) ? itemIcons[item.itemName] : defaultItemSprite;
                itemImages[i].enabled = true;

                // Wyświetlanie ilości zasobów
                itemTexts[i].text = treasureResources.resourceCategories[0].resourceCount.ToString();
                itemTexts[i].gameObject.SetActive(true);

                // Wyświetlanie kategorii zasobów
                string categoryName = treasureResources.resourceCategories[0].name;
                itemCategoryTexts[i].text = categoryName; // Użycie name z ResourceCategory
                itemCategoryTexts[i].gameObject.SetActive(true);
            }
        }
    }

    public void UpdateWeaponUI(Gun gun)
    {
        if (gun == null)
        {
            //Debug.LogWarning("Gun is null, skipping UI update.");
            return;
        }

        //Debug.Log("Updating Weapon UI for: " + gun.name); // Logujemy nazwę broni

        // Pokazanie UI amunicji
        ammoText.gameObject.SetActive(true);
        totalAmmoText.gameObject.SetActive(true);
        slashText.gameObject.SetActive(true);
        reloadingText.gameObject.SetActive(gun.IsReloading());

        // Aktualizacja wartości amunicji
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