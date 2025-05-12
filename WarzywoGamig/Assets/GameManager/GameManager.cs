using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton

    public float playerCurrency = 0f; // Waluta gracza
    public DateTime lastSaveTime; // Data i godzina ostatniego zapisu

    private string dataPath; // Œcie¿ka do pliku z danymi

    private void Awake()
    {
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

        dataPath = Application.persistentDataPath + "/playerData.json";
    }

    // Metoda automatycznego zapisu danych gracza
    public void SavePlayerData()
    {
        try
        {
            // ZnajdŸ gracza w scenie po tagu "Player"
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("Nie znaleziono obiektu gracza z tagiem 'Player'. Zapis przerwany.");
                return;
            }

            // Pobierz dane gracza
            Vector3 playerPosition = player.transform.position;
            Quaternion playerRotation = player.transform.rotation;

            // Tworzymy obiekt danych do zapisania
            PlayerData data = new PlayerData
            {
                playerCurrency = this.playerCurrency,
                playerPosition = playerPosition,
                playerRotation = playerRotation,
                sceneName = SceneManager.GetActiveScene().name,
                lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // Serializujemy dane do formatu JSON
            string json = JsonUtility.ToJson(data, true);

            // Zapisujemy dane do pliku
            File.WriteAllText(dataPath, json);

            // Aktualizujemy datê ostatniego zapisu
            lastSaveTime = DateTime.Now;

            Debug.Log($"Dane gracza zapisane w {dataPath}.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"B³¹d podczas zapisywania danych: {ex.Message}");
        }
    }

    // Metoda automatycznego wczytywania danych gracza
    public void LoadPlayerData()
    {
        if (!File.Exists(dataPath))
        {
            Debug.LogWarning("Brak pliku zapisu gry. Wczytywanie przerwane.");
            return;
        }

        try
        {
            // Odczytaj dane z pliku
            string json = File.ReadAllText(dataPath);

            // Deserializuj dane z formatu JSON
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);

            // Wczytaj scenê
            SceneManager.LoadScene(data.sceneName);

            // Po za³adowaniu sceny ustaw dane gracza
            StartCoroutine(SetPlayerDataAfterSceneLoad(data));
        }
        catch (Exception ex)
        {
            Debug.LogError($"B³¹d podczas wczytywania danych: {ex.Message}");
        }
    }

    private System.Collections.IEnumerator SetPlayerDataAfterSceneLoad(PlayerData data)
    {
        // Czekaj na za³adowanie sceny
        yield return new WaitForEndOfFrame();

        // ZnajdŸ gracza w scenie po tagu "Player"
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Nie znaleziono obiektu gracza z tagiem 'Player'. Wczytywanie przerwane.");
            yield break;
        }

        // Ustaw dane gracza
        player.transform.position = data.playerPosition;
        player.transform.rotation = data.playerRotation;
        this.playerCurrency = data.playerCurrency;

        Debug.Log($"Dane gracza zosta³y wczytane. Pozycja: {data.playerPosition}, Waluta: {data.playerCurrency}");
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

    // Metoda usuwaj¹ca plik zapisu
    public void ResetSaveFile()
    {
        if (File.Exists(dataPath))
        {
            File.Delete(dataPath);
            Debug.Log("Plik zapisu zosta³ usuniêty.");
        }
        else
        {
            Debug.LogWarning("Brak pliku zapisu do usuniêcia.");
        }
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