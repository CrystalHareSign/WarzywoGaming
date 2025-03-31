using System.Collections.Generic;
using UnityEngine;

public class AudioChanger : MonoBehaviour
{
    // Lista colliderów przypisanych do strefy Inside
    public List<Collider> zoneInsideColliders = new List<Collider>();

    // Lista colliderów przypisanych do strefy Outside
    public List<Collider> zoneOutsideColliders = new List<Collider>();

    // Lista wszystkich obiektów, które posiadaj¹ PlaySoundOnObject
    private List<PlaySoundOnObject> playSoundObjects = new List<PlaySoundOnObject>();

    // Metoda wywo³ywana, gdy gracz wchodzi w strefê (przez OnTriggerEnter)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))  // Sprawdzamy, czy to gracz
        {
            // Sprawdzamy, czy gracz wchodzi do strefy Inside
            foreach (var zoneCollider in zoneInsideColliders)
            {
                if (other == zoneCollider) // Gracz wszed³ w collider strefy Inside
                {
                    Debug.Log("Gracz wszed³ do strefy Inside");

                    foreach (var playSoundOnObject in playSoundObjects)
                    {
                        if (playSoundOnObject == null) continue;

                        playSoundOnObject.ChangePitch("Storm", 0.7f);
                        Debug.Log("Zmieniamy pitch dla obiektu w strefie Inside na: 0.7f");
                    }
                }
            }

            // Sprawdzamy, czy gracz wchodzi do strefy Outside
            foreach (var zoneCollider in zoneOutsideColliders)
            {
                if (other == zoneCollider) // Gracz wszed³ w collider strefy Outside
                {
                    Debug.Log("Gracz wszed³ do strefy Outside");

                    foreach (var playSoundOnObject in playSoundObjects)
                    {
                        if (playSoundOnObject == null) continue;

                        playSoundOnObject.ChangePitch("Storm", 1.0f);
                        Debug.Log("Zmieniamy pitch dla obiektu w strefie Outside na: 1.0f");
                    }
                }
            }
        }
    }
}
