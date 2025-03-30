using UnityEngine;
using System.Collections.Generic;

public class PlaySoundOnObject : MonoBehaviour
{
    [System.Serializable]
    public class AudioSourceWithName
    {
        public AudioSource audioSource;  // èrÛd≥o düwiÍku
        public string soundName;         // Nazwa düwiÍku
    }

    public List<AudioSourceWithName> audioSourcesWithNames;  // Lista grupujπcych AudioSource z nazwπ

    private void Start()
    {
        // Jeúli nie przypisano ürÛd≥a düwiÍku, dodaj je automatycznie
        if (audioSourcesWithNames.Count == 0)
        {
            foreach (var audioSource in GetComponents<AudioSource>())
            {
                audioSourcesWithNames.Add(new AudioSourceWithName { audioSource = audioSource, soundName = audioSource.name });
            }
        }
    }

    // Publiczna metoda do odtwarzania düwiÍku
    public void PlaySound(string soundName, float volume = 1f, bool loop = false)
    {
        AudioSourceWithName foundSource = audioSourcesWithNames.Find(x => x.soundName == soundName);

        if (foundSource != null)
        {
            AudioManager.Instance.PlaySound(foundSource.soundName, foundSource.audioSource, volume, loop);
        }
        else
        {
            Debug.LogWarning($"DüwiÍk {soundName} nie jest przypisany do {gameObject.name}!");
        }
    }

    // Nowa metoda do zatrzymywania düwiÍku
    public void StopSound(string soundName)
    {
        AudioSourceWithName foundSource = audioSourcesWithNames.Find(x => x.soundName == soundName);

        if (foundSource != null && foundSource.audioSource.isPlaying)
        {
            foundSource.audioSource.Stop();
        }
        else
        {
            Debug.LogWarning($"DüwiÍk {soundName} nie jest aktualnie odtwarzany na {gameObject.name}!");
        }
    }
    // Metoda do zatrzymania wszystkich düwiÍkÛw
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
