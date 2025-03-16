using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionRange = 4f;
    public LayerMask interactableLayer;
    public Camera playerCamera;
    public Image progressCircle;
    public TMP_Text messageText;
    public Inventory inventory;
    public InventoryUI inventoryUI;

    private float interactionTimer = 0f;
    private InteractableItem currentInteractableItem = null;
    private float requiredHoldTime = 5f;
    private TurretController turretController; // Dodajemy referencjê do TurretController
    private TreasureRefiner treasureRefiner;

    void Start()
    {
        // Znajdujemy TurretController w scenie
        turretController = Object.FindFirstObjectByType<TurretController>();
        if (turretController == null)
        {
            Debug.LogError("Brak obiektu TurretController w scenie.");
        }
        // Znajdujemy TreasureRefiner w scenie
        treasureRefiner = Object.FindFirstObjectByType<TreasureRefiner>();
        if (treasureRefiner  == null)
        {
            Debug.LogError("Brak obiektu TurretController w scenie.");
        }
    }

    void Update()
    {
        if (playerCamera == null)
        {
            Debug.LogError("PlayerCamera is not assigned in the Inspector.");
            return;
        }

        // Sprawdzamy, czy gracz trzyma loot
        Inventory playerInventory = Object.FindFirstObjectByType<Inventory>();
        if (playerInventory != null && playerInventory.lootParent != null && playerInventory.lootParent.childCount > 0)
        {
            HideUI();
            return; // Zatrzymujemy dalsze przetwarzanie, gdy gracz trzyma loot
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
                    messageText.gameObject.SetActive(true);
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
                        if (progressCircle != null)
                        {
                            progressCircle.fillAmount = interactionTimer / requiredHoldTime;
                        }

                        if (interactionTimer >= requiredHoldTime)
                        {
                            if (currentInteractableItem.isTurret) // Sprawdzamy, czy obiekt to wie¿yczka
                            {
                                UseTurret(interactableItem); // Uruchamiamy tryb wie¿yczki
                            }
                            if (currentInteractableItem.isRefiner) // Sprawdzamy, czy obiekt to wie¿yczka
                            {
                                RemoveOldestItemFromInventory(interactableItem); // Uruchamiamy Refiner
                            }
                            else
                            {
                                InteractWithObject(currentInteractableItem);
                            }
                            interactionTimer = 0f;
                            HideUI();
                        }
                        if (inventory != null && inventory.currentWeaponPrefab != null)
                        {
                            inventory.currentWeaponPrefab.SetActive(false);
                            inventoryUI.UpdateWeaponUI(inventory.currentWeaponPrefab.GetComponent<Gun>());
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
            //Debug.Log($"{interactableItem.itemName} jest teraz w trybie wie¿yczki!");
            turretController.UseTurret(); // U¿ywamy metody z TurretController do aktywacji wie¿yczki
        }
    }
    private void RemoveOldestItemFromInventory(InteractableItem interactableItem)
    {
        if (treasureRefiner != null)
        {
            treasureRefiner.RemoveOldestItemFromInventory("Item_1"); // Przekazujemy nazwê przedmiotu
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

        currentInteractableItem = null;

        // Sprawdzamy, czy gracz nie jest w trybie wie¿yczki przed aktywowaniem broni
        if (turretController != null && !turretController.isUsingTurret && inventory != null && inventory.currentWeaponPrefab != null)
        {
            inventory.currentWeaponPrefab.SetActive(true);
            inventoryUI.UpdateWeaponUI(inventory.currentWeaponPrefab.GetComponent<Gun>());
        }
    }

}
