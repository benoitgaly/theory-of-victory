using TheoryOfVictory.Core;

namespace TheoryOfVictory.Engine;

/// <summary>A whole scenario played out, ready to be replayed turn by turn on screen.</summary>
public sealed class PlayedGame
{
    public required string ScenarioCode { get; init; }

    public required string Title { get; init; }

    public required string Subtitle { get; init; }

    public required string Description { get; init; }

    public required List<TurnSnapshot> Turns { get; init; }

    public required List<FrontSector> FinalSectors { get; init; }

    public GameOutcome? Outcome { get; init; }

    public double TotalHexesGained
    {
        get { return Turns.Count == 0 ? 0d : Turns[^1].TotalHexesGained; }
    }
}

/// <summary>Plays a deterministic scenario end to end. Same input, same output, always.</summary>
public sealed class GameRunner
{
    private readonly TurnEngine _engine = new();

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

        for (int turn = 1; turn <= scenario.TurnCount; turn++)
        {
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

            TurnSnapshot snapshot = _engine.ExecuteTurn(state, scenario, invaderDoctrine, defenderDoctrine);
            turns.Add(snapshot);
            state.History.Add(snapshot);

            if (state.Outcome is not null && state.Outcome.Code != "frozen_front")
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
            Outcome = state.Outcome,
        };
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
