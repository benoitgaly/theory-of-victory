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

    public double PoliticalCost { get; init; }

    public double MoneyCost { get; init; }

    /// <summary>Rules text, one readable line per effect.</summary>
    public List<string> RulesText { get; init; } = [];

    /// <summary>Illustration key, drives which artwork the frame renders.</summary>
    public required string Art { get; init; }
}
