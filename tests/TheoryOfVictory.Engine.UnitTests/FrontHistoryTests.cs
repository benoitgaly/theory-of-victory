using TheoryOfVictory.Core;
using TheoryOfVictory.Engine.Scenarios;
using Xunit;

namespace TheoryOfVictory.Engine.UnitTests;

/// <summary>
/// The chronicle of the real front is data the BOARD reads and the engine does not. These tests
/// lock the two things that make that separation safe: the file has to be loadable and internally
/// consistent, and it has to line up with the calendar quarter for quarter — a chronicle that
/// slipped a season against the game would put the fall of Mariupol on the wrong turn and nothing
/// would fail. They also check the one convention that would be invisible if it broke: Kursk is
/// Russian soil, so "held by the invader" there means "at home", and the day that line goes
/// missing the map paints an oblast of Russia as Ukrainian without complaining.
/// </summary>
public sealed class FrontHistoryTests
{
    private static readonly FrontHistory History = FrontHistoryLibrary.Load();

    [Fact]
    public void TheChronicle_LoadsAndNamesTwentyZones_OverTwentyQuarters()
    {
        Assert.Equal(20, History.Vocabulary.Count);
        Assert.Equal(20, History.Quarters.Count);
        Assert.All(History.Quarters, quarter => Assert.False(string.IsNullOrWhiteSpace(quarter.Headline)));
        Assert.All(History.Quarters, quarter => Assert.NotEmpty(quarter.Sources));
    }

    /// <summary>
    /// The join the map depends on is (year, season) and nothing else. If the scenario ever moves
    /// its opening again — it has moved once already, from the invasion back to the autumn 2021
    /// build-up — this is what catches the chronicle sliding out from under it.
    /// </summary>
    [Fact]
    public void EveryDocumentedQuarter_LandsOnATurnOfTheRun()
    {
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Resolve));

        foreach (FrontQuarter quarter in History.Quarters)
        {
            Assert.True(
                game.Turns.Any(turn => turn.Year == quarter.Year && turn.Season == quarter.Season),
                $"{quarter.Season} {quarter.Year} is documented but is not a quarter of the run.");
        }

        // And the handover is where the design says it is: the first twenty turns, in order.
        for (int i = 0; i < History.Quarters.Count; i++)
        {
            Assert.Equal(History.Quarters[i].Year, game.Turns[i].Year);
            Assert.Equal(History.Quarters[i].Season, game.Turns[i].Season);
        }

        Assert.True(game.Turns.Count > History.Quarters.Count,
            "The run has to go past the chronicle, or the map would never show a projection.");
    }

    [Fact]
    public void TheOpeningQuarter_IsTheLineOf2014_AndNothingElse()
    {
        FrontQuarter opening = History.Quarters[0];

        Assert.Equal(2021, opening.Year);
        Assert.Equal(Season.Autumn, opening.Season);
        Assert.Empty(opening.Contested);
        Assert.Empty(opening.HeldByDefender);
        Assert.Equal(
            ["crimea", "donbas_2014", "kursk_incursion"],
            opening.HeldByInvader.OrderBy(zone => zone, StringComparer.Ordinal).ToArray());
    }

    /// <summary>
    /// Kursk is the exception to the rule that an unlisted zone belongs to the defender, and the
    /// only zone that ever appears in <c>heldByDefender</c>. Both halves are checked, because
    /// dropping either one silently changes what the map claims about a piece of Russia.
    /// </summary>
    [Fact]
    public void Kursk_IsNeverUnlisted_AndIsTheOnlyGroundTheDefenderEverHoldsAbroad()
    {
        foreach (FrontQuarter quarter in History.Quarters)
        {
            bool named = quarter.HeldByInvader.Contains("kursk_incursion")
                || quarter.Contested.Contains("kursk_incursion")
                || quarter.HeldByDefender.Contains("kursk_incursion");

            Assert.True(named, $"{quarter.Season} {quarter.Year} leaves Kursk unlisted, which reads as Ukrainian.");
            Assert.All(quarter.HeldByDefender, zone => Assert.Equal("kursk_incursion", zone));
        }

        Assert.Contains(History.Quarters, quarter => quarter.HeldByDefender.Contains("kursk_incursion"));
    }

    /// <summary>
    /// The map draws twenty zones and the file names twenty zones — but nothing forces the two
    /// lists to be the same list, and a zone the map cannot place would simply not be painted.
    /// The outlines live in JavaScript, so this test holds the codes the drawing knows about.
    /// </summary>
    [Fact]
    public void EveryZoneOfTheVocabulary_HasAnOutlineOnTheMap()
    {
        string[] drawn =
        [
            "crimea", "donbas_2014", "kyiv_axis", "chernihiv_axis", "sumy_axis",
            "kharkiv_north", "izioum", "lyman", "severodonetsk", "bakhmout",
            "avdiivka", "vouhledar", "pokrovsk", "koupiansk", "mariupol",
            "melitopol", "zaporijjia_south", "kherson_right", "kherson_left",
            "kursk_incursion",
        ];

        Assert.Equal(
            drawn.OrderBy(zone => zone, StringComparer.Ordinal),
            History.Vocabulary.OrderBy(zone => zone, StringComparer.Ordinal));
    }
}
