using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] private Transform button1;
    [SerializeField] private string scene1;

    [SerializeField] private Transform button2;
    [SerializeField] private string scene2;

    public TreasureRefiner treasureRefiner;
    public Inventory inventory;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // lewy klik myszy
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == button1)
                {
                    TryChangeScene(scene1);
                }
                else if (hit.transform == button2)
                {
                    TryChangeScene(scene2);
                }
            }
        }
    }
    private void TryChangeScene(string sceneName)
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Zmieniamy scen� tylko wtedy, gdy to nie ta sama scena, na kt�rej ju� jeste�my
        if (currentScene != sceneName)
        {
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
        inventory.ClearInventory();
        treasureRefiner.ResetSlots();

        // Tutaj umieszczamy logik�, aby znale�� prefab z komponentem TurretCollector
        TurretCollector turretCollector = FindObjectOfType<TurretCollector>();

        if (turretCollector != null)
        {
            // Wywo�anie metody ze skryptu TurretCollector
            turretCollector.ClearAllSlots(); // Zmie� "SomeMethod" na odpowiedni� metod� w TurretCollector
        }
        else
        {
            Debug.LogWarning("Nie znaleziono komponentu TurretCollector w scenie.");
        }
    }
}
