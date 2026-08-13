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

    /// <summary>What the civilians live on. Never reaches the front, and holds up consent.</summary>
    public CivilianIndustry Civilian { get; init; } = new();

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

    /// <summary>
    /// What was consumed away from the front — interceptors fired at incoming waves, strike
    /// vectors launched. It never enters a coverage, only the sizing of the depots that
    /// hold it: a magazine is sized on what it fires, not on what the factory makes.
    /// </summary>
    public Dictionary<string, double> BurntThisTurn { get; } = [];

    /// <summary>Budget put on each spending line this turn, for the economic view.</summary>
    public Dictionary<string, double> AllocationThisTurn { get; } = [];

    /// <summary>
    /// Each capital post as it stood on the first quarter of this side's own war. Every index
    /// on the band is read against it, and never against the other camp: Russia holds 310 Md
    /// of reserves against 29 for Ukraine, and putting those two masses on one scale would
    /// only say, falsely, that the game was over before it began.
    /// </summary>
    public Dictionary<string, double> CapitalBaseline { get; } = [];

    /// <summary>Units that left the factories this turn, before transmission losses.</summary>
    public Dictionary<string, double> ProducedThisTurn { get; } = [];

    /// <summary>
    /// Share of the rations and fuel the treasury could not pay for this turn. Sustainment
    /// is a charge and not a decision, so this is only ever non-zero when the cash has run
    /// out — which makes it one of the sharpest signals on the board.
    /// </summary>
    public double SustainmentShortfall { get; set; }

    /// <summary>
    /// Units of aid the depots could not take this turn. Not waste to be hidden: it is the
    /// measure of a donor shipping past what the receiver can hold, and it says the aid
    /// ceiling has stopped being the binding constraint.
    /// </summary>
    public double AidBeyondDepotCapacity { get; set; }

    /// <summary>Sustainable combat power: men in the line times the scarcest flow, never a sum.</summary>
    public double SustainableCombatPower { get; set; }

    /// <summary>
    /// Level of the shortest stave: the smallest coverage among the three consumed flows.
    /// It is what caps the whole force, whatever its size.
    /// </summary>
    public double MaterialCoverage { get; set; } = 1d;

    /// <summary>Which consumed flow is currently the binding constraint. The single most useful readout.</summary>
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

    /// <summary>
    /// How full a depot is allowed to get, in units. THE single definition — every path that
    /// can fill a depot goes through it, whether the units were ordered from a domestic
    /// factory or handed over by a donor. There used to be two paths and only one ceiling:
    /// aid in kind was added straight to the stock, eleven times the national capacity every
    /// quarter, and the interceptor pile grew to twenty-three times the ceiling the game had
    /// set itself. A wall like that makes saturation arithmetically impossible, which killed
    /// the mechanism the whole deep-strike phase exists to show.
    ///
    /// The ceiling is measured against whichever is larger, what the factories make or what
    /// the front burns: an army may hold a few quarters of its own consumption even when it
    /// produces none of it, and it does not hold what it can neither use nor replace.
    /// </summary>
    public double DepotCeiling(ResourceKind kind)
    {
        double capacity = Industry.GetCapacityPerTurn(kind);

        // What the front asked for, or what the air defence actually fired — an interceptor
        // is burnt in the deep-strike phase and never appears in a front requirement, so a
        // ceiling reading only the front would size the magazines on the factory alone and
        // leave the sky open.
        double burnt = Math.Max(
            NeedThisTurn.GetValueOrDefault(kind.Code),
            BurntThisTurn.GetValueOrDefault(kind.Code));

        if (burnt <= 0d)
        {
            // Opening turn: revenue is collected before the front has consumed anything, so
            // no consumption is known yet. The only statement anyone has made about how much
            // this army holds is the depot the scenario handed it — a ceiling below that
            // would declare the starting stock illegal and refuse every delivery of the
            // first quarter.
            return Math.Max(capacity * Industry.DepotQuartersHeld, Stock.GetActual(kind));
        }

        return Math.Max(capacity, burnt) * Industry.DepotQuartersHeld;
    }

    /// <summary>
    /// Adds what the depot can still take and returns what it could not. A donor who ships
    /// more than the receiver can hold is a real event, not an anomaly: the surplus sits in
    /// Poland, ages, or is never delivered at all.
    /// </summary>
    public double FillDepot(ResourceKind kind, double units)
    {
        if (units <= 0d)
        {
            return 0d;
        }

        double room = Math.Max(0d, DepotCeiling(kind) - Stock.GetActual(kind));
        double accepted = Math.Min(units, room);
        if (accepted > 0d)
        {
            Stock.Add(kind, accepted);
        }

        return units - accepted;
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
