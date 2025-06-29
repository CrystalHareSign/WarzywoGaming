using System.Collections.Generic;

public static class MissionSettings
{
    public static string locationName;
    public static int roomCount;
    public static MissionLocationType locationType;
    public static float totalDistanceKm;
    public static float dangerZoneKm;

    public static int lootLevel = 3; // domyœlny poziom loot enrichment (1-5)

    // S³ownik: klucz to itemName z ItemPrefabData, wartoœæ to maxCount
    public static Dictionary<string, int> itemMaxCounts = new();

    public static bool IsRouteOnly => locationType == MissionLocationType.RouteOnly;
    public static bool IsProceduralRaid => locationType == MissionLocationType.ProceduralRaid;
}