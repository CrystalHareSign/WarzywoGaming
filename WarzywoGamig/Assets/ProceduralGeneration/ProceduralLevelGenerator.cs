using UnityEngine;
using System.Collections.Generic;

public class ProceduralLevelGenerator : MonoBehaviour
{
    [Header("Room Prefabs")]
    public List<RoomPrefabData> roomPrefabs;

    [Header("Start Room")]
    public RoomPrefabData startRoomPrefab;

    [Header("Start Room Position")]
    public Vector3 startRoomPositionOverride = Vector3.zero;

    [Header("Generation Settings")]
    [Tooltip("Ile pokoi ma byæ w poziomie")]
    public int roomCount = 10;

    [Header("Direction Restriction")]
    public DoorDirection forbiddenDirection = DoorDirection.North;
    public float forbiddenMargin = 0.01f;

    [Header("Debug/Generation")]
    [Tooltip("Czy generowaæ automatycznie na starcie")]
    public bool autoGenerateOnStart = true;

    [Header("Door Prefabs")]
    public List<GameObject> doorClosedPrefabs;   // Lista zamkniêtych (œlepych) drzwi
    public List<GameObject> doorActivePrefabs;   // Lista otwartych (przechodnich) drzwi

    private List<PlacedRoom> placedRooms = new List<PlacedRoom>();
    private Vector3 startRoomPosition;

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
        // Pobierz roomCount z MissionSettings jeœli zosta³ ustawiony (np. przez MissionDefiner w poprzedniej scenie)
        if (MissionSettings.roomCount > 0)
        {
            roomCount = MissionSettings.roomCount;
            // (opcjonalnie) mo¿esz u¿yæ te¿ MissionSettings.locationName
        }

        if (autoGenerateOnStart)
            GenerateLevel();
    }

    private System.Collections.IEnumerator GenerateLevelCoroutine()
    {
        ClearPreviousLevel();
        bool success = TryGenerateLevelWithBacktracking();
        if (success)
            SpawnDoors();
        else
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

    private bool TryGenerateLevelWithBacktracking()
    {
        generationSteps.Clear();

        RoomPrefabData startRoomData = startRoomPrefab != null ? startRoomPrefab : GetRandomRoomPrefab();
        // Start room zawsze w domyœlnej rotacji, pozycja z inspektora!
        Quaternion yRot = Quaternion.identity;
        GameObject startRoomGO = Instantiate(startRoomData.prefab, startRoomPositionOverride, yRot);
        Bounds startBounds = GetRoomBounds(startRoomGO);
        Debug.Log($"START ROOM bounds: center={startBounds.center}, size={startBounds.size}");
        var startRoom = new PlacedRoom(startRoomGO, startRoomData, GetDoorwaysWorld(startRoomGO, startRoomData), startBounds);
        placedRooms.Add(startRoom);
        generationSteps.Add(new GenerationStep { room = startRoom, parentRoomIndex = -1, parentDoorIndex = -1, thisDoorIndex = -1 });
        startRoomPosition = startRoomGO.transform.position;

        int placed = 1;
        int maxAttempts = 10000;
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
                if (generationSteps.Count <= 1)
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

                        if (roomData == placed.data)
                            continue;

                        if (Vector3.Angle(doorway.direction, -candidateDir) > 1f)
                            continue;

                        Quaternion rot = Quaternion.FromToRotation(candidateDir, -doorway.direction) * yRot;
                        Vector3 worldTarget = doorway.position;
                        Vector3 candidateLocal = candidateDoor.localPosition;
                        Vector3 offset = worldTarget - (rot * candidateLocal);

                        var newRoomGO = Instantiate(roomData.prefab, offset, rot);
                        Bounds newBounds = GetRoomBounds(newRoomGO);
                        Debug.Log($"NEW ROOM prefab={roomData.prefab.name} bounds: center={newBounds.center}, size={newBounds.size}");

                        // Sprawdzenie "œlepych drzwi" dla nowego pokoju
                        bool badDoorInWall = false;
                        var newDoorways = GetDoorwaysWorld(newRoomGO, roomData);
                        for (int dwIdx = 0; dwIdx < newDoorways.Count; dwIdx++)
                        {
                            if (dwIdx == j) continue;
                            var testDoor = newDoorways[dwIdx];
                            foreach (var other in placedRooms)
                            {
                                if (other == placed) continue;
                                if (other.bounds.Contains(testDoor.position))
                                {
                                    bool hasMatchingDoor = false;
                                    for (int o = 0; o < other.doorways.Count; o++)
                                    {
                                        var odw = other.doorways[o];
                                        if (Vector3.Distance(odw.position, testDoor.position) < 0.2f &&
                                            Vector3.Angle(odw.direction, -testDoor.direction) < 5f)
                                        {
                                            hasMatchingDoor = true;
                                            break;
                                        }
                                    }
                                    if (!hasMatchingDoor)
                                    {
                                        badDoorInWall = true;
                                        break;
                                    }
                                }
                            }
                            if (badDoorInWall) break;
                        }
                        if (badDoorInWall)
                        {
                            Destroy(newRoomGO);
                            continue;
                        }

                        // Sprawdzenie czy istniej¹ce drzwi nie prowadz¹ w œcianê nowego pokoju
                        bool otherDoorInNewWall = false;
                        foreach (var other in placedRooms)
                        {
                            for (int o = 0; o < other.doorways.Count; o++)
                            {
                                var odw = other.doorways[o];
                                if (other.IsDoorConnected(o)) continue;
                                if (newBounds.Contains(odw.position))
                                {
                                    bool hasMatchingDoor = false;
                                    for (int nd = 0; nd < newDoorways.Count; nd++)
                                    {
                                        var ndw = newDoorways[nd];
                                        if (Vector3.Distance(ndw.position, odw.position) < 0.2f &&
                                            Vector3.Angle(ndw.direction, -odw.direction) < 5f)
                                        {
                                            hasMatchingDoor = true;
                                            break;
                                        }
                                    }
                                    if (!hasMatchingDoor)
                                    {
                                        otherDoorInNewWall = true;
                                        break;
                                    }
                                }
                            }
                            if (otherDoorInNewWall) break;
                        }
                        if (otherDoorInNewWall)
                        {
                            Destroy(newRoomGO);
                            continue;
                        }

                        if (IsColliding(newBounds))
                        {
                            Debug.LogWarning($"KOLIZJA podczas próby dodania {roomData.prefab.name} w {newBounds.center} (size={newBounds.size})");
                            Destroy(newRoomGO);
                            continue;
                        }
                        if (IsInForbiddenHemisphere(newBounds.center, startRoomPosition, forbiddenDirection, forbiddenMargin))
                        {
                            Debug.LogWarning($"POKÓJ {roomData.prefab.name} znalaz³ siê w zabronionej pó³sferze!");
                            Destroy(newRoomGO);
                            continue;
                        }

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
            if (placed != null && BoundsTouchOrOverlap(placed.bounds, newBounds))
            {
                Debug.LogWarning($"Collision between {placed.room.name} and new room at {newBounds.center}");
                return true;
            }
        }
        return false;
    }

    bool BoundsTouchOrOverlap(Bounds a, Bounds b)
    {
        float tolerance = 0.05f;
        if (a.max.x - tolerance <= b.min.x || b.max.x - tolerance <= a.min.x) return false;
        if (a.max.y - tolerance <= b.min.y || b.max.y - tolerance <= a.min.y) return false;
        if (a.max.z - tolerance <= b.min.z || b.max.z - tolerance <= a.min.z) return false;
        return true;
    }

    Bounds GetRoomBounds(GameObject roomGO)
    {
        var colliders = roomGO.GetComponentsInChildren<Collider>();
        if (colliders.Length > 0)
        {
            Bounds bounds = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++)
                bounds.Encapsulate(colliders[i].bounds);
            bounds.Expand(-0.2f);
            return bounds;
        }
        var renderers = roomGO.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);
            bounds.Expand(-0.2f);
            return bounds;
        }
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

    // --- Spawnowanie drzwi po wygenerowaniu poziomu ---
    void SpawnDoors()
    {
        foreach (var placed in placedRooms)
        {
            for (int i = 0; i < placed.doorways.Count; i++)
            {
                var dw = placed.doorways[i];

                // Czy za tymi drzwiami jest inny pokój z drzwiami naprzeciwko?
                bool hasExit = false;
                foreach (var other in placedRooms)
                {
                    if (other == placed) continue;
                    foreach (var odw in other.doorways)
                    {
                        if (
                            Vector3.Distance(odw.position, dw.position) < 0.2f &&
                            Vector3.Angle(odw.direction, -dw.direction) < 5f
                        )
                        {
                            hasExit = true;
                            break;
                        }
                    }
                    if (hasExit) break;
                }

                // Losowy prefab z listy
                GameObject prefab = null;
                if (hasExit && doorActivePrefabs != null && doorActivePrefabs.Count > 0)
                {
                    prefab = doorActivePrefabs[Random.Range(0, doorActivePrefabs.Count)];
                }
                else if (!hasExit && doorClosedPrefabs != null && doorClosedPrefabs.Count > 0)
                {
                    prefab = doorClosedPrefabs[Random.Range(0, doorClosedPrefabs.Count)];
                }

                if (prefab != null)
                {
                    GameObject go = GameObject.Instantiate(prefab, dw.position, Quaternion.LookRotation(dw.direction), placed.room.transform);
                }
            }
        }
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