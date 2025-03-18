using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AssignInteraction : MonoBehaviour
{
    public GameObject interactableLeft; // Przedmiot interaktywny 1
    public GameObject interactableRight; // Przedmiot interaktywny 2
    [Header("TAGS: Loot, Item, Weapon, Turret")]
    [Header("TAGS: Body")]
    public GameObject[] manualMoveObjects; // Rêcznie przypisane obiekty
    private List<GameObject> moveObjects = new List<GameObject>(); // Lista przedmiotów do przenoszenia
    private int lastLootCount = 0;
    [Header("Body Move")]
    public float moveDistance = 1.0f; // Odleg³oœæ przenoszenia obiektów
    public float moveDuration = 1.0f; // Czas trwania przenoszenia obiektów
    [Header("WheelTurn")]
    public float rotationSpeed = 100f;
    // Cztery transformy dla kó³
    [Header("LP")]
    public Transform frontLeftWheel;
    [Header("PP")]
    public Transform frontRightWheel;
    [Header("LT")]
    public Transform backLeftWheel;
    [Header("PT")]
    public Transform backRightWheel;

    void Start()
    {
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
        AddObjectsWithTag("Weapon");
        AddObjectsWithTag("Turret"); // Dodane dla tagu "Turret"

        // Przypisz funkcje do interaktywnych przedmiotów
        if (interactableLeft != null)
        {
            InteractableItem item1 = interactableLeft.GetComponent<InteractableItem>();
            item1.hasCooldown = true; // Ustaw cooldown dla tego przedmiotu
            item1.onInteract = () => MoveAll(Vector3.forward);
        }

        if (interactableRight != null)
        {
            InteractableItem item2 = interactableRight.GetComponent<InteractableItem>();
            item2.hasCooldown = true; // Ustaw cooldown dla tego przedmiotu
            item2.onInteract = () => MoveAll(Vector3.back);
        }
    }
    private void Update()
    {
        // Rotacja wszystkich kó³
        RotateWheel(frontLeftWheel, rotationSpeed);
        RotateWheel(frontRightWheel, rotationSpeed);
        RotateWheel(backLeftWheel, rotationSpeed);
        RotateWheel(backRightWheel, rotationSpeed);

        GameObject[] foundLootObjects = GameObject.FindGameObjectsWithTag("Loot");
        if (foundLootObjects.Length != lastLootCount) // Sprawdza, czy liczba obiektów z tagiem "Loot" siê zmieni³a
        {
            AddObjectsWithTag("Loot");
            lastLootCount = foundLootObjects.Length; // Aktualizuje zapisan¹ liczbê obiektów
        }
    }
    // Funkcja do rotacji kó³ z uwzglêdnieniem odbicia lustrzanego dla prawych kó³
    private void RotateWheel(Transform wheel, float speed)
    {
        // Jeœli to prawe ko³o, odbij kierunek rotacji
        if (wheel == frontRightWheel || wheel == backRightWheel)
        {
            wheel.Rotate(Vector3.back, -speed * Time.deltaTime);  // Obrót w przeciwn¹ stronê
        }
        else
        {
            wheel.Rotate(Vector3.back, speed * Time.deltaTime);  // Standardowy obrót
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
        foreach (var item in moveObjects)
        {
            if (item != null && !item.CompareTag("Player")) // SprawdŸ, czy obiekt istnieje i nie jest graczem
            {
                StartCoroutine(Move(item, direction));
            }
        }
    }

    IEnumerator Move(GameObject item, Vector3 direction)
    {
        Vector3 startPosition = item.transform.position;
        Vector3 endPosition = startPosition + direction * moveDistance;
        float elapsedTime = 0;

        while (elapsedTime < moveDuration)
        {
            item.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        item.transform.position = endPosition;
        //Debug.Log($"Moved {item.name} in direction {direction} by {moveDistance} units.");
    }
}
