using TheoryOfVictory.Core;
using TheoryOfVictory.Core.Localization;
using TheoryOfVictory.Engine;
using TheoryOfVictory.Engine.Scenarios;
using Xunit;

namespace TheoryOfVictory.Engine.UnitTests;

/// <summary>
/// Locks what the war-capital band claims. The band exists for one sentence — the capital
/// produces what the front consumes, so a side can advance for several quarters on a capital
/// that is already emptying — and a display that cannot be checked is a display that teaches
/// whatever the reader already believed.
/// </summary>
public sealed class WarCapitalTests
{
    private static readonly string[] Posts =
    [
        CapitalReader.Reserves,
        CapitalReader.Grid,
        CapitalReader.Oil,
        CapitalReader.Civilian,
        CapitalReader.Arms,
        CapitalReader.Regime,
        CapitalReader.Foreign,
        CapitalReader.International,
    ];

    [Fact]
    public void TheEightPosts_AreFilled_ForBothSidesOnEveryQuarter()
    {
        foreach (SupportVariant variant in Enum.GetValues<SupportVariant>())
        {
            PlayedGame game = new GameRunner().Run(UkraineScenario.Build(variant));

            foreach (TurnSnapshot turn in game.Turns)
            {
                foreach (SideSnapshot side in new[] { turn.Invader, turn.Defender })
                {
                    Assert.Equal(Posts.Length, side.Capital.Count);

                    foreach (string code in Posts)
                    {
                        CapitalPost post = side.Capital.Single(candidate => candidate.Code == code);
                        Assert.True(
                            double.IsFinite(post.Value) && double.IsFinite(post.Index),
                            $"{side.SideCode} T{turn.Turn} : le poste {code} sort une valeur non finie.");
                    }

                    Assert.True(
                        side.CapitalIndex > 0d && double.IsFinite(side.CapitalIndex),
                        $"{side.SideCode} T{turn.Turn} : indice de capital {side.CapitalIndex}.");
                }
            }
        }
    }

    [Fact]
    public void EveryPost_OpensAtAHundred_OnItsOwnFirstQuarter()
    {
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Holds));
        TurnSnapshot first = game.Turns[0];

        // Trajectories compare, masses do not: each side is indexed against itself, so both
        // start at a hundred whatever the gulf between 310 billions of reserve and 29.
        foreach (SideSnapshot side in new[] { first.Invader, first.Defender })
        {
            foreach (CapitalPost post in side.Capital)
            {
                Assert.Equal(100d, post.Index, 6);
            }

            Assert.Equal(100d, side.CapitalIndex, 6);
        }
    }

    [Fact]
    public void EveryProduction_IsWorthFiveYearsOfItself_AndNothingElseIsCapitalised()
    {
        // The rule of the balance sheet, checked on the two posts whose yearly production the
        // snapshot publishes on its own — the oil receipt and the standing power fleet. One
        // multiple, one place, and no coefficient invented post by post.
        foreach (SupportVariant variant in Enum.GetValues<SupportVariant>())
        {
            PlayedGame game = new GameRunner().Run(UkraineScenario.Build(variant));

            foreach (TurnSnapshot turn in game.Turns)
            {
                foreach (SideSnapshot side in new[] { turn.Invader, turn.Defender })
                {
                    CapitalPost grid = side.Capital.Single(post => post.Code == CapitalReader.Grid);
                    Assert.Equal(CapitalReader.GridValue(side.GridAvailableGw), grid.Value, 6);

                    CapitalPost oil = side.Capital.Single(post => post.Code == CapitalReader.Oil);
                    if (side.OilRevenue > 0d)
                    {
                        Assert.Equal(side.OilRevenue * 4d * CapitalReader.CapitalisationMultiple, oil.Value, 6);
                    }

                    // Aid is a flow that one election cancels: capitalising it would book five
                    // guaranteed years of a package that can stop in a day. The post is worth
                    // exactly the year it is given, and the band totals it apart for that reason.
                    CapitalPost foreign = side.Capital.Single(post => post.Code == CapitalReader.Foreign);
                    Assert.Equal(CapitalNature.AnnualFlow, foreign.Nature);
                    Assert.Equal(CapitalNature.AnnualFlow, side.Capital.Single(post => post.Code == CapitalReader.Regime).Nature);
                }
            }
        }
    }

    [Fact]
    public void EveryPost_IsPricedInBillions_AndTotalledByNature()
    {
        // The whole point of the conversion: one unit, so the seven posts are a balance sheet
        // rather than a list of five languages. The diplomatic position is the one exception,
        // because it is not a possession — and it therefore enters neither total.
        foreach (SupportVariant variant in Enum.GetValues<SupportVariant>())
        {
            PlayedGame game = new GameRunner().Run(UkraineScenario.Build(variant));

            foreach (TurnSnapshot turn in game.Turns)
            {
                foreach (SideSnapshot side in new[] { turn.Invader, turn.Defender })
                {
                    foreach (CapitalPost post in side.Capital)
                    {
                        string expected = post.Code == CapitalReader.International
                            ? TextCodes.Capital.UnitOutOfHundred
                            : TextCodes.Capital.UnitBillions;
                        Assert.Equal(expected, post.Unit.Code);
                    }

                    // Une charge se retranche du bilan au lieu de le gonfler : la facture
                    // pétrolière ukrainienne est de l'argent qui sort, pas du capital détenu.
                    Assert.Equal(
                        side.Capital
                            .Where(post => post.Nature == CapitalNature.Stock)
                            .Sum(post => post.Inverted ? -post.Value : post.Value),
                        side.CapitalStock,
                        6);

                    Assert.True(
                        double.IsFinite(side.CapitalFlow),
                        $"{side.SideCode} T{turn.Turn} : flux annuel non fini.");
                }
            }
        }
    }

    [Fact]
    public void TheBalanceSheet_ShowsRussiaAnOrderOfMagnitudeHeavier_ExceptOnForeignSupport()
    {
        // What the dollar reading reveals and the base-100 index could not: the two camps are
        // an order of magnitude apart on every holding and every production — and on exactly
        // ONE post the smaller camp is the bigger, because it is given what the other has to
        // buy. That single inversion is the game, and it is now visible without a tooltip.
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Holds));
        TurnSnapshot first = game.Turns[0];

        Assert.True(
            first.Invader.CapitalStock > first.Defender.CapitalStock * 5d,
            $"Patrimoine {first.Invader.CapitalStock:F0} contre {first.Defender.CapitalStock:F0} Md$.");

        foreach (string code in new[] { CapitalReader.Reserves, CapitalReader.Grid, CapitalReader.Civilian, CapitalReader.Arms })
        {
            CapitalPost russian = first.Invader.Capital.Single(post => post.Code == code);
            CapitalPost ukrainian = first.Defender.Capital.Single(post => post.Code == code);
            Assert.True(
                russian.Value > ukrainian.Value * 4d,
                $"{code} : {russian.Value:F1} contre {ukrainian.Value:F1} Md$.");
        }

        CapitalPost bought = first.Invader.Capital.Single(post => post.Code == CapitalReader.Foreign);
        CapitalPost given = first.Defender.Capital.Single(post => post.Code == CapitalReader.Foreign);
        Assert.True(
            given.Value > bought.Value,
            $"Soutien extérieur : {given.Value:F1} donné contre {bought.Value:F1} acheté.");

        // Le pétrole n'est pas un poste plus petit chez l'un : c'est un poste de signe opposé.
        // L'Ukraine n'exporte pas d'hydrocarbures, elle en importe — le poste est une charge, et
        // il pèse un ordre de grandeur sous la recette russe. Aucune symétrie n'est forcée.
        CapitalPost receipt = first.Invader.Capital.Single(post => post.Code == CapitalReader.Oil);
        CapitalPost bill = first.Defender.Capital.Single(post => post.Code == CapitalReader.Oil);
        Assert.False(receipt.Inverted);
        Assert.True(bill.Inverted, "Le pétrole ukrainien doit se lire comme une facture.");
        Assert.True(
            receipt.Value > bill.Value * 10d,
            $"Pétrole : {receipt.Value:F0} de recette capitalisée contre {bill.Value:F0} de facture.");
    }

    [Fact]
    public void TheHoldingPost_EmptiesWithTheMargin_AndIsPricedOnSustainableCapacity()
    {
        // Pricing the bill on its own would have swelled the post as the regime got closer to
        // rupture, since a regime in trouble pays MORE to hold. What is priced is the margin,
        // so the post still empties exactly as the apparatus cracks.
        foreach (SupportVariant variant in Enum.GetValues<SupportVariant>())
        {
            PlayedGame game = new GameRunner().Run(UkraineScenario.Build(variant));

            foreach (TurnSnapshot turn in game.Turns)
            {
                foreach (SideSnapshot side in new[] { turn.Invader, turn.Defender })
                {
                    CapitalPost post = side.Capital.Single(candidate => candidate.Code == CapitalReader.Regime);
                    Assert.True(post.Value >= 0d);

                    double margin = Math.Max(0d, 58d - side.RegimeStress);
                    if (margin < 1d || side.ProductiveCapacity <= 0d)
                    {
                        continue;
                    }

                    // The post is the yearly holding bill times the share of the margin left,
                    // and that bill is a share of the SUSTAINABLE capacity — never of headline
                    // GDP, which the war inflates and which would have a regime looking richer
                    // in survival the poorer its economy got.
                    double share = post.Value / (side.ProductiveCapacity * margin / 58d);
                    Assert.True(
                        Math.Abs(share - 0.035d) < 1e-9d || Math.Abs(share - 0.02d) < 1e-9d,
                        $"{side.SideCode} T{turn.Turn} : part de tenue à {share:F4}.");
                }
            }
        }
    }

    [Fact]
    public void TheCapitalIndex_IsNeverZeroedByASinglePost()
    {
        // A side does not die of having lost its oil. It dies of having lost it at the same
        // time as everything else — which is the whole reason the composite is a geometric
        // mean with a floor rather than the minimum rule that governs the front flows.
        List<CapitalPost> ruined = [.. Posts.Select((code, rank) => Synthetic(code, rank == 0 ? 0d : 100d))];
        List<CapitalPost> healthy = [.. Posts.Select(code => Synthetic(code, 100d))];
        List<CapitalPost> halved = [.. Posts.Select(code => Synthetic(code, 50d))];

        Assert.True(CapitalReader.Index(ruined) > 0d);
        Assert.Equal(100d, CapitalReader.Index(healthy), 6);

        // Imbalance is punished, never annulled: one post gone weighs less than every post
        // being half gone, and a sum would have said the exact opposite.
        Assert.True(CapitalReader.Index(halved) < CapitalReader.Index(ruined));
    }

    [Fact]
    public void TheLivingStandard_IsThePowerSupplyExactly_WhileNothingIsStruck()
    {
        // The channel from the rear to the regime used to read the power supply on its own.
        // It now reads the living standard, and the two have to be the same number to the bit
        // on an untouched civilian base — otherwise publishing this post would silently move
        // the three runs it is supposed to observe.
        CivilianIndustry intact = new() { CapacityBillions = 413d };

        foreach (double supply in new[] { 1d, 0.83d, 0.5d, 0.07d, 0d })
        {
            intact.UpdateLivingStandard(supply);
            Assert.Equal(supply, intact.LivingStandard);
        }
    }

    [Fact]
    public void NoScriptedRun_TouchesTheCivilianBase_SoNoOutcomeMoves()
    {
        // The mechanism exists and no calendar exercises it: the strike target is not aimed at
        // by any doctrine and no card of the deck carries the effect. That is what makes the
        // whole post publishable without recalibrating anything.
        foreach (SupportVariant variant in Enum.GetValues<SupportVariant>())
        {
            PlayedGame game = new GameRunner().Run(UkraineScenario.Build(variant));

            foreach (TurnSnapshot turn in game.Turns)
            {
                foreach (SideSnapshot side in new[] { turn.Invader, turn.Defender })
                {
                    CapitalPost civilian = side.Capital.Single(post => post.Code == CapitalReader.Civilian);
                    Assert.Null(civilian.DestructionCause);
                }
            }
        }
    }

    [Fact]
    public void BurningTheWarehouses_MovesTheRegime_WithoutTakingAnythingFromTheFront()
    {
        PlayedGame untouched = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Holds));
        PlayedGame struck = new GameRunner().Run(AimedAtWarehouses(SupportVariant.Holds));

        int last = Math.Min(untouched.Turns.Count, struck.Turns.Count) - 1;
        SideSnapshot before = untouched.Turns[last].Defender;
        SideSnapshot after = struck.Turns[last].Defender;

        CapitalPost civilianBefore = before.Capital.Single(post => post.Code == CapitalReader.Civilian);
        CapitalPost civilianAfter = after.Capital.Single(post => post.Code == CapitalReader.Civilian);

        Assert.True(
            civilianAfter.Value < civilianBefore.Value * 0.8d,
            $"Usines civiles à {civilianAfter.Value:F0} Md$ contre {civilianBefore.Value:F0} sans la campagne.");

        // The whole chain, checked link by link: the plant, then the living standard, then
        // discontent, then the margin the regime has left. The margin is now priced, so it is
        // checked as a share of itself — the Ukrainian holding bill is small in dollars and a
        // threshold in billions would only be measuring the size of the economy.
        Assert.True(civilianAfter.Secondary < 90d, $"Niveau de vie à {civilianAfter.Secondary:F0} %.");
        Assert.True(after.PopularDiscontent > before.PopularDiscontent + 10d);

        CapitalPost regimeBefore = before.Capital.Single(post => post.Code == CapitalReader.Regime);
        CapitalPost regimeAfter = after.Capital.Single(post => post.Code == CapitalReader.Regime);
        Assert.True(
            regimeAfter.Value < regimeBefore.Value * 0.9d,
            $"Tenue du pouvoir à {regimeAfter.Value:F1} Md$ contre {regimeBefore.Value:F1}.");

        // And the point of the whole exercise: the waves that did this took nothing off the
        // front. They were spent on the rear of the rear, and the line is no worse for it.
        Assert.True(
            after.CombatPower >= before.CombatPower * 0.98d,
            $"Puissance à {after.CombatPower:F0} contre {before.CombatPower:F0} : la campagne aurait coûté au front.");
    }

    [Fact]
    public void TheCapitalFalls_WhileTheFrontStillLooksHealthy_InTheVictoryRun()
    {
        // The scissor. Without this the centrepiece of the band demonstrates nothing: the
        // Russian capital has to be visibly draining on a quarter where combat power is still
        // well above where it started.
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Resolve));
        double opening = game.Turns[0].Invader.CombatPower;

        List<TurnSnapshot> diverging =
        [
            .. game.Turns.Where(turn =>
                turn.Invader.CapitalIndex <= 92d
                && turn.Invader.CombatPower >= opening * 1.2d),
        ];

        Assert.True(
            diverging.Count > 0,
            "Aucun trimestre où le capital russe descend pendant que le front paraît encore en forme.");
    }

    [Fact]
    public void EveryRibbon_NamesItsOrigin_OrIsNotDrawnAtAll()
    {
        foreach (SupportVariant variant in Enum.GetValues<SupportVariant>())
        {
            PlayedGame game = new GameRunner().Run(UkraineScenario.Build(variant));

            foreach (TurnSnapshot turn in game.Turns)
            {
                foreach (SideSnapshot side in new[] { turn.Invader, turn.Defender })
                {
                    if (side.Chain is null)
                    {
                        continue;
                    }

                    Assert.False(string.IsNullOrWhiteSpace(side.Chain.Origin.ToString()));
                    Assert.True(side.Chain.Links.Count > 1, "Un ruban d'un seul maillon n'est pas une chaîne.");
                }
            }
        }
    }

    [Fact]
    public void EveryAlertSentence_CarriesTheArticle_BecauseTheBandPrintsItVerbatim()
    {
        // The band prints the engine's own sentence in the tooltip of the post under pressure,
        // so that sentence is display copy and not a log line. It used to be printed by a
        // banner above the band; the banner is gone, the verbatim printing is not.
        // A bare country name inside French prose reads as a caption
        // — "que Ukraine voudrait dépenser" — and the elision is missing on top of it. The
        // article is carried by the belligerent, so no call site has to remember the rule.
        string[] broken =
        [
            "que Russie", "que Ukraine", "de Russie", "de Ukraine",
            "l'Russie", "la Ukraine",
        ];

        foreach (SupportVariant variant in Enum.GetValues<SupportVariant>())
        {
            PlayedGame game = new GameRunner().Run(UkraineScenario.Build(variant));

            foreach (TurnSnapshot turn in game.Turns)
            {
                foreach (PressureAlert alert in turn.Alerts)
                {
                    string detail = Phrasebook.Say(alert.Detail);

                    foreach (string fragment in broken)
                    {
                        Assert.False(
                            detail.Contains(fragment, StringComparison.Ordinal),
                            $"T{turn.Turn} / {alert.Code} : « {fragment} » dans « {detail} ».");
                    }

                    Assert.False(
                        detail.StartsWith("Russie", StringComparison.Ordinal)
                            || detail.StartsWith("Ukraine", StringComparison.Ordinal),
                        $"T{turn.Turn} / {alert.Code} : la phrase s'ouvre sur un nom nu — « {detail} ».");
                }
            }
        }
    }

    [Fact]
    public void TheDiplomaticPost_IsOneQuantity_ReadFromBothSidesOfTheTable()
    {
        // The only post of the eight where one side's gain is exactly the other's loss. Both
        // camps print the same figure — the trade latitude the world still leaves the invader —
        // and the two masses pull against each other. A band that showed it falling for both
        // would be claiming the West can sanction Russia at its own expense.
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Resolve));

        bool everDiverged = false;

        foreach (TurnSnapshot turn in game.Turns)
        {
            CapitalPost invader = turn.Invader.Capital.Single(post => post.Code == CapitalReader.International);
            CapitalPost defender = turn.Defender.Capital.Single(post => post.Code == CapitalReader.International);

            Assert.Equal(invader.Value, defender.Value, 6);
            Assert.False(invader.Inverted);
            Assert.True(defender.Inverted);

            // Read from the point of view of whoever reads it: a door the world closes is a
            // loss in Moscow and a gain in Kyiv, and the same sign under both would put a minus
            // under the side that just won.
            if (Math.Abs(invader.Value - invader.Opening) > 0.01d)
            {
                everDiverged = true;
                Assert.True(
                    invader.PercentDelta * defender.PercentDelta < 0d,
                    "Les deux lectures doivent aller en sens contraire.");
            }
        }

        Assert.True(everDiverged, "Le poste diplomatique n'a jamais bougé : il ne démontre rien.");
    }

    [Fact]
    public void TheDiplomaticPost_CanFallWithNoCardPlayed_BecauseSanctionsErode()
    {
        // Sanctioning is upkeep, not an act: circumvention routes get built and every channel
        // decays on its own. The post must therefore be allowed to move on a quarter where
        // nobody played anything — that is a fact of the game, and the band books it as
        // ordinary consumption rather than inventing a culprit for it.
        PlayedGame game = new GameRunner().Run(UkraineScenario.Build(SupportVariant.Holds));

        bool erodedQuietly = false;

        foreach (TurnSnapshot turn in game.Turns)
        {
            CapitalPost post = turn.Invader.Capital.Single(candidate => candidate.Code == CapitalReader.International);

            if (turn.CardsPlayed.Count == 0 && post.Regeneration > 0.01d)
            {
                erodedQuietly = true;
                Assert.Null(post.DestructionCause);
            }
        }

        Assert.True(erodedQuietly, "L'érosion des sanctions ne se voit jamais sur le poste diplomatique.");
    }

    /// <summary>The same run, with the invader's waves aimed at the civilian base throughout.</summary>
    private static Scenario AimedAtWarehouses(SupportVariant variant)
    {
        Scenario scenario = UkraineScenario.Build(variant);
        scenario.InvaderDoctrine.PrimaryStrikeTarget = StrikeTarget.CivilianIndustry;

        foreach (DoctrineShift shift in scenario.DoctrineShifts)
        {
            if (shift.SideCode == Side.Invader.Code)
            {
                shift.Doctrine.PrimaryStrikeTarget = StrikeTarget.CivilianIndustry;
            }
        }

        return scenario;
    }

    private static CapitalPost Synthetic(string code, double index)
    {
        return new CapitalPost
        {
            Code = code,
            Name = LocalizedText.Of(TextCodes.Verbatim, code),
            Unit = LocalizedText.Of(TextCodes.Capital.UnitBillions),
            Nature = CapitalNature.Stock,
            Value = index,
            Opening = index,
            Reference = 100d,
        };
    }
}
