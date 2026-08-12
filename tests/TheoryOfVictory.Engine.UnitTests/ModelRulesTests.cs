using TheoryOfVictory.Core;
using TheoryOfVictory.Engine;
using TheoryOfVictory.Engine.Scenarios;
using Xunit;

namespace TheoryOfVictory.Engine.UnitTests;

/// <summary>Locks the rules the whole game is supposed to demonstrate.</summary>
public sealed class ModelRulesTests
{
    [Fact]
    public void Run_IsDeterministic_SameScenarioProducesIdenticalOutput()
    {
        GameRunner runner = new();

        PlayedGame first = runner.Run(UkraineScenario.Build(SupportVariant.Holds));
        PlayedGame second = runner.Run(UkraineScenario.Build(SupportVariant.Holds));

        Assert.Equal(first.Turns.Count, second.Turns.Count);
        for (int index = 0; index < first.Turns.Count; index++)
        {
            Assert.Equal(first.Turns[index].Invader.CombatPower, second.Turns[index].Invader.CombatPower, 6);
            Assert.Equal(first.Turns[index].Defender.CombatPower, second.Turns[index].Defender.CombatPower, 6);
            Assert.Equal(first.Turns[index].SquareKilometresGained, second.Turns[index].SquareKilometresGained, 6);
        }
    }

    [Fact]
    public void PlayingTheCards_BreaksTheInvader_WithoutTakingGround()
    {
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Resolve));

        Assert.NotNull(game.Outcome);
        Assert.Equal(Side.Defender.Code, game.Outcome!.WinnerSideCode);

        // The invader breaks at the rear, not at the front: its power collapses while
        // the defender's holds. Cutting the money is what did it.
        TurnSnapshot last = game.Turns[^1];
        Assert.True(last.Invader.CombatPower < game.Turns[4].Invader.CombatPower);
        Assert.True(last.Defender.ForceGenerationRatio >= 0.9d);
    }

    [Fact]
    public void SupportHolding_FreezesTheFront_NobodyWins()
    {
        Scenario scenario = UkraineScenario.Build(SupportVariant.Holds);
        PlayedGame game = new GameRunner().Run(scenario);

        Assert.NotNull(game.Outcome);
        Assert.Equal("frozen_front", game.Outcome!.Code);

        // Nobody wins means nobody breaks: the run has to go the distance.
        Assert.Equal(scenario.TurnCount, game.Turns.Count);
    }

    [Fact]
    public void SupportCollapsing_BreaksTheDefender_AfterALatency()
    {
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Collapses));

        Assert.NotNull(game.Outcome);
        Assert.Equal("military_collapse", game.Outcome!.Code);
        Assert.Equal(Side.Invader.Code, game.Outcome.WinnerSideCode);

        // The turn aid stops, nothing happens yet: stocks still cover the need.
        TurnSnapshot cutTurn = game.Turns[5];
        Assert.True(cutTurn.Defender.ForceGenerationRatio >= 0.95d);

        // Two turns later the flow has dried up and power has collapsed.
        TurnSnapshot afterTurn = game.Turns[7];
        Assert.True(afterTurn.Defender.CombatPower < cutTurn.Defender.CombatPower * 0.6d);
    }

    [Fact]
    public void CuttingSupport_IsTheOnlyDifferenceBetweenTheTwoRuns()
    {
        PlayedGame holds = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Holds));
        PlayedGame collapses = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Collapses));

        // Before the cut at turn 6, both runs are strictly identical.
        for (int index = 0; index < 5; index++)
        {
            Assert.Equal(
                holds.Turns[index].Defender.CombatPower,
                collapses.Turns[index].Defender.CombatPower,
                6);
        }
    }

    [Fact]
    public void CombatPower_IsTheScarcestFlow_NeverTheSum()
    {
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Holds));

        foreach (TurnSnapshot turn in game.Turns)
        {
            foreach (SideSnapshot side in new[] { turn.Invader, turn.Defender })
            {
                double scarcest = double.MaxValue;
                foreach (KeyValuePair<string, double> entry in side.Coverage)
                {
                    scarcest = Math.Min(scarcest, entry.Value);
                }

                Assert.Equal(scarcest, side.Coverage[side.BottleneckCode!], 6);
            }
        }
    }

    [Fact]
    public void LoadShedding_IsAThreshold_DamageUnderTheMarginCostsNothing()
    {
        EnergyGrid grid = new()
        {
            NominalCapacityGw = 40d,
            BaseDemandGw = 20d,
            WinterDemandMultiplier = 1.5d,
        };

        // Ten GW of damage still leaves 30 GW against a 30 GW winter demand: no shedding.
        grid.PermanentDamageGw = 10d;
        Assert.Equal(0d, grid.ShortfallRatio(Season.Winter));

        // The next five GW bite immediately.
        grid.PermanentDamageGw = 15d;
        Assert.True(grid.ShortfallRatio(Season.Winter) > 0d);
    }

    [Fact]
    public void Mobilisation_CostsMoreGdpEachWave()
    {
        Manpower manpower = new()
        {
            BaseGdpCostPerThousand = 0.03d,
            MarginalCostExponent = 1.4d,
            TotalMobilisedEver = 0d,
        };

        double firstWave = manpower.MarginalGdpCost(300d);
        manpower.TotalMobilisedEver = 1200d;
        double fourthWave = manpower.MarginalGdpCost(300d);

        Assert.True(fourthWave > firstWave * 1.5d);
    }

    [Fact]
    public void Sanctions_Erode_UnlessTightened()
    {
        SanctionsRegime sanctions = new();
        sanctions.Tighten(0.8d, 0.8d, 0.8d);

        double initialPrice = sanctions.PriceSeverity;
        for (int turn = 0; turn < 6; turn++)
        {
            sanctions.AdvanceTurn();
        }

        Assert.True(sanctions.PriceSeverity < initialPrice * 0.6d);
    }

    [Fact]
    public void Innovation_MovesTheBottleneck_ItDoesNotAddPower()
    {
        Innovation innovation = new();
        double before = innovation.WeaponDemandMultiplier;

        innovation.TacticalDroneEdge = 1d;
        double after = innovation.WeaponDemandMultiplier;

        // Drones cut the shells needed for the same effect; they never inflate combat power.
        Assert.True(after < before);
    }

    [Fact]
    public void SovereignFund_IsReallySpent_SoTheBarrelDecides()
    {
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Resolve));

        // A fund counted as fundable but never liquidated makes the oil price decorative.
        // It has to be visibly emptied by the war it is paying for.
        double first = game.Turns[0].Invader.Reserves;
        double last = game.Turns[^1].Invader.Reserves;
        Assert.True(last < first * 0.5d, $"Réserves {first:F0} → {last:F0} : le fonds n'est pas ponctionné.");

        // And once it can no longer plug the hole, the war effort is visibly underfunded.
        Assert.True(game.Turns[^1].Invader.FundingGap > 0.25d);
    }

    [Fact]
    public void TheAsphyxiationIsVisible_InvaderPowerFalls_WithoutLosingGround()
    {
        PlayedGame resolve = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Resolve));

        double peak = 0d;
        foreach (TurnSnapshot turn in resolve.Turns)
        {
            peak = Math.Max(peak, turn.Invader.CombatPower);
        }

        // The player must be able to watch the invader weaken on the one number they read,
        // turn after turn. A rear that breaks while combat power keeps climbing teaches
        // the opposite of the thesis.
        Assert.True(
            resolve.Turns[^1].Invader.CombatPower < peak * 0.7d,
            $"Pic {peak:F0}, fin {resolve.Turns[^1].Invader.CombatPower:F0} : l'asphyxie ne se voit pas.");

        // And it is not done by taking ground: the defender never runs a real offensive.
        Assert.True(resolve.Turns[^1].SquareKilometresGained > 0d);
    }

    [Fact]
    public void ThreatIsSignalledBeforeItStrikes_NotOnTheTurnItLands()
    {
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Collapses));

        int collapseTurn = game.Turns.Count;
        int firstCritical = int.MaxValue;

        foreach (TurnSnapshot turn in game.Turns)
        {
            foreach (PressureAlert alert in turn.Alerts)
            {
                if (alert.SideCode == Side.Defender.Code && alert.Level == AlertLevel.Critical)
                {
                    firstCritical = Math.Min(firstCritical, turn.Turn);
                }
            }
        }

        // The whole point of a threshold model is that the player can see it coming.
        Assert.True(firstCritical < collapseTurn - 1, $"Première alerte critique T{firstCritical}, chute T{collapseTurn}.");
    }

    [Fact]
    public void CuttingSupport_DoesNothingForTwoTurns_ThenEverythingAtOnce()
    {
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Collapses));

        // Aid stops at turn 6. The depot still covers the need that turn and the next:
        // this latency is the demonstration, not an artefact of it.
        Assert.Equal(1d, game.Turns[5].Defender.Coverage["weapons"], 2);
        Assert.Equal(1d, game.Turns[6].Defender.Coverage["weapons"], 2);

        // The turn after, the depot is empty and coverage falls off a cliff.
        Assert.True(game.Turns[7].Defender.Coverage["weapons"] < 0.6d);
    }

    [Fact]
    public void AnArmyThatCannotBePaid_Shrinks_WithoutAnyAssault()
    {
        Belligerent side = UkraineScenario.Build(SupportVariant.Holds).Invader;
        Manpower manpower = side.Manpower;

        manpower.TargetForceSize = 500d;
        manpower.PayableForceSize = double.PositiveInfinity;
        Assert.Equal(500d, manpower.EffectiveForceSize);

        // Payroll is most of a war budget: it is how a collapsing revenue reaches the line.
        manpower.PayableForceSize = 300d;
        Assert.Equal(300d, manpower.EffectiveForceSize);
    }

    [Fact]
    public void DeepStrikeDeck_BeatsFrontalAttrition_OnTheSameWarAndTheSameBudget()
    {
        List<DuelResult> duels = DeckDuel.Compare();

        DuelResult deepStrike = duels.Single(duel => duel.Archetype == DeckArchetype.DeepStrike);
        DuelResult attrition = duels.Single(duel => duel.Archetype == DeckArchetype.FrontalAttrition);

        // The design document names this as the balance criterion: if the grinding deck
        // wins, the game says the opposite of its own thesis.
        Assert.True(deepStrike.DefenderWins, "Le deck frappe profonde ne gagne pas.");
        Assert.False(attrition.DefenderWins, "Le deck attrition frontale gagne : la thèse est inversée.");

        // And the losing deck is the one that took the most ground — you can lose the war
        // while gaining hexes every single turn.
        Assert.True(
            attrition.GroundTaken > deepStrike.GroundTaken * 3d,
            $"Attrition {attrition.GroundTaken:F0} km², frappe profonde {deepStrike.GroundTaken:F0} km².");
    }

    [Fact]
    public void NoDeckIsDominant_TheyAreComparedAtEqualPoliticalCost()
    {
        List<DuelResult> duels = DeckDuel.Compare();

        foreach (DuelResult duel in duels)
        {
            Assert.Equal(DeckDuel.CapitalBudget, duel.PoliticalCost, 0);
        }
    }

    [Fact]
    public void ACounterCard_StopsItsTarget_WhichIsPlayedAndDoesNothing()
    {
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Resolve));

        PlayedCard? countered = game.Turns
            .SelectMany(turn => turn.CardsPlayed)
            .FirstOrDefault(card => card.Countered);

        // The counter type existed in the model with no card ever using it: the bluff the
        // design rests on could not happen. It has to be reachable in a real run.
        Assert.NotNull(countered);
    }

    [Fact]
    public void SaturationPrecedesPenetration_CheapDronesEmptyTheHeavyMagazines()
    {
        Scenario scenario = UkraineScenario.Build(SupportVariant.Holds);
        Belligerent attacker = scenario.Invader;
        Belligerent defender = scenario.Defender;

        StrikeResolution small = StrikeResolver.Resolve(StrikeTarget.PowerGrid, 50d, 200d, attacker, defender);
        StrikeResolution massed = StrikeResolver.Resolve(StrikeTarget.PowerGrid, 6000d, 200d, attacker, defender);

        // Same missile count, far more of them get through once the defence is saturated.
        Assert.True(massed.MissilesLeaked > small.MissilesLeaked);
        Assert.True(massed.Saturated);
    }
}
