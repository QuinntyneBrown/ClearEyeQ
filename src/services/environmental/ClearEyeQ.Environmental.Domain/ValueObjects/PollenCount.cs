namespace ClearEyeQ.Environmental.Domain.ValueObjects;

public sealed record PollenCount(int Tree, int Grass, int Weed, string OverallLevel)
{
    public static string ClassifyOverallLevel(int tree, int grass, int weed)
    {
        var maxCount = Math.Max(tree, Math.Max(grass, weed));
        return maxCount switch
        {
            <= 20 => "Low",
            <= 80 => "Moderate",
            <= 200 => "High",
            _ => "Very High"
        };
    }
}
