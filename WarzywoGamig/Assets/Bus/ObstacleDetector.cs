using UnityEngine;
using UnityEngine.UI;

public class ObstacleDetector : MonoBehaviour
{
    public Image[] warningImages;  // Obrazy UI przypisane do detektora
    public ObstacleDetectorManager detectorManager; // Odwo³anie do mened¿era detektora (odleg³oœci i referencje)
    private Transform detectedObstacle;
    private bool isBlinking = false; // Flaga informuj¹ca, czy miganie jest aktywne
    private float blinkTimer = 0f;  // Czas do nastêpnej zmiany stanu migania

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle")) // Sprawdza, czy przeszkoda posiada tag "Obstacle"
        {
            //Debug.Log("Przeszkoda wykryta: " + other.name);  // Logujemy wykryt¹ przeszkodê
            detectedObstacle = other.transform;
            // Uruchamiamy sprawdzanie odleg³oœci co updateFrequency sekund
            InvokeRepeating("UpdateDistanceAndUI", 0f, detectorManager.updateFrequency);
            isBlinking = true; // Rozpoczynamy miganie
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Obstacle")) // Sprawdza, czy przeszkoda posiada tag "Obstacle"
        {
            //Debug.Log("Przeszkoda opuœci³a detektor: " + other.name);  // Logujemy opuszczenie detektora

            // Resetujemy stan tylko wtedy, gdy wyjdzie ostatnia przeszkoda z obszaru
            if (detectedObstacle == other.transform)
            {
                detectedObstacle = null;
                ResetWarningImages();           // Resetujemy obrazy UI
                CancelInvoke("UpdateDistanceAndUI"); // Zatrzymujemy okresowe wywo³ywanie sprawdzania odleg³oœci
                isBlinking = false; // Zatrzymujemy miganie
            }
        }
    }

    private void Update()
    {
        if (detectedObstacle != null)
        {
            float distance = Vector3.Distance(transform.position, detectedObstacle.position);
            UpdateWarningUI(distance); // Zaktualizuj UI na podstawie odleg³oœci

            if (isBlinking)
            {
                blinkTimer += Time.deltaTime; // Zwiêkszamy timer
                if (blinkTimer >= detectorManager.blinkFrequency)
                {
                    ToggleWarningImages(); // Zmiana stanu migania UI
                    blinkTimer = 0f; // Resetujemy timer
                }
            }
        }
        else
        {
            //Debug.Log("Detektor nie wykrywa ¿adnej przeszkody.");  // Logujemy, kiedy detektor nie ma przeszkody
        }
    }

    // Ta metoda bêdzie wywo³ywana co updateFrequency sekund, aby zaktualizowaæ UI
    private void UpdateDistanceAndUI()
    {
        if (detectedObstacle != null)
        {
            float distance = Vector3.Distance(transform.position, detectedObstacle.position);
            UpdateWarningUI(distance); // Zaktualizuj UI na podstawie odleg³oœci
        }
    }

    // Zaktualizuj UI dla tego detektora
    private void UpdateWarningUI(float distance)
    {
        Color warningColor = detectorManager.GetWarningColor(distance);

        // Zaktualizuj ka¿dy obraz UI
        foreach (var img in warningImages)
        {
            img.color = warningColor;  // Ustaw kolor na podstawie odleg³oœci
        }
    }

    // Prze³¹czamy stan widocznoœci obrazów UI
    private void ToggleWarningImages()
    {
        foreach (var img in warningImages)
        {
            img.enabled = !img.enabled;  // Prze³¹czamy stan widocznoœci obrazu
        }
    }

    // Resetowanie wszystkich obrazów UI
    private void ResetWarningImages()
    {
        foreach (var img in warningImages)
        {
            img.enabled = false;  // Wy³¹cza wszystkie obrazy
        }
    }
}
