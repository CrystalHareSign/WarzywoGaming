using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] private Transform button1;
    [SerializeField] private string scene1;

    [SerializeField] private Transform button2;
    [SerializeField] private string scene2;

    public bool isSceneChanging = false;  // Dodanie zmiennej bool
    private AssignInteraction assignInteraction;
    public GameObject TurretBody;
    public PlaySoundOnObject playSoundOnObject;

    void Start()
    {
        assignInteraction = Object.FindFirstObjectByType<AssignInteraction>(); // Pobierz referencjê do skryptu AssignInteraction
    }

    private void Update()
    {
        // Sprawdzamy, czy lewy przycisk myszy zosta³ wciœniêty i czy nie ma ju¿ trwaj¹cej zmiany sceny
        if (Input.GetMouseButtonDown(0) && !isSceneChanging)
        {
            // Sprawdzamy, czy obiekt wci¹¿ siê porusza
            if (assignInteraction != null && assignInteraction.isMoving)
            {
                Debug.Log("Nie mo¿esz zmieniæ sceny, gdy obiekt siê porusza.");
                return;  // Zatrzymanie zmiany sceny, jeœli obiekt siê porusza
            }

            // Tworzymy promieñ z kamery na pozycjê myszy
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Sprawdzamy, czy promieñ trafi³ w jakikolwiek obiekt
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Sprawdzamy, czy button1 jest przypisany i klikniêto w button1
                if (button1 != null && hit.transform == button1)
                {
                    TryChangeScene(scene1);  // Próba zmiany sceny na scene1
                }
                // Sprawdzamy, czy button2 jest przypisany i klikniêto w button2
                else if (button2 != null && hit.transform == button2)
                {
                    TryChangeScene(scene2);  // Próba zmiany sceny na scene2
                }
            }
        }
    }


    private void TryChangeScene(string sceneName)
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene != sceneName && !isSceneChanging)
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

            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.Log("Ju¿ jesteœ w tej scenie!");
        }
    }

    private void ExecuteMethodsForMainScene()
    {

        playSoundOnObject.PlaySound("DieselBusEngine", 1f, false);

        Inventory inventory = Object.FindFirstObjectByType<Inventory>();
        if (inventory == null)
        {
            Debug.LogWarning("Nie znaleziono komponentu Inventory w scenie.");
        }
        else
        {
            inventory.ClearInventory();
        }

        TreasureRefiner treasureRefiner = Object.FindFirstObjectByType<TreasureRefiner>();
        if (treasureRefiner == null)
        {
            Debug.LogWarning("Nie znaleziono komponentu TreasureRefiner w scenie.");
        }
        else
        {
            treasureRefiner.ResetSlots();
        }

        // Sprawdzamy, czy TurretBody istnieje w scenie
        if (TurretBody != null)
        {
            // Pobierz komponent TurretController
            TurretController turretController = TurretBody.GetComponent<TurretController>();

            if (turretController != null)
            {
                // Teraz znajdŸ komponent TurretCollector na dziecku
                TurretCollector turretCollector = turretController.GetComponentInChildren<TurretCollector>();

                if (turretCollector != null)
                {
                    turretCollector.ClearAllSlots();
                }
                else
                {
                    Debug.LogWarning("Nie znaleziono komponentu TurretCollector na dziecku obiektu.");
                }
            }
            else
            {
                Debug.LogWarning("Nie znaleziono komponentu TurretController na obiekcie.");
            }
        }
        else
        {
            Debug.LogWarning("TurretBody nie istnieje w scenie.");
        }
    }
    private void ExecuteMethodsForHomeScene()
    {
        playSoundOnObject.StopSound("DieselBusEngine");
    }

    // Mo¿na dodaæ metodê do monitorowania, kiedy scena jest ju¿ za³adowana, aby zresetowaæ flagê
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isSceneChanging = false; // Zmieniamy flagê na false po za³adowaniu sceny
    }

    // Subskrybujemy zdarzenie za³adowania sceny
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Odsubskrybowanie zdarzenia przy wy³¹czaniu obiektu
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
