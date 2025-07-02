using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public Image weaponImage;
    public Image weaponBackgroundImage;
    public Image[] itemImages = new Image[5];
    public Image[] slotBackgrounds = new Image[5];
    public TextMeshProUGUI[] itemTexts = new TextMeshProUGUI[5];
    public TextMeshProUGUI[] itemCategoryTexts = new TextMeshProUGUI[5];
    public TextMeshProUGUI[] slotNumberTexts = new TextMeshProUGUI[5];

    public Sprite defaultWeaponSprite;
    public Sprite defaultItemSprite;

    public Dictionary<string, Sprite> weaponIcons = new Dictionary<string, Sprite>();
    public Dictionary<string, Sprite> itemIcons = new Dictionary<string, Sprite>();

    public TextMeshProUGUI weaponNameText;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI totalAmmoText;
    public TextMeshProUGUI reloadingText;
    public TextMeshProUGUI slashText;

    public Color normalItemColor = Color.yellow;
    public Color selectedItemColor = Color.white;

    public bool isInputBlocked = false;

    private Gun currentWeapon;
    public WeaponDatabase weaponDatabase;
    public static InventoryUI Instance;

    private int lastWeaponCount = 0;

    // Now: itemWindowStartIndex to ZAWSZE najniższy indeks widoczny w karuzeli
    public int itemWindowStartIndex = 0;
    private const int itemWindowSize = 5;

    // selectedSlotIndex = indeks slotu UI, na którym jest kursor
    private int _selectedSlotIndex = 2;
    public int selectedSlotIndex
    {
        get { return _selectedSlotIndex; }
        set { _selectedSlotIndex = Mathf.Clamp(value, 0, itemImages.Length - 1); }
    }

    // Nowy: index itemu, na którym "jest kursor" (czyli wybrany item względem całej listy)
    private int selectedItemIndex = 0;

    // UI arrow indicators
    public GameObject leftArrowIndicator;
    public GameObject rightArrowIndicator;

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
            UpdateWeaponUI(currentWeapon);
        else
            HideWeaponUI();

        HideItemUI();

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
        int itemCount = inventory.items.Count;

        if (weaponSlots >= 1 && Input.GetKeyDown(KeyCode.Alpha1)) inventory.EquipWeapon(inventory.weapons[0]);
        if (weaponSlots >= 2 && Input.GetKeyDown(KeyCode.Alpha2)) inventory.EquipWeapon(inventory.weapons[1]);
        if (weaponSlots >= 3 && Input.GetKeyDown(KeyCode.Alpha3)) inventory.EquipWeapon(inventory.weapons[2]);

        // -- Nowa logika karuzeli:
        // selectedItemIndex - indeks itemu "pod kursorem" (względem całej listy)
        // itemWindowStartIndex - indeks pierwszego itemu w oknie karuzeli
        // selectedSlotIndex - pozycja slotu UI, na którym jest kursor (0-4)

        int maxSelectedIndex = Mathf.Max(0, itemCount - 1);

        // Scroll & strzałki: zmiana wybranego itemu (kursora) w zakresie [0, itemCount-1]
        if (itemCount > 0)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0.01f)
            {
                if (selectedItemIndex > 0)
                    selectedItemIndex--;
            }
            else if (scroll < -0.01f)
            {
                if (selectedItemIndex < maxSelectedIndex)
                    selectedItemIndex++;
            }

            // Klawisze szybkiego wyboru
            // 4,5,6,7,8 – wybierają item z danego slotu UI (jeśli istnieje)
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                int target = itemWindowStartIndex + 0;
                if (target < itemCount) selectedItemIndex = target;
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                int target = itemWindowStartIndex + 1;
                if (target < itemCount) selectedItemIndex = target;
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                int target = itemWindowStartIndex + 2;
                if (target < itemCount) selectedItemIndex = target;
            }
            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                int target = itemWindowStartIndex + 3;
                if (target < itemCount) selectedItemIndex = target;
            }
            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                int target = itemWindowStartIndex + 4;
                if (target < itemCount) selectedItemIndex = target;
            }
        }
        else
        {
            selectedItemIndex = 0;
        }

        // -- Obliczanie okna karuzeli (itemWindowStartIndex) i pozycji kursora (selectedSlotIndex) --
        CalculateCarousel(itemCount);

        UpdateInventoryUI(inventory.weapons, inventory.items, inventory.currentWeaponName);
    }

    /// <summary>
    /// Ustawia itemWindowStartIndex oraz selectedSlotIndex tak, by nie pokazywać pustych slotów karuzeli,
    /// nawet przy końcach listy itemów. Kursor przesuwa się automatycznie na 4/5, 7/8 itd.
    /// </summary>
    private void CalculateCarousel(int itemCount)
    {
        // Zakładamy, że selectedItemIndex jest poprawny (0 <= selectedItemIndex < itemCount, jeśli itemCount > 0)
        if (itemCount <= 0)
        {
            itemWindowStartIndex = 0;
            selectedSlotIndex = 0;
            return;
        }

        if (itemCount <= itemWindowSize)
        {
            itemWindowStartIndex = 0;
            selectedSlotIndex = selectedItemIndex;
            return;
        }

        // Jeśli możemy wyśrodkować...
        int preferedStart = selectedItemIndex - 2;
        // Jeśli za mało po lewej
        if (preferedStart <= 0)
        {
            itemWindowStartIndex = 0;
            selectedSlotIndex = selectedItemIndex;
        }
        // Jeśli za mało po prawej
        else if (preferedStart + itemWindowSize >= itemCount)
        {
            itemWindowStartIndex = itemCount - itemWindowSize;
            selectedSlotIndex = selectedItemIndex - itemWindowStartIndex;
        }
        // Normalnie kursor w środku
        else
        {
            itemWindowStartIndex = preferedStart;
            selectedSlotIndex = 2;
        }
    }

    public void UpdateInventoryUI(List<string> weapons, List<GameObject> items, string currentWeaponName)
    {
        int oldWeaponCount = lastWeaponCount;
        int weaponCount = weapons.Count;
        int itemCount = items.Count;
        lastWeaponCount = weaponCount;

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
                    HideWeaponUI();
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
                        HideWeaponUI();
                }
                else
                    HideWeaponUI();
            }
            ShowWeaponUI();
        }
        else
        {
            HideWeaponUI(showBackgroundIfLoot: itemCount > 0);
        }

        UpdateItemUI(items);
        UpdateArrowIndicators(items.Count);
    }

    private void UpdateItemUI(List<GameObject> items)
    {
        int itemCount = items.Count;
        int maxSlots = itemImages.Length;

        for (int i = 0; i < maxSlots; i++)
        {
            int itemIdx = itemWindowStartIndex + i;
            bool hasItem = (itemIdx >= 0 && itemIdx < itemCount);

            // TŁA i NUMERY zawsze aktywne (nie zmieniaj numeracji slotów, używaj tej z edytora)
            if (slotBackgrounds != null && slotBackgrounds[i] != null)
                slotBackgrounds[i].enabled = true;

            if (slotNumberTexts != null && slotNumberTexts[i] != null)
                slotNumberTexts[i].gameObject.SetActive(true);

            // OBRAZEK slotu zawsze aktywny
            if (itemImages[i] != null)
            {
                itemImages[i].enabled = true;
                if (hasItem)
                {
                    GameObject itemObj = items[itemIdx];
                    var item = itemObj?.GetComponent<InteractableItem>();
                    if (item != null && itemIcons.ContainsKey(item.itemName))
                        itemImages[i].sprite = itemIcons[item.itemName];
                    else if (item != null)
                        itemImages[i].sprite = defaultItemSprite;
                    else
                        itemImages[i].sprite = null;

                    itemImages[i].color = (i == selectedSlotIndex) ? selectedItemColor : normalItemColor;
                }
                else
                {
                    itemImages[i].sprite = null;
                    itemImages[i].color = normalItemColor;
                }
            }

            // TEKSTY: tylko jeśli slot ma item
            if (itemTexts[i] != null)
            {
                if (hasItem)
                {
                    GameObject itemObj = items[itemIdx];
                    var treasureResources = itemObj?.GetComponent<TreasureResources>();
                    if (treasureResources != null && treasureResources.resourceCategories != null && treasureResources.resourceCategories.Count > 0)
                    {
                        itemTexts[i].text = treasureResources.resourceCategories[0].resourceCount.ToString();
                        itemTexts[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        itemTexts[i].text = "";
                        itemTexts[i].gameObject.SetActive(false);
                    }
                }
                else
                {
                    itemTexts[i].text = "";
                    itemTexts[i].gameObject.SetActive(false);
                }
            }

            if (itemCategoryTexts[i] != null)
            {
                if (hasItem)
                {
                    GameObject itemObj = items[itemIdx];
                    var treasureResources = itemObj?.GetComponent<TreasureResources>();
                    if (treasureResources != null && treasureResources.resourceCategories != null && treasureResources.resourceCategories.Count > 0)
                    {
                        itemCategoryTexts[i].text = treasureResources.resourceCategories[0].name;
                        itemCategoryTexts[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        itemCategoryTexts[i].text = "";
                        itemCategoryTexts[i].gameObject.SetActive(false);
                    }
                }
                else
                {
                    itemCategoryTexts[i].text = "";
                    itemCategoryTexts[i].gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// Pokazuje/ukrywa wskaźniki strzałek UI, gdy poza slotami są jeszcze itemy.
    /// </summary>
    private void UpdateArrowIndicators(int itemCount)
    {
        if (leftArrowIndicator != null)
            leftArrowIndicator.SetActive(itemWindowStartIndex > 0);

        if (rightArrowIndicator != null)
            rightArrowIndicator.SetActive(itemWindowStartIndex + itemWindowSize < itemCount);
    }

    public void UpdateWeaponUI(Gun gun)
    {
        if (gun == null) return;

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

    // Pobierz wybrany item (kursor – aktualny selectedItemIndex)
    public GameObject GetSelectedItem()
    {
        var items = Inventory.Instance.items;
        int idx = selectedItemIndex;
        if (idx < 0 || idx >= items.Count) return null;
        return items[idx];
    }
}