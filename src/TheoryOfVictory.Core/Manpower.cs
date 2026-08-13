namespace TheoryOfVictory.Core;

/// <summary>
/// Everything here is counted in THOUSANDS OF MEN. <c>AtFront = 560</c> means 560 000 soldiers
/// deployed in the theatre of operations, front line and its immediate rear alike.
///
/// Soldiers are deliberately NOT treated as a consumed flow. Shells, fuel and rations are burnt
/// every quarter and their requirement is derived from the number of men in the line: the men
/// are the denominator of those requirements, never a numerator of their own. Saying that
/// infantry is "covered at 100 % of the need" is a category error — there is no exogenous need
/// for men to be compared against.
///
/// In the Liebig barrel the board draws, the infantry on the line is the SIZE of the barrel;
/// the three material flows are its staves, and the shortest stave sets the level. A manpower
/// deficit is therefore never a missing coverage: it is a smaller barrel, plus the cohesion
/// penalty a unit pays for fighting below establishment.
///
/// Three counts, never to be confused — the public debate conflates them constantly and the
/// gap between them is enormous:
///
/// <list type="number">
/// <item><c>TotalUnderArms</c> — everyone in uniform. The figure leaders quote.</item>
/// <item><c>AtFront</c> — the grouping committed to the theatre. What consumes.</item>
/// <item><c>InContact</c> — the combat units on the line. What fights, and what runs out.</item>
/// </list>
/// </summary>
public sealed class Manpower
{
    /// <summary>Thousands of men physically available for mobilisation, the demographic ceiling.</summary>
    public double MobilisablePool { get; set; }

    /// <summary>Thousands of recruits in training, one entry per turn remaining.</summary>
    public Queue<double> TrainingPipeline { get; } = new();

    /// <summary>Thousands of men deployed in the theatre of operations.</summary>
    public double AtFront { get; set; }

    /// <summary>
    /// Thousands of men the command tries to keep in the theatre — the establishment, and the
    /// reference the manning ratio is measured against. It is a structural figure: it never
    /// enters the minimum rule as a "need to be covered".
    /// </summary>
    public double TargetForceSize { get; set; }

    /// <summary>
    /// Thousands of men the establishment grows by each quarter as the war institutionalises
    /// its own recruitment. Both armies grew steadily through the war rather than jumping once.
    /// </summary>
    public double TargetForceGrowthPerTurn { get; set; }

    /// <summary>Thousands of men the establishment never exceeds — the observed wartime maximum.</summary>
    public double TargetForceCeiling { get; set; } = double.PositiveInfinity;

    /// <summary>Maximum thousands of recruits the training system can absorb per quarter.</summary>
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

    /// <summary>
    /// Quarterly cost in billions of keeping a thousand men in the field — pay, bonuses, upkeep.
    /// The largest single line of any war budget, and the one that makes a revenue collapse
    /// reach the front: an army you cannot pay is an army that shrinks.
    /// </summary>
    public double UpkeepCostPerThousand { get; set; }

    /// <summary>
    /// Thousands of men the treasury can actually keep in the field this turn. Infinite while
    /// the money holds; the binding constraint the quarter it stops.
    /// </summary>
    public double PayableForceSize { get; set; } = double.PositiveInfinity;

    /// <summary>
    /// The establishment the treasury can afford: what command wants, capped by what it can pay
    /// for. Ordering men you cannot pay does not put them in the line.
    /// </summary>
    public double EffectiveForceSize
    {
        get { return Math.Min(TargetForceSize, PayableForceSize); }
    }

    /// <summary>
    /// Thousands of men actually held in the theatre this quarter: those present, capped by
    /// those the treasury can pay. Everyone counted here eats, burns fuel and has to be moved,
    /// so this is the denominator of every material requirement — but it is NOT what fights.
    /// </summary>
    public double ForceInLine
    {
        get { return Math.Min(AtFront, PayableForceSize); }
    }

    /// <summary>
    /// Men under arms outside the theatre, per man inside it: training establishment, air
    /// defence of the cities, navy, home districts, and — on the Russian side — everything
    /// this war never committed. A tail, not a reserve: none of it holds ground.
    /// </summary>
    public double RearEstablishmentRatio { get; set; }

    /// <summary>
    /// Share of the theatre force serving in the combat units that hold the line. The rest is
    /// the tail that makes the theatre force possible without ever standing in a trench.
    /// </summary>
    public double ContactShare { get; set; } = 0.5d;

    /// <summary>
    /// Thousands of men in uniform, everything included. The figure political leaders quote,
    /// and the one that means least: an army of a million can run out of infantry.
    /// </summary>
    public double TotalUnderArms
    {
        get { return (AtFront * (1d + RearEstablishmentRatio)) + InTraining; }
    }

    /// <summary>
    /// Thousands of men in the combat units on the line of contact. THIS is the size of the
    /// barrel — the only one of the three counts that holds ground, and the one both armies
    /// ran out of. Combat power scales on it linearly: twice the infantry at equal material
    /// coverage is twice the power.
    ///
    /// It follows that growing the tail is worse than useless. More men in the theatre with
    /// the same men on the line raises the shells, fuel and rations the front demands without
    /// adding an ounce of power — which is the Ukrainian crisis of 2024-2026, in one ratio.
    /// </summary>
    public double InContact
    {
        get { return ForceInLine * ContactShare; }
    }

    /// <summary>Everyone ever taken out of the economy, drives the marginal cost.</summary>
    public double TotalMobilisedEver { get; set; }

    public double CumulativeLosses { get; set; }

    public double InTraining
    {
        get { return TrainingPipeline.Sum(); }
    }

    /// <summary>
    /// Men present over establishment. A structural reading of how filled the order of battle
    /// is — NOT a coverage ratio, and never a stave of the barrel: it never caps the material
    /// flows and the material flows never cap it.
    /// </summary>
    public double ManningRatio
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

    /// <summary>Share of the men present the treasury can actually pay for this quarter.</summary>
    public double PayRatio
    {
        get
        {
            if (AtFront <= 0.01d)
            {
                return 1d;
            }

            return Math.Min(1.5d, PayableForceSize / AtFront);
        }
    }

    /// <summary>
    /// What an army loses by fighting below establishment. Understrength units hold the same
    /// ground with thinner lines, fewer reliefs and no reserve, so they fight worse than their
    /// headcount alone would say. This is how a manpower deficit stays painful without being
    /// dressed up as a missing "coverage": the barrel is smaller AND it leaks.
    /// </summary>
    public double CohesionFactor
    {
        get
        {
            double manning = Math.Min(1d, ManningRatio);
            return 0.7d + (0.3d * Math.Max(0d, manning));
        }
    }

    /// <summary>
    /// Grows the establishment one quarter, up to the wartime ceiling. Called once per turn:
    /// the war institutionalises its own recruitment rather than settling on a fixed size.
    /// </summary>
    public void GrowEstablishment()
    {
        if (TargetForceGrowthPerTurn <= 0d)
        {
            return;
        }

        TargetForceSize = Math.Min(TargetForceCeiling, TargetForceSize + TargetForceGrowthPerTurn);
    }

    /// <summary>GDP cost in billions of taking <paramref name="thousands"/> more men out of the economy.</summary>
    public double MarginalGdpCost(double thousands)
    {
        if (thousands <= 0d)
        {
            return 0d;
        }

        // The first wave is nearly free; later ones take the men who build the shells.
        // One "wave" is 300 k men — the size of the Russian partial mobilisation of autumn 2022.
        double wavesTaken = Math.Max(1d, TotalMobilisedEver / 300d);
        double severity = Math.Pow(wavesTaken, MarginalCostExponent - 1d);
        return thousands * BaseGdpCostPerThousand * severity;
    }
}
