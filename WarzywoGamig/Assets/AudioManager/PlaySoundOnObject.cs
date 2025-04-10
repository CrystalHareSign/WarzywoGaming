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
        [HideInInspector] public float originalVolume; // Ukryte w inspektorze, ustawiane automatycznie
    }

    public List<AudioSourceWithName> audioSourcesWithNames = new List<AudioSourceWithName>();

    private void Start()
    {
        // Inicjalizacja oryginalnych g�o�no�ci dla ka�dego �r�d�a d�wi�ku
        foreach (var source in audioSourcesWithNames)
        {
            if (source.audioSource != null)
            {
                source.originalVolume = source.audioSource.volume; // Automatyczne ustawienie oryginalnej g�o�no�ci
            }
            else
            {
                Debug.LogWarning($"AudioSource dla d�wi�ku '{source.soundName}' nie jest przypisany w obiekcie {gameObject.name}!");
            }
        }
    }

    public void PlaySound(string soundName, float volume = 1f, bool loop = false)
    {
        // Pobierz AudioSource na podstawie nazwy
        var foundSource = GetAudioSourceByName(soundName);
        if (foundSource == null || foundSource.audioSource == null || foundSource.audioSource.clip == null)
        {
            Debug.LogWarning($"Brak d�wi�ku '{soundName}' w obiekcie {gameObject.name}!");
            return;
        }

        // Oblicz finaln� g�o�no�� z uwzgl�dnieniem kategorii d�wi�ku
        float finalVolume = volume * foundSource.originalVolume; // Podstawowa warto�� g�o�no�ci
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

        // Odtwarzanie d�wi�ku
        if (foundSource.soundType == AudioManager.SoundType.SFX)
        {
            // Metoda PlayOneShot dla SFX
            foundSource.audioSource.PlayOneShot(foundSource.audioSource.clip, finalVolume);
            Debug.Log($"Odtworzono SFX '{soundName}' jako PlayOneShot w obiekcie {gameObject.name}.");
        }
        else
        {
            // Standardowe odtwarzanie dla pozosta�ych typ�w d�wi�k�w
            foundSource.audioSource.loop = loop; // Czy d�wi�k ma by� odtwarzany w p�tli
            foundSource.audioSource.volume = finalVolume; // Ustawienie g�o�no�ci

            if (!foundSource.audioSource.isPlaying)
            {
                foundSource.audioSource.Play(); // Odtw�rz d�wi�k
                Debug.Log($"Odtworzono d�wi�k '{soundName}' w obiekcie {gameObject.name}.");
            }
            else
            {
                Debug.Log($"D�wi�k '{soundName}' ju� jest odtwarzany w obiekcie {gameObject.name}.");
            }
        }
    }

    private void Update()
    {
        // Dynamiczne dostosowanie g�o�no�ci wszystkich odtwarzanych d�wi�k�w
        foreach (var source in audioSourcesWithNames)
        {
            if (source.audioSource != null && source.audioSource.isPlaying)
            {
                // Finalna g�o�no�� na podstawie typu d�wi�ku i oryginalnej g�o�no�ci
                float finalVolume = source.originalVolume; // Zawsze u�ywamy oryginalnej g�o�no�ci jako podstawy

                switch (source.soundType)
                {
                    case AudioManager.SoundType.Music:
                        source.audioSource.volume = finalVolume * AudioManager.Instance.masterMusicVolume;
                        break;
                    case AudioManager.SoundType.SFX:
                        // Dla SFX z PlayOneShot nie zmieniamy dynamicznie
                        break;
                    case AudioManager.SoundType.Ambient:
                        source.audioSource.volume = finalVolume * AudioManager.Instance.masterAmbientVolume;
                        break;
                }
            }
        }
    }

    private AudioSourceWithName GetAudioSourceByName(string soundName)
    {
        return audioSourcesWithNames.Find(x => x.soundName == soundName);
    }

    public void StopSound(string soundName)
    {
        var foundSource = GetAudioSourceByName(soundName);
        if (foundSource == null || foundSource.audioSource == null)
        {
            Debug.LogWarning($"D�wi�k '{soundName}' nie znaleziony lub audioSource jest null.");
            return;
        }

        if (foundSource.audioSource.isPlaying)
        {
            foundSource.audioSource.Stop();
            Debug.Log($"Zatrzymano d�wi�k '{soundName}' w obiekcie {gameObject.name}.");
        }
    }

    public void StopAllSounds()
    {
        foreach (var source in audioSourcesWithNames)
        {
            if (source.audioSource != null && source.audioSource.isPlaying)
            {
                source.audioSource.Stop();
            }
        }
    }
}