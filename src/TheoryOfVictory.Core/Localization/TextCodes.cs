namespace TheoryOfVictory.Core.Localization;

/// <summary>
/// Les faits que le moteur peut énoncer. Un code par fait, en anglais comme tous les
/// identifiants du projet, et rien d'autre : la phrase qui va avec vit dans le
/// <see cref="Phrasebook"/>, et elle n'y vit qu'une fois par langue.
///
/// Les tests s'appuient sur ces codes plutôt que sur des morceaux de phrase. Une vérification
/// qui cherchait « mobilisés » dans un récit cassait à la première virgule déplacée et passait
/// pour une régression du moteur ; elle porte maintenant sur ce que le moteur a DIT.
/// </summary>
public static class TextCodes
{
    /// <summary>
    /// Le passe-droit du texte qui vient des DONNÉES : un titre de carte, une famille déjà
    /// écrite dans son fichier. Il traverse le moteur sans être traduit, et la seule chose que
    /// le livre de phrases en fait est de le recopier.
    /// </summary>
    public const string Verbatim = "verbatim";

    /// <summary>
    /// Un camp se nomme de trois façons, et le français a besoin des trois : le nom nu sur une
    /// étiquette, le nom AVEC SON ARTICLE au milieu d'une phrase — « les dépôts de la Russie »
    /// —, et le même en tête de phrase, où l'article prend la majuscule. Une règle qui
    /// déduirait l'article de l'orthographe devinerait le genre ; on l'écrit.
    /// </summary>
    public static class Side
    {
        public const string Invader = "side.invader";
        public const string Defender = "side.defender";
        public const string InvaderInProse = "side.invader.in-prose";
        public const string DefenderInProse = "side.defender.in-prose";
        public const string InvaderOpening = "side.invader.opening";
        public const string DefenderOpening = "side.defender.opening";
        public const string EitherSide = "side.either";
    }

    public static class Resource
    {
        public const string Weapons = "resource.weapons";
        public const string Fuel = "resource.fuel";
        public const string Food = "resource.food";
        public const string StrikeDrones = "resource.strike-drones";
        public const string Missiles = "resource.missiles";
        public const string CheapInterceptors = "resource.cheap-interceptors";
        public const string HeavyInterceptors = "resource.heavy-interceptors";

        /// <summary>Le même nom au milieu d'une phrase, où le français le veut en minuscule.</summary>
        public const string WeaponsInline = "resource.weapons.inline";
        public const string FuelInline = "resource.fuel.inline";
        public const string FoodInline = "resource.food.inline";
        public const string StrikeDronesInline = "resource.strike-drones.inline";
        public const string MissilesInline = "resource.missiles.inline";
        public const string CheapInterceptorsInline = "resource.cheap-interceptors.inline";
        public const string HeavyInterceptorsInline = "resource.heavy-interceptors.inline";
        public const string UnitsInline = "resource.units.inline";
    }

    public static class Season
    {
        public const string Winter = "season.winter";
        public const string Spring = "season.spring";
        public const string Summer = "season.summer";
        public const string Autumn = "season.autumn";
    }

    /// <summary>
    /// Les sept postes du capital de guerre : leur nom, leur seuil, leur seconde lecture, leur
    /// unité, et ce qui peut les détruire. Un poste porte tout cela en propre parce que le
    /// bandeau l'écrit tel quel, et que rien de tout cela n'est un chiffre.
    /// </summary>
    public static class Capital
    {
        public const string Reserves = "capital.reserves";
        public const string Grid = "capital.grid";
        public const string OilRevenue = "capital.oil-revenue";
        public const string OilBill = "capital.oil-bill";
        public const string Civilian = "capital.civilian";
        public const string Arms = "capital.arms";
        public const string Regime = "capital.regime";
        public const string ForeignReceived = "capital.foreign-received";
        public const string ForeignBought = "capital.foreign-bought";
        public const string InternationalLatitude = "capital.international.latitude";
        public const string InternationalPressure = "capital.international.pressure";
        public const string LivingStandardLink = "capital.link.living-standard";

        public const string ReserveThreshold = "capital.threshold.reserve";
        public const string WinterDemand = "capital.threshold.winter-demand";

        public const string QuarterDraw = "capital.secondary.quarter-draw";
        public const string StandingPlant = "capital.secondary.standing-plant";
        public const string YearRevenue = "capital.secondary.year-revenue";
        public const string YearBill = "capital.secondary.year-bill";
        public const string YearProduction = "capital.secondary.year-production";
        public const string LivingStandard = "capital.secondary.living-standard";
        public const string PoliticalCapital = "capital.secondary.political-capital";
        public const string BackersWill = "capital.secondary.backers-will";
        public const string AbilityToPay = "capital.secondary.ability-to-pay";
        public const string SupplierDependency = "capital.secondary.supplier-dependency";
        public const string ComponentLock = "capital.secondary.component-lock";

        public const string UnitBillions = "capital.unit.billions";
        public const string UnitOutOfHundred = "capital.unit.out-of-hundred";
        public const string UnitGigawatts = "capital.unit.gigawatts";
        public const string UnitPoints = "capital.unit.points";
        public const string UnitPercent = "capital.unit.percent";
        public const string UnitPreWarShare = "capital.unit.pre-war-share";

        public const string GridStrike = "capital.cause.grid-strike";
        public const string RefiningStrike = "capital.cause.refining-strike";
        public const string ArmsStrike = "capital.cause.arms-strike";
        public const string CivilianStrike = "capital.cause.civilian-strike";
        public const string DeepStrike = "capital.cause.deep-strike";
    }

    /// <summary>Ce qu'une alerte de pression annonce, et ce qu'elle explique.</summary>
    public static class Alert
    {
        public const string DepotTitle = "alert.depot.title";
        public const string DepotDetail = "alert.depot.detail";
        public const string SustainmentTitle = "alert.sustainment.title";
        public const string SustainmentDetail = "alert.sustainment.detail";
        public const string ReservesTitle = "alert.reserves.title";
        public const string ReservesDetail = "alert.reserves.detail";
        public const string FundingGapTitle = "alert.funding-gap.title";
        public const string FundingGapDetail = "alert.funding-gap.detail";
        public const string CollapseNow = "alert.collapse.now";
        public const string CollapseIn = "alert.collapse.in";
        public const string CollapseDetail = "alert.collapse.detail";
        public const string GenerationTitle = "alert.generation.title";
        public const string GenerationDetail = "alert.generation.detail";
        public const string ApparatusStressTitle = "alert.apparatus-stress.title";
        public const string ApparatusStressDetail = "alert.apparatus-stress.detail";
        public const string WillStressTitle = "alert.will-stress.title";
        public const string NegotiationDetail = "alert.negotiation.detail";
        public const string WinterSheddingTitle = "alert.winter-shedding.title";
        public const string WinterSheddingIndustrial = "alert.winter-shedding.industrial";
        public const string WinterSheddingCivilian = "alert.winter-shedding.civilian";
        public const string EdgeDecayTitle = "alert.edge-decay.title";
        public const string EdgeDecayDetail = "alert.edge-decay.detail";
        public const string ExternalWillTitle = "alert.external-will.title";
        public const string ExternalWillDetail = "alert.external-will.detail";
    }

    /// <summary>Ce que le trimestre a produit, phrase par phrase.</summary>
    public static class Narrative
    {
        public const string MobilisationEmpty = "narrative.mobilisation-empty";
        public const string Mobilised = "narrative.mobilised";
        public const string WarBudgetCapped = "narrative.war-budget-capped";
        public const string ForeignPurchases = "narrative.foreign-purchases";
        public const string UnspentMoney = "narrative.unspent-money";
        public const string PayNotCovered = "narrative.pay-not-covered";
        public const string SustainmentUnpaid = "narrative.sustainment-unpaid";
        public const string Recruited = "narrative.recruited";
        public const string IndustrialShedding = "narrative.industrial-shedding";
        public const string CivilianCuts = "narrative.civilian-cuts";
        public const string NewCapacity = "narrative.new-capacity";
        public const string ReserveDraw = "narrative.reserve-draw";
        public const string DelayedEffect = "narrative.delayed-effect";
        public const string CardCountered = "narrative.card-countered";
        public const string CardOverdraft = "narrative.card-overdraft";
        public const string StrikeSaturated = "narrative.strike-saturated";
        public const string Strike = "narrative.strike";
        public const string Rupture = "narrative.rupture";
        public const string QuietQuarter = "narrative.quiet-quarter";

        /// <summary>
        /// Le délitement, et c'est la seule phrase du moteur qui nomme une armée : « l'armée
        /// russe », « l'armée ukrainienne ». L'adjectif s'accorde, et aucune règle ne devine
        /// l'accord — il y a donc une phrase par camp, écrite en entier.
        /// </summary>
        public const string NoOnePaysInvader = "narrative.no-one-pays.invader";
        public const string NoOnePaysDefender = "narrative.no-one-pays.defender";
        public const string ArmisticeInvader = "narrative.armistice.invader";
        public const string ArmisticeDefender = "narrative.armistice.defender";
    }

    /// <summary>
    /// Ce qu'une vague frappe. Une phrase par cible ET par camp : « les usines d'armement
    /// ukrainiennes » et « le réseau électrique de l'arrière russe » n'accordent pas leur
    /// adjectif de la même façon, et la grammaire n'appartient pas au moteur.
    /// </summary>
    public static class Target
    {
        public const string GridInvader = "target.grid.invader";
        public const string GridDefender = "target.grid.defender";
        public const string RefiningInvader = "target.refining.invader";
        public const string RefiningDefender = "target.refining.defender";
        public const string IndustryInvader = "target.industry.invader";
        public const string IndustryDefender = "target.industry.defender";
        public const string LogisticsInvader = "target.logistics.invader";
        public const string LogisticsDefender = "target.logistics.defender";
        public const string CivilianInvader = "target.civilian.invader";
        public const string CivilianDefender = "target.civilian.defender";
        public const string Rear = "target.rear";
    }

    /// <summary>Ce qu'un secteur a fait du trimestre, et comment il s'appelle.</summary>
    public static class Sector
    {
        public const string HolderCollapsed = "sector.holder-collapsed";
        public const string NoMovement = "sector.no-movement";
        public const string Nibbling = "sector.nibbling";
        public const string Advance = "sector.advance";
        public const string Breakthrough = "sector.breakthrough";

        public const string Kharkiv = "sector.kharkiv";
        public const string Kupiansk = "sector.kupiansk";
        public const string Lyman = "sector.lyman";
        public const string Bakhmut = "sector.bakhmut";
        public const string Pokrovsk = "sector.pokrovsk";
        public const string Vuhledar = "sector.vuhledar";
        public const string Zaporizhzhia = "sector.zaporizhzhia";
        public const string Kherson = "sector.kherson";
    }

    /// <summary>Comment la guerre se termine, et pourquoi.</summary>
    public static class Outcome
    {
        public const string MilitaryCollapseTitle = "outcome.military-collapse.title";
        public const string MilitaryCollapseExplanation = "outcome.military-collapse.explanation";
        public const string RegimeCollapseTitle = "outcome.regime-collapse.title";
        public const string RegimeCollapseExplanation = "outcome.regime-collapse.explanation";
        public const string NegotiatedTitle = "outcome.negotiated.title";
        public const string NegotiatedExplanation = "outcome.negotiated.explanation";
        public const string MutualExhaustionTitle = "outcome.mutual-exhaustion.title";
        public const string MutualExhaustionExplanation = "outcome.mutual-exhaustion.explanation";
        public const string FrozenFrontTitle = "outcome.frozen-front.title";
        public const string FrozenFrontExplanation = "outcome.frozen-front.explanation";
        public const string ArmisticeTitle = "outcome.armistice.title";
        public const string ArmisticeExplanation = "outcome.armistice.explanation";
        public const string ArmisticeCauseRegime = "outcome.armistice.cause.regime";
        public const string ArmisticeCauseWill = "outcome.armistice.cause.will";
        public const string ArmisticeCauseGeneration = "outcome.armistice.cause.generation";
        public const string Unresolved = "outcome.unresolved";

        /// <summary>Pourquoi ce camp a rompu, en trois mots, pour le journal de partie.</summary>
        public const string ReasonGeneration = "outcome.reason.generation";
        public const string ReasonRegime = "outcome.reason.regime";
        public const string ReasonWill = "outcome.reason.will";
    }

    /// <summary>Ce qu'une carte fait, ligne par ligne, sur son cartouche de règles.</summary>
    public static class Card
    {
        public const string TypeLine = "card.type-line";
        public const string Delayed = "card.delayed";
        public const string Permanent = "card.type.permanent";
        public const string Instant = "card.type.instant";
        public const string SlowRitual = "card.type.slow-ritual";
        public const string Counter = "card.type.counter";
        public const string Plain = "card.type.plain";

        public const string FamilyEconomic = "card.family.economic";
        public const string FamilyWesternPolitics = "card.family.western-politics";
        public const string FamilyDomesticPolitics = "card.family.domestic-politics";
        public const string FamilyEnergy = "card.family.energy";
        public const string FamilyMilitary = "card.family.military";
        public const string FamilyExternal = "card.family.external";

        public const string OilPrice = "card.effect.oil-price";
        public const string AidPledge = "card.effect.aid-pledge";
        public const string AidDisbursement = "card.effect.aid-disbursement";
        public const string ForeignCeiling = "card.effect.foreign-ceiling";
        public const string BarrelDiscount = "card.effect.barrel-discount";
        public const string CustomsFriction = "card.effect.customs-friction";
        public const string ComponentAccess = "card.effect.component-access";
        public const string Mobilisation = "card.effect.mobilisation";
        public const string RecruitmentCost = "card.effect.recruitment-cost";
        public const string Morale = "card.effect.morale";
        public const string Discontent = "card.effect.discontent";
        public const string EliteCohesion = "card.effect.elite-cohesion";
        public const string BackersWill = "card.effect.backers-will";
        public const string Corruption = "card.effect.corruption";
        public const string PoliticalCapital = "card.effect.political-capital";
        public const string TacticalDroneEdge = "card.effect.tactical-drone-edge";
        public const string DeepStrikeEdge = "card.effect.deep-strike-edge";
        public const string CounterDroneEdge = "card.effect.counter-drone-edge";
        public const string IndustrialCapacity = "card.effect.industrial-capacity";
        public const string GridDestroyed = "card.effect.grid-destroyed";
        public const string CivilianDestroyed = "card.effect.civilian-destroyed";
        public const string Refining = "card.effect.refining";
        public const string Logistics = "card.effect.logistics";
        public const string Treasury = "card.effect.treasury";
        public const string StockDelta = "card.effect.stock-delta";
        public const string AidConditionality = "card.effect.aid-conditionality";
        public const string Unnamed = "card.effect.unnamed";
    }

    /// <summary>Ce que valait une carte, une fois la guerre rejouée sans elle.</summary>
    public static class Verdict
    {
        public const string Decisive = "verdict.decisive";
        public const string BringsForward = "verdict.brings-forward";
        public const string Lengthens = "verdict.lengthens";
        public const string NoMeasurableEffect = "verdict.no-measurable-effect";
        public const string WithoutSeparating = "verdict.without-separating";
        public const string InFavourOf = "verdict.in-favour-of";

        public const string ReadableFrozenFront = "verdict.readable.frozen-front";
        public const string ReadableMutualExhaustion = "verdict.readable.mutual-exhaustion";
        public const string ReadableMilitaryCollapse = "verdict.readable.military-collapse";
        public const string ReadableRegimeCollapse = "verdict.readable.regime-collapse";
        public const string ReadableNegotiated = "verdict.readable.negotiated";
        public const string ReadableUnresolved = "verdict.readable.unresolved";
    }

    /// <summary>Les trois familles de deck que le banc d'essai fait s'affronter.</summary>
    public static class Deck
    {
        public const string DeepStrike = "deck.deep-strike";
        public const string FrontalAttrition = "deck.frontal-attrition";
        public const string Political = "deck.political";
    }

    /// <summary>Les trois déroulés, et ce que chacun met à l'épreuve.</summary>
    public static class Scenario
    {
        public const string ResolveTitle = "scenario.resolve.title";
        public const string ResolveSubtitle = "scenario.resolve.subtitle";
        public const string ResolveDescription = "scenario.resolve.description";
        public const string HoldsTitle = "scenario.holds.title";
        public const string HoldsSubtitle = "scenario.holds.subtitle";
        public const string HoldsDescription = "scenario.holds.description";
        public const string CollapseTitle = "scenario.collapse.title";
        public const string CollapseSubtitle = "scenario.collapse.subtitle";
        public const string CollapseDescription = "scenario.collapse.description";
    }
}
