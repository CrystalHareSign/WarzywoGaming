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
        PlaySound("DieselBusEngine", 1f, false);  // 1f to g�o�no��, false to brak p�tli

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
        // Wyszukaj w li�cie AudioSourceWithName na podstawie nazwy d�wi�ku
        AudioSourceWithName foundSource = audioSourcesWithNames.Find(x => x.soundName == soundName);

        if (foundSource != null)
        {
            // Odtwarzanie d�wi�ku przez AudioManager
            AudioManager.Instance.PlaySound(foundSource.soundName, foundSource.audioSource, volume, loop);
        }
        else
        {
            Debug.LogWarning($"D�wi�k {soundName} nie jest przypisany do {gameObject.name}!");
        }
    }
}
