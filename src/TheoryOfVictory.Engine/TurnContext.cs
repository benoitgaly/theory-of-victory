using TheoryOfVictory.Core;
using TheoryOfVictory.Core.Localization;

namespace TheoryOfVictory.Engine;

/// <summary>Everything one turn needs, plus what it produced, handed from phase to phase.</summary>
public sealed class TurnContext
{
    public required GameState State { get; init; }

    public required Scenario Scenario { get; init; }

    public required Doctrine InvaderDoctrine { get; set; }

    public required Doctrine DefenderDoctrine { get; set; }

    public List<LocalizedText> Narrative { get; } = [];

    public List<PlayedCard> CardsPlayed { get; } = [];

    public List<SectorResolution> SectorResolutions { get; } = [];

    public StrikeResolution? InvaderStrike { get; set; }

    public StrikeResolution? DefenderStrike { get; set; }

    /// <summary>Men who reached the line this turn, per side. Numerator of the generation ratio.</summary>
    public Dictionary<string, double> ReplacementsArrived { get; } = [];

    public Dictionary<string, double> LossesThisTurn { get; } = [];

    public Dictionary<string, double> WeaponsConsumed { get; } = [];

    public Dictionary<string, double> WeaponsDelivered { get; } = [];

    /// <summary>Depots as they stood before the turn ran, per side. The slope needs two points.</summary>
    public Dictionary<string, Dictionary<string, double>> OpeningStocks { get; } = [];

    /// <summary>Generation ratio carried in from last turn, per side. Same reason.</summary>
    public Dictionary<string, double> OpeningGenerationRatio { get; } = [];

    /// <summary>
    /// The capital posts as they stood before the turn ran, per side. Read with the very same
    /// ruler as the closing position, so the band never subtracts one turn from another: a
    /// variant switched mid-replay would otherwise invent a variation nobody played.
    /// </summary>
    public Dictionary<string, Dictionary<string, double>> OpeningCapital { get; } = [];

    /// <summary>
    /// The cards this turn actually played, with their typed effects. The printed card carries
    /// its rules text and not its effect kinds, and attributing a destruction to a card demands
    /// the kinds — naming the wrong card would be worse than naming none.
    /// </summary>
    public List<EventCard> EventCardsPlayed { get; } = [];

    /// <summary>Forward-looking readings, filled once the ten phases have run.</summary>
    public List<PressureReading> Readings { get; } = [];

    public Doctrine DoctrineFor(Side side)
    {
        return side == Side.Invader ? InvaderDoctrine : DefenderDoctrine;
    }

    public void Say(LocalizedText line)
    {
        Narrative.Add(line);
    }

    public static double Read(Dictionary<string, double> map, Side side)
    {
        return map.TryGetValue(side.Code, out double value) ? value : 0d;
    }

    public static void Accumulate(Dictionary<string, double> map, Side side, double value)
    {
        map[side.Code] = Read(map, side) + value;
    }
}

/// <summary>One step of the nine-phase turn.</summary>
public interface ITurnPhase
{
    string Name { get; }

    void Execute(TurnContext context);
}
