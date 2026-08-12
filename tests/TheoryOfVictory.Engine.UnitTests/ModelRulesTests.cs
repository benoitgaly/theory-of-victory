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
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Holds));

        Assert.NotNull(game.Outcome);
        Assert.Equal("frozen_front", game.Outcome!.Code);
        Assert.Equal(16, game.Turns.Count);
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
