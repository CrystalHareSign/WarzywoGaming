using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<GameObject> collectedItems = new List<GameObject>(); // Lista zebranych przedmiotów
    public LayerMask interactableLayer; // Warstwa interaktywnych przedmiotów
    public float dropDistance = 2.0f; // Odleg³oœæ, na jak¹ przedmiot jest upuszczany przed graczem
    public float dropHeight = 1.0f; // Wysokoœæ, na jak¹ przedmiot jest upuszczany nad pod³og¹
    public InventoryUI inventoryUI; // Odniesienie do skryptu InventoryUI

    void Start()
    {
        // Upewnij siê, ¿e lista collectedItems jest pusta na starcie
        collectedItems.Clear();
        UpdateInventoryUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            CollectItem();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            DropItem();
        }
    }

    void CollectItem()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, interactableLayer))
        {
            InteractableItem interactableItem = hit.collider.GetComponent<InteractableItem>();
            if (interactableItem != null && interactableItem.canBePickedUp && !collectedItems.Contains(hit.collider.gameObject))
            {
                collectedItems.Add(hit.collider.gameObject);
                hit.collider.gameObject.SetActive(false); // Deaktywuj zebrany przedmiot zamiast go niszczyæ
                Debug.Log($"Collected {interactableItem.itemName}.");
                UpdateInventoryUI();
            }
            else
            {
                Debug.LogWarning("InteractableItem is null, cannot be picked up, or already collected.");
            }
        }
        else
        {
            Debug.LogWarning("No interactable item hit.");
        }
    }

    void DropItem()
    {
        if (collectedItems.Count > 0)
        {
            GameObject itemToDrop = collectedItems[collectedItems.Count - 1];
            if (itemToDrop != null)
            {
                InteractableItem interactableItem = itemToDrop.GetComponent<InteractableItem>();
                if (interactableItem != null && interactableItem.canBeDropped)
                {
                    collectedItems.RemoveAt(collectedItems.Count - 1);
                    itemToDrop.SetActive(true); // Aktywuj upuszczony przedmiot

                    // Ustaw pozycjê upuszczonego przedmiotu przed graczem na wysokoœci wy¿szej ni¿ pod³oga
                    Vector3 dropPosition = new Vector3(transform.position.x, transform.position.y + dropHeight, transform.position.z);
                    itemToDrop.transform.position = dropPosition;

                    Debug.Log($"Dropped {itemToDrop.name}.");
                    UpdateInventoryUI();
                }
                else
                {
                    Debug.LogWarning($"Cannot drop {itemToDrop.name}, it cannot be dropped.");
                }
            }
            else
            {
                Debug.LogWarning("Item to drop is null.");
            }
        }
        else
        {
            Debug.LogWarning("No items to drop.");
        }
    }

    private void UpdateInventoryUI()
    {
        if (inventoryUI != null)
        {
            inventoryUI.UpdateInventoryUI(); // Aktualizuj UI ekwipunku
        }
        else
        {
            Debug.LogError("InventoryUI component is missing.");
        }
    }
}