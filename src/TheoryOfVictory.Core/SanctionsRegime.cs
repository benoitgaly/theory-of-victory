namespace TheoryOfVictory.Core;

/// <summary>
/// Sanctions are not a GDP malus. They pinch three channels and GDP follows.
/// The one that decides is the slowest: components cap arms production.
/// Every channel erodes on its own — sanctioning is upkeep, not an act.
/// </summary>
public sealed class SanctionsRegime
{
    /// <summary>Widens the export discount per barrel. Immediate.</summary>
    public double PriceSeverity { get; set; }

    /// <summary>Permanent tax on everything crossing the border. Fast, moderate.</summary>
    public double FrictionSeverity { get; set; }

    /// <summary>Caps arms production: machine tools, bearings, optics. Slow, decisive.</summary>
    public double ComponentSeverity { get; set; }

    /// <summary>Circumvention routes get built, so severity decays unless tightened.</summary>
    public double ErosionPerTurn { get; set; } = 0.11d;

    /// <summary>Component effects arrive with a lag, this is the share already biting.</summary>
    public double ComponentRealisation { get; set; }

    public double ExportDiscountPerBarrel
    {
        get { return PriceSeverity * 22d; }
    }

    public double FrictionRate
    {
        get { return Math.Clamp(FrictionSeverity * 0.18d, 0d, 0.35d); }
    }

    /// <summary>Multiplier applied to domestic arms production capacity.</summary>
    public double ProductionCeilingMultiplier
    {
        get { return Math.Clamp(1d - (ComponentSeverity * ComponentRealisation * 0.4d), 0.5d, 1d); }
    }

    public void Tighten(double price, double friction, double component)
    {
        PriceSeverity = Math.Clamp(PriceSeverity + price, 0d, 1d);
        FrictionSeverity = Math.Clamp(FrictionSeverity + friction, 0d, 1d);
        ComponentSeverity = Math.Clamp(ComponentSeverity + component, 0d, 1d);
    }

    public void AdvanceTurn()
    {
        PriceSeverity = Math.Max(0d, PriceSeverity * (1d - ErosionPerTurn));
        FrictionSeverity = Math.Max(0d, FrictionSeverity * (1d - ErosionPerTurn));
        ComponentSeverity = Math.Max(0d, ComponentSeverity * (1d - (ErosionPerTurn * 0.6d)));

        // Component pain builds up slowly, which is why it is underestimated.
        ComponentRealisation = Math.Min(1d, ComponentRealisation + 0.08d);
    }
}
