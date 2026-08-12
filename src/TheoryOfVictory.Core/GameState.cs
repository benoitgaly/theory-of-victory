namespace TheoryOfVictory.Core;

public sealed class GameState
{
    public required Belligerent Invader { get; init; }

    public required Belligerent DefenderSide { get; init; }

    public required List<FrontSector> Sectors { get; init; }

    public int Turn { get; set; }

    public int Year { get; set; }

    public Season Season { get; set; }

    /// <summary>Brent in dollars. One variable, four channels, all pushing the same way.</summary>
    public double OilPrice { get; set; }

    /// <summary>Lasting shift applied by cards on top of the calendar, never reset.</summary>
    public double OilPriceShift { get; set; }

    public List<PendingEffect> PendingEffects { get; } = [];

    public List<TurnSnapshot> History { get; } = [];

    public GameOutcome? Outcome { get; set; }

    public Belligerent Get(Side side)
    {
        return side == Side.Invader ? Invader : DefenderSide;
    }

    public string Label
    {
        get { return $"T{Turn} — {Season.ToFrench()} {Year}"; }
    }
}

public sealed class GameOutcome
{
    public required string Code { get; init; }

    public required string Title { get; init; }

    public required string Explanation { get; init; }

    public string? WinnerSideCode { get; init; }

    public int Turn { get; init; }
}

/// <summary>Everything the board display needs for one turn, frozen at end of turn.</summary>
public sealed class TurnSnapshot
{
    public required int Turn { get; init; }

    public required int Year { get; init; }

    public required Season Season { get; init; }

    public required double OilPrice { get; init; }

    public required SideSnapshot Invader { get; init; }

    public required SideSnapshot Defender { get; init; }

    public List<SectorResolution> Sectors { get; init; } = [];

    public List<PlayedCard> CardsPlayed { get; init; } = [];

    public List<string> Narrative { get; init; } = [];

    public StrikeResolution? InvaderStrike { get; init; }

    public StrikeResolution? DefenderStrike { get; init; }

    /// <summary>Average advance across the line in hexes, weighted by sector width.</summary>
    public double TotalHexesGained { get; init; }

    /// <summary>Cumulative ground taken since the start, the figure the reports quote.</summary>
    public double SquareKilometresGained { get; init; }

    public GameOutcome? Outcome { get; init; }
}

/// <summary>One side's readable position at the end of a turn.</summary>
public sealed class SideSnapshot
{
    public required string SideCode { get; init; }

    public required string Name { get; init; }

    public double HeadlineGdp { get; init; }

    public double ProductiveCapacity { get; init; }

    public double Treasury { get; init; }

    public double Reserves { get; init; }

    public double MilitarySpend { get; init; }

    public double OilRevenue { get; init; }

    public double ForeignSupport { get; init; }

    public double SoldiersAtFront { get; init; }

    public double SoldiersInTraining { get; init; }

    public double MobilisablePool { get; init; }

    public double CumulativeLosses { get; init; }

    public double CombatPower { get; init; }

    public double ForceGenerationRatio { get; init; }

    public string? BottleneckCode { get; init; }

    public string? BottleneckName { get; init; }

    public Dictionary<string, double> Coverage { get; init; } = [];

    public Dictionary<string, double> Stocks { get; init; } = [];

    /// <summary>What the front required, per flow — the denominator of every coverage.</summary>
    public Dictionary<string, double> Need { get; init; } = [];

    public Dictionary<string, double> Delivered { get; init; } = [];

    public Dictionary<string, double> Produced { get; init; } = [];

    public Dictionary<string, double> Capacity { get; init; } = [];

    /// <summary>Budget per spending line, for the economic flow view.</summary>
    public Dictionary<string, double> Allocation { get; init; } = [];

    public double FiscalRevenue { get; init; }

    public double InKindAid { get; init; }

    public double TargetForceSize { get; init; }

    public double GridAvailableGw { get; init; }

    public double GridDemandGw { get; init; }

    public double GridShortfall { get; init; }

    public double PermanentGridDamage { get; init; }

    public double Morale { get; init; }

    public double EliteCohesion { get; init; }

    public double PopularDiscontent { get; init; }

    public double RegimeStress { get; init; }

    public double Corruption { get; init; }

    public double TransmissionRate { get; init; }

    public double LogisticsIntegrity { get; init; }

    public double RefiningIntegrity { get; init; }

    public double PoliticalCapital { get; init; }

    public double ExternalWill { get; init; }

    public double SanctionsPrice { get; init; }

    public double SanctionsFriction { get; init; }

    public double SanctionsComponent { get; init; }

    public double ProductionCeiling { get; init; }

    public double TacticalDroneEdge { get; init; }

    public double StrikeEdge { get; init; }

    public double CounterDroneEdge { get; init; }

    public double Dependency { get; init; }

    public bool HasCollapsed { get; init; }
}
