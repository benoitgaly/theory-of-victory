using TheoryOfVictory.Core;
using TheoryOfVictory.Core.Localization;
using TheoryOfVictory.Engine.Phases;

namespace TheoryOfVictory.Engine;

/// <summary>
/// Reads the state once the turn has run and says what is about to break, and when.
/// It changes nothing: every figure here is a projection of the turn just played,
/// which is exactly what makes it honest — the model has to earn its own warnings.
///
/// The design signature is that effects are thresholds, not slopes. A threshold the
/// player only discovers by crossing it teaches nothing; this is what puts the slope
/// back on screen without putting it back in the rules.
/// </summary>
public static class PressureAnalyser
{
    /// <summary>Below this many quarters of depot left, a flow is worth an alert.</summary>
    private const double DepotHorizonQuarters = 3d;

    /// <summary>Below this many quarters of sovereign fund left, the war is on a clock.</summary>
    private const double TreasuryHorizonQuarters = 6d;

    /// <summary>Regime stress at which the apparatus is worth watching, well before it breaks.</summary>
    private const double RegimeWatchStress = 40d;

    private const double RegimeCollapseStress = 58d;

    public static void Read(TurnContext context)
    {
        foreach (Side side in Side.All)
        {
            context.Readings.Add(ReadSide(context, context.State.Get(side)));
        }
    }

    private static PressureReading ReadSide(TurnContext context, Belligerent belligerent)
    {
        List<PressureAlert> alerts = [];
        Dictionary<string, double> quartersLeft = [];

        ReadDepots(context, belligerent, quartersLeft, alerts);
        ReadTreasury(belligerent, alerts);
        ReadGeneration(belligerent, alerts);
        ReadRegime(belligerent, alerts);
        ReadWinter(context, belligerent, alerts);
        ReadRedQueen(belligerent, alerts);
        ReadExternalWill(belligerent, alerts);

        alerts.Sort(CompareSeverity);

        double previousRatio = TurnContext.Read(context.OpeningGenerationRatio, belligerent.Side);

        return new PressureReading
        {
            SideCode = belligerent.Side.Code,
            StockQuartersLeft = quartersLeft,
            ReserveQuartersLeft = belligerent.Economy.ReserveQuartersLeft,
            TurnsBelowThreshold = belligerent.Politics.TurnsBelowCollapseThreshold,
            TurnsToCollapse = belligerent.Politics.TurnsBelowCollapseThreshold <= 0
                ? -1
                : Math.Max(0, ControlPhase.TurnsBeforeCollapse - belligerent.Politics.TurnsBelowCollapseThreshold),
            GenerationTrend = belligerent.ForceGenerationRatio - previousRatio,
            ThreatIndex = ThreatIndex(context, belligerent, quartersLeft),
            Alerts = alerts,
        };
    }

    /// <summary>
    /// A depot is read in quarters of cover, and only alerts on the way down. The pile
    /// that matters is the one that is produced or given — rations and fuel are bought on
    /// the market every turn, so their signal is the treasury, handled separately below.
    ///
    /// This is the alert the whole abandonment run rests on: the turn the aid stops,
    /// nothing happens, because the depot still covers. The player should be told exactly
    /// how many turns that "nothing" is going to last.
    /// </summary>
    private static void ReadDepots(
        TurnContext context,
        Belligerent belligerent,
        Dictionary<string, double> quartersLeft,
        List<PressureAlert> alerts)
    {
        context.OpeningStocks.TryGetValue(belligerent.Side.Code, out Dictionary<string, double>? opening);

        foreach (ResourceKind kind in ResourceKind.FrontFlows)
        {
            double stock = belligerent.Stock.GetActual(kind);
            double need = belligerent.NeedThisTurn.GetValueOrDefault(kind.Code);
            double drawPerTurn = need / belligerent.TransmissionRate;

            if (drawPerTurn <= 0.01d)
            {
                quartersLeft[kind.Code] = double.PositiveInfinity;
                continue;
            }

            double quarters = stock / drawPerTurn;
            quartersLeft[kind.Code] = quarters;

            // Market-bought sustainment refills every turn: its risk is cash, not stock.
            if (kind != ResourceKind.Weapons)
            {
                continue;
            }

            bool falling = opening is not null && opening.GetValueOrDefault(kind.Code) - stock > 0.01d;
            if (!falling || quarters > DepotHorizonQuarters)
            {
                continue;
            }

            AlertLevel level = quarters <= 1d
                ? AlertLevel.Critical
                : quarters <= 2d ? AlertLevel.Alert : AlertLevel.Watch;

            alerts.Add(new PressureAlert
            {
                Code = $"depot_{kind.Code}",
                SideCode = belligerent.Side.Code,
                Level = level,
                Title = LocalizedText.Of(
                    TextCodes.Alert.DepotTitle, kind.Label, LocalizedText.Number(quarters, "F1")),
                Detail = LocalizedText.Of(
                    TextCodes.Alert.DepotDetail, belligerent.NameInProse, LocalizedText.Number(quarters, "F1")),
                TurnsAhead = quarters,
                Value = stock,
                Threshold = 0d,
            });
        }

        if (belligerent.SustainmentShortfall > 0.05d)
        {
            alerts.Add(new PressureAlert
            {
                Code = "sustainment",
                SideCode = belligerent.Side.Code,
                Level = AlertLevel.Critical,
                Title = LocalizedText.Of(
                    TextCodes.Alert.SustainmentTitle,
                    LocalizedText.Number(belligerent.SustainmentShortfall * 100d, "F0")),
                Detail = LocalizedText.Of(TextCodes.Alert.SustainmentDetail, belligerent.NameOpeningSentence),
                Value = belligerent.SustainmentShortfall,
                Threshold = 0d,
            });
        }
    }

    /// <summary>The barrel decides through this: a fund being liquidated has a date of death.</summary>
    private static void ReadTreasury(Belligerent belligerent, List<PressureAlert> alerts)
    {
        Economy economy = belligerent.Economy;
        double quarters = economy.ReserveQuartersLeft;

        if (economy.LastTurnReserveDrawBillions > 0.01d && quarters <= TreasuryHorizonQuarters)
        {
            AlertLevel level = quarters <= 2d
                ? AlertLevel.Critical
                : quarters <= 4d ? AlertLevel.Alert : AlertLevel.Watch;

            alerts.Add(new PressureAlert
            {
                Code = "reserves",
                SideCode = belligerent.Side.Code,
                Level = level,
                Title = LocalizedText.Of(TextCodes.Alert.ReservesTitle, LocalizedText.Number(quarters, "F1")),
                Detail = LocalizedText.Of(
                    TextCodes.Alert.ReservesDetail,
                    belligerent.NameOpeningSentence,
                    LocalizedText.Number(economy.LastTurnReserveDrawBillions, "F1"),
                    LocalizedText.Number(economy.ReservesBillions, "F0")),
                TurnsAhead = quarters,
                Value = economy.ReservesBillions,
                Threshold = 0d,
            });
        }

        double gap = economy.FundingGap;
        if (gap > 0.1d)
        {
            alerts.Add(new PressureAlert
            {
                Code = "funding_gap",
                SideCode = belligerent.Side.Code,
                Level = gap > 0.4d ? AlertLevel.Critical : gap > 0.25d ? AlertLevel.Alert : AlertLevel.Watch,
                Title = LocalizedText.Of(
                    TextCodes.Alert.FundingGapTitle, LocalizedText.Number((1d - gap) * 100d, "F0")),
                Detail = LocalizedText.Of(
                    TextCodes.Alert.FundingGapDetail,
                    LocalizedText.Number(economy.WarFundableBillions, "F1"),
                    LocalizedText.Number(economy.HeadlineGdpBillions * economy.WarBudgetCeilingShare, "F1"),
                    belligerent.NameInProse),
                TurnsAhead = 1d,
                Value = economy.WarFundableBillions,
                Threshold = economy.HeadlineGdpBillions * economy.WarBudgetCeilingShare,
            });
        }
    }

    private static void ReadGeneration(Belligerent belligerent, List<PressureAlert> alerts)
    {
        double ratio = belligerent.ForceGenerationRatio;
        int below = belligerent.Politics.TurnsBelowCollapseThreshold;

        if (below > 0)
        {
            int left = Math.Max(0, ControlPhase.TurnsBeforeCollapse - below);
            alerts.Add(new PressureAlert
            {
                Code = "collapse_countdown",
                SideCode = belligerent.Side.Code,
                Level = AlertLevel.Critical,
                Title = left <= 0
                    ? LocalizedText.Of(TextCodes.Alert.CollapseNow)
                    : LocalizedText.Of(TextCodes.Alert.CollapseIn, left),
                Detail = LocalizedText.Of(
                    TextCodes.Alert.CollapseDetail,
                    belligerent.NameOpeningSentence,
                    LocalizedText.Number(ratio, "F2"),
                    LocalizedText.Number(ControlPhase.CollapseThreshold, "F2"),
                    below),
                TurnsAhead = left,
                Value = ratio,
                Threshold = ControlPhase.CollapseThreshold,
            });
            return;
        }

        if (ratio < ControlPhase.CollapseThreshold + 0.12d)
        {
            alerts.Add(new PressureAlert
            {
                Code = "generation_thin",
                SideCode = belligerent.Side.Code,
                Level = AlertLevel.Alert,
                Title = LocalizedText.Of(
                    TextCodes.Alert.GenerationTitle,
                    LocalizedText.Number(ratio, "F2"),
                    LocalizedText.Number(ControlPhase.CollapseThreshold, "F2")),
                Detail = LocalizedText.Of(TextCodes.Alert.GenerationDetail, belligerent.NameOpeningSentence),
                Value = ratio,
                Threshold = ControlPhase.CollapseThreshold,
            });
        }
    }

    /// <summary>
    /// Elite cohesion is invisible until the last moment by design. Invisible in the
    /// rules is right; invisible on the table is a cheat. The gauge is shown, the
    /// threshold is not moved.
    /// </summary>
    private static void ReadRegime(Belligerent belligerent, List<PressureAlert> alerts)
    {
        double stress = belligerent.Politics.RegimeStress;
        if (stress < RegimeWatchStress)
        {
            return;
        }

        bool authoritarian = belligerent.Politics.Regime == RegimeType.Authoritarian;

        alerts.Add(new PressureAlert
        {
            Code = "regime_stress",
            SideCode = belligerent.Side.Code,
            Level = stress >= RegimeCollapseStress - 4d
                ? AlertLevel.Critical
                : stress >= RegimeCollapseStress - 10d ? AlertLevel.Alert : AlertLevel.Watch,
            Title = LocalizedText.Of(
                authoritarian ? TextCodes.Alert.ApparatusStressTitle : TextCodes.Alert.WillStressTitle,
                LocalizedText.Number(stress, "F0"),
                LocalizedText.Number(RegimeCollapseStress, "F0")),
            Detail = authoritarian
                ? LocalizedText.Of(
                    TextCodes.Alert.ApparatusStressDetail,
                    LocalizedText.Number(belligerent.Politics.EliteCohesion, "F0"),
                    LocalizedText.Number(belligerent.Politics.LatentTension, "F0"))
                : LocalizedText.Of(
                    TextCodes.Alert.NegotiationDetail,
                    LocalizedText.Number(belligerent.Politics.Morale, "F0"),
                    LocalizedText.Number(belligerent.Politics.PopularDiscontent, "F0")),
            Value = stress,
            Threshold = RegimeCollapseStress,
        });
    }

    /// <summary>
    /// The seasonal alert, and the one that gives the game its annual rhythm: a strike
    /// campaign is prepared in autumn so that it bites in January.
    /// </summary>
    private static void ReadWinter(TurnContext context, Belligerent belligerent, List<PressureAlert> alerts)
    {
        Season next = (Season)(((int)context.State.Season + 1) % 4);
        if (next != Season.Winter)
        {
            return;
        }

        double winterShortfall = belligerent.Grid.ShortfallRatio(Season.Winter);
        if (winterShortfall <= 0.01d)
        {
            return;
        }

        double industrial = belligerent.Grid.IndustrialSupplyRatio(Season.Winter);

        alerts.Add(new PressureAlert
        {
            Code = "winter_shedding",
            SideCode = belligerent.Side.Code,
            Level = industrial < 1d ? AlertLevel.Critical : AlertLevel.Alert,
            Title = LocalizedText.Of(
                TextCodes.Alert.WinterSheddingTitle, LocalizedText.Number(winterShortfall * 100d, "F0")),
            Detail = industrial < 1d
                ? LocalizedText.Of(
                    TextCodes.Alert.WinterSheddingIndustrial,
                    belligerent.NameInProse,
                    LocalizedText.Number((1d - industrial) * 100d, "F0"))
                : LocalizedText.Of(TextCodes.Alert.WinterSheddingCivilian),
            TurnsAhead = 1d,
            Value = belligerent.Grid.AvailableCapacityGw,
            Threshold = belligerent.Grid.DemandGw(Season.Winter),
        });
    }

    /// <summary>An edge nobody feeds is an edge already half gone. Nothing acquired stays acquired.</summary>
    private static void ReadRedQueen(Belligerent belligerent, List<PressureAlert> alerts)
    {
        double edge = belligerent.Innovation.TacticalDroneEdge;
        if (edge < 0.25d)
        {
            return;
        }

        double decay = belligerent.Innovation.DecayPerTurn;
        double inTwo = edge * (1d - decay) * (1d - decay);
        if (inTwo > 0.2d)
        {
            return;
        }

        alerts.Add(new PressureAlert
        {
            Code = "red_queen",
            SideCode = belligerent.Side.Code,
            Level = AlertLevel.Watch,
            Title = LocalizedText.Of(
                TextCodes.Alert.EdgeDecayTitle,
                LocalizedText.Number(edge, "F2"),
                LocalizedText.Number(inTwo, "F2")),
            Detail = LocalizedText.Of(
                TextCodes.Alert.EdgeDecayDetail,
                LocalizedText.Number(decay * 100d, "F0"),
                belligerent.NameInProse),
            TurnsAhead = 2d,
            Value = edge,
            Threshold = 0.2d,
        });
    }

    /// <summary>The free flow is free and can stop in a day. That is its whole nature.</summary>
    private static void ReadExternalWill(Belligerent belligerent, List<PressureAlert> alerts)
    {
        if (belligerent.Foreign.Mode != SupportMode.Granted)
        {
            return;
        }

        double will = belligerent.Politics.ExternalWill;
        if (will > 45d)
        {
            return;
        }

        alerts.Add(new PressureAlert
        {
            Code = "external_will",
            SideCode = belligerent.Side.Code,
            Level = will <= 25d ? AlertLevel.Critical : AlertLevel.Alert,
            Title = LocalizedText.Of(TextCodes.Alert.ExternalWillTitle, LocalizedText.Number(will, "F0")),
            Detail = LocalizedText.Of(TextCodes.Alert.ExternalWillDetail, belligerent.NameOpeningSentence),
            Value = will,
            Threshold = 0d,
        });
    }

    /// <summary>
    /// One number for the whole position: the closest breaking point wins. Deliberately
    /// a maximum and not an average — being comfortable everywhere except on shells is
    /// the same as having no shells.
    /// </summary>
    private static double ThreatIndex(
        TurnContext context,
        Belligerent belligerent,
        Dictionary<string, double> quartersLeft)
    {
        double worst = 0d;

        // Generation: the whole distance from a healthy 1.10 down to the collapse threshold.
        double span = 1.1d - ControlPhase.CollapseThreshold;
        double generation = (1.1d - belligerent.ForceGenerationRatio) / span;
        worst = Math.Max(worst, generation * 70d);

        // Already counting down: the last three turns are the top of the scale.
        if (belligerent.Politics.TurnsBelowCollapseThreshold > 0)
        {
            double share = (double)belligerent.Politics.TurnsBelowCollapseThreshold / ControlPhase.TurnsBeforeCollapse;
            worst = Math.Max(worst, 70d + (share * 30d));
        }

        // The rear: regime stress read against its own threshold.
        worst = Math.Max(worst, belligerent.Politics.RegimeStress / RegimeCollapseStress * 100d);

        // The materiel depot, and only while it is emptying. A side living hand to mouth in
        // equilibrium is not in danger; one whose pile is going down is. Reading the level
        // instead of the slope pinned this gauge at maximum for the whole game and made it
        // say nothing at all.
        if (IsDraining(context, belligerent, ResourceKind.Weapons))
        {
            double quarters = quartersLeft.GetValueOrDefault(ResourceKind.Weapons.Code, double.PositiveInfinity);
            if (!double.IsPositiveInfinity(quarters))
            {
                worst = Math.Max(worst, Math.Clamp((4d - quarters) / 4d, 0d, 1d) * 85d);
            }
        }

        // The cash: an emptying sovereign fund is a countdown on everything above.
        double reserve = belligerent.Economy.ReserveQuartersLeft;
        if (!double.IsPositiveInfinity(reserve))
        {
            worst = Math.Max(worst, Math.Clamp((8d - reserve) / 8d, 0d, 1d) * 60d);
        }

        worst = Math.Max(worst, belligerent.Economy.FundingGap * 90d);

        return Math.Clamp(worst, 0d, 100d);
    }

    private static bool IsDraining(TurnContext context, Belligerent belligerent, ResourceKind kind)
    {
        if (!context.OpeningStocks.TryGetValue(belligerent.Side.Code, out Dictionary<string, double>? opening))
        {
            return false;
        }

        return opening.GetValueOrDefault(kind.Code) - belligerent.Stock.GetActual(kind) > 0.01d;
    }

    private static int CompareSeverity(PressureAlert left, PressureAlert right)
    {
        int byLevel = right.Level.CompareTo(left.Level);
        if (byLevel != 0)
        {
            return byLevel;
        }

        double leftAhead = left.TurnsAhead < 0d ? double.MaxValue : left.TurnsAhead;
        double rightAhead = right.TurnsAhead < 0d ? double.MaxValue : right.TurnsAhead;
        return leftAhead.CompareTo(rightAhead);
    }
}
