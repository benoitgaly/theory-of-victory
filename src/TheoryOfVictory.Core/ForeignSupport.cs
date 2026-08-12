namespace TheoryOfVictory.Core;

/// <summary>How a side gets foreign materiel. The asymmetry is the point of the game.</summary>
public enum SupportMode
{
    /// <summary>Given: free, abundant, and hanging by a political thread.</summary>
    Granted = 0,

    /// <summary>Bought: expensive, and it never stops as long as the cash lasts.</summary>
    Purchased = 1,
}

/// <summary>
/// A free flow that can vanish, against a paid flow that never vanishes.
/// Ukraine is granted; Russia buys. Cutting the first takes a day, cutting
/// the second means going after the money, therefore through oil.
/// </summary>
public sealed class ForeignSupport
{
    public required SupportMode Mode { get; init; }

    /// <summary>Pledged value per turn in billions. Granted side only.</summary>
    public double PledgedPerTurnBillions { get; set; }

    /// <summary>Share of the pledge actually disbursed, driven by external political will.</summary>
    public double DisbursementRate { get; set; } = 1d;

    /// <summary>Ceiling the suppliers can physically deliver per turn. Purchased side only.</summary>
    public double SupplyCeilingBillions { get; set; }

    /// <summary>Price premium paid for sanctioned, discreet supply.</summary>
    public double PricePremium { get; set; } = 1.3d;

    /// <summary>Rises with every purchase, paid later in concessions.</summary>
    public double Dependency { get; set; }

    /// <summary>Share of granted materiel that cannot be maintained without the donor.</summary>
    public double UnsustainableShare { get; set; }

    /// <summary>Aid delivered as materiel, not cash: it bypasses domestic capacity entirely.</summary>
    public double InKindShare { get; set; } = 0.6d;

    /// <summary>Tightens when corruption rises, reducing the flow. Granted side only.</summary>
    public double Conditionality { get; set; }

    public double EffectiveGrantBillions
    {
        get
        {
            if (Mode != SupportMode.Granted)
            {
                return 0d;
            }

            return PledgedPerTurnBillions * DisbursementRate * Math.Clamp(1d - Conditionality, 0d, 1d);
        }
    }

    /// <summary>Materiel obtained by spending <paramref name="budgetBillions"/> abroad.</summary>
    public double Purchase(double budgetBillions)
    {
        if (Mode != SupportMode.Purchased || budgetBillions <= 0d)
        {
            return 0d;
        }

        double delivered = Math.Min(budgetBillions / PricePremium, SupplyCeilingBillions);
        Dependency = Math.Clamp(Dependency + (delivered * 0.012d), 0d, 1d);
        return delivered;
    }
}
