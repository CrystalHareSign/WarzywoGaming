using UnityEngine;
using UnityEngine.UI;

public class ObstacleDetector : MonoBehaviour
{
    public Image[] warningImages;  // Obrazy UI przypisane do detektora
    public ObstacleDetectorManager detectorManager; // Odwo�anie do mened�era detektora (odleg�o�ci i referencje)
    private Transform detectedObstacle;
    private bool isBlinking = false; // Flaga informuj�ca, czy miganie jest aktywne
    private float blinkTimer = 0f;  // Czas do nast�pnej zmiany stanu migania

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle")) // Sprawdza, czy przeszkoda posiada tag "Obstacle"
        {
            //Debug.Log("Przeszkoda wykryta: " + other.name);  // Logujemy wykryt� przeszkod�
            detectedObstacle = other.transform;
            // Uruchamiamy sprawdzanie odleg�o�ci co updateFrequency sekund
            InvokeRepeating("UpdateDistanceAndUI", 0f, detectorManager.updateFrequency);
            isBlinking = true; // Rozpoczynamy miganie
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Obstacle")) // Sprawdza, czy przeszkoda posiada tag "Obstacle"
        {
            //Debug.Log("Przeszkoda opu�ci�a detektor: " + other.name);  // Logujemy opuszczenie detektora

            // Resetujemy stan tylko wtedy, gdy wyjdzie ostatnia przeszkoda z obszaru
            if (detectedObstacle == other.transform)
            {
                detectedObstacle = null;
                ResetWarningImages();           // Resetujemy obrazy UI
                CancelInvoke("UpdateDistanceAndUI"); // Zatrzymujemy okresowe wywo�ywanie sprawdzania odleg�o�ci
                isBlinking = false; // Zatrzymujemy miganie
            }
        }
    }

    private void Update()
    {
        if (detectedObstacle != null)
        {
            float distance = Vector3.Distance(transform.position, detectedObstacle.position);
            UpdateWarningUI(distance); // Zaktualizuj UI na podstawie odleg�o�ci

            if (isBlinking)
            {
                blinkTimer += Time.deltaTime; // Zwi�kszamy timer
                if (blinkTimer >= detectorManager.blinkFrequency)
                {
                    ToggleWarningImages(); // Zmiana stanu migania UI
                    blinkTimer = 0f; // Resetujemy timer
                }
            }
        }
        else
        {
            //Debug.Log("Detektor nie wykrywa �adnej przeszkody.");  // Logujemy, kiedy detektor nie ma przeszkody
        }
    }

    // Ta metoda b�dzie wywo�ywana co updateFrequency sekund, aby zaktualizowa� UI
    private void UpdateDistanceAndUI()
    {
        if (detectedObstacle != null)
        {
            float distance = Vector3.Distance(transform.position, detectedObstacle.position);
            UpdateWarningUI(distance); // Zaktualizuj UI na podstawie odleg�o�ci
        }
    }

    // Zaktualizuj UI dla tego detektora
    private void UpdateWarningUI(float distance)
    {
        Color warningColor = detectorManager.GetWarningColor(distance);

        // Zaktualizuj ka�dy obraz UI
        foreach (var img in warningImages)
        {
            img.color = warningColor;  // Ustaw kolor na podstawie odleg�o�ci
        }
    }

    // Prze��czamy stan widoczno�ci obraz�w UI
    private void ToggleWarningImages()
    {
        foreach (var img in warningImages)
        {
            img.enabled = !img.enabled;  // Prze��czamy stan widoczno�ci obrazu
        }
    }

    // Resetowanie wszystkich obraz�w UI
    private void ResetWarningImages()
    {
        foreach (var img in warningImages)
        {
            img.enabled = false;  // Wy��cza wszystkie obrazy
        }
    }
}
