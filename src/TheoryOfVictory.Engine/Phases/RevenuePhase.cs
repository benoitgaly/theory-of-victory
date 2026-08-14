using TheoryOfVictory.Core;
using TheoryOfVictory.Core.Localization;

namespace TheoryOfVictory.Engine.Phases;

/// <summary>
/// Phase 2. One variable, four channels: oil pays Russia, costs Ukraine,
/// tires the West, and buys social peace in Moscow.
/// </summary>
public sealed class RevenuePhase : ITurnPhase
{
    private const double DaysPerTurn = 91.25d;

    public string Name
    {
        get { return "Revenue"; }
    }

    public void Execute(TurnContext context)
    {
        GameState state = context.State;
        state.OilPrice = Math.Max(18d, context.Scenario.OilPriceAt(state.Turn) + state.OilPriceShift);

        foreach (Side side in Side.All)
        {
            Belligerent belligerent = state.Get(side);
            Economy economy = belligerent.Economy;

            double fiscal = economy.HeadlineGdpBillions * economy.FiscalCaptureRate;
            double oilRevenue = 0d;
            double oilCost = 0d;

            if (economy.OilExportCapacityMbd > 0d)
            {
                double netPrice = Math.Max(5d, state.OilPrice - belligerent.Sanctions.ExportDiscountPerBarrel);
                double volume = economy.OilExportCapacityMbd * economy.RefiningIntegrity;
                oilRevenue = volume * DaysPerTurn * netPrice / 1000d;
                oilRevenue *= 1d - belligerent.Sanctions.FrictionRate;
            }

            if (economy.OilImportMbd > 0d)
            {
                oilCost = economy.OilImportMbd * DaysPerTurn * state.OilPrice / 1000d;
            }

            double grant = belligerent.Foreign.EffectiveGrantBillions;

            // Most aid arrives as materiel, not cash: it bypasses the domestic capacity ceiling.
            // It does NOT bypass the depot ceiling — a receiving army holds what it can hold,
            // and the rest never reaches it. Aid is not all ammunition either: vehicles,
            // training and maintenance eat a third of it.
            double inKind = grant * belligerent.Foreign.InKindShare;
            double refused = 0d;
            if (inKind > 0d)
            {
                refused += DeliverInKind(belligerent, ResourceKind.Weapons, inKind * 0.52d);
                refused += DeliverInKind(belligerent, ResourceKind.CheapInterceptors, inKind * 0.12d);
                refused += DeliverInKind(belligerent, ResourceKind.HeavyInterceptors, inKind * 0.18d);
            }

            // What the depots could not take does not evaporate: a donor facing a receiver who
            // cannot absorb more materiel sends money instead, and the Kiel tracker shows that
            // very shift — the military share of Western support falls as the financial share
            // rises. So the refused value moves to the cash side of the same grant.
            belligerent.AidBeyondDepotCapacity = refused;
            double inKindDelivered = Math.Max(0d, inKind - refused);

            // The war chest is not the whole budget: ordinary spending has first claim.
            // Oil and aid are what actually fund the fighting, quarter by quarter.
            double ordinary =
                (fiscal * economy.MilitaryFiscalShare)
                + oilRevenue
                + (grant - inKindDelivered)
                - oilCost;

            // The sovereign fund plugs whatever the quarter's revenue leaves short of the
            // ceiling, and it is really liquidated doing so. A fund counted but never spent
            // would make the barrel decorative: this is the line that puts it back in charge.
            double warCeiling = economy.HeadlineGdpBillions * economy.WarBudgetCeilingShare;
            double liquidable = economy.ReservesBillions * economy.ReserveDrawRate;
            double draw = Math.Clamp(warCeiling - ordinary, 0d, liquidable);

            economy.ReservesBillions = Math.Max(0d, economy.ReservesBillions - draw);
            economy.LastTurnReserveDrawBillions = draw;
            economy.OrdinaryWarFundingBillions = ordinary;
            economy.WarFundableBillions = ordinary + draw;

            economy.LastTurnOilRevenueBillions = oilRevenue;
            economy.LastTurnRevenueBillions = fiscal + oilRevenue + (grant - inKindDelivered) - oilCost;
            economy.TreasuryBillions += economy.LastTurnRevenueBillions;

            // Reserves absorb the deficit until they no longer can.
            if (economy.TreasuryBillions < 0d)
            {
                double gap = -economy.TreasuryBillions;
                double drawn = Math.Min(gap, economy.ReservesBillions);
                economy.ReservesBillions -= drawn;
                economy.DebtBillions += gap - drawn;
                economy.TreasuryBillions = 0d;

                if (drawn > 0d)
                {
                    context.Say(LocalizedText.Of(
                        TextCodes.Narrative.ReserveDraw,
                        belligerent.Name,
                        LocalizedText.Number(drawn, "F1")));
                }
            }

            // A barrel over 95 buys social peace; under 55 it stops paying for it.
            if (economy.OilExportCapacityMbd > 0d)
            {
                if (state.OilPrice >= 95d)
                {
                    belligerent.Politics.EliteCohesion = Math.Min(100d, belligerent.Politics.EliteCohesion + 1.5d);
                }
                else if (state.OilPrice <= 55d)
                {
                    belligerent.Politics.EliteCohesion = Math.Max(0d, belligerent.Politics.EliteCohesion - 2.5d);
                }
            }
        }

        // Expensive oil feeds inflation among the donors and erodes their will.
        Belligerent granted = state.Get(Side.Defender);
        if (granted.Foreign.Mode == SupportMode.Granted)
        {
            double energyPain = (state.OilPrice - 75d) / 75d;
            if (energyPain > 0d)
            {
                granted.Politics.ExternalWill = Math.Max(0d, granted.Politics.ExternalWill - (energyPain * 4d));
            }
        }
    }

    /// <summary>
    /// Hands over a slice of aid as materiel and returns, in billions, the part the depot
    /// could not take. Converting back to money is what keeps the aid's value conserved:
    /// the units refused here reappear as cash on the same grant.
    /// </summary>
    private static double DeliverInKind(Belligerent belligerent, ResourceKind kind, double billions)
    {
        if (billions <= 0d)
        {
            return 0d;
        }

        double units = billions * 1000d / kind.UnitCostMillions;
        double refusedUnits = belligerent.FillDepot(kind, units);
        return refusedUnits * kind.UnitCostMillions / 1000d;
    }
}
