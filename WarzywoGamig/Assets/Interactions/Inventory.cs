﻿using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class Inventory : MonoBehaviour
{
    public List<GameObject> weapons = new List<GameObject>(); // Lista broni
    public List<GameObject> items = new List<GameObject>(); // Lista innych przedmiotów
    private List<GameObject> loot = new List<GameObject>(); // Lista lootów
    public int maxWeapons = 3; // Maksymalna liczba broni, które gracz może nosić
    public int maxItems = 5; // Maksymalna liczba innych przedmiotów
    public int maxLoot = 5; // Maksymalna liczba przedmiotów loot
    public float dropHeight = 1f; // Wysokość, na jakiej loot ma upaść
    public LayerMask interactableLayer; // Warstwa interaktywnych przedmiotów
    public InventoryUI inventoryUI; // Odniesienie do skryptu InventoryUI

    public Transform weaponParent; // Transform, do którego będą przypisywane bronie jako dzieci
    public Transform lootParent; // Transform, do którego będą przypisane lootowe przedmioty
    public bool isLootBeingDropped = false; // Flaga kontrolująca proces upuszczania lootu


    [System.Serializable]
    public class WeaponPrefabEntry
    {
        public string weaponName; // Nazwa broni
        public GameObject weaponPrefab; // Prefab broni
    }

    public List<WeaponPrefabEntry> weaponPrefabsList = new List<WeaponPrefabEntry>();
    private Dictionary<string, GameObject> weaponPrefabs = new Dictionary<string, GameObject>();

    public GameObject currentWeaponPrefab; // Przechowuje aktualnie wyposażoną broń
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
            DropItemFromInventory();
        }
    }

    void CollectItem()
    {
        // ❌ Jeśli gracz trzyma loot, nie może wchodzić w interakcje z innymi przedmiotami
        if (lootParent != null && lootParent.childCount > 0)
        {
            Debug.Log("Nie możesz podnosić przedmiotów, gdy trzymasz loot.");
            return;
        }

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
                if (currentWeaponPrefab != null)
                {
                    Gun gunScript = currentWeaponPrefab.GetComponent<Gun>();
                    if (gunScript != null)
                    {
                        inventoryUI.UpdateWeaponUI(gunScript);
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

    void DropItemFromInventory()
    {
        // Jeśli loot jest w trakcie upuszczania, nie rób nic
        if (isLootBeingDropped) return;

        if (loot.Count > 0) // Jeśli mamy loot do upuszczenia
        {
            DropLoot();
        }
        else if (currentWeaponItem != null) // Jeśli mamy broń do upuszczenia
        {
            DropWeapon();
        }
        else if (items.Count > 0) // Jeśli mamy przedmioty do upuszczenia
        {
            DropItem();
        }
    }

    void DropItem()
    {
        if (items.Count == 0) return;

        GameObject item = items[0];
        InteractableItem interactableItem = item.GetComponent<InteractableItem>(); // Pobieramy komponent InteractableItem

        // Sprawdzamy, czy przedmiot może być upuszczony
        if (interactableItem != null && interactableItem.canBeDropped)
        {
            items.RemoveAt(0);

            item.transform.SetParent(null);

            // Wyznaczamy pozycję upuszczenia przy zadanej wysokości
            Vector3 dropPosition = transform.position; // Pozycja gracza (można dostosować do innego obiektu)
            dropPosition.y = dropHeight; // Ustawiamy wysokość upuszczenia na wartość dropHeight

            item.transform.position = dropPosition; // Ustawiamy pozycję przedmiotu
            item.transform.rotation = Quaternion.identity; // Reset rotacji

            item.SetActive(true);

            UpdateInventoryUI();
        }
        else
        {
            Debug.LogWarning("Nie możesz upuścić tego przedmiotu, ponieważ 'canBeDropped' jest ustawione na false.");
        }
    }

    void DropWeapon()
    {
        if (currentWeaponItem != null) // Jeśli to broń
        {
            InteractableItem interactableItem = currentWeaponItem.GetComponent<InteractableItem>(); // Pobieramy komponent InteractableItem

            // Sprawdzamy, czy broń może być upuszczona
            if (interactableItem != null && interactableItem.canBeDropped)
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
            else
            {
                Debug.LogWarning("Nie możesz upuścić tej broni, ponieważ 'canBeDropped' jest ustawione na false.");
            }
        }
    }

    void DropLoot()
    {
        if (loot.Count == 0) return;

        GameObject lootItem = loot[0];
        InteractableItem interactableItem = lootItem.GetComponent<InteractableItem>(); // Pobieramy komponent InteractableItem

        // Sprawdzamy, czy loot może być upuszczony
        if (interactableItem != null && interactableItem.canBeDropped)
        {
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
        else
        {
            Debug.LogWarning("Nie możesz upuścić tego lootu, ponieważ 'canBeDropped' jest ustawione na false.");
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
