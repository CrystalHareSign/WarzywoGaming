using System.Collections;
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
        // Inicjalizacja oryginalnych g³oœnoœci dla ka¿dego Ÿród³a dŸwiêku
        foreach (var source in audioSourcesWithNames)
        {
            if (source.audioSource != null)
            {
                source.originalVolume = source.audioSource.volume; // Automatyczne ustawienie oryginalnej g³oœnoœci
            }
            else
            {
                //Debug.LogWarning($"AudioSource dla dŸwiêku '{source.soundName}' nie jest przypisany w obiekcie {gameObject.name}!");
            }
        }
    }

    public void PlaySound(string soundName, float volume = 1f, bool loop = false)
    {
        // Pobierz AudioSource na podstawie nazwy
        var foundSource = GetAudioSourceByName(soundName);
        if (foundSource == null || foundSource.audioSource == null || foundSource.audioSource.clip == null)
        {
            //Debug.LogWarning($"Brak dŸwiêku '{soundName}' w obiekcie {gameObject.name}!");
            return;
        }

        // Oblicz finaln¹ g³oœnoœæ z uwzglêdnieniem kategorii dŸwiêku
        float finalVolume = volume * foundSource.originalVolume; // Podstawowa wartoœæ g³oœnoœci
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

        // Odtwarzanie dŸwiêku
        if (foundSource.soundType == AudioManager.SoundType.SFX)
        {
            // Metoda PlayOneShot dla SFX
            foundSource.audioSource.PlayOneShot(foundSource.audioSource.clip, finalVolume);
            //Debug.Log($"Odtworzono SFX '{soundName}' jako PlayOneShot w obiekcie {gameObject.name}.");
        }
        else
        {
            // Standardowe odtwarzanie dla pozosta³ych typów dŸwiêków
            foundSource.audioSource.loop = loop; // Czy dŸwiêk ma byæ odtwarzany w pêtli
            foundSource.audioSource.volume = finalVolume; // Ustawienie g³oœnoœci

            if (!foundSource.audioSource.isPlaying)
            {
                foundSource.audioSource.Play(); // Odtwórz dŸwiêk
                //Debug.Log($"Odtworzono dŸwiêk '{soundName}' w obiekcie {gameObject.name}.");
            }
            else
            {
                //Debug.Log($"DŸwiêk '{soundName}' ju¿ jest odtwarzany w obiekcie {gameObject.name}.");
            }
        }
    }

    private void Update()
    {
        // Dynamiczne dostosowanie g³oœnoœci wszystkich odtwarzanych dŸwiêków
        foreach (var source in audioSourcesWithNames)
        {
            if (source.audioSource != null && source.audioSource.isPlaying)
            {
                // Finalna g³oœnoœæ na podstawie typu dŸwiêku i oryginalnej g³oœnoœci
                float finalVolume = source.originalVolume; // Zawsze u¿ywamy oryginalnej g³oœnoœci jako podstawy

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
            //Debug.LogWarning($"DŸwiêk '{soundName}' nie znaleziony lub audioSource jest null.");
            return;
        }

        if (foundSource.audioSource.isPlaying)
        {
            foundSource.audioSource.Stop();
            //Debug.Log($"Zatrzymano dŸwiêk '{soundName}' w obiekcie {gameObject.name}.");
        }
    }

    public void FadeOutSound(string soundName, float fadeDuration)
    {
        var foundSource = GetAudioSourceByName(soundName);
        if (foundSource == null || foundSource.audioSource == null || !foundSource.audioSource.isPlaying)
        {
            //Debug.LogWarning($"Nie mo¿na wyciszyæ dŸwiêku '{soundName}' – Ÿród³o nie istnieje lub nie jest odtwarzane.");
            return;
        }

        StartCoroutine(FadeOutCoroutine(foundSource, fadeDuration));
    }

    private System.Collections.IEnumerator FadeOutCoroutine(AudioSourceWithName source, float duration)
    {
        AudioSource audio = source.audioSource;
        float startVolume = audio.volume;

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            audio.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        audio.Stop();
        audio.volume = source.originalVolume; // Przywróæ oryginaln¹ g³oœnoœæ do przysz³ego odtwarzania
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

    public void PauseSound(string soundName, float fadeOutSeconds = 0f)
    {
        var foundSource = GetAudioSourceByName(soundName);
        if (foundSource == null || foundSource.audioSource == null)
            return;

        if (foundSource.audioSource.isPlaying)
        {
            if (fadeOutSeconds > 0f)
                StartCoroutine(FadeOutAndPause(foundSource, fadeOutSeconds));
            else
                foundSource.audioSource.Pause();
        }
    }

    public void ResumeSound(string soundName, float fadeInSeconds = 0f)
    {
        var foundSource = GetAudioSourceByName(soundName);
        if (foundSource == null || foundSource.audioSource == null)
            return;

        if (!foundSource.audioSource.isPlaying && foundSource.audioSource.time > 0f)
        {
            foundSource.audioSource.UnPause();
            if (fadeInSeconds > 0f)
                StartCoroutine(FadeIn(foundSource, fadeInSeconds));
            else
                foundSource.audioSource.volume = foundSource.originalVolume;
        }
    }

    public void PauseAllSoundsExcept(string[] excludedSoundNames, float fadeOutSeconds = 0f)
    {
        foreach (var source in audioSourcesWithNames)
        {
            if (source.audioSource != null &&
                source.audioSource.isPlaying &&
                System.Array.IndexOf(excludedSoundNames, source.soundName) < 0)
            {
                if (fadeOutSeconds > 0f)
                    StartCoroutine(FadeOutAndPause(source, fadeOutSeconds));
                else
                    source.audioSource.Pause();
            }
        }
    }

    public void ResumeAllSoundsExcept(string[] excludedSoundNames, float fadeInSeconds = 0f)
    {
        foreach (var source in audioSourcesWithNames)
        {
            if (source.audioSource != null &&
                !source.audioSource.isPlaying &&
                source.audioSource.time > 0f &&
                System.Array.IndexOf(excludedSoundNames, source.soundName) < 0)
            {
                source.audioSource.UnPause();
                if (fadeInSeconds > 0f)
                    StartCoroutine(FadeIn(source, fadeInSeconds));
                else
                    source.audioSource.volume = source.originalVolume;
            }
        }
    }

    // Fade out, potem pauza
    private IEnumerator FadeOutAndPause(AudioSourceWithName source, float duration)
    {
        AudioSource audio = source.audioSource;
        float startVolume = audio.volume;
        float time = 0f;
        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            audio.volume = Mathf.Lerp(startVolume, 0f, time / duration);
            yield return null;
        }
        audio.volume = 0f;
        audio.Pause();
    }

    // Fade in po wznowieniu
    private IEnumerator FadeIn(AudioSourceWithName source, float duration)
    {
        AudioSource audio = source.audioSource;
        float targetVolume = source.originalVolume;
        audio.volume = 0f;
        float time = 0f;
        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            audio.volume = Mathf.Lerp(0f, targetVolume, time / duration);
            yield return null;
        }
        audio.volume = targetVolume;
    }
}