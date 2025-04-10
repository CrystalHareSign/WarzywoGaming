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

    // Metoda PlaySound z uwzgl�dnieniem volume
    public void PlaySound(string soundName, SoundType soundType, AudioSource customSource = null, float volume = 1f, bool loop = false)
    {
        if (!soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            Debug.LogWarning($"D�wi�k {soundName} nie zosta� znaleziony!");
            return;
        }

        // Finalna g�o�no�� d�wi�ku
        float finalVolume = sound.volume * volume;

        // Dostosowanie finalVolume wed�ug typu d�wi�ku
        finalVolume *= soundType switch
        {
            SoundType.Music => masterMusicVolume,
            SoundType.SFX => masterSFXVolume,
            SoundType.Ambient => masterAmbientVolume,
            _ => 1f
        };

        // Je�li dostarczono customSource
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

        // Sprawdzamy odpowiednie �r�d�o d�wi�ku
        switch (soundType)
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

    public void SetMusicVolume(float volume)
    {
        masterMusicVolume = volume;
        //Debug.Log($"Nowa g�o�no�� muzyki: {masterMusicVolume}");
    }

    public void SetSFXVolume(float volume)
    {
        masterSFXVolume = volume;
        //Debug.Log($"Nowa g�o�no�� SFX: {masterSFXVolume}");
    }

    public void SetAmbientVolume(float volume)
    {
        masterAmbientVolume = volume;
        //Debug.Log($"Nowa g�o�no�� ambientu: {masterAmbientVolume}");
    }

}
