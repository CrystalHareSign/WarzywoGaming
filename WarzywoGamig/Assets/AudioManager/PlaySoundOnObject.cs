using UnityEngine;
using System.Collections.Generic;

public class PlaySoundOnObject : MonoBehaviour
{
    [System.Serializable]
    public class AudioSourceWithName
    {
        public AudioSource audioSource;
        public string soundName;
    }

    public List<AudioSourceWithName> audioSourcesWithNames = new List<AudioSourceWithName>();

    /// <summary>
    /// Odtwarza d�wi�k przypisany do TEGO obiektu.
    /// </summary>
    public void PlaySound(string soundName, float volume = 1f, bool loop = false)
    {
        if (audioSourcesWithNames == null || audioSourcesWithNames.Count == 0)
        {
            Debug.LogWarning($"Brak AudioSource w obiekcie {gameObject.name}!");
            return;
        }

        AudioSourceWithName foundSource = audioSourcesWithNames.Find(x => x.soundName == soundName);

        if (foundSource != null && foundSource.audioSource != null)
        {
            if (foundSource.audioSource.clip == null)
            {
                Debug.LogWarning($"AudioSource '{soundName}' w obiekcie {gameObject.name} nie ma przypisanego pliku d�wi�kowego!");
                return;
            }

            foundSource.audioSource.loop = loop;
            foundSource.audioSource.volume = volume;

            if (loop)
            {
                if (!foundSource.audioSource.isPlaying)
                {
                    foundSource.audioSource.Play();
                    Debug.Log($"Odtworzono p�tl� d�wi�ku '{soundName}' w obiekcie {gameObject.name}.");
                }
            }
            else
            {
                foundSource.audioSource.PlayOneShot(foundSource.audioSource.clip, volume);
                Debug.Log($"Odtworzono d�wi�k '{soundName}' na obiekcie {gameObject.name}.");
            }
        }
        else
        {
            //Debug.LogWarning($"D�wi�k '{soundName}' nie jest przypisany do {gameObject.name}!");
        }
    }

    /// <summary>
    /// Zatrzymuje d�wi�k o podanej nazwie.
    /// </summary>
    public void StopSound(string soundName)
    {
        if (audioSourcesWithNames == null || audioSourcesWithNames.Count == 0)
        {
            Debug.LogWarning($"Brak AudioSource w obiekcie {gameObject.name}!");
            return;
        }

        AudioSourceWithName foundSource = audioSourcesWithNames.Find(x => x.soundName == soundName);

        if (foundSource != null && foundSource.audioSource != null)
        {
            if (foundSource.audioSource.isPlaying)
            {
                foundSource.audioSource.Stop();
            }
            else
            {
                Debug.LogWarning($"D�wi�k '{soundName}' nie jest aktualnie odtwarzany na {gameObject.name}.");
            }
        }
        else
        {
            //Debug.LogWarning($"D�wi�k '{soundName}' nie zosta� znaleziony lub przypisanie audioSource jest niepoprawne w obiekcie {gameObject.name}.");
        }
    }

    /// <summary>
    /// Zatrzymuje wszystkie d�wi�ki w tym obiekcie.
    /// </summary>
    public void StopAllSounds()
    {
        if (audioSourcesWithNames == null || audioSourcesWithNames.Count == 0)
        {
            Debug.LogWarning($"Brak AudioSource w obiekcie {gameObject.name}!");
            return;
        }

        foreach (var source in audioSourcesWithNames)
        {
            if (source.audioSource != null && source.audioSource.isPlaying)
            {
                source.audioSource.Stop();
            }
        }
    }

    /// <summary>
    /// Odtwarza d�wi�k na dowolnym obiekcie w scenie.
    /// </summary>
    /// <summary>
    /// Odtwarza d�wi�k na dowolnym obiekcie w scenie.
    /// </summary>
    public static void PlaySoundGlobal(string soundName, float volume = 1f, bool loop = false)
    {
        PlaySoundOnObject[] allAudioManagers = Object.FindObjectsByType<PlaySoundOnObject>(FindObjectsSortMode.None);

        foreach (var manager in allAudioManagers)
        {
            AudioSourceWithName foundSource = manager.audioSourcesWithNames.Find(x => x.soundName == soundName);
            if (foundSource != null && foundSource.audioSource != null)
            {
                if (foundSource.audioSource.clip == null)
                {
                    Debug.LogWarning($"AudioSource '{soundName}' w obiekcie {manager.gameObject.name} nie ma przypisanego pliku d�wi�kowego!");
                    return;
                }

                foundSource.audioSource.loop = loop;
                foundSource.audioSource.volume = volume;

                if (loop)
                {
                    if (!foundSource.audioSource.isPlaying)
                    {
                        foundSource.audioSource.Play();
                        Debug.Log($"Odtworzono p�tl� d�wi�ku '{soundName}' w obiekcie {manager.gameObject.name}.");
                    }
                }
                else
                {
                    foundSource.audioSource.PlayOneShot(foundSource.audioSource.clip, volume);
                    Debug.Log($"Odtworzono d�wi�k '{soundName}' na obiekcie {manager.gameObject.name}.");
                }

                return; // Przerywamy p�tl� po znalezieniu pierwszego pasuj�cego d�wi�ku
            }
        }

        Debug.LogWarning($"D�wi�k '{soundName}' nie zosta� znaleziony w �adnym obiekcie w scenie!");
    }
}
