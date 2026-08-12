namespace TheoryOfVictory.Core;

/// <summary>What a deep strike wave is aimed at. Geography differs by side.</summary>
public enum StrikeTarget
{
    /// <summary>Power generation — the input of every input. Ukraine's vulnerability.</summary>
    PowerGrid = 0,

    /// <summary>Refineries and export terminals — Russia's vulnerability, concentrated and few.</summary>
    Refining = 1,

    /// <summary>Arms plants — slowest payoff, hardest to repair.</summary>
    Industry = 2,

    /// <summary>Rail nodes, bridges, depots — degrades transmission to the front.</summary>
    Logistics = 3,
}

/// <summary>Outcome of one quarterly strike wave, kept for display and for the lesson.</summary>
public sealed class StrikeResolution
{
    public required StrikeTarget Target { get; init; }

    public double DronesSent { get; init; }

    public double MissilesSent { get; init; }

    public double DronesIntercepted { get; init; }

    public double MissilesIntercepted { get; init; }

    public double CheapInterceptorsSpent { get; init; }

    public double HeavyInterceptorsSpent { get; init; }

    /// <summary>Abstract damage points, converted by the phase that applies them.</summary>
    public double DamageInflicted { get; init; }

    public double PermanentDamageShare { get; init; }

    /// <summary>Cost of the rounds fired over the cost of what they destroyed.</summary>
    public double ExchangeRatio { get; init; }

    /// <summary>True when the wave overwhelmed the magazines rather than the radars.</summary>
    public bool Saturated { get; init; }

    public double DronesLeaked
    {
        get { return Math.Max(0d, DronesSent - DronesIntercepted); }
    }

    public double MissilesLeaked
    {
        get { return Math.Max(0d, MissilesSent - MissilesIntercepted); }
    }

    public double InterceptionRate
    {
        get
        {
            double sent = DronesSent + MissilesSent;
            if (sent <= 0d)
            {
                return 0d;
            }

            return (DronesIntercepted + MissilesIntercepted) / sent;
        }
    }
}

/// <summary>
/// Resolves a strike wave. Interception depends on magazine depth against salvo size,
/// never on a constant rate: saturation precedes penetration.
/// </summary>
public static class StrikeResolver
{
    // Calibrated so a sustained multi-quarter campaign, not a single wave, breaks a grid.
    private const double DroneDamagePerLeaker = 0.00018d;
    private const double MissileDamagePerLeaker = 0.0075d;
    private const double MissilePermanentShare = 0.55d;

    public static StrikeResolution Resolve(
        StrikeTarget target,
        double dronesSent,
        double missilesSent,
        Belligerent attacker,
        Belligerent defender)
    {
        AirDefenceSystem defence = defender.AirDefence;
        double penetration = 1d + attacker.Innovation.StrikeEdge;

        double cheapAvailable = defender.Stock.GetActual(ResourceKind.CheapInterceptors) * defence.RearShare;
        double heavyAvailable = defender.Stock.GetActual(ResourceKind.HeavyInterceptors) * defence.RearShare;

        // Cheap defence engages the saturating wave; electronic warfare multiplies its reach.
        double cheapEfficiency = defence.CheapEngagementsPerUnit
            * (1d + defender.Innovation.CounterDroneEdge)
            * defence.Coverage
            / penetration;

        double cheapCapacity = cheapAvailable * cheapEfficiency;
        double dronesKilledCheap = Math.Min(dronesSent, cheapCapacity);
        double cheapSpent = cheapEfficiency <= 0d ? 0d : dronesKilledCheap / cheapEfficiency;

        double dronesLeaking = Math.Max(0d, dronesSent - dronesKilledCheap);
        bool saturated = dronesLeaking > 0d;

        // Heavy rounds burnt on cheap leakers are the rounds the missiles will not meet.
        double heavyWasted = Math.Min(heavyAvailable, dronesLeaking * defence.HeavyWasteOnDrones);
        double dronesKilledHeavy = Math.Min(dronesLeaking, heavyWasted);

        double heavyEfficiency = defence.HeavyEngagementsPerUnit * defence.Coverage / penetration;
        double heavyRemaining = Math.Max(0d, heavyAvailable - heavyWasted);
        double missilesIntercepted = Math.Min(missilesSent, heavyRemaining * heavyEfficiency);
        double heavySpentOnMissiles = heavyEfficiency <= 0d ? 0d : missilesIntercepted / heavyEfficiency;

        double dronesIntercepted = dronesKilledCheap + dronesKilledHeavy;
        double heavySpent = heavyWasted + heavySpentOnMissiles;

        double dronesLeaked = Math.Max(0d, dronesSent - dronesIntercepted);
        double missilesLeaked = Math.Max(0d, missilesSent - missilesIntercepted);

        double damage = (dronesLeaked * DroneDamagePerLeaker) + (missilesLeaked * MissileDamagePerLeaker);

        // Missiles reach the turbine halls; drones mostly hit what can be repaired.
        double permanentPoints = missilesLeaked * MissileDamagePerLeaker * MissilePermanentShare;
        double permanentShare = damage <= 0d ? 0d : Math.Clamp(permanentPoints / damage, 0d, 1d);

        double roundsValue = (cheapSpent * ResourceKind.CheapInterceptors.UnitCostMillions)
            + (heavySpent * ResourceKind.HeavyInterceptors.UnitCostMillions);
        double killedValue = (dronesIntercepted * ResourceKind.StrikeDrones.UnitCostMillions)
            + (missilesIntercepted * ResourceKind.Missiles.UnitCostMillions);
        double exchange = killedValue <= 0d ? 0d : roundsValue / killedValue;

        return new StrikeResolution
        {
            Target = target,
            DronesSent = dronesSent,
            MissilesSent = missilesSent,
            DronesIntercepted = dronesIntercepted,
            MissilesIntercepted = missilesIntercepted,
            CheapInterceptorsSpent = cheapSpent,
            HeavyInterceptorsSpent = heavySpent,
            DamageInflicted = damage,
            PermanentDamageShare = permanentShare,
            ExchangeRatio = exchange,
            Saturated = saturated,
        };
    }
}
