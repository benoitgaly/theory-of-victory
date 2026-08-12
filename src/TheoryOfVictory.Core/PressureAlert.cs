namespace TheoryOfVictory.Core;

/// <summary>How close to breaking. Three steps, because a gauge nobody can read is decoration.</summary>
public enum AlertLevel
{
    /// <summary>Worth knowing. Nothing breaks yet.</summary>
    Watch = 0,

    /// <summary>A threshold is now within reach of a bad quarter.</summary>
    Alert = 1,

    /// <summary>It breaks unless something changes this turn.</summary>
    Critical = 2,
}

/// <summary>
/// One readable pressure signal, always derived from the state, never scripted.
/// The point is to make a threshold visible several turns before it is crossed:
/// collapse is a threshold, and a threshold nobody saw coming teaches nothing.
/// </summary>
public sealed class PressureAlert
{
    public required string Code { get; init; }

    /// <summary>Which side is under this pressure.</summary>
    public required string SideCode { get; init; }

    public required AlertLevel Level { get; init; }

    /// <summary>Short label, board-ready.</summary>
    public required string Title { get; init; }

    /// <summary>One sentence saying what breaks, and what it would take not to.</summary>
    public required string Detail { get; init; }

    /// <summary>Turns before it bites, when the model can honestly say. Negative means unknown.</summary>
    public double TurnsAhead { get; init; } = -1d;

    /// <summary>The number the alert is about, for a gauge that moves.</summary>
    public double Value { get; init; }

    /// <summary>The value at which it breaks, so the display can draw the line.</summary>
    public double Threshold { get; init; }
}

/// <summary>
/// Everything forward-looking one side knows about itself at the end of a turn.
/// Nothing here changes the simulation: it reads it, so the player can see the
/// slope before the cliff.
/// </summary>
public sealed class PressureReading
{
    public required string SideCode { get; init; }

    /// <summary>Quarters of depot left per front flow at the observed burn rate.</summary>
    public Dictionary<string, double> StockQuartersLeft { get; init; } = [];

    /// <summary>Quarters of sovereign reserve left at the current liquidation rate.</summary>
    public double ReserveQuartersLeft { get; init; } = double.PositiveInfinity;

    /// <summary>Turns already spent under the generation threshold.</summary>
    public int TurnsBelowThreshold { get; init; }

    /// <summary>Turns left before the front gives way. Negative while the ratio holds.</summary>
    public int TurnsToCollapse { get; init; } = -1;

    /// <summary>Generation ratio moved by this much since last turn. The slope, not the level.</summary>
    public double GenerationTrend { get; init; }

    /// <summary>
    /// Zero to a hundred: how close this side is to any of its breaking points.
    /// The gauge that must move in the victory run while combat power looks fine.
    /// </summary>
    public double ThreatIndex { get; init; }

    public List<PressureAlert> Alerts { get; init; } = [];
}
