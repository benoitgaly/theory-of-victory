using TheoryOfVictory.Core;

namespace TheoryOfVictory.Engine.Phases;

/// <summary>
/// Phase 5. What leaves the depot is not what arrives. Corruption leaks it,
/// enemy interdiction cuts it, and what is left is all the front ever sees.
/// </summary>
public sealed class LogisticsPhase : ITurnPhase
{
    /// <summary>Thousands of rounds one thousand men burn in a quarter of sustained combat.</summary>
    public const double WeaponsPerThousandMen = 1.8d;

    public const double FuelPerThousandMen = 0.55d;

    public const double FoodPerThousandMen = 0.42d;

    public string Name
    {
        get { return "Logistique"; }
    }

    public void Execute(TurnContext context)
    {
        foreach (Side side in Side.All)
        {
            Belligerent belligerent = context.State.Get(side);
            Doctrine doctrine = context.DoctrineFor(side);

            double intensity = 0.7d + (doctrine.OffensivePosture * 0.6d);
            double men = belligerent.Manpower.AtFront;

            Deliver(belligerent, ResourceKind.Weapons, men * WeaponsPerThousandMen * intensity * belligerent.Innovation.WeaponDemandMultiplier);
            Deliver(belligerent, ResourceKind.Fuel, men * FuelPerThousandMen * intensity);
            Deliver(belligerent, ResourceKind.Food, men * FoodPerThousandMen);
        }
    }

    private void Deliver(Belligerent belligerent, ResourceKind kind, double required)
    {
        belligerent.NeedThisTurn[kind.Code] = required;

        if (required <= 0d)
        {
            belligerent.SetDelivered(kind, 0d);
            belligerent.CoverageThisTurn[kind.Code] = 1d;
            return;
        }

        // A depot exists to be drawn on harder than the front consumes: what leaks on the
        // way has to leave the warehouse too. Capping the draw at the need would make the
        // pile decorative — and a stock that cannot absorb a bad quarter buys no latency,
        // which is precisely the latency the whole demonstration rests on.
        double leavingDepot = required / belligerent.TransmissionRate;
        double drawn = belligerent.Stock.Consume(kind, leavingDepot);
        double delivered = drawn * belligerent.TransmissionRate;

        belligerent.SetDelivered(kind, delivered);
        belligerent.CoverageThisTurn[kind.Code] = Math.Clamp(delivered / required, 0d, 1.6d);
    }
}
