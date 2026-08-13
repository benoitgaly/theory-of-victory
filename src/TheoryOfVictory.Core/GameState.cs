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

    /// <summary>
    /// What each side ordered on the front this quarter. The four verbs the board names —
    /// hold, press, dig in, strike deep — are read from here and from the budget, never
    /// guessed from the result.
    /// </summary>
    public List<SectorOrders> Orders { get; init; } = [];

    public List<PlayedCard> CardsPlayed { get; init; } = [];

    public List<string> Narrative { get; init; } = [];

    public StrikeResolution? InvaderStrike { get; init; }

    public StrikeResolution? DefenderStrike { get; init; }

    /// <summary>Average advance across the line in hexes, weighted by sector width.</summary>
    public double TotalHexesGained { get; init; }

    /// <summary>Cumulative ground taken since the start, the figure the reports quote.</summary>
    public double SquareKilometresGained { get; init; }

    /// <summary>
    /// The single sentence this turn is about, picked from the sharpest pressure on the
    /// board. What a turn-by-turn replay needs to give a reason to press next.
    /// </summary>
    public string Headline { get; init; } = string.Empty;

    /// <summary>Both sides' alerts, sharpest first. The board's tension strip.</summary>
    public List<PressureAlert> Alerts { get; init; } = [];

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

    // Every count below is in MEN, not in thousands: the engine works in thousands, the board
    // reads people. 560 000 means 560 000 soldiers, and nothing on the page has to convert.

    /// <summary>Men in uniform, everything included. The figure leaders quote, and the vaguest.</summary>
    public double MenUnderArms { get; init; }

    /// <summary>Men in the grouping committed to the theatre. This is what consumes.</summary>
    public double MenInTheatre { get; init; }

    /// <summary>
    /// Men in the combat units on the line of contact. This is what fights, what holds ground,
    /// and what both armies ran out of. Never equal to the theatre grouping.
    /// </summary>
    public double MenInContact { get; init; }

    /// <summary>Men in training, one quarter from the line.</summary>
    public double MenInTraining { get; init; }

    /// <summary>Men the state could still put under arms — the demographic ceiling, in men.</summary>
    public double MenMobilisable { get; init; }

    /// <summary>Men lost since the first turn, killed and permanently out of the line alike.</summary>
    public double MenLost { get; init; }

    /// <summary>Men the command intends to hold in the theatre: the establishment, in men.</summary>
    public double MenEstablishment { get; init; }

    /// <summary>Men present over establishment. A structural reading, never a coverage.</summary>
    public double ManningRatio { get; init; }

    /// <summary>What fighting below establishment costs, on top of the missing men themselves.</summary>
    public double CohesionFactor { get; init; }

    /// <summary>Level of the shortest stave: the smallest coverage among the three consumed flows.</summary>
    public double MaterialCoverage { get; init; }

    /// <summary>Share of the men present the treasury can still pay for. Below one, the line empties.</summary>
    public double PayRatio { get; init; }

    /// <summary>
    /// Combat power, in MEN of fully supplied infantry equivalent — the same unit as the counts
    /// above, so it can be read against them. It is the men in contact, cut down by the
    /// scarcest flow, by training quality and by cohesion.
    /// </summary>
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

    /// <summary>Cumulated political capital the scripted calendar spent beyond what was held.</summary>
    public double PoliticalCapitalOverdraft { get; init; }

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

    /// <summary>Sovereign reserves actually liquidated this turn to hold the war effort up.</summary>
    public double ReserveDraw { get; init; }

    /// <summary>What this quarter's own revenue funds, before touching the sovereign fund.</summary>
    public double OrdinaryWarFunding { get; init; }

    /// <summary>What the war effort could fund this turn, reserves included.</summary>
    public double WarFundable { get; init; }

    /// <summary>What it wanted to spend. The gap between the two is where regimes die.</summary>
    public double WarBudgetCeiling { get; init; }

    /// <summary>Share of the intended war effort this turn's revenue could not cover.</summary>
    public double FundingGap { get; init; }

    /// <summary>Everything forward-looking: countdowns, depot horizons, threat index, alerts.</summary>
    public PressureReading? Pressure { get; init; }

    /// <summary>
    /// The seven posts of war capital, in the band's own order, every one of them in billions
    /// of dollars. What this side still holds to make war with, against what the front shows.
    /// </summary>
    public List<CapitalPost> Capital { get; init; } = [];

    /// <summary>
    /// What this side owns, in billions: the sovereign fund, plus every production valued at
    /// five years of itself. The oil bill of an importer comes off it as a liability.
    /// </summary>
    public double CapitalStock { get; init; }

    /// <summary>
    /// A year of what this side does not own — the aid it is given, the margin it can still
    /// spend holding power — in billions. Never added to <see cref="CapitalStock"/>: an asset
    /// and an income are not the same object, and one figure covering both is the arithmetic of
    /// a wartime communiqué. The ratio between the two is the question the band asks: a side
    /// whose war runs on the flow is a side that can be switched off.
    /// </summary>
    public double CapitalFlow { get; init; }

    /// <summary>
    /// The seven posts in one figure, base 100 at the first quarter, as a geometric mean
    /// floored at 15 points a post. Not a minimum — a treasury at zero is survived for a few
    /// quarters and a dead grid is worked around for a while — and not a sum either, which
    /// would let 310 Md of reserves hide a grid in ruins. It exists for one purpose: to be
    /// drawn against combat power, and to show the front living off the capital.
    /// </summary>
    public double CapitalIndex { get; init; } = 100d;

    /// <summary>The quarter's sharpest destruction, followed downstream. Null on a quiet quarter.</summary>
    public CapitalChain? Chain { get; init; }
}
