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

    public bool isLoading = false;

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
        InputBlocker.Active = true;
        try
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

                // --- ZAPIS KOLEKTOR”W ---
                var allCollectors = UnityEngine.Object.FindObjectsByType<TurretCollector>(FindObjectsSortMode.None);

                foreach (var collector in allCollectors)
                {
                    TurretCollectorSaveData save = new TurretCollectorSaveData();
                    save.slotSaveDatas = collector.GetSlotsSaveData();
                    data.collectors.Add(save);
                }
                // --- KONIEC ZAPISU KOLEKTOR”W ---

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
                    if (tyre.itemName.StartsWith("Opona"))
                    {
                        var health = typeof(InteractableItem)
                            .GetField("currentHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            .GetValue(tyre);
                        data.wheelHealths.Add(new TyreHealthData
                        {
                            itemName = tyre.itemName,
                            health = (int)health
                        });
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
                Debug.LogError($"B≥πd zapisu: {ex.Message}");
            }
        }

        finally
        {
            InputBlocker.Active = false;
        }
    }

    public void LoadPlayerData(int slotIndex)
    {
        InputBlocker.Active = true;
        isLoading = true;
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

            SceneManager.LoadScene("LoadingScreen");
            StartCoroutine(WaitAndLoadGameRoutine(data));
        }
        catch (Exception ex)
        {
            Debug.LogError($"B≥πd odczytu: {ex.Message}");
        }
    }

    // Czeka aø LoadingScreenUI.Instance siÍ pojawi, potem odpala ≥adowanie scen i progres:
    private IEnumerator WaitAndLoadGameRoutine(PlayerData data)
    {
        // Czekaj aø LoadingScreenUI (ze sceny LoadingScreen) bÍdzie gotowy
        while (LoadingScreen.Instance == null)
            yield return null;

        // Moøesz od razu zresetowaÊ pasek postÍpu
        LoadingScreen.Instance.SetProgress(0f);

        // Teraz w≥aúciwa logika ≥adowania, czyli to co by≥o w LoadMainThenTargetScene:
        yield return StartCoroutine(LoadMainThenTargetScene(data));
    }

    private IEnumerator LoadMainThenTargetScene(PlayerData data)
    {
        // 1. Najpierw ≥aduj scenÍ Main additive
        AsyncOperation mainLoad = SceneManager.LoadSceneAsync("Main", LoadSceneMode.Additive);
        mainLoad.allowSceneActivation = false;

        // Pasek postÍpu od 0 do 0.5
        while (mainLoad.progress < 0.9f)
        {
            LoadingScreen.Instance.SetProgress(Mathf.Lerp(0f, 0.5f, mainLoad.progress / 0.9f));
            yield return null;
        }
        LoadingScreen.Instance.SetProgress(0.5f);
        mainLoad.allowSceneActivation = true;
        yield return new WaitUntil(() => mainLoad.isDone);
        yield return null; // Daj czas na inicjalizacjÍ singletonÛw i loadingu

        // 2. NastÍpnie ≥aduj scenÍ docelowπ z save, teø additive
        AsyncOperation targetLoad = SceneManager.LoadSceneAsync(data.sceneName, LoadSceneMode.Additive);
        targetLoad.allowSceneActivation = false;

        while (targetLoad.progress < 0.9f)
        {
            LoadingScreen.Instance.SetProgress(Mathf.Lerp(0.5f, 1f, targetLoad.progress / 0.9f));
            yield return null;
        }
        LoadingScreen.Instance.SetProgress(1f);
        targetLoad.allowSceneActivation = true;
        yield return new WaitUntil(() => targetLoad.isDone);
        yield return null; // Daj czas na inicjalizacjÍ

        // 3. Ustaw aktywnπ scenÍ na docelowπ
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(data.sceneName));

        yield return SceneManager.UnloadSceneAsync("Main");

        // 4. ZNAJDè GRACZA DDOL

        foreach (var p in GameObject.FindGameObjectsWithTag("Player"))
        {
            p.transform.position = data.playerPosition;
            p.transform.rotation = data.playerRotation;
        }

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

            // Najpierw wyczyúÊ stare itemy
            inventory.items.Clear();

            List<string> loadedItemNames = new List<string>();
            List<string> loadedItemCats = new List<string>();
            List<string> loadedItemQuant = new List<string>();

            foreach (var itemSave in data.itemSaveDatas)
            {
                if (inventory.itemPrefabs.TryGetValue(itemSave.itemName, out var prefab) && prefab != null)
                {
                    GameObject itemObj = UnityEngine.Object.Instantiate(prefab);

                    // USU— TreasureDefiner, øeby nie losowa≥ nowych danych
                    var definer = itemObj.GetComponent<TreasureDefiner>();
                    if (definer != null)
                        GameObject.Destroy(definer);

                    // ODTW”RZ stan zasobÛw na podstawie save
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
                        Debug.LogWarning("Brak TreasureResources lub pusty save zasobÛw dla: " + itemSave.itemName);
                    }

                    inventory.items.Add(itemObj);
                    itemObj.SetActive(false); // Bo itemy sπ w ekwipunku

                    loadedItemNames.Add(itemSave.itemName);
                    // Kategorie i iloúci ñ z pierwszego zasobu jeúli istnieje
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

            //Debug.Log("Load: items loaded = " + string.Join(",", loadedItemNames));
            //Debug.Log("Load: items categories = " + string.Join(",", loadedItemCats));
            //Debug.Log("Load: items quantities = " + string.Join(",", loadedItemQuant));

            if (inventoryUI != null)
                inventoryUI.UpdateInventoryUI(inventory.weapons, inventory.items);
        }

        // --- WCZYTANIE KOLEKTOR”W (TurretCollector) ---
        Dictionary<string, GameObject> resourcePrefabs = new Dictionary<string, GameObject>();
        foreach (var prefab in Resources.LoadAll<GameObject>("TreasurePrefabs")) // ZmieÒ úcieøkÍ jeúli inna!
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
            refiner.UpdateButtonStates();

        var allTyres = UnityEngine.Object.FindObjectsByType<InteractableItem>(FindObjectsSortMode.None);
        foreach (var tyre in allTyres)
        {
            if (tyre.itemName.StartsWith("Opona"))
            {
                var saved = data.wheelHealths.Find(t => t.itemName == tyre.itemName);
                if (saved != null)
                {
                    tyre.SetCurrentHealth(saved.health);
                }
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

        // 6. (opcjonalnie) Unload sceny LoadingScreen po zakoÒczeniu
        yield return SceneManager.UnloadSceneAsync("LoadingScreen");

        StartCoroutine(FixPlayerPositionAfterLoad(data, 120));
    }

    public IEnumerator FixPlayerPositionAfterLoad(PlayerData data, int frameCount)
    {
        for (int i = 0; i < frameCount; i++)
            yield return null;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = data.playerPosition;
            player.transform.rotation = data.playerRotation;

            // Wy≥πcz PlayerInteraction na czas ≥adowania
            var interaction = player.GetComponent<PlayerInteraction>();
            if (interaction != null)
                interaction.enabled = false;

            if (Inventory.Instance.currentWeaponPrefab != null)
            {
                var gunScript = Inventory.Instance.currentWeaponPrefab.GetComponent<Gun>();
                if (gunScript != null)
                    gunScript.enabled = false;
            }
        }

        SetPlayerMovementEnabled(false);

        if (LoadingScreen.Instance != null)
            LoadingScreen.Instance.ShowContinuePrompt(true);

        yield return new WaitUntil(() => Input.anyKeyDown);

        if (LoadingScreen.Instance != null)
            LoadingScreen.Instance.Hide();

        isLoading = false;
        InputBlocker.Active = false;

        SetPlayerMovementEnabled(true);

        // PRZERWA pÛ≥ sekundy
        yield return new WaitForSeconds(1f);


        // W£•CZ PlayerInteraction po przerwie
        if (player != null)
        {
            var interaction = player.GetComponent<PlayerInteraction>();
            if (interaction != null)
                interaction.enabled = true;
        }

        if (Inventory.Instance.currentWeaponPrefab != null)
        {
            var gunScript = Inventory.Instance.currentWeaponPrefab.GetComponent<Gun>();
            if (gunScript != null)
                gunScript.enabled = true;
        }
    }

    private void SetPlayerMovementEnabled(bool enabled)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // Szukaj wszÍdzie w hierarchii gracza
        var movement = player.GetComponentInChildren<PlayerMovement>(true);
        if (movement != null) movement.enabled = enabled;

        var look = player.GetComponentInChildren<MouseLook>(true);
        if (look != null) look.enabled = enabled;
    }

    public void AddCurrency(float amount)
    {
        playerCurrency += amount;
        Debug.Log($"Dodano {amount} waluty. Obecny stan: {playerCurrency}");
    }

    public void SubtractCurrency(float amount)
    {
        playerCurrency = Mathf.Max(playerCurrency - amount, 0);
        Debug.Log($"OdjÍto {amount} waluty. Obecny stan: {playerCurrency}");
    }

    public void ResetCurrency()
    {
        playerCurrency = 0f;
        //Debug.Log("Waluta gracza zosta≥a zresetowana.");
    }

    public void ResetSaveSlot(int slotIndex)
    {
        try
        {
            string path = GetSlotFilePath(slotIndex);

            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"UsuniÍto zapis slotu {slotIndex}: {path}");
            }
            else
            {
                Debug.Log($"Nie znaleziono zapisu dla slotu {slotIndex}: {path}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"B≥πd podczas usuwania zapisu slotu {slotIndex}: {ex.Message}");
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

    public List<TyreHealthData> wheelHealths = new List<TyreHealthData>();

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

[Serializable]
public class TyreHealthData
{
    public string itemName;
    public int health;
}
