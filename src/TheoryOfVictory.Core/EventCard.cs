namespace TheoryOfVictory.Core;

/// <summary>Magic-style timing classes. V1 only plays them on scripted turns.</summary>
public enum CardType
{
    /// <summary>Stays in play and shifts a flow every turn. The accumulating kind.</summary>
    Permanent = 0,

    /// <summary>A single shot: a strike, an emergency delivery, a diplomatic coup.</summary>
    Instant = 1,

    /// <summary>Paid now, effective in a few turns. Mobilisation is exactly this.</summary>
    SlowRitual = 2,

    /// <summary>Cancels or blunts an opposing card. Creates the bluff in V2.</summary>
    Counter = 3,
}

/// <summary>
/// A finite, business-meaningful effect vocabulary. Adding a card is data;
/// only adding a new kind of effect touches code.
/// </summary>
public enum EffectKind
{
    OilPriceDelta,
    AidPledgeDelta,
    AidDisbursementRate,
    ForeignSupplyCeilingDelta,
    SanctionsPriceDelta,
    SanctionsFrictionDelta,
    SanctionsComponentDelta,
    MobilisationWave,
    RecruitmentCostMultiplier,
    MoraleDelta,
    PopularDiscontentDelta,
    EliteCohesionDelta,
    ExternalWillDelta,
    CorruptionDelta,
    PoliticalCapitalDelta,
    InnovationTacticalJump,
    InnovationStrikeJump,
    InnovationCounterJump,
    ProductionCapacityMultiplier,
    GridPermanentDamage,
    CivilianIndustryDamage,
    RefiningIntegrityDelta,
    LogisticsIntegrityDelta,
    TreasuryDelta,

    /// <summary>
    /// Moves the sovereign reserve itself, not the quarter's cash. The one effect that can
    /// immobilise a holding rather than spend it: a freeze does not empty the fund, it puts it
    /// out of reach, and from the war's point of view those are the same thing.
    /// </summary>
    ReservesDelta,
    StockDelta,
    ConditionalityDelta,
}

/// <summary>One typed effect, targeted at a side and optionally at a resource.</summary>
public sealed class CardEffect
{
    public required EffectKind Kind { get; init; }

    /// <summary>Which side is affected. Null means both.</summary>
    public string? TargetSideCode { get; init; }

    public double Value { get; init; }

    /// <summary>Resource code for stock-oriented effects.</summary>
    public string? ResourceCode { get; init; }

    /// <summary>Turns before the effect lands. Slow rituals use this.</summary>
    public int DelayTurns { get; init; }
}

public sealed class EventCard
{
    public required string Code { get; init; }

    public required string Title { get; init; }

    public required string Family { get; init; }

    public string Description { get; init; } = string.Empty;

    public CardType Type { get; init; } = CardType.Instant;

    /// <summary>Owning side for V2 decks. Null means a world card both sides suffer.</summary>
    public string? OwnerSideCode { get; init; }

    /// <summary>V2 cost in political capital.</summary>
    public double PoliticalCost { get; init; }

    /// <summary>V2 cost in billions.</summary>
    public double MoneyCost { get; init; }

    /// <summary>Base probability, unused in V1.0 where the calendar is fixed.</summary>
    public double BaseProbability { get; init; }

    /// <summary>
    /// Code of the card this one answers. A counter played on the same turn as its target
    /// stops the target outright; this is what will create the bluff in V2, when the
    /// attacker has to decide whether to commit against two unknown cards in hand.
    /// </summary>
    public string? CountersCardCode { get; init; }

    public List<CardEffect> Effects { get; init; } = [];
}

/// <summary>An effect waiting for its delay to expire.</summary>
public sealed class PendingEffect
{
    public required CardEffect Effect { get; init; }

    public required string CardTitle { get; init; }

    public required int TurnsRemaining { get; set; }
}
