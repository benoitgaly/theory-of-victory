using TheoryOfVictory.Core.Localization;

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
    /// <summary>Le nom de la saison, dans la langue du lecteur et nulle part ailleurs.</summary>
    public static LocalizedText Label(this Season season)
    {
        return LocalizedText.Of(season switch
        {
            Season.Winter => TextCodes.Season.Winter,
            Season.Spring => TextCodes.Season.Spring,
            Season.Summer => TextCodes.Season.Summer,
            _ => TextCodes.Season.Autumn,
        });
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
