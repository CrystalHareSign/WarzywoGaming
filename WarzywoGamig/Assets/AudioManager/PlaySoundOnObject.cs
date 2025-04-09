using UnityEngine;
using System.Collections.Generic;

public class PlaySoundOnObject : MonoBehaviour
{
    [System.Serializable]
    public class AudioSourceWithName
    {
        public AudioSource audioSource;
        public string soundName;
        public AudioManager.SoundType soundType;
        public bool isPlaying;
    }

    public List<AudioSourceWithName> audioSourcesWithNames = new List<AudioSourceWithName>();
    private List<AudioSourceWithName> activeAudioSources = new List<AudioSourceWithName>();

    // Sprawdzanie poprawno�ci listy audio sources
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
            Debug.LogWarning($"Brak d�wi�ku '{soundName}' w obiekcie {gameObject.name}!");
            return;
        }

        // Uwzgl�dnienie aktualnej g�o�no�ci zale�nej od typu d�wi�ku
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

        foundSource.audioSource.loop = loop;
        foundSource.audioSource.volume = finalVolume;

        if (loop)
        {
            if (!foundSource.audioSource.isPlaying)
            {
                foundSource.audioSource.Play();
                activeAudioSources.Add(foundSource);  // Dodajemy do aktywnych �r�de�
                Debug.Log($"Odtworzono p�tl� d�wi�ku '{soundName}' w obiekcie {gameObject.name}.");
            }
        }
        else
        {
            foundSource.audioSource.PlayOneShot(foundSource.audioSource.clip, finalVolume);
            activeAudioSources.Add(foundSource);  // Dodajemy do aktywnych �r�de�
            Debug.Log($"Odtworzono d�wi�k '{soundName}' na obiekcie {gameObject.name}.");
        }
    }

    // Metoda do dynamicznej zmiany g�o�no�ci dla aktywnych d�wi�k�w
    public void UpdateActiveSoundsVolume()
    {
        foreach (var activeSource in activeAudioSources)
        {
            if (activeSource.isPlaying)
            {
                float finalVolume = 0f;
                switch (activeSource.soundType)
                {
                    case AudioManager.SoundType.Music:
                        finalVolume = activeSource.audioSource.volume * AudioManager.Instance.masterMusicVolume;
                        break;
                    case AudioManager.SoundType.SFX:
                        finalVolume = activeSource.audioSource.volume * AudioManager.Instance.masterSFXVolume;
                        break;
                    case AudioManager.SoundType.Ambient:
                        finalVolume = activeSource.audioSource.volume * AudioManager.Instance.masterAmbientVolume;
                        break;
                }
                activeSource.audioSource.volume = finalVolume; // Zaktualizowanie g�o�no�ci
            }
        }
    }

    public void StopSound(string soundName)
    {
        CheckAudioSourceList();

        var foundSource = GetAudioSourceByName(soundName);
        if (foundSource == null || foundSource.audioSource == null)
        {
            Debug.LogWarning($"D�wi�k '{soundName}' nie znaleziony lub audioSource jest null.");
            return;
        }

        if (foundSource.audioSource.isPlaying)
        {
            foundSource.audioSource.Stop();
            activeAudioSources.Remove(foundSource);  // Usuwamy z listy aktywnych d�wi�k�w
            Debug.Log($"Zatrzymano d�wi�k '{soundName}' w obiekcie {gameObject.name}.");
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
                activeAudioSources.Remove(source);  // Usuwamy z listy aktywnych d�wi�k�w
            }
        }
    }
}
