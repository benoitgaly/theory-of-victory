using TheoryOfVictory.Core;

namespace TheoryOfVictory.Engine.Phases;

/// <summary>
/// Phase 4. Orders mature, capacity finally comes online, recruits leave training.
/// Everything here was decided two to four turns ago.
/// </summary>
public sealed class ProductionPhase : ITurnPhase
{
    public string Name
    {
        get { return "Production"; }
    }

    public void Execute(TurnContext context)
    {
        foreach (Side side in Side.All)
        {
            Belligerent belligerent = context.State.Get(side);
            DeliverOrders(belligerent);
            CommissionExpansions(context, belligerent);
            GraduateRecruits(context, belligerent);
        }
    }

    private void DeliverOrders(Belligerent belligerent)
    {
        belligerent.ProducedThisTurn.Clear();
        List<ProductionOrder> due = [];
        foreach (ProductionOrder order in belligerent.Industry.Orders)
        {
            order.TurnsRemaining--;
            if (order.TurnsRemaining <= 0)
            {
                due.Add(order);
            }
        }

        foreach (ProductionOrder order in due)
        {
            belligerent.Industry.Orders.Remove(order);

            // Rigged procurement delivers substandard goods: the units exist, they underperform.
            double qualityLoss = Math.Clamp(belligerent.Politics.Corruption / 100d * 0.2d, 0d, 0.2d);
            double usable = order.Units * (1d - qualityLoss);
            belligerent.Stock.Add(order.Kind, usable);

            belligerent.ProducedThisTurn[order.Kind.Code] =
                belligerent.ProducedThisTurn.GetValueOrDefault(order.Kind.Code) + usable;
        }
    }

    private void CommissionExpansions(TurnContext context, Belligerent belligerent)
    {
        List<CapacityExpansion> due = [];
        foreach (CapacityExpansion expansion in belligerent.Industry.Expansions)
        {
            expansion.TurnsRemaining--;
            if (expansion.TurnsRemaining <= 0)
            {
                due.Add(expansion);
            }
        }

        foreach (CapacityExpansion expansion in due)
        {
            belligerent.Industry.Expansions.Remove(expansion);
            belligerent.Industry.AddCapacityPerTurn(expansion.Kind, expansion.AddedUnitsPerTurn);

            if (expansion.Kind == ResourceKind.Weapons && expansion.AddedUnitsPerTurn > 20d)
            {
                context.Say($"{belligerent.Name} : nouvelle capacité en service, +{expansion.AddedUnitsPerTurn:F0} k unités par tour.");
            }
        }
    }

    private void GraduateRecruits(TurnContext context, Belligerent belligerent)
    {
        Manpower manpower = belligerent.Manpower;
        if (manpower.TrainingPipeline.Count < manpower.TrainingTurns)
        {
            return;
        }

        double arriving = manpower.TrainingPipeline.Dequeue();
        manpower.AtFront += arriving;
        TurnContext.Accumulate(context.ReplacementsArrived, belligerent.Side, arriving);
    }
}
