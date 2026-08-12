using TheoryOfVictory.Core;

namespace TheoryOfVictory.Engine.Phases;

/// <summary>
/// Phase 7. The waves that take no ground and decide the war. Saturation precedes
/// penetration, and the exchange ratio decides who can keep doing this.
/// </summary>
public sealed class DeepStrikePhase : ITurnPhase
{
    public string Name
    {
        get { return "Frappes en profondeur"; }
    }

    public void Execute(TurnContext context)
    {
        StrikeResolution? invader = Launch(context, Side.Invader);
        StrikeResolution? defender = Launch(context, Side.Defender);

        context.InvaderStrike = invader;
        context.DefenderStrike = defender;
    }

    private StrikeResolution? Launch(TurnContext context, Side side)
    {
        Belligerent attacker = context.State.Get(side);
        Belligerent target = context.State.Get(side.Opponent);
        Doctrine doctrine = context.DoctrineFor(side);

        double drones = attacker.Stock.Consume(ResourceKind.StrikeDrones, attacker.Stock.GetActual(ResourceKind.StrikeDrones));
        double missiles = attacker.Stock.Consume(ResourceKind.Missiles, attacker.Stock.GetActual(ResourceKind.Missiles));

        if (drones <= 0d && missiles <= 0d)
        {
            return null;
        }

        StrikeResolution resolution = StrikeResolver.Resolve(
            doctrine.PrimaryStrikeTarget,
            drones,
            missiles,
            attacker,
            target);

        target.Stock.Destroy(ResourceKind.CheapInterceptors, resolution.CheapInterceptorsSpent);
        target.Stock.Destroy(ResourceKind.HeavyInterceptors, resolution.HeavyInterceptorsSpent);

        ApplyDamage(context, target, resolution);
        Report(context, attacker, target, resolution);

        return resolution;
    }

    private void ApplyDamage(TurnContext context, Belligerent target, StrikeResolution resolution)
    {
        double damage = resolution.DamageInflicted;
        if (damage <= 0d)
        {
            return;
        }

        switch (resolution.Target)
        {
            case StrikeTarget.PowerGrid:
                double permanent = damage * resolution.PermanentDamageShare;
                target.Grid.PermanentDamageGw += permanent;
                target.Grid.ReversibleDamageGw += damage - permanent;
                break;

            case StrikeTarget.Refining:
                target.Economy.RefiningIntegrity = Math.Clamp(target.Economy.RefiningIntegrity - (damage * 0.09d), 0.05d, 1d);
                break;

            case StrikeTarget.Industry:
                foreach (ResourceKind kind in ResourceKind.All)
                {
                    double capacity = target.Industry.GetCapacityPerTurn(kind);
                    target.Industry.SetCapacityPerTurn(kind, capacity * Math.Max(0.7d, 1d - (damage * 0.02d)));
                }

                break;

            case StrikeTarget.Logistics:
                target.Politics.LogisticsIntegrity = Math.Clamp(
                    target.Politics.LogisticsIntegrity - (damage * 0.05d),
                    0.25d,
                    1d);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(resolution), resolution.Target, "Unhandled strike target.");
        }
    }

    private static string Adjective(Belligerent belligerent)
    {
        return belligerent.Side == Side.Invader ? "russe" : "ukrainien";
    }

    private void Report(TurnContext context, Belligerent attacker, Belligerent target, StrikeResolution resolution)
    {
        string targetName = resolution.Target switch
        {
            StrikeTarget.PowerGrid => $"le réseau électrique de l'arrière {Adjective(target)}",
            StrikeTarget.Refining => $"le raffinage et les terminaux {Adjective(target)}s",
            StrikeTarget.Industry => $"les usines d'armement {Adjective(target)}s",
            StrikeTarget.Logistics => $"les nœuds logistiques {Adjective(target)}s",
            _ => "l'arrière adverse",
        };

        if (resolution.Saturated && resolution.ExchangeRatio > 1.5d)
        {
            context.Say(
                $"{attacker.Name} sature {targetName} — {resolution.InterceptionRate * 100d:F0} % interceptés, "
                + $"mais {resolution.ExchangeRatio:F1} € dépensés par € détruit.");
            return;
        }

        context.Say(
            $"{attacker.Name} frappe {targetName} — {resolution.InterceptionRate * 100d:F0} % interceptés.");
    }
}
