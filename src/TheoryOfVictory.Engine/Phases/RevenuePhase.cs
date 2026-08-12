using TheoryOfVictory.Core;

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
        get { return "Revenus"; }
    }

    public void Execute(TurnContext context)
    {
        GameState state = context.State;
        state.OilPrice = context.Scenario.OilPriceAt(state.Turn);

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
            double inKind = grant * belligerent.Foreign.InKindShare;
            if (inKind > 0d)
            {
                // Aid is not mostly ammunition: vehicles, training and maintenance eat most of it.
                belligerent.Stock.Add(ResourceKind.Weapons, inKind * 1000d * 0.38d / ResourceKind.Weapons.UnitCostMillions);
                belligerent.Stock.Add(ResourceKind.CheapInterceptors, inKind * 1000d * 0.12d / ResourceKind.CheapInterceptors.UnitCostMillions);
                belligerent.Stock.Add(ResourceKind.HeavyInterceptors, inKind * 1000d * 0.18d / ResourceKind.HeavyInterceptors.UnitCostMillions);
            }

            economy.LastTurnOilRevenueBillions = oilRevenue;
            economy.LastTurnRevenueBillions = fiscal + oilRevenue + (grant - inKind) - oilCost;
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
                    context.Say($"{belligerent.Name} : {drawn:F1} Md ponctionnés sur les réserves.");
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
}
