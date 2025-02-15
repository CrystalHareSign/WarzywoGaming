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
        if (other.gameObject.name.Contains("Obstacle")) // Sprawdza nazwê obiektu
        {
            obstacleDetected = true;
            if (blinkCoroutine == null)
                blinkCoroutine = StartCoroutine(BlinkWarning());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name.Contains("Obstacle")) // Sprawdza nazwê obiektu
        {
            obstacleDetected = false;
        }
    }

    IEnumerator BlinkWarning()
    {
        while (obstacleDetected)
        {
            warningImage.color = Color.yellow; // Zmiana na ¿ó³ty
            yield return new WaitForSeconds(blinkDuration);
            warningImage.color = Color.clear; // Ukrycie obrazu
            yield return new WaitForSeconds(blinkDuration);
        }

        warningImage.color = Color.clear; // Upewnienie siê, ¿e wróci do domyœlnego stanu
        blinkCoroutine = null;
    }
}
