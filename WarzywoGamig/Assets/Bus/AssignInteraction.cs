using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AssignInteraction : MonoBehaviour
{
    public GameObject interactableLeft; // Przedmiot interaktywny 1
    public GameObject interactableRight; // Przedmiot interaktywny 2
    [Header("TAGS: Loot, Item, Weapon, Turret")]
    [Header("TAGS: Body")]
    public GameObject[] manualMoveObjects; // R�cznie przypisane obiekty
    private List<GameObject> moveObjects = new List<GameObject>(); // Lista przedmiot�w do przenoszenia
    private int lastLootCount = 0;
    public float moveDistance = 1.0f; // Odleg�o�� przenoszenia obiekt�w
    public float moveDuration = 1.0f; // Czas trwania przenoszenia obiekt�w

    public WheelManager wheelManager;

    void Start()
    {
        // Dodaj r�cznie przypisane obiekty do listy
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

        // Przypisz funkcje do interaktywnych przedmiot�w
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
        GameObject[] foundLootObjects = GameObject.FindGameObjectsWithTag("Loot");
        if (foundLootObjects.Length != lastLootCount) // Sprawdza, czy liczba obiekt�w z tagiem "Loot" si� zmieni�a
        {
            AddObjectsWithTag("Loot");
            lastLootCount = foundLootObjects.Length; // Aktualizuje zapisan� liczb� obiekt�w
        }
    }

    void AddObjectsWithTag(string tag)
    {
        GameObject[] foundObjects = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in foundObjects)
        {
            if (obj != null && !moveObjects.Contains(obj)) // Unikamy duplikat�w
            {
                moveObjects.Add(obj);
            }
        }
    }

    void MoveAll(Vector3 direction)
    {
        foreach (var item in moveObjects)
        {
            if (item != null && !item.CompareTag("Player")) // Sprawd�, czy obiekt istnieje i nie jest graczem
            {
                StartCoroutine(Move(item, direction));
            }
        }

        // Przeka� kierunek i czas ruchu do WheelManager
        if (wheelManager != null)
        {
            wheelManager.StartSteering(direction, moveDuration);
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
