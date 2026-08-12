using TheoryOfVictory.Core;

namespace TheoryOfVictory.Engine.Phases;

/// <summary>
/// Phase 8. The front is a thermometer, not an engine. Power is the scarcest flow,
/// never the sum, and attacking costs three to five times what holding costs.
/// </summary>
public sealed class FrontPhase : ITurnPhase
{
    private const double BaseDefenderLossRate = 0.021d;
    private const double CollapseMovementMultiplier = 3.5d;

    public string Name
    {
        get { return "Front"; }
    }

    public void Execute(TurnContext context)
    {
        ComputeCombatPower(context, Side.Invader);
        ComputeCombatPower(context, Side.Defender);

        Dictionary<string, double> invaderPush = OffensivePressure(context, Side.Invader);
        Dictionary<string, double> defenderPush = OffensivePressure(context, Side.Defender);

        Dictionary<string, double> invaderHold = DefensiveCover(context, Side.Invader, defenderPush);
        Dictionary<string, double> defenderHold = DefensiveCover(context, Side.Defender, invaderPush);

        foreach (FrontSector sector in context.State.Sectors)
        {
            ResolveSector(context, sector, invaderPush, defenderPush, invaderHold, defenderHold);
        }
    }

    /// <summary>Power is the binding constraint: a minimum, and we remember which one binds.</summary>
    private void ComputeCombatPower(TurnContext context, Side side)
    {
        Belligerent belligerent = context.State.Get(side);
        Manpower manpower = belligerent.Manpower;

        double infantry = Math.Clamp(manpower.InfantryCoverage, 0d, 1.6d);
        belligerent.CoverageThisTurn["infantry"] = infantry;

        // Money for salaries is a front flow like any other: it is scarce, it is consumed
        // every quarter, and running out of it empties the line as surely as running out
        // of shells. Treating it as a fourth stave keeps one single rule for everything.
        belligerent.CoverageThisTurn["payroll"] = manpower.TargetForceSize <= 0d
            ? 1.5d
            : Math.Clamp(manpower.PayableForceSize / manpower.TargetForceSize, 0d, 1.5d);

        string bottleneck = "infantry";
        double scarcest = infantry;

        foreach (string code in belligerent.CoverageThisTurn.Keys)
        {
            double coverage = belligerent.CoverageThisTurn[code];
            if (coverage < scarcest)
            {
                scarcest = coverage;
                bottleneck = code;
            }
        }

        belligerent.BottleneckCode = bottleneck;
        belligerent.SustainableCombatPower =
            manpower.TargetForceSize * Math.Clamp(scarcest, 0d, 1.2d) * manpower.TrainingQuality;

        TurnContext.Accumulate(context.WeaponsDelivered, side, belligerent.GetDelivered(ResourceKind.Weapons));
        TurnContext.Accumulate(context.WeaponsConsumed, side, belligerent.GetDelivered(ResourceKind.Weapons));
    }

    /// <summary>The attacker concentrates: this is where a local ratio comes from.</summary>
    private static Dictionary<string, double> OffensivePressure(TurnContext context, Side side)
    {
        Belligerent belligerent = context.State.Get(side);
        Doctrine doctrine = context.DoctrineFor(side);

        double total = 0d;
        foreach (FrontSector sector in context.State.Sectors)
        {
            total += Weight(doctrine, sector.Code);
        }

        Dictionary<string, double> pressure = [];
        foreach (FrontSector sector in context.State.Sectors)
        {
            double share = total <= 0d ? 0d : Weight(doctrine, sector.Code) / total;
            pressure[sector.Code] = belligerent.SustainableCombatPower * share * doctrine.OffensivePosture;
        }

        return pressure;
    }

    /// <summary>
    /// The defender must hold everywhere, but shifts reserves towards pressure.
    /// Reactivity rides on logistics: a side whose rear is cut can no longer redeploy.
    /// </summary>
    private static Dictionary<string, double> DefensiveCover(
        TurnContext context,
        Side side,
        Dictionary<string, double> enemyPressure)
    {
        Belligerent belligerent = context.State.Get(side);
        Doctrine doctrine = context.DoctrineFor(side);

        double available = belligerent.SustainableCombatPower * (1d - doctrine.OffensivePosture);
        // Redeployment is always late and partial: this is what leaves room for concentration.
        double reactivity = belligerent.HasCollapsed
            ? 0d
            : Math.Clamp(belligerent.Politics.LogisticsIntegrity * 0.45d, 0d, 0.45d);

        double totalPressure = 0d;
        foreach (double value in enemyPressure.Values)
        {
            totalPressure += value;
        }

        int count = context.State.Sectors.Count;
        Dictionary<string, double> cover = [];

        foreach (FrontSector sector in context.State.Sectors)
        {
            double uniform = count <= 0 ? 0d : 1d / count;
            double reactive = totalPressure <= 0d
                ? uniform
                : enemyPressure.GetValueOrDefault(sector.Code) / totalPressure;

            double share = (uniform * (1d - reactivity)) + (reactive * reactivity);
            cover[sector.Code] = available * share;
        }

        return cover;
    }

    private static double Weight(Doctrine doctrine, string sectorCode)
    {
        return doctrine.SectorEffort.TryGetValue(sectorCode, out double value) ? value : 1d;
    }

    private void ResolveSector(
        TurnContext context,
        FrontSector sector,
        Dictionary<string, double> invaderPush,
        Dictionary<string, double> defenderPush,
        Dictionary<string, double> invaderHold,
        Dictionary<string, double> defenderHold)
    {
        Belligerent invader = context.State.Invader;
        Belligerent defender = context.State.DefenderSide;

        double pushIn = invaderPush.GetValueOrDefault(sector.Code);
        double pushDef = defenderPush.GetValueOrDefault(sector.Code);

        Belligerent attacker;
        Belligerent holder;
        double push;
        double hold;
        double direction;

        if (pushIn >= pushDef)
        {
            attacker = invader;
            holder = defender;
            push = pushIn;
            hold = defenderHold.GetValueOrDefault(sector.Code);
            direction = 1d;
        }
        else
        {
            attacker = defender;
            holder = invader;
            push = pushDef;
            hold = invaderHold.GetValueOrDefault(sector.Code);
            direction = -1d;
        }

        // Tactical drones make every attack dearer for both sides: this is what freezes the front.
        double droneFriction = 1d + ((attacker.Innovation.TacticalDroneEdge + holder.Innovation.TacticalDroneEdge) * 0.45d);
        double resistance = Math.Max(0.001d, hold)
            * sector.DefensiveMultiplier(holder.Side)
            * droneFriction
            / context.State.Season.OffensiveModifier();

        double ratio = push / resistance;
        double hexes = MovementFor(ratio);

        if (holder.HasCollapsed)
        {
            hexes *= CollapseMovementMultiplier;
        }

        double engagedHolder = holder.Manpower.AtFront * SectorManpowerShare(context, holder, sector);
        double holderLosses = engagedHolder * BaseDefenderLossRate;
        double attackerLosses = holderLosses * AttackCostMultiplier(ratio);

        sector.HexesGained += hexes * direction;
        ApplyLosses(context, attacker, attackerLosses);
        ApplyLosses(context, holder, holderLosses);

        // Ground taken lengthens your own supply lines: advance carries its own penalty.
        if (hexes > 0d)
        {
            attacker.Politics.LogisticsIntegrity = Math.Clamp(
                attacker.Politics.LogisticsIntegrity - (hexes * 0.006d),
                0.25d,
                1d);
        }

        context.SectorResolutions.Add(new SectorResolution
        {
            SectorCode = sector.Code,
            SectorName = sector.Name,
            AttackerPower = push,
            DefenderPower = resistance,
            Ratio = ratio,
            HexesMoved = hexes * direction,
            HexesCumulative = sector.HexesGained,
            SectorWidth = sector.Width,
            AttackerLosses = attackerLosses,
            DefenderLosses = holderLosses,
            Outcome = DescribeOutcome(ratio, hexes, attacker, holder),
        });
    }

    private static double SectorManpowerShare(TurnContext context, Belligerent belligerent, FrontSector sector)
    {
        Doctrine doctrine = context.DoctrineFor(belligerent.Side);
        double total = 0d;
        foreach (FrontSector candidate in context.State.Sectors)
        {
            total += Weight(doctrine, candidate.Code);
        }

        return total <= 0d ? 0d : Weight(doctrine, sector.Code) / total;
    }

    /// <summary>The table a board game can print: ratio in, hexes out.</summary>
    private static double MovementFor(double ratio)
    {
        if (ratio < 1.1d)
        {
            return 0d;
        }

        if (ratio < 2d)
        {
            return (ratio - 1.1d) / 0.9d;
        }

        if (ratio < 3d)
        {
            return 1d + (ratio - 2d);
        }

        // Thirty kilometres a quarter is already a historic breakthrough: cap it there.
        return Math.Min(3d, 2d + ((ratio - 3d) * 0.5d));
    }

    /// <summary>Attacking costs three to five times holding, unless the defence has broken.</summary>
    private static double AttackCostMultiplier(double ratio)
    {
        if (ratio < 1.1d)
        {
            return 5d;
        }

        if (ratio < 2d)
        {
            return 4d;
        }

        if (ratio < 3d)
        {
            return 2.5d;
        }

        return 1.2d;
    }

    private void ApplyLosses(TurnContext context, Belligerent belligerent, double losses)
    {
        double actual = Math.Min(belligerent.Manpower.AtFront, losses);
        belligerent.Manpower.AtFront -= actual;
        belligerent.Manpower.CumulativeLosses += actual;
        TurnContext.Accumulate(context.LossesThisTurn, belligerent.Side, actual);
    }

    private static string DescribeOutcome(double ratio, double hexes, Belligerent attacker, Belligerent holder)
    {
        if (holder.HasCollapsed)
        {
            return $"Effondrement de {holder.Name} — avance libre";
        }

        if (ratio < 1.1d)
        {
            return "Aucun mouvement, usure réciproque";
        }

        if (hexes < 1d)
        {
            return $"Grignotage par {attacker.Name}";
        }

        if (ratio < 3d)
        {
            return $"Avance de {attacker.Name}";
        }

        return $"Percée de {attacker.Name}";
    }
}
