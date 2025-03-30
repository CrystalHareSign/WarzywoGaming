using UnityEngine;
using System.Collections.Generic;

public class PlaySoundOnObject : MonoBehaviour
{
    [System.Serializable]
    public class AudioSourceWithName
    {
        public AudioSource audioSource;  // �r�d�o d�wi�ku
        public string soundName;         // Nazwa d�wi�ku
    }

    public List<AudioSourceWithName> audioSourcesWithNames;  // Lista grupuj�cych AudioSource z nazw�

    private void Start()
    {
        // Je�li nie przypisano �r�d�a d�wi�ku, dodaj je automatycznie
        if (audioSourcesWithNames.Count == 0)
        {
            foreach (var audioSource in GetComponents<AudioSource>())
            {
                audioSourcesWithNames.Add(new AudioSourceWithName { audioSource = audioSource, soundName = audioSource.name });
            }
        }
    }

    // Publiczna metoda do odtwarzania d�wi�ku
    public void PlaySound(string soundName, float volume = 1f, bool loop = false)
    {
        AudioSourceWithName foundSource = audioSourcesWithNames.Find(x => x.soundName == soundName);

        if (foundSource != null)
        {
            AudioManager.Instance.PlaySound(foundSource.soundName, foundSource.audioSource, volume, loop);
        }
        else
        {
            Debug.LogWarning($"D�wi�k {soundName} nie jest przypisany do {gameObject.name}!");
        }
    }

    // Nowa metoda do zatrzymywania d�wi�ku
    public void StopSound(string soundName)
    {
        AudioSourceWithName foundSource = audioSourcesWithNames.Find(x => x.soundName == soundName);

        if (foundSource != null && foundSource.audioSource.isPlaying)
        {
            foundSource.audioSource.Stop();
        }
        else
        {
            Debug.LogWarning($"D�wi�k {soundName} nie jest aktualnie odtwarzany na {gameObject.name}!");
        }
    }
    // Metoda do zatrzymania wszystkich d�wi�k�w
    public void StopAllSounds()
    {
        foreach (var source in audioSourcesWithNames)
        {
            if (source.audioSource.isPlaying)
            {
                source.audioSource.Stop();
            }
        }
    }
}
