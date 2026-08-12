namespace TheoryOfVictory.Core;

/// <summary>
/// Electricity never reaches the front: it is the input of every input.
/// Load shedding is a threshold, not a slope — damage below the margin costs nothing.
/// </summary>
public sealed class EnergyGrid
{
    /// <summary>Installed generation in GW, before any damage.</summary>
    public double NominalCapacityGw { get; set; }

    /// <summary>Substations and transformers: weeks to repair, so the attacker must come back.</summary>
    public double ReversibleDamageGw { get; set; }

    /// <summary>Turbine halls: unique parts, foreign makers, years of delay. Never comes back.</summary>
    public double PermanentDamageGw { get; set; }

    /// <summary>Share of reversible damage repaired each turn.</summary>
    public double RepairRatePerTurn { get; set; } = 0.55d;

    /// <summary>Nuclear and hydro survive: a grid is never destroyed outright.</summary>
    public double MaxPermanentLossShare { get; set; } = 0.55d;

    /// <summary>Turbine halls are rebuilt, slowly, with imported parts.</summary>
    public double PermanentRebuildPerTurn { get; set; } = 0.06d;

    /// <summary>Baseline demand in GW outside winter.</summary>
    public double BaseDemandGw { get; set; }

    /// <summary>Winter demand multiplier — one turn in four is the crisis turn.</summary>
    public double WinterDemandMultiplier { get; set; } = 1.45d;

    /// <summary>Share of demand that is civilian, shed first.</summary>
    public double CivilianShareOfDemand { get; set; } = 0.55d;

    /// <summary>Damage saturates: past a point, further strikes hit what is already dark.</summary>
    public double MaxTotalLossShare { get; set; } = 0.6d;

    public double AvailableCapacityGw
    {
        get
        {
            double damage = Math.Min(
                ReversibleDamageGw + PermanentDamageGw,
                NominalCapacityGw * MaxTotalLossShare);

            return Math.Max(0d, NominalCapacityGw - damage);
        }
    }

    public double DemandGw(Season season)
    {
        return season == Season.Winter ? BaseDemandGw * WinterDemandMultiplier : BaseDemandGw;
    }

    /// <summary>Unmet demand as a share, zero while the margin absorbs the damage.</summary>
    public double ShortfallRatio(Season season)
    {
        double demand = DemandGw(season);
        if (demand <= 0d)
        {
            return 0d;
        }

        double deficit = demand - AvailableCapacityGw;
        if (deficit <= 0d)
        {
            return 0d;
        }

        return Math.Clamp(deficit / demand, 0d, 1d);
    }

    /// <summary>Civilians are shed first, so industry only suffers past the civilian buffer.</summary>
    public double IndustrialSupplyRatio(Season season)
    {
        double shortfall = ShortfallRatio(season);
        if (shortfall <= CivilianShareOfDemand)
        {
            return 1d;
        }

        double industrialShare = 1d - CivilianShareOfDemand;
        double industrialDeficit = shortfall - CivilianShareOfDemand;
        return Math.Clamp(1d - (industrialDeficit / industrialShare), 0d, 1d);
    }

    /// <summary>Civilian supply drives GDP and heating, so it drives morale too.</summary>
    public double CivilianSupplyRatio(Season season)
    {
        double shortfall = ShortfallRatio(season);
        if (CivilianShareOfDemand <= 0d)
        {
            return 1d;
        }

        return Math.Clamp(1d - (shortfall / CivilianShareOfDemand), 0d, 1d);
    }

    public void Repair()
    {
        ReversibleDamageGw = Math.Max(0d, ReversibleDamageGw * (1d - RepairRatePerTurn));
        PermanentDamageGw = Math.Max(0d, PermanentDamageGw * (1d - PermanentRebuildPerTurn));
        PermanentDamageGw = Math.Min(PermanentDamageGw, NominalCapacityGw * MaxPermanentLossShare);
    }
}
