using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<GameObject> weapons = new List<GameObject>(); // Lista broni
    public List<GameObject> items = new List<GameObject>(); // Lista innych przedmiot�w
    public int maxWeapons = 3; // Maksymalna liczba broni, kt�re gracz mo�e nosi�
    public int maxItems = 5; // Maksymalna liczba innych przedmiot�w
    public LayerMask interactableLayer; // Warstwa interaktywnych przedmiot�w
    public InventoryUI inventoryUI; // Odniesienie do skryptu InventoryUI

    public Transform weaponParent; // Transform, do kt�rego b�d� przypisywane bronie jako dzieci

    [System.Serializable]
    public class WeaponPrefabEntry
    {
        public string weaponName; // Nazwa broni
        public GameObject weaponPrefab; // Prefab broni
    }

    public List<WeaponPrefabEntry> weaponPrefabsList = new List<WeaponPrefabEntry>(); // Lista, kt�r� edytujesz w inspektorze
    private Dictionary<string, GameObject> weaponPrefabs = new Dictionary<string, GameObject>(); // S�ownik prefab�w broni

    private GameObject currentWeaponPrefab; // Przechowuje aktualnie wyposa�on� bro�
    private bool isWeaponEquipped = false; // Flaga do sprawdzenia, czy bro� jest wyposa�ona

    public Vector3 weaponPositionOffset = new Vector3(0.5f, -0.3f, 1.0f); // Przesuni�cie broni wzgl�dem gracza
    public Vector3 weaponRotationOffset = new Vector3(0, 90, 0); // Rotacja broni wzgl�dem gracza


    void Start()
    {
        weapons.Clear();
        items.Clear();

        // Zbuduj s�ownik weaponPrefabs z listy
        weaponPrefabs.Clear();
        foreach (WeaponPrefabEntry entry in weaponPrefabsList)
        {
            weaponPrefabs[entry.weaponName] = entry.weaponPrefab;
        }

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
                // Sprawdzamy, czy przedmiot jest broni�, czy innym przedmiotem
                if (interactableItem.isWeapon)
                {
                    if (weapons.Count < maxWeapons)
                    {
                        weapons.Add(hit.collider.gameObject);
                        hit.collider.gameObject.SetActive(false); // Deaktywuj zebrany przedmiot

                        // Aktywuj prefab broni
                        EquipWeapon(interactableItem);

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

    void EquipWeapon(InteractableItem interactableItem)
    {
        // Sprawdzamy, czy istnieje prefab broni dla tej broni
        if (weaponPrefabs.ContainsKey(interactableItem.itemName))
        {
            if (currentWeaponPrefab != null)
            {
                Destroy(currentWeaponPrefab); // Usuwamy poprzedni� bro�, je�li by�a
            }

            // Instaluje now� bro�
            currentWeaponPrefab = Instantiate(weaponPrefabs[interactableItem.itemName], weaponParent);

            // Ustawienie pozycji i rotacji broni w stosunku do gracza
            currentWeaponPrefab.transform.localPosition = weaponPositionOffset; // Przesuni�cie lokalne
            currentWeaponPrefab.transform.localRotation = Quaternion.Euler(weaponRotationOffset); // Rotacja lokalna

            currentWeaponPrefab.SetActive(true);

            // Znajd� skrypt Gun i przypisz go do broni
            Gun gunScript = currentWeaponPrefab.GetComponent<Gun>();
            if (gunScript != null)
            {
                gunScript.enabled = true; // Aktywuj strzelanie
                gunScript.EquipWeapon(); // Aktywuj bro� do strzelania
            }

            isWeaponEquipped = true; // Bro� jest teraz wyposa�ona
        }
        else
        {
            Debug.LogWarning("Weapon prefab not found for this weapon.");
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
                    itemToDrop.SetActive(true); // Aktywuj upuszczon� bro�

                    // Ustaw pozycj� upuszczonego przedmiotu przed graczem
                    Vector3 dropPosition = new Vector3(transform.position.x, transform.position.y + 1.0f, transform.position.z);
                    itemToDrop.transform.position = dropPosition;

                    // Zniszcz prefab broni, je�li by� przypisany
                    if (currentWeaponPrefab != null)
                    {
                        Destroy(currentWeaponPrefab);
                        currentWeaponPrefab = null;
                    }

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

                    // Ustaw pozycj� upuszczonego przedmiotu przed graczem
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
            inventoryUI.UpdateInventoryUI(weapons, items); // Przekazujemy bro� i inne przedmioty
        }
        else
        {
            Debug.LogError("InventoryUI component is missing.");
        }
    }
}
