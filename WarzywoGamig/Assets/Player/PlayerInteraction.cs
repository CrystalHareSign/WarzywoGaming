using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionRange = 4f;
    public LayerMask interactableLayer;
    public Camera playerCamera;
    public Image progressCircle;
    public TMP_Text messageText;
    public TMP_Text keyText;
    public Inventory inventory;
    public InventoryUI inventoryUI;
    public GameObject crosshair;

    private float interactionTimer = 0f;
    private InteractableItem currentInteractableItem = null;
    private float requiredHoldTime = 5f;
    private TurretController turretController;
    private TreasureRefiner treasureRefiner;
    private CameraToMonitor cameraToMonitor;
    private AudioChanger audioChanger;
    private bool hasRefinerBeenUsed = false;
    private bool wasHoldingLoot = false;

    void Awake()
    {
        if (GameObject.FindGameObjectsWithTag("Player").Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        audioChanger = Object.FindFirstObjectByType<AudioChanger>();
        if (audioChanger == null)
            Debug.LogError("Brak obiektu AudioChanger w scenie.");

        turretController = Object.FindFirstObjectByType<TurretController>();
        if (turretController == null)
            Debug.LogError("Brak obiektu TurretController w scenie.");

        treasureRefiner = Object.FindFirstObjectByType<TreasureRefiner>();
        if (treasureRefiner == null)
            Debug.LogError("Brak obiektu TreasureRefiner w scenie.");

        cameraToMonitor = Object.FindFirstObjectByType<CameraToMonitor>();
        if (cameraToMonitor == null)
            Debug.LogError("Brak obiektu CameraToMonitor w scenie.");

        if (progressCircle != null)
        {
            progressCircle.gameObject.SetActive(false);
            progressCircle.fillAmount = 0f;
        }
        if (messageText != null)
            messageText.gameObject.SetActive(false);
        if (keyText != null)
            keyText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (playerCamera == null)
        {
            Debug.LogError("PlayerCamera is not assigned in the Inspector.");
            return;
        }

        // --- Blokada interakcji gdy aktywny MissionDefiner lub DriverSeat ---
        if (MissionDefiner.IsAnyDefinerActive || DriverSeatInteraction.IsAnyDriverSeatActive)
            return;

        Inventory playerInventory = Object.FindFirstObjectByType<Inventory>();
        bool isHoldingLoot = playerInventory != null && playerInventory.lootParent != null && playerInventory.lootParent.childCount > 0;

        bool isAnyMonitorActive = false;
        var allMonitors = Object.FindObjectsByType<CameraToMonitor>(FindObjectsSortMode.None);
        foreach (var monitor in allMonitors)
        {
            if (monitor.isUsingMonitor)
            {
                isAnyMonitorActive = true;
                break;
            }
        }
        bool isTurretActive = false;
        TurretController checkTurretController = Object.FindFirstObjectByType<TurretController>();
        if (checkTurretController != null && checkTurretController.isUsingTurret)
            isTurretActive = true;

        bool justDroppedLoot = wasHoldingLoot && !isHoldingLoot;
        wasHoldingLoot = isHoldingLoot;

        if (isHoldingLoot)
        {
            HideUI();
            if (inventoryUI != null)
            {
                Transform loot = playerInventory.lootParent.GetChild(0);
                TreasureValue treasureValue = loot.GetComponent<TreasureValue>();
                if (treasureValue != null)
                {
                    inventoryUI.weaponNameText.text = treasureValue.category;
                    inventoryUI.weaponNameText.gameObject.SetActive(true);
                    inventoryUI.ammoText.text = treasureValue.amount.ToString();
                    inventoryUI.ammoText.gameObject.SetActive(true);
                    inventoryUI.totalAmmoText.gameObject.SetActive(false);
                    inventoryUI.slashText.gameObject.SetActive(false);
                    inventoryUI.reloadingText.gameObject.SetActive(false);
                    inventoryUI.weaponImage.gameObject.SetActive(false);
                    inventoryUI.HideItemUI();
                }
                else
                {
                    inventoryUI.HideWeaponUI();
                    inventoryUI.HideItemUI();
                }
            }
            return;
        }

        if (justDroppedLoot && inventoryUI != null && !isAnyMonitorActive && !isTurretActive)
        {
            if (inventory == null || inventory.currentWeaponPrefab == null)
                inventoryUI.HideWeaponUI();
            else
            {
                Gun gunScript = inventory.currentWeaponPrefab.GetComponent<Gun>();
                if (gunScript != null)
                {
                    inventoryUI.UpdateWeaponUI(gunScript);
                    inventoryUI.ShowWeaponUI();
                }
            }
            inventoryUI.ShowItemUI(inventory.items);
            return;
        }

        if (!isAnyMonitorActive && !isTurretActive && inventoryUI != null)
        {
            if (inventory == null || inventory.currentWeaponPrefab == null)
                inventoryUI.HideWeaponUI();
            else
            {
                Gun gunScript = inventory.currentWeaponPrefab.GetComponent<Gun>();
                if (gunScript != null)
                {
                    inventoryUI.UpdateWeaponUI(gunScript);
                    inventoryUI.ShowWeaponUI();
                }
            }
            inventoryUI.ShowItemUI(inventory.items);
        }

        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, interactionRange, interactableLayer))
        {
            InteractableItem interactableItem = hit.collider.GetComponent<InteractableItem>();
            if (interactableItem != null && !interactableItem.hoverMessage.isInteracted)
            {
                // --- FOTEL KIEROWCY: przytrzymaj E aby potwierdziæ podró¿ ---
                if (interactableItem.isDriverSeat)
                {
                    requiredHoldTime = interactableItem.requiredHoldTime > 0f ? interactableItem.requiredHoldTime : 2f;
                    if (messageText != null && interactableItem.hoverMessage != null)
                    {
                        messageText.text = interactableItem.hoverMessage.message;
                        messageText.fontSize = interactableItem.hoverMessage.messageFontSize;
                        messageText.gameObject.SetActive(true);
                    }
                    if (keyText != null && interactableItem.hoverMessage != null)
                    {
                        keyText.text = interactableItem.hoverMessage.keyText;
                        keyText.fontSize = interactableItem.hoverMessage.keyFontSize;
                        keyText.gameObject.SetActive(true);
                    }
                    if (progressCircle != null)
                        progressCircle.gameObject.SetActive(true);

                    if (Input.GetKey(KeyCode.E))
                    {
                        if (currentInteractableItem == interactableItem)
                        {
                            interactionTimer += Time.deltaTime;
                            if (progressCircle != null)
                                progressCircle.fillAmount = interactionTimer / requiredHoldTime;
                            if (interactionTimer >= requiredHoldTime)
                            {
                                UseDriverSeat(interactableItem);
                                interactionTimer = 0f;
                                HideUI();
                            }
                        }
                        else
                        {
                            currentInteractableItem = interactableItem;
                            interactionTimer = 0f;
                            if (progressCircle != null)
                                progressCircle.fillAmount = 0f;
                        }
                    }
                    else
                    {
                        interactionTimer = 0f;
                        if (progressCircle != null)
                            progressCircle.fillAmount = 0f;
                    }
                    return;
                }

                bool isWheel = interactableItem.category == "Wheel" || interactableItem.itemName.StartsWith("Opona");
                if (isWheel && audioChanger != null && audioChanger.isPlayerInside)
                {
                    HideUI();
                    return;
                }
                bool isBusMonitor = interactableItem.isMonitor && interactableItem.busMonitor;
                if (isBusMonitor && audioChanger != null && !audioChanger.isPlayerInside)
                {
                    HideUI();
                    return;
                }

                if (messageText != null && interactableItem.hoverMessage != null)
                {
                    messageText.text = interactableItem.hoverMessage.message;
                    messageText.fontSize = interactableItem.hoverMessage.messageFontSize;
                    messageText.gameObject.SetActive(true);
                }
                if (keyText != null && interactableItem.hoverMessage != null)
                {
                    keyText.text = interactableItem.hoverMessage.keyText;
                    keyText.fontSize = interactableItem.hoverMessage.keyFontSize;
                    keyText.gameObject.SetActive(true);
                }
                if (progressCircle != null)
                {
                    progressCircle.gameObject.SetActive(true);
                }

                if (Input.GetKey(KeyCode.E))
                {
                    if (currentInteractableItem == interactableItem)
                    {
                        requiredHoldTime = currentInteractableItem.requiredHoldTime;
                        interactionTimer += Time.deltaTime;

                        if (requiredHoldTime > 0f && interactionTimer == Time.deltaTime)
                        {
                            if (inventory != null && inventory.currentWeaponPrefab != null)
                            {
                                Gun gunScript = inventory.currentWeaponPrefab.GetComponent<Gun>();
                                if (gunScript != null)
                                    gunScript.CancelReload();

                                inventory.currentWeaponPrefab.SetActive(false);
                                inventory.enabled = false;
                                inventoryUI.UpdateWeaponUI(inventory.currentWeaponPrefab.GetComponent<Gun>());
                                inventoryUI.HideWeaponUI();
                            }
                        }

                        if (progressCircle != null)
                        {
                            progressCircle.fillAmount = interactionTimer / requiredHoldTime;
                        }

                        if (interactionTimer >= requiredHoldTime)
                        {
                            if (currentInteractableItem.isTurret)
                            {
                                UseTurret(interactableItem);

                                if (inventory != null && inventory.currentWeaponPrefab != null)
                                {
                                    Gun gunScript = inventory.currentWeaponPrefab.GetComponent<Gun>();
                                    if (gunScript != null)
                                        gunScript.CancelReload();
                                    inventory.currentWeaponPrefab.SetActive(false);
                                    inventory.enabled = false;
                                    inventoryUI.UpdateWeaponUI(inventory.currentWeaponPrefab.GetComponent<Gun>());
                                    inventoryUI.HideWeaponUI();
                                }
                                if (inventoryUI != null)
                                    inventoryUI.HideItemUI();
                            }

                            if (currentInteractableItem.isMonitor)
                            {
                                UseMonitor(interactableItem);

                                if (inventory != null && inventory.currentWeaponPrefab != null)
                                {
                                    Gun gunScript = inventory.currentWeaponPrefab.GetComponent<Gun>();
                                    if (gunScript != null)
                                        gunScript.CancelReload();
                                    inventory.currentWeaponPrefab.SetActive(false);
                                    inventory.enabled = false;
                                    inventoryUI.UpdateWeaponUI(inventory.currentWeaponPrefab.GetComponent<Gun>());
                                    inventoryUI.HideWeaponUI();
                                }
                                if (inventoryUI != null)
                                    inventoryUI.HideItemUI();
                            }

                            // --- MissionDefiner obs³uga ---
                            if (currentInteractableItem.isMissionDefiner)
                            {
                                UseMissionDefiner(interactableItem);

                                if (inventory != null && inventory.currentWeaponPrefab != null)
                                {
                                    Gun gunScript = inventory.currentWeaponPrefab.GetComponent<Gun>();
                                    if (gunScript != null)
                                        gunScript.CancelReload();
                                    inventory.currentWeaponPrefab.SetActive(false);
                                    inventory.enabled = false;
                                    inventoryUI.UpdateWeaponUI(inventory.currentWeaponPrefab.GetComponent<Gun>());
                                    inventoryUI.HideWeaponUI();
                                }
                                if (inventoryUI != null)
                                    inventoryUI.HideItemUI();
                            }
                            // -----------------------------

                            if (currentInteractableItem.isRefiner && !hasRefinerBeenUsed)
                            {
                                RemoveOldestItemFromInventory(interactableItem);
                                inventory.RefreshItemListChronologically();
                                hasRefinerBeenUsed = true;
                            }
                            else if (!currentInteractableItem.isTurret && !currentInteractableItem.isMonitor && !currentInteractableItem.isMissionDefiner && !currentInteractableItem.isRefiner)
                            {
                                InteractWithObject(currentInteractableItem);
                            }

                            interactionTimer = 0f;
                            HideUI();
                        }
                    }
                    else
                    {
                        currentInteractableItem = interactableItem;
                        interactionTimer = 0f;
                        if (progressCircle != null)
                        {
                            progressCircle.fillAmount = 0f;
                        }
                    }
                }
                else
                {
                    hasRefinerBeenUsed = false;
                    interactionTimer = 0f;
                    if (progressCircle != null)
                    {
                        progressCircle.fillAmount = 0f;
                    }
                }
            }
            else
            {
                ResetInteraction();
            }
        }
        else
        {
            ResetInteraction();
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // --- DODAJ TO: wymuœ ukrycie UI i broni jeœli nadal trwa podró¿ fotelowa ---
        if (DriverSeatInteraction.IsAnyDriverSeatActive)
        {
            if (inventory != null && inventory.currentWeaponPrefab != null)
                inventory.currentWeaponPrefab.SetActive(false);

            if (inventoryUI != null)
            {
                inventoryUI.HideWeaponUI();
                inventoryUI.HideItemUI();
            }
            HideUI();
        }
    }

    private void UseDriverSeat(InteractableItem interactableItem)
    {
        var driverSeat = interactableItem.GetComponent<DriverSeatInteraction>();
        if (driverSeat != null)
        {
            driverSeat.StartTravel();
        }
    }

    private void InteractWithObject(InteractableItem interactableItem)
    {
        if (interactableItem != null)
        {
            interactableItem.Interact();
        }
    }

    private void UseTurret(InteractableItem interactableItem)
    {
        if (turretController != null)
        {
            turretController.UseTurret();
        }
    }

    private void UseMonitor(InteractableItem interactableItem)
    {
        CameraToMonitor specificMonitor = interactableItem.GetComponent<CameraToMonitor>();
        if (specificMonitor != null)
        {
            specificMonitor.UseMonitor();
        }
    }

    private void UseMissionDefiner(InteractableItem interactableItem)
    {
        MissionDefiner missionDefiner = interactableItem.GetComponent<MissionDefiner>();
        if (missionDefiner != null)
        {
            missionDefiner.UseDefiner();
        }
    }

    private void RemoveOldestItemFromInventory(InteractableItem interactableItem)
    {
        if (treasureRefiner != null)
        {
            treasureRefiner.RemoveSelectedItemFromInventory(inventoryUI.selectedSlotIndex);
        }
    }

    private void HideUI()
    {
        if (progressCircle != null)
            progressCircle.gameObject.SetActive(false);
        if (messageText != null)
            messageText.gameObject.SetActive(false);
        if (keyText != null)
            keyText.gameObject.SetActive(false);
    }

    private void ResetInteraction()
    {
        interactionTimer = 0f;
        if (progressCircle != null)
        {
            progressCircle.fillAmount = 0f;
            progressCircle.gameObject.SetActive(false);
        }
        if (messageText != null)
            messageText.gameObject.SetActive(false);
        if (keyText != null)
            keyText.gameObject.SetActive(false);

        currentInteractableItem = null;

        // --- Blokada przywracania UI/broni gdy aktywny MissionDefiner lub DriverSeat ---
        if (MissionDefiner.IsAnyDefinerActive || DriverSeatInteraction.IsAnyDriverSeatActive)
            return;

        bool isAnyMonitorActive = false;
        var allMonitors = Object.FindObjectsByType<CameraToMonitor>(FindObjectsSortMode.None);
        foreach (var monitor in allMonitors)
        {
            if (monitor.isUsingMonitor)
            {
                isAnyMonitorActive = true;
                break;
            }
        }

        if (turretController != null && !turretController.isUsingTurret &&
            inventory != null && inventory.currentWeaponPrefab != null &&
            !isAnyMonitorActive)
        {
            inventory.currentWeaponPrefab.SetActive(true);
            inventory.enabled = true;
            inventoryUI.UpdateWeaponUI(inventory.currentWeaponPrefab.GetComponent<Gun>());
            inventoryUI.ShowWeaponUI();
            inventoryUI.ShowItemUI(inventory.items);

            Gun gunScript = inventory.currentWeaponPrefab.GetComponent<Gun>();
            if (gunScript != null)
                gunScript.EquipWeapon();
        }
    }

    public void ReactivateInventoryAndUI()
    {
        if (inventoryUI != null)
            inventoryUI.isInputBlocked = false;

        if (inventory != null)
        {
            inventory.enabled = true;
            if (inventory.currentWeaponPrefab != null)
            {
                inventory.currentWeaponPrefab.SetActive(true);
                if (inventoryUI != null)
                {
                    inventoryUI.UpdateWeaponUI(inventory.currentWeaponPrefab.GetComponent<Gun>());
                    inventoryUI.ShowWeaponUI();
                    inventoryUI.ShowItemUI(inventory.items);
                }
            }
            else
            {
                Debug.LogWarning("currentWeaponPrefab jest null w ReactivateInventoryAndUI!");
                if (inventoryUI != null)
                    inventoryUI.HideWeaponUI();
            }
        }
    }
}