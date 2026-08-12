namespace TheoryOfVictory.Core;

/// <summary>
/// Innovation never multiplies raw power: it lets the same effect be obtained with
/// another resource — the one you actually have. It therefore moves the bottleneck.
/// Every edge decays on its own each turn: the Red Queen.
/// </summary>
public sealed class Innovation
{
    /// <summary>Substitution of drones for shells, reduces weapon demand at the front.</summary>
    public double TacticalDroneEdge { get; set; }

    /// <summary>Penetration edge of strike vectors against enemy defences.</summary>
    public double StrikeEdge { get; set; }

    /// <summary>Electronic warfare and cheap kill chains, raises interception per euro.</summary>
    public double CounterDroneEdge { get; set; }

    /// <summary>Share of an edge lost per turn as the enemy adapts.</summary>
    public double DecayPerTurn { get; set; } = 0.14d;

    /// <summary>How fast investment converts into edge. Ukraine is fast and small, Russia slow and vast.</summary>
    public double AdoptionSpeed { get; set; } = 1d;

    /// <summary>Ceiling reachable by this side, set by industrial scale.</summary>
    public double ScaleCeiling { get; set; } = 1d;

    /// <summary>Weapon demand multiplier: drone substitution lowers the shells needed.</summary>
    public double WeaponDemandMultiplier
    {
        get { return Math.Clamp(1d - (TacticalDroneEdge * 0.35d), 0.55d, 1d); }
    }

    public void Decay()
    {
        TacticalDroneEdge = Math.Max(0d, TacticalDroneEdge * (1d - DecayPerTurn));
        StrikeEdge = Math.Max(0d, StrikeEdge * (1d - DecayPerTurn));
        CounterDroneEdge = Math.Max(0d, CounterDroneEdge * (1d - DecayPerTurn));
    }

    public void Invest(double billions, double tacticalShare, double strikeShare, double counterShare)
    {
        double gain = billions * AdoptionSpeed * 0.045d;
        TacticalDroneEdge = Math.Min(ScaleCeiling, TacticalDroneEdge + (gain * tacticalShare));
        StrikeEdge = Math.Min(ScaleCeiling, StrikeEdge + (gain * strikeShare));
        CounterDroneEdge = Math.Min(ScaleCeiling, CounterDroneEdge + (gain * counterShare));
    }
}
