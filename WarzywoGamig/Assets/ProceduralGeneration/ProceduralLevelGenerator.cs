using System.Collections.Generic;
using UnityEngine;

public class ProceduralLevelGenerator : MonoBehaviour
{
    [Header("Room Prefabs")]
    public List<RoomPrefabData> roomPrefabs;

    [Header("Start Room")]
    public RoomPrefabData startRoomPrefab;

    [Header("Generation Settings")]
    [Tooltip("Ile pokoi ma byæ w poziomie")]
    public int roomCount = 10;

    [Header("Direction Restriction")]
    public DoorDirection forbiddenDirection = DoorDirection.North;

    public float forbiddenMargin = 0.01f;

    [Header("Debug/Generation")]
    [Tooltip("Czy generowaæ automatycznie na starcie")]
    public bool autoGenerateOnStart = true;

    private List<PlacedRoom> placedRooms = new List<PlacedRoom>();
    private Vector3 startRoomPosition;

    // Struktura do backtrackingu: zapamiêtuje jak pokój by³ do³o¿ony
    private class GenerationStep
    {
        public PlacedRoom room;
        public int parentRoomIndex;
        public int parentDoorIndex;
        public int thisDoorIndex;
    }
    private List<GenerationStep> generationSteps = new List<GenerationStep>();

    public void GenerateLevel()
    {
        StartCoroutine(GenerateLevelCoroutine());
    }

    void Start()
    {
        if (autoGenerateOnStart)
            GenerateLevel();
    }

    private System.Collections.IEnumerator GenerateLevelCoroutine()
    {
        ClearPreviousLevel();
        bool success = TryGenerateLevelWithBacktracking();
        if (!success)
            Debug.LogError($"Nie uda³o siê wygenerowaæ poziomu z {roomCount} pokojami (po backtrackingu)!");
        yield break;
    }

    private void ClearPreviousLevel()
    {
        foreach (var room in placedRooms)
            if (room != null && room.room != null)
                Destroy(room.room);
        placedRooms.Clear();
        generationSteps.Clear();
    }

    /// <summary>
    /// Pêtla generacji z backtrackingiem i poprawnym odznaczaniem drzwi
    /// </summary>
    private bool TryGenerateLevelWithBacktracking()
    {
        generationSteps.Clear();

        // Start room
        RoomPrefabData startRoomData = startRoomPrefab != null ? startRoomPrefab : GetRandomRoomPrefab();
        Quaternion yRot = GetRandomYRotation();
        GameObject startRoomGO = Instantiate(startRoomData.prefab, Vector3.zero, yRot);
        Bounds startBounds = GetRoomBounds(startRoomGO);
        var startRoom = new PlacedRoom(startRoomGO, startRoomData, GetDoorwaysWorld(startRoomGO, startRoomData), startBounds);
        placedRooms.Add(startRoom);
        generationSteps.Add(new GenerationStep { room = startRoom, parentRoomIndex = -1, parentDoorIndex = -1, thisDoorIndex = -1 });
        startRoomPosition = startRoomGO.transform.position;

        int placed = 1;
        int maxAttempts = 10000; // zabezpieczenie przed wieczn¹ pêtl¹
        int attempts = 0;

        while (placed < roomCount && attempts < maxAttempts)
        {
            if (TryPlaceNextRoomWithTrace(out int parentRoomIdx, out int parentDoor, out int thisDoor))
            {
                placed++;
                generationSteps.Add(new GenerationStep
                {
                    room = placedRooms[placedRooms.Count - 1],
                    parentRoomIndex = parentRoomIdx,
                    parentDoorIndex = parentDoor,
                    thisDoorIndex = thisDoor
                });
            }
            else
            {
                // BACKTRACK: usuñ ostatni krok i odblokuj drzwi w rodzicu
                if (generationSteps.Count <= 1) // nie usuwaj startowego
                    break;
                var lastStep = generationSteps[generationSteps.Count - 1];
                generationSteps.RemoveAt(generationSteps.Count - 1);

                if (lastStep.parentRoomIndex >= 0 && lastStep.parentDoorIndex >= 0)
                    placedRooms[lastStep.parentRoomIndex].UnmarkDoorConnected(lastStep.parentDoorIndex);

                placedRooms.RemoveAt(placedRooms.Count - 1);
                if (lastStep.room.room != null) Destroy(lastStep.room.room);
                placed--;
            }
            attempts++;
        }

        if (placed >= roomCount)
            return true;
        ClearPreviousLevel();
        return false;
    }

    // Fisher-Yates shuffle
    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    RoomPrefabData GetRandomRoomPrefab()
    {
        var eligibleRooms = new List<RoomPrefabData>();
        int total = 0;
        foreach (var room in roomPrefabs)
        {
            if (room != null && room.spawnChance > 0)
            {
                eligibleRooms.Add(room);
                total += room.spawnChance;
            }
        }
        if (eligibleRooms.Count == 0)
            throw new System.Exception("No available rooms to spawn (all spawnChance == 0 or null)!");

        int pick = Random.Range(0, total);
        int sum = 0;
        foreach (var room in eligibleRooms)
        {
            sum += room.spawnChance;
            if (pick < sum)
                return room;
        }
        return eligibleRooms[eligibleRooms.Count - 1];
    }

    // Nowa metoda: TryPlaceNextRoomWithTrace
    bool TryPlaceNextRoomWithTrace(out int usedRoomIdx, out int usedParentDoor, out int usedThisDoor)
    {
        usedRoomIdx = -1; usedParentDoor = -1; usedThisDoor = -1;
        var openDoorways = new List<(PlacedRoom room, int roomIndex, int doorwayIndex)>();
        for (int idx = 0; idx < placedRooms.Count; idx++)
        {
            var placed = placedRooms[idx];
            for (int i = 0; i < placed.doorways.Count; i++)
            {
                var dw = placed.doorways[i];
                if (!placed.IsDoorConnected(i) && Vector3.Angle(dw.direction, GetDirectionVector(forbiddenDirection)) >= 1f)
                {
                    openDoorways.Add((placed, idx, i));
                }
            }
        }

        if (openDoorways.Count == 0)
            return false;

        Shuffle(openDoorways);

        foreach (var (placed, roomIdx, i) in openDoorways)
        {
            for (int roomTry = 0; roomTry < 10; roomTry++)
            {
                RoomPrefabData roomData = GetRandomRoomPrefab();
                if (roomData == null || roomData.prefab == null || roomData.doorways == null || roomData.doorways.Count == 0)
                    continue;

                int[] yAngles = GetShuffledRotations();
                List<int> candidateDoorIndices = new List<int>();
                for (int j = 0; j < roomData.doorways.Count; j++) candidateDoorIndices.Add(j);
                Shuffle(candidateDoorIndices);

                foreach (int angle in yAngles)
                {
                    var yRot = Quaternion.Euler(0, angle, 0);

                    foreach (int j in candidateDoorIndices)
                    {
                        var candidateDoor = roomData.doorways[j];
                        if (candidateDoor == null || candidateDoor.direction == forbiddenDirection)
                            continue;
                        Vector3 candidateDir = yRot * GetDirectionVector(candidateDoor.direction);

                        var doorway = placed.doorways[i];
                        if (Vector3.Angle(doorway.direction, -candidateDir) > 1f)
                            continue;

                        Quaternion rot = Quaternion.FromToRotation(candidateDir, -doorway.direction) * yRot;
                        Vector3 worldTarget = doorway.position;
                        Vector3 candidateLocal = candidateDoor.localPosition;
                        Vector3 offset = worldTarget - (rot * candidateLocal);

                        var newRoomGO = Instantiate(roomData.prefab, offset, rot);
                        Bounds newBounds = GetRoomBounds(newRoomGO);

                        if (IsColliding(newBounds))
                        {
                            Destroy(newRoomGO);
                            continue;
                        }
                        if (IsInForbiddenHemisphere(newBounds.center, startRoomPosition, forbiddenDirection, forbiddenMargin))
                        {
                            Destroy(newRoomGO);
                            continue;
                        }

                        var newDoorways = GetDoorwaysWorld(newRoomGO, roomData);
                        var newPlaced = new PlacedRoom(newRoomGO, roomData, newDoorways, newBounds);
                        placedRooms.Add(newPlaced);
                        placed.MarkDoorConnected(i);
                        newPlaced.MarkDoorConnected(j);

                        usedRoomIdx = roomIdx;
                        usedParentDoor = i;
                        usedThisDoor = j;
                        return true;
                    }
                }
            }
        }
        return false;
    }

    static bool IsInForbiddenHemisphere(Vector3 check, Vector3 start, DoorDirection forbidden, float margin)
    {
        switch (forbidden)
        {
            case DoorDirection.North:
                return check.z > start.z + margin;
            case DoorDirection.South:
                return check.z < start.z - margin;
            case DoorDirection.East:
                return check.x > start.x + margin;
            case DoorDirection.West:
                return check.x < start.x - margin;
            default:
                return false;
        }
    }

    int[] GetShuffledRotations()
    {
        int[] yAngles = { 0, 90, 180, 270 };
        for (int i = yAngles.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = yAngles[i];
            yAngles[i] = yAngles[j];
            yAngles[j] = temp;
        }
        return yAngles;
    }

    bool IsColliding(Bounds newBounds)
    {
        foreach (var placed in placedRooms)
        {
            if (placed != null && placed.bounds.Intersects(newBounds))
            {
                Debug.LogWarning($"Collision between {placed.room.name} and new room at {newBounds.center}");
                return true;
            }
        }
        return false;
    }

    Bounds GetRoomBounds(GameObject roomGO)
    {
        Renderer rend = roomGO.GetComponentInChildren<Renderer>();
        if (rend != null)
            return rend.bounds;
        else
            return new Bounds(roomGO.transform.position, Vector3.one * 5f);
    }

    Quaternion GetRandomYRotation()
    {
        int[] yAngles = { 0, 90, 180, 270 };
        int angle = yAngles[Random.Range(0, yAngles.Length)];
        return Quaternion.Euler(0, angle, 0);
    }

    public static Vector3 GetDirectionVector(DoorDirection dir)
    {
        switch (dir)
        {
            case DoorDirection.North: return Vector3.forward;
            case DoorDirection.South: return Vector3.back;
            case DoorDirection.East: return Vector3.right;
            case DoorDirection.West: return Vector3.left;
            default: return Vector3.forward;
        }
    }

    List<PlacedDoorway> GetDoorwaysWorld(GameObject roomGO, RoomPrefabData data)
    {
        var result = new List<PlacedDoorway>();
        if (data == null || data.doorways == null) return result;
        foreach (var doorway in data.doorways)
        {
            if (doorway == null) continue;
            result.Add(new PlacedDoorway
            {
                localPosition = doorway.localPosition,
                position = roomGO.transform.TransformPoint(doorway.localPosition),
                direction = roomGO.transform.rotation * GetDirectionVector(doorway.direction)
            });
        }
        return result;
    }
}

public class PlacedRoom
{
    public GameObject room;
    public RoomPrefabData data;
    public List<PlacedDoorway> doorways;
    public Bounds bounds;
    private HashSet<int> connectedDoors = new HashSet<int>();

    public PlacedRoom(GameObject room, RoomPrefabData data, List<PlacedDoorway> doorways, Bounds bounds)
    {
        this.room = room;
        this.data = data;
        this.doorways = doorways;
        this.bounds = bounds;
    }

    public bool IsDoorConnected(int doorwayIndex)
    {
        return connectedDoors.Contains(doorwayIndex);
    }

    public void MarkDoorConnected(int doorwayIndex)
    {
        connectedDoors.Add(doorwayIndex);
    }

    public void UnmarkDoorConnected(int doorwayIndex)
    {
        connectedDoors.Remove(doorwayIndex);
    }
}

[System.Serializable]
public class PlacedDoorway
{
    public Vector3 localPosition;
    public Vector3 position;
    public Vector3 direction;
}