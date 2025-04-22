using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AssignInteraction : MonoBehaviour
{
    public GameObject interactableLeft; // Przedmiot interaktywny 1
    public GameObject interactableRight; // Przedmiot interaktywny 2

    [Header("TAGS: Loot, Item, Weapon, Turret")]
    public GameObject[] manualMoveObjects; // Rêcznie przypisane obiekty

    private List<GameObject> moveObjects = new List<GameObject>(); // Lista przedmiotów do przenoszenia

    private int lastLootCount = 0;
    public float moveDistance = 1.0f; // Odleg³oœæ przenoszenia obiektów
    public float moveDuration = 1.0f; // Czas trwania przenoszenia obiektów
    public bool isMoving = false; // Flaga informuj¹ca, czy obiekt jest w trakcie ruchu

    public WheelManager wheelManager;

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

        // Pobierz obiekty z tagami
        AddObjectsWithTag("Item");
        AddObjectsWithTag("Turret");

        SetupInteractable(interactableLeft, Vector3.forward);
        SetupInteractable(interactableRight, Vector3.back);
    }

    private void Update()
    {
        // Sprawdzenie zmiany iloœci obiektów z tagiem "Loot"
        GameObject[] foundLootObjects = GameObject.FindGameObjectsWithTag("Loot");
        if (foundLootObjects.Length != lastLootCount)
        {
            AddObjectsWithTag("Loot");
            lastLootCount = foundLootObjects.Length;
        }
    }

    private void SetupInteractable(GameObject interactable, Vector3 direction)
    {
        if (interactable != null)
        {
            InteractableItem item = interactable.GetComponent<InteractableItem>();
            if (item != null)
            {
                item.hasCooldown = true;
                item.onInteract = () => MoveAll(direction);
            }
            else
            {
                Debug.LogWarning($"{interactable.name} nie ma komponentu InteractableItem!");
            }
        }
    }

    void AddObjectsWithTag(string tag)
    {
        GameObject[] foundObjects = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in foundObjects)
        {
            if (obj != null && !moveObjects.Contains(obj))
            {
                moveObjects.Add(obj);
            }
        }
    }

    void MoveAll(Vector3 direction)
    {
        foreach (var item in moveObjects)
        {
            if (item != null && !item.CompareTag("Player"))
            {
                StartCoroutine(Move(item, direction));
            }
        }

        if (wheelManager != null)
        {
            wheelManager.StartSteering(direction, moveDuration);
        }
    }

    IEnumerator Move(GameObject item, Vector3 direction)
    {
        if (item == null)
        {
            yield break;
        }

        Vector3 startPosition = item.transform.position;
        Vector3 endPosition = startPosition + direction * moveDistance;
        float elapsedTime = 0;

        while (elapsedTime < moveDuration)
        {
            if (item == null)
            {
                yield break;
            }

            item.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (item != null)
        {
            item.transform.position = endPosition;
        }
    }
}
