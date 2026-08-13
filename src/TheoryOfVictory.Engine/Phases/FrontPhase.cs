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

    /// <summary>Hexes a HELD line yields in a quarter — thirty kilometres, a historic breakthrough.</summary>
    private const double BaseMovementCeiling = 3d;

    /// <summary>Hexes an EMPTY line yields — a hundred and eighty kilometres, March 2022 in the south.</summary>
    private const double MaxMovementCeiling = 18d;

    private const double SectorKilometresPerHex = 10d;

    /// <summary>
    /// Men in contact per kilometre on a line that is actually held. The model produces 312 for
    /// the invader and 325 for the defender on the quarter of the invasion, spread evenly over
    /// the 480 km modelled, and climbs past 700 by 2026. The real breakthroughs of 2022 happened
    /// where the figure was far below two hundred.
    /// </summary>
    private const double HeldLineDensity = 300d;

    /// <summary>An empty line is six times thinner than a held one, and never more.</summary>
    private const double MaxThinness = 6d;

    public string Name
    {
        get { return "Front"; }
    }

    public void Execute(TurnContext context)
    {
        // Combat power is still read every turn — the board draws it, and the prologue is
        // precisely about watching a force being built. Only the resolution is skipped.
        ComputeCombatPower(context, Side.Invader);
        ComputeCombatPower(context, Side.Defender);

        if (context.State.Turn < context.Scenario.CombatStartsOnTurn)
        {
            return;
        }

        Belligerent invader = context.State.Invader;
        Belligerent defender = context.State.DefenderSide;

        Dictionary<string, double> invaderPush = OffensivePressure(context, Side.Invader, invader.SustainableCombatPower);
        Dictionary<string, double> defenderPush = OffensivePressure(context, Side.Defender, defender.SustainableCombatPower);

        Dictionary<string, double> invaderHold = DefensiveCover(context, Side.Invader, invader.SustainableCombatPower, defenderPush);
        Dictionary<string, double> defenderHold = DefensiveCover(context, Side.Defender, defender.SustainableCombatPower, invaderPush);

        // The same split, applied to the power a full stave would have allowed. It decides
        // nothing: no phase reads it, and it exists so the board can draw the men who are
        // present and unsupplied. The enemy pressure passed in is the REAL one on purpose —
        // it only sets the reserve shares, and scaling both sides would not change them.
        double invaderFull = EstablishmentPower(invader);
        double defenderFull = EstablishmentPower(defender);

        Dictionary<string, double> invaderFullPush = OffensivePressure(context, Side.Invader, invaderFull);
        Dictionary<string, double> defenderFullPush = OffensivePressure(context, Side.Defender, defenderFull);
        Dictionary<string, double> invaderFullHold = DefensiveCover(context, Side.Invader, invaderFull, defenderPush);
        Dictionary<string, double> defenderFullHold = DefensiveCover(context, Side.Defender, defenderFull, invaderPush);

        foreach (FrontSector sector in context.State.Sectors)
        {
            SectorCommitment commitment = new()
            {
                InvaderCommitted = invaderPush.GetValueOrDefault(sector.Code) + invaderHold.GetValueOrDefault(sector.Code),
                DefenderCommitted = defenderPush.GetValueOrDefault(sector.Code) + defenderHold.GetValueOrDefault(sector.Code),
                InvaderEstablishment = invaderFullPush.GetValueOrDefault(sector.Code) + invaderFullHold.GetValueOrDefault(sector.Code),
                DefenderEstablishment = defenderFullPush.GetValueOrDefault(sector.Code) + defenderFullHold.GetValueOrDefault(sector.Code),
            };

            ResolveSector(context, sector, invaderPush, defenderPush, invaderHold, defenderHold, commitment);
        }
    }

    /// <summary>What a side's combat power on the line would be with every stave full.
    /// Deliberately the expression of <see cref="ComputeCombatPower"/> minus its coverage
    /// factor — the two must be changed together.</summary>
    private static double EstablishmentPower(Belligerent belligerent)
    {
        Manpower manpower = belligerent.Manpower;
        return manpower.InContact * manpower.TrainingQuality * manpower.CohesionFactor;
    }

    /// <summary>Per-side power on one sector, gathered for publication and read by nothing else.</summary>
    private sealed class SectorCommitment
    {
        public double InvaderCommitted { get; init; }

        public double DefenderCommitted { get; init; }

        public double InvaderEstablishment { get; init; }

        public double DefenderEstablishment { get; init; }
    }

    /// <summary>
    /// Liebig, stated properly. The infantry on the line is the SIZE of the barrel — it scales
    /// the whole thing, linearly, and it is never a stave. The staves are the three flows the
    /// front actually consumes: shells, fuel, rations. Only a consumed flow can have a "need"
    /// and therefore a coverage, and the shortest of them sets the level for all the others.
    ///
    /// Power rides on the men in contact, not on the theatre grouping: an army can double its
    /// tail, double what it eats, and hold exactly the same ground. That is why the requirement
    /// is computed on the theatre force and the power on the contact force — the two ends of
    /// the same men, and the gap between them is where both armies actually lost.
    ///
    /// A manpower deficit is still punished, twice over and without pretending men are a flow:
    /// the barrel shrinks with every man missing, and fighting below establishment costs
    /// cohesion on top of it.
    /// </summary>
    private void ComputeCombatPower(TurnContext context, Side side)
    {
        Belligerent belligerent = context.State.Get(side);
        Manpower manpower = belligerent.Manpower;

        // The staves. The minimum rule is strict and applies to these three, and only these.
        string bottleneck = ResourceKind.Weapons.Code;
        double scarcest = double.MaxValue;

        foreach (ResourceKind kind in ResourceKind.FrontFlows)
        {
            double coverage = belligerent.GetCoverage(kind.Code);
            if (coverage < scarcest)
            {
                scarcest = coverage;
                bottleneck = kind.Code;
            }
        }

        if (scarcest == double.MaxValue)
        {
            scarcest = 1d;
        }

        belligerent.BottleneckCode = bottleneck;
        belligerent.MaterialCoverage = scarcest;

        // The barrel. Infantry on the line, capped by the men the treasury can still pay:
        // an army that cannot be paid shrinks without anyone assaulting it.
        belligerent.SustainableCombatPower =
            manpower.InContact
            * Math.Clamp(scarcest, 0d, 1.2d)
            * manpower.TrainingQuality
            * manpower.CohesionFactor;

        TurnContext.Accumulate(context.WeaponsDelivered, side, belligerent.GetDelivered(ResourceKind.Weapons));
        TurnContext.Accumulate(context.WeaponsConsumed, side, belligerent.GetDelivered(ResourceKind.Weapons));
    }

    /// <summary>The attacker concentrates: this is where a local ratio comes from.</summary>
    private static Dictionary<string, double> OffensivePressure(TurnContext context, Side side, double power)
    {
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
            pressure[sector.Code] = power * share * doctrine.OffensivePosture;
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
        double power,
        Dictionary<string, double> enemyPressure)
    {
        Belligerent belligerent = context.State.Get(side);
        Doctrine doctrine = context.DoctrineFor(side);

        double available = power * (1d - doctrine.OffensivePosture);
        // Redeployment is always late and partial: this is what leaves room for concentration.
        double reactivity = belligerent.HasCollapsed
            ? 0d
            : Math.Clamp(belligerent.Politics.LogisticsIntegrity * 0.45d * doctrine.ReserveMobility, 0d, 0.45d);

        double totalPressure = 0d;
        foreach (double value in enemyPressure.Values)
        {
            totalPressure += value;
        }

        // The standing share is the doctrine's, not a flat eighth: a defender also decides where
        // his men stand. With uniform weights this is exactly the old 1/count, so nothing moves
        // until a scenario says otherwise — and February 2022 says otherwise very loudly, with
        // everything in the fortified Donbass and one brigade for two hundred kilometres of south.
        int count = context.State.Sectors.Count;
        Dictionary<string, double> cover = [];

        foreach (FrontSector sector in context.State.Sectors)
        {
            double standing = DefenceShare(context, doctrine, sector);

            double reactive = totalPressure <= 0d
                ? standing
                : enemyPressure.GetValueOrDefault(sector.Code) / totalPressure;

            double share = (standing * (1d - reactivity)) + (reactive * reactivity);
            cover[sector.Code] = available * share;
        }

        return cover;
    }

    /// <summary>
    /// Share of the standing defence this sector holds. Uniform unless the doctrine says
    /// otherwise — a defender covers the whole line, and only February 2022 says otherwise.
    /// </summary>
    private static double DefenceShare(TurnContext context, Doctrine doctrine, FrontSector sector)
    {
        int count = context.State.Sectors.Count;
        if (doctrine.SectorDefence.Count == 0)
        {
            return count <= 0 ? 0d : 1d / count;
        }

        double total = 0d;
        foreach (FrontSector candidate in context.State.Sectors)
        {
            total += DefenceWeight(doctrine, candidate.Code);
        }

        return total <= 0d
            ? (count <= 0 ? 0d : 1d / count)
            : DefenceWeight(doctrine, sector.Code) / total;
    }

    private static double DefenceWeight(Doctrine doctrine, string sectorCode)
    {
        return doctrine.SectorDefence.TryGetValue(sectorCode, out double value) ? value : 1d;
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
        Dictionary<string, double> defenderHold,
        SectorCommitment commitment)
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
        double seasonModifier = context.State.Season.OffensiveModifier();
        double resistance = Math.Max(0.001d, hold)
            * sector.DefensiveMultiplier(holder.Side)
            * droneFriction
            / seasonModifier;

        double ratio = push / resistance;
        double ceiling = PenetrationCeiling(context, holder, sector, droneFriction);
        double hexes = MovementFor(ratio, ceiling);

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

        // Everything below is publication: values the phase has just used, converted once into
        // the unit the board reads. No figure here is computed for the first time, and nothing
        // in the simulation reads any of it back.
        context.SectorResolutions.Add(new SectorResolution
        {
            SectorCode = sector.Code,
            SectorName = sector.Name,
            AttackerSideCode = attacker.Side.Code,
            // Not rounded: the board prints these two and their ratio, and a reader who
            // divides them must land on the ratio he is being shown.
            AttackerPush = ManCount.ExactFromThousands(push),
            HolderResistance = ManCount.ExactFromThousands(resistance),
            InvaderCommitted = ManCount.FromThousands(commitment.InvaderCommitted),
            DefenderCommitted = ManCount.FromThousands(commitment.DefenderCommitted),
            InvaderEstablishment = ManCount.FromThousands(commitment.InvaderEstablishment),
            DefenderEstablishment = ManCount.FromThousands(commitment.DefenderEstablishment),
            Ratio = ratio,
            HexesMoved = hexes * direction,
            HexesCumulative = sector.HexesGained,
            SectorWidth = sector.Width,
            TerrainMultiplier = sector.TerrainMultiplier,
            Urbanisation = sector.Urbanisation,
            InvaderFortification = sector.InvaderFortification,
            DefenderFortification = sector.DefenderFortification,
            DroneFriction = droneFriction,
            SeasonModifier = seasonModifier,
            AttackerLosses = ManCount.FromThousands(attackerLosses),
            DefenderLosses = ManCount.FromThousands(holderLosses),
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

    /// <summary>
    /// The table a board game can print: ratio in, hexes out. Everything up to a ratio of three
    /// is unchanged — that is the grinding war, and it is calibrated. Past three, the advance
    /// runs into how much line there is left to break, which is what <paramref name="ceiling"/>
    /// carries: a thin line does not merely yield its first hex faster, it yields the next ten.
    /// </summary>
    private static double MovementFor(double ratio, double ceiling)
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

        double slope = 0.5d * (ceiling / BaseMovementCeiling);
        return Math.Min(ceiling, 2d + ((ratio - 3d) * slope));
    }

    /// <summary>
    /// How deep a breakthrough can run this quarter, in hexes. THE point of the whole model: the
    /// rush of March 2022 and the collapse of Kharkiv six months later were not battles won, they
    /// were empty ground. A line is not broken by brilliance, it is broken where nobody is
    /// standing — so what caps an advance is what the holder has in depth, and nothing else.
    ///
    /// Three things put depth on a line, and the engine already carries all three: men per
    /// kilometre, trenches, and drones. Thirty kilometres a quarter — the historic breakthrough
    /// the old fixed cap encoded — is what a HELD line yields. It is the floor here, never the
    /// ceiling: this function can only open the cap, never close it, so the grinding war of
    /// 2024-2026 is untouched by construction.
    ///
    /// It also says, without anyone writing it down, that the drone ended the war of movement.
    /// In 2022 the friction is 1,0 and the cap can open; from 2023 it climbs towards 1,45 and
    /// shuts it for good, whatever the headcount.
    /// </summary>
    private static double PenetrationCeiling(
        TurnContext context,
        Belligerent holder,
        FrontSector sector,
        double droneFriction)
    {
        double kilometres = Math.Max(1d, sector.Width * SectorKilometresPerHex);
        double density =
            ManCount.ExactFromThousands(holder.Manpower.InContact)
            * DefenceShare(context, context.DoctrineFor(holder.Side), sector)
            / kilometres;

        double thinness = Math.Clamp(HeldLineDensity / Math.Max(1d, density), 1d, MaxThinness);
        double depth = thinness
            / ((1d + sector.FortificationOf(holder.Side)) * Math.Max(1d, droneFriction));

        double ceiling = Math.Clamp(BaseMovementCeiling * depth, BaseMovementCeiling, MaxMovementCeiling);

        // A broken defence and an empty one are the same statement, so take the larger of the two
        // and never their product: multiplied, they would move six hundred kilometres in a quarter.
        return holder.HasCollapsed
            ? Math.Max(ceiling, BaseMovementCeiling * CollapseMovementMultiplier)
            : ceiling;
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
