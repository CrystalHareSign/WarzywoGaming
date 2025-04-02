using UnityEngine;
using System.Collections.Generic;

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

    public float stormAudioVolume;
    public float transitionSpeed = 1f;

    private float targetCutoff;
    private float targetVolume;
    private float currentCutoff;
    private float currentVolume;

    public List<Transform> soundAffectingObjects = new List<Transform>();
    public float MaxDistance = 10f;
    public float MinDistance = 0f;
    public float CurrentDistance = 0f;

    private Transform playerTransform;

    private void Start()
    {
        if (stormAudio != null)
        {
            outdoorVolume = stormAudio.volume;
            currentVolume = stormAudio.volume;
        }

        triggerCollider = GetComponent<Collider>();

        currentCutoff = lowPassFilter.cutoffFrequency;
        targetCutoff = outdoorCutoff;
        targetVolume = outdoorVolume * stormAudioVolume;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void Update()
    {
        if (isPlayerInside && playerTransform != null)
        {
            CurrentDistance = GetClosestDistanceToObjects();

            // Im wiêksza odleg³oœæ, tym bardziej wyciszony i wyt³umiony dŸwiêk
            float distanceFactor = Mathf.InverseLerp(MinDistance, MaxDistance, CurrentDistance);
            targetCutoff = Mathf.Lerp(outdoorCutoff, indoorCutoff, distanceFactor);
            targetVolume = Mathf.Lerp(outdoorVolume * stormAudioVolume, outdoorVolume * indoorVolumeMultiplier * stormAudioVolume, distanceFactor);
        }
        else
        {
            targetCutoff = outdoorCutoff;
            targetVolume = outdoorVolume * stormAudioVolume;
        }

        currentCutoff = Mathf.MoveTowards(currentCutoff, targetCutoff, transitionSpeed * Time.deltaTime * Mathf.Abs(targetCutoff - currentCutoff));
        currentVolume = Mathf.MoveTowards(currentVolume, targetVolume, transitionSpeed * Time.deltaTime);

        if (lowPassFilter != null)
        {
            lowPassFilter.cutoffFrequency = currentCutoff;
        }

        if (stormAudio != null)
        {
            stormAudio.volume = currentVolume;
        }
    }

    private float GetClosestDistanceToObjects()
    {
        float minDistance = MaxDistance;

        foreach (Transform obj in soundAffectingObjects)
        {
            if (obj != null)
            {
                float distance = Vector3.Distance(playerTransform.position, obj.position);
                minDistance = Mathf.Min(minDistance, distance);
            }
        }

        return Mathf.Clamp(minDistance, MinDistance, MaxDistance);
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
