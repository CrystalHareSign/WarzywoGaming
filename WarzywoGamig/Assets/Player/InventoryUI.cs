using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Experimental.GraphView;

public class InventoryUI : MonoBehaviour
{
    public Image weaponImage; // Ikona broni
    public Image weaponBackgroundImage; // Tło pod UI broni
    public Image[] itemImages = new Image[6]; // Tablica obrazków dla przedmiotów
    public Image[] slotBackgrounds = new Image[6]; // Tła dla slotów (np. szare, czerwone, itp.)
    public TextMeshProUGUI[] itemTexts = new TextMeshProUGUI[6]; // Tablica tekstów dla ilości przedmiotów
    public TextMeshProUGUI[] itemCategoryTexts = new TextMeshProUGUI[6]; // Tablica tekstów dla kategorii zasobów
    public TextMeshProUGUI[] slotNumberTexts = new TextMeshProUGUI[6];

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
    private int lastWeaponCount = 0;
    public int selectedSlotIndex = 0;
    public Color normalItemColor = Color.yellow;
    public Color selectedItemColor = Color.white;

    public bool isInputBlocked = false;

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
            HideWeaponUI(false); // Ukryj UI broni, jeśli nie masz broni (nie pokazuj tła)
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
        weaponBackgroundImage.gameObject.SetActive(false);
    }

    public void Update()
    {
        if (isInputBlocked)
            return;

        var inventory = Inventory.Instance;
        int weaponSlots = Mathf.Min(inventory.weapons.Count, 3);
        int itemSlots = Mathf.Min(inventory.items.Count, 6);
        int totalSlots = weaponSlots + itemSlots;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (totalSlots > 0)
        {
            if (scroll > 0f)
                SelectPreviousSlot(inventory, totalSlots);
            else if (scroll < 0f)
                SelectNextSlot(inventory, totalSlots);
        }

        // Klawisze 1-3: wybierz broń TYLKO jeśli istnieje
        if (weaponSlots >= 1 && Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(inventory, 0);
        if (weaponSlots >= 2 && Input.GetKeyDown(KeyCode.Alpha2)) SelectSlot(inventory, 1);
        if (weaponSlots >= 3 && Input.GetKeyDown(KeyCode.Alpha3)) SelectSlot(inventory, 2);

        // Klawisze 4-9: wybierz item TYLKO jeśli istnieje taki indeks!
        if (itemSlots >= 1 && Input.GetKeyDown(KeyCode.Alpha4)) SelectSlot(inventory, weaponSlots + 0);
        if (itemSlots >= 2 && Input.GetKeyDown(KeyCode.Alpha5)) SelectSlot(inventory, weaponSlots + 1);
        if (itemSlots >= 3 && Input.GetKeyDown(KeyCode.Alpha6)) SelectSlot(inventory, weaponSlots + 2);
        if (itemSlots >= 4 && Input.GetKeyDown(KeyCode.Alpha7)) SelectSlot(inventory, weaponSlots + 3);
        if (itemSlots >= 5 && Input.GetKeyDown(KeyCode.Alpha8)) SelectSlot(inventory, weaponSlots + 4);
        if (itemSlots >= 6 && Input.GetKeyDown(KeyCode.Alpha9)) SelectSlot(inventory, weaponSlots + 5);
    }


    public void SelectNextSlot(Inventory inventory, int totalSlots)
    {
        if (totalSlots == 0) return;
        selectedSlotIndex = (selectedSlotIndex + 1) % totalSlots;
        SelectSlot(inventory, selectedSlotIndex);
    }

    public void SelectPreviousSlot(Inventory inventory, int totalSlots)
    {
        if (totalSlots == 0) return;
        selectedSlotIndex = (selectedSlotIndex - 1 + totalSlots) % totalSlots;
        SelectSlot(inventory, selectedSlotIndex);
    }

    public void SelectSlot(Inventory inventory, int slotIndex)
    {
        int weaponSlots = Mathf.Min(inventory.weapons.Count, 3);
        int itemSlots = Mathf.Min(inventory.items.Count, 6);
        int totalSlots = weaponSlots + itemSlots;

        // Clamp to valid range
        slotIndex = Mathf.Clamp(slotIndex, 0, totalSlots - 1);
        selectedSlotIndex = slotIndex;

        if (slotIndex < weaponSlots)
        {
            // Wybierz broń
            inventory.EquipWeapon(inventory.weapons[slotIndex]);
        }
        else if (slotIndex - weaponSlots < itemSlots)
        {
            // Wybierz item
            int itemIdx = slotIndex - weaponSlots;
            UpdateItemUI(inventory.items); // podświetlanie
                                           // (dalsza obsługa aktywnego itemu, np. użycie, jeśli chcesz)
        }
        UpdateInventoryUI(inventory.weapons, inventory.items, inventory.currentWeaponName);
    }

    public void UpdateInventoryUI(List<string> weapons, List<GameObject> items, string currentWeaponName)
    {
        int oldWeaponCount = lastWeaponCount;
        int weaponCount = weapons.Count;
        int itemCount = items.Count;
        int totalSlots = weaponCount + itemCount;

        lastWeaponCount = weaponCount;

        if (weaponCount != oldWeaponCount && selectedSlotIndex >= oldWeaponCount)
        {
            selectedSlotIndex += (weaponCount - oldWeaponCount);
            selectedSlotIndex = Mathf.Clamp(selectedSlotIndex, 0, totalSlots - 1);
        }

        if (selectedSlotIndex >= totalSlots)
            selectedSlotIndex = Mathf.Max(0, totalSlots - 1);
        if (selectedSlotIndex < 0 && totalSlots > 0)
            selectedSlotIndex = 0;
        if (totalSlots == 1)
            selectedSlotIndex = 0;

        Gun gun = null;
        GameObject currentWeaponPrefab = null;
        if (Inventory.Instance != null)
            currentWeaponPrefab = Inventory.Instance.currentWeaponPrefab;

        if (!string.IsNullOrEmpty(currentWeaponName))
        {
            weaponImage.sprite = weaponIcons.ContainsKey(currentWeaponName)
                ? weaponIcons[currentWeaponName]
                : defaultWeaponSprite;
            weaponImage.enabled = true;

            weaponNameText.text = currentWeaponName;
            weaponNameText.gameObject.SetActive(true);

            if (currentWeaponPrefab != null)
            {
                gun = currentWeaponPrefab.GetComponent<Gun>();
                if (gun != null)
                    UpdateWeaponUI(gun);
                else
                    HideWeaponUI(false);
            }
            else
            {
                WeaponPrefabEntry found = weaponDatabase.weaponPrefabsList.Find(w => w.weaponName == currentWeaponName);
                if (found != null && found.weaponPrefab != null)
                {
                    gun = found.weaponPrefab.GetComponent<Gun>();
                    if (gun != null)
                        UpdateWeaponUI(gun);
                    else
                        HideWeaponUI(false);
                }
                else
                    HideWeaponUI(false);
            }
            ShowWeaponUI();
        }
        else
        {
            // SHOW BG IF ANY ITEMS (loot)
            bool hasLoot = (itemCount > 0);
            HideWeaponUI(hasLoot);
        }

        UpdateItemUI(items);
    }

    private void UpdateItemUI(List<GameObject> items)
    {
        if (items == null)
        {
            Debug.LogWarning("UpdateItemUI called with null items list!");
            return;
        }

        int weaponCount = Inventory.Instance.weapons.Count;

        for (int i = 0; i < itemImages.Length; i++)
        {
            if (itemImages[i] != null)
                itemImages[i].enabled = false;
            if (itemTexts[i] != null)
                itemTexts[i].gameObject.SetActive(false);
            if (itemCategoryTexts[i] != null)
                itemCategoryTexts[i].gameObject.SetActive(false);
            if (slotBackgrounds != null && i < slotBackgrounds.Length && slotBackgrounds[i] != null)
                slotBackgrounds[i].enabled = false;
            if (slotNumberTexts != null && i < slotNumberTexts.Length && slotNumberTexts[i] != null)
                slotNumberTexts[i].gameObject.SetActive(false);
        }

        int maxSlots = Mathf.Min(items.Count, itemImages.Length, itemTexts.Length, itemCategoryTexts.Length, slotBackgrounds != null ? slotBackgrounds.Length : int.MaxValue);

        for (int i = 0; i < maxSlots; i++)
        {
            if (items[i] == null)
            {
                Debug.LogWarning($"items[{i}] is null!");
                if (slotNumberTexts != null && i < slotNumberTexts.Length && slotNumberTexts[i] != null)
                    slotNumberTexts[i].gameObject.SetActive(false);
                continue;
            }

            var item = items[i].GetComponent<InteractableItem>();
            var treasureResources = items[i].GetComponent<TreasureResources>();
            if (item == null || treasureResources == null || treasureResources.resourceCategories == null || treasureResources.resourceCategories.Count == 0)
            {
                Debug.LogWarning($"Item or TreasureResources missing on items[{i}]");
                if (slotNumberTexts != null && i < slotNumberTexts.Length && slotNumberTexts[i] != null)
                    slotNumberTexts[i].gameObject.SetActive(false);
                continue;
            }

            if (slotBackgrounds != null && i < slotBackgrounds.Length && slotBackgrounds[i] != null)
                slotBackgrounds[i].enabled = true;

            if (itemImages[i] != null)
            {
                itemImages[i].sprite = itemIcons.ContainsKey(item.itemName) ? itemIcons[item.itemName] : defaultItemSprite;
                itemImages[i].enabled = true;
                if (selectedSlotIndex >= weaponCount && (i == selectedSlotIndex - weaponCount))
                    itemImages[i].color = selectedItemColor;
                else
                    itemImages[i].color = normalItemColor;
            }

            if (itemTexts[i] != null)
            {
                itemTexts[i].text = treasureResources.resourceCategories[0].resourceCount.ToString();
                itemTexts[i].gameObject.SetActive(true);
            }

            if (itemCategoryTexts[i] != null)
            {
                string categoryName = treasureResources.resourceCategories[0].name;
                itemCategoryTexts[i].text = categoryName;
                itemCategoryTexts[i].gameObject.SetActive(true);
            }

            if (slotNumberTexts != null && i < slotNumberTexts.Length && slotNumberTexts[i] != null)
            {
                slotNumberTexts[i].text = (i + 4).ToString();
                slotNumberTexts[i].gameObject.SetActive(true);
            }
        }
    }

    public void UpdateWeaponUI(Gun gun)
    {
        if (gun == null)
        {
            return;
        }

        var interactable = gun.GetComponent<InteractableItem>();
        weaponNameText.text = interactable != null ? interactable.itemName : "";

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
            weaponBackgroundImage.gameObject.SetActive(false);
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

    // Poprawiona HideWeaponUI:
    public void HideWeaponUI(bool showBackgroundIfLoot = false)
    {
        weaponNameText.gameObject.SetActive(false);
        ammoText.gameObject.SetActive(false);
        totalAmmoText.gameObject.SetActive(false);
        slashText.gameObject.SetActive(false);
        weaponImage.gameObject.SetActive(false);
        weaponBackgroundImage.gameObject.SetActive(showBackgroundIfLoot);
    }
    public void ShowWeaponUI()
    {
        weaponNameText.gameObject.SetActive(true);
        ammoText.gameObject.SetActive(true);
        totalAmmoText.gameObject.SetActive(true);
        slashText.gameObject.SetActive(true);
        weaponImage.gameObject.SetActive(true);
        weaponBackgroundImage.gameObject.SetActive(true);
    }

    public void HideItemUI()
    {
        for (int i = 0; i < itemImages.Length; i++)
        {
            if (itemImages[i] != null)
                itemImages[i].enabled = false;
            if (itemTexts[i] != null)
                itemTexts[i].gameObject.SetActive(false);
            if (itemCategoryTexts[i] != null)
                itemCategoryTexts[i].gameObject.SetActive(false);
            if (slotBackgrounds != null && i < slotBackgrounds.Length && slotBackgrounds[i] != null)
                slotBackgrounds[i].enabled = false;
            if (slotNumberTexts != null && i < slotNumberTexts.Length && slotNumberTexts[i] != null)
                slotNumberTexts[i].gameObject.SetActive(false);
        }
    }

    public void ShowItemUI(List<GameObject> items)
    {
        int maxSlots = Mathf.Min(
            items.Count,
            itemImages.Length,
            itemTexts.Length,
            itemCategoryTexts.Length,
            slotBackgrounds != null ? slotBackgrounds.Length : int.MaxValue,
            slotNumberTexts != null ? slotNumberTexts.Length : int.MaxValue
        );

        for (int i = 0; i < maxSlots; i++)
        {
            if (items[i] == null)
            {
                if (slotNumberTexts != null && i < slotNumberTexts.Length && slotNumberTexts[i] != null)
                    slotNumberTexts[i].gameObject.SetActive(false);
                continue;
            }

            InteractableItem item = items[i].GetComponent<InteractableItem>();
            TreasureResources treasureResources = items[i].GetComponent<TreasureResources>();

            if (item != null && treasureResources != null && treasureResources.resourceCategories != null && treasureResources.resourceCategories.Count > 0)
            {
                if (itemImages[i] != null)
                    itemImages[i].enabled = true;
                if (itemTexts[i] != null)
                    itemTexts[i].gameObject.SetActive(true);
                if (itemCategoryTexts[i] != null)
                    itemCategoryTexts[i].gameObject.SetActive(true);
                if (slotBackgrounds != null && i < slotBackgrounds.Length && slotBackgrounds[i] != null)
                    slotBackgrounds[i].enabled = true;
                if (slotNumberTexts != null && i < slotNumberTexts.Length && slotNumberTexts[i] != null)
                {
                    slotNumberTexts[i].text = (i + 4).ToString();
                    slotNumberTexts[i].gameObject.SetActive(true);
                }
            }
            else
            {
                if (slotNumberTexts != null && i < slotNumberTexts.Length && slotNumberTexts[i] != null)
                    slotNumberTexts[i].gameObject.SetActive(false);
            }
        }
    }
}