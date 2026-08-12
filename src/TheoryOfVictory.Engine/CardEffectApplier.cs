using TheoryOfVictory.Core;

namespace TheoryOfVictory.Engine;

/// <summary>
/// Applies the finite effect vocabulary. Adding a card is data; only a genuinely
/// new kind of effect ever touches this file.
/// </summary>
public static class CardEffectApplier
{
    public static void Apply(GameState state, CardEffect effect, List<string> narrative)
    {
        foreach (Side side in Resolve(effect.TargetSideCode))
        {
            Belligerent belligerent = state.Get(side);
            ApplyToSide(state, belligerent, effect, narrative);
        }
    }

    private static IEnumerable<Side> Resolve(string? sideCode)
    {
        if (string.IsNullOrWhiteSpace(sideCode))
        {
            return Side.All;
        }

        return [Side.FromCode(sideCode)];
    }

    private static void ApplyToSide(GameState state, Belligerent belligerent, CardEffect effect, List<string> narrative)
    {
        double value = effect.Value;

        switch (effect.Kind)
        {
            case EffectKind.OilPriceDelta:
                state.OilPrice = Math.Max(15d, state.OilPrice + value);
                break;

            case EffectKind.AidPledgeDelta:
                belligerent.Foreign.PledgedPerTurnBillions = Math.Max(0d, belligerent.Foreign.PledgedPerTurnBillions + value);
                break;

            case EffectKind.AidDisbursementRate:
                belligerent.Foreign.DisbursementRate = Math.Clamp(belligerent.Foreign.DisbursementRate + value, 0d, 1.5d);
                break;

            case EffectKind.ForeignSupplyCeilingDelta:
                belligerent.Foreign.SupplyCeilingBillions = Math.Max(0d, belligerent.Foreign.SupplyCeilingBillions + value);
                break;

            case EffectKind.SanctionsPriceDelta:
                belligerent.Sanctions.Tighten(value, 0d, 0d);
                break;

            case EffectKind.SanctionsFrictionDelta:
                belligerent.Sanctions.Tighten(0d, value, 0d);
                break;

            case EffectKind.SanctionsComponentDelta:
                belligerent.Sanctions.Tighten(0d, 0d, value);
                break;

            case EffectKind.MobilisationWave:
                Mobilise(belligerent, value, narrative);
                break;

            case EffectKind.RecruitmentCostMultiplier:
                belligerent.Manpower.ContractCostPerThousand *= Math.Max(0.1d, value);
                break;

            case EffectKind.MoraleDelta:
                belligerent.Politics.Morale = Math.Clamp(belligerent.Politics.Morale + value, 0d, 100d);
                break;

            case EffectKind.PopularDiscontentDelta:
                belligerent.Politics.PopularDiscontent = Math.Clamp(belligerent.Politics.PopularDiscontent + value, 0d, 100d);
                break;

            case EffectKind.EliteCohesionDelta:
                belligerent.Politics.EliteCohesion = Math.Clamp(belligerent.Politics.EliteCohesion + value, 0d, 100d);
                break;

            case EffectKind.ExternalWillDelta:
                belligerent.Politics.ExternalWill = Math.Clamp(belligerent.Politics.ExternalWill + value, 0d, 100d);
                break;

            case EffectKind.CorruptionDelta:
                belligerent.Politics.Corruption = Math.Clamp(belligerent.Politics.Corruption + value, 0d, 100d);
                break;

            case EffectKind.PoliticalCapitalDelta:
                belligerent.Politics.PoliticalCapital = Math.Max(0d, belligerent.Politics.PoliticalCapital + value);
                break;

            case EffectKind.InnovationTacticalJump:
                belligerent.Innovation.TacticalDroneEdge = Math.Min(
                    belligerent.Innovation.ScaleCeiling,
                    belligerent.Innovation.TacticalDroneEdge + value);
                break;

            case EffectKind.InnovationStrikeJump:
                belligerent.Innovation.StrikeEdge = Math.Min(
                    belligerent.Innovation.ScaleCeiling,
                    belligerent.Innovation.StrikeEdge + value);
                break;

            case EffectKind.InnovationCounterJump:
                belligerent.Innovation.CounterDroneEdge = Math.Min(
                    belligerent.Innovation.ScaleCeiling,
                    belligerent.Innovation.CounterDroneEdge + value);
                break;

            case EffectKind.ProductionCapacityMultiplier:
                foreach (ResourceKind kind in ResourceKind.All)
                {
                    belligerent.Industry.SetCapacityPerTurn(kind, belligerent.Industry.GetCapacityPerTurn(kind) * value);
                }

                break;

            case EffectKind.GridPermanentDamage:
                belligerent.Grid.PermanentDamageGw += Math.Max(0d, value);
                break;

            case EffectKind.RefiningIntegrityDelta:
                belligerent.Economy.RefiningIntegrity = Math.Clamp(belligerent.Economy.RefiningIntegrity + value, 0.05d, 1d);
                break;

            case EffectKind.LogisticsIntegrityDelta:
                belligerent.Politics.LogisticsIntegrity = Math.Clamp(belligerent.Politics.LogisticsIntegrity + value, 0.2d, 1d);
                break;

            case EffectKind.TreasuryDelta:
                belligerent.Economy.TreasuryBillions = Math.Max(0d, belligerent.Economy.TreasuryBillions + value);
                break;

            case EffectKind.StockDelta:
                if (!string.IsNullOrWhiteSpace(effect.ResourceCode))
                {
                    ResourceKind kind = ResourceKind.FromCode(effect.ResourceCode);
                    if (value >= 0d)
                    {
                        belligerent.Stock.Add(kind, value);
                    }
                    else
                    {
                        belligerent.Stock.Destroy(kind, -value);
                    }
                }

                break;

            case EffectKind.ConditionalityDelta:
                belligerent.Foreign.Conditionality = Math.Clamp(belligerent.Foreign.Conditionality + value, 0d, 0.9d);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(effect), effect.Kind, "Unhandled effect kind.");
        }
    }

    private static void Mobilise(Belligerent belligerent, double thousands, List<string> narrative)
    {
        Manpower manpower = belligerent.Manpower;
        double taken = Math.Min(thousands, manpower.MobilisablePool);
        if (taken <= 0d)
        {
            narrative.Add($"{belligerent.Name} : mobilisation décrétée, mais le réservoir est vide.");
            return;
        }

        manpower.MobilisablePool -= taken;
        manpower.TotalMobilisedEver += taken;
        manpower.TrainingPipeline.Enqueue(taken);

        // A mobilisation also raises the force the command intends to sustain.
        manpower.TargetForceSize += taken * 0.6d;

        double gdpHit = manpower.MarginalGdpCost(taken);
        belligerent.Economy.ProductiveCapacityBillions =
            Math.Max(0d, belligerent.Economy.ProductiveCapacityBillions - gdpHit);

        // Forced mobilisation is free in cash and expensive in consent.
        belligerent.Politics.PopularDiscontent = Math.Min(100d, belligerent.Politics.PopularDiscontent + (taken / 12d));
        belligerent.Politics.Morale = Math.Max(0d, belligerent.Politics.Morale - (taken / 45d));

        // Rushing the cycle degrades quality, which raises losses, which forces the next wave.
        manpower.TrainingQuality = Math.Max(0.55d, manpower.TrainingQuality - 0.08d);

        narrative.Add($"{belligerent.Name} : {taken:F0} k hommes mobilisés, {gdpHit:F1} Md de capacité productive perdus.");
    }
}
