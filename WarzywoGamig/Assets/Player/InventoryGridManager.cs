using UnityEngine;

public class InventoryGridManager : MonoBehaviour
{
    public static InventoryGridManager Instance { get; private set; }

    public GameObject backpackPanel;
    public InventorySlotUI[] slots;
    public int unlockedSlots = 12;
    public GameObject crosshair;

    private PlayerMovement playerMovementScript;
    private MouseLook playerCameraScript;

    public static bool InventoryOpen = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        playerMovementScript = Object.FindFirstObjectByType<PlayerMovement>();
        playerCameraScript = Object.FindFirstObjectByType<MouseLook>();

        UpdateUnlockedSlots();
        if (backpackPanel != null)
            backpackPanel.SetActive(false);

        InventoryOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        bool nowOpen = !backpackPanel.activeSelf;
        backpackPanel.SetActive(nowOpen);

        InventoryOpen = nowOpen;

        if (playerMovementScript != null)
            playerMovementScript.enabled = !nowOpen;
        if (playerCameraScript != null)
            playerCameraScript.enabled = !nowOpen;

        if (crosshair != null)
            crosshair.SetActive(!nowOpen);

        if (nowOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void UpdateUnlockedSlots()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].SetUnlocked(i < unlockedSlots);
        }
    }

    public void UnlockMoreSlots(int amount)
    {
        unlockedSlots = Mathf.Min(unlockedSlots + amount, slots.Length);
        UpdateUnlockedSlots();
    }

    // Zmiana: przekazuj InteractableItem zamiast Item
    public void SetItemInSlot(int slotIndex, InteractableItem item, bool showNumber)
    {
        if (slots != null && slotIndex >= 0 && slotIndex < slots.Length)
        {
            slots[slotIndex].SetItem(item);

            if (showNumber && item != null)
                slots[slotIndex].SetSlotNumber(slotIndex);
            else
                slots[slotIndex].HideSlotNumber();
        }
    }

    // Zmiana: przekazuj InteractableItem zamiast Item
    public void AddItemToGrid(InteractableItem item)
    {
        for (int i = 0; i < slots.Length && i < unlockedSlots; i++)
        {
            if (slots[i].IsEmpty)
            {
                slots[i].SetItem(item);
                slots[i].SetSlotNumber(i);
                break;
            }
        }
    }
}