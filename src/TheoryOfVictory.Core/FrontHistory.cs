namespace TheoryOfVictory.Core;

/// <summary>
/// How the sentence of a quarter finds the quarter it belongs to, now that the chronicle and
/// its prose live in two files. Year and season: the two things about a quarter that cannot
/// move.
/// </summary>
public static class FrontHistoryIds
{
    public static string Quarter(int year, Season season)
    {
        return $"{year}|{season}";
    }
}

/// <summary>
/// The real position of the front at the END of one quarter, expressed in the twenty-zone
/// vocabulary of <see cref="FrontHistory"/>. This is a chronicle, never an output of the model:
/// nothing in this file is computed, everything in it is sourced.
///
/// Any zone absent from the three lists is held by the defender. The one exception is
/// <c>kursk_incursion</c>, which is Russian soil and therefore sits in <see cref="HeldByInvader"/>
/// whenever nobody has crossed into it — "held by the invader" there means "at home".
/// </summary>
public sealed class FrontQuarter
{
    public required int Year { get; init; }

    public required Season Season { get; init; }

    /// <summary>Zones under the invader's control at the close of the quarter.</summary>
    public IReadOnlyList<string> HeldByInvader { get; init; } = [];

    /// <summary>
    /// Zones neither side owns outright. Two distinct cases, and the same rule for both: urban
    /// combat still running, and divergence between sources. In doubt, contested — never a ruling.
    /// </summary>
    public IReadOnlyList<string> Contested { get; init; } = [];

    /// <summary>Ground the defender holds INSIDE the invader's country. Kursk, and nothing else.</summary>
    public IReadOnlyList<string> HeldByDefender { get; init; } = [];

    /// <summary>What happened, in one sentence, with its dates.</summary>
    public required string Headline { get; init; }

    public IReadOnlyList<string> Sources { get; init; } = [];

    /// <summary>"haute" everywhere except the quarter that is not over yet.</summary>
    public required string Confidence { get; init; }
}

/// <summary>
/// Twenty quarters of the real war, from the autumn 2021 build-up to the summer of 2026.
/// The map reads this for every documented quarter and the model for everything after it, which
/// is the rule the calendar lays down: what is reconstructed must be distinguishable, on screen,
/// from what the engine computes.
/// </summary>
public sealed class FrontHistory
{
    /// <summary>The twenty zone codes. Anything outside this list is a data error, not a place.</summary>
    public IReadOnlyList<string> Vocabulary { get; init; } = [];

    public IReadOnlyList<FrontQuarter> Quarters { get; init; } = [];

    /// <summary>The quarter of that calendar slot, or null once the documented period is over.</summary>
    public FrontQuarter? At(int year, Season season)
    {
        foreach (FrontQuarter quarter in Quarters)
        {
            if (quarter.Year == year && quarter.Season == season)
            {
                return quarter;
            }
        }

        return null;
    }

    /// <summary>The last documented quarter: the position the model projects forward from.</summary>
    public FrontQuarter? Last
    {
        get { return Quarters.Count == 0 ? null : Quarters[^1]; }
    }
}
