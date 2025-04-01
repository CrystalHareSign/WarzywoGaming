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

    // G�o�no�� przekazywana z SceneManagera
    public float stormAudioVolume;

    // Parametry p�ynnych zmian
    public float transitionSpeed = 1f;  // Szybko�� przej�cia (od 0 do 1)

    private float targetCutoff;
    private float targetVolume;
    private float currentCutoff;
    private float currentVolume;

    private void Start()
    {
        if (stormAudio != null)
        {
            outdoorVolume = stormAudio.volume;
            currentVolume = stormAudio.volume; // Pocz�tkowa g�o�no��
        }

        triggerCollider = GetComponent<Collider>();

        // Ustawiamy pocz�tkowy cutoff
        currentCutoff = lowPassFilter.cutoffFrequency;
        targetCutoff = outdoorCutoff; // Pocz�tkowa warto��
        targetVolume = outdoorVolume * stormAudioVolume;
    }

    private void Update()
    {
        // Je�li gracz jest w �rodku, ustawiamy docelowe warto�ci
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

        // P�ynna zmiana cutoffFrequency - kontrolowanie zakresu
        currentCutoff = Mathf.MoveTowards(currentCutoff, targetCutoff, transitionSpeed * Time.deltaTime * Mathf.Abs(targetCutoff - currentCutoff));

        // P�ynna zmiana g�o�no�ci
        currentVolume = Mathf.MoveTowards(currentVolume, targetVolume, transitionSpeed * Time.deltaTime);

        // Ustawiamy warto�ci na podstawie p�ynnych przej��
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
            Debug.Log("Gracz wszed� do wn�trza.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            hasEntered = false;
            isPlayerInside = false;
            Debug.Log("Gracz wyszed� na zewn�trz.");
        }
    }
}
