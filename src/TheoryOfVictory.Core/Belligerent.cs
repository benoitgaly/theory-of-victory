namespace TheoryOfVictory.Core;

public sealed class Belligerent
{
    public required Side Side { get; init; }

    public required string Name { get; init; }

    public required PoliticalState Politics { get; init; }

    public required ForeignSupport Foreign { get; init; }

    public Economy Economy { get; init; } = new();

    public EnergyGrid Grid { get; init; } = new();

    public Manpower Manpower { get; init; } = new();

    public ArmsIndustry Industry { get; init; } = new();

    public Innovation Innovation { get; init; } = new();

    public AirDefenceSystem AirDefence { get; init; } = new();

    public SanctionsRegime Sanctions { get; init; } = new();

    public Stockpile Stock { get; init; } = new();

    /// <summary>Resources that actually reached the front this turn, after transmission losses.</summary>
    public Dictionary<string, double> DeliveredThisTurn { get; } = [];

    /// <summary>Coverage ratio per front flow, computed at the front phase.</summary>
    public Dictionary<string, double> CoverageThisTurn { get; } = [];

    /// <summary>What the front actually required this turn, denominator of coverage.</summary>
    public Dictionary<string, double> NeedThisTurn { get; } = [];

    /// <summary>Budget put on each spending line this turn, for the economic view.</summary>
    public Dictionary<string, double> AllocationThisTurn { get; } = [];

    /// <summary>Units that left the factories this turn, before transmission losses.</summary>
    public Dictionary<string, double> ProducedThisTurn { get; } = [];

    /// <summary>
    /// Share of the rations and fuel the treasury could not pay for this turn. Sustainment
    /// is a charge and not a decision, so this is only ever non-zero when the cash has run
    /// out — which makes it one of the sharpest signals on the board.
    /// </summary>
    public double SustainmentShortfall { get; set; }

    /// <summary>Sustainable combat power: the scarcest resource, never the sum.</summary>
    public double SustainableCombatPower { get; set; }

    /// <summary>Which flow is currently the binding constraint. The single most useful readout.</summary>
    public string? BottleneckCode { get; set; }

    /// <summary>Force regenerated over force consumed. Below one for too long means collapse.</summary>
    public double ForceGenerationRatio { get; set; } = 1d;

    public bool HasCollapsed { get; set; }

    public string? CollapseReason { get; set; }

    public double GetDelivered(ResourceKind kind)
    {
        return DeliveredThisTurn.TryGetValue(kind.Code, out double value) ? value : 0d;
    }

    public void SetDelivered(ResourceKind kind, double units)
    {
        DeliveredThisTurn[kind.Code] = units;
    }

    public double GetCoverage(string code)
    {
        return CoverageThisTurn.TryGetValue(code, out double value) ? value : 1d;
    }

    /// <summary>Money actually converted into materiel, after leakage and interdiction.</summary>
    public double TransmissionRate
    {
        get
        {
            return Math.Clamp(
                (1d - Politics.BudgetLeakRate) * Politics.LogisticsIntegrity,
                0.05d,
                1d);
        }
    }
}
