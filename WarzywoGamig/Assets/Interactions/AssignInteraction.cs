using UnityEngine;

public class AssignInteraction : MonoBehaviour
{
    public GameObject interactableLeft; // Przedmiot interaktywny 1
    public GameObject interactableRight; // Przedmiot interaktywny 2
    public GameObject[] moveObjects; // Zbiór przedmiotów do przenoszenia w lewo
    public float moveDistance = 1.0f; // Odleg³oœæ przenoszenia obiektów
    public float moveDuration = 1.0f; // Czas trwania przenoszenia obiektów

    void Start()
    {
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

    void MoveAll(Vector3 direction)
    {
        foreach (var item in moveObjects)
        {
            if (!item.CompareTag("Player"))
            {
                StartCoroutine(Move(item, direction));
            }
        }
    }

    System.Collections.IEnumerator Move(GameObject item, Vector3 direction)
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
