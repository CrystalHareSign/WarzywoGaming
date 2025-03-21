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
        // Nas�uchujemy na zmian� sceny
        SceneManager.sceneLoaded += OnSceneLoaded;
        CheckSceneStatus(); // Sprawdzamy status sceny na pocz�tku
    }

    private void OnDestroy()
    {
        // Usuwamy nas�uchiwacz przy zniszczeniu obiektu
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Sprawdzamy status przy ka�dej zmianie sceny
        CheckSceneStatus();
    }

    private void CheckSceneStatus()
    {
        // Sprawdzamy, w jakiej scenie si� znajdujemy
        string currentSceneName = SceneManager.GetActiveScene().name;

        // Je�li scena nie pasuje do ustawionych bool�w, ustawiamy isInteracted na true
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
            // Je�li scena jest odpowiednia, ustawiamy isInteracted na false
            isInteracted = false;
        }
    }
}
