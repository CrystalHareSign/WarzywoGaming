using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Experimental.GraphView;

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

    // --- DODANE: obsługa wyboru aktywnego itemu ---
    public int selectedItemIndex = 0;
    public Color normalItemColor = Color.yellow;
    public Color selectedItemColor = Color.white;

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
        else
        {
            HideWeaponUI(); // Ukryj UI broni, jeśli nie masz broni
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

    void Update()
    {
        var inventory = Inventory.Instance;
        var ui = InventoryUI.Instance;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) // scroll w górę
            ui.SelectPreviousItem(inventory.items); // w górę: poprzedni
        else if (scroll < 0f) // scroll w dół
            ui.SelectNextItem(inventory.items); // w dół: następny
    }

    // --- DODANE: przewijanie wyboru itemu ---
    public void SelectNextItem(List<GameObject> items)
    {
        if (items == null || items.Count == 0)
            return;
        selectedItemIndex = (selectedItemIndex + 1) % Mathf.Min(items.Count, itemImages.Length);
        UpdateItemUI(items);
    }

    public void SelectPreviousItem(List<GameObject> items)
    {
        if (items == null || items.Count == 0)
            return;
        selectedItemIndex = (selectedItemIndex - 1 + Mathf.Min(items.Count, itemImages.Length)) % Mathf.Min(items.Count, itemImages.Length);
        UpdateItemUI(items);
    }

    public void UpdateInventoryUI(List<string> weapons, List<GameObject> items)
    {
        // --- Zabezpieczenie indexu, żeby NIGDY nie wybiegał poza zakres ---
        if (selectedItemIndex >= items.Count)
            selectedItemIndex = Mathf.Max(0, items.Count - 1);
        if (selectedItemIndex < 0 && items.Count > 0)
            selectedItemIndex = 0;
        // --- Jeśli jest tylko jeden item, index zawsze 0 ---
        if (items.Count == 1)
            selectedItemIndex = 0;

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
            HideWeaponUI(); // <--- dodaj to
        }
        else
        {
            ShowWeaponUI();
        }

        // Aktualizacja UI dla przedmiotów
        UpdateItemUI(items);
    }

    private void UpdateItemUI(List<GameObject> items)
    {
        // --- Zabezpieczenie indexu, żeby NIGDY nie wybiegał poza zakres ---
        if (selectedItemIndex >= items.Count)
            selectedItemIndex = Mathf.Max(0, items.Count - 1);
        if (selectedItemIndex < 0 && items.Count > 0)
            selectedItemIndex = 0;
        // --- Jeśli jest tylko jeden item, index zawsze 0 ---
        if (items.Count == 1)
            selectedItemIndex = 0;

        // Ukryj wszystkie ikony, teksty ilości oraz kategorie przedmiotów
        for (int i = 0; i < itemImages.Length; i++)
        {
            itemImages[i].enabled = false;
            itemTexts[i].gameObject.SetActive(false);
            itemCategoryTexts[i].gameObject.SetActive(false);
        }

        int maxSlots = Mathf.Min(items.Count, itemImages.Length, itemTexts.Length, itemCategoryTexts.Length);

        for (int i = 0; i < maxSlots; i++)
        {
            if (items[i] == null)
                continue;

            InteractableItem item = items[i].GetComponent<InteractableItem>();
            TreasureResources treasureResources = items[i].GetComponent<TreasureResources>();

            if (item != null && treasureResources != null && treasureResources.resourceCategories != null && treasureResources.resourceCategories.Count > 0)
            {
                itemImages[i].sprite = itemIcons.ContainsKey(item.itemName) ? itemIcons[item.itemName] : defaultItemSprite;
                itemImages[i].enabled = true;

                itemTexts[i].text = treasureResources.resourceCategories[0].resourceCount.ToString();
                itemTexts[i].gameObject.SetActive(true);

                string categoryName = treasureResources.resourceCategories[0].name;
                itemCategoryTexts[i].text = categoryName;
                itemCategoryTexts[i].gameObject.SetActive(true);

                // PODŚWIETLENIE aktualnie wybranego itemu
                if (i == selectedItemIndex)
                    itemImages[i].color = selectedItemColor;
                else
                    itemImages[i].color = normalItemColor;
            }
            else
            {
                // Ukryj slot jeśli brak danych
                itemImages[i].enabled = false;
                itemTexts[i].gameObject.SetActive(false);
                itemCategoryTexts[i].gameObject.SetActive(false);
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
    public void HideWeaponUI()
    {
        weaponNameText.gameObject.SetActive(false);
        ammoText.gameObject.SetActive(false);
        totalAmmoText.gameObject.SetActive(false);
        slashText.gameObject.SetActive(false);
        weaponImage.gameObject.SetActive(false);
    }
    public void ShowWeaponUI()
    {
        weaponNameText.gameObject.SetActive(true);
        ammoText.gameObject.SetActive(true);
        totalAmmoText.gameObject.SetActive(true);
        slashText.gameObject.SetActive(true);
        weaponImage.gameObject.SetActive(true);
    }
}