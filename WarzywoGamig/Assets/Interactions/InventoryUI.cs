using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel; // Panel ekwipunku
    public GameObject itemSlotPrefab; // Prefab slotu na przedmiot
    public Inventory inventory; // Odniesienie do skryptu ekwipunku
    public Dictionary<string, Sprite> itemIcons = new Dictionary<string, Sprite>(); // S�ownik przechowuj�cy ikony przedmiot�w

    void Start()
    {
        if (itemSlotPrefab == null)
        {
            Debug.LogError("Item slot prefab is missing.");
        }

        UpdateInventoryUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        }
    }

    public void UpdateInventoryUI()
    {
        if (inventory == null)
        {
            Debug.LogError("Inventory reference is missing.");
            return;
        }

        if (inventoryPanel == null)
        {
            Debug.LogError("Inventory panel is missing.");
            return;
        }

        if (itemSlotPrefab == null)
        {
            Debug.LogError("Item slot prefab is missing.");
            return;
        }

        // Zniszcz wszystkie istniej�ce sloty przedmiot�w
        foreach (Transform child in inventoryPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // Dodaj sloty dla wszystkich przedmiot�w w ekwipunku
        foreach (GameObject item in inventory.collectedItems)
        {
            if (item != null)
            {
                GameObject slot = Instantiate(itemSlotPrefab, inventoryPanel.transform);
                if (slot.transform.childCount > 0)
                {
                    Image icon = slot.transform.GetChild(0).GetComponent<Image>();
                    InteractableItem interactableItem = item.GetComponent<InteractableItem>();
                    if (interactableItem != null && itemIcons.ContainsKey(interactableItem.itemName))
                    {
                        icon.sprite = itemIcons[interactableItem.itemName];
                    }
                    else
                    {
                        Debug.LogWarning($"No icon found for {interactableItem.itemName}.");
                    }
                }
                else
                {
                    Debug.LogWarning("Slot does not have any children.");
                }
            }
            else
            {
                Debug.LogWarning("Item in collectedItems is null.");
            }
        }
    }
}