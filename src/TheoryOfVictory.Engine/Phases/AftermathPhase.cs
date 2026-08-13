using TheoryOfVictory.Core;

namespace TheoryOfVictory.Engine.Phases;

/// <summary>
/// What happens after a side breaks. A regime does not hand back ground the day it falls:
/// it stops paying its army, the army stops being an army, and the front unwinds on its own.
/// This is the only moment of the game where a lot of terrain changes hands without anyone
/// attacking, and it is the closing argument of the whole model — the front is a thermometer.
///
/// The phase does exactly ONE thing: it caps how many men the broken side can still hold in
/// the line. Everything else is left to the phases that already exist. The line moves because
/// <see cref="FrontPhase"/> finds no resistance in front of it, not because this phase moves it;
/// combat power falls because the barrel shrank and cohesion went with it, not because a malus
/// was applied. Scripting the unwinding here would prove nothing: letting the existing
/// resolution produce it is the demonstration. An earlier version put the winner into a pursuit
/// posture and the engine immediately billed it 99 000 men for 61 km² — the model refuses to be
/// told that a collapse is won by attacking, and the lever was removed.
///
/// It holds no state of its own, and no turn number of its own either. Everything it needs is
/// derived from what the engine already publishes — the outcome that recorded the collapse, the
/// turn it happened, and the establishment the broken army was trying to hold — and everything
/// it decides comes from <see cref="AftermathRules"/>, which belongs to the scenario. A run is
/// therefore reproducible from the snapshots alone, and the calendar can change length, or shift
/// by a quarter, without a line of this file moving.
/// </summary>
public sealed class AftermathPhase : ITurnPhase
{
    private const string ArmisticeCode = "armistice";

    public string Name
    {
        get { return "Dénouement"; }
    }

    /// <summary>
    /// True while the war still has something to play: nothing has been decided yet, or a side
    /// has broken and its army has not finished dissolving. False once an ending is final —
    /// which is what tells the runner to stop without cutting the story short.
    /// </summary>
    public static bool KeepsPlaying(GameState state)
    {
        if (state.Outcome is null)
        {
            return true;
        }

        // A frozen front is not an ending, it is the absence of one: the calendar decides.
        return state.Outcome.Code == "frozen_front" || IsCollapse(state.Outcome.Code);
    }

    /// <summary>True once a side has broken and the aftermath is running.</summary>
    public static bool IsUnwinding(GameState state)
    {
        return state.Outcome is not null && IsCollapse(state.Outcome.Code);
    }

    public void Execute(TurnContext context)
    {
        GameState state = context.State;
        if (state.Outcome is null || !IsCollapse(state.Outcome.Code))
        {
            return;
        }

        AftermathRules rules = context.Scenario.Aftermath;
        Belligerent broken = BrokenSide(state);

        // Quarters since the rupture. A difference, never an absolute turn: the calendar may
        // start in 2021 or in 2022 and this phase must not notice.
        int quarters = state.Turn - state.Outcome.Turn;
        if (quarters <= 0)
        {
            return;
        }

        Dissolve(context, rules, broken, quarters);
        AbandonThePositions(state, rules, broken);

        if (broken.Manpower.ManningRatio > rules.ArmisticeManningRatio && quarters < rules.MaxTurns)
        {
            return;
        }

        Declare(context, rules, state, broken, quarters);
    }

    /// <summary>
    /// The army melts. The ceiling falls from the establishment, not from the force observed last
    /// turn: whatever recruitment manages to push back into the line between two quarters cannot
    /// slow the dissolution down. The trajectory belongs to the collapse, not to the budget.
    /// </summary>
    private static void Dissolve(TurnContext context, AftermathRules rules, Belligerent broken, int quarters)
    {
        Manpower manpower = broken.Manpower;

        // A state that has stopped functioning does not train men and does not send them to the
        // line either. Without this the dissolution is not monotonic: recruitment refills part of
        // the line between two quarters, the force bounces back, and the front stalls for a
        // quarter or two before letting go all at once — which reads as a scripted flip.
        manpower.TrainingCapacityPerTurn = 0d;

        double ceiling = manpower.TargetForceSize * rules.RemainingShareAfter(quarters);

        double left = Math.Max(0d, manpower.AtFront - ceiling);
        if (left <= 0d)
        {
            return;
        }

        manpower.AtFront -= left;

        // These men are not casualties. They went home, and counting them as losses would be a
        // plain lie on a page that shows cumulative losses in the next column.
        manpower.MobilisablePool += left;

        context.Say(
            $"Plus personne ne paie l'armée {Adjective(broken)} : {left * 1000d:N0} hommes ont quitté "
            + "la ligne ce trimestre. Aucun assaut ne les en a délogés.");
    }

    /// <summary>
    /// A trench held by a tenth of the men who dug it is worth a tenth. Prepared positions decay
    /// at the same rate as the army that mans them — not to zero at once, which would make the
    /// front let go in a single quarter, but quarter after quarter, so the resistance falls
    /// smoothly instead of falling off a cliff.
    ///
    /// This is what turns the ending into something you watch happen. Without it, the line holds
    /// perfectly until the exact quarter the ratio crosses its threshold and then moves all at
    /// once — which reads as a scripted flip rather than as a delitement. With it, the sectors
    /// come back one after another, in the order their terrain allows.
    /// </summary>
    private static void AbandonThePositions(GameState state, AftermathRules rules, Belligerent broken)
    {
        double kept = 1d - rules.DissolutionPerTurn;

        // What dissolves is not the men, it is the army as an organisation — and the supply chain
        // goes with it. Without this the dissolution produces the opposite of what it should: the
        // depots stay full while the mouths disappear, per-man coverage climbs from 0,12 to 0,84
        // in one quarter, and the combat power of a halved army TRIPLES. The front then holds for
        // two quarters and lets go all at once. An army nobody pays has no logistics either.
        broken.Politics.LogisticsIntegrity = Math.Max(0.05d, broken.Politics.LogisticsIntegrity * kept);

        foreach (FrontSector sector in state.Sectors)
        {
            if (broken.Side == Side.Invader)
            {
                sector.InvaderFortification *= kept;
                continue;
            }

            sector.DefenderFortification *= kept;
        }
    }

    /// <summary>
    /// The ending, named. It is declared before the ten phases run, and the quarter that declares
    /// it is not played at all: there is no front left to resolve, and resolving one anyway sends
    /// the line running on a ratio that has lost all meaning.
    /// </summary>
    private static void Declare(
        TurnContext context,
        AftermathRules rules,
        GameState state,
        Belligerent broken,
        int quarters)
    {
        string cause = state.Outcome!.Code switch
        {
            "regime_collapse" => "Le régime est tombé, et l'armée a cessé d'être payée.",
            "negotiated_capitulation" => "La volonté a cédé, et l'armée a cessé d'être tenue.",
            _ => "La génération de force s'est tarie, et l'armée a cessé d'être alimentée.",
        };

        state.Outcome = new GameOutcome
        {
            Code = ArmisticeCode,
            Title = $"Armistice — {broken.Name} se retire",
            Explanation = cause
                + $" En {quarters} trimestres, elle est tombée sous "
                + $"{rules.ArmisticeManningRatio * 100d:F0} % de son effectif théorique sans qu'une "
                + "seule attaque l'ait emportée : les secteurs sont revenus parce qu'il n'y avait "
                + "plus personne pour les tenir. Le front n'a jamais été le moteur de cette guerre — "
                + "il en était le thermomètre, et il vient de le montrer une dernière fois.",
            WinnerSideCode = broken.Side.Opponent.Code,

            // The turn the war was DECIDED, not the turn it stopped. The armistice is a
            // consequence with a date of its own, but the demonstration is about the quarter the
            // flux ran out — and that is the turn the design pins, so it is the one published.
            Turn = state.Outcome.Turn,
        };

        context.Say(
            $"ARMISTICE — l'armée {Adjective(broken)} n'existe plus comme force organisée. "
            + "Le terrain a changé de mains sans bataille.");
    }

    /// <summary>
    /// The side that broke. Every collapse outcome names its winner, so the loser is never
    /// ambiguous — including in the run where both sides end up marked as collapsed.
    /// </summary>
    private static Belligerent BrokenSide(GameState state)
    {
        string? winner = state.Outcome?.WinnerSideCode;
        if (winner is not null)
        {
            return state.Get(Side.FromCode(winner).Opponent);
        }

        return state.Invader.HasCollapsed ? state.Invader : state.DefenderSide;
    }

    private static bool IsCollapse(string code)
    {
        return code is "military_collapse" or "regime_collapse" or "negotiated_capitulation";
    }

    private static string Adjective(Belligerent belligerent)
    {
        return belligerent.Side == Side.Invader ? "russe" : "ukrainienne";
    }
}
