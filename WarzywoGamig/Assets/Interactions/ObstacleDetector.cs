using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ObstacleDetector : MonoBehaviour
{
    public Image warningImage; // Obraz ostrzegawczy na UI
    public float blinkDuration = 0.5f; // Czas migania
    private bool obstacleDetected = false;
    private Coroutine blinkCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Contains("Obstacle")) // Sprawdza nazw� obiektu
        {
            obstacleDetected = true;
            if (blinkCoroutine == null)
                blinkCoroutine = StartCoroutine(BlinkWarning());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name.Contains("Obstacle")) // Sprawdza nazw� obiektu
        {
            obstacleDetected = false;
        }
    }

    IEnumerator BlinkWarning()
    {
        while (obstacleDetected)
        {
            warningImage.color = Color.yellow; // Zmiana na ��ty
            yield return new WaitForSeconds(blinkDuration);
            warningImage.color = Color.clear; // Ukrycie obrazu
            yield return new WaitForSeconds(blinkDuration);
        }

        warningImage.color = Color.clear; // Upewnienie si�, �e wr�ci do domy�lnego stanu
        blinkCoroutine = null;
    }
}
