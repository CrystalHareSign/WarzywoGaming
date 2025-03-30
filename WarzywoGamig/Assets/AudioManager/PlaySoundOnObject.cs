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
        PlaySound("DieselBusEngine", 1f, false);  // 1f to g≥oúnoúÊ, false to brak pÍtli

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
        // Wyszukaj w liúcie AudioSourceWithName na podstawie nazwy düwiÍku
        AudioSourceWithName foundSource = audioSourcesWithNames.Find(x => x.soundName == soundName);

        if (foundSource != null)
        {
            // Odtwarzanie düwiÍku przez AudioManager
            AudioManager.Instance.PlaySound(foundSource.soundName, foundSource.audioSource, volume, loop);
        }
        else
        {
            Debug.LogWarning($"DüwiÍk {soundName} nie jest przypisany do {gameObject.name}!");
        }
    }
}
