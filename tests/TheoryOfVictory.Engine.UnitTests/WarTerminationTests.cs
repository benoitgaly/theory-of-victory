using TheoryOfVictory.Core;
using TheoryOfVictory.Engine;
using TheoryOfVictory.Engine.Scenarios;
using Xunit;

namespace TheoryOfVictory.Engine.UnitTests;

/// <summary>
/// Locks how a war ends, and nothing else. Not a single assertion here names a turn number, a
/// year, or which side breaks: the calendar and the calibration belong to the scenario and move
/// under this file on purpose. What must not move is the mechanism — an army that stops being
/// paid dissolves, a front nobody holds gives way, and the war ends on a named armistice.
/// </summary>
public sealed class WarTerminationTests
{
    private static readonly SupportVariant[] AllVariants =
        [SupportVariant.Resolve, SupportVariant.Holds, SupportVariant.Collapses];

    /// <summary>Every run of every variant, so no test has to bet on which one breaks.</summary>
    private static IEnumerable<(Scenario Scenario, PlayedGame Game)> AllRuns()
    {
        foreach (SupportVariant variant in AllVariants)
        {
            Scenario scenario = UkraineScenario.Build(variant);
            yield return (scenario, new GameRunner().Run(scenario));
        }
    }

    private static (Scenario Scenario, PlayedGame Game) ARunThatEndsInAnArmistice()
    {
        foreach ((Scenario scenario, PlayedGame game) in AllRuns())
        {
            if (game.Outcome?.Code == "armistice")
            {
                return (scenario, game);
            }
        }

        Assert.Fail("Aucun des trois déroulés ne se termine par un armistice : le dénouement ne tourne plus.");
        return default;
    }

    [Fact]
    public void ASideBreaking_IsNotTheEndOfTheGame_ButTheStartOfTheAftermath()
    {
        (_, PlayedGame game) = ARunThatEndsInAnArmistice();

        // The rupture is kept, and it is not the same event as the ending.
        Assert.NotNull(game.Decision);
        Assert.NotEqual("armistice", game.Decision!.Code);

        // The war was decided when the side broke, and it stopped later.
        Assert.True(
            game.Turns[^1].Turn > game.Decision.Turn,
            $"Rupture au T{game.Decision.Turn}, dernier tour joué T{game.Turns[^1].Turn} : "
                + "l'après-chute n'a pas été joué.");
    }

    /// <summary>
    /// The published turn of the outcome is the quarter the war was decided, never the quarter it
    /// stopped. The three runs have to keep landing where the design says they land.
    /// </summary>
    [Fact]
    public void TheOutcome_IsDatedOnTheRupture_NotOnTheArmistice()
    {
        (_, PlayedGame game) = ARunThatEndsInAnArmistice();

        Assert.Equal(game.Decision!.Turn, game.Outcome!.Turn);
        Assert.Equal(game.Decision.WinnerSideCode, game.Outcome.WinnerSideCode);
    }

    /// <summary>
    /// One mechanism, both directions. Whichever side breaks, the ending is the same kind of
    /// ending with the winner simply swapped — and the loser is always the one that dissolved.
    /// </summary>
    [Fact]
    public void WhicheverSideBreaks_TheOtherOneWins_AndTheBrokenOneIsTheOneThatMelted()
    {
        int armistices = 0;

        foreach ((_, PlayedGame game) in AllRuns())
        {
            if (game.Outcome?.Code != "armistice")
            {
                continue;
            }

            armistices++;
            Assert.NotNull(game.Decision);
            Assert.Equal(game.Decision!.WinnerSideCode, game.Outcome.WinnerSideCode);

            SideSnapshot broken = BrokenSideOf(game, game.Turns[^1]);
            SideSnapshot brokenAtRupture = BrokenSideOf(game, game.Turns[game.Decision.Turn - 1]);
            SideSnapshot winner = WinnerSideOf(game, game.Turns[^1]);
            SideSnapshot winnerAtRupture = WinnerSideOf(game, game.Turns[game.Decision.Turn - 1]);

            Assert.True(
                broken.MenInTheatre < brokenAtRupture.MenInTheatre * 0.5d,
                $"{game.Title} : le camp brisé garde {broken.MenInTheatre:N0} hommes sur "
                    + $"{brokenAtRupture.MenInTheatre:N0}. Il ne se dissout pas.");

            Assert.True(
                winner.MenInTheatre > winnerAtRupture.MenInTheatre * 0.5d,
                $"{game.Title} : le vainqueur fond autant que le vaincu. Ce n'est pas un dénouement.");
        }

        Assert.True(armistices > 0, "Aucun déroulé ne se termine par un armistice.");
    }

    /// <summary>
    /// The army dissolves because nobody pays it, and the men who leave are not casualties. That
    /// distinction is the difference between a demonstration and a lie on the losses column.
    /// </summary>
    [Fact]
    public void TheBrokenArmy_GoesHome_ItIsNotKilled()
    {
        (_, PlayedGame game) = ARunThatEndsInAnArmistice();

        SideSnapshot atRupture = BrokenSideOf(game, game.Turns[game.Decision!.Turn - 1]);
        SideSnapshot atEnd = BrokenSideOf(game, game.Turns[^1]);

        double menWhoLeft = atRupture.MenInTheatre - atEnd.MenInTheatre;
        double lossesDuringAftermath = atEnd.MenLost - atRupture.MenLost;

        Assert.True(menWhoLeft > 0d, "Personne n'a quitté la ligne : l'armée ne se dissout pas.");
        Assert.True(
            lossesDuringAftermath < menWhoLeft * 0.5d,
            $"{menWhoLeft:N0} hommes ont quitté la ligne dont {lossesDuringAftermath:N0} comptés "
                + "en pertes : la dissolution est maquillée en saignée.");
    }

    /// <summary>
    /// Ground changes hands during the aftermath, and it does so because nobody is left to hold
    /// it — not because the winner attacked. If the winner had to assault, the front resolution
    /// would bill it the attacker's price, and it does not.
    /// </summary>
    [Fact]
    public void TheFront_Unwinds_WithoutTheWinnerHavingToAssault()
    {
        (_, PlayedGame game) = ARunThatEndsInAnArmistice();

        TurnSnapshot rupture = game.Turns[game.Decision!.Turn - 1];
        TurnSnapshot end = game.Turns[^1];

        Assert.NotEqual(rupture.SquareKilometresGained, end.SquareKilometresGained);

        // The line moves towards the winner, and the winner pays less for it than the side that
        // is dissolving. An advance that costs the attacker more is an assault, not an unwinding.
        double winnerLosses = WinnerSideOf(game, end).MenLost - WinnerSideOf(game, rupture).MenLost;
        double brokenLosses = BrokenSideOf(game, end).MenLost - BrokenSideOf(game, rupture).MenLost;

        Assert.True(
            winnerLosses < brokenLosses,
            $"Le vainqueur perd {winnerLosses:N0} hommes contre {brokenLosses:N0} au camp brisé : "
                + "le dénouement est payé comme une offensive.");
    }

    /// <summary>A run that nobody broke has no aftermath: it goes the distance and stops there.</summary>
    [Fact]
    public void ARunNobodyBreaks_GoesTheWholeCalendar_AndNotAQuarterMore()
    {
        foreach ((Scenario scenario, PlayedGame game) in AllRuns())
        {
            if (game.Outcome?.Code != "frozen_front")
            {
                continue;
            }

            Assert.Null(game.Decision);
            Assert.Equal(scenario.TurnCount, game.Turns.Count);
        }
    }

    /// <summary>
    /// The timeline can never fill with empty quarters, whatever the scenario does. This is the
    /// guarantee that lets the calendar change length without anyone rechecking this file.
    /// </summary>
    [Fact]
    public void NoRun_EverOutlivesItsHardBound()
    {
        foreach ((Scenario scenario, PlayedGame game) in AllRuns())
        {
            Assert.InRange(game.Turns.Count, 1, scenario.TurnCount + scenario.Aftermath.MaxTurns);
        }
    }

    /// <summary>
    /// The length of the ending belongs to the scenario, not to the engine. Whoever writes the
    /// calendar has to be able to move it without asking for code — and to read, before writing a
    /// single turn, how many quarters to leave after the rupture.
    /// </summary>
    [Fact]
    public void TheLengthOfTheEnding_BelongsToTheScenario_NotToTheEngine()
    {
        (Scenario reference, _) = ARunThatEndsInAnArmistice();

        Scenario slow = UkraineScenario.Build(VariantOf(reference));
        slow.Aftermath = new AftermathRules { DissolutionPerTurn = 0.3d, ArmisticeManningRatio = 0.06d };

        Scenario fast = UkraineScenario.Build(VariantOf(reference));
        fast.Aftermath = new AftermathRules { DissolutionPerTurn = 0.8d, ArmisticeManningRatio = 0.06d };

        // The parameters say how long the ending lasts before anything is played.
        Assert.True(slow.Aftermath.QuartersToArmistice > fast.Aftermath.QuartersToArmistice);

        PlayedGame slowGame = new GameRunner().Run(slow);
        PlayedGame fastGame = new GameRunner().Run(fast);

        Assert.Equal("armistice", slowGame.Outcome!.Code);
        Assert.Equal("armistice", fastGame.Outcome!.Code);
        Assert.True(
            slowGame.Turns.Count > fastGame.Turns.Count,
            $"Dissolution lente : {slowGame.Turns.Count} tours, rapide : {fastGame.Turns.Count}. "
                + "La durée du dénouement ne dépend pas du scénario.");
    }

    /// <summary>
    /// The announced number has to be the one the engine actually produces, or the calendar will
    /// be written against a figure nobody honoured.
    /// </summary>
    [Fact]
    public void QuartersToArmistice_IsWhatTheRunActuallyTakes()
    {
        (Scenario scenario, PlayedGame game) = ARunThatEndsInAnArmistice();

        int played = game.Turns[^1].Turn - game.Decision!.Turn;

        // The armistice is declared on a quarter that is not played, so the run shows one less.
        // An army already under its establishment when it breaks gets there sooner, never later.
        Assert.InRange(played, 1, scenario.Aftermath.QuartersToArmistice - 1);
    }

    /// <summary>The aftermath is part of the model, so it is as deterministic as the rest.</summary>
    [Fact]
    public void TheAftermath_IsDeterministic_LikeEverythingElse()
    {
        GameRunner runner = new();
        (Scenario reference, _) = ARunThatEndsInAnArmistice();

        PlayedGame first = runner.Run(UkraineScenario.Build(VariantOf(reference)));
        PlayedGame second = runner.Run(UkraineScenario.Build(VariantOf(reference)));

        Assert.Equal(first.Turns.Count, second.Turns.Count);
        Assert.Equal(first.Outcome!.Code, second.Outcome!.Code);
        Assert.Equal(first.Outcome.Turn, second.Outcome.Turn);

        for (int index = 0; index < first.Turns.Count; index++)
        {
            Assert.Equal(first.Turns[index].Invader.MenInTheatre, second.Turns[index].Invader.MenInTheatre, 6);
            Assert.Equal(first.Turns[index].SquareKilometresGained, second.Turns[index].SquareKilometresGained, 6);
        }
    }

    /// <summary>
    /// The ending has to be readable on the page without the page knowing anything about the
    /// mechanism: a title that names the side that walks away, and words on the quarters it took.
    /// </summary>
    [Fact]
    public void TheEnding_IsNamed_NotJustTheAbsenceOfMoreTurns()
    {
        (_, PlayedGame game) = ARunThatEndsInAnArmistice();

        Assert.Contains("Armistice", game.Outcome!.Title, StringComparison.Ordinal);
        Assert.Contains(BrokenSideOf(game, game.Turns[^1]).Name, game.Outcome.Title, StringComparison.Ordinal);
        Assert.False(string.IsNullOrWhiteSpace(game.Outcome.Explanation));

        Assert.Contains(
            game.Turns[^1].Narrative,
            line => line.Contains("ARMISTICE", StringComparison.Ordinal));

        Assert.Contains(
            game.Turns.SelectMany(turn => turn.Narrative),
            line => line.Contains("plus personne ne paie", StringComparison.OrdinalIgnoreCase));
    }

    private static SideSnapshot BrokenSideOf(PlayedGame game, TurnSnapshot turn)
    {
        return game.Outcome!.WinnerSideCode == Side.Invader.Code ? turn.Defender : turn.Invader;
    }

    private static SideSnapshot WinnerSideOf(PlayedGame game, TurnSnapshot turn)
    {
        return game.Outcome!.WinnerSideCode == Side.Invader.Code ? turn.Invader : turn.Defender;
    }

    private static SupportVariant VariantOf(Scenario scenario)
    {
        foreach (SupportVariant variant in AllVariants)
        {
            if (UkraineScenario.Build(variant).Code == scenario.Code)
            {
                return variant;
            }
        }

        return SupportVariant.Resolve;
    }
}
