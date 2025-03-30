using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public enum SoundType { SFX, Music, Ambient }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource ambientSource;

    [Header("Volume Controls")]
    [Range(0f, 1f)] public float masterMusicVolume = 1f;
    [Range(0f, 1f)] public float masterSFXVolume = 1f;
    [Range(0f, 1f)] public float masterAmbientVolume = 1f;

    [Header("Sound Data")]
    public SoundData soundData;

    private Dictionary<string, Sound> soundDictionary;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        soundDictionary = new Dictionary<string, Sound>();
        foreach (var sound in soundData.sounds)
        {
            soundDictionary[sound.name] = sound;
        }
    }

    public void PlaySound(string soundName, AudioSource customSource = null, float volume = 1f, bool loop = false)
    {
        if (!soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            Debug.LogWarning($"D�wi�k {soundName} nie zosta� znaleziony!");
            return;
        }

        float finalVolume = sound.volume * volume;

        // Mno�ymy finalVolume przez odpowiedni� g�o�no�� master
        finalVolume *= sound.type switch
        {
            SoundType.Music => masterMusicVolume,
            SoundType.SFX => masterSFXVolume,
            SoundType.Ambient => masterAmbientVolume,
            _ => 1f // Domy�lna warto��, gdyby typ nie by� rozpoznany
        };

        if (customSource != null)
        {
            if (customSource.gameObject == null)
            {
                Debug.LogWarning($"AudioSource dla d�wi�ku {soundName} zosta� zniszczony!");
                return;
            }

            customSource.clip = sound.clip;
            customSource.loop = loop;
            customSource.volume = finalVolume;
            customSource.Play();
            return;
        }

        // Sprawdzenie �r�de� d�wi�ku przed ich u�yciem
        switch (sound.type)
        {
            case SoundType.Music:
                if (musicSource == null)
                {
                    Debug.LogWarning("MusicSource nie jest przypisany!");
                    return;
                }
                musicSource.clip = sound.clip;
                musicSource.loop = loop;
                musicSource.volume = finalVolume;
                musicSource.Play();
                break;

            case SoundType.SFX:
                if (sfxSource == null)
                {
                    Debug.LogWarning("SFXSource nie jest przypisany!");
                    return;
                }
                sfxSource.PlayOneShot(sound.clip, finalVolume);
                break;

            case SoundType.Ambient:
                if (ambientSource == null)
                {
                    Debug.LogWarning("AmbientSource nie jest przypisany!");
                    return;
                }
                ambientSource.clip = sound.clip;
                ambientSource.loop = loop;
                ambientSource.volume = finalVolume;
                ambientSource.Play();
                break;
        }
    }

    // Metody do zmiany g�o�no�ci
    public void SetMusicVolume(float volume)
    {
        masterMusicVolume = volume;
        musicSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        masterSFXVolume = volume;
        // sfxSource.volume = volume; // Dla PlayOneShot g�o�no�� jest ustawiana w PlaySound, wi�c ta linia nie jest potrzebna
    }

    public void SetAmbientVolume(float volume)
    {
        masterAmbientVolume = volume;
        ambientSource.volume = volume;
    }
}
