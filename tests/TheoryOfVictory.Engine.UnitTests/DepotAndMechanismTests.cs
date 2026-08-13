using TheoryOfVictory.Core;
using TheoryOfVictory.Engine;
using TheoryOfVictory.Engine.Scenarios;
using Xunit;

namespace TheoryOfVictory.Engine.UnitTests;

/// <summary>
/// The realism audit found the four central mechanisms of the game inert: the minimum rule
/// never bit, no wave was ever saturated, load shedding never happened, and the 2023-2024
/// ammunition crisis never appeared. All four had one cause — depots that aid in kind
/// refilled without ever passing a ceiling.
///
/// These tests lock the fix and, more importantly, lock the mechanisms themselves: a rule
/// that never fires demonstrates nothing, and the failure mode is silent.
/// </summary>
public sealed class DepotAndMechanismTests
{
    /// <summary>
    /// One definition of a full depot, whatever fills it. The old engine had two paths and
    /// one ceiling: purchases obeyed it, aid in kind did not, and the interceptor pile ended
    /// the run at twenty-three times the ceiling the game had set itself.
    /// </summary>
    [Fact]
    public void EveryPathThatFillsADepot_ObeysTheSameCeiling()
    {
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Holds));
        Scenario scenario = UkraineScenario.Build(SupportVariant.Holds);

        // The ceiling grows with consumption, so it is not a constant to compare against.
        // What can be checked is the order of magnitude the audit called a wall: half a
        // million interceptors held by an army producing a few thousand a quarter.
        double ceilingOrderOfMagnitude =
            scenario.Defender.Industry.GetCapacityPerTurn(ResourceKind.CheapInterceptors)
            * scenario.Defender.Industry.DepotQuartersHeld
            * 4d;

        foreach (TurnSnapshot turn in game.Turns)
        {
            double held = turn.Defender.Stocks[ResourceKind.CheapInterceptors.Code];
            Assert.True(
                held < ceilingOrderOfMagnitude,
                $"T{turn.Turn} : {held:N0} intercepteurs en dépôt, mur reconstitué.");
        }
    }

    /// <summary>
    /// A depot is sized on what it fires, not only on what the factory makes. Interceptors
    /// are burnt in the deep-strike phase and never appear in a front requirement: a ceiling
    /// reading the front alone would size the magazines on the factory and leave the sky open.
    /// </summary>
    [Fact]
    public void TheDepotCeiling_IsSizedOnWhatIsBurnt_NotOnlyOnWhatIsMade()
    {
        Belligerent belligerent = UkraineScenario.Build(SupportVariant.Holds).Defender;
        belligerent.Industry.DepotQuartersHeld = 2d;
        belligerent.Industry.SetCapacityPerTurn(ResourceKind.Missiles, 10d);

        // A small consumption keeps us out of the opening-turn fallback, where nothing has
        // been consumed yet and the ceiling defers to the depot the scenario handed over.
        belligerent.NeedThisTurn[ResourceKind.Missiles.Code] = 5d;
        belligerent.BurntThisTurn[ResourceKind.Missiles.Code] = 0d;
        Assert.Equal(20d, belligerent.DepotCeiling(ResourceKind.Missiles));

        // Fired far above what the line makes: the magazine follows the firing, not the line.
        belligerent.BurntThisTurn[ResourceKind.Missiles.Code] = 300d;
        Assert.Equal(600d, belligerent.DepotCeiling(ResourceKind.Missiles));
    }

    /// <summary>
    /// What a depot cannot take is not destroyed. A donor facing a receiver who cannot absorb
    /// more materiel sends money instead — and the aid keeps its value, which is what stops
    /// the ceiling from quietly becoming a cut in support.
    /// </summary>
    [Fact]
    public void AidTheDepotsRefuse_IsNotDestroyed()
    {
        Belligerent belligerent = UkraineScenario.Build(SupportVariant.Holds).Defender;
        belligerent.Industry.DepotQuartersHeld = 1d;
        belligerent.Industry.SetCapacityPerTurn(ResourceKind.Weapons, 100d);
        belligerent.NeedThisTurn[ResourceKind.Weapons.Code] = 100d;

        double before = belligerent.Stock.GetActual(ResourceKind.Weapons);
        double refused = belligerent.FillDepot(ResourceKind.Weapons, 1000d);
        double accepted = belligerent.Stock.GetActual(ResourceKind.Weapons) - before;

        Assert.Equal(1000d, accepted + refused, 6);
        Assert.True(refused > 0d, "Le plafond n'a rien refusé : il ne mord pas.");
    }

    /// <summary>
    /// THE criterion of the whole correction. The minimum rule is the game's single combat
    /// rule and its centrepiece illustration; the audit found it biting in zero of the
    /// thirty-eight camp-turn readings of the reference run. A barrel that is always full
    /// teaches nothing, and the red BOTTLENECK label above it is then a lie.
    /// </summary>
    [Fact]
    public void TheMinimumRule_ActuallyBites_InTheReferenceRun()
    {
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Holds));

        int pinched = 0;
        double worst = 1d;

        foreach (TurnSnapshot turn in game.Turns)
        {
            foreach (SideSnapshot side in new[] { turn.Invader, turn.Defender })
            {
                worst = Math.Min(worst, side.MaterialCoverage);
                if (side.MaterialCoverage < 0.97d)
                {
                    pinched++;
                }
            }
        }

        Assert.True(pinched > 0, "La règle du minimum ne mord dans aucun tour : le tonneau est décoratif.");
        Assert.True(worst < 0.9d, $"Couverture la plus basse {worst:P0} : aucune vraie pénurie.");
    }

    /// <summary>
    /// Saturation precedes penetration — the design's own words. The audit found not a single
    /// saturated wave in any of the three runs, and interception pinned at 100 % for seventeen
    /// consecutive turns, against 80 to 97 % observed.
    /// </summary>
    [Fact]
    public void AtLeastOneWave_Saturates_AndInterceptionIsNeverPerfect()
    {
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Holds));

        bool saturated = false;
        double worstInterception = 1d;

        foreach (TurnSnapshot turn in game.Turns)
        {
            foreach (StrikeResolution? strike in new[] { turn.InvaderStrike, turn.DefenderStrike })
            {
                if (strike is null)
                {
                    continue;
                }

                saturated |= strike.Saturated;
                worstInterception = Math.Min(worstInterception, strike.InterceptionRate);
            }
        }

        Assert.True(saturated, "Aucune vague saturée : le mécanisme le plus mis en avant du jeu est inerte.");
        Assert.True(worstInterception < 0.99d, "Interception parfaite à tous les tours.");

        // And the correction must not overshoot the other way: a defence that stops almost
        // nothing would be as false as one that stops everything.
        Assert.True(
            worstInterception > 0.4d,
            $"Interception tombée à {worstInterception:P0}, très loin des 80 à 97 % observés.");
    }

    /// <summary>
    /// Load shedding is a threshold and the season decides. The audit found the shortfall at
    /// zero in every turn of every run: a grid dimensioned to absorb everything the strike
    /// campaigns could inflict, so the winter campaigns the design describes as its annual
    /// rhythm never happened.
    ///
    /// Reads the winter shortfall, which is what the snapshot exposes for both sides whatever
    /// the actual season — so this asserts that a winter WOULD shed, not that a given quarter did.
    /// </summary>
    [Fact]
    public void TheUkrainianGrid_CanActuallyBeBroughtUnderItsThreshold()
    {
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Holds));

        bool shed = false;
        foreach (TurnSnapshot turn in game.Turns)
        {
            shed |= turn.Defender.GridShortfall > 0d;
        }

        Assert.True(shed, "Le réseau ukrainien n'est jamais passé sous son seuil : le délestage est décoratif.");
    }

    /// <summary>
    /// Two thirds of the Russian military spend existed, was debited, and appeared nowhere on
    /// the economic view: the payroll line was written and then erased by the clear that
    /// prepared the discretionary split.
    /// </summary>
    [Fact]
    public void ThePayroll_AppearsInTheAllocation_ItIsTwoThirdsOfTheSpend()
    {
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Holds));

        foreach (TurnSnapshot turn in game.Turns)
        {
            Assert.True(
                turn.Invader.Allocation.ContainsKey("payroll"),
                $"T{turn.Turn} : la solde a disparu du tableau d'allocation.");
            Assert.True(turn.Invader.Allocation["payroll"] > 0d);
        }
    }

    /// <summary>
    /// A quarterly spend over an annual GDP reads four times too low. Harmless while nothing
    /// displays it, fatal the day something does — the war effort would be announced at 2 %
    /// of Russian GDP.
    /// </summary>
    [Fact]
    public void TheWarEffortShare_IsAnnualised_BeforeItIsEverDisplayed()
    {
        Economy economy = new()
        {
            HeadlineGdpBillions = 1000d,
            LastTurnMilitarySpendBillions = 40d,
            WarBudgetCeilingShare = 0.038d,
        };

        // Four quarters at 40 over a GDP of 1 000 is 16 %, not 4 %.
        Assert.Equal(0.16d, economy.WarEffortShare, 6);
        Assert.Equal(0.152d, economy.AnnualWarEffortShareOfGdp, 6);
    }
}
