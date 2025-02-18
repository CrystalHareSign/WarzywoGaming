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

    public Transform weaponParent; // Transform, do którego bêd¹ przypisywane bronie jako dzieci

    [System.Serializable]
    public class WeaponPrefabEntry
    {
        public string weaponName; // Nazwa broni
        public GameObject weaponPrefab; // Prefab broni
    }

    public List<WeaponPrefabEntry> weaponPrefabsList = new List<WeaponPrefabEntry>(); // Lista, któr¹ edytujesz w inspektorze
    private Dictionary<string, GameObject> weaponPrefabs = new Dictionary<string, GameObject>(); // S³ownik prefabów broni

    private GameObject currentWeaponPrefab; // Przechowuje aktualnie wyposa¿on¹ broñ
    private bool isWeaponEquipped = false; // Flaga do sprawdzenia, czy broñ jest wyposa¿ona

    public Vector3 weaponPositionOffset = new Vector3(0.5f, -0.3f, 1.0f); // Przesuniêcie broni wzglêdem gracza
    public Vector3 weaponRotationOffset = new Vector3(0, 90, 0); // Rotacja broni wzglêdem gracza


    void Start()
    {
        weapons.Clear();
        items.Clear();

        // Zbuduj s³ownik weaponPrefabs z listy
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
                // Sprawdzamy, czy przedmiot jest broni¹, czy innym przedmiotem
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
                Destroy(currentWeaponPrefab); // Usuwamy poprzedni¹ broñ, jeœli by³a
            }

            // Instaluje now¹ broñ
            currentWeaponPrefab = Instantiate(weaponPrefabs[interactableItem.itemName], weaponParent);

            // Ustawienie pozycji i rotacji broni w stosunku do gracza
            currentWeaponPrefab.transform.localPosition = weaponPositionOffset; // Przesuniêcie lokalne
            currentWeaponPrefab.transform.localRotation = Quaternion.Euler(weaponRotationOffset); // Rotacja lokalna

            currentWeaponPrefab.SetActive(true);

            // ZnajdŸ skrypt Gun i przypisz go do broni
            Gun gunScript = currentWeaponPrefab.GetComponent<Gun>();
            if (gunScript != null)
            {
                gunScript.enabled = true; // Aktywuj strzelanie
                gunScript.EquipWeapon(); // Aktywuj broñ do strzelania
            }

            isWeaponEquipped = true; // Broñ jest teraz wyposa¿ona
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
                    itemToDrop.SetActive(true); // Aktywuj upuszczon¹ broñ

                    // Ustaw pozycjê upuszczonego przedmiotu przed graczem
                    Vector3 dropPosition = new Vector3(transform.position.x, transform.position.y + 1.0f, transform.position.z);
                    itemToDrop.transform.position = dropPosition;

                    // Zniszcz prefab broni, jeœli by³ przypisany
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
