using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance; // Singleton

    public float playerCurrency = 0f; // Waluta gracza
    public DateTime lastSaveTime; // Data i godzina ostatniego zapisu

    private int currentSlotIndex = -1; // Bie¿¹cy slot zapisu
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

        // Jeœli slot nie zosta³ ustawiony, przypisujemy domyœlny slot (np. slot 1)
        if (currentSlotIndex == -1)
        {
            currentSlotIndex = 1;  // Domyœlnie slot 1, mo¿na zmieniæ w zale¿noœci od potrzeb
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
                lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

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

            // Zapamiêtaj dane docelowej sceny i parametry gracza
            StartCoroutine(LoadMainThenTargetScene(data));
        }
        catch (Exception ex)
        {
            Debug.LogError($"B³¹d odczytu: {ex.Message}");
        }
    }

    private IEnumerator LoadMainThenTargetScene(PlayerData data)
    {
        // 1. £aduj najpierw "Main", jeœli nie jesteœmy ju¿ w niej
        if (SceneManager.GetActiveScene().name != "Main")
        {
            AsyncOperation mainLoad = SceneManager.LoadSceneAsync("Main");
            while (!mainLoad.isDone)
                yield return null;
        }

        // 2. Poczekaj do koñca klatki na wszelki wypadek
        yield return null;

        // 3. Teraz ³aduj scenê docelow¹ z save
        if (SceneManager.GetActiveScene().name != data.sceneName)
        {
            AsyncOperation targetLoad = SceneManager.LoadSceneAsync(data.sceneName);
            while (!targetLoad.isDone)
                yield return null;
        }

        // 4. Poczekaj a¿ gracz pojawi siê w scenie docelowej (max 2 sekundy)
        yield return null;
        float waitTime = 0f;
        GameObject player = null;
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

        // 5. Ustaw dane gracza z save
        player.transform.position = data.playerPosition;
        player.transform.rotation = data.playerRotation;
        this.playerCurrency = data.playerCurrency;

        // Zaktualizuj UI lub inne systemy jeœli trzeba
        LootShop lootShop = FindFirstObjectByType<LootShop>();
        if (lootShop != null)
            lootShop.UpdatePlayerCurrencyUI();

        Debug.Log($"Wczytano gracza na pozycjê {player.transform.position}, rotacja {player.transform.rotation}, scena {data.sceneName}");
    }

    // Metoda dodaj¹ca walutê gracza
    public void AddCurrency(float amount)
    {
        playerCurrency += amount; // Dodaje podan¹ iloœæ do waluty gracza
        Debug.Log($"Dodano {amount} waluty. Obecny stan: {playerCurrency}");
    }

    // Metoda odejmuj¹ca walutê gracza
    public void SubtractCurrency(float amount)
    {
        playerCurrency = Mathf.Max(playerCurrency - amount, 0); // Upewnia siê, ¿e waluta nie spadnie poni¿ej 0
        Debug.Log($"Odjêto {amount} waluty. Obecny stan: {playerCurrency}");
    }

    public void ResetCurrency()
    {
        playerCurrency = 0f;
        Debug.Log("Waluta gracza zosta³a zresetowana.");
    }

    // Metoda resetuj¹ca pozycjê i rotacjê gracza
    public void ResetPositionAndRotation()
    {
        // ZnajdŸ obiekt gracza w scenie
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = Vector3.zero; // Ustaw domyœln¹ pozycjê
            player.transform.rotation = Quaternion.identity; // Ustaw domyœln¹ rotacjê
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

        // Zak³adamy 3 sloty (1,2,3) – zmieñ jeœli masz ich wiêcej!
        for (int i = 1; i <= 3; i++)
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

// Klasa pomocnicza przechowuj¹ca dane gracza
[Serializable]
public class PlayerData
{
    public float playerCurrency; // Waluta gracza
    public Vector3 playerPosition; // Pozycja gracza
    public Quaternion playerRotation; // Rotacja gracza
    public string sceneName; // Nazwa sceny
    public string lastSaveTime; // Data ostatniego zapisu
}