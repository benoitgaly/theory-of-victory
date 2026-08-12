using TheoryOfVictory.Core;

namespace TheoryOfVictory.Engine.Scenarios;

/// <summary>The single parameter that separates the two runs.</summary>
public enum SupportVariant
{
    /// <summary>Foreign support wavers but holds. Both sides regenerate; the front freezes.</summary>
    Holds = 0,

    /// <summary>Foreign support stops at turn 6. Nothing happens for two turns, then everything does.</summary>
    Collapses = 1,
}

/// <summary>
/// February 2022, sixteen quarterly turns. Starting from 2022 is the only way to check
/// the model finds the late-2023 ammunition crisis on its own instead of being told to.
/// All figures are working orders of magnitude, not sourced facts.
/// </summary>
public static class UkraineScenario
{
    /// <summary>Brent per quarter, 2022 Q1 to 2025 Q4. Estimation, not a sourced series.</summary>
    private static readonly double[] OilCalendar =
    [
        100d, 114d, 100d, 89d,
        82d, 78d, 86d, 84d,
        83d, 85d, 80d, 74d,
        75d, 67d, 68d, 65d,
    ];

    public static Scenario Build(SupportVariant variant)
    {
        List<EventCard> deck = CardLibrary.Load();

        Scenario scenario = new()
        {
            Code = variant == SupportVariant.Holds ? "ukraine_2022_a" : "ukraine_2022_b",
            Title = variant == SupportVariant.Holds
                ? "Déroulé A — le soutien tient"
                : "Déroulé B — le soutien s'arrête",
            Subtitle = variant == SupportVariant.Holds
                ? "Les deux camps régénèrent"
                : "Un seul paramètre change, au tour 6",
            Description = variant == SupportVariant.Holds
                ? "Le soutien extérieur vacille mais ne rompt jamais. Les deux camps parviennent à "
                    + "remplacer ce qu'ils consomment, et le front se fige. Personne ne gagne."
                : "Même situation de départ, mêmes cartes, même calendrier pétrolier. Seule différence : "
                    + "au tour 6, le flux gratuit s'arrête. Rien ne bouge pendant deux tours, puis tout cède.",
            StartYear = 2022,
            StartSeason = Season.Winter,
            TurnCount = 16,
            OilPriceCalendar = [.. OilCalendar],
            Invader = BuildRussia(),
            Defender = BuildUkraine(),
            Sectors = BuildSectors(),
            InvaderDoctrine = RussianDoctrine(),
            DefenderDoctrine = UkrainianDoctrine(),
            Deck = deck,
        };

        BuildCalendar(scenario, variant);
        BuildDoctrineShifts(scenario);
        return scenario;
    }

    private static Belligerent BuildRussia()
    {
        Belligerent russia = new()
        {
            Side = Side.Invader,
            Name = "Russie",
            Politics = new PoliticalState
            {
                Regime = RegimeType.Authoritarian,
                Morale = 74d,
                EliteCohesion = 86d,
                PopularDiscontent = 12d,
                Corruption = 46d,
                BaselineCorruption = 46d,
                Repression = 0.55d,
                LogisticsIntegrity = 0.92d,
                ExternalWill = 100d,
                PoliticalCapital = 12d,
            },
            Foreign = new ForeignSupport
            {
                Mode = SupportMode.Purchased,
                SupplyCeilingBillions = 1.5d,
                PricePremium = 1.35d,
            },
        };

        russia.Economy.HeadlineGdpBillions = 1800d;
        russia.Economy.ProductiveCapacityBillions = 1720d;
        russia.Economy.TreasuryBillions = 40d;
        russia.Economy.ReservesBillions = 310d;
        russia.Economy.FiscalCaptureRate = 0.088d;
        russia.Economy.WarBudgetCeilingShare = 0.021d;
        russia.Economy.CivilianGrowthPerTurn = 0.003d;
        russia.Economy.MilitarySpendingMultiplier = 0.6d;
        russia.Economy.CapitalDecayPerTurn = 0.007d;
        russia.Economy.OilExportCapacityMbd = 5.1d;

        // A vast, redundant, over-provisioned grid: near impossible to bring down.
        russia.Grid.NominalCapacityGw = 245d;
        russia.Grid.BaseDemandGw = 148d;
        russia.Grid.CivilianShareOfDemand = 0.5d;

        russia.Manpower.MobilisablePool = 4200d;
        russia.Manpower.AtFront = 190d;
        russia.Manpower.TargetForceSize = 420d;
        russia.Manpower.TrainingCapacityPerTurn = 105d;
        russia.Manpower.TrainingTurns = 1;
        russia.Manpower.ContractCostPerThousand = 0.021d;
        russia.Manpower.BaseGdpCostPerThousand = 0.031d;
        russia.Manpower.MarginalCostExponent = 1.4d;

        russia.Industry.SetCapacityPerTurn(ResourceKind.Weapons, 700d);
        russia.Industry.SetCapacityPerTurn(ResourceKind.StrikeDrones, 900d);
        russia.Industry.SetCapacityPerTurn(ResourceKind.Missiles, 130d);
        russia.Industry.SetCapacityPerTurn(ResourceKind.CheapInterceptors, 1400d);
        russia.Industry.SetCapacityPerTurn(ResourceKind.HeavyInterceptors, 95d);

        // Slow to adopt, but when it does, it does so at industrial scale.
        russia.Innovation.AdoptionSpeed = 0.65d;
        russia.Innovation.ScaleCeiling = 1.5d;
        russia.Innovation.DecayPerTurn = 0.12d;

        russia.AirDefence.Coverage = 0.72d;
        russia.AirDefence.RearShare = 0.55d;
        russia.AirDefence.CheapPurchaseShare = 0.55d;

        russia.Stock.Add(ResourceKind.Weapons, 900d);
        russia.Stock.Add(ResourceKind.Fuel, 260d);
        russia.Stock.Add(ResourceKind.Food, 190d);
        russia.Stock.Add(ResourceKind.StrikeDrones, 400d);
        russia.Stock.Add(ResourceKind.Missiles, 620d);
        russia.Stock.Add(ResourceKind.CheapInterceptors, 2600d);
        russia.Stock.Add(ResourceKind.HeavyInterceptors, 520d);

        return russia;
    }

    private static Belligerent BuildUkraine()
    {
        Belligerent ukraine = new()
        {
            Side = Side.Defender,
            Name = "Ukraine",
            Politics = new PoliticalState
            {
                Regime = RegimeType.Democratic,
                Morale = 92d,
                EliteCohesion = 78d,
                PopularDiscontent = 8d,
                Corruption = 56d,
                BaselineCorruption = 52d,
                Repression = 0.05d,
                LogisticsIntegrity = 0.9d,
                ExternalWill = 88d,
                PoliticalCapital = 8d,
            },
            Foreign = new ForeignSupport
            {
                Mode = SupportMode.Granted,
                PledgedPerTurnBillions = 4d,
                DisbursementRate = 1d,
                InKindShare = 0.62d,
                UnsustainableShare = 0.45d,
            },
        };

        ukraine.Economy.HeadlineGdpBillions = 196d;
        ukraine.Economy.ProductiveCapacityBillions = 188d;
        ukraine.Economy.TreasuryBillions = 8d;
        ukraine.Economy.ReservesBillions = 29d;
        ukraine.Economy.FiscalCaptureRate = 0.072d;
        ukraine.Economy.WarBudgetCeilingShare = 0.115d;
        ukraine.Economy.CivilianGrowthPerTurn = -0.02d;
        ukraine.Economy.MilitarySpendingMultiplier = 0.35d;
        ukraine.Economy.CapitalDecayPerTurn = 0.014d;
        ukraine.Economy.OilImportMbd = 0.24d;

        // A far smaller grid with a thin margin: the threshold is within reach of a drone campaign.
        ukraine.Grid.NominalCapacityGw = 36d;
        ukraine.Grid.BaseDemandGw = 15.5d;
        ukraine.Grid.WinterDemandMultiplier = 1.5d;
        ukraine.Grid.CivilianShareOfDemand = 0.6d;

        ukraine.Manpower.MobilisablePool = 2300d;
        ukraine.Manpower.AtFront = 250d;
        ukraine.Manpower.TargetForceSize = 430d;
        ukraine.Manpower.TrainingCapacityPerTurn = 62d;
        ukraine.Manpower.TrainingTurns = 1;
        ukraine.Manpower.ContractCostPerThousand = 0.013d;
        ukraine.Manpower.BaseGdpCostPerThousand = 0.062d;
        ukraine.Manpower.MarginalCostExponent = 1.55d;

        ukraine.Industry.SetCapacityPerTurn(ResourceKind.Weapons, 45d);
        ukraine.Industry.SetCapacityPerTurn(ResourceKind.StrikeDrones, 700d);
        ukraine.Industry.SetCapacityPerTurn(ResourceKind.Missiles, 25d);
        ukraine.Industry.SetCapacityPerTurn(ResourceKind.CheapInterceptors, 1100d);
        ukraine.Industry.SetCapacityPerTurn(ResourceKind.HeavyInterceptors, 12d);

        // Fast cycles, small structures: frequent jumps that are hard to scale.
        ukraine.Innovation.AdoptionSpeed = 2.1d;
        ukraine.Innovation.ScaleCeiling = 1.1d;
        ukraine.Innovation.DecayPerTurn = 0.16d;

        ukraine.AirDefence.Coverage = 0.55d;
        ukraine.AirDefence.RearShare = 0.62d;
        ukraine.AirDefence.CheapPurchaseShare = 0.6d;

        ukraine.Stock.Add(ResourceKind.Weapons, 700d);
        ukraine.Stock.Add(ResourceKind.Fuel, 120d);
        ukraine.Stock.Add(ResourceKind.Food, 140d);
        ukraine.Stock.Add(ResourceKind.StrikeDrones, 150d);
        ukraine.Stock.Add(ResourceKind.Missiles, 40d);
        ukraine.Stock.Add(ResourceKind.CheapInterceptors, 900d);
        ukraine.Stock.Add(ResourceKind.HeavyInterceptors, 260d);

        return ukraine;
    }

    private static List<FrontSector> BuildSectors()
    {
        return
        [
            new FrontSector
            {
                Code = "kharkiv",
                Name = "Kharkiv",
                TerrainMultiplier = 1.05d,
                Urbanisation = 0.35d,
                Width = 7,
                StrategicValue = 1.3d,
            },
            new FrontSector
            {
                Code = "kupiansk",
                Name = "Koupiansk",
                TerrainMultiplier = 1.1d,
                Urbanisation = 0.12d,
                Width = 6,
                StrategicValue = 1.1d,
            },
            new FrontSector
            {
                Code = "lyman",
                Name = "Lyman",
                TerrainMultiplier = 1.25d,
                Urbanisation = 0.08d,
                Width = 5,
                StrategicValue = 0.9d,
            },
            new FrontSector
            {
                Code = "bakhmut",
                Name = "Bakhmout — Tchassiv Iar",
                TerrainMultiplier = 1.15d,
                Urbanisation = 0.4d,
                Width = 5,
                StrategicValue = 1.2d,
            },
            new FrontSector
            {
                Code = "pokrovsk",
                Name = "Pokrovsk",
                TerrainMultiplier = 0.95d,
                Urbanisation = 0.22d,
                Width = 7,
                StrategicValue = 1.5d,
            },
            new FrontSector
            {
                Code = "vuhledar",
                Name = "Vouhledar",
                TerrainMultiplier = 1d,
                Urbanisation = 0.18d,
                Width = 5,
                StrategicValue = 0.9d,
            },
            new FrontSector
            {
                Code = "zaporizhzhia",
                Name = "Zaporijjia",
                TerrainMultiplier = 1.2d,
                Urbanisation = 0.15d,
                Width = 7,
                StrategicValue = 1.4d,
            },
            new FrontSector
            {
                Code = "kherson",
                Name = "Kherson — Dniepr",
                TerrainMultiplier = 1.6d,
                Urbanisation = 0.2d,
                Width = 6,
                StrategicValue = 1.2d,
            },
        ];
    }

    private static Doctrine RussianDoctrine()
    {
        Doctrine doctrine = new()
        {
            RecruitmentShare = 0.2d,
            WeaponsShare = 0.28d,
            StrikeVectorsShare = 0.12d,
            AirDefenceShare = 0.08d,
            IndustrialExpansionShare = 0.1d,
            InnovationShare = 0.04d,
            FortificationShare = 0.04d,
            AntiCorruptionShare = 0.01d,
            CivilianShare = 0.09d,
            ForeignPurchaseShare = 0.04d,
            RearDefenceShare = 0.55d,
            PrimaryStrikeTarget = StrikeTarget.PowerGrid,
            OffensivePosture = 0.62d,
            InnovationTacticalShare = 0.45d,
            InnovationStrikeShare = 0.35d,
            InnovationCounterShare = 0.2d,
        };

        // The attacker concentrates; that concentration is the whole source of a local ratio.
        doctrine.SectorEffort["kharkiv"] = 0.6d;
        doctrine.SectorEffort["kupiansk"] = 0.7d;
        doctrine.SectorEffort["lyman"] = 0.6d;
        doctrine.SectorEffort["bakhmut"] = 2.6d;
        doctrine.SectorEffort["pokrovsk"] = 3.2d;
        doctrine.SectorEffort["vuhledar"] = 0.7d;
        doctrine.SectorEffort["zaporizhzhia"] = 0.6d;
        doctrine.SectorEffort["kherson"] = 0.3d;
        return doctrine;
    }

    private static Doctrine UkrainianDoctrine()
    {
        Doctrine doctrine = new()
        {
            RecruitmentShare = 0.22d,
            WeaponsShare = 0.24d,
            StrikeVectorsShare = 0.14d,
            AirDefenceShare = 0.12d,
            IndustrialExpansionShare = 0.08d,
            InnovationShare = 0.07d,
            FortificationShare = 0.06d,
            AntiCorruptionShare = 0.02d,
            CivilianShare = 0.05d,
            ForeignPurchaseShare = 0d,
            RearDefenceShare = 0.62d,
            PrimaryStrikeTarget = StrikeTarget.Refining,
            OffensivePosture = 0.34d,
            InnovationTacticalShare = 0.5d,
            InnovationStrikeShare = 0.3d,
            InnovationCounterShare = 0.2d,
        };

        // The defender must hold everywhere: near-uniform, and reserves do the rest.
        doctrine.SectorEffort["kharkiv"] = 1d;
        doctrine.SectorEffort["kupiansk"] = 1d;
        doctrine.SectorEffort["lyman"] = 1d;
        doctrine.SectorEffort["bakhmut"] = 1.1d;
        doctrine.SectorEffort["pokrovsk"] = 1.1d;
        doctrine.SectorEffort["vuhledar"] = 1d;
        doctrine.SectorEffort["zaporizhzhia"] = 1d;
        doctrine.SectorEffort["kherson"] = 0.9d;
        return doctrine;
    }

    private static void BuildCalendar(Scenario scenario, SupportVariant variant)
    {
        List<ScheduledCard> calendar =
        [
            new ScheduledCard { Turn = 1, CardCode = "sanctions_package_1" },
            new ScheduledCard { Turn = 2, CardCode = "western_aid_opens" },
            new ScheduledCard { Turn = 3, CardCode = "himars_deep_strike" },
            new ScheduledCard { Turn = 4, CardCode = "partial_mobilisation" },
            new ScheduledCard { Turn = 4, CardCode = "counter_offensive_2022" },
            new ScheduledCard { Turn = 5, CardCode = "grid_campaign" },
            new ScheduledCard { Turn = 5, CardCode = "oil_price_cap" },
            new ScheduledCard { Turn = 6, CardCode = "component_embargo" },
            new ScheduledCard { Turn = 7, CardCode = "failed_offensive" },
            new ScheduledCard { Turn = 8, CardCode = "attention_elsewhere" },
            new ScheduledCard { Turn = 8, CardCode = "foreign_shells" },
            new ScheduledCard { Turn = 9, CardCode = "grid_campaign" },
            new ScheduledCard { Turn = 10, CardCode = "refinery_strikes" },
            new ScheduledCard { Turn = 10, CardCode = "licence_transfer" },
            new ScheduledCard { Turn = 11, CardCode = "harsh_winter" },
            new ScheduledCard { Turn = 12, CardCode = "fibre_optic_drones" },
            new ScheduledCard { Turn = 13, CardCode = "shadow_fleet_sanctions" },
            new ScheduledCard { Turn = 13, CardCode = "grid_campaign" },
            new ScheduledCard { Turn = 14, CardCode = "anticorruption_crisis" },
            new ScheduledCard { Turn = 15, CardCode = "elite_fracture" },
        ];

        if (variant == SupportVariant.Holds)
        {
            // The 2023-2024 ammunition crisis: suspended, then released. The model must find it alone.
            calendar.Add(new ScheduledCard { Turn = 9, CardCode = "aid_blocked" });
            calendar.Add(new ScheduledCard { Turn = 11, CardCode = "aid_unblocked" });
        }
        else
        {
            calendar.Add(new ScheduledCard { Turn = 6, CardCode = "aid_collapse" });
        }

        scenario.Calendar.AddRange(calendar);
    }

    private static void BuildDoctrineShifts(Scenario scenario)
    {
        // Autumn 2022: two thin sectors, concentrated hard. It worked because they were thin.
        Doctrine ukrainian2022 = UkrainianDoctrine();
        ukrainian2022.OffensivePosture = 0.62d;
        ukrainian2022.FortificationShare = 0.02d;
        ukrainian2022.SectorEffort["kharkiv"] = 2.8d;
        ukrainian2022.SectorEffort["kherson"] = 2.2d;
        ukrainian2022.SectorEffort["bakhmut"] = 0.6d;
        ukrainian2022.SectorEffort["pokrovsk"] = 0.6d;

        // Summer 2023: the same concentration against a fortified, mined, drone-covered sector.
        Doctrine ukrainianOffensive = UkrainianDoctrine();
        ukrainianOffensive.OffensivePosture = 0.66d;
        ukrainianOffensive.FortificationShare = 0.02d;
        ukrainianOffensive.SectorEffort["zaporizhzhia"] = 3.2d;
        ukrainianOffensive.SectorEffort["vuhledar"] = 1.6d;
        ukrainianOffensive.SectorEffort["kharkiv"] = 0.6d;
        ukrainianOffensive.SectorEffort["kherson"] = 0.5d;

        Doctrine ukrainianDefensive = UkrainianDoctrine();
        ukrainianDefensive.OffensivePosture = 0.26d;
        ukrainianDefensive.FortificationShare = 0.1d;

        Doctrine russianGrinding = RussianDoctrine();
        russianGrinding.OffensivePosture = 0.7d;
        russianGrinding.WeaponsShare = 0.32d;
        russianGrinding.PrimaryStrikeTarget = StrikeTarget.PowerGrid;

        scenario.DoctrineShifts.AddRange(
        [
            new DoctrineShift
            {
                Turn = 4,
                SideCode = Side.Defender.Code,
                Doctrine = ukrainian2022,
                Reason = "Contre-offensives de Kharkiv et Kherson",
            },
            new DoctrineShift
            {
                Turn = 5,
                SideCode = Side.Defender.Code,
                Doctrine = ukrainianDefensive,
                Reason = "Retour à la défensive et fortification",
            },
            new DoctrineShift
            {
                Turn = 7,
                SideCode = Side.Defender.Code,
                Doctrine = ukrainianOffensive,
                Reason = "Contre-offensive d'été",
            },
            new DoctrineShift
            {
                Turn = 8,
                SideCode = Side.Defender.Code,
                Doctrine = ukrainianDefensive,
                Reason = "L'offensive s'enlise, retour à la défense",
            },
            new DoctrineShift
            {
                Turn = 9,
                SideCode = Side.Invader.Code,
                Doctrine = russianGrinding,
                Reason = "Passage au grignotage soutenu",
            },
        ]);
    }
}
