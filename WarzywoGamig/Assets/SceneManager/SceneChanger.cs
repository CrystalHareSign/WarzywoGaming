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

    void Start()
    {
        assignInteraction = Object.FindFirstObjectByType<AssignInteraction>(); // Pobierz referencj� do skryptu AssignInteraction
    }

    private void Update()
    {
        // Sprawdzamy, czy lewy przycisk myszy zosta� wci�ni�ty i czy nie ma ju� trwaj�cej zmiany sceny
        if (Input.GetMouseButtonDown(0) && !isSceneChanging)
        {
            // Sprawdzamy, czy obiekt wci�� si� porusza
            if (assignInteraction != null && assignInteraction.isMoving)
            {
                Debug.Log("Nie mo�esz zmieni� sceny, gdy obiekt si� porusza.");
                return;  // Zatrzymanie zmiany sceny, je�li obiekt si� porusza
            }

            // Tworzymy promie� z kamery na pozycj� myszy
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Sprawdzamy, czy promie� trafi� w jakikolwiek obiekt
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Sprawdzamy, czy button1 jest przypisany i klikni�to w button1
                if (button1 != null && hit.transform == button1)
                {
                    TryChangeScene(scene1);  // Pr�ba zmiany sceny na scene1
                }
                // Sprawdzamy, czy button2 jest przypisany i klikni�to w button2
                else if (button2 != null && hit.transform == button2)
                {
                    TryChangeScene(scene2);  // Pr�ba zmiany sceny na scene2
                }
            }
        }
    }


    private void TryChangeScene(string sceneName)
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Zmieniamy scen� tylko wtedy, gdy to nie ta sama scena, na kt�rej ju� jeste�my
        if (currentScene != sceneName && !isSceneChanging)
        {
            isSceneChanging = true;  // Ustawiamy flag� na true przed rozpocz�ciem zmiany sceny

            // Je�li przechodzimy z Home do Main, wykonaj metod�
            if (currentScene == "Home" && sceneName == "Main")
            {
                ExecuteMethodsForMainScene();
            }

            // Za�aduj now� scen�
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.Log("Ju� jeste� w tej scenie!");
        }
    }

    private void ExecuteMethodsForMainScene()
    {
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
                // Teraz znajd� komponent TurretCollector na dziecku
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

    // Mo�na doda� metod� do monitorowania, kiedy scena jest ju� za�adowana, aby zresetowa� flag�
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isSceneChanging = false; // Zmieniamy flag� na false po za�adowaniu sceny
    }

    // Subskrybujemy zdarzenie za�adowania sceny
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Odsubskrybowanie zdarzenia przy wy��czaniu obiektu
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
