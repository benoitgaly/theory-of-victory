namespace TheoryOfVictory.Core;

/// <summary>How this side's war ends from the rear when it ends from the rear.</summary>
public enum RegimeType
{
    /// <summary>Falls when the apparatus splits, not when the street shouts.</summary>
    Authoritarian = 0,

    /// <summary>Falls when the will to continue runs out, through the ballot box.</summary>
    Democratic = 1,
}

/// <summary>
/// Two gauges, not one. The street is spectacular and rarely fatal; elite cohesion
/// is silent and decides. Repression closes the valve and raises the pressure.
/// </summary>
public sealed class PoliticalState
{
    public required RegimeType Regime { get; init; }

    /// <summary>Will to keep fighting, 0 to 100.</summary>
    public double Morale { get; set; } = 100d;

    /// <summary>Visible unrest: mobilisation, deaths, living standards.</summary>
    public double PopularDiscontent { get; set; }

    /// <summary>Does the war still pay those who matter? Invisible until the last turn.</summary>
    public double EliteCohesion { get; set; } = 100d;

    /// <summary>Pressure held under the lid. Repression converts discontent into this.</summary>
    public double LatentTension { get; set; }

    /// <summary>Repression effort, hides unrest and compounds tension.</summary>
    public double Repression { get; set; }

    /// <summary>The card-playing currency of V2, generated differently by each side.</summary>
    public double PoliticalCapital { get; set; }

    /// <summary>Corruption index, 0 to 100. V1 uses it as a single transmission dial.</summary>
    public double Corruption { get; set; }

    /// <summary>Structural level corruption returns to; cards and audits move it away.</summary>
    public double BaselineCorruption { get; set; } = 40d;

    /// <summary>Rail, bridges and depots, degraded by enemy deep strikes.</summary>
    public double LogisticsIntegrity { get; set; } = 1d;

    /// <summary>Foreign political will to keep funding this side, 0 to 100.</summary>
    public double ExternalWill { get; set; } = 100d;

    /// <summary>Consecutive turns below the force generation collapse threshold.</summary>
    public int TurnsBelowCollapseThreshold { get; set; }

    /// <summary>Share of the budget that buys nothing at all.</summary>
    public double BudgetLeakRate
    {
        get { return Math.Clamp(Corruption / 100d * 0.35d, 0d, 0.35d); }
    }

    /// <summary>Unit price inflation caused by rigged procurement.</summary>
    public double ProcurementInflation
    {
        get { return 1d + (Corruption / 100d * 0.6d); }
    }

    /// <summary>Regime collapse risk. A threshold, never a slope.</summary>
    public double RegimeStress
    {
        get
        {
            double visible = PopularDiscontent * (1d - (Repression * 0.7d));
            double elite = Math.Max(0d, 100d - EliteCohesion);

            // Elite fracture weighs double: regimes fall from inside, not from the street.
            return Math.Clamp((visible + (elite * 2d) + LatentTension) / 3.2d, 0d, 100d);
        }
    }

    /// <summary>Repression buys silence now and pays for it later, with interest.</summary>
    public void ApplyRepression()
    {
        if (Repression <= 0d)
        {
            return;
        }

        LatentTension = Math.Clamp(LatentTension + (PopularDiscontent * Repression * 0.25d), 0d, 100d);
        PopularDiscontent = Math.Max(0d, PopularDiscontent * (1d - (Repression * 0.35d)));
    }
}
