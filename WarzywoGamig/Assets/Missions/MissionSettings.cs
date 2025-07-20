using System.Collections.Generic;

public static class MissionSettings
{
    public static string locationName;
    public static int roomCount;
    public static MissionLocationType locationType;
    public static float totalDistanceKm;
    public static float dangerZoneKm;

    public static int lootLevel = 3; // enrichment
    public static int lootRarity = 2; // rarity

    // S³ownik: klucz to itemName z ItemPrefabData, wartoœæ to maxCount
    public static Dictionary<string, int> itemMaxCounts = new();

    // Dodane! Referencja do wybranej lokacji (np. z MissionLocationIcon)
    public static MissionLocationIcon selectedLocation;

    public static bool IsRouteOnly => locationType == MissionLocationType.RouteOnly;
    public static bool IsProceduralRaid => locationType == MissionLocationType.ProceduralRaid;
}