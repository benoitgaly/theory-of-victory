using TheoryOfVictory.Core;
using TheoryOfVictory.Engine.Phases;

namespace TheoryOfVictory.Engine;

/// <summary>Runs the ten phases of a three-month turn and freezes the result.</summary>
public sealed class TurnEngine
{
    private readonly List<ITurnPhase> _phases;

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

        foreach (ITurnPhase phase in _phases)
        {
            phase.Execute(context);
        }

        return Freeze(context);
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

        return new TurnSnapshot
        {
            Turn = state.Turn,
            Year = state.Year,
            Season = state.Season,
            OilPrice = state.OilPrice,
            Invader = Capture(state.Invader),
            Defender = Capture(state.DefenderSide),
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

    private static SideSnapshot Capture(Belligerent belligerent)
    {
        Dictionary<string, double> stocks = [];
        foreach (ResourceKind kind in ResourceKind.All)
        {
            stocks[kind.Code] = belligerent.Stock.GetActual(kind);
        }

        Dictionary<string, double> coverage = [];
        foreach (KeyValuePair<string, double> entry in belligerent.CoverageThisTurn)
        {
            coverage[entry.Key] = entry.Value;
        }

        string? bottleneckName = belligerent.BottleneckCode switch
        {
            null => null,
            "infantry" => "Soldats",
            _ => ResourceKind.FromCode(belligerent.BottleneckCode).DisplayName,
        };

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
            SoldiersAtFront = belligerent.Manpower.AtFront,
            SoldiersInTraining = belligerent.Manpower.InTraining,
            MobilisablePool = belligerent.Manpower.MobilisablePool,
            CumulativeLosses = belligerent.Manpower.CumulativeLosses,
            CombatPower = belligerent.SustainableCombatPower,
            ForceGenerationRatio = belligerent.ForceGenerationRatio,
            BottleneckCode = belligerent.BottleneckCode,
            BottleneckName = bottleneckName,
            Coverage = coverage,
            Stocks = stocks,
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
        };
    }
}
