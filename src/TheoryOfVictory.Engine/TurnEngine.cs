using TheoryOfVictory.Core;
using TheoryOfVictory.Engine.Phases;

namespace TheoryOfVictory.Engine;

/// <summary>Runs the ten phases of a three-month turn and freezes the result.</summary>
public sealed class TurnEngine
{
    /// <summary>The engine's internal unit is the thousand men; the board's unit is the man.</summary>
    private const double ThousandsToMen = 1000d;

    private readonly List<ITurnPhase> _phases;

    /// <summary>
    /// Turns an internal count of thousands into men, rounded to the nearest thousand.
    ///
    /// The rounding is the point, not a detail. Exposing 671 412 men would claim a precision
    /// nobody has: the underlying estimates carry ± 15 %, so the last three digits would be
    /// pure invention dressed as a census. The thousand is the finest grain any of the sources
    /// behind this model actually support.
    /// </summary>
    private static double Men(double thousands)
    {
        return Math.Round(thousands, MidpointRounding.AwayFromZero) * ThousandsToMen;
    }

    public TurnEngine()
    {
        _phases =
        [
            new EnergyPhase(),
            new RevenuePhase(),
            new AllocationPhase(),
            new ProductionPhase(),
            new LogisticsPhase(),
            new EventPhase(),
            new DeepStrikePhase(),
            new FrontPhase(),
            new AttritionPhase(),
            new ControlPhase(),
        ];
    }

    public IReadOnlyList<ITurnPhase> Phases
    {
        get { return _phases; }
    }

    public TurnSnapshot ExecuteTurn(GameState state, Scenario scenario, Doctrine invaderDoctrine, Doctrine defenderDoctrine)
    {
        TurnContext context = new()
        {
            State = state,
            Scenario = scenario,
            InvaderDoctrine = invaderDoctrine,
            DefenderDoctrine = defenderDoctrine,
        };

        CaptureOpeningPosition(context);

        foreach (ITurnPhase phase in _phases)
        {
            phase.Execute(context);
        }

        // Not an eleventh phase: it decides nothing, it reads what the ten just did.
        PressureAnalyser.Read(context);

        return Freeze(context);
    }

    /// <summary>A slope needs two points: this is the first one, taken before anything runs.</summary>
    private static void CaptureOpeningPosition(TurnContext context)
    {
        foreach (Side side in Side.All)
        {
            Belligerent belligerent = context.State.Get(side);

            Dictionary<string, double> stocks = [];
            foreach (ResourceKind kind in ResourceKind.All)
            {
                stocks[kind.Code] = belligerent.Stock.GetActual(kind);
            }

            context.OpeningStocks[side.Code] = stocks;
            context.OpeningGenerationRatio[side.Code] = belligerent.ForceGenerationRatio;
        }
    }

    private static TurnSnapshot Freeze(TurnContext context)
    {
        GameState state = context.State;

        // Average advance across the line, weighted by sector width: a front, not a sum.
        double weighted = 0d;
        double width = 0d;
        double squareKm = 0d;
        foreach (FrontSector sector in state.Sectors)
        {
            weighted += sector.HexesGained * sector.Width;
            width += sector.Width;

            // One hex is 10 km deep; a sector is Width hexes wide.
            squareKm += sector.HexesGained * 10d * sector.Width * 10d;
        }

        double totalHexes = width <= 0d ? 0d : weighted / width;

        PressureReading? invaderPressure = FindReading(context, Side.Invader);
        PressureReading? defenderPressure = FindReading(context, Side.Defender);

        List<PressureAlert> alerts = [];
        alerts.AddRange(invaderPressure?.Alerts ?? []);
        alerts.AddRange(defenderPressure?.Alerts ?? []);
        alerts.Sort((left, right) => right.Level.CompareTo(left.Level));

        return new TurnSnapshot
        {
            Turn = state.Turn,
            Year = state.Year,
            Season = state.Season,
            OilPrice = state.OilPrice,
            Invader = Capture(state.Invader, invaderPressure),
            Defender = Capture(state.DefenderSide, defenderPressure),
            Headline = Headline(state, alerts),
            Alerts = alerts,
            Sectors = [.. context.SectorResolutions],
            CardsPlayed = [.. context.CardsPlayed],
            Narrative = [.. context.Narrative],
            InvaderStrike = context.InvaderStrike,
            DefenderStrike = context.DefenderStrike,
            TotalHexesGained = totalHexes,
            SquareKilometresGained = squareKm,
            Outcome = state.Outcome,
        };
    }

    private static PressureReading? FindReading(TurnContext context, Side side)
    {
        foreach (PressureReading reading in context.Readings)
        {
            if (reading.SideCode == side.Code)
            {
                return reading;
            }
        }

        return null;
    }

    /// <summary>
    /// The sentence the turn is about. An outcome speaks for itself; otherwise the
    /// sharpest pressure on the board is what the player needs to hear before deciding
    /// whether the next turn matters.
    /// </summary>
    private static string Headline(GameState state, List<PressureAlert> alerts)
    {
        if (state.Outcome is not null && state.Outcome.Code != "frozen_front")
        {
            return state.Outcome.Title;
        }

        if (alerts.Count == 0)
        {
            return "Les deux camps remplacent ce qu'ils consomment. Rien ne bouge, et c'est le sujet.";
        }

        PressureAlert sharpest = alerts[0];
        return sharpest.Title;
    }

    private static SideSnapshot Capture(Belligerent belligerent, PressureReading? pressure)
    {
        Dictionary<string, double> stocks = [];
        foreach (ResourceKind kind in ResourceKind.All)
        {
            stocks[kind.Code] = belligerent.Stock.GetActual(kind);
        }

        Dictionary<string, double> coverage = new(belligerent.CoverageThisTurn);
        Dictionary<string, double> need = new(belligerent.NeedThisTurn);
        Dictionary<string, double> delivered = new(belligerent.DeliveredThisTurn);
        Dictionary<string, double> produced = new(belligerent.ProducedThisTurn);
        Dictionary<string, double> allocation = new(belligerent.AllocationThisTurn);

        Dictionary<string, double> capacity = [];
        foreach (ResourceKind kind in ResourceKind.All)
        {
            capacity[kind.Code] = belligerent.Industry.GetCapacityPerTurn(kind);
        }

        // Only a consumed flow can be a bottleneck: the men are the size of the barrel,
        // never one of its staves.
        string? bottleneckName = belligerent.BottleneckCode is null
            ? null
            : ResourceKind.FromCode(belligerent.BottleneckCode).DisplayName;

        return new SideSnapshot
        {
            SideCode = belligerent.Side.Code,
            Name = belligerent.Name,
            HeadlineGdp = belligerent.Economy.HeadlineGdpBillions,
            ProductiveCapacity = belligerent.Economy.ProductiveCapacityBillions,
            Treasury = belligerent.Economy.TreasuryBillions,
            Reserves = belligerent.Economy.ReservesBillions,
            MilitarySpend = belligerent.Economy.LastTurnMilitarySpendBillions,
            OilRevenue = belligerent.Economy.LastTurnOilRevenueBillions,
            ForeignSupport = belligerent.Foreign.Mode == SupportMode.Granted
                ? belligerent.Foreign.EffectiveGrantBillions
                : belligerent.Foreign.Dependency * 100d,
            // The engine counts in thousands; nothing leaves it in thousands. A board that
            // prints "560" teaches nothing — "560 000 hommes" is immediately a war.
            MenUnderArms = Men(belligerent.Manpower.TotalUnderArms),
            MenInTheatre = Men(belligerent.Manpower.AtFront),
            MenInContact = Men(belligerent.Manpower.InContact),
            MenInTraining = Men(belligerent.Manpower.InTraining),
            MenMobilisable = Men(belligerent.Manpower.MobilisablePool),
            MenLost = Men(belligerent.Manpower.CumulativeLosses),
            MenEstablishment = Men(belligerent.Manpower.TargetForceSize),
            ManningRatio = belligerent.Manpower.ManningRatio,
            CohesionFactor = belligerent.Manpower.CohesionFactor,
            MaterialCoverage = belligerent.MaterialCoverage,
            PayRatio = belligerent.Manpower.PayRatio,
            CombatPower = Men(belligerent.SustainableCombatPower),
            ForceGenerationRatio = belligerent.ForceGenerationRatio,
            BottleneckCode = belligerent.BottleneckCode,
            BottleneckName = bottleneckName,
            Coverage = coverage,
            Stocks = stocks,
            Need = need,
            Delivered = delivered,
            Produced = produced,
            Capacity = capacity,
            Allocation = allocation,
            FiscalRevenue = belligerent.Economy.HeadlineGdpBillions * belligerent.Economy.FiscalCaptureRate,
            InKindAid = belligerent.Foreign.Mode == SupportMode.Granted
                ? belligerent.Foreign.EffectiveGrantBillions * belligerent.Foreign.InKindShare
                : 0d,
            GridAvailableGw = belligerent.Grid.AvailableCapacityGw,
            GridDemandGw = belligerent.Grid.BaseDemandGw,
            GridShortfall = belligerent.Grid.ShortfallRatio(Season.Winter),
            PermanentGridDamage = belligerent.Grid.PermanentDamageGw,
            Morale = belligerent.Politics.Morale,
            EliteCohesion = belligerent.Politics.EliteCohesion,
            PopularDiscontent = belligerent.Politics.PopularDiscontent,
            RegimeStress = belligerent.Politics.RegimeStress,
            Corruption = belligerent.Politics.Corruption,
            TransmissionRate = belligerent.TransmissionRate,
            LogisticsIntegrity = belligerent.Politics.LogisticsIntegrity,
            RefiningIntegrity = belligerent.Economy.RefiningIntegrity,
            PoliticalCapital = belligerent.Politics.PoliticalCapital,
            PoliticalCapitalOverdraft = belligerent.Politics.PoliticalCapitalOverdraft,
            ExternalWill = belligerent.Politics.ExternalWill,
            SanctionsPrice = belligerent.Sanctions.PriceSeverity,
            SanctionsFriction = belligerent.Sanctions.FrictionSeverity,
            SanctionsComponent = belligerent.Sanctions.ComponentSeverity,
            ProductionCeiling = belligerent.Sanctions.ProductionCeilingMultiplier,
            TacticalDroneEdge = belligerent.Innovation.TacticalDroneEdge,
            StrikeEdge = belligerent.Innovation.StrikeEdge,
            CounterDroneEdge = belligerent.Innovation.CounterDroneEdge,
            Dependency = belligerent.Foreign.Dependency,
            HasCollapsed = belligerent.HasCollapsed,
            ReserveDraw = belligerent.Economy.LastTurnReserveDrawBillions,
            OrdinaryWarFunding = belligerent.Economy.OrdinaryWarFundingBillions,
            WarFundable = belligerent.Economy.WarFundableBillions,
            WarBudgetCeiling = belligerent.Economy.HeadlineGdpBillions * belligerent.Economy.WarBudgetCeilingShare,
            FundingGap = belligerent.Economy.FundingGap,
            Pressure = pressure,
        };
    }
}
