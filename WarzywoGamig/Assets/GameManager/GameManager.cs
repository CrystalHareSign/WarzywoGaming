using System.IO;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton

    public float playerCurrency = 0f; // Waluta gracza

    private string dataPath; // Œcie¿ka do pliku z danymi

    private void Awake()
    {
        // Tworzymy Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Nie niszcz obiektu przy zmianie sceny
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Ustawiamy œcie¿kê do pliku z danymi
        dataPath = Application.persistentDataPath + "/playerData.json";

        // Wczytujemy dane przy starcie gry
        LoadPlayerData();
    }

    private void OnApplicationQuit()
    {
        // Zapisujemy dane przy zamykaniu gry
        SavePlayerData();
    }

    public void AddCurrency(float amount)
    {
        playerCurrency += amount;
    }

    public void SubtractCurrency(float amount)
    {
        playerCurrency = Mathf.Max(playerCurrency - amount, 0); // Unikamy wartoœci ujemnych
    }

    public void SavePlayerData()
    {
        // Tworzymy obiekt danych do zapisania
        PlayerData data = new PlayerData
        {
            playerCurrency = this.playerCurrency
        };

        // Serializujemy dane do formatu JSON
        string json = JsonUtility.ToJson(data);

        // Zapisujemy dane do pliku
        File.WriteAllText(dataPath, json);

        Debug.Log("Dane gracza zapisane w " + dataPath);
    }

    public void LoadPlayerData()
    {
        // Sprawdzamy, czy plik z danymi istnieje
        if (File.Exists(dataPath))
        {
            // Odczytujemy dane z pliku
            string json = File.ReadAllText(dataPath);

            // Deserializujemy dane z formatu JSON
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);

            // Ustawiamy odczytan¹ walutê gracza
            this.playerCurrency = data.playerCurrency;

            Debug.Log("Dane gracza wczytane z " + dataPath);
        }
        else
        {
            Debug.LogWarning("Brak pliku z danymi gracza, ustawiono domyœln¹ wartoœæ waluty.");
            this.playerCurrency = 0f; // Domyœlna wartoœæ
        }
    }

    // Funkcja resetuj¹ca dane gracza
    public void ResetPlayerData()
    {
        playerCurrency = 0f;
        Debug.Log("Dane gracza zosta³y zresetowane.");
    }
}

// Klasa pomocnicza przechowuj¹ca dane gracza
[System.Serializable]
public class PlayerData
{
    public float playerCurrency; // Waluta gracza
}