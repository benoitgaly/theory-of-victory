using TheoryOfVictory.Core;
using TheoryOfVictory.Core.Localization;
using TheoryOfVictory.Engine;
using TheoryOfVictory.Engine.Scenarios;
using System.Linq;

namespace TheoryOfVictory.Web.Services;

/// <summary>
/// V1.0 is deterministic, so both runs are played once at startup and replayed
/// from memory. No database, no state, same output every time.
///
/// The runs themselves are played ONCE, not once per language: they are arithmetic, and a
/// second run in another language would be a second chance for the two sites to disagree on
/// what happened. Only what is read from a data file — the deck, the chronicle of the real
/// front — exists per language, because that prose lives beside its data.
/// </summary>
public sealed class PlayedGameLibrary
{
    private readonly Dictionary<string, PlayedGame> _games = [];

    private readonly Dictionary<Language, IReadOnlyList<PlayedCard>> _decks = [];

    private readonly Dictionary<Language, FrontHistory> _fronts = [];

    public PlayedGameLibrary()
    {
        GameRunner runner = new();

        foreach (SupportVariant variant in Enum.GetValues<SupportVariant>())
        {
            Scenario scenario = UkraineScenario.Build(variant);
            PlayedGame game = runner.Run(scenario);
            _games[game.ScenarioCode] = game;
        }

        foreach (Language language in Languages.All)
        {
            _decks[language] = [.. CardLibrary.Load(language).Select(CardPrinter.Print)];
            _fronts[language] = FrontHistoryLibrary.Load(language);
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

    /// <summary>
    /// The full printed deck. V1.0 plays a calendar, but the page shows each side the hand
    /// it would have been choosing from — which is the whole V2 gesture, previewed.
    /// </summary>
    public IReadOnlyList<PlayedCard> Deck(Language language)
    {
        return _decks[language];
    }

    /// <summary>
    /// The real front, quarter by quarter, from the autumn of 2021 to the summer of 2026. The
    /// board draws the documented quarters from this and the projected ones from the run — and
    /// says on screen which of the two it is showing. The engine never reads it.
    /// </summary>
    public FrontHistory FrontHistory(Language language)
    {
        return _fronts[language];
    }
}
