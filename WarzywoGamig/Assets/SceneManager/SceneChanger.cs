using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class SceneChanger : MonoBehaviour
{

    [SerializeField] private string scene1;
    [SerializeField] private string scene2;

    [Header("Ustaw punkt spawnu gracza po za³adowaniu sceny")]
    [SerializeField] private Transform playerSpawnPoint;
    [Header("Pozycja startowa busa")]
    [SerializeField] private Vector3 busStartPosition = new Vector3(0f, 0f, 0f);
    [SerializeField] private GameObject bus;
    [Header("Dane z Turreta do skryptów")]
    [SerializeField] private GameObject TurretBody;
    [SerializeField] private Vector3 turretStartPosition = new Vector3(0f, 0f, 0f);
    [SerializeField] private Quaternion turretStartRotation = Quaternion.identity;

    [SerializeField] private CameraToMonitor cameraToMonitor; // Referencja do CameraToMonitor

    private bool isSceneChanging = false;
    private AssignInteraction assignInteraction;
    private GameObject player;
    private List<PlaySoundOnObject> playSoundObjects = new List<PlaySoundOnObject>();


    void Start()
    {
        assignInteraction = Object.FindFirstObjectByType<AssignInteraction>();
        playSoundObjects.AddRange(Object.FindObjectsByType<PlaySoundOnObject>(FindObjectsSortMode.None));

        assignInteraction = Object.FindFirstObjectByType<AssignInteraction>();
        playSoundObjects.AddRange(Object.FindObjectsByType<PlaySoundOnObject>(FindObjectsSortMode.None));

        if (TurretBody != null)
        {
            turretStartPosition = TurretBody.transform.position;
            turretStartRotation = TurretBody.transform.rotation;
        }
    }

    public void OnMainSceneButtonClick()
    {
        if (!isSceneChanging)
            TryChangeScene(scene1);
    }
    public void OnHomeSceneButtonClick()
    {
        if (!isSceneChanging)
            TryChangeScene(scene2);
    }

    public void TryChangeScene(string sceneName)
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene != sceneName && !isSceneChanging && cameraToMonitor.canInteract == true)
        {
            isSceneChanging = true;

            if (currentScene == "Home" && sceneName == "Main")
            {
                ExecuteMethodsForMainScene();
            }
            else if (currentScene == "Main" && sceneName == "Home")
            {
                ExecuteMethodsForHomeScene();
            }

            // Wyœwietlenie logu w konsoli monitora
            if (cameraToMonitor != null)
            {
                cameraToMonitor.ShowConsoleMessage(">>> Próba zmiany sceny...", "#00E700");
            }

            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.Log("Ju¿ jesteœ w tej scenie!");

            // Wyœwietlenie logu w konsoli monitora
            if (cameraToMonitor != null && cameraToMonitor.canInteract == true)
            {
                cameraToMonitor.ShowConsoleMessage(">>> Ju¿ jesteœ w tej scenie...","#FF0000");
            }
        }
    }

    private void ExecuteMethodsForMainScene()
    {
        foreach (var playSound in playSoundObjects)
        {
            if (playSound == null) continue;
            playSound.PlaySound("TiresOnGravel", 0.01f, true);
            playSound.PlaySound("DieselBusEngine", 1.2f, true);
            playSound.PlaySound("Storm", 0.1f, true);
        }

        var inventory = Object.FindFirstObjectByType<Inventory>();
        if (inventory != null) inventory.ClearInventory();

        var treasureRefiner = Object.FindFirstObjectByType<TreasureRefiner>();
        if (treasureRefiner != null) treasureRefiner.ResetSlots();

        if (TurretBody != null)
        {
            var turretController = TurretBody.GetComponent<TurretController>();
            if (turretController != null)
            {
                var turretCollector = turretController.GetComponentInChildren<TurretCollector>();
                if (turretCollector != null) turretCollector.ClearAllSlots();
            }
        }
    }

    private void ExecuteMethodsForHomeScene()
    {
        foreach (var playSound in playSoundObjects)
        {
            if (playSound == null) continue;
            playSound.StopSound("DieselBusEngine");
            playSound.StopSound("TiresOnGravel");
            playSound.StopSound("Storm");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isSceneChanging = false;

        if (bus != null)
        {
            bus.transform.position = busStartPosition;
        }
        else
        {
            Debug.LogWarning("Bus nie zosta³ przypisany w inspektorze!");
        }

        // Reset turret position and rotation
        if (TurretBody != null)
        {
            TurretBody.transform.position = turretStartPosition;
            TurretBody.transform.rotation = turretStartRotation;
        }
        else
        {
            Debug.LogWarning("TurretBody nie zosta³ przypisany w inspektorze!");
        }

        StartCoroutine(SpawnPlayerWhenBusIsReady());

        var cameraToMonitor = Object.FindFirstObjectByType<CameraToMonitor>();
        if (cameraToMonitor != null && cameraToMonitor.inputField != null)
        {
            cameraToMonitor.inputField.gameObject.SetActive(true);
            cameraToMonitor.inputField.ActivateInputField(); // <- TO JEST KLUCZOWE
        }
    }

    private IEnumerator SpawnPlayerWhenBusIsReady()
    {
        yield return null;

        player = GameObject.FindGameObjectWithTag("Player");

        if (bus == null)
        {
            Debug.LogWarning("Bus nie przypisany – nie mo¿na ustawiæ gracza.");
            yield break;
        }

        if (player != null && playerSpawnPoint != null)
        {
            player.transform.position = playerSpawnPoint.position;
            player.transform.rotation = playerSpawnPoint.rotation;
        }
        else
        {
            Debug.LogWarning("Brak gracza lub punktu spawnu!");
        }
    }


    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
