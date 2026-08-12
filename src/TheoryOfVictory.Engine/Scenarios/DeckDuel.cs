using TheoryOfVictory.Core;

namespace TheoryOfVictory.Engine.Scenarios;

/// <summary>
/// Three explicit theories of victory, each a deck. The deck is the strategic gesture,
/// and it is what the title of the game refers to.
/// </summary>
public enum DeckArchetype
{
    /// <summary>Cut the flows at their source: refining, exports, components, rail.</summary>
    DeepStrike = 0,

    /// <summary>Out-produce and out-man the enemy, and grind the line forward.</summary>
    FrontalAttrition = 1,

    /// <summary>Exhaust the will — the enemy's, and its supporters'.</summary>
    Political = 2,
}

/// <summary>What one deck did to the same war.</summary>
public sealed class DuelResult
{
    public required DeckArchetype Archetype { get; init; }

    public required string Name { get; init; }

    /// <summary>Total political capital the deck costs to play in full.</summary>
    public required double PoliticalCost { get; init; }

    public required int Plays { get; init; }

    public required string OutcomeCode { get; init; }

    public required string OutcomeTitle { get; init; }

    /// <summary>Turn the war was decided on. The scenario length when it was not.</summary>
    public required int DecidedOnTurn { get; init; }

    public required bool DefenderWins { get; init; }

    /// <summary>Invader combat power at the end, against the baseline that plays no deck.</summary>
    public required double InvaderPowerAtEnd { get; init; }

    /// <summary>Ground taken over the whole war, in square kilometres. Negative is ground retaken.</summary>
    public required double GroundTaken { get; init; }
}

/// <summary>
/// The balance instrument the design document asks for by name: on the same war, the deep
/// strike deck has to beat the frontal attrition deck. If it does not, the game says the
/// opposite of its own thesis, and no amount of flavour text repairs that.
///
/// All three decks are built to the same political capital budget, so the comparison is
/// between theories of victory and not between budgets.
/// </summary>
public static class DeckDuel
{
    /// <summary>Every deck is priced to this, so the duel compares ideas and not spending.</summary>
    public const double CapitalBudget = 44d;

    public static Scenario Build(DeckArchetype archetype)
    {
        // The frozen-front run is the control: on its own it decides nothing, so whatever
        // the deck changes is the deck's doing.
        Scenario scenario = UkraineScenario.Build(SupportVariant.Holds);

        scenario.Calendar.AddRange(Deck(archetype));
        Steer(scenario, archetype);
        return scenario;
    }

    public static List<DuelResult> Compare()
    {
        GameRunner runner = new();
        List<DuelResult> results = [];

        foreach (DeckArchetype archetype in Enum.GetValues<DeckArchetype>())
        {
            Scenario scenario = Build(archetype);
            PlayedGame game = runner.Run(scenario);

            List<ScheduledCard> deck = Deck(archetype);
            double cost = 0d;
            foreach (ScheduledCard scheduled in deck)
            {
                foreach (EventCard card in scenario.Deck)
                {
                    if (string.Equals(card.Code, scheduled.CardCode, StringComparison.OrdinalIgnoreCase))
                    {
                        cost += card.PoliticalCost;
                        break;
                    }
                }
            }

            TurnSnapshot last = game.Turns[^1];

            results.Add(new DuelResult
            {
                Archetype = archetype,
                Name = Name(archetype),
                PoliticalCost = cost,
                Plays = deck.Count,
                OutcomeCode = game.Outcome?.Code ?? "unresolved",
                OutcomeTitle = game.Outcome?.Title ?? "Partie non conclue",
                DecidedOnTurn = game.Turns.Count,
                DefenderWins = game.Outcome?.WinnerSideCode == Side.Defender.Code,
                InvaderPowerAtEnd = last.Invader.CombatPower,
                GroundTaken = last.SquareKilometresGained,
            });
        }

        return results;
    }

    public static string Name(DeckArchetype archetype)
    {
        return archetype switch
        {
            DeckArchetype.DeepStrike => "Frappe profonde",
            DeckArchetype.FrontalAttrition => "Attrition frontale",
            DeckArchetype.Political => "Épuisement politique",
            _ => archetype.ToString(),
        };
    }

    /// <summary>
    /// Each deck also carries the doctrine that goes with it: a theory of victory is not
    /// only which cards you hold, it is what you do with the budget every turn.
    /// </summary>
    private static void Steer(Scenario scenario, DeckArchetype archetype)
    {
        Doctrine doctrine = scenario.DefenderDoctrine;

        switch (archetype)
        {
            case DeckArchetype.DeepStrike:
                // Hold the line cheaply, spend on the vectors that reach the refineries.
                doctrine.OffensivePosture = 0.24d;
                doctrine.StrikeVectorsShare = 0.26d;
                doctrine.WeaponsShare = 0.16d;
                doctrine.RecruitmentShare = 0.16d;
                doctrine.FortificationShare = 0.1d;
                doctrine.PrimaryStrikeTarget = StrikeTarget.Refining;
                break;

            case DeckArchetype.FrontalAttrition:
                // More men, more shells, and push. The intuitive deck, and the expensive one.
                doctrine.OffensivePosture = 0.56d;
                doctrine.StrikeVectorsShare = 0.05d;
                doctrine.WeaponsShare = 0.34d;
                doctrine.RecruitmentShare = 0.3d;
                doctrine.IndustrialExpansionShare = 0.12d;
                doctrine.FortificationShare = 0.02d;
                break;

            case DeckArchetype.Political:
                // Neither ground nor refineries: the enemy's cohesion and its own supporters.
                doctrine.OffensivePosture = 0.26d;
                doctrine.StrikeVectorsShare = 0.12d;
                doctrine.WeaponsShare = 0.2d;
                doctrine.RecruitmentShare = 0.18d;
                doctrine.FortificationShare = 0.1d;
                doctrine.PrimaryStrikeTarget = StrikeTarget.Logistics;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(archetype), archetype, "Unknown deck.");
        }

        // The doctrine shifts scripted into the historical run would overwrite the deck's
        // own posture two turns in, and the duel would compare nothing at all.
        scenario.DoctrineShifts.RemoveAll(shift => shift.SideCode == Side.Defender.Code);
    }

    private static List<ScheduledCard> Deck(DeckArchetype archetype)
    {
        return archetype switch
        {
            // 6×4 on the campaigns + 7 + 4 + 3×3 = 44.
            DeckArchetype.DeepStrike =>
            [
                new ScheduledCard { Turn = 13, CardCode = "rail_interdiction" },
                new ScheduledCard { Turn = 4, CardCode = "refinery_campaign_sustained" },
                new ScheduledCard { Turn = 6, CardCode = "refinery_campaign_sustained" },
                new ScheduledCard { Turn = 8, CardCode = "refinery_campaign_sustained" },
                new ScheduledCard { Turn = 10, CardCode = "refinery_campaign_sustained" },
                new ScheduledCard { Turn = 12, CardCode = "refinery_campaign_sustained" },
                new ScheduledCard { Turn = 14, CardCode = "refinery_campaign_sustained" },
                new ScheduledCard { Turn = 5, CardCode = "component_embargo_total" },
                new ScheduledCard { Turn = 7, CardCode = "rail_interdiction" },
                new ScheduledCard { Turn = 11, CardCode = "rail_interdiction" },
                new ScheduledCard { Turn = 9, CardCode = "component_embargo" },
            ],

            // 8 + 10 + 7 + 4 + 4 + 4 + 4 + 3 = 44.
            DeckArchetype.FrontalAttrition =>
            [
                new ScheduledCard { Turn = 4, CardCode = "aid_predictable" },
                new ScheduledCard { Turn = 6, CardCode = "frozen_assets_released" },
                new ScheduledCard { Turn = 7, CardCode = "conscription_law" },
                new ScheduledCard { Turn = 9, CardCode = "drone_swarm_scaling" },
                new ScheduledCard { Turn = 5, CardCode = "western_aid_opens" },
                new ScheduledCard { Turn = 11, CardCode = "drone_swarm_scaling" },
                new ScheduledCard { Turn = 13, CardCode = "conscription_law" },
            ],

            // 8 + 8 + 10 + 6 + 6 + 6 + 0 = 44.
            DeckArchetype.Political =>
            [
                new ScheduledCard { Turn = 5, CardCode = "decapitation_strike" },
                new ScheduledCard { Turn = 9, CardCode = "decapitation_strike" },
                new ScheduledCard { Turn = 7, CardCode = "frozen_assets_released" },
                new ScheduledCard { Turn = 11, CardCode = "supplier_withdraws" },
                new ScheduledCard { Turn = 13, CardCode = "shadow_fleet_sanctions" },
                new ScheduledCard { Turn = 14, CardCode = "supplier_withdraws" },
                new ScheduledCard { Turn = 15, CardCode = "elite_fracture" },
            ],

            _ => throw new ArgumentOutOfRangeException(nameof(archetype), archetype, "Unknown deck."),
        };
    }
}
