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
    private InteractableItem interactableItem;
    private GameObject player;
    private List<PlaySoundOnObject> playSoundObjects = new List<PlaySoundOnObject>();

    // Statyczna zmienna na pozycjê wzglêdn¹ gracza wzglêdem busa
    public static Vector3 lastRelativePlayerPos = Vector3.zero;
    public static Vector3 defaultRelativePlayerPos = new Vector3(0f, 5f, 0f); // przyk³adowy offset obok busa


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

        // ZnajdŸ TreasureRefiner w scenie
        TreasureRefiner treasureRefiner = Object.FindFirstObjectByType<TreasureRefiner>();

        // SprawdŸ, czy rafinacja jest w toku
        if (treasureRefiner != null && treasureRefiner.isSpawning)
        {
            Debug.Log("Rafinacja w toku. Nie mo¿na zmieniæ sceny.");

            // Wyœwietl komunikat w konsoli monitora tylko, gdy mo¿emy wchodziæ w interakcje
            if (cameraToMonitor != null && cameraToMonitor.canInteract)
            {
                cameraToMonitor.ShowConsoleMessage(">>> Rafinacja w toku. Nie mo¿na zmieniæ sceny.", "#FF0000");
            }

            return; // Zakoñcz dzia³anie metody, nie zmieniaj¹c sceny
        }

        // Sprawdzenie, czy próba zmiany sceny na tê sam¹
        if (currentScene == sceneName)
        {
            Debug.Log("Ju¿ jesteœ w tej scenie!");

            // Wyœwietl komunikat w konsoli monitora
            if (cameraToMonitor != null && cameraToMonitor.canInteract)
            {
                cameraToMonitor.ShowConsoleMessage(">>> Ju¿ jesteœ w tej scenie...", "#FF0000");
            }

            return; // Zakoñcz dzia³anie metody, nie zmieniaj¹c sceny
        }

        // Je¿eli scena jest inna i rafinacja nie jest w toku, przeprowadŸ zmianê sceny
        if (!isSceneChanging && cameraToMonitor.canInteract)
        {
            isSceneChanging = true;

            // --- ZAPAMIÊTAJ POZYCJÊ GRACZA WZGLÊDEM BUSA ---
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null && bus != null)
                lastRelativePlayerPos = playerObj.transform.position - bus.transform.position;

            // Dostosowanie: Wykonaj metody specyficzne dla scen
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

            // Za³aduj scenê
            SceneManager.LoadScene(sceneName);
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

        //var inventory = Object.FindFirstObjectByType<Inventory>();
        //if (inventory != null) inventory.ClearInventory();            czyszcenie itemow po zmienie sceny

        //var treasureRefiner = Object.FindFirstObjectByType<TreasureRefiner>();
        //if (treasureRefiner != null) treasureRefiner.ResetSlots();    czyszczenie refinera po zmianie sceny

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

        if (InventoryUI.Instance != null)
        {
            InventoryUI.Instance.UpdateInventoryUI(
                Inventory.Instance.weapons,
                Inventory.Instance.items,
                Inventory.Instance.currentWeaponName // <-- dodane
            );
        }

        StartCoroutine(RefreshWheelsUIDelayed());
    }

    private IEnumerator RefreshWheelsUIDelayed()
    {
        // Odczekaj jedn¹ klatkê, ¿eby WheelHealthUI zd¹¿y³ siê zainicjalizowaæ!
        yield return null;
        var allWheels = Object.FindObjectsByType<InteractableItem>(FindObjectsSortMode.None);
        foreach (var item in allWheels)
        {
            if (item.usesHealthSystem)
                item.UpdateUI();
        }
    }

    private IEnumerator SpawnPlayerWhenBusIsReady()
    {
        // Jeœli trwa ³adowanie z save, NIE ustawiaj pozycji gracza (SaveManager siê tym zajmie)
        if (SaveManager.Instance != null && SaveManager.Instance.isLoading)
        {
            yield break;
        }

        yield return null;

        player = GameObject.FindGameObjectWithTag("Player");

        if (bus == null)
        {
            Debug.LogWarning("Bus nie przypisany – nie mo¿na ustawiæ gracza.");
            yield break;
        }

        if (player != null && bus != null)
        {
            Vector3 relPos = SceneChanger.lastRelativePlayerPos;
            if (relPos == Vector3.zero)
                relPos = SceneChanger.defaultRelativePlayerPos; // <- tu ustawiasz domyœlne miejsce na start

            player.transform.position = bus.transform.position + relPos;
        }

        else
        {
            Debug.LogWarning("Brak gracza lub busa do ustawienia pozycji!");
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
