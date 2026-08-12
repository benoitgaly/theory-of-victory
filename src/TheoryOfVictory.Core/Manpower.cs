namespace TheoryOfVictory.Core;

/// <summary>
/// Soldiers are a flow before being a stock. The marginal GDP cost rises with every
/// wave: the first is nearly free, the third takes the men who build the shells.
/// </summary>
public sealed class Manpower
{
    /// <summary>Men physically available for mobilisation, the demographic ceiling.</summary>
    public double MobilisablePool { get; set; }

    /// <summary>Recruits in training, one entry per turn remaining.</summary>
    public Queue<double> TrainingPipeline { get; } = new();

    public double AtFront { get; set; }

    /// <summary>Force the command tries to sustain, denominator of infantry coverage.</summary>
    public double TargetForceSize { get; set; }

    /// <summary>Maximum recruits the training system can absorb per turn.</summary>
    public double TrainingCapacityPerTurn { get; set; }

    /// <summary>Turns spent in training before reaching the line.</summary>
    public int TrainingTurns { get; set; } = 1;

    /// <summary>Degraded when recruitment is rushed; raises losses, which forces more recruitment.</summary>
    public double TrainingQuality { get; set; } = 1d;

    /// <summary>Billions of GDP lost per thousand men taken out of the economy, first wave.</summary>
    public double BaseGdpCostPerThousand { get; set; }

    /// <summary>Above one, each further wave costs more GDP than the last.</summary>
    public double MarginalCostExponent { get; set; } = 1.35d;

    /// <summary>Cost in billions of recruiting a thousand men under contract, bonuses included.</summary>
    public double ContractCostPerThousand { get; set; }

    /// <summary>Everyone ever taken out of the economy, drives the marginal cost.</summary>
    public double TotalMobilisedEver { get; set; }

    public double CumulativeLosses { get; set; }

    public double InTraining
    {
        get { return TrainingPipeline.Sum(); }
    }

    public double InfantryCoverage
    {
        get
        {
            if (TargetForceSize <= 0d)
            {
                return 1d;
            }

            return AtFront / TargetForceSize;
        }
    }

    /// <summary>GDP cost in billions of taking <paramref name="thousands"/> more men out of the economy.</summary>
    public double MarginalGdpCost(double thousands)
    {
        if (thousands <= 0d)
        {
            return 0d;
        }

        // The first wave is nearly free; later ones take the men who build the shells.
        double wavesTaken = Math.Max(1d, TotalMobilisedEver / 300d);
        double severity = Math.Pow(wavesTaken, MarginalCostExponent - 1d);
        return thousands * BaseGdpCostPerThousand * severity;
    }
}
