using TheoryOfVictory.Core;

namespace TheoryOfVictory.Engine;

/// <summary>Turns a card's typed effects into printable rules text.</summary>
public static class CardPrinter
{
    public static PlayedCard Print(EventCard card)
    {
        List<string> rules = [];
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
            TypeLine = $"{TypeName(card.Type)} — {card.Family}",
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

    private static string TypeName(CardType type)
    {
        return type switch
        {
            CardType.Permanent => "Permanent",
            CardType.Instant => "Éphémère",
            CardType.SlowRitual => "Rituel lent",
            CardType.Counter => "Contre-carte",
            _ => "Carte",
        };
    }

    private static string Side(string? code)
    {
        return code switch
        {
            "invader" => "Russie",
            "defender" => "Ukraine",
            _ => "chaque camp",
        };
    }

    private static string Signed(double value, string unit)
    {
        string sign = value >= 0d ? "+" : "−";
        return $"{sign}{Math.Abs(value):0.##} {unit}";
    }

    private static string Describe(CardEffect effect)
    {
        string who = Side(effect.TargetSideCode);
        string delay = effect.DelayTurns > 0 ? $" (dans {effect.DelayTurns} tours)" : string.Empty;

        string body = effect.Kind switch
        {
            EffectKind.OilPriceDelta => $"Prix du baril {Signed(effect.Value, "$")}",
            EffectKind.AidPledgeDelta => $"{who} : aide promise {Signed(effect.Value, "Md par tour")}",
            EffectKind.AidDisbursementRate => $"{who} : versement de l'aide {Signed(effect.Value * 100d, "%")}",
            EffectKind.ForeignSupplyCeilingDelta => $"{who} : plafond d'achat étranger {Signed(effect.Value, "Md")}",
            EffectKind.SanctionsPriceDelta => $"{who} : décote sur le baril {Signed(effect.Value * 100d, "pts")}",
            EffectKind.SanctionsFrictionDelta => $"{who} : friction douanière {Signed(effect.Value * 100d, "pts")}",
            EffectKind.SanctionsComponentDelta => $"{who} : accès aux composants {Signed(-effect.Value * 100d, "pts")}",
            EffectKind.MobilisationWave => $"{who} : mobilise {effect.Value:0} k hommes",
            EffectKind.RecruitmentCostMultiplier => $"{who} : coût de recrutement ×{effect.Value:0.##}",
            EffectKind.MoraleDelta => $"{who} : moral {Signed(effect.Value, string.Empty)}",
            EffectKind.PopularDiscontentDelta => $"{who} : mécontentement {Signed(effect.Value, string.Empty)}",
            EffectKind.EliteCohesionDelta => $"{who} : cohésion des élites {Signed(effect.Value, string.Empty)}",
            EffectKind.ExternalWillDelta => $"{who} : volonté des soutiens {Signed(effect.Value, string.Empty)}",
            EffectKind.CorruptionDelta => $"{who} : corruption {Signed(effect.Value, string.Empty)}",
            EffectKind.PoliticalCapitalDelta => $"{who} : capital politique {Signed(effect.Value, string.Empty)}",
            EffectKind.InnovationTacticalJump => $"{who} : avance drones tactiques {Signed(effect.Value, string.Empty)}",
            EffectKind.InnovationStrikeJump => $"{who} : avance frappe profonde {Signed(effect.Value, string.Empty)}",
            EffectKind.InnovationCounterJump => $"{who} : avance contre-drone {Signed(effect.Value, string.Empty)}",
            EffectKind.ProductionCapacityMultiplier => $"{who} : capacité industrielle ×{effect.Value:0.##}",
            EffectKind.GridPermanentDamage => $"{who} : {effect.Value:0.#} GW détruits définitivement",
            EffectKind.RefiningIntegrityDelta => $"{who} : raffinage {Signed(effect.Value * 100d, "%")}",
            EffectKind.LogisticsIntegrityDelta => $"{who} : logistique {Signed(effect.Value * 100d, "%")}",
            EffectKind.TreasuryDelta => $"{who} : trésorerie {Signed(effect.Value, "Md")}",
            EffectKind.StockDelta => $"{who} : stock {Signed(effect.Value, StockName(effect.ResourceCode))}",
            EffectKind.ConditionalityDelta => $"{who} : conditionnalité de l'aide {Signed(effect.Value * 100d, "pts")}",
            _ => effect.Kind.ToString(),
        };

        return body + delay;
    }

    private static string StockName(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return "unités";
        }

        return ResourceKind.FromCode(code).DisplayName.ToLowerInvariant();
    }
}
