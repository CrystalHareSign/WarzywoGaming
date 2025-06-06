using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class Inventory : MonoBehaviour
{
    // Lista broni: teraz tylko nazwy broni!
    public List<string> weapons = new List<string>(); // Lista broni (nazwy)
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
    public bool isLootBeingDropped = false; // Flaga kontrolująca proces upuszczania lootu
    public Light flashlight; // Przeciągnij latarkę z Hierarchii do tego pola w Inspectorze
    public Dictionary<string, AmmoState> weaponAmmoStates = new Dictionary<string, AmmoState>();

    public Dictionary<string, GameObject> weaponPrefabs = new Dictionary<string, GameObject>();

    public Dictionary<string, GameObject> itemPrefabs = new Dictionary<string, GameObject>();

    // NOWOŚĆ: prefaby lootów
    public List<LootPrefabEntry> lootPrefabsList = new List<LootPrefabEntry>();
    public Dictionary<string, GameObject> lootPrefabs = new Dictionary<string, GameObject>();

    public GameObject currentWeaponPrefab; // Przechowuje aktualnie wyposażoną broń (instancja w ręce gracza)
    public string currentWeaponName = null; // Nowość: nazwa aktualnie wyposażonej broni

    private GameObject currentWeaponItem; // Pozostawiam, jeśli jest wykorzystywane gdzieś dalej

    public Vector3 weaponPositionOffset = new Vector3(0.5f, -0.3f, 1.0f);
    public Vector3 weaponRotationOffset = new Vector3(0, 90, 0);

    public Vector3 lootPositionOffset_1x1 = new Vector3(0f, 1f, 0f); // Ręczna pozycja lootów 1x1 względem gracza
    public Vector3 lootRotationOffset_1x1 = new Vector3(0f, 0f, 0f); // Ręczna rotacja lootów 1x1 względem gracza

    public Vector3 lootPositionOffset_2x2 = new Vector3(0f, 1.5f, 0f); // Ręczna pozycja lootów 2x2 względem gracza
    public Vector3 lootRotationOffset_2x2 = new Vector3(0f, 0f, 0f); // Ręczna rotacja lootów 2x2 względem gracza

    // Lista wszystkich obiektów, które posiadają PlaySoundOnObject
    private List<PlaySoundOnObject> playSoundObjects = new List<PlaySoundOnObject>();

    public WeaponDatabase weaponDatabase;
    public ItemDatabase itemDatabase;

    public static Inventory Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Inventory: Instance already exists, destroying duplicate. (Awake)");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        //Debug.Log("Inventory Awake: " + this.GetInstanceID());
    }


    void Start()
    {

        weaponPrefabs.Clear();
        foreach (var entry in weaponDatabase.weaponPrefabsList)
        {
            weaponPrefabs[entry.weaponName] = entry.weaponPrefab;
        }

        itemPrefabs.Clear();
        if (itemDatabase != null)
        {
            foreach (var entry in itemDatabase.itemPrefabsList)
                itemPrefabs[entry.itemName] = entry.itemPrefab;
        }

        lootPrefabs.Clear();
        foreach (var entry in lootPrefabsList)
            lootPrefabs[entry.lootName] = entry.lootPrefab;

        playSoundObjects.AddRange(Object.FindObjectsByType<PlaySoundOnObject>(FindObjectsSortMode.None));

        UpdateInventoryUI();
        //Debug.Log("Inventory Start: " + this.GetInstanceID() + " - weapons count: " + weapons.Count);
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
        if (Input.GetKeyDown(KeyCode.F))
        {
            // Sprawdź WSZYSTKIE monitory
            var monitors = Object.FindObjectsByType<CameraToMonitor>(FindObjectsSortMode.None);
            foreach (var monitor in monitors)
            {
                if (monitor.isUsingMonitor)
                    return; // Jeśli którykolwiek monitor jest aktywny, blokujemy latarkę
            }

            // Zablokuj jeśli wieżyczka aktywna
            var turrets = Object.FindObjectsByType<TurretController>(FindObjectsSortMode.None);
            foreach (var t in turrets)
            {
                if (t.isUsingTurret)
                    return;
            }
            if (flashlight.enabled)
                FlashlightOff();
            else
                FlashlightOn();
        }
    }

    public void FlashlightOn()
    {
        if (flashlight != null)
            flashlight.enabled = true;
    }

    public void FlashlightOff()
    {
        if (flashlight != null)
            flashlight.enabled = false;
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
                return;
            }

            // ❌ Jeśli gracz trzyma loot, nie może podnosić innych przedmiotów (poza bronią, ale to już blokujemy powyżej)
            if (lootParent != null && lootParent.childCount > 0 && !interactableItem.isWeapon)
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
                        // Dodajemy nazwę broni do ekwipunku!
                        if (!weapons.Contains(interactableItem.itemName))
                            weapons.Add(interactableItem.itemName);

                        hit.collider.gameObject.SetActive(false);
                        EquipWeapon(interactableItem.itemName); // Zmieniamy na przekazanie nazwy
                    }
                }
                else if (interactableItem.isLoot)
                {
                    if (loot.Count < maxLoot)
                    {
                        Vector3 previousPosition = hit.collider.gameObject.transform.position;
                        loot.Add(hit.collider.gameObject);
                        EquipLoot(hit.collider.gameObject);

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
        for (int i = 0; i < items.Count; i++)
        {
            items[i].name = "Item_" + (i + 1);
        }
    }

    void ReplaceCurrentWeapon(InteractableItem newWeapon, GameObject newWeaponItem)
    {
        Debug.Log("ReplaceCurrentWeapon: Replacing weapon with " + newWeapon.itemName);
        if (currentWeaponPrefab != null)
        {
            Destroy(currentWeaponPrefab);
        }
        // Usuwamy aktualną nazwę broni z listy
        if (currentWeaponName != null && weapons.Contains(currentWeaponName))
        {
            weapons.Remove(currentWeaponName);
        }
        // Dodajemy nową nazwę broni do listy
        if (!weapons.Contains(newWeapon.itemName))
            weapons.Add(newWeapon.itemName);

        newWeaponItem.SetActive(false);
        EquipWeapon(newWeapon.itemName);
    }

    // Wyekwipowanie broni na podstawie NAZWY z zachowaniem amunicji
    public void EquipWeapon(string weaponName)
    {
        Debug.Log($"EquipWeapon: Trying to equip weapon '{weaponName}'");

        // ZAPISZ amunicję poprzedniej broni (jeśli była)
        if (currentWeaponName != null && currentWeaponPrefab != null)
        {
            Gun oldGun = currentWeaponPrefab.GetComponent<Gun>();
            if (oldGun != null)
            {
                if (!weaponAmmoStates.ContainsKey(currentWeaponName))
                    weaponAmmoStates[currentWeaponName] = new AmmoState();
                weaponAmmoStates[currentWeaponName].currentAmmo = oldGun.currentAmmo;
                weaponAmmoStates[currentWeaponName].totalAmmo = oldGun.totalAmmo;

                oldGun.CancelReload(); // <--- TO DODAJ!
            }
        }

        // Zniszcz starą broń
        if (currentWeaponPrefab != null)
        {
            Debug.LogWarning("EquipWeapon: Destroying currentWeaponPrefab: " + currentWeaponPrefab.name + " (before new instantiation!)");
            Destroy(currentWeaponPrefab);
        }

        var prefab = weaponPrefabs[weaponName];
        if (prefab == null)
        {
            Debug.LogError($"EquipWeapon: Prefab for weapon '{weaponName}' is null! Check your WeaponDatabase in Inspector.");
            return;
        }

        Debug.Log("EquipWeapon: Instantiating prefab for " + weaponName + " to parent " + (weaponParent ? weaponParent.name : "NULL"));
        currentWeaponPrefab = Instantiate(prefab, weaponParent);
        Debug.Log("EquipWeapon: Instantiated prefab: " + currentWeaponPrefab.name);

        currentWeaponPrefab.transform.localPosition = weaponPositionOffset;
        currentWeaponPrefab.transform.localRotation = Quaternion.Euler(weaponRotationOffset);
        currentWeaponPrefab.SetActive(true);

        GameObject rootWeapon = currentWeaponPrefab.transform.root.gameObject;
        DontDestroyOnLoad(rootWeapon);

        Gun gunScript = currentWeaponPrefab.GetComponent<Gun>();
        if (gunScript != null)
        {
            Debug.Log("EquipWeapon: Gun component found, enabling and equipping.");
            // PRZYWRÓĆ amunicję z zapamiętanego stanu, jeśli istnieje
            if (weaponAmmoStates.ContainsKey(weaponName))
            {
                gunScript.currentAmmo = weaponAmmoStates[weaponName].currentAmmo;
                gunScript.totalAmmo = weaponAmmoStates[weaponName].totalAmmo;
            }
            else
            {
                // Pierwsze podniesienie tej broni – zapisz domyślne wartości
                weaponAmmoStates[weaponName] = new AmmoState
                {
                    currentAmmo = gunScript.currentAmmo,
                    totalAmmo = gunScript.totalAmmo
                };
            }

            gunScript.enabled = true;
            gunScript.EquipWeapon();
        }
        else
        {
            Debug.LogWarning("EquipWeapon: Gun component NOT found on prefab!");
        }

        currentWeaponName = weaponName;

        Debug.Log($"EquipWeapon: Equipped '{weaponName}', prefab: {currentWeaponPrefab?.name}, parent: {currentWeaponPrefab?.transform.parent?.name ?? "brak parenta"}");

        // Aktualizacja UI po zmianie broni
        UpdateInventoryUI();

        // TEST: opóźniony log sprawdzający, czy prefab nadal istnieje po 1 sekundzie
        StartCoroutine(CheckWeaponAfterDelay());
    }

    private System.Collections.IEnumerator CheckWeaponAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        Debug.LogWarning("CheckWeaponAfterDelay: currentWeaponPrefab = " + (currentWeaponPrefab ? currentWeaponPrefab.name : "NULL") +
            ", parent: " + (currentWeaponPrefab && currentWeaponPrefab.transform.parent ? currentWeaponPrefab.transform.parent.name : "NULL"));
    }

    // Zachowuję wersję dla kompatybilności, ale przekierowuję na wersję stringową
    public void EquipWeapon(InteractableItem interactableItem, GameObject weaponItem)
    {
        EquipWeapon(interactableItem.itemName);
    }

    void EquipLoot(GameObject lootItem)
    {
        if (lootItem != null)
        {
            if (loot.Contains(lootItem))
            {
                Vector3 lootPosition = lootPositionOffset_1x1;
                Vector3 lootRotation = lootRotationOffset_1x1;

                Collider lootCollider = lootItem.GetComponent<Collider>();
                if (lootCollider != null)
                {
                    if (lootCollider.bounds.size.x > 1f && lootCollider.bounds.size.z > 1f)
                    {
                        lootPosition = lootPositionOffset_2x2;
                        lootRotation = lootRotationOffset_2x2;
                    }
                }

                Transform parentTransform = lootParent != null ? lootParent : transform;

                lootItem.transform.SetParent(parentTransform);
                lootItem.transform.localPosition = lootPosition;
                lootItem.transform.localRotation = Quaternion.Euler(lootRotation);

                lootItem.SetActive(true);

                // WYŁĄCZ FIZYKĘ
                Rigidbody rb = lootItem.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                if (lootCollider != null)
                {
                    lootCollider.isTrigger = true;
                }

                loot.Remove(lootItem);
            }
        }
    }

    void DropItemFromInventory()
    {
        if (isLootBeingDropped) return;

        if (loot.Count > 0)
        {
            //DropLoot();
        }
        else if (currentWeaponName != null) // Zmiana: sprawdzamy po nazwie
        {
            DropWeapon();
        }
        else if (items.Count > 0)
        {
            DropItem();
        }
    }

    void DropItem()
    {
        if (items.Count == 0) return;

        GameObject item = items[0];
        InteractableItem interactableItem = item.GetComponent<InteractableItem>();

        if (interactableItem != null && interactableItem.canBeDropped)
        {
            items.RemoveAt(0);

            item.transform.SetParent(null);

            Vector3 dropPosition = transform.position;
            dropPosition.y = dropHeight;

            item.transform.position = dropPosition;
            item.transform.rotation = Quaternion.identity;

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
        if (currentWeaponName != null)
        {
            // Usuwamy nazwę broni z listy
            weapons.Remove(currentWeaponName);

            if (currentWeaponPrefab != null)
            {
                currentWeaponPrefab.transform.SetParent(null);

                Vector3 dropPosition = transform.position;
                dropPosition.y = dropHeight;

                currentWeaponPrefab.transform.position = dropPosition;
                currentWeaponPrefab.transform.rotation = Quaternion.identity;

                currentWeaponPrefab.SetActive(true);
                Destroy(currentWeaponPrefab);

                currentWeaponPrefab = null;
            }
            currentWeaponName = null;

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

        string objectToRemoveName = objectToRemove.name.Replace("(Clone)", "").Replace("(Clone)(Clone)", "").Trim();

        Transform foundObject = null;

        foreach (Transform child in lootParent)
        {
            string childName = child.gameObject.name.Replace("(Clone)", "").Replace("(Clone)(Clone)", "").Trim();

            if (childName == objectToRemoveName)
            {
                foundObject = child;
                break;
            }
        }

        if (foundObject != null)
        {
            Destroy(foundObject.gameObject);
        }
        else
        {
            Debug.LogWarning($"Nie znaleziono odpowiedniego obiektu w LootParent dla: {objectToRemove.name}");
        }
    }

    public void UpdateInventoryUI()
    {
        if (inventoryUI != null)
        {
            inventoryUI.UpdateInventoryUI(weapons, items, currentWeaponName);
        }
    }
    public void ClearInventory()
    {
        foreach (GameObject item in items)
        {
            Destroy(item);
        }
        items.Clear();

        UpdateInventoryUI();
    }
}

// Klasy pomocnicze zostają bez zmian
[System.Serializable]
public class ItemPrefabEntry
{
    public string itemName;
    public GameObject itemPrefab;
}

[System.Serializable]
public class LootPrefabEntry
{
    public string lootName;
    public GameObject lootPrefab;
}

[System.Serializable]
public class WeaponPrefabEntry
{
    public string weaponName; // Nazwa broni
    public GameObject weaponPrefab; // Prefab broni
}
public class AmmoState
{
    public int currentAmmo;
    public int totalAmmo;
}
