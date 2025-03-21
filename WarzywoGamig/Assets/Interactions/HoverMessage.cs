using UnityEngine;
using UnityEngine.SceneManagement;  // Dodajemy ten namespace

public class HoverMessage : MonoBehaviour
{
    public string message;
    public float interactionDistance = 5f;
    public bool isInteracted = false;
    public bool alwaysActive = false;

    [Header("SCENY")]
    public bool SceneMain = false;
    public bool SceneHome = false;

    private void Start()
    {
        // Nas³uchujemy na zmianê sceny
        SceneManager.sceneLoaded += OnSceneLoaded;
        CheckSceneStatus(); // Sprawdzamy status sceny na pocz¹tku
    }

    private void OnDestroy()
    {
        // Usuwamy nas³uchiwacz przy zniszczeniu obiektu
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Sprawdzamy status przy ka¿dej zmianie sceny
        CheckSceneStatus();
    }

    private void CheckSceneStatus()
    {
        // Sprawdzamy, w jakiej scenie siê znajdujemy
        string currentSceneName = SceneManager.GetActiveScene().name;

        // Jeœli scena nie pasuje do ustawionych boolów, ustawiamy isInteracted na true
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
            // Jeœli scena jest odpowiednia, ustawiamy isInteracted na false
            isInteracted = false;
        }
    }
}
