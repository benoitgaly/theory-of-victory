using TheoryOfVictory.Core;

namespace TheoryOfVictory.Engine;

/// <summary>
/// Reads the war capital of one side. Like <see cref="PressureAnalyser"/>, this is not an
/// eleventh phase: it decides nothing and changes nothing, it looks at what the ten just did.
///
/// The band it feeds exists to make one thing visible — a front that still looks healthy while
/// the capital feeding it gives way. That only works if a fall is attributed: an ordinary
/// draw-down and a destroyed turbine hall are the same number and not the same event.
/// </summary>
public static class CapitalReader
{
    private const double DaysPerTurn = 91.25d;

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

    private static string Label(StrikeTarget target)
    {
        return target switch
        {
            StrikeTarget.PowerGrid => "frappe sur le réseau électrique",
            StrikeTarget.Refining => "frappe sur le raffinage",
            StrikeTarget.Industry => "frappe sur les usines d'armement",
            StrikeTarget.CivilianIndustry => "frappe sur les entrepôts civils",
            _ => "frappe en profondeur",
        };
    }

    /// <summary>
    /// The raw scalars of one side, taken identically before and after the ten phases. Two
    /// readings of the same ruler: anything derived from them is a real move, never an artefact.
    /// </summary>
    public static Dictionary<string, double> Measure(Belligerent belligerent, GameState state)
    {
        Economy economy = belligerent.Economy;

        // One post, two opposite economies: the barrel is a receipt on the side that exports
        // it and a bill on the side that imports it. The bill is read as a capital further on,
        // which is where a rising barrel starts costing Ukraine.
        double oil = economy.OilExportCapacityMbd > 0d
            ? economy.LastTurnOilRevenueBillions
            : economy.OilImportMbd * DaysPerTurn * state.OilPrice / 1000d;

        double tank = belligerent.Foreign.Mode == SupportMode.Granted
            ? belligerent.Politics.ExternalWill
            : Math.Min(economy.TreasuryBillions, belligerent.Foreign.SupplyCeilingBillions * belligerent.Foreign.PricePremium);

        return new Dictionary<string, double>
        {
            [Reserves] = economy.ReservesBillions,
            [Grid] = belligerent.Grid.AvailableCapacityGw,
            ["grid_permanent"] = belligerent.Grid.PermanentDamageGw,
            [Oil] = oil,
            // The plant that still stands, not what it managed to deliver: a factory left dark
            // by a strike on the grid is idle, not destroyed, and booking it as lost capital
            // would count the same wave twice — once on the grid post, once here.
            [Civilian] = belligerent.Civilian.CapacityBillions * belligerent.Civilian.Integrity,
            ["living_standard"] = belligerent.Civilian.LivingStandard,
            ["civilian_permanent"] = belligerent.Civilian.PermanentDamage,
            [Arms] = belligerent.Industry.TotalCapacityValueBillions(),
            [Regime] = Math.Max(0d, Phases.ControlPhase.RegimeCollapseStress - belligerent.Politics.RegimeStress),
            [Foreign] = tank,
            [International] = Latitude(state.Invader),
        };
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
        double tap = granted
            ? belligerent.Foreign.EffectiveGrantBillions
            : belligerent.AllocationThisTurn.GetValueOrDefault("foreign");

        List<CapitalPost> posts =
        [
            Build(Reserves, "Réserves monétaires", "Md", 0, false, closing, opening, reference,
                threshold: draw > 0.01d ? draw * ReserveWarningQuarters : null,
                secondary: draw, secondaryLabel: "ponction du trimestre"),

            Build(Grid, "Centrales électriques", "GW", 0, false, closing, opening, reference,
                threshold: belligerent.Grid.DemandGw(Season.Winter),
                secondary: belligerent.Grid.DemandGw(state.Season), secondaryLabel: "demande"),

            Build(Oil, exporter ? "Pétrole — recette" : "Pétrole — facture", "Md", 1, !exporter,
                closing, opening, reference,
                secondary: belligerent.Economy.RefiningIntegrity * 100d,
                secondaryLabel: exporter ? "raffinage" : null),

            Build(Civilian, "Usines civiles", "Md", 0, false, closing, opening, reference,
                secondary: belligerent.Civilian.LivingStandard, secondaryLabel: "niveau de vie"),

            Build(Arms, "Usines d'armement", "Md/tour", 2, false, closing, opening, reference,
                secondary: belligerent.Sanctions.ProductionCeilingMultiplier * 100d,
                secondaryLabel: "plafond composants"),

            // No threshold rule: this post IS its own threshold. The mass reaching zero is the
            // regime falling, and a line drawn there would only repeat the floor of the column.
            Build(Regime, "Tenue du pouvoir", "pts", 0, false, closing, opening, reference,
                secondary: belligerent.Politics.PoliticalCapital, secondaryLabel: "capital politique"),

            Build(Foreign, granted ? "Soutien étranger — reçu" : "Soutien étranger — acheté",
                granted ? "/100" : "Md", granted ? 0 : 1, false, closing, opening, reference,
                secondary: tap, secondaryLabel: granted ? "versé ce trimestre" : "acheté ce trimestre"),

            // Le soutien étranger, c'est ce qu'on reçoit ; le soutien international, c'est la
            // position diplomatique qui décide si le premier continue. Deux leviers, deux
            // cartouches — et une seule quantité, lue à l'endroit pour qui la possède et à
            // l'envers pour qui la lui prend.
            Build(International,
                invader ? "Soutien international — latitude" : "Soutien international — pression obtenue",
                "/100", 0, !invader, closing, opening, reference,
                secondary: invader ? belligerent.Foreign.Dependency * 100d : belligerent.Politics.ExternalWill,
                secondaryLabel: invader ? "dépendance aux fournisseurs" : "volonté des soutiens"),
        ];

        return [.. posts.Select(post => Attribute(post, belligerent, closing, opening, cardsPlayed, incoming))];
    }

    private static CapitalPost Build(
        string code,
        string name,
        string unit,
        int decimals,
        bool inverted,
        IReadOnlyDictionary<string, double> closing,
        IReadOnlyDictionary<string, double> opening,
        IReadOnlyDictionary<string, double> reference,
        double? threshold = null,
        double? secondary = null,
        string? secondaryLabel = null)
    {
        double value = closing.GetValueOrDefault(code);
        double open = opening.TryGetValue(code, out double measured) ? measured : value;

        // Le pétrole ukrainien est une facture : elle se lit comme une facture, à la baisse
        // quand elle baisse. Le soutien international est une latitude partagée : elle se lit
        // du point de vue du camp, sans quoi le bandeau mettrait un moins sous celui qui vient
        // de gagner. Seul ce poste-là inverse le signe imprimé.
        double display = code == International && inverted ? open - value : value - open;

        return new CapitalPost
        {
            Code = code,
            Name = name,
            Unit = unit,
            Decimals = decimals,
            Inverted = inverted,
            Value = value,
            Opening = open,
            Reference = reference.GetValueOrDefault(code),
            Threshold = threshold,
            Secondary = secondaryLabel is null ? null : secondary,
            SecondaryLabel = secondaryLabel,
            DisplayDelta = display,
        };
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
        string? cause = Cause(post.Code, belligerent, cardsPlayed, incoming);

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

    private static string? Cause(
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
                    return card.Title;
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
        string? cause = null,
        bool permanent = false)
    {
        return new CapitalPost
        {
            Code = post.Code,
            Name = post.Name,
            Unit = post.Unit,
            Decimals = post.Decimals,
            Inverted = post.Inverted,
            Value = post.Value,
            Opening = post.Opening,
            Reference = post.Reference,
            Threshold = post.Threshold,
            Secondary = post.Secondary,
            SecondaryLabel = post.SecondaryLabel,
            DisplayDelta = post.DisplayDelta,
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

    /// <summary>How far a post moved this quarter, relative to where it opened.</summary>
    private static double Relative(CapitalPost post)
    {
        if (post.OpeningIndex <= 0.01d)
        {
            return 0d;
        }

        return (post.Index - post.OpeningIndex) / post.OpeningIndex * 100d;
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
            PercentDelta = Relative(origin),
        });

        // The living standard is not a post — it is the road between a burnt warehouse and an
        // angry population, and the ribbon is the one place it has to be visible.
        if (origin.Code == Civilian && origin.Secondary is double standard)
        {
            chain.Links.Add(new CapitalLink
            {
                PostCode = "living_standard",
                Label = "Niveau de vie",
                PercentDelta = (standard - 1d) * 100d,
            });
        }

        foreach (string code in Downstream.GetValueOrDefault(origin.Code, []))
        {
            CapitalPost? next = posts.FirstOrDefault(post => post.Code == code);
            if (next is null || next.Reference <= 0d)
            {
                continue;
            }

            double moved = Relative(next);
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
