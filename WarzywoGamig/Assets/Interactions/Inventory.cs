using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<GameObject> weapons = new List<GameObject>(); // Lista broni
    public List<GameObject> items = new List<GameObject>(); // Lista innych przedmiotów
    public int maxWeapons = 3; // Maksymalna liczba broni, które gracz mo¿e nosiæ
    public int maxItems = 5; // Maksymalna liczba innych przedmiotów
    public LayerMask interactableLayer; // Warstwa interaktywnych przedmiotów
    public InventoryUI inventoryUI; // Odniesienie do skryptu InventoryUI

    void Start()
    {
        weapons.Clear();
        items.Clear();
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
            if (interactableItem != null && interactableItem.canBePickedUp)
            {
                // Sprawdzamy, czy przedmiot jest broni¹, czy innym przedmiotem
                if (interactableItem.isWeapon)
                {
                    if (weapons.Count < maxWeapons)
                    {
                        weapons.Add(hit.collider.gameObject);
                        hit.collider.gameObject.SetActive(false); // Deaktywuj zebrany przedmiot
                        Debug.Log($"Collected weapon: {interactableItem.itemName}");
                        UpdateInventoryUI();
                    }
                    else
                    {
                        Debug.LogWarning("Cannot carry more weapons.");
                    }
                }
                else
                {
                    if (items.Count < maxItems)
                    {
                        items.Add(hit.collider.gameObject);
                        hit.collider.gameObject.SetActive(false); // Deaktywuj zebrany przedmiot
                        Debug.Log($"Collected item: {interactableItem.itemName}");
                        UpdateInventoryUI();
                    }
                    else
                    {
                        Debug.LogWarning("Cannot carry more items.");
                    }
                }
            }
            else
            {
                Debug.LogWarning("InteractableItem is null, cannot be picked up.");
            }
        }
        else
        {
            Debug.LogWarning("No interactable item hit.");
        }
    }

    void DropItem()
    {
        if (weapons.Count > 0)
        {
            GameObject itemToDrop = weapons[weapons.Count - 1];
            if (itemToDrop != null)
            {
                InteractableItem interactableItem = itemToDrop.GetComponent<InteractableItem>();
                if (interactableItem != null && interactableItem.canBeDropped)
                {
                    weapons.RemoveAt(weapons.Count - 1);
                    itemToDrop.SetActive(true); // Aktywuj upuszczon¹ broñ

                    // Ustaw pozycjê upuszczonego przedmiotu przed graczem
                    Vector3 dropPosition = new Vector3(transform.position.x, transform.position.y + 1.0f, transform.position.z);
                    itemToDrop.transform.position = dropPosition;

                    Debug.Log($"Dropped weapon: {itemToDrop.name}");
                    UpdateInventoryUI();
                }
            }
        }
        else if (items.Count > 0)
        {
            GameObject itemToDrop = items[items.Count - 1];
            if (itemToDrop != null)
            {
                InteractableItem interactableItem = itemToDrop.GetComponent<InteractableItem>();
                if (interactableItem != null && interactableItem.canBeDropped)
                {
                    items.RemoveAt(items.Count - 1);
                    itemToDrop.SetActive(true); // Aktywuj upuszczony przedmiot

                    // Ustaw pozycjê upuszczonego przedmiotu przed graczem
                    Vector3 dropPosition = new Vector3(transform.position.x, transform.position.y + 1.0f, transform.position.z);
                    itemToDrop.transform.position = dropPosition;

                    Debug.Log($"Dropped item: {itemToDrop.name}");
                    UpdateInventoryUI();
                }
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
            inventoryUI.UpdateInventoryUI(weapons, items); // Przekazujemy broñ i inne przedmioty
        }
        else
        {
            Debug.LogError("InventoryUI component is missing.");
        }
    }
}
