public static class MissionSettings
{
    public static string locationName;
    public static int roomCount;
    public static MissionLocationType locationType;

    public static bool IsRouteOnly => locationType == MissionLocationType.RouteOnly;
    public static bool IsProceduralRaid => locationType == MissionLocationType.ProceduralRaid;
}