using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("UI Waluty gracza")]
    public TMP_Text playerCurrencyText;

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

    public Image categoryIndicatorImage;
    public Color normalCategoryColor = Color.green;
    public Color usableCategoryColor = Color.yellow;

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

    [Header("Boost Timer UI")]
    public GameObject boostTimerPanel;
    public Image boostTimerFillImage;
    public TextMeshProUGUI boostValueText;

    [Header("Hold To Use UI")]
    public Image holdToUseProgressImage;
    public float requiredHoldTime = 2f;
    public KeyCode useKey = KeyCode.F;
    public float slowPercent = 0.5f;
    public bool isHoldingUse = false;
    private float holdTimer = 0f;
    private float originalMoveSpeed = 0f;

    [Header("Hold Tab To Use Menu")]
    public float menuHoldTime = 0.5f;
    private float tabHoldTimer = 0f;
    private bool tabPressed = false;

    [Header("Inventory Menu Panel")]
    public GameObject inventoryMenuPanel;

    [Header("Statyczna lista zwykłych itemów (max 20)")]
    public Image[] normalItemImages = new Image[20];
    public Image[] normalItemBackgrounds = new Image[20];
    public TMP_Text[] normalItemTexts = new TMP_Text[20];
    public TMP_Text[] normalItemCategoryTexts = new TMP_Text[20];

    [Header("Statyczna lista używalnych itemów (max 20)")]
    public Image[] usableItemImages = new Image[20];
    public Image[] usableItemBackgrounds = new Image[20];
    public TMP_Text[] usableItemTexts = new TMP_Text[20];
    public TMP_Text[] usableItemCategoryTexts = new TMP_Text[20];

    [Header("Tab Buttons")]
    public Button dataTabButton;
    public Button notesTabButton;
    public Button infoTabButton;
    public Button questsTabButton;
    public Button otherTabButton;

    [Header("Tab Contents")]
    public GameObject dataTabContent;
    public GameObject notesTabContent;
    public GameObject infoTabContent;
    public GameObject questsTabContent;
    public GameObject otherTabContent;

    [Header("Item description")]
    public TextMeshProUGUI itemDescriptionText;

    public MouseLook mouseLook;
    public PlayerInteraction playerInteraction;
    public NotesManagerUI notesManagerUI;

    private List<PlaySoundOnObject> playSoundObjects = new List<PlaySoundOnObject>();

    private int activeTabIndex = 0;
    public bool isMenuActive = false;

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
        if (mouseLook == null)
            mouseLook = UnityEngine.Object.FindFirstObjectByType<MouseLook>();

        playSoundObjects.AddRange(Object.FindObjectsByType<PlaySoundOnObject>(FindObjectsSortMode.None));

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

        UpdateCategoryIndicatorSprite();

        // BOOST TIMER UI
        if (boostTimerPanel != null) boostTimerPanel.SetActive(false);
        if (boostTimerFillImage != null) boostTimerFillImage.fillAmount = 0f;
        if (boostValueText != null) boostValueText.text = "";

        // HOLD TO USE UI
        if (holdToUseProgressImage != null)
        {
            holdToUseProgressImage.fillAmount = 0f;
            holdToUseProgressImage.gameObject.SetActive(false);
        }

        if (inventoryMenuPanel != null)
            inventoryMenuPanel.SetActive(false);

        if (dataTabButton != null) dataTabButton.onClick.AddListener(() => ShowTab(0));
        if (notesTabButton != null) notesTabButton.onClick.AddListener(() => ShowTab(1));
        if (infoTabButton != null) infoTabButton.onClick.AddListener(() => ShowTab(2));
        if (questsTabButton != null) questsTabButton.onClick.AddListener(() => ShowTab(3));
        if (otherTabButton != null) otherTabButton.onClick.AddListener(() => ShowTab(4));


        ShowTab(0);
    }

    public void Update()
    {
        if (isInputBlocked)
            return;
        if (DialogueManager.DialogueActive)
            return;

        // --- AUDIO PAUSE/RESUME SYSTEM ---
        // (Zalecane: playSoundObjects jako pole w klasie, przypisanie w Start)
        // Dodaj logikę pauzowania/restartowania dźwięków przy otwieraniu/zamykaniu menu

        // Zamknij inventory ESC
        if (isMenuActive && Input.GetKeyDown(KeyCode.Escape))
        {
            isMenuActive = false;
            if (inventoryMenuPanel != null)
                inventoryMenuPanel.SetActive(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
            if (mouseLook == null)
                mouseLook = UnityEngine.Object.FindFirstObjectByType<MouseLook>();
            if (mouseLook != null)
                mouseLook.enabled = true; // ODblokuj ruch kamerą

            // Odblokuj interakcje gracza po zamknięciu menu
            if (playerInteraction != null)
                playerInteraction.enabled = true;

            // --- WZNAWIANIE DŹWIĘKÓW ---
            if (playSoundObjects != null)
            {
                foreach (var playSoundOnObject in playSoundObjects)
                {
                    if (playSoundOnObject == null) continue;
                    playSoundOnObject.FadeOutSound("InventoryMenuMusic", 1f); // fade out muzyki menu
                    playSoundOnObject.ResumeAllSoundsExcept(new string[] { "InventoryMenuMusic" }, 0.5f); // fade in pozostałe
                }
            }

            tabPressed = false;
            tabHoldTimer = 0f;
            return;
        }

        // --- TAB obsługa ---
        if (!isMenuActive) // MENU ZAMKNIĘTE
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                tabPressed = true;
                tabHoldTimer = 0f;
            }

            if (tabPressed)
            {
                if (Input.GetKey(KeyCode.Tab))
                {
                    tabHoldTimer += Time.unscaledDeltaTime;
                    if (tabHoldTimer >= menuHoldTime)
                    {
                        isMenuActive = true;
                        if (inventoryMenuPanel != null)
                            inventoryMenuPanel.SetActive(true);

                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                        Time.timeScale = 0f;
                        ShowTab(activeTabIndex);

                        if (mouseLook == null)
                            mouseLook = UnityEngine.Object.FindFirstObjectByType<MouseLook>();
                        if (mouseLook != null)
                            mouseLook.enabled = false; // ZABLOKUJ ruch kamerą

                        // Zablokuj interakcje gracza na czas menu
                        if (playerInteraction != null)
                            playerInteraction.enabled = false;

                        // --- PAUZOWANIE DŹWIĘKÓW ---
                        if (playSoundObjects != null)
                        {
                            foreach (var playSoundOnObject in playSoundObjects)
                            {
                                if (playSoundOnObject == null) continue;
                                playSoundOnObject.PlaySound("InventoryMenuMusic", 1.0f, true); // jeśli masz osobną muzykę menu
                                playSoundOnObject.PauseAllSoundsExcept(new string[] { "InventoryMenuMusic" }, 0.5f); // fade out inne
                            }
                        }

                        tabPressed = false;
                        tabHoldTimer = 0f;
                        return;
                    }
                }
                else
                {
                    if (tabHoldTimer < menuHoldTime)
                    {
                        if (activeCategory == ItemCategory.Normal)
                            activeCategory = ItemCategory.Usable;
                        else
                            activeCategory = ItemCategory.Normal;

                        UpdateCategoryIndicatorSprite();
                    }
                    tabPressed = false;
                    tabHoldTimer = 0f;
                }
            }
        }
        else // MENU OTWARTE
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                isMenuActive = false;
                if (inventoryMenuPanel != null)
                    inventoryMenuPanel.SetActive(false);

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1f;
                if (mouseLook == null)
                    mouseLook = UnityEngine.Object.FindFirstObjectByType<MouseLook>();
                if (mouseLook != null)
                    mouseLook.enabled = true; // ODblokuj ruch kamerą

                // Odblokuj interakcje gracza po zamknięciu menu
                if (playerInteraction != null)
                    playerInteraction.enabled = true;

                // --- WZNAWIANIE DŹWIĘKÓW ---
                if (playSoundObjects != null)
                {
                    foreach (var playSoundOnObject in playSoundObjects)
                    {
                        if (playSoundOnObject == null) continue;
                        playSoundOnObject.FadeOutSound("InventoryMenuMusic", 1f); // fade out muzyki menu
                        playSoundOnObject.ResumeAllSoundsExcept(new string[] { "InventoryMenuMusic" }, 0.5f); // fade in pozostałe
                    }
                }

                tabPressed = false;
                tabHoldTimer = 0f;
                return;
            }
        }

        // Jeśli menu aktywne, blokuj resztę UI inventory
        if (inventoryMenuPanel != null && inventoryMenuPanel.activeSelf)
            return;

        var inventory = Inventory.Instance;
        int weaponSlots = Mathf.Min(inventory.weapons.Count, 3);

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

        // --- HOLD TO USE LOGIC ---
        if (activeCategory == ItemCategory.Usable)
        {
            // BLOKADA użycia itema jeśli gracz trzyma loot:
            if (Inventory.Instance != null && Inventory.Instance.IsHoldingLoot)
            {
                if (holdToUseProgressImage != null)
                {
                    holdToUseProgressImage.fillAmount = 0f;
                    holdToUseProgressImage.gameObject.SetActive(false);
                }
                isHoldingUse = false;
                holdTimer = 0f;
                return;
            }

            if (!isHoldingUse && Input.GetKeyDown(useKey))
            {
                if (Inventory.Instance != null && Inventory.Instance.currentWeaponPrefab != null)
                {
                    Gun gun = Inventory.Instance.currentWeaponPrefab.GetComponent<Gun>();
                    if (gun != null && gun.IsReloading())
                        gun.CancelReload();
                }

                isHoldingUse = true;
                holdTimer = 0f;

                var player = UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();
                if (player != null)
                {
                    if (!player.isSlowedByItemUse)
                    {
                        originalMoveSpeed = player.moveSpeed;
                        player.moveSpeed *= slowPercent;
                        player.isSlowedByItemUse = true;
                    }
                    player.isSprintBlockedByUI = true;
                    player.StopSprinting();
                }

                if (holdToUseProgressImage != null)
                {
                    holdToUseProgressImage.fillAmount = 0f;
                    holdToUseProgressImage.gameObject.SetActive(true);
                }
            }

            if (isHoldingUse)
            {
                if (Input.GetKey(useKey))
                {
                    holdTimer += Time.unscaledDeltaTime;
                    float prog = Mathf.Clamp01(holdTimer / requiredHoldTime);

                    if (holdToUseProgressImage != null)
                    {
                        holdToUseProgressImage.fillAmount = prog;
                        holdToUseProgressImage.gameObject.SetActive(true);
                    }

                    if (holdTimer >= requiredHoldTime)
                    {
                        isHoldingUse = false;
                        holdTimer = 0f;

                        var player = UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();
                        if (player != null)
                        {
                            if (player.isSlowedByItemUse)
                            {
                                player.moveSpeed = originalMoveSpeed;
                                player.isSlowedByItemUse = false;
                            }
                            player.isSprintBlockedByUI = false;
                        }

                        if (holdToUseProgressImage != null)
                        {
                            holdToUseProgressImage.fillAmount = 0f;
                            holdToUseProgressImage.gameObject.SetActive(false);
                        }

                        TryUseSelectedUsableItem();
                    }
                }
                else
                {
                    isHoldingUse = false;
                    holdTimer = 0f;

                    var player = UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();
                    if (player != null)
                    {
                        if (player.isSlowedByItemUse)
                        {
                            player.moveSpeed = originalMoveSpeed;
                            player.isSlowedByItemUse = false;
                        }
                        player.isSprintBlockedByUI = false;
                    }

                    if (holdToUseProgressImage != null)
                    {
                        holdToUseProgressImage.fillAmount = 0f;
                        holdToUseProgressImage.gameObject.SetActive(false);
                    }
                }
            }
        }
        else
        {
            if (holdToUseProgressImage != null)
            {
                holdToUseProgressImage.fillAmount = 0f;
                holdToUseProgressImage.gameObject.SetActive(false);
            }
            isHoldingUse = false;
            holdTimer = 0f;
        }

        UpdateInventoryUI(inventory.weapons, inventory.items, inventory.usableItems, inventory.currentWeaponName);

        // BOOST TIMER UI update
        UpdateBoostTimerUI();
    }

    public void UpdatePlayerCurrency(float value)
    {
        if (playerCurrencyText != null)
            playerCurrencyText.text = value.ToString("0.##");
        else
            Debug.LogWarning("InventoryUI: playerCurrencyText nie jest przypisany!");
    }

    public void TryUseSelectedUsableItem()
    {
        if (activeCategory != ItemCategory.Usable)
            return;

        if (Inventory.Instance == null)
        {
            Debug.LogWarning("Inventory.Instance is null!");
            return;
        }
        int idx = selectedItemIndex_Usable;
        if (Inventory.Instance.usableItems == null)
        {
            Debug.LogWarning("usableItems is null!");
            return;
        }
        if (idx < 0 || idx >= Inventory.Instance.usableItems.Count)
        {
            Debug.LogWarning($"idx out of range: {idx}");
            return;
        }

        GameObject itemObj = Inventory.Instance.usableItems[idx];
        if (itemObj == null)
        {
            Debug.LogWarning($"itemObj at idx {idx} is null!");
            return;
        }

        var interactable = itemObj.GetComponent<InteractableItem>();
        if (interactable == null)
        {
            Debug.LogWarning("InteractableItem component missing!");
            return;
        }
        if (!interactable.isUsableItem || interactable.usableType != UsableType.Quick)
        {
            Debug.LogWarning("Item is not usable or not quick usable!");
            return;
        }

        // Nowość: wywołanie przez managera efektów
        if (interactable.data != null)
        {
            if (ItemEffectManager.Instance == null)
            {
                Debug.LogWarning("ItemEffectManager.Instance is null!");
                return;
            }
            // BOOST TIMER UI --- wyświetl jeśli to boost
            if (interactable.data.effectType == ItemEffectType.StaminaBoost)
            {
                ShowBoostTimerUI(interactable.data.effectValue, interactable.data.effectDuration);
            }

            // POPRAWKA: sprawdzamy, czy item został faktycznie użyty
            bool used = ItemEffectManager.Instance.UseItem(interactable.data, itemObj);
            if (!used)
            {
                // Nie usuwaj itemu, jeśli nie został użyty!
                return;
            }
        }
        else if (interactable.onInteract != null)
        {
            interactable.onInteract.Invoke();
        }
        else
        {
            Debug.LogWarning("No data or onInteract found!");
        }

        // Po usunięciu itema
        Inventory.Instance.usableItems.RemoveAt(idx);
        Destroy(itemObj);

        // Cofnij kursor jeśli był poza końcem listy
        if (selectedItemIndex_Usable >= Inventory.Instance.usableItems.Count)
        {
            if (Inventory.Instance.usableItems.Count > 0)
                selectedItemIndex_Usable = Inventory.Instance.usableItems.Count - 1;
            else
                selectedItemIndex_Usable = 0;
        }

        UpdateInventoryUI(
            Inventory.Instance.weapons,
            Inventory.Instance.items,
            Inventory.Instance.usableItems,
            Inventory.Instance.currentWeaponName
        );
    }

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

        // zsynchronizuj kolor wskaźnika kategorii także przy UpdateInventoryUI (nie tylko po TAB)
        UpdateCategoryIndicatorSprite();
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
                itemImages[i].enabled = true;

            // Sprawdzenie czy item istnieje i nie został zniszczony
            GameObject itemObj = (hasItem && items != null) ? items[itemIdx] : null;
            bool validItem = hasItem && itemObj != null;

            // OBRAZEK slotu
            if (itemImages[i] != null)
            {
                if (validItem)
                {
                    var item = itemObj.GetComponent<InteractableItem>();
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

            // TEKSTY: tylko jeśli slot ma poprawny item
            if (itemTexts[i] != null)
            {
                if (validItem)
                {
                    var treasureResources = itemObj.GetComponent<TreasureResources>();
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
                if (validItem)
                {
                    var treasureResources = itemObj.GetComponent<TreasureResources>();
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

    private void UpdateArrowIndicators(int itemCount, int windowStart)
    {
        if (leftArrowIndicator != null)
            leftArrowIndicator.SetActive(windowStart > 0);

        if (rightArrowIndicator != null)
            rightArrowIndicator.SetActive(windowStart + itemWindowSize < itemCount);
    }

    private void UpdateCategoryIndicatorSprite()
    {
        if (categoryIndicatorImage == null) return;

        if (activeCategory == ItemCategory.Normal)
            categoryIndicatorImage.color = normalCategoryColor;
        else
            categoryIndicatorImage.color = usableCategoryColor;
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
        // SCHOWAJ wskaźnik kategorii
        if (categoryIndicatorImage != null)
            categoryIndicatorImage.gameObject.SetActive(false);

        // BOOST TIMER UI
        if (boostTimerPanel != null)
            boostTimerPanel.SetActive(false);

        // HOLD TO USE UI
        if (holdToUseProgressImage != null)
        {
            holdToUseProgressImage.fillAmount = 0f;
            holdToUseProgressImage.gameObject.SetActive(false);
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
        // POKAŻ wskaźnik kategorii
        if (categoryIndicatorImage != null)
            categoryIndicatorImage.gameObject.SetActive(true);
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

    // Getter i setter dla indeksu wybranego itemu (zwykłe itemy)
    public int GetSelectedItemIndex_Normal()
    {
        return selectedItemIndex_Normal;
    }
    public void SetSelectedItemIndex_Normal(int value)
    {
        selectedItemIndex_Normal = Mathf.Clamp(value, 0, Mathf.Max(0, Inventory.Instance.items.Count - 1));
    }

    public void ShowBoostTimerUI(float boostValue, float duration)
    {
        if (boostTimerFillImage != null)
            boostTimerFillImage.fillAmount = 1f;
        if (boostValueText != null)
            boostValueText.text = BoostValueToRoman(boostValue);
        if (boostTimerPanel != null)
            boostTimerPanel.SetActive(true);
    }

    public void UpdateBoostTimerUI()
    {
        var stats = PlayerStats.Instance;
        if (stats == null || stats.staminaBonus <= 0f || stats.staminaBonusDuration <= 0f)
        {
            if (boostTimerPanel != null) boostTimerPanel.SetActive(false);
            if (boostValueText != null) boostValueText.text = "";
            return;
        }

        if (boostTimerPanel != null) boostTimerPanel.SetActive(true);

        float left = Mathf.Clamp01(stats.staminaBonusTimeLeft / stats.staminaBonusDuration);

        if (boostTimerFillImage != null)
            boostTimerFillImage.fillAmount = left;

        if (boostValueText != null)
            boostValueText.text = BoostValueToRoman(stats.staminaBonus);

        if (left <= 0f)
        {
            if (boostTimerPanel != null) boostTimerPanel.SetActive(false);
            if (boostValueText != null) boostValueText.text = "";
        }
    }

    private string BoostValueToRoman(float value)
    {
        if (value >= 100) return "IV";
        if (value >= 75) return "III";
        if (value >= 50) return "II";
        if (value >= 25) return "I";
        return value.ToString("0");
    }

    // --- ZAKŁADKI MENU ---
    public void ShowTab(int tabIndex)
    {
        activeTabIndex = tabIndex;

        if (dataTabContent != null) dataTabContent.SetActive(tabIndex == 0);
        if (notesTabContent != null) notesTabContent.SetActive(tabIndex == 1);
        if (infoTabContent != null) infoTabContent.SetActive(tabIndex == 2);
        if (questsTabContent != null) questsTabContent.SetActive(tabIndex == 3);
        if (otherTabContent != null) otherTabContent.SetActive(tabIndex == 4);

        // Wszystko obsługuje jeden manager!
        switch (tabIndex)
        {
            case 0: // Data
                UpdateDataTab();
                break;
            case 1: // Notes
                if (notesManagerUI != null)
                    notesManagerUI.ShowNotesTab();
                break;
            case 2: // Info
                if (notesManagerUI != null)
                    notesManagerUI.ShowInfoTab();
                break;
            case 3: // Quests
                if (notesManagerUI != null)
                    notesManagerUI.ShowQuestsTab();
                break;
            //case 4: // Other
            //    if (notesManagerUI != null)
            //        notesManagerUI.ShowOtherTab(); // Dodaj metodę ShowOtherTab() w NotesManagerUI jeśli chcesz mieć osobną obsługę
            //    break;
        }
    }

    public void UpdateDataTab()
    {
        // Zwykłe itemy
        for (int i = 0; i < normalItemImages.Length; i++)
        {
            bool hasItem = Inventory.Instance.items != null && i < Inventory.Instance.items.Count;
            var itemObj = hasItem ? Inventory.Instance.items[i] : null;
            var item = hasItem && itemObj != null ? itemObj.GetComponent<InteractableItem>() : null;
            var treasure = hasItem && itemObj != null ? itemObj.GetComponent<TreasureResources>() : null;

            // Ikona slotu
            if (normalItemImages[i] != null)
            {
                normalItemImages[i].enabled = hasItem;
                if (hasItem && item != null && itemIcons.ContainsKey(item.itemName))
                    normalItemImages[i].sprite = itemIcons[item.itemName];
                else if (hasItem && item != null)
                    normalItemImages[i].sprite = defaultItemSprite;
                else
                    normalItemImages[i].sprite = null;
            }

            // Tło slotu
            if (normalItemBackgrounds[i] != null)
                normalItemBackgrounds[i].enabled = hasItem;

            // Tekst ilości
            if (normalItemTexts[i] != null)
            {
                if (hasItem && treasure != null && treasure.resourceCategories != null && treasure.resourceCategories.Count > 0)
                {
                    int count = treasure.resourceCategories[0].resourceCount;
                    normalItemTexts[i].text = count > 1 ? count.ToString() : "";
                    normalItemTexts[i].gameObject.SetActive(true);
                }
                else
                {
                    normalItemTexts[i].text = "";
                    normalItemTexts[i].gameObject.SetActive(false);
                }
            }
            // Tekst kategorii
            if (normalItemCategoryTexts[i] != null)
            {
                if (hasItem && treasure != null && treasure.resourceCategories != null && treasure.resourceCategories.Count > 0)
                {
                    normalItemCategoryTexts[i].text = treasure.resourceCategories[0].name;
                    normalItemCategoryTexts[i].gameObject.SetActive(true);
                }
                else
                {
                    normalItemCategoryTexts[i].text = "";
                    normalItemCategoryTexts[i].gameObject.SetActive(false);
                }
            }

            // --- DYNAMICZNE EVENTY NA TLE SLOTU ---
            if (normalItemBackgrounds[i] != null)
            {
                var trigger = normalItemBackgrounds[i].GetComponent<UnityEngine.EventSystems.EventTrigger>();
                if (trigger == null)
                    trigger = normalItemBackgrounds[i].gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                trigger.triggers.Clear();

                string desc = "";
                if (hasItem && item != null && item.data != null && item.data.hasDescription)
                    desc = GetItemDescription(item.data);

                var entryEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
                entryEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
                entryEnter.callback.AddListener((eventData) => ShowItemDescription(desc));
                trigger.triggers.Add(entryEnter);

                var entryExit = new UnityEngine.EventSystems.EventTrigger.Entry();
                entryExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
                entryExit.callback.AddListener((eventData) => HideItemDescription());
                trigger.triggers.Add(entryExit);
            }

            // --- DYNAMICZNE EVENTY NA IKONIE ITEMU ---
            if (normalItemImages[i] != null)
            {
                var trigger = normalItemImages[i].GetComponent<UnityEngine.EventSystems.EventTrigger>();
                if (trigger == null)
                    trigger = normalItemImages[i].gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                trigger.triggers.Clear();

                string desc = "";
                if (hasItem && item != null && item.data != null && item.data.hasDescription)
                    desc = GetItemDescription(item.data);

                var entryEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
                entryEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
                entryEnter.callback.AddListener((eventData) => ShowItemDescription(desc));
                trigger.triggers.Add(entryEnter);

                var entryExit = new UnityEngine.EventSystems.EventTrigger.Entry();
                entryExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
                entryExit.callback.AddListener((eventData) => HideItemDescription());
                trigger.triggers.Add(entryExit);
            }
        }

        // Używalne itemy
        for (int i = 0; i < usableItemImages.Length; i++)
        {
            bool hasItem = Inventory.Instance.usableItems != null && i < Inventory.Instance.usableItems.Count;
            var itemObj = hasItem ? Inventory.Instance.usableItems[i] : null;
            var item = hasItem && itemObj != null ? itemObj.GetComponent<InteractableItem>() : null;
            var treasure = hasItem && itemObj != null ? itemObj.GetComponent<TreasureResources>() : null;

            // Ikona slotu
            if (usableItemImages[i] != null)
            {
                usableItemImages[i].enabled = hasItem;
                if (hasItem && item != null && itemIcons.ContainsKey(item.itemName))
                    usableItemImages[i].sprite = itemIcons[item.itemName];
                else if (hasItem && item != null)
                    usableItemImages[i].sprite = defaultItemSprite;
                else
                    usableItemImages[i].sprite = null;
            }

            // Tło slotu
            if (usableItemBackgrounds[i] != null)
                usableItemBackgrounds[i].enabled = hasItem;

            // Tekst ilości
            if (usableItemTexts[i] != null)
            {
                if (hasItem && treasure != null && treasure.resourceCategories != null && treasure.resourceCategories.Count > 0)
                {
                    int count = treasure.resourceCategories[0].resourceCount;
                    usableItemTexts[i].text = count > 1 ? count.ToString() : "";
                    usableItemTexts[i].gameObject.SetActive(true);
                }
                else
                {
                    usableItemTexts[i].text = "";
                    usableItemTexts[i].gameObject.SetActive(false);
                }
            }
            // Tekst kategorii
            if (usableItemCategoryTexts[i] != null)
            {
                if (hasItem && treasure != null && treasure.resourceCategories != null && treasure.resourceCategories.Count > 0)
                {
                    usableItemCategoryTexts[i].text = treasure.resourceCategories[0].name;
                    usableItemCategoryTexts[i].gameObject.SetActive(true);
                }
                else
                {
                    usableItemCategoryTexts[i].text = "";
                    usableItemCategoryTexts[i].gameObject.SetActive(false);
                }
            }

            // --- DYNAMICZNE EVENTY NA TLE SLOTU ---
            if (usableItemBackgrounds[i] != null)
            {
                var trigger = usableItemBackgrounds[i].GetComponent<UnityEngine.EventSystems.EventTrigger>();
                if (trigger == null)
                    trigger = usableItemBackgrounds[i].gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                trigger.triggers.Clear();

                string desc = "";
                if (hasItem && item != null && item.data != null && item.data.hasDescription)
                    desc = GetItemDescription(item.data);

                var entryEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
                entryEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
                entryEnter.callback.AddListener((eventData) => ShowItemDescription(desc));
                trigger.triggers.Add(entryEnter);

                var entryExit = new UnityEngine.EventSystems.EventTrigger.Entry();
                entryExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
                entryExit.callback.AddListener((eventData) => HideItemDescription());
                trigger.triggers.Add(entryExit);
            }

            // --- DYNAMICZNE EVENTY NA IKONIE ITEMU ---
            if (usableItemImages[i] != null)
            {
                var trigger = usableItemImages[i].GetComponent<UnityEngine.EventSystems.EventTrigger>();
                if (trigger == null)
                    trigger = usableItemImages[i].gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                trigger.triggers.Clear();

                string desc = "";
                if (hasItem && item != null && item.data != null && item.data.hasDescription)
                    desc = GetItemDescription(item.data);

                var entryEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
                entryEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
                entryEnter.callback.AddListener((eventData) => ShowItemDescription(desc));
                trigger.triggers.Add(entryEnter);

                var entryExit = new UnityEngine.EventSystems.EventTrigger.Entry();
                entryExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
                entryExit.callback.AddListener((eventData) => HideItemDescription());
                trigger.triggers.Add(entryExit);
            }
        }
    }

    public string GetItemDescription(ItemPrefabData data)
    {
        if (data == null || data.description == null)
            return "";

        string desc;

        switch (LanguageManager.Instance.currentLanguage)
        {
            case LanguageManager.Language.Polski:
                desc = data.description.polish;
                break;
            case LanguageManager.Language.Deutsch:
                desc = data.description.german;
                break;
            case LanguageManager.Language.English:
            default:
                desc = data.description.english;
                break;
        }

        desc = desc.Replace("{amount}", $"<color=#FFD200>{data.effectValue}</color>");
        desc = desc.Replace("{duration}", $"<color=#FFD200>{FormatTime(data.effectDuration)}</color>");
        desc = desc.Replace("{itemName}", $"<color=#FFD200>{data.itemName}</color>");
        desc = desc.Replace("{rarity}", $"<color=#FFD200>{data.lootRarity}</color>");

        return desc;
    }

    // --- funkcja pomocnicza ---
    public string FormatTime(float seconds)
    {
        int min = Mathf.FloorToInt(seconds / 60f);
        int sec = Mathf.FloorToInt(seconds % 60f);
        return $"{min}:{sec:00}";
    }

    public void ShowItemDescription(string desc)
    {
        if (itemDescriptionText != null)
        {
            itemDescriptionText.text = desc;
            itemDescriptionText.gameObject.SetActive(true);
            Debug.Log("[InventoryUI] Pokazuję opis: " + desc);
        }
        else
        {
            Debug.LogWarning("[InventoryUI] Próba pokazania opisu, ale itemDescriptionText == null!");
        }
    }

    public void HideItemDescription()
    {
        if (itemDescriptionText != null)
        {
            Debug.Log("[InventoryUI] Ukrywam opis przedmiotu");
            itemDescriptionText.text = "";
            itemDescriptionText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[InventoryUI] Próba ukrycia opisu, ale itemDescriptionText == null!");
        }
    }
}