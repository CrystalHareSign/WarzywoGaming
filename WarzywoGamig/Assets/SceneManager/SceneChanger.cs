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
        if (SceneManager.GetActiveScene().name != sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.Log("Ju¿ jesteœ w tej scenie!");
        }
    }
}
