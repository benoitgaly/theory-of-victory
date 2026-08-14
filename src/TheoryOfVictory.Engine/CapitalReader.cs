using TheoryOfVictory.Core;
using TheoryOfVictory.Core.Localization;

namespace TheoryOfVictory.Engine;

/// <summary>
/// Reads the war capital of one side. Like <see cref="PressureAnalyser"/>, this is not an
/// eleventh phase: it decides nothing and changes nothing, it looks at what the ten just did.
///
/// The band it feeds exists to make one thing visible — a front that still looks healthy while
/// the capital feeding it gives way. That only works if a fall is attributed: an ordinary
/// draw-down and a destroyed turbine hall are the same number and not the same event.
///
/// Everything here is priced in BILLIONS OF DOLLARS, both camps and all seven posts. The game
/// is capitalist in its premise — the capital produces what the front consumes — and a capital
/// is counted in money, not in an index for one post, gigawatts for the next and points of
/// margin for a third. The conversion coefficients below are working orders of magnitude, in
/// the sense the README gives that phrase: they are posed so the band produces a balance sheet
/// worth arguing with, and each of them is written down with its uncertainty in
/// <c>docs/design/08-capital-de-guerre.md</c>. None of them touches the simulation.
/// </summary>
public static class CapitalReader
{
    private const double DaysPerTurn = 91.25d;

    /// <summary>The engine thinks in quarters; a balance sheet is read by the year.</summary>
    private const double QuartersPerYear = 4d;

    /// <summary>
    /// THE rule of the balance sheet: a productive asset is worth five years of what it makes.
    ///
    /// It is the only reason the seven posts can be read against one another. An oil field, a
    /// power fleet, a shell line and a consumer-goods industry have nothing in common except
    /// that each produces something every year — so each is valued the same way, and no post
    /// gets a coefficient of its own invented for it. Five is a capitalisation multiple in the
    /// low range, which is the honest range for an asset sitting in a war zone: nobody prices
    /// twenty years of future output for a refinery inside missile range.
    ///
    /// Two posts are deliberately NOT capitalised, and the exception is the point rather than
    /// an oversight. Foreign support is a flow that can stop in a day — that is rule five of
    /// the model — and multiplying it by five would book five guaranteed years of an aid
    /// package that one election cancels. Holding on to power is not a production either: it
    /// is a bill the regime pays, and what the band prices is the margin it has left to pay it.
    ///
    /// Consequence, and it is wanted: the oil post now revalues and depreciates WITH the
    /// barrel, five times over. A barrel collapsing takes far more out of the Russian capital
    /// than the quarter's lost revenue — which is exactly the mechanism the victory run works.
    /// </summary>
    public const double CapitalisationMultiple = 5d;

    /// <summary>
    /// A year of electricity sales per gigawatt of standing plant, in billions: 8 760 hours at
    /// roughly half load — a mixed post-Soviet fleet runs at about 50 % — sold near 50 $ the
    /// megawatt-hour. It is the one physical price the band still needs, because the grid is
    /// the only post the engine holds in a unit that is not money; everything downstream of it
    /// is the same capitalisation rule as every other production.
    /// </summary>
    public const double GridAnnualOutputPerGwBillions = 0.22d;

    /// <summary>
    /// What holding on to power costs in a year, as a share of the sustainable productive
    /// capacity: internal security, the clientele, the subsidies that buy social peace. An
    /// apparatus that has to buy obedience pays more for it than one that is elected — and it
    /// is read on the sustainable capacity rather than on headline GDP, because headline GDP is
    /// inflated by the war itself and a regime consuming its own economy would otherwise look
    /// better able to pay for its survival every quarter it got poorer.
    /// </summary>
    private const double HoldingCostShareAutocracy = 0.035d;

    private const double HoldingCostShareDemocracy = 0.02d;

    /// <summary>Quarters of reserve left below which the countdown is worth drawing.</summary>
    private const double ReserveWarningQuarters = 4d;

    /// <summary>No single post can take the whole capital to zero: that is a front-flow rule.</summary>
    private const double IndexFloor = 15d;

    private const double IndexCeiling = 150d;

    public const string Reserves = "reserves";
    public const string Grid = "grid";
    public const string Oil = "oil";
    public const string Civilian = "civilian";
    public const string Arms = "arms";
    public const string Regime = "regime";
    public const string Foreign = "foreign";
    public const string International = "international";

    /// <summary>
    /// Components weigh double in the diplomatic composite, as they do everywhere else in the
    /// model: price and friction get worked around, machine tools do not. It is the slow
    /// channel, and it is the only one that decides.
    /// </summary>
    private const double ComponentWeight = 2d;

    /// <summary>Posts whose fall drags the ones after it, for the consequence ribbon.</summary>
    private static readonly Dictionary<string, string[]> Downstream = new()
    {
        [Grid] = [Arms, Regime],
        [Oil] = [Reserves, Regime],
        [Civilian] = [Regime],
        [Foreign] = [Reserves, Arms],
        [Reserves] = [Regime],
        [Arms] = [],
        [Regime] = [],
        [International] = [Oil, Arms, Foreign],
    };

    /// <summary>Which post an incoming strike wave lands on.</summary>
    private static string? PostOf(StrikeTarget target)
    {
        return target switch
        {
            StrikeTarget.PowerGrid => Grid,
            StrikeTarget.Refining => Oil,
            StrikeTarget.Industry => Arms,
            StrikeTarget.CivilianIndustry => Civilian,
            _ => null,
        };
    }

    /// <summary>Which post a card effect moves. Anything absent moves no post of the band.</summary>
    private static string? PostOf(EffectKind kind)
    {
        return kind switch
        {
            EffectKind.OilPriceDelta or EffectKind.SanctionsPriceDelta
                or EffectKind.SanctionsFrictionDelta or EffectKind.RefiningIntegrityDelta => Oil,
            EffectKind.AidPledgeDelta or EffectKind.AidDisbursementRate
                or EffectKind.ForeignSupplyCeilingDelta or EffectKind.ConditionalityDelta
                or EffectKind.ExternalWillDelta => Foreign,
            EffectKind.SanctionsComponentDelta or EffectKind.ProductionCapacityMultiplier => Arms,
            EffectKind.GridPermanentDamage => Grid,
            EffectKind.CivilianIndustryDamage => Civilian,
            EffectKind.TreasuryDelta => Reserves,
            EffectKind.MoraleDelta or EffectKind.PopularDiscontentDelta
                or EffectKind.EliteCohesionDelta or EffectKind.CorruptionDelta
                or EffectKind.MobilisationWave => Regime,
            _ => null,
        };
    }

    private static LocalizedText Label(StrikeTarget target)
    {
        return LocalizedText.Of(target switch
        {
            StrikeTarget.PowerGrid => TextCodes.Capital.GridStrike,
            StrikeTarget.Refining => TextCodes.Capital.RefiningStrike,
            StrikeTarget.Industry => TextCodes.Capital.ArmsStrike,
            StrikeTarget.CivilianIndustry => TextCodes.Capital.CivilianStrike,
            _ => TextCodes.Capital.DeepStrike,
        });
    }

    /// <summary>
    /// The seven posts of one side in billions of dollars, taken identically before and after
    /// the ten phases. Two readings of the same ruler: anything derived from them is a real
    /// move, never an artefact.
    ///
    /// Five posts are ASSETS — the cash in the fund, and four productions valued at five years
    /// of themselves (<see cref="CapitalisationMultiple"/>). Two are FLOWS the side has no
    /// title to and cannot capitalise: what it is given from outside, and what it can still
    /// spend holding itself in place. The band totals the two natures apart.
    /// </summary>
    public static Dictionary<string, double> Measure(Belligerent belligerent, GameState state)
    {
        Economy economy = belligerent.Economy;

        // One post, two opposite economies: the barrel is a receipt on the side that exports
        // it and a bill on the side that imports it. The bill is read as a capital further on,
        // which is where a rising barrel starts costing Ukraine. Priced at the barrel of the
        // day, so the post moves the quarter the price moves — which is the point of the lever.
        double oilPerQuarter = economy.OilExportCapacityMbd > 0d
            ? economy.LastTurnOilRevenueBillions
            : economy.OilImportMbd * DaysPerTurn * state.OilPrice / 1000d;

        double tapPerQuarter = belligerent.Foreign.Mode == SupportMode.Granted
            ? belligerent.Foreign.EffectiveGrantBillions
            : belligerent.AllocationThisTurn.GetValueOrDefault("foreign");

        return new Dictionary<string, double>
        {
            [Reserves] = economy.ReservesBillions,
            [Grid] = GridValue(belligerent.Grid.AvailableCapacityGw),
            ["grid_permanent"] = GridValue(belligerent.Grid.PermanentDamageGw),
            [Oil] = oilPerQuarter * QuartersPerYear * CapitalisationMultiple,
            // The plant that still stands, not what it managed to deliver: a factory left dark
            // by a strike on the grid is idle, not destroyed, and booking it as lost capital
            // would count the same wave twice — once on the grid post, once here. The capacity
            // is already a year of civilian output, so the arms works are read the same way and
            // the two factories stay comparable: nearly seven to one between the camps, and
            // thirty to one between the civilian base and the arms works inside each of them.
            [Civilian] = belligerent.Civilian.CapacityBillions * belligerent.Civilian.Integrity * CapitalisationMultiple,
            ["civilian_permanent"] = belligerent.Civilian.PermanentDamage * CapitalisationMultiple,
            [Arms] = belligerent.Industry.TotalCapacityValueBillions() * QuartersPerYear * CapitalisationMultiple,
            [Regime] = HoldingCapacity(belligerent),
            [Foreign] = tapPerQuarter * QuartersPerYear,
            [International] = Latitude(state.Invader),
        };
    }

    /// <summary>
    /// A standing power fleet, priced like every other production: a year of what it sells,
    /// capitalised. It is the same two steps as the oil field and the two factories, and the
    /// only reason it needs a line of its own is that the engine holds it in gigawatts.
    /// </summary>
    public static double GridValue(double gigawatts)
    {
        return gigawatts * GridAnnualOutputPerGwBillions * CapitalisationMultiple;
    }

    /// <summary>
    /// What the regime can still spend a year on holding itself in place, before the apparatus
    /// stops obeying. The full bill — internal security, clientele, social peace — multiplied
    /// by the share of the margin to rupture it has left.
    ///
    /// Pricing the bill alone would have said the opposite of what happens: a regime in trouble
    /// pays MORE to hold, so its post would swell as it approached collapse. What is being held
    /// here is the margin, and the money only says what that margin is worth — so the post
    /// still empties exactly as the regime cracks, and the mass reaching the gutter is still
    /// the apparatus giving way.
    /// </summary>
    private static double HoldingCapacity(Belligerent belligerent)
    {
        double share = belligerent.Politics.Regime == RegimeType.Authoritarian
            ? HoldingCostShareAutocracy
            : HoldingCostShareDemocracy;

        double bill = belligerent.Economy.ProductiveCapacityBillions * share;
        double threshold = Phases.ControlPhase.RegimeCollapseStress;
        double margin = Math.Max(0d, threshold - belligerent.Politics.RegimeStress);

        return bill * margin / threshold;
    }

    /// <summary>
    /// The trade latitude the world still leaves the invader, on a hundred. A world that buys
    /// its oil without a discount is a world that supports it; a world tightening the three
    /// channels is a world isolating it.
    ///
    /// ONE quantity, read from both sides of the table: it is the invader's diplomatic capital,
    /// and it is exactly what its adversaries have taken from it. The band therefore prints the
    /// same figure under both camps and draws two masses pulling in opposite directions —
    /// the only post of the eight where one side's gain is the other's loss.
    ///
    /// It falls on its own if nobody tightens: sanctions erode, circumvention routes get built,
    /// and a post that drops with no card played is a fact of the game rather than a defect.
    /// </summary>
    private static double Latitude(Belligerent invader)
    {
        SanctionsRegime sanctions = invader.Sanctions;
        double severity = (sanctions.PriceSeverity
            + sanctions.FrictionSeverity
            + (ComponentWeight * sanctions.ComponentSeverity)) / (2d + ComponentWeight);

        return 100d * Math.Clamp(1d - severity, 0d, 1d);
    }

    /// <summary>Builds the seven posts of one side, deltas split by cause.</summary>
    public static List<CapitalPost> Read(
        Belligerent belligerent,
        GameState state,
        IReadOnlyDictionary<string, double> opening,
        IReadOnlyDictionary<string, double> reference,
        IReadOnlyList<EventCard> cardsPlayed,
        StrikeResolution? incoming)
    {
        Dictionary<string, double> closing = Measure(belligerent, state);
        bool granted = belligerent.Foreign.Mode == SupportMode.Granted;
        bool exporter = belligerent.Economy.OilExportCapacityMbd > 0d;
        bool invader = belligerent.Side == Side.Invader;

        double draw = belligerent.Economy.LastTurnReserveDrawBillions;

        // The tank behind the tap: a will that can vanish overnight on the side that is given
        // its materiel, a ceiling of cash on the side that buys it. The band prints the flow,
        // because that is what a year of foreign support is worth; the tank says how long the
        // flow has left, which is a different question and belongs to the second reading.
        double tank = granted
            ? belligerent.Politics.ExternalWill
            : Math.Min(
                belligerent.Economy.TreasuryBillions,
                belligerent.Foreign.SupplyCeilingBillions * belligerent.Foreign.PricePremium * QuartersPerYear);

        List<CapitalPost> posts =
        [
            Build(Reserves, LocalizedText.Of(TextCodes.Capital.Reserves), CapitalNature.Stock, false, closing, opening, reference,
                threshold: draw > 0.01d ? draw * ReserveWarningQuarters : null,
                thresholdLabel: LocalizedText.Of(TextCodes.Capital.ReserveThreshold),
                secondary: draw,
                secondaryLabel: LocalizedText.Of(TextCodes.Capital.QuarterDraw),
                secondaryUnit: LocalizedText.Of(TextCodes.Capital.UnitBillions)),

            Build(Grid, LocalizedText.Of(TextCodes.Capital.Grid), CapitalNature.Stock, false, closing, opening, reference,
                threshold: GridValue(belligerent.Grid.DemandGw(Season.Winter)),
                thresholdLabel: LocalizedText.Of(
                    TextCodes.Capital.WinterDemand,
                    LocalizedText.Number(belligerent.Grid.DemandGw(Season.Winter), "F0")),
                secondary: belligerent.Grid.AvailableCapacityGw,
                secondaryLabel: LocalizedText.Of(TextCodes.Capital.StandingPlant),
                secondaryUnit: LocalizedText.Of(TextCodes.Capital.UnitGigawatts)),

            // La seconde lecture des postes capitalisés est leur production de l'année : c'est
            // elle qui rend la règle du bilan lisible sans document — la valeur affichée en est
            // exactement cinq fois celle-ci.
            Build(Oil,
                LocalizedText.Of(exporter ? TextCodes.Capital.OilRevenue : TextCodes.Capital.OilBill),
                CapitalNature.Stock, !exporter,
                closing, opening, reference,
                secondary: closing.GetValueOrDefault(Oil) / CapitalisationMultiple,
                secondaryLabel: LocalizedText.Of(exporter ? TextCodes.Capital.YearRevenue : TextCodes.Capital.YearBill),
                secondaryUnit: LocalizedText.Of(TextCodes.Capital.UnitBillions)),

            Build(Civilian, LocalizedText.Of(TextCodes.Capital.Civilian), CapitalNature.Stock, false, closing, opening, reference,
                secondary: belligerent.Civilian.LivingStandard * 100d,
                secondaryLabel: LocalizedText.Of(TextCodes.Capital.LivingStandard),
                secondaryUnit: LocalizedText.Of(TextCodes.Capital.UnitPreWarShare)),

            Build(Arms, LocalizedText.Of(TextCodes.Capital.Arms), CapitalNature.Stock, false, closing, opening, reference,
                secondary: closing.GetValueOrDefault(Arms) / CapitalisationMultiple,
                secondaryLabel: LocalizedText.Of(TextCodes.Capital.YearProduction),
                secondaryUnit: LocalizedText.Of(TextCodes.Capital.UnitBillions)),

            // No threshold rule: this post IS its own threshold. The mass reaching zero is the
            // regime falling, and a line drawn there would only repeat the floor of the column.
            Build(Regime, LocalizedText.Of(TextCodes.Capital.Regime), CapitalNature.AnnualFlow, false, closing, opening, reference,
                secondary: belligerent.Politics.PoliticalCapital,
                secondaryLabel: LocalizedText.Of(TextCodes.Capital.PoliticalCapital),
                secondaryUnit: LocalizedText.Of(TextCodes.Capital.UnitPoints)),

            Build(Foreign,
                LocalizedText.Of(granted ? TextCodes.Capital.ForeignReceived : TextCodes.Capital.ForeignBought),
                CapitalNature.AnnualFlow, false, closing, opening, reference,
                secondary: tank,
                secondaryLabel: LocalizedText.Of(granted ? TextCodes.Capital.BackersWill : TextCodes.Capital.AbilityToPay),
                secondaryUnit: LocalizedText.Of(granted ? TextCodes.Capital.UnitOutOfHundred : TextCodes.Capital.UnitBillions)),

            // Le soutien étranger, c'est ce qu'on reçoit ; le soutien international, c'est la
            // position diplomatique qui décide si le premier continue. Une seule quantité, lue
            // à l'endroit pour qui la possède et à l'envers pour qui la lui prend — et la seule
            // du bandeau qui ne soit pas un capital, donc la seule qui ne se compte pas en
            // dollars et n'entre dans aucun des deux totaux.
            Build(International,
                LocalizedText.Of(invader ? TextCodes.Capital.InternationalLatitude : TextCodes.Capital.InternationalPressure),
                CapitalNature.Position, !invader, closing, opening, reference,
                // Vu de Moscou, ce que l'achat coûte vraiment ; vu de Kyiv, le canal lent —
                // les composants, qui pèsent double dans la latitude et sont le seul des trois
                // qui décide. La volonté des soutiens, elle, se lit déjà sur le poste voisin :
                // deux fois la même mesure dans la même infobulle ne dit pas deux choses.
                secondary: invader
                    ? belligerent.Foreign.Dependency * 100d
                    : state.Invader.Sanctions.ComponentSeverity * 100d,
                secondaryLabel: LocalizedText.Of(invader ? TextCodes.Capital.SupplierDependency : TextCodes.Capital.ComponentLock),
                secondaryUnit: LocalizedText.Of(TextCodes.Capital.UnitPercent)),
        ];

        return [.. posts.Select(post => Attribute(post, belligerent, closing, opening, cardsPlayed, incoming))];
    }

    private static CapitalPost Build(
        string code,
        LocalizedText name,
        CapitalNature nature,
        bool inverted,
        IReadOnlyDictionary<string, double> closing,
        IReadOnlyDictionary<string, double> opening,
        IReadOnlyDictionary<string, double> reference,
        double? threshold = null,
        LocalizedText? thresholdLabel = null,
        double? secondary = null,
        LocalizedText? secondaryLabel = null,
        LocalizedText? secondaryUnit = null)
    {
        double value = closing.GetValueOrDefault(code);
        double open = opening.TryGetValue(code, out double measured) ? measured : value;

        return new CapitalPost
        {
            Code = code,
            Name = name,
            // Six posts of the seven are money; the diplomatic position is not a possession
            // and is read on a hundred, which is exactly why it has no column of its own.
            Unit = LocalizedText.Of(nature == CapitalNature.Position
                ? TextCodes.Capital.UnitOutOfHundred
                : TextCodes.Capital.UnitBillions),
            Nature = nature,
            Inverted = inverted,
            Value = value,
            Opening = open,
            Reference = reference.GetValueOrDefault(code),
            Threshold = threshold,
            ThresholdLabel = threshold is null ? null : thresholdLabel,
            Secondary = secondaryLabel is null ? null : secondary,
            SecondaryLabel = secondaryLabel,
            SecondaryUnit = secondaryLabel is null ? null : secondaryUnit,
        };
    }

    /// <summary>
    /// What the side owns, in billions: the fund, and four productions capitalised at five
    /// years each.
    ///
    /// Two totals rather than one, and never their sum. Adding an asset to a year of aid would
    /// be adding a holding to an income, which is the arithmetic every wartime communiqué does
    /// and none of them survives. Two figures per camp force the reader to ask the only
    /// question that matters — is this side living off what it owns, or off what it is handed.
    /// </summary>
    public static double Stock(IReadOnlyList<CapitalPost> posts)
    {
        return Total(posts, CapitalNature.Stock);
    }

    /// <summary>A year of what the side is given or can still spend holding on, in billions.</summary>
    public static double Flow(IReadOnlyList<CapitalPost> posts)
    {
        return Total(posts, CapitalNature.AnnualFlow);
    }

    private static double Total(IReadOnlyList<CapitalPost> posts, CapitalNature nature)
    {
        double total = 0d;
        foreach (CapitalPost post in posts)
        {
            if (post.Nature != nature)
            {
                continue;
            }

            // A charge is a liability, and a liability comes off a balance sheet rather than
            // swelling it: the Ukrainian oil bill is money leaving, not capital held.
            total += post.Inverted ? -post.Value : post.Value;
        }

        return total;
    }

    /// <summary>
    /// Splits the quarter's move into regeneration, ordinary consumption and destruction.
    /// A fall is only ever called a destruction when something can be NAMED for it — an
    /// incoming wave or a card of this very turn. An unattributed red figure is a figure
    /// the reader cannot argue with, which is exactly what a teaching tool must refuse.
    /// </summary>
    private static CapitalPost Attribute(
        CapitalPost post,
        Belligerent belligerent,
        IReadOnlyDictionary<string, double> closing,
        IReadOnlyDictionary<string, double> opening,
        IReadOnlyList<EventCard> cardsPlayed,
        StrikeResolution? incoming)
    {
        // In capital terms, not in unit terms: a rising oil bill is a falling capital.
        double move = post.Inverted ? post.Opening - post.Value : post.Value - post.Opening;

        if (move >= 0d)
        {
            return Clone(post, regeneration: move);
        }

        double loss = -move;
        LocalizedText? cause = Cause(post.Code, belligerent, cardsPlayed, incoming);

        if (cause is null)
        {
            return Clone(post, consumption: loss);
        }

        // Turbine halls and assembly lines do not come back inside a war; refining does, and a
        // treasury refills. The padlock is drawn on what the state itself says is permanent.
        bool permanent = Grew(closing, opening, "grid_permanent") && post.Code == Grid;
        permanent |= Grew(closing, opening, "civilian_permanent") && post.Code == Civilian;
        permanent |= post.Code == Arms;

        return Clone(post, destruction: loss, cause: cause, permanent: permanent);
    }

    private static bool Grew(
        IReadOnlyDictionary<string, double> closing,
        IReadOnlyDictionary<string, double> opening,
        string key)
    {
        return closing.GetValueOrDefault(key) > opening.GetValueOrDefault(key) + 0.001d;
    }

    private static LocalizedText? Cause(
        string code,
        Belligerent belligerent,
        IReadOnlyList<EventCard> cardsPlayed,
        StrikeResolution? incoming)
    {
        if (incoming is not null && PostOf(incoming.Target) == code && incoming.DamageInflicted > 0d)
        {
            return Label(incoming.Target);
        }

        foreach (EventCard card in cardsPlayed)
        {
            foreach (CardEffect effect in card.Effects)
            {
                if (PostOf(effect.Kind) != code)
                {
                    continue;
                }

                bool hitsThisSide = string.IsNullOrWhiteSpace(effect.TargetSideCode)
                    || string.Equals(effect.TargetSideCode, belligerent.Side.Code, StringComparison.OrdinalIgnoreCase);

                if (hitsThisSide)
                {
                    // Le titre vient du fichier de cartes : il traverse sans être traduit.
                    return LocalizedText.Of(TextCodes.Verbatim, card.Title);
                }
            }
        }

        return null;
    }

    private static CapitalPost Clone(
        CapitalPost post,
        double regeneration = 0d,
        double consumption = 0d,
        double destruction = 0d,
        LocalizedText? cause = null,
        bool permanent = false)
    {
        return new CapitalPost
        {
            Code = post.Code,
            Name = post.Name,
            Unit = post.Unit,
            Nature = post.Nature,
            Inverted = post.Inverted,
            Value = post.Value,
            Opening = post.Opening,
            Reference = post.Reference,
            Threshold = post.Threshold,
            ThresholdLabel = post.ThresholdLabel,
            Secondary = post.Secondary,
            SecondaryLabel = post.SecondaryLabel,
            SecondaryUnit = post.SecondaryUnit,
            Regeneration = regeneration,
            Consumption = consumption,
            Destruction = destruction,
            DestructionCause = cause,
            PermanentLoss = permanent,
        };
    }

    /// <summary>
    /// The capital as one number, base 100 at turn one.
    ///
    /// Not a minimum — that law governs the flows the front burns, where the shortest stave
    /// caps everything; a capital does not work that way, an empty treasury is survivable for
    /// a few quarters. Not a sum either, which would let 310 billions of reserve hide a dead
    /// grid. A geometric mean punishes imbalance without letting one post decide alone, and
    /// the floor says the rest in one figure: no side dies of having lost its oil, it dies of
    /// having lost it at the same time as everything else.
    /// </summary>
    public static double Index(IReadOnlyList<CapitalPost> posts)
    {
        if (posts.Count == 0)
        {
            return 100d;
        }

        double product = 1d;
        foreach (CapitalPost post in posts)
        {
            product *= Math.Clamp(post.Index, IndexFloor, IndexCeiling);
        }

        return Math.Pow(product, 1d / posts.Count);
    }

    /// <summary>
    /// The consequence ribbon: the sharpest named destruction of the quarter, and what fell
    /// behind it. Returns null on a quiet quarter, and a quiet quarter drawing nothing is
    /// itself information.
    /// </summary>
    public static CapitalChain? Chain(IReadOnlyList<CapitalPost> posts)
    {
        CapitalPost? origin = null;
        double sharpest = 0d;

        foreach (CapitalPost post in posts)
        {
            if (post.DestructionCause is null || post.Reference <= 0d)
            {
                continue;
            }

            double points = post.OpeningIndex - post.Index;
            if (points > sharpest)
            {
                sharpest = points;
                origin = post;
            }
        }

        if (origin is null || sharpest < 0.5d)
        {
            return null;
        }

        CapitalChain chain = new() { Origin = origin.DestructionCause! };
        chain.Links.Add(new CapitalLink
        {
            PostCode = origin.Code,
            Label = origin.Name,
            PercentDelta = origin.PercentDelta,
        });

        // The living standard is not a post — it is the road between a burnt warehouse and an
        // angry population, and the ribbon is the one place it has to be visible. It is
        // published as a percentage of its pre-war level, so its distance to a hundred is
        // already the move the ribbon prints.
        if (origin.Code == Civilian && origin.Secondary is double standard)
        {
            chain.Links.Add(new CapitalLink
            {
                PostCode = "living_standard",
                Label = LocalizedText.Of(TextCodes.Capital.LivingStandardLink),
                PercentDelta = standard - 100d,
            });
        }

        foreach (string code in Downstream.GetValueOrDefault(origin.Code, []))
        {
            CapitalPost? next = posts.FirstOrDefault(post => post.Code == code);
            if (next is null || next.Reference <= 0d)
            {
                continue;
            }

            double moved = next.PercentDelta;
            if (moved < -0.05d)
            {
                chain.Links.Add(new CapitalLink
                {
                    PostCode = next.Code,
                    Label = next.Name,
                    PercentDelta = moved,
                });
            }
        }

        return chain.Links.Count > 1 ? chain : null;
    }
}
