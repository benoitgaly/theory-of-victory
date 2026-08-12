using TheoryOfVictory.Core;
using TheoryOfVictory.Engine;
using TheoryOfVictory.Engine.Scenarios;

namespace TheoryOfVictory.Web.Services;

/// <summary>
/// V1.0 is deterministic, so both runs are played once at startup and replayed
/// from memory. No database, no state, same output every time.
/// </summary>
public sealed class PlayedGameLibrary
{
    private readonly Dictionary<string, PlayedGame> _games = [];

    public PlayedGameLibrary()
    {
        GameRunner runner = new();

        foreach (SupportVariant variant in Enum.GetValues<SupportVariant>())
        {
            Scenario scenario = UkraineScenario.Build(variant);
            PlayedGame game = runner.Run(scenario);
            _games[game.ScenarioCode] = game;
        }
    }

    public IReadOnlyList<PlayedGame> All
    {
        get { return [.. _games.Values]; }
    }

    public PlayedGame? Get(string code)
    {
        return _games.GetValueOrDefault(code);
    }

    /// <summary>Sector geometry is shared by both runs; the board is drawn from it.</summary>
    public IReadOnlyList<FrontSector> BoardSectors
    {
        get { return _games.Values.First().FinalSectors; }
    }
}
