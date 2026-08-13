namespace TheoryOfVictory.Core;

/// <summary>A scripted card play: V1.0 has no dice, the calendar is written in advance.</summary>
public sealed class ScheduledCard
{
    public required int Turn { get; init; }

    public required string CardCode { get; init; }
}

/// <summary>A doctrine change written into the scenario, standing in for a player decision.</summary>
public sealed class DoctrineShift
{
    public required int Turn { get; init; }

    public required string SideCode { get; init; }

    public required Doctrine Doctrine { get; init; }

    public string? Reason { get; init; }
}

/// <summary>
/// A full deterministic run: the starting position, the oil price calendar,
/// the doctrines, and the cards that fall on which turn.
/// </summary>
public sealed class Scenario
{
    public required string Code { get; init; }

    public required string Title { get; init; }

    public string Subtitle { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public required int StartYear { get; init; }

    public required Season StartSeason { get; init; }

    public required int TurnCount { get; init; }

    /// <summary>
    /// First turn on which the front and the deep-strike campaigns are resolved at all. Turns
    /// before it are prologue: forces are generated, budgets are spent, factories run — and not
    /// a shot is fired.
    ///
    /// It has to SKIP the two phases rather than neutralise them. Setting both offensive postures
    /// to zero stops every hex from moving, but <c>ResolveSector</c> applies its losses before it
    /// looks at the result: the quietest quarter of the run would still cost tens of thousands of
    /// men, and the strike phases would still report saturation on campaigns nobody launched.
    ///
    /// One means no prologue, which is the historical default for a scenario opening on its war.
    /// </summary>
    public int CombatStartsOnTurn { get; init; } = 1;

    /// <summary>
    /// How the war ends once a side breaks. Owned by the scenario because the calendar is: read
    /// <see cref="AftermathRules.QuartersToArmistice"/> to know how many quarters to leave after
    /// the rupture. The default dissolves an army over four quarters.
    /// </summary>
    public AftermathRules Aftermath { get; set; } = new();

    /// <summary>Brent per turn, written in advance. V1.1 replaces this with a process.</summary>
    public List<double> OilPriceCalendar { get; init; } = [];

    public required Belligerent Invader { get; init; }

    public required Belligerent Defender { get; init; }

    public required List<FrontSector> Sectors { get; init; }

    public required Doctrine InvaderDoctrine { get; init; }

    public required Doctrine DefenderDoctrine { get; init; }

    public List<DoctrineShift> DoctrineShifts { get; init; } = [];

    public List<ScheduledCard> Calendar { get; init; } = [];

    public List<EventCard> Deck { get; init; } = [];

    public double OilPriceAt(int turn)
    {
        if (OilPriceCalendar.Count == 0)
        {
            return 80d;
        }

        int index = Math.Clamp(turn - 1, 0, OilPriceCalendar.Count - 1);
        return OilPriceCalendar[index];
    }
}
