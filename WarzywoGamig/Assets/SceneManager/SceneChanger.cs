using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] private Transform button1;
    [SerializeField] private string scene1;

    [SerializeField] private Transform button2;
    [SerializeField] private string scene2;

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

        // Zmieniamy scenê tylko wtedy, gdy to nie ta sama scena, na której ju¿ jesteœmy
        if (currentScene != sceneName)
        {
            // Jeœli przechodzimy z Home do Main, wykonaj metodê
            if (currentScene == "Home" && sceneName == "Main")
            {
                ExecuteMethodsForMainScene();
            }

            // Za³aduj now¹ scenê
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.Log("Ju¿ jesteœ w tej scenie!");
        }
    }

    private void ExecuteMethodsForMainScene()
    {
        Inventory inventory = FindObjectOfType<Inventory>();
        if (inventory == null)
        {
            Debug.LogWarning("Nie znaleziono komponentu Inventory w scenie.");
        }
        else
        {
            inventory.ClearInventory();
        }

        TreasureRefiner treasureRefiner = FindObjectOfType<TreasureRefiner>();
        if (treasureRefiner == null)
        {
            Debug.LogWarning("Nie znaleziono komponentu TreasureRefiner w scenie.");
        }
        else
        {
            treasureRefiner.ResetSlots();
        }

        TurretCollector turretCollector = FindObjectOfType<TurretCollector>();
        if (turretCollector == null)
        {
            Debug.LogWarning("Nie znaleziono komponentu TurretCollector w scenie.");
        }
        else
        {
            turretCollector.ClearAllSlots();
        }
    }

}
