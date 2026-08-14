using TheoryOfVictory.Core;
using TheoryOfVictory.Core.Localization;

namespace TheoryOfVictory.Engine;

/// <summary>
/// Turns a card's typed effects into printable rules text.
///
/// Le cartouche ne contient plus de phrase : il contient des FAITS — un code d'effet, la cible
/// et la valeur — et la phrase se compose à la lecture. Une ligne de règles est de la langue,
/// et un deck rejoué dans une autre langue doit dire la même chose sans que rien du moteur ne
/// change.
/// </summary>
public static class CardPrinter
{
    public static PlayedCard Print(EventCard card)
    {
        List<LocalizedText> rules = [];
        List<string> affected = [];
        foreach (CardEffect effect in card.Effects)
        {
            rules.Add(Describe(effect));

            // A null target means the whole world — the oil price, and both sides with it.
            foreach (string side in effect.TargetSideCode is null
                         ? new[] { Core.Side.Invader.Code, Core.Side.Defender.Code }
                         : [effect.TargetSideCode])
            {
                if (!affected.Contains(side))
                {
                    affected.Add(side);
                }
            }
        }

        return new PlayedCard
        {
            Code = card.Code,
            Title = card.Title,
            Family = card.Family,
            TypeLine = LocalizedText.Of(TextCodes.Card.TypeLine, TypeName(card.Type), FamilyName(card.Family)),
            Description = card.Description,
            OwnerSideCode = card.OwnerSideCode,
            PoliticalCost = card.PoliticalCost,
            MoneyCost = card.MoneyCost,
            RulesText = rules,
            AffectedSideCodes = affected,
            CountersCardCode = card.CountersCardCode,
            Art = card.Family,
        };
    }

    private static LocalizedText TypeName(CardType type)
    {
        return LocalizedText.Of(type switch
        {
            CardType.Permanent => TextCodes.Card.Permanent,
            CardType.Instant => TextCodes.Card.Instant,
            CardType.SlowRitual => TextCodes.Card.SlowRitual,
            CardType.Counter => TextCodes.Card.Counter,
            _ => TextCodes.Card.Plain,
        });
    }

    /// <summary>
    /// La famille est une valeur de DONNÉE — cards.fr.json l'écrit, et la page s'en sert pour
    /// choisir le ciel et la teinte de la carte. Elle ne se traduit donc qu'à l'écriture, par
    /// une branche par valeur : passer la variable à Loc rendrait la clé illisible dans le code
    /// et invisible pour l'inventaire. Une famille inconnue s'imprime telle quelle.
    /// </summary>
    private static LocalizedText FamilyName(string family)
    {
        return family switch
        {
            "Économique" => LocalizedText.Of(TextCodes.Card.FamilyEconomic),
            "Politique occidentale" => LocalizedText.Of(TextCodes.Card.FamilyWesternPolitics),
            "Politique interne" => LocalizedText.Of(TextCodes.Card.FamilyDomesticPolitics),
            "Énergie" => LocalizedText.Of(TextCodes.Card.FamilyEnergy),
            "Militaire et technologique" => LocalizedText.Of(TextCodes.Card.FamilyMilitary),
            "Externe" => LocalizedText.Of(TextCodes.Card.FamilyExternal),
            _ => LocalizedText.Of(TextCodes.Verbatim, family),
        };
    }

    private static LocalizedText Side(string? code)
    {
        return LocalizedText.Of(code switch
        {
            "invader" => TextCodes.Side.Invader,
            "defender" => TextCodes.Side.Defender,
            _ => TextCodes.Side.EitherSide,
        });
    }

    /// <summary>
    /// Le signe voyage avec le nombre, dans son format : « +3 », « −0,45 ». Les deux sections du
    /// format s'en chargent, ce qui laisse la valeur brute jusqu'à la lecture — et le séparateur
    /// décimal suit alors la langue au lieu d'être figé à l'impression de la carte.
    /// </summary>
    private static IFormattable Signed(double value)
    {
        return LocalizedText.Number(value, "+0.##;−0.##");
    }

    private static LocalizedText Describe(CardEffect effect)
    {
        LocalizedText who = Side(effect.TargetSideCode);

        LocalizedText body = effect.Kind switch
        {
            EffectKind.OilPriceDelta => LocalizedText.Of(TextCodes.Card.OilPrice, Signed(effect.Value)),
            EffectKind.AidPledgeDelta => LocalizedText.Of(TextCodes.Card.AidPledge, who, Signed(effect.Value)),
            EffectKind.AidDisbursementRate => LocalizedText.Of(TextCodes.Card.AidDisbursement, who, Signed(effect.Value * 100d)),
            EffectKind.ForeignSupplyCeilingDelta => LocalizedText.Of(TextCodes.Card.ForeignCeiling, who, Signed(effect.Value)),
            EffectKind.SanctionsPriceDelta => LocalizedText.Of(TextCodes.Card.BarrelDiscount, who, Signed(effect.Value * 100d)),
            EffectKind.SanctionsFrictionDelta => LocalizedText.Of(TextCodes.Card.CustomsFriction, who, Signed(effect.Value * 100d)),
            EffectKind.SanctionsComponentDelta => LocalizedText.Of(TextCodes.Card.ComponentAccess, who, Signed(-effect.Value * 100d)),
            EffectKind.MobilisationWave => LocalizedText.Of(TextCodes.Card.Mobilisation, who, LocalizedText.Number(effect.Value, "0")),
            EffectKind.RecruitmentCostMultiplier => LocalizedText.Of(TextCodes.Card.RecruitmentCost, who, LocalizedText.Number(effect.Value, "0.##")),
            EffectKind.MoraleDelta => LocalizedText.Of(TextCodes.Card.Morale, who, Signed(effect.Value)),
            EffectKind.PopularDiscontentDelta => LocalizedText.Of(TextCodes.Card.Discontent, who, Signed(effect.Value)),
            EffectKind.EliteCohesionDelta => LocalizedText.Of(TextCodes.Card.EliteCohesion, who, Signed(effect.Value)),
            EffectKind.ExternalWillDelta => LocalizedText.Of(TextCodes.Card.BackersWill, who, Signed(effect.Value)),
            EffectKind.CorruptionDelta => LocalizedText.Of(TextCodes.Card.Corruption, who, Signed(effect.Value)),
            EffectKind.PoliticalCapitalDelta => LocalizedText.Of(TextCodes.Card.PoliticalCapital, who, Signed(effect.Value)),
            EffectKind.InnovationTacticalJump => LocalizedText.Of(TextCodes.Card.TacticalDroneEdge, who, Signed(effect.Value)),
            EffectKind.InnovationStrikeJump => LocalizedText.Of(TextCodes.Card.DeepStrikeEdge, who, Signed(effect.Value)),
            EffectKind.InnovationCounterJump => LocalizedText.Of(TextCodes.Card.CounterDroneEdge, who, Signed(effect.Value)),
            EffectKind.ProductionCapacityMultiplier => LocalizedText.Of(TextCodes.Card.IndustrialCapacity, who, LocalizedText.Number(effect.Value, "0.##")),
            EffectKind.GridPermanentDamage => LocalizedText.Of(TextCodes.Card.GridDestroyed, who, LocalizedText.Number(effect.Value, "0.#")),
            EffectKind.CivilianIndustryDamage => LocalizedText.Of(TextCodes.Card.CivilianDestroyed, who, LocalizedText.Number(effect.Value, "0.#")),
            EffectKind.RefiningIntegrityDelta => LocalizedText.Of(TextCodes.Card.Refining, who, Signed(effect.Value * 100d)),
            EffectKind.LogisticsIntegrityDelta => LocalizedText.Of(TextCodes.Card.Logistics, who, Signed(effect.Value * 100d)),
            EffectKind.TreasuryDelta => LocalizedText.Of(TextCodes.Card.Treasury, who, Signed(effect.Value)),
            EffectKind.StockDelta => LocalizedText.Of(TextCodes.Card.StockDelta, who, Signed(effect.Value), StockName(effect.ResourceCode)),
            EffectKind.ConditionalityDelta => LocalizedText.Of(TextCodes.Card.AidConditionality, who, Signed(effect.Value * 100d)),
            _ => LocalizedText.Of(TextCodes.Card.Unnamed, effect.Kind.ToString()),
        };

        return effect.DelayTurns > 0
            ? LocalizedText.Of(TextCodes.Card.Delayed, body, effect.DelayTurns)
            : body;
    }

    /// <summary>
    /// Le nom de la ressource au milieu d'une phrase, où le français le veut en minuscule. Une
    /// mise en minuscule automatique ferait le travail en français et le referait mal ailleurs :
    /// l'anglais garde ses majuscules à certains noms, l'allemand à tous les siens.
    /// </summary>
    private static LocalizedText StockName(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return LocalizedText.Of(TextCodes.Resource.UnitsInline);
        }

        return LocalizedText.Of(code switch
        {
            "weapons" => TextCodes.Resource.WeaponsInline,
            "fuel" => TextCodes.Resource.FuelInline,
            "food" => TextCodes.Resource.FoodInline,
            "strike_drones" => TextCodes.Resource.StrikeDronesInline,
            "missiles" => TextCodes.Resource.MissilesInline,
            "cheap_interceptors" => TextCodes.Resource.CheapInterceptorsInline,
            "heavy_interceptors" => TextCodes.Resource.HeavyInterceptorsInline,
            _ => TextCodes.Resource.UnitsInline,
        });
    }
}
