using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AssignInteraction : MonoBehaviour
{
    public GameObject interactableLeft; // Przedmiot interaktywny 1
    public GameObject interactableRight; // Przedmiot interaktywny 2
    [Header("TAGS: Loot, Item, Weapon, Turret")]
    [Header("TAGS: Body")]
    public GameObject[] manualMoveObjects; // Rêcznie przypisane obiekty
    private List<GameObject> moveObjects = new List<GameObject>(); // Lista przedmiotów do przenoszenia
    public GameObject bus; // Rêcznie przypisany obiekt Busa
    private Vector3 busOriginalPosition;
    public Vector3 playerStartPosition;
    public GameObject player; // Przypisz gracza rêcznie

    private int lastLootCount = 0;
    public float moveDistance = 1.0f; // Odleg³oœæ przenoszenia obiektów
    public float moveDuration = 1.0f; // Czas trwania przenoszenia obiektów
    public bool isMoving = false; // Flaga informuj¹ca, czy obiekt jest w trakcie ruchu

    public WheelManager wheelManager;
    private SceneChanger sceneChanger; // Referencja do SceneChanger
    void Start()
    {

        if (bus != null)
        {
            busOriginalPosition = bus.transform.position;
        }

        if (player != null && bus != null)
        {
            player.transform.position = playerStartPosition;
        }

        else
        {
            Debug.LogWarning("Bus is not assigned in the inspector.");
        }

        // Dodajemy listenera do za³adowania sceny
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Dodaj rêcznie przypisane obiekty do listy
        if (manualMoveObjects != null)
        {
            foreach (GameObject obj in manualMoveObjects)
            {
                if (obj != null && !moveObjects.Contains(obj))
                {
                    moveObjects.Add(obj);
                }
            }
        }

        // Pobierz wszystkie obiekty z tagami "Loot", "Item", "Weapon" oraz "Turret"
        AddObjectsWithTag("Item");
        //AddObjectsWithTag("Weapon");
        AddObjectsWithTag("Turret"); // Dodane dla tagu "Turret"

        if (interactableLeft != null)
        {
            InteractableItem item1 = interactableLeft.GetComponent<InteractableItem>();
            if (item1 != null)
            {
                item1.hasCooldown = true;
                item1.onInteract = () => MoveAll(Vector3.forward);
            }
            else
            {
                Debug.LogWarning("interactableLeft nie ma komponentu InteractableItem!");
            }
        }

        if (interactableRight != null)
        {
            InteractableItem item2 = interactableRight.GetComponent<InteractableItem>();
            if (item2 != null)
            {
                item2.hasCooldown = true;
                item2.onInteract = () => MoveAll(Vector3.back);
            }
            else
            {
                Debug.LogWarning("interactableRight nie ma komponentu InteractableItem!");
            }
        }

        // Pobierz referencjê do SceneChanger
        sceneChanger = Object.FindFirstObjectByType<SceneChanger>();
    }
    private void Update()
    {
        GameObject[] foundLootObjects = GameObject.FindGameObjectsWithTag("Loot");
        if (foundLootObjects.Length != lastLootCount) // Sprawdza, czy liczba obiektów z tagiem "Loot" siê zmieni³a
        {
            AddObjectsWithTag("Loot");
            lastLootCount = foundLootObjects.Length; // Aktualizuje zapisan¹ liczbê obiektów
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (bus != null)
        {
            bus.transform.position = busOriginalPosition;

        }

        if (player != null)
        {
            player.transform.position = playerStartPosition;
        }

        else
        {
            Debug.LogWarning("Bus is not assigned. Cannot reset position.");
        }

        // Po za³adowaniu sceny wyczyœæ listê i dodaj wszystkie obiekty z tagami
        moveObjects.Clear();
        AddObjectsWithTag("Item");
        //AddObjectsWithTag("Weapon");
        AddObjectsWithTag("Turret");
        AddObjectsWithTag("Loot");

        // Ponownie inicjujemy obiekty rêcznie przypisane
        if (manualMoveObjects != null)
        {
            foreach (GameObject obj in manualMoveObjects)
            {
                if (obj != null && !moveObjects.Contains(obj))
                {
                    moveObjects.Add(obj);
                }
            }
        }
    }

    void AddObjectsWithTag(string tag)
    {
        GameObject[] foundObjects = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in foundObjects)
        {
            if (obj != null && !moveObjects.Contains(obj)) // Unikamy duplikatów
            {
                moveObjects.Add(obj);
            }
        }
    }

    void MoveAll(Vector3 direction)
    {
        if (sceneChanger != null && sceneChanger.isSceneChanging) // Sprawdzenie, czy scena jest zmieniana
        {
            Debug.Log("Scene is changing, cannot move objects.");
            return; // Przerywamy, jeœli scena jest zmieniana
        }

        foreach (var item in moveObjects)
        {
            if (item != null && !item.CompareTag("Player")) // SprawdŸ, czy obiekt istnieje i nie jest graczem
            {
                StartCoroutine(Move(item, direction));
            }
        }

        // Przeka¿ kierunek i czas ruchu do WheelManager
        if (wheelManager != null)
        {
            wheelManager.StartSteering(direction, moveDuration);
        }
    }

    IEnumerator Move(GameObject item, Vector3 direction)
    {
        // SprawdŸ, czy obiekt nadal istnieje
        if (item == null)
        {
            yield break; // Jeœli obiekt nie istnieje, zakoñcz coroutine
        }

        Vector3 startPosition = item.transform.position;
        Vector3 endPosition = startPosition + direction * moveDistance;
        float elapsedTime = 0;

        while (elapsedTime < moveDuration)
        {
            // SprawdŸ, czy obiekt nadal istnieje podczas wykonywania ruchu
            if (item == null)
            {
                yield break; // Jeœli obiekt zosta³ zniszczony, zakoñcz coroutine
            }

            item.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ustaw koñcow¹ pozycjê
        if (item != null) // SprawdŸ jeszcze raz, czy obiekt istnieje
        {
            item.transform.position = endPosition;
        }
    }
}
