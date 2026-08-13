namespace TheoryOfVictory.Core;

/// <summary>
/// The civilian productive base: warehouses, assembly lines, retail logistics. It builds
/// nothing the front consumes, and that is the point — it produces the LIVING STANDARD, and
/// the living standard is what buys consent for the war.
///
/// Burning a distribution hub takes no ground, destroys no shell, and moves nothing on the
/// map for a quarter. What it moves is the population's tolerance, then the regime's margin,
/// then the political capital that pays for cards. Six links, all of them already in the
/// engine except the first.
///
/// Two damage levels, exactly as the power grid has: a warehouse is rebuilt in a quarter, an
/// assembly line is not rebuilt inside a war. Aiming at the line rather than at the pallet is
/// the whole difference between a nuisance and a loss.
/// </summary>
public sealed class CivilianIndustry
{
    /// <summary>Installed civilian output in billions. Working order of magnitude.</summary>
    public double CapacityBillions { get; set; }

    /// <summary>Warehouses and distribution: weeks. The attacker has to come back.</summary>
    public double ReversibleDamage { get; set; }

    /// <summary>Assembly lines and tooling: years. Gone for the war.</summary>
    public double PermanentDamage { get; set; }

    public double RepairRatePerTurn { get; set; } = 0.5d;

    public double RebuildPerTurn { get; set; } = 0.05d;

    /// <summary>Damage saturates: past a point the strikes hit what is already rubble.</summary>
    public double MaxLossShare { get; set; } = 0.6d;

    /// <summary>
    /// Consumption per head against its pre-war level, one at the outset. Set every turn by
    /// the energy phase, because a factory without power produces exactly as much as a
    /// factory that has been flattened.
    /// </summary>
    public double LivingStandard { get; set; } = 1d;

    /// <summary>Share of the civilian base still able to produce at all.</summary>
    public double Integrity
    {
        get
        {
            if (CapacityBillions <= 0d)
            {
                return 1d;
            }

            double damage = Math.Min(ReversibleDamage + PermanentDamage, CapacityBillions * MaxLossShare);
            return Math.Clamp(1d - (damage / CapacityBillions), 0d, 1d);
        }
    }

    // No "what it delivered this quarter" property here, deliberately. The band reads this post
    // as CapacityBillions × Integrity — the plant that still stands. Multiplying by the living
    // standard instead would fold the power supply into it, and a factory left dark by a strike
    // on the grid would be booked as civilian capital destroyed: the same wave counted twice,
    // once on the grid cartouche and once here. The living standard is published as the post's
    // secondary reading, which is where a flow belongs.

    /// <summary>
    /// The living standard of the quarter. Deliberately the product of intact plant and
    /// available power, and nothing else: any further term would move on the historical
    /// trajectory, and this quantity has to reproduce the existing discontent exactly on a
    /// run where nobody strikes the civilian base.
    /// </summary>
    public void UpdateLivingStandard(double civilianPowerSupply)
    {
        LivingStandard = Math.Clamp(Integrity * civilianPowerSupply, 0d, 1d);
    }

    public void Repair()
    {
        ReversibleDamage = Math.Max(0d, ReversibleDamage * (1d - RepairRatePerTurn));
        PermanentDamage = Math.Max(0d, PermanentDamage * (1d - RebuildPerTurn));
    }
}
