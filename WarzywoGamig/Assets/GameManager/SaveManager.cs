using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance; // Singleton

    public float playerCurrency = 0f;
    public DateTime lastSaveTime;

    private int currentSlotIndex = -1;
    public int debugSlotIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (currentSlotIndex == -1)
        {
            currentSlotIndex = 1;
        }
    }

    private string GetSlotFilePath(int slotIndex)
    {
        return Application.persistentDataPath + $"/playerData_slot{slotIndex}.json";
    }

    public void SetCurrentSlot(int slotIndex)
    {
        currentSlotIndex = slotIndex;
    }

    public void SavePlayerData()
    {
        if (currentSlotIndex == -1)
        {
            Debug.LogWarning("Nie ustawiono slotu zapisu!");
            return;
        }

        string path = GetSlotFilePath(currentSlotIndex);

        try
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("Nie znaleziono gracza.");
                return;
            }

            Vector3 playerPosition = player.transform.position;
            Quaternion playerRotation = player.transform.rotation;

            PlayerData data = new PlayerData
            {
                playerCurrency = this.playerCurrency,
                playerPosition = playerPosition,
                playerRotation = playerRotation,
                sceneName = SceneManager.GetActiveScene().name,
                lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                weapons = new List<string>(),
                itemNames = new List<string>(),
                lootNames = new List<string>(),
                weaponSaveDatas = new List<WeaponSaveData>(),
                itemSaveDatas = new List<ItemSaveData>(),
                collectors = new List<TurretCollectorSaveData>() // <- DODANE!
            };

            Inventory inventory = UnityEngine.Object.FindFirstObjectByType<Inventory>();
            if (inventory != null)
            {
                foreach (var weaponName in inventory.weapons)
                {
                    if (!string.IsNullOrEmpty(weaponName))
                    {
                        data.weapons.Add(weaponName);
                        if (inventory.currentWeaponPrefab != null && inventory.currentWeaponName == weaponName)
                        {
                            Gun gun = inventory.currentWeaponPrefab.GetComponent<Gun>();
                            if (gun != null)
                            {
                                data.weaponSaveDatas.Add(new WeaponSaveData
                                {
                                    weaponName = weaponName,
                                    currentAmmo = gun.currentAmmo,
                                    totalAmmo = gun.totalAmmo
                                });
                            }
                        }
                    }
                }

                foreach (var itemObj in inventory.items)
                {
                    if (itemObj != null)
                    {
                        var interactable = itemObj.GetComponent<InteractableItem>();
                        var treasure = itemObj.GetComponent<TreasureResources>();
                        if (interactable != null && treasure != null)
                        {
                            ItemSaveData itemSave = new ItemSaveData
                            {
                                itemName = interactable.itemName,
                                resourceCategoriesData = new List<ResourceCategoryData>()
                            };
                            foreach (var cat in treasure.resourceCategories)
                            {
                                itemSave.resourceCategoriesData.Add(new ResourceCategoryData
                                {
                                    name = cat.name,
                                    resourceCount = cat.resourceCount
                                });
                            }
                            data.itemSaveDatas.Add(itemSave);
                        }
                    }
                }

                foreach (var loot in inventory.loot)
                    if (loot != null)
                        data.lootNames.Add(loot.name.Replace("(Clone)", "").Trim());
            }

            // --- ZAPIS KOLEKTORÓW ---
            var allCollectors = UnityEngine.Object.FindObjectsByType<TurretCollector>(FindObjectsSortMode.None);

            foreach (var collector in allCollectors)
            {
                TurretCollectorSaveData save = new TurretCollectorSaveData();
                save.slotSaveDatas = collector.GetSlotsSaveData();
                data.collectors.Add(save);
            }
            // --- KONIEC ZAPISU KOLEKTORÓW ---

            // --- ZAPIS REFINERA ---
            TreasureRefiner refiner = UnityEngine.Object.FindFirstObjectByType<TreasureRefiner>();
            if (refiner != null)
                data.treasureRefiner = refiner.GetSaveData();
            else
                data.treasureRefiner = null;

            // --- ZAPIS ZDROWIA OPON ---
            data.wheelHealths.Clear();
            foreach (var tyre in UnityEngine.Object.FindObjectsByType<InteractableItem>(FindObjectsSortMode.None))
            {
                if (tyre.itemName.StartsWith("Opona")) // lub inna logika rozpoznawania opon
                {
                    // U¿yj refleksji lub property aby dostaæ siê do currentHealth (jeœli jest private, zrób property publiczne)
                    var health = typeof(InteractableItem)
                        .GetField("currentHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .GetValue(tyre);
                    data.wheelHealths.Add((int)health);
                }
            }

            // --- ZAPIS ODBLOKOWANIA TERMINALA ---
            foreach (var monitor in UnityEngine.Object.FindObjectsByType<CameraToMonitor>(FindObjectsSortMode.None))
            {
                if (!string.IsNullOrEmpty(monitor.monitorID) && monitor.saveUnlockState)
                {
                    data.monitorUnlockStates.Add(new MonitorUnlockState
                    {
                        monitorID = monitor.monitorID,
                        isUnlocked = !monitor.securedMonitor
                    });
                }
            }

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);
            lastSaveTime = DateTime.Now;

            Debug.Log($"Zapisano dane w slocie {currentSlotIndex}: {path}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"B³¹d zapisu: {ex.Message}");
        }
    }

    public void LoadPlayerData(int slotIndex)
    {
        string path = GetSlotFilePath(slotIndex);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Brak pliku dla slotu {slotIndex}: {path}");
            return;
        }

        try
        {
            currentSlotIndex = slotIndex;

            string json = File.ReadAllText(path);
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);

            StartCoroutine(LoadMainThenTargetScene(data));
        }
        catch (Exception ex)
        {
            Debug.LogError($"B³¹d odczytu: {ex.Message}");
        }
    }

    private IEnumerator LoadMainThenTargetScene(PlayerData data)
    {
        // 1. Najpierw ³aduj scenê Main
        AsyncOperation mainLoad = SceneManager.LoadSceneAsync("Main");
        while (!mainLoad.isDone)
            yield return null;
        yield return null; // Daj czas na inicjalizacjê singletonów i loadingu

        // 2. Nastêpnie ³aduj scenê docelow¹ z save
        AsyncOperation targetLoad = SceneManager.LoadSceneAsync(data.sceneName);
        while (!targetLoad.isDone)
            yield return null;
        yield return null; // Daj czas na inicjalizacjê

        // 3. Poczekaj a¿ gracz siê pojawi
        GameObject player = null;
        float waitTime = 0f;
        while (player == null && waitTime < 2f)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                yield return null;
                waitTime += Time.deltaTime;
            }
        }

        if (player == null)
        {
            Debug.LogWarning("Nie znaleziono gracza po za³adowaniu sceny docelowej.");
            yield break;
        }

        // 4. Ustaw pozycjê i rotacjê gracza z zapisu
        player.transform.position = data.playerPosition;
        player.transform.rotation = data.playerRotation;
        this.playerCurrency = data.playerCurrency;

        Inventory inventory = UnityEngine.Object.FindFirstObjectByType<Inventory>();
        InventoryUI inventoryUI = UnityEngine.Object.FindFirstObjectByType<InventoryUI>();

        if (inventory != null)
        {
            inventory.weapons.Clear();
            foreach (string weaponName in data.weapons)
            {
                if (inventory.weaponPrefabs.TryGetValue(weaponName, out var prefab) && prefab != null)
                {
                    inventory.weapons.Add(weaponName);
                }
                else
                {
                    Debug.LogWarning("Brak prefabu dla: " + weaponName);
                }
            }

            if (inventory.weapons.Count > 0)
            {
                string weaponName = inventory.weapons[0];
                inventory.EquipWeapon(weaponName);

                if (inventory.currentWeaponPrefab != null)
                {
                    Gun gun = inventory.currentWeaponPrefab.GetComponent<Gun>();
                    if (gun != null)
                    {
                        var saved = data.weaponSaveDatas.Find(w => w.weaponName == weaponName);
                        if (saved != null)
                        {
                            gun.currentAmmo = saved.currentAmmo;
                            gun.totalAmmo = saved.totalAmmo;
                        }
                        else
                        {
                            Debug.LogWarning("Load: Brak informacji o amunicji dla " + weaponName);
                        }
                        inventoryUI.UpdateWeaponUI(gun);
                    }
                    inventoryUI.SetWeaponUI(inventory.currentWeaponPrefab);
                }
                else
                {
                    Debug.LogWarning("Load: currentWeaponPrefab == null po EquipWeapon");
                }
            }

            // Najpierw wyczyœæ stare itemy
            inventory.items.Clear();

            List<string> loadedItemNames = new List<string>();
            List<string> loadedItemCats = new List<string>();
            List<string> loadedItemQuant = new List<string>();

            foreach (var itemSave in data.itemSaveDatas)
            {
                if (inventory.itemPrefabs.TryGetValue(itemSave.itemName, out var prefab) && prefab != null)
                {
                    GameObject itemObj = UnityEngine.Object.Instantiate(prefab);

                    // USUÑ TreasureDefiner, ¿eby nie losowa³ nowych danych
                    var definer = itemObj.GetComponent<TreasureDefiner>();
                    if (definer != null)
                        GameObject.Destroy(definer);

                    // ODTWÓRZ stan zasobów na podstawie save
                    var treasure = itemObj.GetComponent<TreasureResources>();
                    if (treasure != null && itemSave.resourceCategoriesData != null && itemSave.resourceCategoriesData.Count > 0)
                    {
                        treasure.resourceCategories.Clear();
                        foreach (var cat in itemSave.resourceCategoriesData)
                        {
                            var rc = new ResourceCategory();
                            rc.name = cat.name;
                            rc.resourceCount = cat.resourceCount;
                            treasure.resourceCategories.Add(rc);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Brak TreasureResources lub pusty save zasobów dla: " + itemSave.itemName);
                    }

                    inventory.items.Add(itemObj);
                    itemObj.SetActive(false); // Bo itemy s¹ w ekwipunku

                    loadedItemNames.Add(itemSave.itemName);
                    // Kategorie i iloœci – z pierwszego zasobu jeœli istnieje
                    if (treasure != null && treasure.resourceCategories.Count > 0)
                    {
                        loadedItemCats.Add(treasure.resourceCategories[0].name);
                        loadedItemQuant.Add(treasure.resourceCategories[0].resourceCount.ToString());
                    }
                    else
                    {
                        loadedItemCats.Add("");
                        loadedItemQuant.Add("0");
                    }
                }
                else
                {
                    Debug.LogWarning($"Brak prefabu dla itemu: {itemSave.itemName}");
                }
            }

            Debug.Log("Load: items loaded = " + string.Join(",", loadedItemNames));
            Debug.Log("Load: items categories = " + string.Join(",", loadedItemCats));
            Debug.Log("Load: items quantities = " + string.Join(",", loadedItemQuant));

            if (inventoryUI != null)
                inventoryUI.UpdateInventoryUI(inventory.weapons, inventory.items);
        }

        // --- WCZYTANIE KOLEKTORÓW (TurretCollector) ---
        Dictionary<string, GameObject> resourcePrefabs = new Dictionary<string, GameObject>();
        foreach (var prefab in Resources.LoadAll<GameObject>("TreasurePrefabs")) // Zmieñ œcie¿kê jeœli inna!
            resourcePrefabs[prefab.name] = prefab;

        var allCollectors = UnityEngine.Object.FindObjectsByType<TurretCollector>(FindObjectsSortMode.None);
        if (data.collectors != null)
        {
            for (int i = 0; i < allCollectors.Length && i < data.collectors.Count; i++)
            {
                allCollectors[i].LoadSlotsFromSave(data.collectors[i].slotSaveDatas, resourcePrefabs);
            }
        }

        // --- WCZYTYWANIE REFINERA ---
        TreasureRefiner refiner = UnityEngine.Object.FindFirstObjectByType<TreasureRefiner>();
        if (refiner != null && data.treasureRefiner != null)
            refiner.LoadFromSaveData(data.treasureRefiner);

        // --- ODCZYT ZDROWIA OPON ---
        var allTyres = UnityEngine.Object.FindObjectsByType<InteractableItem>(FindObjectsSortMode.None);
        int idx = 0;
        foreach (var tyre in allTyres)
        {
            if (tyre.itemName.StartsWith("Opona") && idx < data.wheelHealths.Count)
            {
                tyre.SetCurrentHealth(data.wheelHealths[idx]);
                idx++;
                tyre.RefreshInteractivity();
            }
        }

        // --- ODCZYT ODBLOKOWANIA TERMINALA ---
        foreach (var monitor in UnityEngine.Object.FindObjectsByType<CameraToMonitor>(FindObjectsSortMode.None))
        {
            if (!string.IsNullOrEmpty(monitor.monitorID) && monitor.saveUnlockState)
            {
                var entry = data.monitorUnlockStates.Find(e => e.monitorID == monitor.monitorID);
                if (entry != null)
                {
                    monitor.securedMonitor = !entry.isUnlocked;
                    if (!monitor.securedMonitor)
                    {
                        monitor.hasWonGame = true;
                        monitor.InitializeLocalizedCommands();
                    }
                }
            }
        }

        // --- ODCZYT WALUTY ---
        LootShop lootShop = FindFirstObjectByType<LootShop>();
        if (lootShop != null)
            lootShop.UpdatePlayerCurrencyUI();

        Debug.Log($"Wczytano gracza na pozycjê {player.transform.position}, rotacja {player.transform.rotation}, scena {data.sceneName}");
    }

    public void AddCurrency(float amount)
    {
        playerCurrency += amount;
        Debug.Log($"Dodano {amount} waluty. Obecny stan: {playerCurrency}");
    }

    public void SubtractCurrency(float amount)
    {
        playerCurrency = Mathf.Max(playerCurrency - amount, 0);
        Debug.Log($"Odjêto {amount} waluty. Obecny stan: {playerCurrency}");
    }

    public void ResetCurrency()
    {
        playerCurrency = 0f;
        Debug.Log("Waluta gracza zosta³a zresetowana.");
    }

    public void ResetPositionAndRotation()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = Vector3.zero;
            player.transform.rotation = Quaternion.identity;
            Debug.Log("Pozycja i rotacja gracza zosta³y zresetowane.");
        }
        else
        {
            Debug.LogWarning("Nie znaleziono obiektu gracza z tagiem 'Player'.");
        }
    }

    public void ResetSaveSlot(int slotIndex)
    {
        try
        {
            string path = GetSlotFilePath(slotIndex);

            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"Usuniêto zapis slotu {slotIndex}: {path}");
            }
            else
            {
                Debug.Log($"Nie znaleziono zapisu dla slotu {slotIndex}: {path}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"B³¹d podczas usuwania zapisu slotu {slotIndex}: {ex.Message}");
        }
    }

    public bool DoesSlotExist(int slotIndex)
    {
        return File.Exists(GetSlotFilePath(slotIndex));
    }

    public PlayerData LoadDataWithoutApplying(int slotIndex)
    {
        string path = GetSlotFilePath(slotIndex);
        if (!File.Exists(path)) return null;

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<PlayerData>(json);
    }

    public int GetLastUsedSlotIndex()
    {
        int lastSlot = -1;
        System.DateTime lastTime = System.DateTime.MinValue;

        for (int i = 0; i <= 2; i++)
        {
            string path = GetSlotFilePath(i);
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                PlayerData data = JsonUtility.FromJson<PlayerData>(json);
                if (System.DateTime.TryParse(data.lastSaveTime, out System.DateTime slotTime))
                {
                    if (slotTime > lastTime)
                    {
                        lastTime = slotTime;
                        lastSlot = i;
                    }
                }
            }
        }
        return lastSlot;
    }
}

[Serializable]
public class PlayerData
{
    public float playerCurrency;
    public Vector3 playerPosition;
    public Quaternion playerRotation;
    public string sceneName;
    public string lastSaveTime;

    public List<string> weapons = new List<string>();
    public List<string> itemNames = new List<string>();
    public List<string> lootNames = new List<string>();
    public List<WeaponSaveData> weaponSaveDatas = new List<WeaponSaveData>();
    public List<ItemSaveData> itemSaveDatas = new List<ItemSaveData>();

    public List<TurretCollectorSaveData> collectors = new List<TurretCollectorSaveData>();

    public TreasureRefinerSaveData treasureRefiner;

    public List<int> wheelHealths = new List<int>();

    public List<MonitorUnlockState> monitorUnlockStates = new List<MonitorUnlockState>();
}

[Serializable]
public class WeaponSaveData
{
    public string weaponName;
    public int currentAmmo;
    public int totalAmmo;
}

[System.Serializable]
public class TurretCollectorSaveData
{
    public List<CollectorSlotSaveData> slotSaveDatas = new List<CollectorSlotSaveData>();
}

[System.Serializable]
public class ResourceCategoryData
{
    public string name;
    public int resourceCount;
}

[System.Serializable]
public class ItemSaveData
{
    public string itemName;
    public List<ResourceCategoryData> resourceCategoriesData = new List<ResourceCategoryData>();
}

[Serializable]
public class MonitorUnlockState
{
    public string monitorID;
    public bool isUnlocked;
}
