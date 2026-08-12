namespace TheoryOfVictory.Core;

/// <summary>
/// Two measures on purpose: headline GDP is inflated by war spending and rises,
/// while productive capacity is eroded and falls. The gap is the war economy trap.
/// </summary>
public sealed class Economy
{
    /// <summary>Headline GDP in billions, the figure the dashboard shows first.</summary>
    public double HeadlineGdpBillions { get; set; }

    /// <summary>Sustainable productive capacity, the figure that actually constrains.</summary>
    public double ProductiveCapacityBillions { get; set; }

    public double TreasuryBillions { get; set; }

    /// <summary>Sovereign reserves burnt when spending exceeds revenue.</summary>
    public double ReservesBillions { get; set; }

    public double DebtBillions { get; set; }

    /// <summary>Share of GDP the state can capture per turn.</summary>
    public double FiscalCaptureRate { get; set; } = 0.09d;

    /// <summary>
    /// Ceiling on what can be poured into the war in one quarter, as a share of GDP.
    /// Without it, unspent cash compounds and the war effort drifts past the whole economy.
    /// </summary>
    public double WarBudgetCeilingShare { get; set; } = 0.03d;

    /// <summary>
    /// Share of ordinary tax revenue the state can divert to the war. The rest of the
    /// war chest has to come from oil, aid or reserves — which is why the barrel decides.
    /// </summary>
    public double MilitaryFiscalShare { get; set; } = 0.1d;

    /// <summary>Share of sovereign reserves that can be burnt in a single quarter.</summary>
    public double ReserveDrawRate { get; set; } = 0.06d;

    /// <summary>What this turn's revenue actually makes fundable, computed at the revenue phase.</summary>
    public double WarFundableBillions { get; set; }

    /// <summary>Baseline civilian growth per turn, before war effects.</summary>
    public double CivilianGrowthPerTurn { get; set; } = 0.004d;

    /// <summary>Short-run GDP multiplier of military spending: the keynesian illusion.</summary>
    public double MilitarySpendingMultiplier { get; set; } = 0.55d;

    public double InflationRate { get; set; }

    /// <summary>Capital consumed each turn without civilian reinvestment.</summary>
    public double CapitalDecayPerTurn { get; set; } = 0.006d;

    /// <summary>Net oil exports in million barrels per day. Invader only.</summary>
    public double OilExportCapacityMbd { get; set; }

    /// <summary>Refining and terminal integrity, hit by deep strikes, repaired over time.</summary>
    public double RefiningIntegrity { get; set; } = 1d;

    public double RefiningRepairPerTurn { get; set; } = 0.4d;

    /// <summary>Oil imported per turn in million barrels per day. Defender pays the market.</summary>
    public double OilImportMbd { get; set; }

    public double LastTurnRevenueBillions { get; set; }

    public double LastTurnOilRevenueBillions { get; set; }

    public double LastTurnMilitarySpendBillions { get; set; }

    /// <summary>Reserves actually liquidated this turn to keep the war effort at its ceiling.</summary>
    public double LastTurnReserveDrawBillions { get; set; }

    /// <summary>War effort the ordinary revenue of the turn funds on its own, reserves excluded.</summary>
    public double OrdinaryWarFundingBillions { get; set; }

    /// <summary>
    /// Quarters of reserve left at the current burn rate. The countdown the barrel drives:
    /// once it reaches zero the war has to live on what it earns.
    /// </summary>
    public double ReserveQuartersLeft
    {
        get
        {
            if (LastTurnReserveDrawBillions <= 0.01d)
            {
                return double.PositiveInfinity;
            }

            return ReservesBillions / LastTurnReserveDrawBillions;
        }
    }

    /// <summary>
    /// Share of the war effort this quarter's revenue could not fund. Above zero the
    /// apparatus starts noticing that the war has stopped paying.
    /// </summary>
    public double FundingGap
    {
        get
        {
            double ceiling = HeadlineGdpBillions * WarBudgetCeilingShare;
            if (ceiling <= 0d)
            {
                return 0d;
            }

            return Math.Clamp(1d - (WarFundableBillions / ceiling), 0d, 1d);
        }
    }

    /// <summary>Share of GDP going to the war, the number the history books quote.</summary>
    public double WarEffortShare
    {
        get
        {
            if (HeadlineGdpBillions <= 0d)
            {
                return 0d;
            }

            return LastTurnMilitarySpendBillions / HeadlineGdpBillions;
        }
    }
}
