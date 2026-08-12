namespace TheoryOfVictory.Core;

/// <summary>Three-month turns make one turn in four the winter crisis turn.</summary>
public enum Season
{
    Winter = 0,
    Spring = 1,
    Summer = 2,
    Autumn = 3,
}

public static class SeasonExtensions
{
    public static string ToFrench(this Season season)
    {
        return season switch
        {
            Season.Winter => "Hiver",
            Season.Spring => "Printemps",
            Season.Summer => "Été",
            Season.Autumn => "Automne",
            _ => season.ToString(),
        };
    }

    /// <summary>Mud season halves mobility, a real constraint on spring and autumn offensives.</summary>
    public static double OffensiveModifier(this Season season)
    {
        return season switch
        {
            Season.Winter => 0.95d,
            Season.Spring => 0.75d,
            Season.Summer => 1.1d,
            Season.Autumn => 0.8d,
            _ => 1d,
        };
    }
}
