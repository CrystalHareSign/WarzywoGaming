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

    private int currentSlotIndex = -1; // Bie��cy slot zapisu
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

        // Je�li slot nie zosta� ustawiony, przypisujemy domy�lny slot (np. slot 1)
        if (currentSlotIndex == -1)
        {
            currentSlotIndex = 1;  // Domy�lnie slot 1, mo�na zmieni� w zale�no�ci od potrzeb
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
            Debug.LogError($"B��d zapisu: {ex.Message}");
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
            currentSlotIndex = slotIndex; // Ustawienie aktywnego slotu

            string json = File.ReadAllText(path);
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);
            SceneManager.LoadScene(data.sceneName);
            StartCoroutine(SetPlayerDataAfterSceneLoad(data));
        }
        catch (Exception ex)
        {
            Debug.LogError($"B��d odczytu: {ex.Message}");
        }
    }

    private IEnumerator SetPlayerDataAfterSceneLoad(PlayerData data)
    {
        yield return new WaitForEndOfFrame();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Nie znaleziono gracza po za�adowaniu sceny.");
            yield break;
        }

        player.transform.position = data.playerPosition;
        player.transform.rotation = data.playerRotation;
        this.playerCurrency = data.playerCurrency;

        Debug.Log($"Wczytano dane z slotu {currentSlotIndex}.");
    }

    // Metoda dodaj�ca walut� gracza
    public void AddCurrency(float amount)
    {
        playerCurrency += amount; // Dodaje podan� ilo�� do waluty gracza
        Debug.Log($"Dodano {amount} waluty. Obecny stan: {playerCurrency}");
    }

    // Metoda odejmuj�ca walut� gracza
    public void SubtractCurrency(float amount)
    {
        playerCurrency = Mathf.Max(playerCurrency - amount, 0); // Upewnia si�, �e waluta nie spadnie poni�ej 0
        Debug.Log($"Odj�to {amount} waluty. Obecny stan: {playerCurrency}");
    }

    public void ResetCurrency()
    {
        playerCurrency = 0f;
        Debug.Log("Waluta gracza zosta�a zresetowana.");
    }

    // Metoda resetuj�ca pozycj� i rotacj� gracza
    public void ResetPositionAndRotation()
    {
        // Znajd� obiekt gracza w scenie
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = Vector3.zero; // Ustaw domy�ln� pozycj�
            player.transform.rotation = Quaternion.identity; // Ustaw domy�ln� rotacj�
            Debug.Log("Pozycja i rotacja gracza zosta�y zresetowane.");
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
                Debug.Log($"Usuni�to zapis slotu {slotIndex}: {path}");
            }
            else
            {
                Debug.LogWarning($"Nie znaleziono zapisu dla slotu {slotIndex}: {path}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"B��d podczas usuwania zapisu slotu {slotIndex}: {ex.Message}");
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
}

// Klasa pomocnicza przechowuj�ca dane gracza
[Serializable]
public class PlayerData
{
    public float playerCurrency; // Waluta gracza
    public Vector3 playerPosition; // Pozycja gracza
    public Quaternion playerRotation; // Rotacja gracza
    public string sceneName; // Nazwa sceny
    public string lastSaveTime; // Data ostatniego zapisu
}