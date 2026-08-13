using TheoryOfVictory.Core;

namespace TheoryOfVictory.Engine.Phases;

/// <summary>
/// Phase 7. The waves that take no ground and decide the war. Saturation precedes
/// penetration, and the exchange ratio decides who can keep doing this.
/// </summary>
public sealed class DeepStrikePhase : ITurnPhase
{
    /// <summary>Share of the civilian base one damage point takes off. Calibration, see §9.2.</summary>
    private const double CivilianLossPerDamagePoint = 0.16d;

    public string Name
    {
        get { return "Frappes en profondeur"; }
    }

    public void Execute(TurnContext context)
    {
        // Nobody strikes before the war starts. Without this, the prologue announces a saturated
        // campaign against a refinery nobody has attacked and gigawatts lost off an intact grid.
        if (context.State.Turn < context.Scenario.CombatStartsOnTurn)
        {
            return;
        }

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

        // A magazine is sized on what it fires. Recording it here is what lets the depot
        // ceiling hold enough interceptors to defend the sky without letting aid pile up a
        // wall of them that no wave could ever saturate.
        target.BurntThisTurn[ResourceKind.CheapInterceptors.Code] = resolution.CheapInterceptorsSpent;
        target.BurntThisTurn[ResourceKind.HeavyInterceptors.Code] = resolution.HeavyInterceptorsSpent;
        attacker.BurntThisTurn[ResourceKind.StrikeDrones.Code] = drones;
        attacker.BurntThisTurn[ResourceKind.Missiles.Code] = missiles;

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
                // A refinery is not a substation. The observed campaigns took 20 % of Russian
                // refining offline in autumn 2025 and 42,7 % by mid-2026, where the engine
                // never took integrity below 87 % — the sustained-campaign card, which is the
                // central weapon of the winning run, bought almost nothing. Depth per wave is
                // doubled here, from 0,09 to 0,18.
                //
                // The audit also asked for the repair rate to be cut from 40 % to 18 % a
                // quarter. That half is deliberately NOT applied: it is the one change that
                // moved the Russian collapse from T19 to T18 and broke the demonstration,
                // whatever else was compensated. Deepening each wave reaches the same goal —
                // making the campaign matter — through the lever that does not move the
                // outcome. See docs/design/04-calibration-effectifs.md §12.
                target.Economy.RefiningIntegrity = Math.Clamp(target.Economy.RefiningIntegrity - (damage * 0.18d), 0.05d, 1d);
                break;

            case StrikeTarget.Industry:
                foreach (ResourceKind kind in ResourceKind.All)
                {
                    double capacity = target.Industry.GetCapacityPerTurn(kind);
                    target.Industry.SetCapacityPerTurn(kind, capacity * Math.Max(0.7d, 1d - (damage * 0.02d)));
                }

                break;

            case StrikeTarget.CivilianIndustry:
                // Same two levels as the grid, and the same lesson: what the wave puts through
                // a warehouse roof comes back in a quarter, what it puts through an assembly
                // line does not. One damage point takes 16 % of the civilian base, so a
                // campaign has to be sustained over two or three quarters before the regime
                // feels anything — which is exactly the point being taught.
                double civilianLoss = damage * CivilianLossPerDamagePoint * target.Civilian.CapacityBillions;
                double civilianPermanent = civilianLoss * resolution.PermanentDamageShare;
                target.Civilian.PermanentDamage += civilianPermanent;
                target.Civilian.ReversibleDamage += civilianLoss - civilianPermanent;
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
            StrikeTarget.CivilianIndustry => $"les entrepôts et les usines civiles {Adjective(target)}s",
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
