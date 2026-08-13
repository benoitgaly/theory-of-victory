using TheoryOfVictory.Core;
using TheoryOfVictory.Engine;
using TheoryOfVictory.Engine.Scenarios;
using Xunit;

namespace TheoryOfVictory.Engine.UnitTests;

/// <summary>
/// The front moves where nobody is standing, and only there. These lock the one claim that makes
/// the map defensible in public: the rush of 2022 and the ebb of that autumn are not scripted
/// dates, they are the same density rule read twice — once against a defender who put everything
/// in the Donbass, once against an invader holding twelve hundred kilometres with nothing.
/// </summary>
public sealed class FrontMovementTests
{
    [Fact]
    public void ThePrologue_ResolvesNoFrontAndNoStrike_NotAShotIsFired()
    {
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Holds));
        TurnSnapshot prologue = game.Turns[0];

        Assert.Equal(2021, prologue.Year);
        Assert.Empty(prologue.Sectors);
        Assert.Null(prologue.InvaderStrike);
        Assert.Null(prologue.DefenderStrike);
        Assert.Equal(0d, prologue.SquareKilometresGained);

        // Force generation, on the other hand, runs in full: that is the whole point of the turn.
        Assert.True(prologue.Invader.CombatPower > 0d);
        Assert.True(game.Turns[1].Invader.MenInContact > prologue.Invader.MenInContact);
    }

    [Fact]
    public void TheRushOfTwentyTwo_HappensWhereTheLineIsEmpty_NotWhereItIsFortified()
    {
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Holds));
        TurnSnapshot invasion = game.Turns[1];

        Assert.Equal(2022, invasion.Year);
        Assert.Equal(Season.Winter, invasion.Season);

        // Ground is taken, and a lot of it by this model's standards.
        Assert.True(
            invasion.SquareKilometresGained > 3000d,
            $"{invasion.SquareKilometresGained:F0} km² au trimestre de l'invasion : la ruée n'a pas lieu.");

        // And it is taken in the south, which Ukraine held with one brigade — never in the
        // Donbass, fortified since 2014 and holding nearly everything it had.
        double south = Moved(invasion, "kherson") + Moved(invasion, "zaporizhzhia");
        double donbass = Moved(invasion, "bakhmut") + Moved(invasion, "pokrovsk") + Moved(invasion, "lyman");

        Assert.True(south > 5d, $"Sud : {south:F1} hexagones, la percée ne s'ouvre pas.");
        Assert.True(
            donbass < 1d,
            $"Donbass : {donbass:F1} hexagones — le front fortifié aurait cédé comme le front vide.");
    }

    [Fact]
    public void TheEbbOfAutumn_IsTheSameRuleReadBackwards_AnInvaderHoldingNothing()
    {
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Holds));
        TurnSnapshot autumn = game.Turns[4];

        Assert.Equal(2022, autumn.Year);
        Assert.Equal(Season.Autumn, autumn.Season);

        // The line comes back, and it comes back at Kharkiv — the sector Russia covered with
        // second-rate units while everything went to Bakhmut.
        Assert.True(
            autumn.SquareKilometresGained < game.Turns[3].SquareKilometresGained - 2000d,
            "Le reflux de l'automne 2022 n'a pas lieu.");

        Assert.True(
            Moved(autumn, "kharkiv") < -2d,
            $"Kharkiv : {Moved(autumn, "kharkiv"):F1} hexagones, la contre-offensive ne mord pas.");
    }

    [Fact]
    public void TheGrindingWar_StaysFrozen_TheCeilingOnlyEverOpens()
    {
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Holds));

        // From the moment the line has formed to the last turn, the front is a thermometer
        // again: four years of war for a few hundred square kilometres. If the density rule
        // ever leaked into this stretch, the whole model would start teaching the opposite.
        double settled = game.Turns[7].SquareKilometresGained;
        double end = game.Turns[^1].SquareKilometresGained;

        Assert.True(
            Math.Abs(end - settled) < 600d,
            $"{settled:F0} km² au T8 contre {end:F0} au dernier tour : l'usure s'est mise à bouger.");
    }

    [Fact]
    public void TheAsphyxiationRun_BreaksThePowerFirst_AndTheLineOnlyAfterwards()
    {
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Resolve));

        double peak = game.Turns.Max(turn => turn.Invader.CombatPower);
        int peakTurn = game.Turns.First(turn => turn.Invader.CombatPower >= peak).Turn;
        int fall = game.Outcome!.Turn;

        // The window that carries the thesis: the invader loses two thirds of its strength and
        // the line does not move an inch. Ground is a consequence here, never a cause.
        double atPeak = game.Turns[peakTurn - 1].SquareKilometresGained;
        double onTheEve = game.Turns[fall - 2].SquareKilometresGained;

        Assert.True(
            game.Turns[fall - 2].Invader.CombatPower < peak * 0.5d,
            "L'étranglement ne se voit pas sur la puissance.");

        Assert.True(
            Math.Abs(onTheEve - atPeak) < 400d,
            $"{atPeak:F0} km² au pic contre {onTheEve:F0} à la veille de la chute : "
                + "le front aurait décidé de la guerre à la place de la caisse.");
    }

    private static double Moved(TurnSnapshot turn, string sectorCode)
    {
        SectorResolution? sector = turn.Sectors.FirstOrDefault(s => s.SectorCode == sectorCode);
        return sector?.HexesCumulative ?? 0d;
    }
}
