using TheoryOfVictory.Core;

namespace TheoryOfVictory.Engine;

/// <summary>Everything one turn needs, plus what it produced, handed from phase to phase.</summary>
public sealed class TurnContext
{
    public required GameState State { get; init; }

    public required Scenario Scenario { get; init; }

    public required Doctrine InvaderDoctrine { get; set; }

    public required Doctrine DefenderDoctrine { get; set; }

    public List<string> Narrative { get; } = [];

    public List<PlayedCard> CardsPlayed { get; } = [];

    public List<SectorResolution> SectorResolutions { get; } = [];

    public StrikeResolution? InvaderStrike { get; set; }

    public StrikeResolution? DefenderStrike { get; set; }

    /// <summary>Men who reached the line this turn, per side. Numerator of the generation ratio.</summary>
    public Dictionary<string, double> ReplacementsArrived { get; } = [];

    public Dictionary<string, double> LossesThisTurn { get; } = [];

    public Dictionary<string, double> WeaponsConsumed { get; } = [];

    public Dictionary<string, double> WeaponsDelivered { get; } = [];

    public Doctrine DoctrineFor(Side side)
    {
        return side == Side.Invader ? InvaderDoctrine : DefenderDoctrine;
    }

    public void Say(string line)
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
