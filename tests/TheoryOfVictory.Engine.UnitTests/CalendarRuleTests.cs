using TheoryOfVictory.Core;
using TheoryOfVictory.Engine.Scenarios;
using Xunit;

namespace TheoryOfVictory.Engine.UnitTests;

/// <summary>
/// The rule of the game the screen is supposed to be showing: a side plays one card per
/// quarter and never two. It is enforced by the shape of the calendar — two slot tables
/// keyed by quarter — and checked here, because the shape is a convention and a convention
/// is one refactoring away from being lost.
/// </summary>
public sealed class CalendarRuleTests
{
    private static readonly SupportVariant[] AllVariants =
        [SupportVariant.Resolve, SupportVariant.Holds, SupportVariant.Collapses];

    [Fact]
    public void AQuarter_HoldsOneDecisionPerSide_AndNeverTwo()
    {
        foreach (SupportVariant variant in AllVariants)
        {
            Scenario scenario = UkraineScenario.Build(variant);
            Dictionary<(int Turn, string Side), List<string>> played = [];

            foreach (ScheduledCard scheduled in scenario.Calendar)
            {
                EventCard card = Find(scenario, scheduled.CardCode);
                string side = card.OwnerSideCode ?? "monde";
                (int, string) slot = (scheduled.Turn, side);

                if (!played.TryGetValue(slot, out List<string>? codes))
                {
                    codes = [];
                    played[slot] = codes;
                }

                codes.Add(scheduled.CardCode);
            }

            foreach (((int turn, string side), List<string> codes) in played)
            {
                Assert.True(
                    codes.Count == 1,
                    $"{scenario.Title} — T{turn}, camp {side} : {codes.Count} cartes jouées "
                        + $"({string.Join(", ", codes)}). Un trimestre ne porte qu'une décision.");
            }
        }
    }

    /// <summary>
    /// A card owned by nobody would be a card no player decided to play, and the whole rule
    /// counts decisions per side. The deck has none, and the calendar must not acquire one.
    /// </summary>
    [Fact]
    public void EveryCardPlayed_IsOwnedByASide_NoneIsMerelySuffered()
    {
        foreach (SupportVariant variant in AllVariants)
        {
            Scenario scenario = UkraineScenario.Build(variant);

            foreach (ScheduledCard scheduled in scenario.Calendar)
            {
                EventCard card = Find(scenario, scheduled.CardCode);
                Assert.False(
                    string.IsNullOrWhiteSpace(card.OwnerSideCode),
                    $"« {card.Title} » est jouée au T{scheduled.Turn} sans camp qui la joue.");
            }
        }
    }

    /// <summary>
    /// The calendar is a slot table written by hand, so a mistyped code would silently schedule
    /// nothing at all — the engine skips what it cannot find — and the quarter would go by empty
    /// without anyone noticing.
    /// </summary>
    [Fact]
    public void EveryScheduledCode_ExistsInTheDeck_ATypoIsNotASilentBlankQuarter()
    {
        foreach (SupportVariant variant in AllVariants)
        {
            Scenario scenario = UkraineScenario.Build(variant);

            foreach (ScheduledCard scheduled in scenario.Calendar)
            {
                Assert.True(
                    scenario.Deck.Exists(card => string.Equals(
                        card.Code,
                        scheduled.CardCode,
                        StringComparison.OrdinalIgnoreCase)),
                    $"T{scheduled.Turn} programme « {scheduled.CardCode} », qui n'est pas dans le deck.");
            }
        }
    }

    /// <summary>
    /// The cards taken off the calendar were not deleted: they stayed in the deck, because the
    /// deck is the hand the V2 will deal from. A run that played every card it holds would leave
    /// the players nothing to choose between.
    /// </summary>
    [Fact]
    public void TheDeck_IsFarLargerThanWhatIsPlayed_TheRestIsTheHand()
    {
        Scenario scenario = UkraineScenario.Build(SupportVariant.Resolve);

        HashSet<string> played = [];
        foreach (ScheduledCard scheduled in scenario.Calendar)
        {
            played.Add(scheduled.CardCode.ToLowerInvariant());
        }

        Assert.True(
            scenario.Deck.Count > played.Count * 2,
            $"{scenario.Deck.Count} cartes au deck pour {played.Count} distinctes jouées : "
                + "il ne reste presque rien à piocher.");
    }

    private static EventCard Find(Scenario scenario, string code)
    {
        EventCard? card = scenario.Deck.Find(
            candidate => string.Equals(candidate.Code, code, StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(card);
        return card!;
    }
}
