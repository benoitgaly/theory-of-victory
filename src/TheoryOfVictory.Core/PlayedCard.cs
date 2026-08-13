namespace TheoryOfVictory.Core;

/// <summary>
/// A card as it appears on the table: everything needed to print the frame,
/// the cost line, the rules box and the flavour text.
/// </summary>
public sealed class PlayedCard
{
    public required string Code { get; init; }

    public required string Title { get; init; }

    public required string Family { get; init; }

    public required string TypeLine { get; init; }

    public required string Description { get; init; }

    /// <summary>invader, defender, or null for a world card both sides suffer.</summary>
    public string? OwnerSideCode { get; init; }

    /// <summary>
    /// Sides this card actually lands on, read from its effects. An unowned card still
    /// hits someone, and it belongs on that side's screen rather than in the resolution.
    /// </summary>
    public List<string> AffectedSideCodes { get; init; } = [];

    public double PoliticalCost { get; init; }

    public double MoneyCost { get; init; }

    /// <summary>Rules text, one readable line per effect.</summary>
    public List<string> RulesText { get; init; } = [];

    /// <summary>Card this one answers, when it is a counter.</summary>
    public string? CountersCardCode { get; init; }

    /// <summary>True when an opposing counter stopped this card: it was played and did nothing.</summary>
    public bool Countered { get; set; }

    /// <summary>
    /// False when the owner did not hold the political capital its cost demanded. V1.0 plays
    /// the calendar regardless, but records the overdraft — it is the V2 currency being
    /// tested against a real run before anyone has to pay it.
    /// </summary>
    public bool AffordedInFull { get; set; } = true;

    /// <summary>Illustration key, drives which artwork the frame renders.</summary>
    public required string Art { get; init; }
}
