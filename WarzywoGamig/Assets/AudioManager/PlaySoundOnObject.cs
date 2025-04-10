using System.Collections.Generic;
using UnityEngine;

public class PlaySoundOnObject : MonoBehaviour
{
    [System.Serializable]
    public class AudioSourceWithName
    {
        public AudioSource audioSource;
        public string soundName;
        public AudioManager.SoundType soundType;
    }

    public List<AudioSourceWithName> audioSourcesWithNames = new List<AudioSourceWithName>();

    // Sprawdzanie poprawnoœci listy audio sources
    private void CheckAudioSourceList()
    {
        if (audioSourcesWithNames == null || audioSourcesWithNames.Count == 0)
        {
            Debug.LogWarning($"Brak AudioSource w obiekcie {gameObject.name}!");
        }
    }

    private AudioSourceWithName GetAudioSourceByName(string soundName)
    {
        return audioSourcesWithNames.Find(x => x.soundName == soundName);
    }

    public void PlaySound(string soundName, float volume = 1f, bool loop = false)
    {
        CheckAudioSourceList();

        var foundSource = GetAudioSourceByName(soundName);
        if (foundSource == null || foundSource.audioSource == null || foundSource.audioSource.clip == null)
        {
            Debug.LogWarning($"Brak dŸwiêku '{soundName}' w obiekcie {gameObject.name}!");
            return;
        }

        // Pocz¹tkowa g³oœnoœæ
        float finalVolume = volume;
        switch (foundSource.soundType)
        {
            case AudioManager.SoundType.Music:
                finalVolume *= AudioManager.Instance.masterMusicVolume;
                break;
            case AudioManager.SoundType.SFX:
                finalVolume *= AudioManager.Instance.masterSFXVolume;
                break;
            case AudioManager.SoundType.Ambient:
                finalVolume *= AudioManager.Instance.masterAmbientVolume;
                break;
        }

        // Ustawienie pocz¹tkowej g³oœnoœci
        foundSource.audioSource.loop = loop;
        foundSource.audioSource.volume = finalVolume;

        // Tylko SFX u¿ywa PlayOneShot
        if (foundSource.soundType == AudioManager.SoundType.SFX)
        {
            foundSource.audioSource.PlayOneShot(foundSource.audioSource.clip, finalVolume);
            Debug.Log($"Odtworzono dŸwiêk SFX '{soundName}' na obiekcie {gameObject.name}.");
        }
        else
        {
            // Dla muzyki i ambientu u¿ywamy Play z ustawion¹ pêtl¹
            if (!foundSource.audioSource.isPlaying)
            {
                foundSource.audioSource.Play();
                Debug.Log($"Odtworzono dŸwiêk '{soundName}' w obiekcie {gameObject.name}.");
            }
        }
    }

    private void Update()
    {
        // Przechodzimy przez wszystkie AudioSource w obiekcie
        var allAudioSources = GetComponentsInChildren<AudioSource>();

        foreach (var audioSource in allAudioSources)
        {
            // Jeœli dŸwiêk jest odtwarzany, dostosowujemy jego g³oœnoœæ
            if (audioSource.isPlaying)
            {
                // Finalna g³oœnoœæ na podstawie typu dŸwiêku
                float finalVolume = audioSource.volume;
                if (audioSource.clip != null)
                {
                    AudioManager.SoundType soundType = AudioManager.SoundType.SFX; // Domyœlnie przypisujemy SFX
                    switch (soundType)
                    {
                        case AudioManager.SoundType.Music:
                            finalVolume = AudioManager.Instance.masterMusicVolume;
                            break;
                        case AudioManager.SoundType.SFX:
                            finalVolume = AudioManager.Instance.masterSFXVolume;
                            break;
                        case AudioManager.SoundType.Ambient:
                            finalVolume = AudioManager.Instance.masterAmbientVolume;
                            break;
                    }

                    // Przypisanie nowej g³oœnoœci
                    audioSource.volume = finalVolume;
                }
            }
        }
    }


    public void StopSound(string soundName)
    {
        CheckAudioSourceList();

        var foundSource = GetAudioSourceByName(soundName);
        if (foundSource == null || foundSource.audioSource == null)
        {
            Debug.LogWarning($"DŸwiêk '{soundName}' nie znaleziony lub audioSource jest null.");
            return;
        }

        if (foundSource.audioSource.isPlaying)
        {
            foundSource.audioSource.Stop();
            Debug.Log($"Zatrzymano dŸwiêk '{soundName}' w obiekcie {gameObject.name}.");
        }
    }

    public void StopAllSounds()
    {
        CheckAudioSourceList();

        foreach (var source in audioSourcesWithNames)
        {
            if (source.audioSource != null && source.audioSource.isPlaying)
            {
                source.audioSource.Stop();
            }
        }
    }
}
