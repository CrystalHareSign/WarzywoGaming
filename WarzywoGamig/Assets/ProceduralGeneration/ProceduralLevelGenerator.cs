using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class ItemEntry
{
    public ItemPrefabData itemData;
    [Range(0f, 1f)]
    public float spawnChance = 0.2f;
    [Tooltip("Maksymalna liczba tego itemu na mapie (0 = wyliczana automatycznie)")]
    public int maxCount = 0;
}

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
    public List<GameObject> doorClosedPrefabs;
    public List<GameObject> doorActivePrefabs;

    // ---- ITEM SYSTEM ----
    [Header("Item Spawning")]
    public List<ItemEntry> items;

    [Header("Loot Enrichment")]
    [Range(1, 5)]
    [Tooltip("Poziom wzbogacenia poziomu w loot: 1 = bardzo ma³o, 5 = bardzo du¿o")]
    public int lootLevel = 3;

    [Tooltip("Dodatkowy mno¿nik szansy na pojawienie siê przedmiotu za ka¿dy pokój oddalony od pokoju startowego.")]
    public float distanceBonusPerRoom = 0.15f;

    [Tooltip("Maksymalny ³¹czny mno¿nik szansy wynikaj¹cy z oddalenia od startowego pokoju.")]
    public float maxDistanceBonus = 2.0f;

    [Tooltip("Wspó³czynnik zmniejszaj¹cy szansê na pojawienie siê kolejnego przedmiotu w tym samym pokoju.")]
    public float roomSpawnPenalty = 0.7f;

    [Tooltip("Bazowa szansa na pojawienie siê przedmiotu w danym miejscu przed uwzglêdnieniem wszystkich bonusów i kar.")]
    public float baseSpawnChance = 0.5f;

    [HideInInspector]
    public int maxItemsOnMap = 10;

    private static readonly float[] LootLevelMultipliers = { 0.7f, 1.0f, 1.2f, 1.5f, 2.0f };

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
        if (MissionSettings.roomCount > 0)
            roomCount = MissionSettings.roomCount;

        AutoBalanceItemSpawnParameters();

        if (autoGenerateOnStart)
            GenerateLevel();
    }

    void AutoBalanceItemSpawnParameters()
    {
        int rooms = roomCount;
        int lootIdx = Mathf.Clamp(lootLevel, 1, 5) - 1;
        float multiplier = LootLevelMultipliers[lootIdx];

        maxItemsOnMap = Mathf.Clamp(Mathf.RoundToInt(rooms * multiplier), 1, 9999);

        baseSpawnChance = Mathf.Clamp01(1.1f / rooms + 0.25f);

        int maxDist = Mathf.Max(rooms - 1, 1);
        maxDistanceBonus = 2.0f;
        distanceBonusPerRoom = (maxDistanceBonus - 1f) / maxDist;

        roomSpawnPenalty = Mathf.Lerp(0.5f, 0.85f, Mathf.InverseLerp(5, 30, rooms));

        // --- Automatyczne, lekko losowe maxCount dla itemów ---
        float sumChances = 0f;
        foreach (var it in items)
            sumChances += Mathf.Max(0f, it.spawnChance);

        foreach (var it in items)
        {
            // Jeœli maxCount <= 0, licz automatycznie. Pozostaw wartoœæ jeœli ustawiono w Inspectorze.
            if (it.maxCount <= 0)
            {
                float softLimit = maxItemsOnMap * it.spawnChance / sumChances;
                float boost = Random.Range(1.10f, 1.25f); // 10–25% nadwy¿ki
                it.maxCount = Mathf.Max(1, Mathf.RoundToInt(softLimit * boost));
            }
        }
    }

    private System.Collections.IEnumerator GenerateLevelCoroutine()
    {
        ClearPreviousLevel();
        bool success = TryGenerateLevelWithBacktracking();
        if (success)
        {
            SpawnDoors();
            SpawnItemsInRooms();
            ValidateDoorwaysNoWall();
        }
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
        Quaternion yRot = Quaternion.identity;
        GameObject startRoomGO = Instantiate(startRoomData.prefab, startRoomPositionOverride, yRot);
        Bounds startBounds = GetRoomBounds(startRoomGO);
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

                bool placedRoom = false;

                foreach (int angle in yAngles)
                {
                    if (placedRoom) break;

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
                        var newDoorways = GetDoorwaysWorld(newRoomGO, roomData);

                        float tolerance = 0.09f;
                        float angleTolerance = 3f;
                        bool invalid = false;

                        // WALIDACJA 1: Nowy doorway nie mo¿e byæ w œcianie istniej¹cego pokoju
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
                                    foreach (var odw in other.doorways)
                                    {
                                        float dist = Vector3.Distance(odw.position, testDoor.position);
                                        float angleDiff = Vector3.Angle(odw.direction, -testDoor.direction);
                                        if (dist < tolerance && angleDiff < angleTolerance)
                                        {
                                            hasMatchingDoor = true;
                                            break;
                                        }
                                    }
                                    if (!hasMatchingDoor)
                                    {
                                        // Walidacja raycastem w œcianê istniej¹cego pokoju
                                        Vector3 from = testDoor.position - testDoor.direction * 0.2f;
                                        Ray ray = new Ray(from, testDoor.direction);
                                        if (Physics.Raycast(ray, out RaycastHit hit, 0.4f))
                                        {
                                            if (hit.collider.transform.IsChildOf(other.room.transform) && !IsDoorCollider(hit.collider))
                                            {
                                                Debug.LogError($"[GENERATOR BLOCKED] Doorway nowego pokoju w œcianie istniej¹cego (raycast): {testDoor.position} w bounds {other.room.name} ({other.bounds.center})");
                                                Destroy(newRoomGO);
                                                invalid = true;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            Debug.LogError($"[GENERATOR BLOCKED] Doorway nowego pokoju w œcianie istniej¹cego: {testDoor.position} w bounds {other.room.name} ({other.bounds.center})");
                                            Destroy(newRoomGO);
                                            invalid = true;
                                            break;
                                        }
                                    }
                                }
                                if (invalid) break;
                            }
                            if (invalid) break;
                        }
                        if (invalid)
                        {
                            continue;
                        }

                        // WALIDACJA 2: Doorwaye istniej¹cych pokoi nie mog¹ byæ w œcianie nowego pokoju
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
                                        float dist = Vector3.Distance(ndw.position, odw.position);
                                        float angleDiff = Vector3.Angle(ndw.direction, -odw.direction);
                                        if (dist < tolerance && angleDiff < angleTolerance)
                                        {
                                            hasMatchingDoor = true;
                                            break;
                                        }
                                    }
                                    if (!hasMatchingDoor)
                                    {
                                        // Walidacja raycastem w œcianê nowego pokoju
                                        Vector3 from = odw.position - odw.direction * 0.2f;
                                        Ray ray = new Ray(from, odw.direction);
                                        if (Physics.Raycast(ray, out RaycastHit hit, 0.4f))
                                        {
                                            if (hit.collider.transform.IsChildOf(newRoomGO.transform) && !IsDoorCollider(hit.collider))
                                            {
                                                Debug.LogError($"[GENERATOR BLOCKED] Doorway istniej¹cego pokoju w œcianie nowego (raycast): {odw.position} w bounds {newBounds.center} ({newRoomGO.name})");
                                                Destroy(newRoomGO);
                                                invalid = true;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            Debug.LogError($"[GENERATOR BLOCKED] Doorway istniej¹cego pokoju w œcianie nowego: {odw.position} w bounds {newBounds.center} ({newRoomGO.name})");
                                            Destroy(newRoomGO);
                                            invalid = true;
                                            break;
                                        }
                                    }
                                }
                                if (invalid) break;
                            }
                            if (invalid) break;
                        }
                        if (invalid)
                        {
                            continue;
                        }

                        // Pozosta³e walidacje
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

                        // Jeœli dotarliœmy tu — pokój jest OK!
                        var newPlaced = new PlacedRoom(newRoomGO, roomData, newDoorways, newBounds);
                        placedRooms.Add(newPlaced);
                        placed.MarkDoorConnected(i);
                        newPlaced.MarkDoorConnected(j);

                        usedRoomIdx = roomIdx;
                        usedParentDoor = i;
                        usedThisDoor = j;
                        placedRoom = true;
                        break;
                    }
                }
                if (placedRoom)
                    return true;
            }
        }
        return false;
    }

    // Funkcja pomocnicza - mo¿esz dostosowaæ do swoich potrzeb, np. po nazwie, layerze, tagu lub zawsze false
    bool IsDoorCollider(Collider col)
    {
        // Jeœli nie masz osobnych colliderów drzwi, zwracaj zawsze false
        return false;
        // Jeœli chcesz sprawdzaæ po nazwie:
        // return col.gameObject.name.ToLower().Contains("door");
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

    void SpawnDoors()
    {
        foreach (var placed in placedRooms)
        {
            for (int i = 0; i < placed.doorways.Count; i++)
            {
                var dw = placed.doorways[i];
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

    // ---- SPAWN ITEMÓW ----
    void SpawnItemsInRooms()
    {
        var allSpawnPoints = new List<(PlacedRoom room, Vector3 localPos)>();
        for (int idx = 0; idx < placedRooms.Count; idx++)
        {
            var room = placedRooms[idx];
            // Pomijamy pokój startowy (zwykle jest pierwszy na liœcie)
            if (idx == 0)
                continue;
            if (room.data.itemSpawnPoints == null) continue;
            foreach (var pos in room.data.itemSpawnPoints)
                allSpawnPoints.Add((room, pos));
        }
        if (allSpawnPoints.Count == 0 || items == null || items.Count == 0) return;

        var roomDistances = ComputeRoomDistances(placedRooms);
        int maxDist = 1;
        foreach (var d in roomDistances.Values)
            if (d > maxDist) maxDist = d;

        var itemsInRoom = new Dictionary<PlacedRoom, int>();
        var itemTypeCounter = new Dictionary<ItemEntry, int>();
        foreach (var r in placedRooms) itemsInRoom[r] = 0;
        foreach (var it in items) itemTypeCounter[it] = 0;

        Shuffle(allSpawnPoints);

        int spawned = 0;
        foreach (var (room, localPos) in allSpawnPoints)
        {
            if (spawned >= maxItemsOnMap) break;

            float normDist = roomDistances.TryGetValue(room, out int dist) ? (float)dist / (float)maxDist : 0f;
            float distanceBonus = 1.0f + Mathf.Min(normDist * distanceBonusPerRoom * maxDist, maxDistanceBonus - 1.0f);
            float roomPenalty = Mathf.Pow(roomSpawnPenalty, itemsInRoom[room]);
            float finalChance = baseSpawnChance * distanceBonus * roomPenalty;

            if (Random.value > finalChance) continue;

            var entry = PickWeightedItem(items, itemTypeCounter);
            if (entry == null || entry.itemData == null || entry.itemData.prefab == null) continue;

            if (entry.maxCount > 0 && itemTypeCounter[entry] >= entry.maxCount)
                continue; // respektuj limit typu

            Vector3 worldPos = room.room.transform.TransformPoint(localPos);
            Instantiate(entry.itemData.prefab, worldPos, Quaternion.identity, room.room.transform);

            spawned++;
            itemsInRoom[room]++;
            itemTypeCounter[entry]++;
        }
    }

    Dictionary<PlacedRoom, int> ComputeRoomDistances(List<PlacedRoom> placedRooms)
    {
        var distances = new Dictionary<PlacedRoom, int>();
        if (placedRooms.Count == 0) return distances;
        var queue = new Queue<PlacedRoom>();
        distances[placedRooms[0]] = 0;
        queue.Enqueue(placedRooms[0]);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            int dist = distances[current];
            foreach (var neighbor in GetConnectedNeighbors(current, placedRooms))
            {
                if (!distances.ContainsKey(neighbor))
                {
                    distances[neighbor] = dist + 1;
                    queue.Enqueue(neighbor);
                }
            }
        }
        return distances;
    }

    List<PlacedRoom> GetConnectedNeighbors(PlacedRoom room, List<PlacedRoom> allRooms)
    {
        var neighbors = new List<PlacedRoom>();
        foreach (var other in allRooms)
        {
            if (other == room) continue;
            foreach (var dw in room.doorways)
                foreach (var odw in other.doorways)
                    if (Vector3.Distance(dw.position, odw.position) < 0.2f && Vector3.Angle(dw.direction, -odw.direction) < 5f)
                        neighbors.Add(other);
        }
        return neighbors;
    }

    ItemEntry PickWeightedItem(List<ItemEntry> list, Dictionary<ItemEntry, int> counters)
    {
        var eligible = new List<ItemEntry>();
        foreach (var i in list)
            if (i.maxCount == 0 || counters[i] < i.maxCount)
                eligible.Add(i);
        if (eligible.Count == 0) return null;
        float sum = 0f;
        foreach (var i in eligible) sum += Mathf.Max(0f, i.spawnChance);
        if (sum <= 0f) return null;
        float roll = Random.value * sum, acc = 0f;
        foreach (var i in eligible)
        {
            acc += Mathf.Max(0f, i.spawnChance);
            if (roll <= acc) return i;
        }
        return eligible[eligible.Count - 1];
    }

    void ValidateDoorwaysNoWall()
    {
        float wallEdgeTolerance = 0.05f;
        float matchingDoorTolerance = 0.13f;
        float angleTolerance = 5f;

        for (int i = 0; i < placedRooms.Count; i++)
        {
            var roomA = placedRooms[i];
            for (int d = 0; d < roomA.doorways.Count; d++)
            {
                var dwA = roomA.doorways[d];
                for (int j = 0; j < placedRooms.Count; j++)
                {
                    if (i == j) continue;
                    var roomB = placedRooms[j];

                    var testBounds = roomB.bounds;
                    testBounds.Expand(-0.05f);
                    Vector3 closest = testBounds.ClosestPoint(dwA.position);
                    float wallDist = Vector3.Distance(dwA.position, closest);

                    if (testBounds.Contains(dwA.position) && wallDist > wallEdgeTolerance)
                    {
                        // SprawdŸ czy jest doorway naprzeciw w roomB
                        bool foundMatch = false;
                        foreach (var dwB in roomB.doorways)
                        {
                            float dist = Vector3.Distance(dwA.position, dwB.position);
                            float angle = Vector3.Angle(dwA.direction, -dwB.direction);
                            if (dist < matchingDoorTolerance && angle < angleTolerance)
                            {
                                foundMatch = true;
                                break;
                            }
                        }
                        if (!foundMatch)
                        {
                            Debug.LogError(
                                $"[WALL CHECK] Doorway Z POKOJU {roomA.room.name} (pos={dwA.position}, dir={dwA.direction}) " +
                                $"jest w scianie pokoju {roomB.room.name}!\n" +
                                $"Bounds center: {testBounds.center}, size: {testBounds.size}, dystans do œciany: {wallDist}\n" +
                                $"DOORWAYS w roomB:\n" +
                                string.Join("\n", roomB.doorways.Select(dw => $"pos={dw.position}, dir={dw.direction}"))
                            );
                        }
                    }
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