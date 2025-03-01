using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class Inventory : MonoBehaviour
{
    public List<GameObject> weapons = new List<GameObject>(); // Lista broni
    public List<GameObject> items = new List<GameObject>(); // Lista innych przedmiotów
    public List<GameObject> loot = new List<GameObject>(); // Lista lootów
    public int maxWeapons = 3; // Maksymalna liczba broni, które gracz może nosić
    public int maxItems = 5; // Maksymalna liczba innych przedmiotów
    public int maxLoot = 5; // Maksymalna liczba przedmiotów loot
    public float dropHeight = 1f; // Wysokość, na jakiej loot ma upaść
    public LayerMask interactableLayer; // Warstwa interaktywnych przedmiotów
    public InventoryUI inventoryUI; // Odniesienie do skryptu InventoryUI

    public Transform weaponParent; // Transform, do którego będą przypisywane bronie jako dzieci
    public Transform lootParent; // Transform, do którego będą przypisane lootowe przedmioty


    [System.Serializable]
    public class WeaponPrefabEntry
    {
        public string weaponName; // Nazwa broni
        public GameObject weaponPrefab; // Prefab broni
    }

    public List<WeaponPrefabEntry> weaponPrefabsList = new List<WeaponPrefabEntry>();
    private Dictionary<string, GameObject> weaponPrefabs = new Dictionary<string, GameObject>();

    private GameObject currentWeaponPrefab; // Przechowuje aktualnie wyposażoną broń
    private GameObject currentWeaponItem; // Przechowuje obiekt aktualnej broni w ekwipunku

    public Vector3 weaponPositionOffset = new Vector3(0.5f, -0.3f, 1.0f);
    public Vector3 weaponRotationOffset = new Vector3(0, 90, 0);
    public Vector3 lootPositionOffset = new Vector3(0f, 1f, 0f); // Ręczna pozycja lootów względem gracza
    public Vector3 lootRotationOffset = new Vector3(0f, 0f, 0f); // Ręczna rotacja lootów względem gracza

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

        // ❌ Jeśli gracz trzyma loot, nie może podnosić broni
        if (lootParent != null && lootParent.childCount > 0 && interactableItem.isWeapon)
        {
            Debug.Log("Nie możesz podnieść broni, gdy trzymasz loot.");
            return;
        }

        // ❌ Jeśli gracz trzyma loot, nie może podnosić innych przedmiotów (poza bronią, ale to już blokujemy powyżej)
        if (lootParent != null && lootParent.childCount > 0 && !interactableItem.isWeapon)
        {
            Debug.Log("Nie możesz podnieść przedmiotu, ponieważ trzymasz loot.");
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
            else if (interactableItem.isLoot)
            {
                if (loot.Count < maxLoot)
                {
                    loot.Add(hit.collider.gameObject);
                    EquipLoot(hit.collider.gameObject);

                    // ✅ Ukrywamy broń, jeśli gracz podnosi loot
                    if (currentWeaponPrefab != null)
                    {
                        currentWeaponPrefab.SetActive(false);
                    }
                }

                if (GridManager.Instance != null)
                {
                    GridManager.Instance.AddToBuildingPrefabs(hit.collider.gameObject);
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
            // Sprawdź, czy przedmiot jest lootem
            if (loot.Contains(lootItem))
            {
                // Jeśli lootParent jest ustawiony, użyj go, jeśli nie, przypnij loot do gracza
                Transform parentTransform = lootParent != null ? lootParent : transform;

                // Ustaw przedmiot jako dziecko odpowiedniego obiektu (lootParent lub gracza)
                lootItem.transform.SetParent(parentTransform);

                // Ustaw ręczną pozycję i rotację z Inspektora
                lootItem.transform.localPosition = lootPositionOffset;
                lootItem.transform.localRotation = Quaternion.Euler(lootRotationOffset);

                // Aktywuj przedmiot, jeśli jest wyłączony
                lootItem.SetActive(true);

                // Usuń przedmiot z listy dostępnych lootów, aby nie można go było ponownie podnieść
                loot.Remove(lootItem);
            }
        }
    }

    void DropItem()
    {
        if (currentWeaponItem != null) // Jeśli to broń
        {
            DropWeapon();
        }
        else if (loot.Count > 0) // Jeśli mamy loot do upuszczenia
        {
            DropLoot();
        }
    }

    void DropWeapon()
    {
        if (currentWeaponItem != null) // Jeśli to broń
        {
            weapons.Remove(currentWeaponItem);
            currentWeaponItem.transform.SetParent(null);

            // Wyznaczamy pozycję upuszczenia przy zadanej wysokości
            Vector3 dropPosition = transform.position; // Pozycja gracza (można dostosować do innego obiektu)
            dropPosition.y = dropHeight; // Ustawiamy wysokość upuszczenia na wartość dropHeight

            currentWeaponItem.transform.position = dropPosition; // Ustawiamy pozycję broni
            currentWeaponItem.transform.rotation = Quaternion.identity; // Reset rotacji

            currentWeaponItem.SetActive(true);
            Destroy(currentWeaponPrefab);

            currentWeaponPrefab = null;
            currentWeaponItem = null;

            UpdateInventoryUI();
        }
    }

    void DropLoot()
    {
        if (loot.Count == 0) return;

        GameObject lootItem = loot[0];
        loot.RemoveAt(0);

        lootItem.transform.SetParent(null);

        Vector3 dropPosition = transform.position;
        dropPosition.y = dropHeight;

        lootItem.transform.position = dropPosition;
        lootItem.transform.rotation = Quaternion.identity;

        lootItem.SetActive(true);

        if (GridManager.Instance != null)
        {
            GridManager.Instance.isBuildingMode = false;
            GridManager.Instance.RemoveFromBuildingPrefabs(lootItem);
        }

        RemoveObjectFromLootParent(lootItem);

        // Jeśli gracz miał broń ukrytą, przywracamy ją
        if (currentWeaponPrefab != null)
        {
            currentWeaponPrefab.SetActive(true);
        }

        UpdateInventoryUI();
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

        // Przeszukujemy dzieci LootParent, ignorując nazwę i skupiając się na prefabriku
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
            Destroy(foundObject.gameObject); // Usuwamy obiekt z LootParent
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
