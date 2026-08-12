using TheoryOfVictory.Core;

namespace TheoryOfVictory.Engine.Phases;

/// <summary>
/// Phase 9. Where the war economy trap plays out: headline GDP rises on military
/// spending while productive capacity is eaten. And every edge decays on its own.
/// </summary>
public sealed class AttritionPhase : ITurnPhase
{
    public string Name
    {
        get { return "Attrition"; }
    }

    public void Execute(TurnContext context)
    {
        foreach (Side side in Side.All)
        {
            Belligerent belligerent = context.State.Get(side);

            UpdateEconomy(context, belligerent);
            UpdateMoraleAndPolitics(context, belligerent);

            belligerent.Sanctions.AdvanceTurn();
            belligerent.Innovation.Decay();

            belligerent.Economy.RefiningIntegrity = Math.Min(
                1d,
                belligerent.Economy.RefiningIntegrity
                    + ((1d - belligerent.Economy.RefiningIntegrity) * belligerent.Economy.RefiningRepairPerTurn));

            // Rail and bridges are repaired too, or the front would starve within a year.
            belligerent.Politics.LogisticsIntegrity = Math.Min(
                1d,
                belligerent.Politics.LogisticsIntegrity + ((1d - belligerent.Politics.LogisticsIntegrity) * 0.3d));

            belligerent.Industry.Orders.RemoveAll(order => order.Units <= 0d);
        }
    }

    private void UpdateEconomy(TurnContext context, Belligerent belligerent)
    {
        Economy economy = belligerent.Economy;

        double civilianSupply = belligerent.Grid.CivilianSupplyRatio(context.State.Season);
        double powerDrag = (1d - civilianSupply) * 0.12d;

        // The keynesian illusion: war spending inflates the headline while capacity is eaten.
        double stimulus = economy.LastTurnMilitarySpendBillions * economy.MilitarySpendingMultiplier * 0.25d;
        economy.HeadlineGdpBillions = Math.Max(
            0d,
            (economy.HeadlineGdpBillions * (1d + economy.CivilianGrowthPerTurn - powerDrag)) + stimulus);

        economy.ProductiveCapacityBillions = Math.Max(
            0d,
            economy.ProductiveCapacityBillions * (1d - economy.CapitalDecayPerTurn - powerDrag));

        // The headline can never durably exceed what the real economy can carry.
        double ceiling = economy.ProductiveCapacityBillions * 1.35d;
        if (economy.HeadlineGdpBillions > ceiling)
        {
            economy.HeadlineGdpBillions = ceiling;
            economy.InflationRate = Math.Min(0.6d, economy.InflationRate + 0.02d);
        }
        else
        {
            economy.InflationRate = Math.Max(0d, economy.InflationRate - 0.005d);
        }

        // Money poured in fast and unchecked breeds corruption; it drifts towards a structural
        // level rather than decaying to zero, and audits are what move that level.
        double spendPressure = economy.HeadlineGdpBillions <= 0d
            ? 0d
            : economy.LastTurnMilitarySpendBillions / economy.HeadlineGdpBillions;

        double target = Math.Clamp(belligerent.Politics.BaselineCorruption + (spendPressure * 60d), 0d, 100d);
        belligerent.Politics.Corruption = Math.Clamp(
            belligerent.Politics.Corruption + ((target - belligerent.Politics.Corruption) * 0.15d),
            0d,
            100d);

        // Corruption is not only an internal loss: it hardens the donor's conditions.
        if (belligerent.Foreign.Mode == SupportMode.Granted)
        {
            belligerent.Foreign.Conditionality = Math.Clamp(
                belligerent.Politics.Corruption / 100d * 0.45d,
                0d,
                0.9d);
        }
    }

    private void UpdateMoraleAndPolitics(TurnContext context, Belligerent belligerent)
    {
        double losses = TurnContext.Read(context.LossesThisTurn, belligerent.Side);
        double lossPain = losses / 30d;

        belligerent.Politics.PopularDiscontent = Math.Clamp(
            belligerent.Politics.PopularDiscontent + lossPain - 1.2d,
            0d,
            100d);

        // Elites do not care about deaths; they care about whether the war still pays.
        double economicPain = belligerent.Economy.ProductiveCapacityBillions <= 0d
            ? 1d
            : Math.Clamp(1d - (belligerent.Economy.HeadlineGdpBillions / belligerent.Economy.ProductiveCapacityBillions), -1d, 1d);

        belligerent.Politics.EliteCohesion = Math.Clamp(
            belligerent.Politics.EliteCohesion - (economicPain * 3d) - (belligerent.Sanctions.ComponentSeverity * 1.2d) + 0.6d,
            0d,
            100d);

        belligerent.Politics.ApplyRepression();

        // Political capital: centralised and steady on one side, diplomatic and jumpy on the other.
        double generated = belligerent.Politics.Regime == RegimeType.Authoritarian
            ? 3d * (belligerent.Politics.Morale / 100d)
            : 2d * (belligerent.Politics.ExternalWill / 100d) * (belligerent.Politics.Morale / 100d);

        belligerent.Politics.PoliticalCapital = Math.Min(30d, belligerent.Politics.PoliticalCapital + generated);
    }
}
