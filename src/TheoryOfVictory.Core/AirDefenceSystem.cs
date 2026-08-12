namespace TheoryOfVictory.Core;

/// <summary>
/// The same interceptor cannot be over Kyiv and over Pokrovsk. This split is the
/// sharpest trade-off in the game, and it is entirely real.
/// </summary>
public sealed class AirDefenceSystem
{
    /// <summary>Share of interceptors held back to protect grid, refining and industry.</summary>
    public double RearShare { get; set; } = 0.6d;

    /// <summary>Radar and launcher coverage of the protected area, 0 to 1.</summary>
    public double Coverage { get; set; } = 0.6d;

    /// <summary>Drones a single cheap interceptor unit can engage per turn.</summary>
    public double CheapEngagementsPerUnit { get; set; } = 1.15d;

    /// <summary>Missiles a single heavy interceptor unit can engage per turn.</summary>
    public double HeavyEngagementsPerUnit { get; set; } = 0.85d;

    /// <summary>Heavy rounds burnt on leaking cheap drones — the exchange-ratio trap.</summary>
    public double HeavyWasteOnDrones { get; set; } = 0.35d;

    /// <summary>Share of the air defence budget going to cheap kill chains rather than heavy rounds.</summary>
    public double CheapPurchaseShare { get; set; } = 0.5d;

    public double FrontShare
    {
        get { return Math.Clamp(1d - RearShare, 0d, 1d); }
    }
}
