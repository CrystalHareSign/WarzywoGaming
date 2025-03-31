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
    /// Odtwarza dŸwiêk przypisany do TEGO obiektu.
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
                Debug.LogWarning($"AudioSource '{soundName}' w obiekcie {gameObject.name} nie ma przypisanego pliku dŸwiêkowego!");
                return;
            }

            foundSource.audioSource.loop = loop;
            foundSource.audioSource.volume = volume;

            if (loop)
            {
                if (!foundSource.audioSource.isPlaying)
                {
                    foundSource.audioSource.Play();
                    //Debug.Log($"Odtworzono pêtlê dŸwiêku '{soundName}' w obiekcie {gameObject.name}.");
                }
            }
            else
            {
                foundSource.audioSource.PlayOneShot(foundSource.audioSource.clip, volume);
                Debug.Log($"Odtworzono dŸwiêk '{soundName}' na obiekcie {gameObject.name}.");
            }
        }
        else
        {
            //Debug.LogWarning($"DŸwiêk '{soundName}' nie jest przypisany do {gameObject.name}!");
        }
    }

    /// <summary>
    /// Zmienia pitch dla konkretnego dŸwiêku w obiekcie.
    /// </summary>
    public void ChangePitch(string soundName, float newPitch)
    {
        AudioSourceWithName foundSource = audioSourcesWithNames.Find(x => x.soundName == soundName);

        if (foundSource != null && foundSource.audioSource != null)
        {
            foundSource.audioSource.pitch = newPitch;
            Debug.Log($"Zmieniono pitch dŸwiêku '{soundName}' na {newPitch} w obiekcie {gameObject.name}.");
        }
        else
        {
            Debug.LogWarning($"DŸwiêk '{soundName}' nie zosta³ znaleziony w obiekcie {gameObject.name}!");
        }
    }
    //////////     WYWO£ANIE     ///////      playSoundOnObject.ChangePitch("StormSound", 1.5f); // Zwiêkszy pitch

    /// <summary>
    /// Zmienia g³oœnoœæ dla konkretnego dŸwiêku w obiekcie.
    /// </summary>
    public void ChangeVolume(string soundName, float newVolume)
    {
        AudioSourceWithName foundSource = audioSourcesWithNames.Find(x => x.soundName == soundName);

        if (foundSource != null && foundSource.audioSource != null)
        {
            foundSource.audioSource.volume = newVolume;
            Debug.Log($"Zmieniono g³oœnoœæ dŸwiêku '{soundName}' na {newVolume} w obiekcie {gameObject.name}.");
        }
        else
        {
            Debug.LogWarning($"DŸwiêk '{soundName}' nie zosta³ znaleziony w obiekcie {gameObject.name}!");
        }
    }

    //////////     WYWO£ANIE     ///////      playSoundOnObject.ChangeVolume("StormSound", 0.5f); // Zmniejszy g³oœnoœæ
    /// <summary>
    /// Zatrzymuje dŸwiêk o podanej nazwie.
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
                //Debug.LogWarning($"DŸwiêk '{soundName}' nie jest aktualnie odtwarzany na {gameObject.name}.");
            }
        }
        else
        {
            //Debug.LogWarning($"DŸwiêk '{soundName}' nie zosta³ znaleziony lub przypisanie audioSource jest niepoprawne w obiekcie {gameObject.name}.");
        }
    }

    /// <summary>
    /// Zatrzymuje wszystkie dŸwiêki w tym obiekcie.
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
    /// Odtwarza dŸwiêk na dowolnym obiekcie w scenie.
    /// </summary>
    /// <summary>
    /// Odtwarza dŸwiêk na dowolnym obiekcie w scenie.
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
                    Debug.LogWarning($"AudioSource '{soundName}' w obiekcie {manager.gameObject.name} nie ma przypisanego pliku dŸwiêkowego!");
                    return;
                }

                foundSource.audioSource.loop = loop;
                foundSource.audioSource.volume = volume;

                if (loop)
                {
                    if (!foundSource.audioSource.isPlaying)
                    {
                        foundSource.audioSource.Play();
                        Debug.Log($"Odtworzono pêtlê dŸwiêku '{soundName}' w obiekcie {manager.gameObject.name}.");
                    }
                }
                else
                {
                    foundSource.audioSource.PlayOneShot(foundSource.audioSource.clip, volume);
                    Debug.Log($"Odtworzono dŸwiêk '{soundName}' na obiekcie {manager.gameObject.name}.");
                }

                return; // Przerywamy pêtlê po znalezieniu pierwszego pasuj¹cego dŸwiêku
            }
        }

        Debug.LogWarning($"DŸwiêk '{soundName}' nie zosta³ znaleziony w ¿adnym obiekcie w scenie!");
    }
}
