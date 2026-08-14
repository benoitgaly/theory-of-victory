using TheoryOfVictory.Core;
using TheoryOfVictory.Core.Localization;
using TheoryOfVictory.Engine.Phases;

namespace TheoryOfVictory.Engine;

/// <summary>A whole scenario played out, ready to be replayed turn by turn on screen.</summary>
public sealed class PlayedGame
{
    public required string ScenarioCode { get; init; }

    public required LocalizedText Title { get; init; }

    public required LocalizedText? Subtitle { get; init; }

    public required LocalizedText? Description { get; init; }

    public required List<TurnSnapshot> Turns { get; init; }

    public required List<FrontSector> FinalSectors { get; init; }

    /// <summary>
    /// Turns the scenario had on the calendar. A run that ends early stopped because the
    /// war ended, not because the timeline ran out — the display has to say which.
    /// </summary>
    public required int PlannedTurns { get; init; }

    public GameOutcome? Outcome { get; init; }

    /// <summary>
    /// How the war was decided, when that is not how it ended. A side breaking is one event and
    /// the armistice that follows is another: <see cref="Outcome"/> carries the ending, this
    /// carries the rupture that caused it. Null whenever the two are the same thing.
    /// </summary>
    public GameOutcome? Decision { get; init; }

    public bool EndedEarly
    {
        get { return Turns.Count < PlannedTurns; }
    }

    public double TotalHexesGained
    {
        get { return Turns.Count == 0 ? 0d : Turns[^1].TotalHexesGained; }
    }
}

/// <summary>Plays a deterministic scenario end to end. Same input, same output, always.</summary>
public sealed class GameRunner
{
    private readonly TurnEngine _engine = new();

    private readonly AftermathPhase _aftermath = new();

    public PlayedGame Run(Scenario scenario)
    {
        GameState state = new()
        {
            Invader = scenario.Invader,
            DefenderSide = scenario.Defender,
            Sectors = scenario.Sectors,
            Turn = 0,
            Year = scenario.StartYear,
            Season = scenario.StartSeason,
            OilPrice = scenario.OilPriceAt(1),
        };

        Doctrine invaderDoctrine = scenario.InvaderDoctrine.Clone();
        Doctrine defenderDoctrine = scenario.DefenderDoctrine.Clone();

        List<TurnSnapshot> turns = [];

        // The rupture, kept before the armistice replaces it: two events, both worth publishing.
        GameOutcome? decision = null;

        // The war can outlast its own calendar. When a side breaks on the last scripted quarter,
        // stopping there would hide the only thing that collapse actually produces — so the loop
        // runs on while the aftermath unwinds, and never further than its hard bound.
        int lastPossibleTurn = scenario.TurnCount + scenario.Aftermath.MaxTurns;

        for (int turn = 1; turn <= lastPossibleTurn; turn++)
        {
            if (turn > scenario.TurnCount && !AftermathPhase.IsUnwinding(state))
            {
                break;
            }

            state.Turn = turn;
            AdvanceCalendar(state, scenario, turn);

            foreach (DoctrineShift shift in scenario.DoctrineShifts)
            {
                if (shift.Turn != turn)
                {
                    continue;
                }

                if (Side.FromCode(shift.SideCode) == Side.Invader)
                {
                    invaderDoctrine = shift.Doctrine.Clone();
                }
                else
                {
                    defenderDoctrine = shift.Doctrine.Clone();
                }
            }

            // The dissolution is settled before the ten phases run. The quarter that declares the
            // armistice is therefore not played at all: there is no front left to resolve, and
            // resolving one anyway would send the line running on a ratio that has lost all
            // meaning. The war stops on the last quarter that still had something to decide.
            List<LocalizedText> aftermathLines = RunAftermath(state, scenario, invaderDoctrine, defenderDoctrine);
            if (!AftermathPhase.KeepsPlaying(state))
            {
                // The quarter that declares the armistice is not played, but it still has the
                // last word: it goes to the last quarter that was.
                if (turns.Count > 0)
                {
                    turns[^1].Narrative.AddRange(aftermathLines);
                }

                break;
            }

            TurnSnapshot snapshot = _engine.ExecuteTurn(state, scenario, invaderDoctrine, defenderDoctrine);
            snapshot.Narrative.InsertRange(0, aftermathLines);

            turns.Add(snapshot);
            state.History.Add(snapshot);

            if (decision is null && AftermathPhase.IsUnwinding(state))
            {
                decision = state.Outcome;
            }

            if (!AftermathPhase.KeepsPlaying(state))
            {
                break;
            }
        }

        return new PlayedGame
        {
            ScenarioCode = scenario.Code,
            Title = scenario.Title,
            Subtitle = scenario.Subtitle,
            Description = scenario.Description,
            Turns = turns,
            FinalSectors = scenario.Sectors,
            PlannedTurns = scenario.TurnCount,
            Outcome = state.Outcome,
            Decision = ReferenceEquals(decision, state.Outcome) ? null : decision,
        };
    }

    /// <summary>
    /// Runs the aftermath on its own context and returns what it had to say. Nothing else in the
    /// turn reads that context: it exists so the phase keeps the ordinary phase signature and can
    /// be moved into the turn engine, in its proper place, the day that file is free.
    /// </summary>
    private List<LocalizedText> RunAftermath(
        GameState state,
        Scenario scenario,
        Doctrine invaderDoctrine,
        Doctrine defenderDoctrine)
    {
        if (!AftermathPhase.IsUnwinding(state))
        {
            return [];
        }

        TurnContext context = new()
        {
            State = state,
            Scenario = scenario,
            InvaderDoctrine = invaderDoctrine,
            DefenderDoctrine = defenderDoctrine,
        };

        _aftermath.Execute(context);
        return context.Narrative;
    }

    private static void AdvanceCalendar(GameState state, Scenario scenario, int turn)
    {
        int offset = turn - 1;
        int seasonIndex = ((int)scenario.StartSeason + offset) % 4;
        int yearsPassed = ((int)scenario.StartSeason + offset) / 4;

        state.Season = (Season)seasonIndex;
        state.Year = scenario.StartYear + yearsPassed;
    }
}
