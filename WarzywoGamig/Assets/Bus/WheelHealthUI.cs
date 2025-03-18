using UnityEngine;
using UnityEngine.UI;

public class WheelHealthUI : MonoBehaviour
{
    public Image[] wheelImages; // Obrazy reprezentuj�ce zdrowie k�
    public Color colorHealth0 = Color.red;
    public Color colorHealth1 = new Color(1f, 0.5f, 0f); // Pomara�czowy
    public Color colorHealth2 = Color.green;

    private void Start()
    {
        // Sprawdzamy, czy wszystkie obrazy s� przypisane
        for (int i = 0; i < wheelImages.Length; i++)
        {
            if (wheelImages[i] == null)
            {
                //Debug.LogError($"[ERROR] WheelHealthUI: Brak przypisanego obrazu dla ko�a {i}!");
            }
            else
            {
                //Debug.Log($"WheelHealthUI: Ko�o {i} przypisane do obrazu {wheelImages[i].name}");
            }
        }
    }

    public void UpdateWheelHealth(int wheelIndex, int health)
    {
        if (wheelIndex < 0 || wheelIndex >= wheelImages.Length)
        {
            Debug.LogError($"[ERROR] WheelHealthUI: Nieprawid�owy indeks ko�a: {wheelIndex}");
            return;
        }

        if (wheelImages[wheelIndex] == null)
        {
            Debug.LogError($"[ERROR] WheelHealthUI: Obraz dla ko�a {wheelIndex} jest NULL!");
            return;
        }

        // Wyb�r odpowiedniego koloru na podstawie zdrowia
        Color newColor = health switch
        {
            0 => colorHealth0,
            1 => colorHealth1,
            2 => colorHealth2,
            _ => Color.black // Dla bezpiecze�stwa
        };

        // Zmiana koloru obrazu
        wheelImages[wheelIndex].color = newColor;
        //Debug.Log($"WheelHealthUI: Zmieniono kolor ko�a {wheelIndex} na {newColor} (zdrowie: {health})");
    }
}
