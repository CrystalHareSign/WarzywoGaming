using UnityEngine;
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

    public Vector3 lootPositionOffset_1x1 = new Vector3(0f, 1f, 0f); // Ręczna pozycja lootów 1x1 względem gracza
    public Vector3 lootRotationOffset_1x1 = new Vector3(0f, 0f, 0f); // Ręczna rotacja lootów 1x1 względem gracza

    public Vector3 lootPositionOffset_2x2 = new Vector3(0f, 1.5f, 0f); // Ręczna pozycja lootów 2x2 względem gracza
    public Vector3 lootRotationOffset_2x2 = new Vector3(0f, 0f, 0f); // Ręczna rotacja lootów 2x2 względem gracza

    // Lista wszystkich obiektów, które posiadają PlaySoundOnObject
    private List<PlaySoundOnObject> playSoundObjects = new List<PlaySoundOnObject>();

    void Start()
    {
        // Znajdź wszystkie obiekty posiadające PlaySoundOnObject i dodaj do listy
        playSoundObjects.AddRange(Object.FindObjectsOfType<PlaySoundOnObject>());

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
            if (isLootBeingDropped) return;
            DropItemFromInventory();
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
                        Vector3 previousPosition = hit.collider.gameObject.transform.position;
                        loot.Add(hit.collider.gameObject);
                        EquipLoot(hit.collider.gameObject);

                        // ✅ Ukrywamy broń, jeśli gracz podnosi loot
                        if (currentWeaponPrefab != null)
                        {
                            currentWeaponPrefab.SetActive(false);
                        }

                        if (GridManager.Instance != null)
                        {
                            PrefabSize prefabSize = hit.collider.gameObject.GetComponent<PrefabSize>();
                            GridManager.Instance.UnmarkTilesAsOccupied(previousPosition, prefabSize);
                            GridManager.Instance.AddToBuildingPrefabs(hit.collider.gameObject);
                        }

                        foreach (var playSoundOnObject in playSoundObjects)
                        {
                            if (playSoundOnObject == null) continue;

                            playSoundOnObject.PlaySound("LootPick", 1.0f, false);
                        }
                    }
                }
                else
                {
                    if (items.Count < maxItems)
                    {
                        items.Add(hit.collider.gameObject);
                        hit.collider.gameObject.SetActive(false);
                        TurretCollector turretCollector = Object.FindFirstObjectByType<TurretCollector>();
                        if (turretCollector != null)
                        {
                            turretCollector.ResetSlotForItem(hit.collider.gameObject);
                        }
                        // Odświeżenie listy, aby utrzymać kolejność chronologiczną
                        RefreshItemListChronologically();

                        foreach (var playSoundOnObject in playSoundObjects)
                        {
                            if (playSoundOnObject == null) continue;

                            playSoundOnObject.PlaySound("PickUpLiquid", 0.8f, false);
                            playSoundOnObject.PlaySound("PickUpSteam", 0.6f, false);
                        }
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
    public void RefreshItemListChronologically()
    {
        // Numerujemy przedmioty w sposób chronologiczny
        for (int i = 0; i < items.Count; i++)
        {
            // Możesz dodać dowolną logikę, jeśli przedmioty mają być numerowane w jakiś specjalny sposób
            items[i].name = "Item_" + (i + 1);
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
                // Sprawdzenie, czy obiekt jest zniszczony lub null, zanim go usuniemy
                if (currentWeaponPrefab != null && currentWeaponPrefab.gameObject != null)
                {
                    Destroy(currentWeaponPrefab);
                }
            }

            currentWeaponPrefab = Instantiate(weaponPrefabs[interactableItem.itemName], weaponParent);
            currentWeaponPrefab.transform.localPosition = weaponPositionOffset;
            currentWeaponPrefab.transform.localRotation = Quaternion.Euler(weaponRotationOffset);
            currentWeaponPrefab.SetActive(true);

            // Zapobiegamy usuwaniu broni między scenami przez przeniesienie na root
            GameObject rootWeapon = currentWeaponPrefab.transform.root.gameObject;
            DontDestroyOnLoad(rootWeapon);

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
                // Określenie, czy przedmiot jest 1x1, 2x2, czy inny
                Vector3 lootPosition = lootPositionOffset_1x1; // Domyślny offset dla 1x1
                Vector3 lootRotation = lootRotationOffset_1x1; // Domyślny rotation dla 1x1

                // Sprawdzamy wielkość przedmiotu (tutaj zakłada się, że lootItem ma collider)
                Collider lootCollider = lootItem.GetComponent<Collider>();
                if (lootCollider != null)
                {
                    // Załóżmy, że przedmioty 2x2 mają większy rozmiar (możesz to dostosować w zależności od własnych kryteriów)
                    if (lootCollider.bounds.size.x > 1f && lootCollider.bounds.size.z > 1f)
                    {
                        lootPosition = lootPositionOffset_2x2; // Przypisujemy offset dla 2x2
                        lootRotation = lootRotationOffset_2x2; // Przypisujemy rotację dla 2x2
                    }
                }

                // Ustaw przedmiot jako dziecko odpowiedniego obiektu (lootParent lub gracza)
                Transform parentTransform = lootParent != null ? lootParent : transform;

                // Ustaw przedmiot w odpowiedniej pozycji
                lootItem.transform.SetParent(parentTransform);

                // Ustaw ręczną pozycję i rotację z Inspektora
                lootItem.transform.localPosition = lootPosition;
                lootItem.transform.localRotation = Quaternion.Euler(lootRotation);

                // Aktywuj przedmiot, jeśli jest wyłączony
                lootItem.SetActive(true);

                // **Ponowne ustawienie `isTrigger = true` dla poprawnego działania kolizji**
                if (lootCollider != null)
                {
                    lootCollider.isTrigger = true;
                }

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
            //DropLoot();
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

    //void DropLoot()
    //{
    //    if (loot.Count == 0) return;

    //    GameObject lootItem = loot[0];
    //    InteractableItem interactableItem = lootItem.GetComponent<InteractableItem>(); // Pobieramy komponent InteractableItem

    //    // Sprawdzamy, czy loot może być upuszczony
    //    if (interactableItem != null && interactableItem.canBeDropped)
    //    {
    //        loot.RemoveAt(0);

    //        lootItem.transform.SetParent(null);

    //        Vector3 dropPosition = transform.position;
    //        dropPosition.y = dropHeight;

    //        Vector3 previousPosition = lootItem.transform.position;

    //        lootItem.transform.position = dropPosition;
    //        lootItem.transform.rotation = Quaternion.identity;

    //        lootItem.SetActive(true);

    //        if (GridManager.Instance != null)
    //        {
    //            GridManager.Instance.isBuildingMode = false;
    //            GridManager.Instance.RemoveFromBuildingPrefabs(lootItem);

    //            PrefabSize prefabSize = lootItem.GetComponent<PrefabSize>();
    //            GridManager.Instance.UnmarkTilesAsOccupied(previousPosition, prefabSize);
    //        }

    //        RemoveObjectFromLootParent(lootItem);

    //        // Jeśli gracz miał broń ukrytą, przywracamy ją
    //        if (currentWeaponPrefab != null)
    //        {
    //            currentWeaponPrefab.SetActive(true);
    //        }

    //        UpdateInventoryUI();
    //    }
    //    else
    //    {
    //        Debug.LogWarning("Nie możesz upuścić tego lootu, ponieważ 'canBeDropped' jest ustawione na false.");
    //    }
    //}

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

        Debug.Log($"LootParent: {lootParent.name}");
        Debug.Log($"Obiekt do usunięcia: {objectToRemove.name} (ID: {objectToRemove.GetInstanceID()})");

        string objectToRemoveName = objectToRemove.name.Replace("(Clone)", "").Replace("(Clone)(Clone)", "").Trim();

        Transform foundObject = null;

        foreach (Transform child in lootParent)
        {
            string childName = child.gameObject.name.Replace("(Clone)", "").Replace("(Clone)(Clone)", "").Trim();

            Debug.Log($"Porównuję: {childName} z {objectToRemoveName}");

            if (childName == objectToRemoveName)
            {
                foundObject = child;
                break;
            }
        }

        if (foundObject != null)
        {
            Debug.Log($"Usuwam obiekt z LootParent: {foundObject.name}");
            Destroy(foundObject.gameObject);
        }
        else
        {
            Debug.LogWarning($"Nie znaleziono odpowiedniego obiektu w LootParent dla: {objectToRemove.name}");
        }
    }

    private void UpdateInventoryUI()
    {
        if (inventoryUI != null)
        {
            inventoryUI.UpdateInventoryUI(weapons, items);
        }
    }
    public void ClearInventory()
    {
        // Usuwamy inne przedmioty
        foreach (GameObject item in items)
        {
            Destroy(item);
        }
        items.Clear();

        // Zaktualizuj UI
        UpdateInventoryUI();
    }

}