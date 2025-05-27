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

    private float interactionTimer = 0f;
    private InteractableItem currentInteractableItem = null;
    private float requiredHoldTime = 5f;
    private TurretController turretController;
    private TreasureRefiner treasureRefiner;
    private CameraToMonitor cameraToMonitor;
    private bool hasRefinerBeenUsed = false;

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
        turretController = Object.FindFirstObjectByType<TurretController>();
        if (turretController == null)
        {
            Debug.LogError("Brak obiektu TurretController w scenie.");
        }

        treasureRefiner = Object.FindFirstObjectByType<TreasureRefiner>();
        if (treasureRefiner == null)
        {
            Debug.LogError("Brak obiektu TreasureRefiner w scenie.");
        }

        cameraToMonitor = Object.FindFirstObjectByType<CameraToMonitor>();
        if (cameraToMonitor == null)
        {
            Debug.LogError("Brak obiektu CameraToMonitor w scenie.");
        }

        if (progressCircle != null)
        {
            progressCircle.gameObject.SetActive(false);
            progressCircle.fillAmount = 0f;
        }

        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }

        if (keyText != null)
        {
            keyText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (playerCamera == null)
        {
            Debug.LogError("PlayerCamera is not assigned in the Inspector.");
            return;
        }

        Inventory playerInventory = Object.FindFirstObjectByType<Inventory>();
        if (playerInventory != null && playerInventory.lootParent != null && playerInventory.lootParent.childCount > 0)
        {
            HideUI();
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, interactionRange, interactableLayer))
        {
            InteractableItem interactableItem = hit.collider.GetComponent<InteractableItem>();
            if (interactableItem != null && !interactableItem.hoverMessage.isInteracted)
            {
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

                        // Dezaktywacja broni tylko przy interakcjach wymagaj¹cych przytrzymania
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

                                // DEAKTYWUJ BROÑ przy wie¿yczce
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

                            if (currentInteractableItem.isMonitor)
                            {
                                UseMonitor(interactableItem);

                                // DEZAKTYWUJ BROÑ:
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

                            if (currentInteractableItem.isRefiner && !hasRefinerBeenUsed)
                            {
                                RemoveOldestItemFromInventory(interactableItem);
                                inventory.RefreshItemListChronologically();
                                hasRefinerBeenUsed = true;
                            }
                            else if (!currentInteractableItem.isTurret && !currentInteractableItem.isMonitor && !currentInteractableItem.isRefiner)
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        progressCircle = GameObject.Find("ProgressCircle")?.GetComponent<Image>();
        messageText = GameObject.Find("MessageText")?.GetComponent<TMP_Text>();

        if (progressCircle != null)
        {
            progressCircle.gameObject.SetActive(false);
            progressCircle.fillAmount = 0f;
        }

        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
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

    private void RemoveOldestItemFromInventory(InteractableItem interactableItem)
    {
        if (treasureRefiner != null)
        {
            treasureRefiner.RemoveSelectedItemFromInventory(inventoryUI.selectedItemIndex);
        }
    }

    private void HideUI()
    {
        if (progressCircle != null)
        {
            progressCircle.gameObject.SetActive(false);
        }

        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }

        if (keyText != null)
        {
            keyText.gameObject.SetActive(false);
        }
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
        {
            messageText.gameObject.SetActive(false);
        }

        if (keyText != null)
        {
            keyText.gameObject.SetActive(false);
        }

        currentInteractableItem = null;

        // SprawdŸ czy którykolwiek monitor jest aktywny
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

            Gun gunScript = inventory.currentWeaponPrefab.GetComponent<Gun>();
            if (gunScript != null)
                gunScript.EquipWeapon();
        }
    }
    public void ReactivateInventoryAndUI()
    {
        if (inventory != null)
        {
            inventory.enabled = true;
            if (inventory.currentWeaponPrefab != null)
            {
                inventory.currentWeaponPrefab.SetActive(true);
                if (inventoryUI != null)
                {
                    inventoryUI.ShowWeaponUI();
                    inventoryUI.UpdateWeaponUI(inventory.currentWeaponPrefab.GetComponent<Gun>());
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