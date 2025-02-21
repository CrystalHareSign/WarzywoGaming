using UnityEngine;

public class ObstacleDetectorManager : MonoBehaviour
{
    [Header("Distance Settings")]
    [Tooltip("Odleg�o��, przy kt�rej kolor to czerwony")]
    public float redDistance = 1.5f;   // Odleg�o��, przy kt�rej kolor to czerwony
    [Tooltip("Odleg�o��, przy kt�rej kolor to pomara�czowy")]
    public float orangeDistance = 3f;  // Odleg�o��, przy kt�rej kolor to pomara�czowy
    [Tooltip("Odleg�o��, przy kt�rej kolor to ��ty")]
    public float yellowDistance = 5f;  // Odleg�o��, przy kt�rej kolor to ��ty

    [Header("Blink Settings")]
    [Tooltip("Cz�stotliwo�� migania UI w sekundach")]
    public float blinkFrequency = 0.5f;  // Cz�stotliwo�� migania UI (w sekundach)

    [Tooltip("Cz�stotliwo�� sprawdzania odleg�o�ci i aktualizacji UI w sekundach.")]
    public float updateFrequency = 0.5f;  // Cz�stotliwo�� wywo�ywania UpdateDistanceAndUI w sekundach (mo�na edytowa� w Inspektorze)

    // Metoda do uzyskania koloru ostrze�enia na podstawie odleg�o�ci
    public Color GetWarningColor(float distance)
    {
        if (distance < redDistance)
        {
            return Color.red;  // Kolor czerwony, je�li bardzo blisko
        }
        else if (distance < orangeDistance)
        {
            return new Color(1f, 0.5f, 0f);  // Kolor pomara�czowy, je�li �rednia odleg�o��
        }
        else
        {
            return Color.yellow;  // Kolor ��ty, je�li daleko
        }
    }
}
