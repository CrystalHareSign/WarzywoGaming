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

    public List<WeaponPrefabEntry> weaponPrefabsList = new List<WeaponPrefabEntry>();
    private Dictionary<string, GameObject> weaponPrefabs = new Dictionary<string, GameObject>();

    private GameObject currentWeaponPrefab; // Przechowuje aktualnie wyposa¿on¹ broñ
    private GameObject currentWeaponItem; // Przechowuje obiekt aktualnej broni w ekwipunku

    public Vector3 weaponPositionOffset = new Vector3(0.5f, -0.3f, 1.0f);
    public Vector3 weaponRotationOffset = new Vector3(0, 90, 0);

    void Start()
    {
        weapons.Clear();
        items.Clear();

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
                    UpdateInventoryUI();
                }
                else
                {
                    if (items.Count < maxItems)
                    {
                        items.Add(hit.collider.gameObject);
                        hit.collider.gameObject.SetActive(false);
                        UpdateInventoryUI();
                    }
                }
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

    void DropItem()
    {
        if (currentWeaponItem != null)
        {
            weapons.Remove(currentWeaponItem);
            currentWeaponItem.SetActive(true);
            currentWeaponItem.transform.position = new Vector3(transform.position.x, transform.position.y + 1.0f, transform.position.z);
            Destroy(currentWeaponPrefab);
            currentWeaponPrefab = null;
            currentWeaponItem = null;
            UpdateInventoryUI();
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
