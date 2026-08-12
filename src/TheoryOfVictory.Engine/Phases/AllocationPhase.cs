using TheoryOfVictory.Core;

namespace TheoryOfVictory.Engine.Phases;

/// <summary>
/// Phase 3. The only place a decision is made. Money buys nothing beyond installed
/// capacity: this is why cash without factories produced no shells in 2023.
/// </summary>
public sealed class AllocationPhase : ITurnPhase
{
    private const int ConsumableLeadTurns = 1;
    private const int ExpansionLeadTurns = 3;

    public string Name
    {
        get { return "Allocation"; }
    }

    public void Execute(TurnContext context)
    {
        foreach (Side side in Side.All)
        {
            Belligerent belligerent = context.State.Get(side);
            Doctrine doctrine = context.DoctrineFor(side);
            Allocate(context, belligerent, doctrine);
        }
    }

    private void Allocate(TurnContext context, Belligerent belligerent, Doctrine doctrine)
    {
        Economy economy = belligerent.Economy;

        // Troops are fed and fuelled before anything is chosen: this is a charge, not a decision.
        BuySustainment(context, belligerent);

        double budgetCeiling = economy.HeadlineGdpBillions * economy.WarBudgetCeilingShare;
        double fundable = Math.Max(0d, economy.WarFundableBillions);

        // Three ceilings, and the war takes the lowest: what it holds, what the economy
        // can bear, and what this quarter's revenue actually funds.
        double budget = Math.Min(Math.Min(economy.TreasuryBillions, budgetCeiling), fundable);
        double withheld = economy.TreasuryBillions - budget;

        if (fundable < budgetCeiling * 0.8d && fundable > 0d)
        {
            context.Say($"{belligerent.Name} : effort de guerre bridé par les recettes, {fundable:F1} Md finançables seulement.");
        }

        if (budget <= 0d)
        {
            return;
        }

        // Payroll comes before every choice, like rations. It is most of a war budget,
        // and it is the line through which a collapsing revenue reaches the front: a
        // state that cannot pay its army does not get to keep that army in the line.
        double afterPayroll = PayTroops(context, belligerent, budget);
        double payrollPaid = budget - afterPayroll;
        budget = afterPayroll;

        if (budget <= 0d)
        {
            economy.LastTurnMilitarySpendBillions = payrollPaid;
            economy.TreasuryBillions = Math.Min(
                Math.Max(0d, economy.TreasuryBillions - payrollPaid),
                Math.Max(budgetCeiling, economy.LastTurnRevenueBillions));
            return;
        }

        double total = doctrine.TotalShare;
        if (total <= 0d)
        {
            return;
        }

        belligerent.AirDefence.RearShare = doctrine.RearDefenceShare;
        belligerent.AllocationThisTurn.Clear();

        double unspent = 0d;
        double militarySpend = payrollPaid;

        double Share(double share)
        {
            return budget * share / total;
        }

        void Book(string line, double amount)
        {
            belligerent.AllocationThisTurn[line] = amount;
        }

        Book("recruitment", Share(doctrine.RecruitmentShare));
        Book("weapons", Share(doctrine.WeaponsShare));
        Book("strike", Share(doctrine.StrikeVectorsShare));
        Book("defence", Share(doctrine.AirDefenceShare));
        Book("expansion", Share(doctrine.IndustrialExpansionShare));
        Book("innovation", Share(doctrine.InnovationShare));
        Book("fortification", Share(doctrine.FortificationShare));
        Book("audit", Share(doctrine.AntiCorruptionShare));
        Book("civilian", Share(doctrine.CivilianShare));
        Book("foreign", Share(doctrine.ForeignPurchaseShare));

        // Recruitment: contract bonuses are the cheapest politically, the dearest fiscally.
        double recruitBudget = Share(doctrine.RecruitmentShare);
        unspent += Recruit(context, belligerent, recruitBudget);
        militarySpend += recruitBudget;

        // Consumables, each capped by installed capacity times power supply times sanctions.
        double industrialSupply = belligerent.Grid.IndustrialSupplyRatio(context.State.Season);
        double ceiling = belligerent.Sanctions.ProductionCeilingMultiplier * industrialSupply;

        double weaponsBudget = Share(doctrine.WeaponsShare);
        unspent += Order(belligerent, ResourceKind.Weapons, weaponsBudget, ceiling);
        militarySpend += weaponsBudget;

        double strikeBudget = Share(doctrine.StrikeVectorsShare);
        double droneBudget = strikeBudget * 0.6d;
        double missileBudget = strikeBudget - droneBudget;
        unspent += Order(belligerent, ResourceKind.StrikeDrones, droneBudget, ceiling);
        unspent += Order(belligerent, ResourceKind.Missiles, missileBudget, ceiling);
        militarySpend += strikeBudget;

        double defenceBudget = Share(doctrine.AirDefenceShare);
        double cheapBudget = defenceBudget * belligerent.AirDefence.CheapPurchaseShare;
        unspent += Order(belligerent, ResourceKind.CheapInterceptors, cheapBudget, ceiling);
        unspent += Order(belligerent, ResourceKind.HeavyInterceptors, defenceBudget - cheapBudget, ceiling);
        militarySpend += defenceBudget;

        // Capacity ordered now produces in three turns. This is the decision that wins 2029.
        double expansionBudget = Share(doctrine.IndustrialExpansionShare);
        Expand(belligerent, expansionBudget);
        militarySpend += expansionBudget;

        double innovationBudget = Share(doctrine.InnovationShare);
        belligerent.Innovation.Invest(
            innovationBudget,
            doctrine.InnovationTacticalShare,
            doctrine.InnovationStrikeShare,
            doctrine.InnovationCounterShare);
        militarySpend += innovationBudget;

        double fortifyBudget = Share(doctrine.FortificationShare);
        Fortify(context, belligerent, fortifyBudget);
        militarySpend += fortifyBudget;

        // Cleaning up costs now and pays in three turns, with a political bill on delivery.
        double auditBudget = Share(doctrine.AntiCorruptionShare);
        if (auditBudget > 0d)
        {
            belligerent.Politics.BaselineCorruption = Math.Max(5d, belligerent.Politics.BaselineCorruption - (auditBudget * 0.5d));
            belligerent.Politics.Corruption = Math.Max(0d, belligerent.Politics.Corruption - (auditBudget * 0.6d));
            belligerent.Politics.PopularDiscontent = Math.Min(100d, belligerent.Politics.PopularDiscontent + (auditBudget * 0.8d));
        }

        // Buying abroad bypasses domestic capacity entirely — that is its whole point.
        double foreignBudget = Share(doctrine.ForeignPurchaseShare);
        if (foreignBudget > 0d && belligerent.Foreign.Mode == SupportMode.Purchased)
        {
            double delivered = belligerent.Foreign.Purchase(foreignBudget);
            double units = delivered * 1000d / (ResourceKind.Weapons.UnitCostMillions * 1.15d);
            belligerent.Stock.Add(ResourceKind.Weapons, units);
            militarySpend += foreignBudget;

            if (delivered > 0.5d)
            {
                context.Say($"{belligerent.Name} : {delivered:F1} Md d'armes achetées à l'étranger, hors capacité nationale.");
            }
        }

        double civilianBudget = Share(doctrine.CivilianShare) * 0.65d;
        if (civilianBudget > 0d)
        {
            economy.ProductiveCapacityBillions += civilianBudget * 0.5d;
            belligerent.Politics.Morale = Math.Min(100d, belligerent.Politics.Morale + (civilianBudget * 0.12d));
        }

        economy.LastTurnMilitarySpendBillions = militarySpend - unspent;

        // Cash the war cannot absorb is taken up by the civilian economy, never hoarded forever.
        economy.TreasuryBillions = Math.Min(
            unspent + withheld,
            Math.Max(budgetCeiling, economy.LastTurnRevenueBillions));

        if (unspent > budget * 0.15d)
        {
            context.Say($"{belligerent.Name} : {unspent:F1} Md non convertis — l'argent existe, la capacité non.");
        }
    }

    /// <summary>
    /// Bills the standing army against the war budget and returns what is left to decide
    /// with. When the bill cannot be met, the force the state can field shrinks to what it
    /// can pay — no assault required, and no arbitrary malus either.
    /// </summary>
    private double PayTroops(TurnContext context, Belligerent belligerent, double budget)
    {
        Manpower manpower = belligerent.Manpower;
        if (manpower.UpkeepCostPerThousand <= 0d)
        {
            manpower.PayableForceSize = double.PositiveInfinity;
            return budget;
        }

        double payroll = manpower.AtFront * manpower.UpkeepCostPerThousand;
        belligerent.AllocationThisTurn["payroll"] = Math.Min(payroll, budget);

        if (payroll <= budget)
        {
            manpower.PayableForceSize = double.PositiveInfinity;
            return budget - payroll;
        }

        double payable = budget / manpower.UpkeepCostPerThousand;
        manpower.PayableForceSize = payable;

        context.Say($"{belligerent.Name} : la solde n'est plus couverte — {payable:F0} k hommes finançables "
            + $"sur {manpower.AtFront:F0} k au front.");

        return 0d;
    }

    /// <summary>
    /// Fuel and rations are bought on the market up to two turns of need, before any
    /// discretionary spending. Cheap, unglamorous, and fatal to forget.
    /// </summary>
    private void BuySustainment(TurnContext context, Belligerent belligerent)
    {
        double intensity = 0.7d + (context.DoctrineFor(belligerent.Side).OffensivePosture * 0.6d);
        double men = belligerent.Manpower.AtFront;

        // Two turns of what actually has to leave the depot, leakage included.
        double perTurn = 2d / belligerent.TransmissionRate;

        double missed = BuyUpTo(belligerent, ResourceKind.Fuel, men * LogisticsPhase.FuelPerThousandMen * intensity * perTurn);
        missed += BuyUpTo(belligerent, ResourceKind.Food, men * LogisticsPhase.FoodPerThousandMen * perTurn);

        belligerent.SustainmentShortfall = Math.Clamp(missed, 0d, 1d);
        if (missed > 0.05d)
        {
            context.Say($"{belligerent.Name} : {missed * 100d:F0} % du ravitaillement impayé — la trésorerie ne suit plus.");
        }
    }

    /// <summary>Returns the share of the top-up the treasury could not pay for.</summary>
    private double BuyUpTo(Belligerent belligerent, ResourceKind kind, double targetStock)
    {
        double missing = targetStock - belligerent.Stock.GetActual(kind);
        if (missing <= 0d)
        {
            return 0d;
        }

        double cost = missing * kind.UnitCostMillions / 1000d;
        double affordable = Math.Min(cost, belligerent.Economy.TreasuryBillions);
        if (affordable <= 0d)
        {
            return targetStock <= 0d ? 0d : missing / targetStock;
        }

        double bought = affordable * 1000d / kind.UnitCostMillions;
        belligerent.Stock.Add(kind, bought);
        belligerent.Economy.TreasuryBillions -= affordable;

        return targetStock <= 0d ? 0d : Math.Max(0d, (missing - bought) / targetStock);
    }

    /// <summary>Returns the part of the budget capacity could not absorb.</summary>
    private double Order(Belligerent belligerent, ResourceKind kind, double budgetBillions, double ceilingMultiplier)
    {
        if (budgetBillions <= 0d)
        {
            return 0d;
        }

        double unitCost = kind.UnitCostMillions * belligerent.Politics.ProcurementInflation;
        double affordable = budgetBillions * 1000d / unitCost;

        double capacity = double.IsPositiveInfinity(ceilingMultiplier)
            ? double.MaxValue
            : belligerent.Industry.GetCapacityPerTurn(kind) * ceilingMultiplier;

        double produced = Math.Min(affordable, capacity);
        if (produced <= 0d)
        {
            return budgetBillions;
        }

        belligerent.Industry.Orders.Add(new ProductionOrder
        {
            Kind = kind,
            Units = produced,
            TurnsRemaining = ConsumableLeadTurns,
        });

        double spent = produced * unitCost / 1000d;
        return Math.Max(0d, budgetBillions - spent);
    }

    private double Recruit(TurnContext context, Belligerent belligerent, double budgetBillions)
    {
        Manpower manpower = belligerent.Manpower;
        if (budgetBillions <= 0d || manpower.ContractCostPerThousand <= 0d)
        {
            return budgetBillions;
        }

        // An army recruits to fill its target, not to spend its budget. Without this cap
        // it grows past what it can ever arm, and starves itself of shells.
        double deficit = Math.Max(0d, manpower.TargetForceSize - manpower.AtFront - manpower.InTraining);

        double affordable = budgetBillions / manpower.ContractCostPerThousand;
        double recruited = Math.Min(affordable, manpower.TrainingCapacityPerTurn);
        recruited = Math.Min(recruited, manpower.MobilisablePool);
        recruited = Math.Min(recruited, deficit);

        if (recruited <= 0d)
        {
            return budgetBillions;
        }

        manpower.MobilisablePool -= recruited;
        manpower.TotalMobilisedEver += recruited;
        manpower.TrainingPipeline.Enqueue(recruited);

        // Taking men out of the economy is paid in GDP, and the price rises each wave.
        double gdpHit = manpower.MarginalGdpCost(recruited);
        belligerent.Economy.ProductiveCapacityBillions =
            Math.Max(0d, belligerent.Economy.ProductiveCapacityBillions - gdpHit);

        if (gdpHit > 1d)
        {
            context.Say($"{belligerent.Name} : {recruited:F0} k recrues, {gdpHit:F1} Md de capacité productive en moins.");
        }

        return Math.Max(0d, budgetBillions - (recruited * manpower.ContractCostPerThousand));
    }

    private void Expand(Belligerent belligerent, double budgetBillions)
    {
        if (budgetBillions <= 0d)
        {
            return;
        }

        // Split across the families the doctrine already buys, weighted by unit cost.
        double perFamily = budgetBillions / 3d;
        QueueExpansion(belligerent, ResourceKind.Weapons, perFamily);
        QueueExpansion(belligerent, ResourceKind.StrikeDrones, perFamily * 0.6d);
        QueueExpansion(belligerent, ResourceKind.CheapInterceptors, perFamily * 0.4d);
    }

    private void QueueExpansion(Belligerent belligerent, ResourceKind kind, double budgetBillions)
    {
        if (budgetBillions <= 0d)
        {
            return;
        }

        double unitCost = kind.UnitCostMillions * belligerent.Industry.ExpansionCostMultiplier;
        double added = budgetBillions * 1000d / unitCost;

        belligerent.Industry.Expansions.Add(new CapacityExpansion
        {
            Kind = kind,
            AddedUnitsPerTurn = added,
            TurnsRemaining = ExpansionLeadTurns,
        });
    }

    private void Fortify(TurnContext context, Belligerent belligerent, double budgetBillions)
    {
        if (budgetBillions <= 0d || context.State.Sectors.Count == 0)
        {
            return;
        }

        double perSector = budgetBillions / context.State.Sectors.Count;
        foreach (FrontSector sector in context.State.Sectors)
        {
            sector.Fortify(belligerent.Side, perSector * 0.06d);
        }
    }
}
