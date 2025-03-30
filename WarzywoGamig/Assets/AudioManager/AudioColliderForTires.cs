using UnityEngine;

public class AudioColliderForTires : MonoBehaviour
{
    [SerializeField] private Collider leftTireCollider;
    [SerializeField] private Collider rightTireCollider;

    // Zamiast string�w, deklarujemy AudioSource
    public PlaySoundOnObject playSoundOnObjectLP;
    public PlaySoundOnObject playSoundOnObjectLT;
    public PlaySoundOnObject playSoundOnObjectPP;
    public PlaySoundOnObject playSoundOnObjectPT;

    private void Start()
    {
        //// Tu przypisujesz konkretne instancje PlaySoundOnObject, np. przez GetComponent
        //playSoundOnObjectLP = Object.FindFirstObjectByType<PlaySoundOnObject>();  // Przypisz odpowiedni� instancj�
        //playSoundOnObjectLT = Object.FindFirstObjectByType<PlaySoundOnObject>();
        //playSoundOnObjectPP = Object.FindFirstObjectByType<PlaySoundOnObject>();
        //playSoundOnObjectPT = Object.FindFirstObjectByType<PlaySoundOnObject>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))  // Sprawdzamy, czy obiekt wchodz�cy w trigger to gracz
        {

            if (other == leftTireCollider)
            {
                Debug.Log("Gracz wszed� w lewy collider");
                playSoundOnObjectLP.PlaySound("TiresOnGravel", 1f, true);
                playSoundOnObjectLT.PlaySound("TiresOnGravel", 1f, true);
            }
            else if (other == rightTireCollider)
            {
                Debug.Log("Gracz wszed� w prawy collider");
                playSoundOnObjectPP.PlaySound("TiresOnGravel", 1f, true);
                playSoundOnObjectPT.PlaySound("TiresOnGravel", 1f, true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))  // Sprawdzamy, czy obiekt opuszczaj�cy trigger to gracz
        {

            if (other == leftTireCollider)
            {
                Debug.Log("Gracz opu�ci� lewy collider");
                playSoundOnObjectLP.StopSound("TiresOnGravel");
                playSoundOnObjectLT.StopSound("TiresOnGravel");
            }
            else if (other == rightTireCollider)
            {
                Debug.Log("Gracz opu�ci� prawy collider");
                playSoundOnObjectPP.StopSound("TiresOnGravel");
                playSoundOnObjectPT.StopSound("TiresOnGravel");
            }
        }
    }
}
