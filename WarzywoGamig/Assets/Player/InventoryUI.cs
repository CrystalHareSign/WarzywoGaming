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

    // --- Karuzele dla obu kategorii ---
    private enum ItemCategory { Normal, Usable }
    private ItemCategory activeCategory = ItemCategory.Normal;

    // Zwykłe itemy
    private int itemWindowStartIndex_Normal = 0;
    private int selectedSlotIndex_Normal = 0;
    private int selectedItemIndex_Normal = 0;
    // Usable itemy
    private int itemWindowStartIndex_Usable = 0;
    private int selectedSlotIndex_Usable = 0;
    private int selectedItemIndex_Usable = 0;

    private const int itemWindowSize = 5;

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

        // Przełączanie kategorii TAB-em
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (activeCategory == ItemCategory.Normal)
                activeCategory = ItemCategory.Usable;
            else
                activeCategory = ItemCategory.Normal;
        }

        // Wybierz aktualną listę i wskaźniki
        List<GameObject> currentList;
        ref int itemWindowStartIndex = ref itemWindowStartIndex_Normal;
        ref int selectedSlotIndex = ref selectedSlotIndex_Normal;
        ref int selectedItemIndex = ref selectedItemIndex_Normal;

        if (activeCategory == ItemCategory.Normal)
        {
            currentList = inventory.items;
            itemWindowStartIndex = ref itemWindowStartIndex_Normal;
            selectedSlotIndex = ref selectedSlotIndex_Normal;
            selectedItemIndex = ref selectedItemIndex_Normal;
        }
        else
        {
            currentList = inventory.usableItems;
            itemWindowStartIndex = ref itemWindowStartIndex_Usable;
            selectedSlotIndex = ref selectedSlotIndex_Usable;
            selectedItemIndex = ref selectedItemIndex_Usable;
        }

        int itemCount = currentList.Count;
        int maxSelectedIndex = Mathf.Max(0, itemCount - 1);

        // Scroll & klawisze wyboru slotu
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

        // Ustal okno i slot kursora dla tej kategorii
        CalculateCarousel(itemCount, ref itemWindowStartIndex, ref selectedSlotIndex, ref selectedItemIndex);

        UpdateInventoryUI(inventory.weapons, inventory.items, inventory.usableItems, inventory.currentWeaponName);
    }

    /// <summary>
    /// Karuzela dla danej kategorii (okno, slot kursora)
    /// </summary>
    private void CalculateCarousel(int itemCount, ref int itemWindowStartIndex, ref int selectedSlotIndex, ref int selectedItemIndex)
    {
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
        int preferedStart = selectedItemIndex - 2;
        if (preferedStart <= 0)
        {
            itemWindowStartIndex = 0;
            selectedSlotIndex = selectedItemIndex;
        }
        else if (preferedStart + itemWindowSize >= itemCount)
        {
            itemWindowStartIndex = itemCount - itemWindowSize;
            selectedSlotIndex = selectedItemIndex - itemWindowStartIndex;
        }
        else
        {
            itemWindowStartIndex = preferedStart;
            selectedSlotIndex = 2;
        }
    }

    // Przeciążona wersja do kompatybilności ze starym Inventory
    public void UpdateInventoryUI(List<string> weapons, List<GameObject> items, string currentWeaponName)
    {
        UpdateInventoryUI(weapons, items, null, currentWeaponName);
    }

    // Nowa wersja obsługująca usableItems
    public void UpdateInventoryUI(List<string> weapons, List<GameObject> items, List<GameObject> usableItems, string currentWeaponName)
    {
        int oldWeaponCount = lastWeaponCount;
        int weaponCount = weapons.Count;
        lastWeaponCount = weaponCount;

        Gun gun = null;
        GameObject currentWeaponPrefab = null;
        if (Inventory.Instance != null)
            currentWeaponPrefab = Inventory.Instance.currentWeaponPrefab;

        // Bronie
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
            HideWeaponUI(showBackgroundIfLoot: (items?.Count ?? 0) + (usableItems?.Count ?? 0) > 0);
        }

        // Wybierz do wyświetlenia odpowiednią kategorię
        List<GameObject> shownList = (activeCategory == ItemCategory.Normal) ? items : usableItems;
        int itemCount = shownList != null ? shownList.Count : 0;
        int windowStart = (activeCategory == ItemCategory.Normal) ? itemWindowStartIndex_Normal : itemWindowStartIndex_Usable;
        int slotCursor = (activeCategory == ItemCategory.Normal) ? selectedSlotIndex_Normal : selectedSlotIndex_Usable;

        UpdateItemUI(shownList, windowStart, slotCursor);
        UpdateArrowIndicators(itemCount, windowStart);
    }

    private void UpdateItemUI(List<GameObject> items, int windowStart, int slotCursor)
    {
        int itemCount = items != null ? items.Count : 0;
        int maxSlots = itemImages.Length;

        for (int i = 0; i < maxSlots; i++)
        {
            int itemIdx = windowStart + i;
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

                    itemImages[i].color = (i == slotCursor) ? selectedItemColor : normalItemColor;
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
                        int count = treasureResources.resourceCategories[0].resourceCount;
                        if (count > 1)
                        {
                            itemTexts[i].text = count.ToString();
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
    private void UpdateArrowIndicators(int itemCount, int windowStart)
    {
        if (leftArrowIndicator != null)
            leftArrowIndicator.SetActive(windowStart > 0);

        if (rightArrowIndicator != null)
            rightArrowIndicator.SetActive(windowStart + itemWindowSize < itemCount);
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

    // Pobierz wybrany item - z aktualnie aktywnej kategorii
    public GameObject GetSelectedItem()
    {
        Inventory inventory = Inventory.Instance;
        List<GameObject> items = (activeCategory == ItemCategory.Normal) ? inventory.items : inventory.usableItems;
        int idx = (activeCategory == ItemCategory.Normal) ? selectedItemIndex_Normal : selectedItemIndex_Usable;
        if (idx < 0 || idx >= items.Count) return null;
        return items[idx];
    }

    // Publiczne gettery na slot i okno dla aktywnej kategorii
    public int GetSelectedSlotIndex()
    {
        return (activeCategory == ItemCategory.Normal) ? selectedSlotIndex_Normal : selectedSlotIndex_Usable;
    }
    public int GetItemWindowStartIndex()
    {
        return (activeCategory == ItemCategory.Normal) ? itemWindowStartIndex_Normal : itemWindowStartIndex_Usable;
    }

    // Publiczne settery dla slotu i okna dla zwykłych itemów
    public void SetSelectedSlotIndex_Normal(int value)
    {
        selectedSlotIndex_Normal = Mathf.Clamp(value, 0, itemImages.Length - 1);
    }
    public void SetItemWindowStartIndex_Normal(int value)
    {
        itemWindowStartIndex_Normal = Mathf.Max(0, value);
    }
}