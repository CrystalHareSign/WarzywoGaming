using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class Inventory : MonoBehaviour
{
    public List<GameObject> weapons = new List<GameObject>(); // Lista broni
    public List<GameObject> items = new List<GameObject>(); // Lista innych przedmiot�w
    public List<GameObject> loot = new List<GameObject>(); // Lista loot�w
    public int maxWeapons = 3; // Maksymalna liczba broni, kt�re gracz mo�e nosi�
    public int maxItems = 5; // Maksymalna liczba innych przedmiot�w
    public int maxLoot = 5; // Maksymalna liczba przedmiot�w loot
    public LayerMask interactableLayer; // Warstwa interaktywnych przedmiot�w
    public InventoryUI inventoryUI; // Odniesienie do skryptu InventoryUI

    public Transform weaponParent; // Transform, do kt�rego b�d� przypisywane bronie jako dzieci
    public Transform lootParent; // Transform, do kt�rego b�d� przypisane lootowe przedmioty


    [System.Serializable]
    public class WeaponPrefabEntry
    {
        public string weaponName; // Nazwa broni
        public GameObject weaponPrefab; // Prefab broni
    }

    public List<WeaponPrefabEntry> weaponPrefabsList = new List<WeaponPrefabEntry>();
    private Dictionary<string, GameObject> weaponPrefabs = new Dictionary<string, GameObject>();

    private GameObject currentWeaponPrefab; // Przechowuje aktualnie wyposa�on� bro�
    private GameObject currentWeaponItem; // Przechowuje obiekt aktualnej broni w ekwipunku

    public Vector3 weaponPositionOffset = new Vector3(0.5f, -0.3f, 1.0f);
    public Vector3 weaponRotationOffset = new Vector3(0, 90, 0);
    public Vector3 lootPositionOffset = new Vector3(0f, 1f, 0f); // R�czna pozycja loot�w wzgl�dem gracza
    public Vector3 lootRotationOffset = new Vector3(0f, 0f, 0f); // R�czna rotacja loot�w wzgl�dem gracza

    void Start()
    {
        weapons.Clear();
        items.Clear();
        loot.Clear();

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

            if (interactableItem == null)
            {
                return;
            }

            if (interactableItem.canBePickedUp)
            {
                if (interactableItem.isWeapon)
                {
                    if (weapons.Count >= maxWeapons)
                    {
                        ReplaceCurrentWeapon(interactableItem, hit.collider.gameObject);
                    }
                    else
                    {
                        weapons.Add(hit.collider.gameObject);
                        hit.collider.gameObject.SetActive(false);
                        EquipWeapon(interactableItem, hit.collider.gameObject);
                    }
                }
                else
                {
                    if (items.Count < maxItems)
                    {
                        items.Add(hit.collider.gameObject);
                        hit.collider.gameObject.SetActive(false);
                    }
                }

                // Dodanie do loot i przypisanie do budowy w GridManager
                if (interactableItem.isLoot)
                {
                    if (loot.Count < maxLoot)
                    {
                        loot.Add(hit.collider.gameObject);
                        EquipLoot(hit.collider.gameObject);  // Wywo�anie EquipLoot po dodaniu do loot
                    }
                    if (GridManager.Instance != null)
                    {
                        GridManager.Instance.AddToBuildingPrefabs(hit.collider.gameObject);
                    }
                }

                UpdateInventoryUI();
            }
        }
    }


    void ReplaceCurrentWeapon(InteractableItem newWeapon, GameObject newWeaponItem)
    {
        if (currentWeaponPrefab != null)
        {
            Destroy(currentWeaponPrefab);
        }
        if (currentWeaponItem != null)
        {
            weapons.Remove(currentWeaponItem);
        }
        weapons.Add(newWeaponItem);
        newWeaponItem.SetActive(false);
        EquipWeapon(newWeapon, newWeaponItem);
    }

    void EquipWeapon(InteractableItem interactableItem, GameObject weaponItem)
    {
        if (weaponPrefabs.ContainsKey(interactableItem.itemName))
        {
            if (currentWeaponPrefab != null)
            {
                Destroy(currentWeaponPrefab);
            }

            currentWeaponPrefab = Instantiate(weaponPrefabs[interactableItem.itemName], weaponParent);
            currentWeaponPrefab.transform.localPosition = weaponPositionOffset;
            currentWeaponPrefab.transform.localRotation = Quaternion.Euler(weaponRotationOffset);
            currentWeaponPrefab.SetActive(true);

            Gun gunScript = currentWeaponPrefab.GetComponent<Gun>();
            if (gunScript != null)
            {
                gunScript.enabled = true;
                gunScript.EquipWeapon();
            }

            currentWeaponItem = weaponItem;
        }
    }
    void EquipLoot(GameObject lootItem)
    {
        if (lootItem != null)
        {
            // Sprawd�, czy przedmiot jest lootem
            if (loot.Contains(lootItem))
            {
                // Je�li lootParent jest ustawiony, u�yj go, je�li nie, przypnij loot do gracza
                Transform parentTransform = lootParent != null ? lootParent : transform;

                // Ustaw przedmiot jako dziecko odpowiedniego obiektu (lootParent lub gracza)
                lootItem.transform.SetParent(parentTransform);

                // Ustaw r�czn� pozycj� i rotacj� z Inspektora
                lootItem.transform.localPosition = lootPositionOffset;  // Przyk�ad: ustawienie pozycji
                lootItem.transform.localRotation = Quaternion.Euler(lootRotationOffset);  // Przyk�ad: ustawienie rotacji

                // Aktywuj przedmiot, je�li jest wy��czony
                lootItem.SetActive(true);
            }
        }
    }

    void DropItem()
    {
        if (currentWeaponItem != null)
        {
            weapons.Remove(currentWeaponItem);
            currentWeaponItem.SetActive(true);
            currentWeaponItem.transform.position = transform.position + Vector3.up;
            Destroy(currentWeaponPrefab);
            currentWeaponPrefab = null;
            currentWeaponItem = null;
            UpdateInventoryUI();
        }
    }

    public void RemoveItem(GameObject item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
        }
        if (loot.Contains(item))
        {
            loot.Remove(item);
        }
    }
    public void RemoveObjectFromLootParent(GameObject objectToRemove)
    {
        if (lootParent == null || objectToRemove == null)
        {
            Debug.LogWarning("LootParent nie jest przypisany lub obiekt jest nullem.");
            return;
        }

        Transform foundObject = null;

        // Przeszukujemy dzieci LootParent, ignoruj�c nazw� i skupiaj�c si� na prefabriku
        foreach (Transform child in lootParent)
        {
            if (PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject) ==
                PrefabUtility.GetCorrespondingObjectFromSource(objectToRemove))
            {
                foundObject = child;
                break;
            }
        }

        if (foundObject != null)
        {
            Debug.Log("Usuwam obiekt z LootParent: " + foundObject.name);
            Destroy(foundObject.gameObject);
        }
        else
        {
            Debug.LogWarning("Nie znaleziono odpowiedniego obiektu w LootParent dla: " + objectToRemove.name);
        }
    }

    private void UpdateInventoryUI()
    {
        if (inventoryUI != null)
        {
            inventoryUI.UpdateInventoryUI(weapons, items);
        }
    }
}
