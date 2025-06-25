using UnityEngine;
using System.Collections.Generic;

public enum DoorDirection
{
    North,  // (0,0,1)
    South,  // (0,0,-1)
    East,   // (1,0,0)
    West    // (-1,0,0)
}

[System.Serializable]
public class Doorway
{
    public Vector3 localPosition;
    public DoorDirection direction;
}

[CreateAssetMenu(menuName = "Procedural/RoomPrefabData")]
public class RoomPrefabData : ScriptableObject
{
    public GameObject prefab;
    public List<Doorway> doorways;
    [Range(0, 100)]
    public int spawnChance = 100;
    public List<Vector3> itemSpawnPoints; // Pozycje lokalne na itemy
}