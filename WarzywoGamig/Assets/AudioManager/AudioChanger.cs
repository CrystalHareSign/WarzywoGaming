using UnityEngine;

public class AudioChanger : MonoBehaviour
{
    public AudioLowPassFilter lowPassFilter;
    public AudioSource stormAudio;
    public float indoorCutoff = 1000f;
    public float outdoorCutoff = 22000f;
    public float indoorVolumeMultiplier = 0.3f;
    private float outdoorVolume;

    private bool hasEntered = false;
    private bool isPlayerInside = false;

    private Collider triggerCollider;

    // G³oœnoœæ przekazywana z SceneManagera
    public float stormAudioVolume;

    // Parametry p³ynnych zmian
    public float transitionSpeed = 1f;  // Szybkoœæ przejœcia (od 0 do 1)

    private float targetCutoff;
    private float targetVolume;
    private float currentCutoff;
    private float currentVolume;

    private void Start()
    {
        if (stormAudio != null)
        {
            outdoorVolume = stormAudio.volume;
            currentVolume = stormAudio.volume; // Pocz¹tkowa g³oœnoœæ
        }

        triggerCollider = GetComponent<Collider>();

        // Ustawiamy pocz¹tkowy cutoff
        currentCutoff = lowPassFilter.cutoffFrequency;
        targetCutoff = outdoorCutoff; // Pocz¹tkowa wartoœæ
        targetVolume = outdoorVolume * stormAudioVolume;
    }

    private void Update()
    {
        // Jeœli gracz jest w œrodku, ustawiamy docelowe wartoœci
        if (isPlayerInside)
        {
            targetCutoff = indoorCutoff;
            targetVolume = outdoorVolume * indoorVolumeMultiplier * stormAudioVolume;
        }
        else
        {
            targetCutoff = outdoorCutoff;
            targetVolume = outdoorVolume * stormAudioVolume;
        }

        // P³ynna zmiana cutoffFrequency - kontrolowanie zakresu
        currentCutoff = Mathf.MoveTowards(currentCutoff, targetCutoff, transitionSpeed * Time.deltaTime * Mathf.Abs(targetCutoff - currentCutoff));

        // P³ynna zmiana g³oœnoœci
        currentVolume = Mathf.MoveTowards(currentVolume, targetVolume, transitionSpeed * Time.deltaTime);

        // Ustawiamy wartoœci na podstawie p³ynnych przejœæ
        if (lowPassFilter != null)
        {
            lowPassFilter.cutoffFrequency = currentCutoff;
        }

        if (stormAudio != null)
        {
            stormAudio.volume = currentVolume;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasEntered)
        {
            hasEntered = true;
            isPlayerInside = true;
            Debug.Log("Gracz wszed³ do wnêtrza.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            hasEntered = false;
            isPlayerInside = false;
            Debug.Log("Gracz wyszed³ na zewn¹trz.");
        }
    }
}
