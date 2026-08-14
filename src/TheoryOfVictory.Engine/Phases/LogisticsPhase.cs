using TheoryOfVictory.Core;

namespace TheoryOfVictory.Engine.Phases;

/// <summary>
/// Phase 5. What leaves the depot is not what arrives. Corruption leaks it,
/// enemy interdiction cuts it, and what is left is all the front ever sees.
/// </summary>
public sealed class LogisticsPhase : ITurnPhase
{
    /// <summary>
    /// Thousands of rounds one thousand men burn in a quarter of sustained combat — so, plainly,
    /// rounds per man per quarter. The men set the requirement; the requirement never sets the men.
    ///
    /// Derived, not guessed (see docs/design/04-calibration-effectifs.md). Russian artillery fired
    /// on the order of 10 000 rounds a day through 2024-2025 with roughly 650 000 men in theatre:
    /// 10 000 × 91 ÷ 650 ≈ 1,40 round per man per quarter, at the offensive posture Russia held
    /// throughout. This constant is read BEFORE the intensity multiplier, which stands at 1,12 for
    /// that posture, so the anchor gives 1,40 ÷ 1,12 ≈ 1,25. Ukraine, rationed, ran at 2 000 to
    /// 6 000 rounds a day for a comparable grouping, i.e. 0,3 to 0,9 — which the model reproduces
    /// through its defensive posture and its coverage, not through a second constant.
    ///
    /// The opening months of 2022 are an outlier at up to 60 000 Russian rounds a day and are
    /// deliberately not fitted: the grinding regime is what nineteen of the twenty simulated
    /// quarters actually lived in. Estimation, ± 40 %.
    /// </summary>
    public const double WeaponsPerThousandMen = 1.25d;

    /// <summary>
    /// Kilotonnes of fuel per thousand men per quarter — about 6 kg a day and a man, which is
    /// the order of magnitude for a mechanised force on short supply lines. Estimation, unsourced.
    /// </summary>
    public const double FuelPerThousandMen = 0.55d;

    /// <summary>
    /// Kilotonnes of rations and water per thousand men per quarter — about 4,6 kg a day and a
    /// man. Unlike shells, this one does not vary with the intensity of the fighting. Estimation.
    /// </summary>
    public const double FoodPerThousandMen = 0.42d;

    public string Name
    {
        get { return "Logistics"; }
    }

    public void Execute(TurnContext context)
    {
        foreach (Side side in Side.All)
        {
            Belligerent belligerent = context.State.Get(side);
            Doctrine doctrine = context.DoctrineFor(side);

            double intensity = 0.7d + (doctrine.OffensivePosture * 0.6d);

            // Thousands of men in the theatre — the denominator of every requirement below.
            double men = belligerent.Manpower.AtFront;

            Deliver(belligerent, ResourceKind.Weapons, men * WeaponsPerThousandMen * intensity * belligerent.Innovation.WeaponDemandMultiplier);
            Deliver(belligerent, ResourceKind.Fuel, men * FuelPerThousandMen * intensity);
            Deliver(belligerent, ResourceKind.Food, men * FoodPerThousandMen);
        }
    }

    private void Deliver(Belligerent belligerent, ResourceKind kind, double required)
    {
        belligerent.NeedThisTurn[kind.Code] = required;

        if (required <= 0d)
        {
            belligerent.SetDelivered(kind, 0d);
            belligerent.CoverageThisTurn[kind.Code] = 1d;
            return;
        }

        // A depot exists to be drawn on harder than the front consumes: what leaks on the
        // way has to leave the warehouse too. Capping the draw at the need would make the
        // pile decorative — and a stock that cannot absorb a bad quarter buys no latency,
        // which is precisely the latency the whole demonstration rests on.
        double leavingDepot = required / belligerent.TransmissionRate;
        double drawn = belligerent.Stock.Consume(kind, leavingDepot);
        double delivered = drawn * belligerent.TransmissionRate;

        belligerent.SetDelivered(kind, delivered);
        belligerent.CoverageThisTurn[kind.Code] = Math.Clamp(delivered / required, 0d, 1.6d);
    }
}
