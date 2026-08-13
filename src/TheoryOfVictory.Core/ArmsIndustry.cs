namespace TheoryOfVictory.Core;

/// <summary>An order placed now that leaves the factory several turns later.</summary>
public sealed class ProductionOrder
{
    public required ResourceKind Kind { get; init; }

    public required double Units { get; init; }

    public required int TurnsRemaining { get; set; }
}

/// <summary>Capacity expansion decided now, usable only after the ramp-up delay.</summary>
public sealed class CapacityExpansion
{
    public required ResourceKind Kind { get; init; }

    public required double AddedUnitsPerTurn { get; init; }

    public required int TurnsRemaining { get; set; }
}

/// <summary>
/// A factory is not a tap: capacity is installed, ramps up slowly, and keeps costing
/// upkeep even when idle. The decision that wins 2029 is taken in 2026.
/// </summary>
public sealed class ArmsIndustry
{
    private readonly Dictionary<string, double> _capacityPerTurn = [];

    public List<ProductionOrder> Orders { get; } = [];

    public List<CapacityExpansion> Expansions { get; } = [];

    /// <summary>Imports bypass domestic capacity but depend on a political relationship.</summary>
    public Dictionary<string, double> ImportsPerTurn { get; } = [];

    /// <summary>Fraction of installed capacity paid every turn just to keep it alive.</summary>
    public double UpkeepRateOfCapacity { get; set; } = 0.04d;

    /// <summary>Cost of adding one unit per turn of permanent capacity.</summary>
    public double ExpansionCostMultiplier { get; set; } = 6d;

    /// <summary>
    /// How far a line can be pushed above its pre-war level. Machine tools, skilled hands
    /// and floor space all run out: Russia roughly tripled its shell output, it did not
    /// multiply it fiftyfold. Without this the capacity compounds to absurdity.
    /// </summary>
    public double ExpansionCeilingMultiple { get; set; } = 3.5d;

    private readonly Dictionary<string, double> _initialCapacity = [];

    public double GetCapacityPerTurn(ResourceKind kind)
    {
        return _capacityPerTurn.TryGetValue(kind.Code, out double value) ? value : 0d;
    }

    public double GetCapacityCeiling(ResourceKind kind)
    {
        double initial = _initialCapacity.TryGetValue(kind.Code, out double value) ? value : 0d;
        return initial * ExpansionCeilingMultiple;
    }

    public void SetCapacityPerTurn(ResourceKind kind, double unitsPerTurn)
    {
        double capacity = Math.Max(0d, unitsPerTurn);

        // The first value set is the pre-war line, and it is what the ceiling scales from.
        if (!_initialCapacity.ContainsKey(kind.Code))
        {
            _initialCapacity[kind.Code] = capacity;
        }

        double ceiling = GetCapacityCeiling(kind);
        _capacityPerTurn[kind.Code] = ceiling > 0d ? Math.Min(capacity, ceiling) : capacity;
    }

    public void AddCapacityPerTurn(ResourceKind kind, double unitsPerTurn)
    {
        SetCapacityPerTurn(kind, GetCapacityPerTurn(kind) + unitsPerTurn);
    }

    public double GetImportsPerTurn(ResourceKind kind)
    {
        return ImportsPerTurn.TryGetValue(kind.Code, out double value) ? value : 0d;
    }

    public void SetImportsPerTurn(ResourceKind kind, double unitsPerTurn)
    {
        ImportsPerTurn[kind.Code] = Math.Max(0d, unitsPerTurn);
    }

    /// <summary>Installed capacity in billions per turn, used to bill upkeep.</summary>
    public double TotalCapacityValueBillions()
    {
        double total = 0d;
        foreach (ResourceKind kind in ResourceKind.All)
        {
            total += GetCapacityPerTurn(kind) * kind.UnitCostMillions;
        }

        return total / 1000d;
    }
}
