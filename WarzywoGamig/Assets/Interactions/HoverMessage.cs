using UnityEngine;
using UnityEngine.SceneManagement;

public class HoverMessage : MonoBehaviour
{
    public string message;
    public float interactionDistance = 5f;
    public bool isInteracted = false;
    public bool alwaysActive = false;

    [Header("SCENY")]
    public bool UsingSceneSystem = false; // Nowy prze³¹cznik
    public bool SceneMain = false;
    public bool SceneHome = false;

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        CheckSceneStatus();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckSceneStatus();
    }

    private void CheckSceneStatus()
    {
        if (!UsingSceneSystem)
        {
            return; // Jeœli system scen jest wy³¹czony, nic nie robimy
        }

        string currentSceneName = SceneManager.GetActiveScene().name;

        if (SceneMain && currentSceneName != "Main")
        {
            isInteracted = true;
        }
        else if (SceneHome && currentSceneName != "Home")
        {
            isInteracted = true;
        }
        else
        {
            isInteracted = false;
        }
    }
}
