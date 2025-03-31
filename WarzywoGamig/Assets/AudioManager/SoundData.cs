using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    public AudioManager.SoundType type;
    public bool enabled = true; // Mo�liwo�� w��czania/wy��czania d�wi�ku
}

[CreateAssetMenu(fileName = "New Sound Data", menuName = "Audio/Sound Data")]
public class SoundData : ScriptableObject
{
    public Sound[] sounds;
}