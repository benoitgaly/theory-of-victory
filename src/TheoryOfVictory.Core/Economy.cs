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
    /// Ceiling on what can be poured into the war in ONE QUARTER, expressed as a share of
    /// ANNUAL GDP. The two periods differ on purpose, and reading them as one is the trap:
    /// 0,038 here is not a war effort of 3,8 % of GDP, it is four quarters of 3,8 %, so
    /// roughly 15 % of GDP a year. Use <see cref="AnnualWarEffortShareOfGdp"/> whenever the
    /// figure is meant for a human — never this one.
    ///
    /// Without the ceiling, unspent cash compounds and the war effort drifts past the whole
    /// economy.
    /// </summary>
    public double WarBudgetCeilingShare { get; set; } = 0.03d;

    /// <summary>
    /// The war effort as a year-on-year share of GDP — the number history books quote, and
    /// the only one fit to be displayed. Four quarterly ceilings over one annual GDP.
    /// </summary>
    public double AnnualWarEffortShareOfGdp
    {
        get { return WarBudgetCeilingShare * 4d; }
    }

    /// <summary>
    /// Share of ordinary tax revenue the state can divert to the war. The rest of the
    /// war chest has to come from oil, aid or reserves — which is why the barrel decides.
    /// </summary>
    public double MilitaryFiscalShare { get; set; } = 0.1d;

    /// <summary>Share of sovereign reserves that can be burnt in a single quarter.</summary>
    public double ReserveDrawRate { get; set; } = 0.06d;

    /// <summary>What this turn's revenue actually makes fundable, computed at the revenue phase.</summary>
    public double WarFundableBillions { get; set; }

    /// <summary>
    /// Share of the productive capacity that is INDUSTRY of any kind — the plant, before
    /// anything is carved out of it. Services, farmland and trade are outside it and always
    /// were: this post has never claimed to hold the whole economy.
    /// </summary>
    public double IndustrialShareOfCapacity { get; set; }

    /// <summary>
    /// Share of the productive capacity produced by the energy sector. Carved out of the
    /// industrial base, because the band already draws it twice over — the power plants and
    /// the oil — and a barrel counted in both places inflates the balance sheet on both sides.
    /// </summary>
    public double EnergyShareOfCapacity { get; set; }

    /// <summary>
    /// Whatever the war takes, the civilian plant stops getting. This is the share it may
    /// never fall below: an economy at war does not stop feeding its people, and a post that
    /// can reach zero would say it does.
    /// </summary>
    public double CivilianFloorShare { get; set; } = 0.06d;

    /// <summary>
    /// What a country already spent on its army while at peace. Carving the WHOLE war effort
    /// out of the civilian base would say that an army costs a country its industry, which is
    /// false: only the surplus over the peacetime line is a war economy.
    /// </summary>
    public double PeacetimeMilitaryShareOfGdp { get; set; }

    /// <summary>
    /// What the civilian plant is worth this turn, in billions: the industrial base, less the
    /// energy sector the band already draws twice over, less the part of the war effort the
    /// economy has to fund out of its own production.
    ///
    /// That last term is the whole point. A war paid for by somebody else's money, by the rent
    /// on a barrel or by a sovereign fund built in peacetime costs the civilian base NOTHING —
    /// which is exactly why both belligerents can hold four years without their people going
    /// hungry, and exactly why the day the aid stops or the barrel falls is the day it starts
    /// showing up on this row. The band stops asserting that the two economies are alike and
    /// starts showing which one is being eaten.
    /// </summary>
    public double CivilianCapacityBillions(double aidBillions)
    {
        double funded = Math.Max(aidBillions, 0d)
            + Math.Max(LastTurnOilRevenueBillions, 0d)
            + Math.Max(LastTurnReserveDrawBillions, 0d);

        double borne = Math.Max(LastTurnMilitarySpendBillions - funded, 0d) * 4d
            - (HeadlineGdpBillions * PeacetimeMilitaryShareOfGdp);

        double warShare = HeadlineGdpBillions > 0d ? Math.Max(borne, 0d) / HeadlineGdpBillions : 0d;
        double left = IndustrialShareOfCapacity - EnergyShareOfCapacity - warShare;
        return ProductiveCapacityBillions * Math.Max(left, CivilianFloorShare);
    }

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

    /// <summary>
    /// Share of GDP actually going to the war, annualised — the number the history books
    /// quote. The spend is a quarter's worth and the GDP is a year's worth, so the quarterly
    /// figure has to be multiplied by four before the two can be divided. Without that, the
    /// ratio reads four times too low and would put Russia at 2 % of GDP.
    /// </summary>
    public double WarEffortShare
    {
        get
        {
            if (HeadlineGdpBillions <= 0d)
            {
                return 0d;
            }

            return LastTurnMilitarySpendBillions * 4d / HeadlineGdpBillions;
        }
    }
}
