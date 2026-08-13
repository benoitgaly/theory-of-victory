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
        //
        // Read on the turn the regime falls, not on the last turn of the run. What follows
        // the collapse is the aftermath, where the beaten army dissolves and both sides'
        // indicators describe a war that has already been decided — a fine thing to model,
        // and a meaningless place to measure who was holding.
        TurnSnapshot fall = game.Turns[game.Outcome.Turn - 1];
        Assert.True(fall.Invader.CombatPower < game.Turns[4].Invader.CombatPower);
        Assert.True(
            fall.Defender.ForceGenerationRatio >= 0.9d,
            $"Régénération ukrainienne à {fall.Defender.ForceGenerationRatio:F2} au tour de la chute russe.");
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
        Assert.Equal(Side.Invader.Code, game.Outcome!.WinnerSideCode);

        // The run now ends on an armistice, but what has to be checked is what CAUSED it:
        // the front broke, and it broke because the flow dried up. The first outcome the run
        // ever recorded is the collapse itself; the armistice is only its epilogue.
        string firstRecorded = game.Turns.First(turn => turn.Outcome is not null).Outcome!.Code;
        Assert.Equal("military_collapse", firstRecorded);

        // The turn aid stops, nothing happens yet: stocks still cover the need.
        TurnSnapshot cutTurn = game.Turns[6];
        Assert.True(cutTurn.Defender.ForceGenerationRatio >= 0.95d);

        // Two turns later the flow has dried up and power has collapsed.
        TurnSnapshot afterTurn = game.Turns[8];
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

    /// <summary>
    /// The correction this whole calibration is about: men are not a consumed flow, so they
    /// cannot be short of anything. Only the three burnt flows may ever be the bottleneck.
    /// </summary>
    [Fact]
    public void Men_AreNeverACoverage_OnlyABurntFlowCanBeShort()
    {
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Holds));

        HashSet<string> burnt = [.. ResourceKind.FrontFlows.Select(kind => kind.Code)];

        foreach (TurnSnapshot turn in game.Turns)
        {
            foreach (SideSnapshot side in new[] { turn.Invader, turn.Defender })
            {
                Assert.DoesNotContain(side.Coverage.Keys, code => !burnt.Contains(code));
                Assert.Contains(side.BottleneckCode!, burnt);
            }
        }
    }

    /// <summary>
    /// Three counts, and the public debate confuses them constantly. They must stay ordered:
    /// nobody can be in contact without being in the theatre, nor in the theatre without
    /// being under arms.
    /// </summary>
    [Fact]
    public void TheThreeCounts_StayOrdered_UnderArmsThenTheatreThenContact()
    {
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Holds));

        foreach (TurnSnapshot turn in game.Turns)
        {
            foreach (SideSnapshot side in new[] { turn.Invader, turn.Defender })
            {
                Assert.True(
                    side.MenUnderArms > side.MenInTheatre,
                    $"T{turn.Turn} {side.Name} : {side.MenUnderArms:N0} sous les drapeaux "
                        + $"pour {side.MenInTheatre:N0} au théâtre.");
                Assert.True(
                    side.MenInTheatre > side.MenInContact,
                    $"T{turn.Turn} {side.Name} : {side.MenInTheatre:N0} au théâtre "
                        + $"pour {side.MenInContact:N0} en contact.");
                Assert.True(side.MenInContact > 0d);
            }
        }
    }

    /// <summary>
    /// The unit the user actually reads. An engine that counts in thousands is fine; a board
    /// that prints "560" for 560 000 soldiers is not, and the conversion belongs here.
    /// </summary>
    [Fact]
    public void Effectives_LeaveTheEngineInMen_NotInThousands()
    {
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Holds));

        foreach (TurnSnapshot turn in game.Turns)
        {
            // Both theatre groupings stayed within these bounds for the whole real war.
            Assert.InRange(turn.Invader.MenInTheatre, 150_000d, 800_000d);
            Assert.InRange(turn.Defender.MenInTheatre, 150_000d, 700_000d);
        }
    }

    /// <summary>
    /// THE INVERSION, stated as an invariant. Men are not a resource whose need is covered:
    /// they are what creates the need. A larger theatre grouping must demand more shells,
    /// and no quantity of shells ever determines how many men there are.
    /// </summary>
    [Fact]
    public void TheInversion_TheMenSetTheMaterialRequirement_NeverTheReverse()
    {
        // Raising the ESTABLISHMENT is the wrong handle, and finding that out is itself a
        // result: Ukrainian recruitment is bound by what the treasury can pay for, quarter
        // after quarter, so ordering a bigger army produces no men at all. Testing through it
        // would compare two identical runs and pass on a tautology.
        //
        // The men themselves are the handle. A hundred thousand more men in the theatre from
        // the first quarter, and the front must demand more shells for them.
        Scenario reference = UkraineScenario.Build(SupportVariant.Holds);

        Scenario larger = UkraineScenario.Build(SupportVariant.Holds);
        larger.Defender.Manpower.AtFront += 100d;

        SideSnapshot lean = new GameRunner().Run(reference).Turns[0].Defender;
        SideSnapshot fat = new GameRunner().Run(larger).Turns[0].Defender;

        Assert.True(fat.MenInTheatre > lean.MenInTheatre);
        Assert.True(
            fat.Need[ResourceKind.Weapons.Code] > lean.Need[ResourceKind.Weapons.Code],
            $"{fat.MenInTheatre:N0} hommes demandent {fat.Need[ResourceKind.Weapons.Code]:F0} "
                + $"contre {lean.Need[ResourceKind.Weapons.Code]:F0} pour {lean.MenInTheatre:N0}.");

        // And the other direction, which had never actually been tested: shells do not make
        // men. Handing an army ten times its depot changes what it can fire and nothing about
        // how many soldiers it has — men are not the output of a material flow.
        Scenario stuffed = UkraineScenario.Build(SupportVariant.Holds);
        stuffed.Defender.Stock.Add(ResourceKind.Weapons, 20_000d);

        SideSnapshot armed = new GameRunner().Run(stuffed).Turns[0].Defender;
        Assert.Equal(lean.MenInTheatre, armed.MenInTheatre);
        Assert.Equal(lean.MenInContact, armed.MenInContact);
    }

    /// <summary>
    /// What fights is the infantry on the line, not the grouping behind it. Halving the
    /// contact share halves the power at identical headcount — an army can hold a smaller
    /// front with the same million men.
    /// </summary>
    [Fact]
    public void CombatPower_RidesOnTheContactInfantry_NotOnTheHeadcount()
    {
        Scenario reference = UkraineScenario.Build(SupportVariant.Holds);

        Scenario thinner = UkraineScenario.Build(SupportVariant.Holds);
        thinner.Invader.Manpower.ContactShare = reference.Invader.Manpower.ContactShare / 2d;

        double referencePower = new GameRunner().Run(reference).Turns[0].Invader.CombatPower;
        double thinnerPower = new GameRunner().Run(thinner).Turns[0].Invader.CombatPower;

        // Both figures leave the engine rounded to the thousand, so the halving can only be
        // checked to within one rounding step — asserting exact equality here would pass or
        // fail on where the rounding happens to land, which teaches nothing.
        Assert.Equal(referencePower / 2d, thinnerPower, 1000d);
    }

    /// <summary>
    /// And the mirror of it: a bigger tail is bought, fed and paid without adding an ounce
    /// of combat power. This is the Ukrainian crisis of 2024-2026 stated as an invariant.
    /// </summary>
    [Fact]
    public void GrowingTheTail_AddsMenUnderArms_AndNoPowerAtAll()
    {
        Scenario reference = UkraineScenario.Build(SupportVariant.Holds);

        Scenario tailHeavy = UkraineScenario.Build(SupportVariant.Holds);
        tailHeavy.Defender.Manpower.RearEstablishmentRatio += 0.5d;

        SideSnapshot lean = new GameRunner().Run(reference).Turns[^1].Defender;
        SideSnapshot heavy = new GameRunner().Run(tailHeavy).Turns[^1].Defender;

        Assert.True(heavy.MenUnderArms > lean.MenUnderArms);
        Assert.Equal(lean.CombatPower, heavy.CombatPower, 6);
        Assert.Equal(lean.MenInContact, heavy.MenInContact, 6);
    }

    /// <summary>
    /// Calibration check against the one source that does not come from a belligerent:
    /// Janis Kluge's series, derived from Russian budget data on salary top-ups — 523 548
    /// in 2023, 667 114 in 2024, 723 477 in 2025, all mid-year. Fifteen per cent is the
    /// honest tolerance: these are estimates, and the turn is a quarter wide.
    /// </summary>
    [Fact]
    public void TheRussianGrouping_TracksTheBudgetDerivedSeries_WithinFifteenPercent()
    {
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Holds));

        // Indices, not turn numbers: the autumn 2021 prologue sits at index 0, so summer 2023
        // is turn 8 and index 7.
        AssertWithin(523_548d, game.Turns[7].Invader.MenInTheatre, 0.15d, "été 2023");
        AssertWithin(667_114d, game.Turns[11].Invader.MenInTheatre, 0.15d, "été 2024");
        AssertWithin(723_477d, game.Turns[15].Invader.MenInTheatre, 0.15d, "été 2025");
    }

    /// <summary>
    /// The two published Ukrainian anchors, January 2025: 880 000 under arms according to
    /// Zelensky, and no more than 300 000 of them on the line according to OSW. The gap
    /// between the two is the whole point of the three-count distinction.
    /// </summary>
    [Fact]
    public void TheUkrainianForce_MatchesItsTwoPublishedAnchors()
    {
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Holds));

        // Zelensky spoke on 15 January 2025, so the winter 2025 quarter: turn 14, index 13.
        SideSnapshot ukraine = game.Turns[13].Defender;

        AssertWithin(880_000d, ukraine.MenUnderArms, 0.15d, "sous les drapeaux, janvier 2025");
        Assert.True(
            ukraine.MenInContact <= 300_000d,
            $"{ukraine.MenInContact:N0} en contact : l'OSW plafonne à 300 000.");
    }

    /// <summary>
    /// The engine uses infinity as a sentinel and JSON cannot carry it: any infinite value
    /// reaching the snapshot travels to the page as null and silently empties a readout.
    /// </summary>
    [Fact]
    public void NoSnapshotFigure_IsInfinite_SoTheBoardCanCarryItAll()
    {
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Collapses));

        foreach (TurnSnapshot turn in game.Turns)
        {
            foreach (SideSnapshot side in new[] { turn.Invader, turn.Defender })
            {
                foreach (System.Reflection.PropertyInfo property in typeof(SideSnapshot).GetProperties())
                {
                    if (property.PropertyType != typeof(double))
                    {
                        continue;
                    }

                    double value = (double)property.GetValue(side)!;
                    Assert.True(
                        double.IsFinite(value),
                        $"T{turn.Turn} {side.Name} : {property.Name} vaut {value}.");
                }
            }
        }
    }

    /// <summary>
    /// Men have no coverage ceiling; they have three real ones. FIRST: the demographic
    /// reservoir. An army cannot recruit men who do not exist, however much it is willing to
    /// pay, and the pool it draws from can never go negative.
    /// </summary>
    [Fact]
    public void TheDemographicCeiling_Binds_NoArmyRecruitsMenItDoesNotHave()
    {
        Scenario reference = UkraineScenario.Build(SupportVariant.Holds);

        Scenario drained = UkraineScenario.Build(SupportVariant.Holds);
        drained.Defender.Manpower.MobilisablePool = 120d;

        PlayedGame referenceGame = new GameRunner().Run(reference);
        PlayedGame drainedGame = new GameRunner().Run(drained);

        SideSnapshot deep = referenceGame.Turns[^1].Defender;
        SideSnapshot empty = drainedGame.Turns[^1].Defender;

        Assert.True(
            empty.MenInTheatre < deep.MenInTheatre,
            $"Réservoir vide : {empty.MenInTheatre:N0} au théâtre contre {deep.MenInTheatre:N0}.");

        foreach (TurnSnapshot turn in drainedGame.Turns)
        {
            Assert.True(turn.Defender.MenMobilisable >= 0d);
        }
    }

    /// <summary>
    /// SECOND ceiling: what a regime dares demand. Forced mobilisation is free in cash and
    /// expensive in consent — and rushing the cycle degrades the training, which is what makes
    /// one mobilisation manufacture the need for the next.
    /// </summary>
    [Fact]
    public void ThePoliticalCeiling_IsPaidInConsent_MobilisationCostsMoraleAndQuality()
    {
        Scenario scenario = UkraineScenario.Build(SupportVariant.Holds);
        Belligerent russia = scenario.Invader;
        double qualityBefore = russia.Manpower.TrainingQuality;

        PlayedGame game = new GameRunner().Run(scenario);

        // The autumn 2022 mobilisation lands at turn 4, and the bill falls the same quarter.
        // Measured on the turn itself: discontent decays every quarter, so comparing the two
        // ends of a nineteen-turn run would hide the very jump this ceiling is made of.
        SideSnapshot before = game.Turns[2].Invader;
        SideSnapshot after = game.Turns[3].Invader;

        Assert.True(
            after.PopularDiscontent > before.PopularDiscontent,
            $"Mécontentement {before.PopularDiscontent:F1} → {after.PopularDiscontent:F1} : "
                + "mobiliser n'a rien coûté en consentement.");
        Assert.True(
            after.Morale < before.Morale,
            $"Moral {before.Morale:F1} → {after.Morale:F1} : mobiliser n'a rien coûté.");

        // Rushing the cycle degrades the training, and that damage never heals: it is how one
        // mobilisation manufactures the need for the next.
        Assert.True(
            russia.Manpower.TrainingQuality < qualityBefore,
            "Le cycle de formation pressé n'a pas dégradé la qualité.");
    }

    /// <summary>
    /// THIRD ceiling: every mobilised man leaves the productive economy, and the price rises
    /// with each wave. This is the ceiling that makes mobilising at the wrong moment actively
    /// suicidal rather than merely useless.
    /// </summary>
    [Fact]
    public void TheEconomicCeiling_TakesTheMenFromTheFactories_AtARisingPrice()
    {
        Manpower manpower = new()
        {
            BaseGdpCostPerThousand = 0.03d,
            MarginalCostExponent = 1.4d,
            TotalMobilisedEver = 0d,
        };

        double firstWave = manpower.MarginalGdpCost(300d);
        manpower.TotalMobilisedEver = 1200d;
        double laterWave = manpower.MarginalGdpCost(300d);

        Assert.True(laterWave > firstWave);

        // And it is really taken out of the economy in a run, not just computed on paper.
        Scenario scenario = UkraineScenario.Build(SupportVariant.Holds);
        Belligerent russia = scenario.Invader;
        double capacityBefore = russia.Economy.ProductiveCapacityBillions;

        new GameRunner().Run(scenario);

        Assert.True(
            russia.Economy.ProductiveCapacityBillions < capacityBefore,
            "La capacité productive n'a pas payé la mobilisation.");
        Assert.True(russia.Manpower.TotalMobilisedEver > 0d);
    }

    /// <summary>
    /// The board reads people, and it must not be told more than the sources know. Every
    /// headcount leaves the engine rounded to the thousand: 671 412 would claim a census
    /// where there is a ± 15 % estimate.
    /// </summary>
    [Fact]
    public void EveryHeadcount_LeavesTheEngineRoundedToTheThousand()
    {
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Holds));

        foreach (TurnSnapshot turn in game.Turns)
        {
            foreach (SideSnapshot side in new[] { turn.Invader, turn.Defender })
            {
                double[] counts =
                [
                    side.MenUnderArms,
                    side.MenInTheatre,
                    side.MenInContact,
                    side.MenInTraining,
                    side.MenMobilisable,
                    side.MenLost,
                    side.MenEstablishment,
                    side.CombatPower,
                ];

                foreach (double count in counts)
                {
                    Assert.Equal(0d, count % 1000d);
                }
            }
        }
    }

    /// <summary>
    /// The three runs are the demonstration itself, and they are only a demonstration if they
    /// land where the design says. Recalibrating the men must never quietly move them.
    /// </summary>
    [Fact]
    public void TheThreeRuns_LandOnTheirAppointedTurns()
    {
        PlayedGame resolve = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Resolve));
        PlayedGame holds = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Holds));
        PlayedGame collapses = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Collapses));

        // Turn 1 is the autumn 2021 build-up, the invasion is turn 2, and the present —
        // summer 2026 — is turn 20. Everything after it is the model speaking about a future
        // nobody has observed, which is exactly what the calendar is for.

        // Spring 2027: the Russian regime falls, three quarters past the present.
        Assert.Equal(Side.Defender.Code, resolve.Outcome!.WinnerSideCode);
        Assert.Equal(23, resolve.Outcome.Turn);

        // Winter 2028: nobody has won, and the calendar is what ends the run.
        Assert.Equal("frozen_front", holds.Outcome!.Code);
        Assert.Equal(26, holds.Outcome.Turn);

        // Spring 2024: the Ukrainian front breaks, two quarters after the aid stops.
        Assert.Equal(Side.Invader.Code, collapses.Outcome!.WinnerSideCode);
        Assert.Equal(11, collapses.Outcome.Turn);

        // A collapse is not the end of the story: the beaten army dissolves over the turns
        // that follow, and the run only stops once the armistice is signed. A run that ended
        // on the turn of the collapse would cut off the model's closing argument.
        Assert.True(resolve.Turns.Count > resolve.Outcome.Turn, "Le dénouement russe n'est pas joué.");
        Assert.True(collapses.Turns.Count > collapses.Outcome.Turn, "Le dénouement ukrainien n'est pas joué.");
        Assert.Equal("armistice", resolve.Outcome.Code);
        Assert.Equal("armistice", collapses.Outcome.Code);
    }

    private static void AssertWithin(double expected, double actual, double tolerance, string label)
    {
        double deviation = Math.Abs(actual - expected) / expected;
        Assert.True(
            deviation <= tolerance,
            $"{label} : {actual:N0} contre {expected:N0} attendus, soit {deviation * 100d:F1} % d'écart.");
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

        // And it is not done by taking ground — which is NOT the same as saying no ground
        // changes hands. Ground does come back to the defender in this run, and the thesis is
        // precisely about the order in which it happens: the invader's power collapses FIRST,
        // and the line follows. The front is a thermometer, never an engine.
        //
        // So what has to be true is the sequence. The quarter the line first moves back is
        // after the quarter the invader's power peaked: the rear had already broken. A run
        // where ground came back before the power fell would be a counter-offensive, and would
        // teach the opposite of everything this model is for.
        int peakTurn = 0;
        for (int index = 0; index < resolve.Turns.Count; index++)
        {
            if (resolve.Turns[index].Invader.CombatPower >= peak)
            {
                peakTurn = index;
                break;
            }
        }

        // The window that carries the whole thesis: from the peak of invader power to the
        // quarter before its regime falls. Across it, the invader loses most of its strength
        // and the line does not move. Ground does change hands elsewhere in this run — the
        // real counter-offensives of autumn 2022 give some back, and the aftermath gives back
        // a great deal — but between those two, the war is decided with a frozen front.
        SideSnapshot atPeak = resolve.Turns[peakTurn].Invader;
        int eve = resolve.Outcome!.Turn - 2;
        SideSnapshot beforeFall = resolve.Turns[eve].Invader;

        Assert.True(
            beforeFall.CombatPower < atPeak.CombatPower * 0.5d,
            $"Puissance russe {atPeak.CombatPower:F0} au pic contre {beforeFall.CombatPower:F0} à la veille "
                + "de la chute : l'étranglement ne se voit pas.");

        // WHY the line does not move, now that it is free to. Ground follows the density of
        // men per kilometre, and the men are still there: the invader has lost its shells, not
        // its infantry. An army starved of ammunition holds the ground it stands on — it simply
        // cannot take any more. The line gives when the MEN go, and the men go when the state
        // stops paying them, which is the quarter the regime falls.
        //
        // This is the guarantee that ground stayed a consequence and did not become the engine:
        // a front reactive enough to drift on its own would have moved during these quarters.
        Assert.True(
            beforeFall.MenInContact > atPeak.MenInContact * 0.85d,
            $"Infanterie de contact {atPeak.MenInContact:N0} au pic contre {beforeFall.MenInContact:N0} "
                + "à la veille de la chute : le terrain a bougé parce que les hommes étaient déjà partis, "
                + "et l'étranglement n'y est pour rien.");

        Assert.True(
            resolve.Turns[^1].Invader.MenInContact < beforeFall.MenInContact * 0.75d,
            "L'armée vaincue n'a jamais lâché la ligne : le terrain n'est pas rendu par la fonte des effectifs.");

        // On the eve of its collapse the invader is still holding ground it took: the defender
        // has not conquered its way to this victory, it has waited for the rear to give.
        double kmBeforeFall = resolve.Turns[eve].SquareKilometresGained;
        Assert.True(
            kmBeforeFall > 0d,
            $"{kmBeforeFall:F0} km² à la veille de la chute : le terrain aurait été repris avant.");

        // Everything comes back afterwards, and only afterwards — the beaten army dissolves
        // and the line follows it. That is the closing argument, and it is worth a decisive
        // sign: the run ends with the invader well behind where it started.
        Assert.True(
            resolve.Turns[^1].SquareKilometresGained < 0d,
            "Le dénouement n'a rendu aucun terrain : l'armée vaincue ne s'est pas défaite.");
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

        // Aid stops at turn 7 — turn 6 before the autumn 2021 prologue was added. The depot
        // still covers the need that turn and the next: this latency is the demonstration,
        // not an artefact of it.
        Assert.Equal(1d, game.Turns[6].Defender.Coverage["weapons"], 2);
        Assert.Equal(1d, game.Turns[7].Defender.Coverage["weapons"], 2);

        // The turn after, the depot is empty and coverage falls off a cliff.
        Assert.True(
            game.Turns[8].Defender.Coverage["weapons"] < 0.6d,
            $"Couverture armes à {game.Turns[8].Defender.Coverage["weapons"]:F2} : la falaise n'a pas eu lieu.");
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

    /// <summary>
    /// The sector detail the board draws its counters from is published, never recomputed on
    /// the page — and publishing it changes nothing. The three runs still land on their turns,
    /// which is the whole guarantee: a figure that moved would mean a quantity was computed a
    /// second time instead of being exposed once.
    /// </summary>
    [Fact]
    public void PublishingSectorDetail_ChangesNoOutcome()
    {
        PlayedGame resolve = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Resolve));
        PlayedGame holds = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Holds));
        PlayedGame collapses = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Collapses));

        Assert.Equal(23, resolve.Outcome!.Turn);
        Assert.Equal("frozen_front", holds.Outcome!.Code);
        Assert.Equal(11, collapses.Outcome!.Turn);
    }

    /// <summary>
    /// What the counters are drawn from holds together: the attacker is named, the ratio is
    /// the published pair, the establishment stands above the power committed wherever a stave
    /// falls short, and the factors that explain a stalled sector are all on the wire. Kept
    /// apart from the outcome guard above so a failure here names the publication and a failure
    /// there names the calibration.
    /// </summary>
    [Fact]
    public void SectorDetail_IsCoherentWithTheResolution()
    {
        PlayedGame resolve = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Resolve));
        PlayedGame holds = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Holds));
        PlayedGame collapses = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Collapses));

        foreach (PlayedGame game in new[] { resolve, holds, collapses })
        {
            foreach (TurnSnapshot turn in game.Turns)
            {
                // Both sides carry orders, and relative weights leave the engine as shares.
                Assert.Equal(2, turn.Orders.Count);
                foreach (SectorOrders orders in turn.Orders)
                {
                    Assert.Equal(1d, orders.EffortShare.Values.Sum(), 6);
                }

                foreach (SectorResolution sector in turn.Sectors)
                {
                    // Who was pushing is stated, not left to the sign of a movement that is
                    // zero on most sectors of most quarters.
                    Assert.Contains(sector.AttackerSideCode, new[] { Side.Invader.Code, Side.Defender.Code });

                    // The ratio is the published pair and nothing else: if these ever drift
                    // apart, the board is drawing one comparison and the engine resolving another.
                    Assert.Equal(sector.AttackerPush / sector.HolderResistance, sector.Ratio, 6);

                    // Liebig, read on the front rather than on the barrel: where the shortest
                    // stave falls short, the establishment stands above the power actually
                    // committed, and the gap is the men who are present and unsupplied. The
                    // two readings must agree — the counter and the barrel draw the same
                    // scarcity, and two surfaces that disagree cost more than either is worth.
                    AssertStavesAgree(turn.Invader.MaterialCoverage, sector.InvaderCommitted, sector.InvaderEstablishment);
                    AssertStavesAgree(turn.Defender.MaterialCoverage, sector.DefenderCommitted, sector.DefenderEstablishment);

                    // The factors that explain a stalled sector are published, not inferred.
                    Assert.True(sector.TerrainMultiplier > 0d);
                    Assert.True(sector.DroneFriction >= 1d);
                    Assert.True(sector.SeasonModifier > 0d);
                }
            }
        }
    }

    /// <summary>
    /// A short stave shows on the counter, a full one leaves it flush. Over-supply is clamped
    /// in the power formula, so the committed figure may sit slightly above the establishment
    /// without meaning anything — the drawing treats that as no gap, and so does this.
    /// </summary>
    private static void AssertStavesAgree(double coverage, double committed, double establishment)
    {
        if (committed <= 0d)
        {
            return;
        }

        // The establishment is a ceiling on the front exactly as it is on the barrel.
        Assert.True(
            establishment >= committed,
            $"Couverture à {coverage:F2} : théorique {establishment:N0} sous l'engagé {committed:N0}.");

        if (coverage >= 0.99d)
        {
            return;
        }

        // Strictly above only once the shortfall outweighs the rounding grain. On a small
        // sector a five per cent gap is fewer than a thousand men: it is real, it is not
        // printable, and rounding it into existence would be the invention the grain exists
        // to prevent.
        double shortfall = committed * ((1d / coverage) - 1d);
        if (shortfall < 2000d)
        {
            return;
        }

        Assert.True(
            establishment > committed,
            $"Couverture à {coverage:F2} : le théorique {establishment:N0} devrait dépasser l'engagé {committed:N0}.");
    }

    /// <summary>
    /// Everything the board reads leaves the engine in men, never in thousands. A sector
    /// counter printing "48" teaches nothing; "48 000 hommes" is immediately an army.
    /// </summary>
    [Fact]
    public void SectorDetail_IsPublishedInMen_LikeEverythingElseTheBoardReads()
    {
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Holds));
        TurnSnapshot turn = game.Turns[8];

        double committed = turn.Sectors.Sum(sector => sector.InvaderCommitted);

        // Sector commitments are shares of one army: assault and cover together add back up to
        // the combat power the side snapshot publishes, and both are in men.
        Assert.True(committed > turn.Invader.CombatPower * 0.5d);
        Assert.True(committed < turn.Invader.CombatPower * 1.5d);

        // A headcount is rounded to the thousand; the two terms of the ratio are not, and the
        // asymmetry is deliberate — see ManCount.
        Assert.True(turn.Sectors.All(sector => sector.InvaderCommitted % 1000d == 0d));
        Assert.True(turn.Sectors.All(sector => sector.DefenderLosses % 1000d == 0d));
    }
}
