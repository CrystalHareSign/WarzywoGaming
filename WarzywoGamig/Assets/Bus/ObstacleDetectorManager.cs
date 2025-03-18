using UnityEngine;

public class ObstacleDetectorManager : MonoBehaviour
{
    [Header("Distance Settings")]
    [Tooltip("Odleg³oœæ, przy której kolor to czerwony")]
    public float redDistance = 1.5f;   // Odleg³oœæ, przy której kolor to czerwony
    [Tooltip("Odleg³oœæ, przy której kolor to pomarañczowy")]
    public float orangeDistance = 3f;  // Odleg³oœæ, przy której kolor to pomarañczowy
    [Tooltip("Odleg³oœæ, przy której kolor to ¿ó³ty")]
    public float yellowDistance = 5f;  // Odleg³oœæ, przy której kolor to ¿ó³ty

    [Header("Blink Settings")]
    [Tooltip("Czêstotliwoœæ migania UI w sekundach")]
    public float blinkFrequency = 0.5f;  // Czêstotliwoœæ migania UI (w sekundach)

    [Tooltip("Czêstotliwoœæ sprawdzania odleg³oœci i aktualizacji UI w sekundach.")]
    public float updateFrequency = 0.5f;  // Czêstotliwoœæ wywo³ywania UpdateDistanceAndUI w sekundach (mo¿na edytowaæ w Inspektorze)

    // Metoda do uzyskania koloru ostrze¿enia na podstawie odleg³oœci
    public Color GetWarningColor(float distance)
    {
        if (distance < redDistance)
        {
            return Color.red;  // Kolor czerwony, jeœli bardzo blisko
        }
        else if (distance < orangeDistance)
        {
            return new Color(1f, 0.5f, 0f);  // Kolor pomarañczowy, jeœli œrednia odleg³oœæ
        }
        else
        {
            return Color.yellow;  // Kolor ¿ó³ty, jeœli daleko
        }
    }
}
