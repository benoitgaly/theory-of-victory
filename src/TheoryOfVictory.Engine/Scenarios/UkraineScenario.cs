using TheoryOfVictory.Core;

namespace TheoryOfVictory.Engine.Scenarios;

/// <summary>What the West chooses to do with the cards it holds.</summary>
public enum SupportVariant
{
    /// <summary>The cards are played: the money is cut, and the war stops paying those who run it.</summary>
    Resolve = 0,

    /// <summary>Support wavers but holds at today's level. Both sides regenerate; the front freezes.</summary>
    Holds = 1,

    /// <summary>Support stops at turn 6. Nothing happens for two turns, then everything does.</summary>
    Collapses = 2,
}

/// <summary>
/// Autumn 2021 to winter 2028, twenty-six quarterly turns. Opening a quarter before the
/// invasion is what shows a war being generated before it is fought; starting there is also
/// the only way to check the model finds the late-2023 ammunition crisis on its own instead
/// of being told to. All figures are working orders of magnitude, not sourced facts.
/// </summary>
public static class UkraineScenario
{
    /// <summary>
    /// Brent per quarter, 2021 Q4 to 2028 Q1. Working estimates, not a sourced series;
    /// everything from 2026 Q4 on is an assumption, not an observation.
    ///
    /// The first entry is the prologue quarter — the autumn the forces were massed, when
    /// the barrel was already paying for what was being prepared.
    /// </summary>
    private static readonly double[] OilCalendar =
    [
        80d,
        100d, 114d, 100d, 89d,
        82d, 78d, 86d, 84d,
        83d, 85d, 80d, 74d,
        75d, 67d, 68d, 65d,
        66d, 64d, 63d,
        62d, 61d, 60d, 60d,
        61d, 62d,
    ];

    public static Scenario Build(SupportVariant variant)
    {
        List<EventCard> deck = CardLibrary.Load();

        Scenario scenario = new()
        {
            Code = variant switch
            {
                SupportVariant.Resolve => "ukraine_2022_resolve",
                SupportVariant.Holds => "ukraine_2022_holds",
                _ => "ukraine_2022_collapse",
            },
            Title = variant switch
            {
                SupportVariant.Resolve => "L'Occident joue ses cartes",
                SupportVariant.Holds => "Le soutien tient, sans plus",
                _ => "Le soutien s'arrête",
            },
            Subtitle = variant switch
            {
                SupportVariant.Resolve => "L'Ukraine l'emporte",
                SupportVariant.Holds => "Front figé, personne ne gagne",
                _ => "L'Ukraine cède",
            },
            Description = variant switch
            {
                SupportVariant.Resolve => "On ne prend pas de terrain : on coupe la caisse. Embargo sur les "
                    + "composants, campagne trimestrielle sur le raffinage, baril effondré, aide rendue "
                    + "prévisible. La guerre cesse de payer ceux qui la tiennent, et c'est l'arrière russe "
                    + "qui cède avant le front ukrainien.",
                SupportVariant.Holds => "Le soutien extérieur vacille mais ne rompt jamais, sans jamais non "
                    + "plus s'intensifier. Les deux camps remplacent ce qu'ils consomment et le front se fige. "
                    + "L'égalité industrielle produit l'enlisement, pas la paix.",
                _ => "Même départ, mêmes cartes, même calendrier pétrolier. Seule différence : au tour 6, le "
                    + "flux gratuit s'arrête. Rien ne bouge pendant deux tours, puis tout cède d'un bloc.",
            },
            // The game opens a quarter BEFORE the invasion: autumn 2021 is pure force
            // generation, without a shot fired, and it is the most demonstrative turn of the
            // whole run. Starting in autumn keeps every winter on turns 2, 6, 10, 14, 18 and 22,
            // so the campaigns against the grid still land in the quarter that hurts.
            StartYear = 2021,
            StartSeason = Season.Autumn,

            // One prologue, nineteen quarters of war to the present, three more of strangulation
            // before the regime gives at T23, then three for the aftermath. Twenty-six is bounded
            // at BOTH ends and there is no room either way: the asphyxiation run reaches its
            // armistice on T26 — verified by running it at 28, where it still stops on 26 — and
            // the frozen-front run gives at T27, the defender finally yielding. A longer game
            // would end the demonstration on the wrong lesson.
            TurnCount = 26,
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

        // The war wants more than the ordinary budget funds — that is what makes it a war
        // effort rather than a line item, and it is what puts the sovereign fund on a clock
        // from the first turn. Set it under ordinary funding and the barrel is decorative.
        //
        // Lowered from 0,038 to 0,028 to carry the strangulation three quarters further. The
        // ceiling drives two things at once: how fast the reserve is drained, and how hard the
        // funding gap gnaws at elite cohesion. At 0,038 the army stopped being paid around T19
        // and the run ended in a MILITARY collapse — the front deciding the war, which is the
        // one thing this scenario must never say. At 0,028 the regime is the first thing to
        // give, at T23, and it gives because the quarter it has to fund is the one it can no
        // longer fund. 0,027 was tried and breaks the frozen-front run: the invader then holds
        // comfortably enough to win it. The figure also reads better against the sources —
        // 0,028 × 1 800 Md is about 202 Md a year of ceiling, against the ~190 Md estimated for
        // 2025, where 0,038 implied 274 Md.
        russia.Economy.WarBudgetCeilingShare = 0.028d;
        russia.Economy.MilitaryFiscalShare = 0.085d;
        russia.Economy.ReserveDrawRate = 0.12d;
        russia.Economy.CivilianGrowthPerTurn = 0.003d;
        russia.Economy.MilitarySpendingMultiplier = 0.6d;
        russia.Economy.CapitalDecayPerTurn = 0.007d;
        russia.Economy.OilExportCapacityMbd = 5.1d;

        // 18 % of the outstanding damage repaired per quarter, the figure the realism audit
        // asked for and the sources support: distillation columns are rebuilt with Western
        // equipment nobody will sell any more, and the IEA expects Russian refining runs to
        // stay depressed into mid-2026.
        //
        // It had been left at the engine default of 40 % because it was the one change that
        // moved the Russian collapse a quarter earlier with no compensation available. That
        // constraint is gone: the collapse turn is now set by the calendar — by when the final
        // cards fall — and the war-budget ceiling above absorbs what the slower repair adds.
        // The concession documented in 04-calibration-effectifs.md §12 can be retired.
        russia.Economy.RefiningRepairPerTurn = 0.18d;

        // A vast, redundant, over-provisioned grid: near impossible to bring down.
        russia.Grid.NominalCapacityGw = 245d;
        russia.Grid.BaseDemandGw = 148d;
        russia.Grid.CivilianShareOfDemand = 0.5d;

        // ── Effectifs, en milliers d'hommes ────────────────────────────────────────────────
        // Sources et incertitudes détaillées : docs/design/04-calibration-effectifs.md.

        // Men of military age the state could realistically put under arms over the war, not
        // the 25 M the 2022 mobilisation decree nominally covered. ESTIMATION, no usable source:
        // the war has absorbed ≈ 1,6 M contract signings and mobilised men in four years without
        // visibly emptying the reservoir, so a working ceiling of 4,2 M is deliberately generous.
        russia.Industry.DepotQuartersHeld = 6d;
        russia.Manpower.MobilisablePool = 4200d;

        // The invasion grouping massed on the Ukrainian border in February 2022: Western
        // estimates converge on ≈ 190 000. Range given by the sources: 150 000 – 190 000.
        russia.Manpower.AtFront = 190d;

        // Russia did not plan a long war: at the outset the establishment IS the invasion force.
        // What follows is the observed trajectory of the grouping in theatre — 523 000 in 2023,
        // 667 000 in 2024, 723 000 in 2025 (Janis Kluge, from Russian budget salary top-ups),
        // 721 300 in June 2026 (Syrskyi). +28 000 a quarter plus the autumn 2022 mobilisation
        // reproduces that curve to within 7 %; the ceiling is the observed maximum.
        russia.Manpower.TargetForceSize = 190d;
        russia.Manpower.TargetForceGrowthPerTurn = 28d;
        russia.Manpower.TargetForceCeiling = 720d;

        // Men under arms outside Ukraine per man inside it. Russian armed forces run at roughly
        // 1,3 M against a theatre grouping of ≈ 720 000: strategic forces, the eastern military
        // district, the navy and the training establishment were never committed to this war.
        russia.Manpower.RearEstablishmentRatio = 0.7d;

        // Share of the theatre grouping serving in the units that hold the line. EXTRAPOLATION:
        // Russia publishes no teeth-to-tail breakdown at all, so this carries the Ukrainian
        // anchor (≈ 300 000 on the line out of a theatre grouping of ≈ 550 000) rather than a
        // figure of its own. Deliberately identical on both sides: inventing an asymmetry here
        // would hand one army an advantage no source supports. Range 0,45 – 0,65.
        russia.Manpower.ContactShare = 0.55d;

        // ≈ 35 000 contract signings a month: 440 000–450 000 in 2024 (≈ 1 200 a day) and
        // ≈ 420 000 in 2025 per the Russian defence ministry, falling to ≈ 800 a day in 2026.
        russia.Manpower.TrainingCapacityPerTurn = 105d;
        russia.Manpower.TrainingTurns = 1;

        // 21 000 $ to sign a man on. Real Russian sign-on bonuses ran at 1,5 to 3 M roubles in
        // 2024-2025, i.e. 17 000 – 35 000 $ — the model sits mid-range.
        russia.Manpower.ContractCostPerThousand = 0.021d;

        // Pay and bonuses are the biggest line of the Russian war budget — deliberately so:
        // the contract army was bought rather than conscripted, and it has to be bought again
        // every quarter. GAME-BALANCE FIGURE, not a sourced cost per soldier: it is set so that
        // payroll keeps the same weight in the war budget now that the force is 1,7 times larger.
        russia.Manpower.UpkeepCostPerThousand = 0.050d;
        russia.Manpower.BaseGdpCostPerThousand = 0.031d;
        russia.Manpower.MarginalCostExponent = 1.4d;

        // 560 000 rounds a quarter, i.e. 2,24 M a year: Ukrainian and Western officials put
        // Russian DOMESTIC artillery production at no more than 2,3 M rounds in 2024, and the
        // industrial expansion line raises it over the run, which is what happened in 2025.
        // The gap to what 700 000 men actually burn is the whole point: Reuters assesses North
        // Korean deliveries at roughly half of the shells Russia fires, so the difference has to
        // be BOUGHT abroad, quarter after quarter. Cut the money and the shells stop — which is
        // the mechanism the asphyxiation run demonstrates, now grounded rather than assumed.
        russia.Industry.SetCapacityPerTurn(ResourceKind.Weapons, 560d);
        // 3 000 strike drones a quarter, rising to 10 500 at the 3,5 × expansion ceiling.
        // Russia launched more than 44 000 Shahed-type drones over 2025 — 170 a day in the
        // summer peaks — against 2 400 a quarter, i.e. 26 a day, in the engine. KNOWN
        // OVERSTATEMENT OF 2022: Russia fired a few hundred Shaheds that year, not three
        // thousand a quarter. The engine has no way to grow a line forty-fold in three years,
        // so the choice is between a right 2022 and a right 2025, and the saturation the game
        // is built to demonstrate lives in 2025.
        russia.Industry.SetCapacityPerTurn(ResourceKind.StrikeDrones, 3000d);
        russia.Industry.SetCapacityPerTurn(ResourceKind.Missiles, 130d);
        // Raised with the strike volumes it has to answer. Ukrainian deep-strike drones went
        // from 700 to 1 800 a quarter in this calibration; leaving Russian counter-drone
        // capacity where it was would have handed Ukraine an unearned free run at the
        // refineries. Russia built exactly this in the real war — mobile fire groups, Pantsir
        // batteries and jamming belts along the strike corridors.
        russia.Industry.SetCapacityPerTurn(ResourceKind.CheapInterceptors, 1600d);
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
                InKindShare = 0.54d,
                UnsustainableShare = 0.45d,
            },
        };

        ukraine.Economy.HeadlineGdpBillions = 196d;
        ukraine.Economy.ProductiveCapacityBillions = 188d;
        ukraine.Economy.TreasuryBillions = 8d;
        ukraine.Economy.ReservesBillions = 29d;
        ukraine.Economy.FiscalCaptureRate = 0.072d;

        // Calibrated on what aid plus its own fiscal capacity actually funds, so that the
        // funding gap reads zero while support holds and opens the turn it stops. A ceiling
        // permanently out of reach would make the gauge cry wolf for nineteen turns.
        ukraine.Economy.WarBudgetCeilingShare = 0.072d;
        ukraine.Economy.MilitaryFiscalShare = 0.62d;
        ukraine.Economy.ReserveDrawRate = 0.09d;
        ukraine.Economy.CivilianGrowthPerTurn = -0.02d;
        ukraine.Economy.MilitarySpendingMultiplier = 0.35d;
        ukraine.Economy.CapitalDecayPerTurn = 0.014d;
        ukraine.Economy.OilImportMbd = 0.24d;

        // A far smaller grid with a thin margin: the threshold is within reach of a drone campaign.
        // OPERABLE capacity, not installed. The IEA put Ukraine's available dispatchable
        // generation at about 38 GW before 2022, then recorded 19 GW lost in the first year of
        // war to occupation and destruction — Zaporizhzhia alone is 6 GW, seized in March 2022
        // and never modelled here. What the country could actually dispatch after that first
        // year is the order of 19 to 22 GW, and 36 GW was the pre-war nameplate figure.
        //
        // Winter peak demand is the other half of the threshold: the IEA expects up to 18,5 GW,
        // which 13 GW of base demand at the ×1,45 winter multiplier reproduces almost exactly.
        // The margin drops from 55 % to 14 % in a winter quarter, and three gigawatts of damage
        // finally bite. At 36 GW against 23,3 GW of winter demand it took 12,7 GW of damage
        // before the first cut — a threshold the engine could reach in theory and never did.
        ukraine.Grid.NominalCapacityGw = 26d;
        ukraine.Grid.BaseDemandGw = 13d;
        ukraine.Grid.WinterDemandMultiplier = 1.45d;
        ukraine.Grid.CivilianShareOfDemand = 0.6d;

        // ── Effectifs, en milliers d'hommes ────────────────────────────────────────────────

        // 3,7 M men aged 25 to 60 assessed as mobilisable in March 2024, the rest being already
        // serving, unfit, abroad or in reserved occupations. That is a 2024 snapshot used here as
        // the 2022 reservoir, which flatters Ukraine slightly: flagged, not corrected.
        ukraine.Manpower.MobilisablePool = 3700d;

        // 196 600 active armed forces at the outbreak (IISS Military Balance 2022), plus the
        // National Guard and Border Guard committed within days. ESTIMATION for the 250 000: the
        // share genuinely engaged in the first weeks is not separable from the total under arms.
        ukraine.Manpower.AtFront = 200d;

        // Ukraine's establishment grew as steadily as Russia's, and from a smaller base. The
        // 2025 Military Balance puts ground strength at 575 000 (army, marines, airborne),
        // excluding territorial defence; Zelensky claims 800 000 to 980 000 in uniform, all
        // services and rear included. 620 000 in theatre is the middle of that spread, reached
        // at +25 000 a quarter. The gap between the two figures is not resolvable: see the doc.
        ukraine.Manpower.TargetForceSize = 250d;
        ukraine.Manpower.TargetForceGrowthPerTurn = 22d;
        ukraine.Manpower.TargetForceCeiling = 560d;

        // Men under arms outside the theatre per man inside it. Zelensky claims 880 000 in
        // uniform in January 2025 and OSW puts the total above a million; against a theatre
        // grouping in the 500 000s, the tail is territorial defence, the air defence of the
        // cities, the training establishment and the rear services.
        ukraine.Manpower.RearEstablishmentRatio = 0.68d;

        // The sourced anchor of the whole three-count distinction: OSW assesses that no more
        // than 300 000 of the million-plus Ukrainians under arms are deployed on the line, and
        // Ukrainian reporting has brigades down to 30 % of establishment. This is why an army
        // of a million can run out of infantry — and why the model puts the power on this count
        // and the consumption on the theatre count. Range 0,45 – 0,65.
        ukraine.Manpower.ContactShare = 0.55d;

        // ≈ 26 000 a month: Zelensky put Ukrainian mobilisation at 25 000 – 27 000 a month
        // against 40 000 – 45 000 for Russia, with a brief 30 000 peak after the 2024 law.
        ukraine.Manpower.TrainingCapacityPerTurn = 78d;
        ukraine.Manpower.TrainingTurns = 1;
        ukraine.Manpower.ContractCostPerThousand = 0.013d;

        // A mobilised army is far cheaper in cash than a bought one — and far dearer in
        // consent and in GDP, which is the whole asymmetry. GAME-BALANCE FIGURE, rescaled with
        // the force size for the same reason as the Russian one.
        ukraine.Manpower.UpkeepCostPerThousand = 0.020d;
        ukraine.Manpower.BaseGdpCostPerThousand = 0.062d;
        ukraine.Manpower.MarginalCostExponent = 1.55d;

        ukraine.Industry.SetCapacityPerTurn(ResourceKind.Weapons, 45d);
        // An army supplied by grant does not build a war reserve. The donor ships against
        // what is being burnt, in convoys negotiated quarter by quarter, and Ukraine spent
        // 2023 and 2024 firing what had arrived that month rather than drawing on a stock it
        // never had. Two quarters against Russia's six is the asymmetry of «donner contre
        // vendre» made physical: the buyer plans a reserve, the receiver lives on the flow —
        // and that is exactly why cutting the flow reaches the Ukrainian front in one quarter
        // and would take Russia a year.
        ukraine.Industry.DepotQuartersHeld = 3d;

        // 1 800 a quarter, i.e. 7 200 a year — deliberately conservative against Ukrainian
        // claims of tens of thousands of long-range drones a year, since those counts mix
        // classes. What matters here is the ratio: Ukraine strikes deep with drones because it
        // has no missiles, and the volume has to be able to saturate a defended target.
        ukraine.Industry.SetCapacityPerTurn(ResourceKind.StrikeDrones, 1800d);
        ukraine.Industry.SetCapacityPerTurn(ResourceKind.Missiles, 25d);
        // 7 000 a quarter, reaching 24 500 at the expansion ceiling — the Ukrainian security
        // council reports 100 000 interceptor drones produced in 2025, i.e. 25 000 a quarter,
        // and deliveries running at 1 000 to 1 500 a day by early 2026. The engine held 1 100
        // a quarter, twelve a day, against a hundred and seventy incoming: two orders of
        // magnitude out. The error was invisible while aid piled up a wall of half a million
        // interceptors outside any ceiling; capping the depots is what exposed it.
        // Same 2022-against-2025 trade-off as the strike drones, and the same answer.
        ukraine.Industry.SetCapacityPerTurn(ResourceKind.CheapInterceptors, 7000d);
        ukraine.Industry.SetCapacityPerTurn(ResourceKind.HeavyInterceptors, 12d);

        // Fast cycles, small structures: frequent jumps that are hard to scale.
        ukraine.Innovation.AdoptionSpeed = 2.1d;
        ukraine.Innovation.ScaleCeiling = 1.1d;
        ukraine.Innovation.DecayPerTurn = 0.16d;

        ukraine.AirDefence.Coverage = 0.55d;
        ukraine.AirDefence.RearShare = 0.62d;
        ukraine.AirDefence.CheapPurchaseShare = 0.6d;

        // The Soviet inheritance: a stock of the right calibres and no way to make more of
        // them. It is what let the front hold through 2022, and running out of it produced
        // the 2023 crisis. DESIGN FIGURE, and the only one in this chain that is: nobody has
        // published what Ukraine held in February 2022. It is set by the demonstration it has
        // to support — the depot buys exactly the two quiet quarters the model promises after
        // a flow is cut, no more. Without a depot the cut lands the same quarter, and the
        // latency is the whole lesson.
        ukraine.Stock.Add(ResourceKind.Weapons, 400d);
        ukraine.Stock.Add(ResourceKind.Fuel, 120d);
        ukraine.Stock.Add(ResourceKind.Food, 140d);
        ukraine.Stock.Add(ResourceKind.StrikeDrones, 150d);
        ukraine.Stock.Add(ResourceKind.Missiles, 40d);
        ukraine.Stock.Add(ResourceKind.CheapInterceptors, 900d);
        ukraine.Stock.Add(ResourceKind.HeavyInterceptors, 260d);

        return ukraine;
    }

    /// <summary>
    /// Anchored on the real February 2022 contact line, north to south. Push vectors point
    /// along each sector's actual axis of advance, in degrees per ten-kilometre hex.
    /// </summary>
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
                Longitude = 36.95d,
                Latitude = 50.05d,
                PushLongitude = -0.095d,
                PushLatitude = -0.045d,
            },
            new FrontSector
            {
                Code = "kupiansk",
                Name = "Koupiansk",
                TerrainMultiplier = 1.1d,
                Urbanisation = 0.12d,
                Width = 6,
                StrategicValue = 1.1d,
                Longitude = 37.75d,
                Latitude = 49.55d,
                PushLongitude = -0.134d,
                PushLatitude = 0d,
            },
            new FrontSector
            {
                Code = "lyman",
                Name = "Lyman",
                TerrainMultiplier = 1.25d,
                Urbanisation = 0.08d,
                Width = 5,
                StrategicValue = 0.9d,
                Longitude = 38.05d,
                Latitude = 49.0d,
                PushLongitude = -0.134d,
                PushLatitude = 0d,
            },
            new FrontSector
            {
                Code = "bakhmut",
                Name = "Bakhmout — Tchassiv Iar",
                TerrainMultiplier = 1.15d,
                Urbanisation = 0.4d,
                Width = 5,
                StrategicValue = 1.2d,
                Longitude = 38.15d,
                Latitude = 48.6d,
                PushLongitude = -0.13d,
                PushLatitude = 0.02d,
            },
            new FrontSector
            {
                Code = "pokrovsk",
                Name = "Pokrovsk",
                TerrainMultiplier = 0.95d,
                Urbanisation = 0.22d,
                Width = 7,
                StrategicValue = 1.5d,
                Longitude = 37.8d,
                Latitude = 48.25d,
                PushLongitude = -0.134d,
                PushLatitude = 0d,
            },
            new FrontSector
            {
                Code = "vuhledar",
                Name = "Vouhledar",
                TerrainMultiplier = 1d,
                Urbanisation = 0.18d,
                Width = 5,
                StrategicValue = 0.9d,
                Longitude = 37.3d,
                Latitude = 47.75d,
                PushLongitude = -0.12d,
                PushLatitude = 0.035d,
            },
            new FrontSector
            {
                Code = "zaporizhzhia",
                Name = "Zaporijjia",
                TerrainMultiplier = 1.2d,
                Urbanisation = 0.15d,
                Width = 7,
                StrategicValue = 1.4d,
                Longitude = 35.9d,
                Latitude = 47.45d,
                PushLongitude = -0.09d,
                PushLatitude = 0.055d,
            },
            new FrontSector
            {
                Code = "kherson",
                Name = "Kherson — Dniepr",
                TerrainMultiplier = 1.6d,
                Urbanisation = 0.2d,
                Width = 6,
                StrategicValue = 1.2d,
                Longitude = 33.4d,
                Latitude = 46.75d,
                PushLongitude = -0.085d,
                PushLatitude = 0.06d,
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
        // Two to four cards per side per quarter. The war is a decision every three months,
        // on both sides, and the screen has to show it — not one card surrounded by filler.
        List<ScheduledCard> calendar =
        [
            new ScheduledCard { Turn = 1, CardCode = "force_concentration" },
            new ScheduledCard { Turn = 1, CardCode = "zapad_exercises" },
            new ScheduledCard { Turn = 1, CardCode = "ultimatum_to_nato" },
            new ScheduledCard { Turn = 1, CardCode = "intelligence_warning" },
            new ScheduledCard { Turn = 1, CardCode = "first_defensive_deliveries" },
            new ScheduledCard { Turn = 1, CardCode = "no_mobilisation_yet" },
            new ScheduledCard { Turn = 2, CardCode = "sanctions_package_1" },
            new ScheduledCard { Turn = 2, CardCode = "allied_intelligence" },
            new ScheduledCard { Turn = 2, CardCode = "nato_training_pipeline" },
            new ScheduledCard { Turn = 2, CardCode = "state_propaganda_surge" },
            new ScheduledCard { Turn = 2, CardCode = "domestic_repression" },
            new ScheduledCard { Turn = 2, CardCode = "rasputitsa" },
            new ScheduledCard { Turn = 3, CardCode = "western_aid_opens" },
            new ScheduledCard { Turn = 3, CardCode = "transparency_reform" },
            new ScheduledCard { Turn = 3, CardCode = "recruitment_reform" },
            new ScheduledCard { Turn = 3, CardCode = "industrial_requisition" },
            new ScheduledCard { Turn = 3, CardCode = "war_economy_conversion" },
            new ScheduledCard { Turn = 4, CardCode = "himars_deep_strike" },
            new ScheduledCard { Turn = 4, CardCode = "depot_strikes" },
            new ScheduledCard { Turn = 4, CardCode = "prison_recruitment" },
            new ScheduledCard { Turn = 4, CardCode = "foreign_drones" },
            new ScheduledCard { Turn = 4, CardCode = "pipeline_sabotage" },
            new ScheduledCard { Turn = 5, CardCode = "counter_offensive_2022" },
            new ScheduledCard { Turn = 5, CardCode = "naval_drones_black_sea" },
            new ScheduledCard { Turn = 5, CardCode = "partial_mobilisation" },
            new ScheduledCard { Turn = 5, CardCode = "foreign_ballistic_missiles" },
            new ScheduledCard { Turn = 5, CardCode = "decoy_saturation" },
            new ScheduledCard { Turn = 6, CardCode = "oil_price_cap" },
            new ScheduledCard { Turn = 6, CardCode = "diplomatic_campaign" },
            new ScheduledCard { Turn = 6, CardCode = "grid_campaign" },
            new ScheduledCard { Turn = 6, CardCode = "glide_bombs" },
            new ScheduledCard { Turn = 6, CardCode = "oil_export_rerouting" },
            new ScheduledCard { Turn = 6, CardCode = "mild_winter" },
            new ScheduledCard { Turn = 7, CardCode = "component_embargo" },
            new ScheduledCard { Turn = 7, CardCode = "cheap_interception" },
            new ScheduledCard { Turn = 7, CardCode = "component_smuggling" },
            new ScheduledCard { Turn = 7, CardCode = "electronic_warfare_scaling" },
            new ScheduledCard { Turn = 7, CardCode = "armed_mutiny" },
            new ScheduledCard { Turn = 8, CardCode = "failed_offensive" },
            new ScheduledCard { Turn = 8, CardCode = "depot_strikes" },
            new ScheduledCard { Turn = 8, CardCode = "domestic_drone_industry" },
            new ScheduledCard { Turn = 8, CardCode = "shahed_plant" },
            new ScheduledCard { Turn = 8, CardCode = "grain_port_strikes" },
            new ScheduledCard { Turn = 8, CardCode = "inflation_surge" },
            new ScheduledCard { Turn = 9, CardCode = "component_embargo" },
            new ScheduledCard { Turn = 9, CardCode = "naval_drones_black_sea" },
            new ScheduledCard { Turn = 9, CardCode = "foreign_shells" },
            new ScheduledCard { Turn = 9, CardCode = "contract_recruitment_drive" },
            new ScheduledCard { Turn = 9, CardCode = "shadow_fleet" },
            new ScheduledCard { Turn = 9, CardCode = "attention_elsewhere" },
            new ScheduledCard { Turn = 9, CardCode = "rasputitsa" },
            new ScheduledCard { Turn = 10, CardCode = "cheap_interception" },
            new ScheduledCard { Turn = 10, CardCode = "conscription_law" },
            new ScheduledCard { Turn = 10, CardCode = "domestic_drone_industry" },
            new ScheduledCard { Turn = 10, CardCode = "grid_campaign" },
            new ScheduledCard { Turn = 10, CardCode = "decoy_saturation" },
            new ScheduledCard { Turn = 10, CardCode = "contract_recruitment_drive" },
            new ScheduledCard { Turn = 11, CardCode = "refinery_strikes" },
            new ScheduledCard { Turn = 11, CardCode = "drone_coalition" },
            new ScheduledCard { Turn = 11, CardCode = "electronic_warfare_ukraine" },
            new ScheduledCard { Turn = 11, CardCode = "licence_transfer" },
            new ScheduledCard { Turn = 11, CardCode = "glide_bombs" },
            new ScheduledCard { Turn = 11, CardCode = "meat_assault" },
            new ScheduledCard { Turn = 11, CardCode = "european_election_swing" },
            new ScheduledCard { Turn = 12, CardCode = "rail_interdiction" },
            new ScheduledCard { Turn = 12, CardCode = "himars_deep_strike" },
            new ScheduledCard { Turn = 12, CardCode = "nato_training_pipeline" },
            new ScheduledCard { Turn = 12, CardCode = "counter_battery" },
            new ScheduledCard { Turn = 12, CardCode = "meat_assault" },
            new ScheduledCard { Turn = 13, CardCode = "frozen_assets_windfall" },
            new ScheduledCard { Turn = 13, CardCode = "cheap_interception" },
            new ScheduledCard { Turn = 13, CardCode = "decentralised_generation" },
            new ScheduledCard { Turn = 13, CardCode = "fibre_optic_drones" },
            new ScheduledCard { Turn = 13, CardCode = "electronic_warfare" },
            new ScheduledCard { Turn = 13, CardCode = "substation_strikes" },
            new ScheduledCard { Turn = 14, CardCode = "shadow_fleet_sanctions" },
            new ScheduledCard { Turn = 14, CardCode = "cheap_interception" },
            new ScheduledCard { Turn = 14, CardCode = "grid_campaign" },
            new ScheduledCard { Turn = 14, CardCode = "evasion_network" },
            new ScheduledCard { Turn = 14, CardCode = "decoy_saturation" },
            new ScheduledCard { Turn = 14, CardCode = "harsh_winter" },
            new ScheduledCard { Turn = 15, CardCode = "anticorruption_crisis" },
            new ScheduledCard { Turn = 15, CardCode = "rail_interdiction" },
            new ScheduledCard { Turn = 15, CardCode = "nato_training_pipeline" },
            new ScheduledCard { Turn = 15, CardCode = "rail_repair_brigades" },
            new ScheduledCard { Turn = 15, CardCode = "diplomatic_complaisance" },
            new ScheduledCard { Turn = 15, CardCode = "air_defence_gap" },
            new ScheduledCard { Turn = 16, CardCode = "drone_coalition" },
            new ScheduledCard { Turn = 16, CardCode = "domestic_drone_industry" },
            new ScheduledCard { Turn = 16, CardCode = "domestic_repression" },
            new ScheduledCard { Turn = 16, CardCode = "meat_assault" },
            new ScheduledCard { Turn = 16, CardCode = "foreign_drones" },
            new ScheduledCard { Turn = 16, CardCode = "ceasefire_talks" },
            new ScheduledCard { Turn = 17, CardCode = "cheap_interception" },
            new ScheduledCard { Turn = 17, CardCode = "transparency_reform" },
            new ScheduledCard { Turn = 17, CardCode = "drone_coalition" },
            new ScheduledCard { Turn = 17, CardCode = "grain_port_strikes" },
            new ScheduledCard { Turn = 17, CardCode = "decoy_saturation" },
            new ScheduledCard { Turn = 17, CardCode = "oil_export_rerouting" },
            new ScheduledCard { Turn = 18, CardCode = "nato_training_pipeline" },
            new ScheduledCard { Turn = 18, CardCode = "domestic_drone_industry" },
            new ScheduledCard { Turn = 18, CardCode = "contract_recruitment_drive" },
            new ScheduledCard { Turn = 18, CardCode = "state_propaganda_surge" },
            new ScheduledCard { Turn = 18, CardCode = "domestic_repression" },
            new ScheduledCard { Turn = 19, CardCode = "cheap_interception" },
            new ScheduledCard { Turn = 19, CardCode = "drone_coalition" },
            new ScheduledCard { Turn = 19, CardCode = "domestic_drone_industry" },
            new ScheduledCard { Turn = 19, CardCode = "glide_bombs" },
            new ScheduledCard { Turn = 19, CardCode = "foreign_ballistic_missiles" },
            new ScheduledCard { Turn = 19, CardCode = "state_propaganda_surge" },

            // Beyond the present quarter, the war does not stop: both sides keep spending
            // what they have left while the strangulation does its work in the background.
            new ScheduledCard { Turn = 20, CardCode = "depot_strikes" },
            new ScheduledCard { Turn = 20, CardCode = "elite_fracture" },
            new ScheduledCard { Turn = 20, CardCode = "cheap_interception" },
            new ScheduledCard { Turn = 20, CardCode = "meat_assault" },
            new ScheduledCard { Turn = 20, CardCode = "decoy_saturation" },
            new ScheduledCard { Turn = 21, CardCode = "drone_coalition" },
            new ScheduledCard { Turn = 21, CardCode = "nato_training_pipeline" },
            new ScheduledCard { Turn = 21, CardCode = "glide_bombs" },
            new ScheduledCard { Turn = 21, CardCode = "meat_assault" },
            new ScheduledCard { Turn = 22, CardCode = "domestic_drone_industry" },
            new ScheduledCard { Turn = 22, CardCode = "cheap_interception" },
            new ScheduledCard { Turn = 22, CardCode = "harsh_winter" },
            new ScheduledCard { Turn = 22, CardCode = "decoy_saturation" },
            new ScheduledCard { Turn = 23, CardCode = "depot_strikes" },
            new ScheduledCard { Turn = 23, CardCode = "meat_assault" },
        ];

        if (variant == SupportVariant.Holds)
        {
            // The 2023-2024 ammunition crisis: suspended, then released. The model must find it alone.
            calendar.Add(new ScheduledCard { Turn = 10, CardCode = "aid_blocked" });
            calendar.Add(new ScheduledCard { Turn = 12, CardCode = "aid_unblocked" });

            // Support thins and gets conditioned, it never stops. Nobody breaks.
            calendar.Add(new ScheduledCard { Turn = 14, CardCode = "budget_fatigue" });
            calendar.Add(new ScheduledCard { Turn = 18, CardCode = "parliament_veto" });
        }
        else if (variant == SupportVariant.Collapses)
        {
            // What actually cuts a free flow: not a battle, a ballot. The two land together
            // because one causes the other — and because the depot has to be full when they do.
            // Nothing before turn 7: the first six turns must stay strictly identical to Holds.
            calendar.Add(new ScheduledCard { Turn = 7, CardCode = "us_election_swing" });
            calendar.Add(new ScheduledCard { Turn = 7, CardCode = "aid_collapse" });
            calendar.Add(new ScheduledCard { Turn = 9, CardCode = "budget_fatigue" });
        }
        else
        {
            // Same war, same events, same barrel calendar as the other runs. The only
            // difference is what the West decides to play on top — and none of these
            // cards takes a single hex: every one of them cuts a flow at its source.
            calendar.Add(new ScheduledCard { Turn = 10, CardCode = "aid_blocked" });
            calendar.Add(new ScheduledCard { Turn = 11, CardCode = "aid_unblocked" });

            calendar.AddRange(
            [
                new ScheduledCard { Turn = 11, CardCode = "component_embargo_total" },
                new ScheduledCard { Turn = 12, CardCode = "aid_predictable" },

                // The refining campaign comes back every third quarter, not every one:
                // the damage half-heals in between, and that gap is what makes the
                // strangulation take years. Rushed, the regime falls a year too early.
                new ScheduledCard { Turn = 13, CardCode = "refinery_campaign_sustained" },
                new ScheduledCard { Turn = 14, CardCode = "frozen_assets_released" },
                new ScheduledCard { Turn = 17, CardCode = "aid_predictable" },

                // Autumn 2025: the two majors are designated and the discount doubles.
                new ScheduledCard { Turn = 17, CardCode = "major_oil_sanctions" },

                // Past the present quarter the tension has to keep climbing, quarter after
                // quarter, without the blow landing early: a plateau here would say the
                // strangulation stalled, and an early break would say the front decided.
                new ScheduledCard { Turn = 20, CardCode = "shadow_fleet_sanctions" },
                new ScheduledCard { Turn = 21, CardCode = "component_embargo_total" },
                new ScheduledCard { Turn = 21, CardCode = "oil_price_cap" },
                new ScheduledCard { Turn = 21, CardCode = "conscription_law" },
                new ScheduledCard { Turn = 22, CardCode = "refinery_campaign_sustained" },
                new ScheduledCard { Turn = 22, CardCode = "currency_collapse" },

                // Spring 2027. The regime does not fall because a front moved: it falls
                // because the quarter it has to fund is the one it can no longer fund.
                new ScheduledCard { Turn = 23, CardCode = "supplier_withdraws" },
                new ScheduledCard { Turn = 23, CardCode = "oil_price_crash" },
                new ScheduledCard { Turn = 23, CardCode = "sovereign_fund_empty" },
                new ScheduledCard { Turn = 23, CardCode = "elite_break" },
                new ScheduledCard { Turn = 23, CardCode = "refinery_campaign_sustained" },
                new ScheduledCard { Turn = 23, CardCode = "sovereign_fund_draw" },
            ]);
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

        // Autumn 2021. Nobody attacks: one side is massing, the other is not mobilising.
        // A null offensive posture drops every sector ratio to zero, so no hex moves.
        Doctrine russianPrologue = RussianDoctrine();
        russianPrologue.OffensivePosture = 0d;

        Doctrine ukrainianPrologue = UkrainianDoctrine();
        ukrainianPrologue.OffensivePosture = 0d;

        scenario.DoctrineShifts.AddRange(
        [
            new DoctrineShift
            {
                Turn = 1,
                SideCode = Side.Invader.Code,
                Doctrine = russianPrologue,
                Reason = "Concentration de forces, aucune offensive",
            },
            new DoctrineShift
            {
                Turn = 1,
                SideCode = Side.Defender.Code,
                Doctrine = ukrainianPrologue,
                Reason = "Ni mobilisation ni provocation",
            },

            // The invasion. Both sides return to their standing doctrine.
            new DoctrineShift
            {
                Turn = 2,
                SideCode = Side.Invader.Code,
                Doctrine = RussianDoctrine(),
                Reason = "Invasion",
            },
            new DoctrineShift
            {
                Turn = 2,
                SideCode = Side.Defender.Code,
                Doctrine = UkrainianDoctrine(),
                Reason = "Défense générale",
            },
            new DoctrineShift
            {
                Turn = 5,
                SideCode = Side.Defender.Code,
                Doctrine = ukrainian2022,
                Reason = "Contre-offensives de Kharkiv et Kherson",
            },
            new DoctrineShift
            {
                Turn = 6,
                SideCode = Side.Defender.Code,
                Doctrine = ukrainianDefensive,
                Reason = "Retour à la défensive et fortification",
            },
            new DoctrineShift
            {
                Turn = 8,
                SideCode = Side.Defender.Code,
                Doctrine = ukrainianOffensive,
                Reason = "Contre-offensive d'été",
            },
            new DoctrineShift
            {
                Turn = 9,
                SideCode = Side.Defender.Code,
                Doctrine = ukrainianDefensive,
                Reason = "L'offensive s'enlise, retour à la défense",
            },
            new DoctrineShift
            {
                Turn = 10,
                SideCode = Side.Invader.Code,
                Doctrine = russianGrinding,
                Reason = "Passage au grignotage soutenu",
            },
        ]);
    }
}
