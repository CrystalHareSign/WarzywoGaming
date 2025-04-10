using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [HideInInspector]
    [Range(0f, 1f)] public float volume = 1f;  // Dodajemy pole volume
}


[CreateAssetMenu(fileName = "New Sound Data", menuName = "Audio/Sound Data")]
public class SoundData : ScriptableObject
{
    public Sound[] sounds;  // Tablica dŸwiêków
}