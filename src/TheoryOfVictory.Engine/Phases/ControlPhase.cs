using TheoryOfVictory.Core;
using TheoryOfVictory.Core.Localization;

namespace TheoryOfVictory.Engine.Phases;

/// <summary>
/// Phase 10. The force generation ratio is itself a minimum: men replaced over men
/// lost, and materiel delivered over materiel burnt. Collapse is a threshold.
/// </summary>
public sealed class ControlPhase : ITurnPhase
{
    public const double CollapseThreshold = 0.75d;

    public const int TurnsBeforeCollapse = 3;

    /// <summary>
    /// Where the apparatus fractures. Public because the capital band draws the margin left
    /// before it: a threshold nobody can see is a threshold nobody plays against.
    /// </summary>
    public const double RegimeCollapseStress = 58d;

    public string Name
    {
        get { return "Control"; }
    }

    public void Execute(TurnContext context)
    {
        foreach (Side side in Side.All)
        {
            UpdateGenerationRatio(context, context.State.Get(side));
        }

        CheckMilitaryCollapse(context);
        CheckPoliticalCollapse(context);
        CheckStalemate(context);
    }

    private void UpdateGenerationRatio(TurnContext context, Belligerent belligerent)
    {
        double replacements = TurnContext.Read(context.ReplacementsArrived, belligerent.Side);
        double losses = TurnContext.Read(context.LossesThisTurn, belligerent.Side);
        double delivered = TurnContext.Read(context.WeaponsDelivered, belligerent.Side);
        double consumed = TurnContext.Read(context.WeaponsConsumed, belligerent.Side);

        // A full army recruits nobody, which does not mean it stopped regenerating:
        // an inactive constraint must never bind.
        double replacementRatio = losses <= 0.01d ? 1.5d : replacements / losses;

        // Men replaced over men lost, floored by how filled the order of battle already is:
        // an army at establishment is regenerating even on a quiet quarter, an army bled down
        // to two thirds of it is not, however few it lost this turn.
        //
        // That floor is a courtesy extended to an army that still exists. Once a side has broken,
        // the courtesy inverts: its quarters are quiet because there is nobody left to fight, not
        // because nothing is wrong. Taking the maximum there would show a dissolving army
        // regenerating perfectly, which is the exact opposite of what is happening to it.
        double menRatio = belligerent.HasCollapsed
            ? belligerent.Manpower.ManningRatio
            : Math.Max(replacementRatio, belligerent.Manpower.ManningRatio);
        double materielRatio = consumed <= 0.01d ? 1.5d : delivered / consumed;

        // Regeneration obeys the same law as combat power: the scarcest side of it governs.
        double ratio = Math.Min(menRatio, Math.Min(materielRatio, CoverageFloor(belligerent)));
        belligerent.ForceGenerationRatio = Math.Clamp(ratio, 0d, 3d);

        if (belligerent.ForceGenerationRatio < CollapseThreshold)
        {
            belligerent.Politics.TurnsBelowCollapseThreshold++;
        }
        else
        {
            belligerent.Politics.TurnsBelowCollapseThreshold = 0;
        }
    }

    /// <summary>A side cannot regenerate faster than its scarcest flow lets it.</summary>
    private static double CoverageFloor(Belligerent belligerent)
    {
        double floor = 1.5d;
        foreach (ResourceKind kind in ResourceKind.FrontFlows)
        {
            floor = Math.Min(floor, belligerent.GetCoverage(kind.Code) + 0.25d);
        }

        // An army whose salaries are unfunded does not regenerate either. Unpaid men are not
        // a missing coverage, they are men the state can no longer hold in the line.
        return Math.Min(floor, belligerent.Manpower.PayRatio + 0.25d);
    }

    private void CheckMilitaryCollapse(TurnContext context)
    {
        foreach (Side side in Side.All)
        {
            Belligerent belligerent = context.State.Get(side);
            if (belligerent.HasCollapsed || belligerent.Politics.TurnsBelowCollapseThreshold < TurnsBeforeCollapse)
            {
                continue;
            }

            belligerent.HasCollapsed = true;
            belligerent.CollapseReason = LocalizedText.Of(TextCodes.Outcome.ReasonGeneration);
            context.Say(LocalizedText.Of(TextCodes.Narrative.Rupture, belligerent.Name));

            context.State.Outcome ??= new GameOutcome
            {
                Code = "military_collapse",
                Title = LocalizedText.Of(TextCodes.Outcome.MilitaryCollapseTitle, belligerent.Name),
                Explanation = LocalizedText.Of(TextCodes.Outcome.MilitaryCollapseExplanation),
                WinnerSideCode = side.Opponent.Code,
                Turn = context.State.Turn,
            };
        }
    }

    private void CheckPoliticalCollapse(TurnContext context)
    {
        foreach (Side side in Side.All)
        {
            Belligerent belligerent = context.State.Get(side);
            if (context.State.Outcome is not null)
            {
                return;
            }

            bool regimeFalls = belligerent.Politics.RegimeStress >= RegimeCollapseStress;
            bool willGone = belligerent.Politics.Morale <= 5d;

            if (!regimeFalls && !willGone)
            {
                continue;
            }

            bool authoritarian = belligerent.Politics.Regime == RegimeType.Authoritarian;
            LocalizedText title = LocalizedText.Of(
                authoritarian ? TextCodes.Outcome.RegimeCollapseTitle : TextCodes.Outcome.NegotiatedTitle,
                belligerent.Name);

            LocalizedText explanation = LocalizedText.Of(authoritarian
                ? TextCodes.Outcome.RegimeCollapseExplanation
                : TextCodes.Outcome.NegotiatedExplanation);

            belligerent.HasCollapsed = true;
            belligerent.CollapseReason = LocalizedText.Of(
                authoritarian ? TextCodes.Outcome.ReasonRegime : TextCodes.Outcome.ReasonWill);

            context.State.Outcome = new GameOutcome
            {
                Code = authoritarian ? "regime_collapse" : "negotiated_capitulation",
                Title = title,
                Explanation = explanation,
                WinnerSideCode = side.Opponent.Code,
                Turn = context.State.Turn,
            };
        }
    }

    private void CheckStalemate(TurnContext context)
    {
        if (context.State.Outcome is not null || context.State.Turn < context.Scenario.TurnCount)
        {
            return;
        }

        Belligerent invader = context.State.Invader;
        Belligerent defender = context.State.DefenderSide;

        // Mutual exhaustion means both sides under the threshold, as the model states it.
        // Testing instead that both are comfortable turned every slow bleed into an
        // armistice, and called a side regenerating at 1.00 exhausted.
        bool bothExhausted = invader.ForceGenerationRatio < CollapseThreshold
            && defender.ForceGenerationRatio < CollapseThreshold;

        context.State.Outcome = new GameOutcome
        {
            Code = bothExhausted ? "mutual_exhaustion" : "frozen_front",
            Title = LocalizedText.Of(bothExhausted
                ? TextCodes.Outcome.MutualExhaustionTitle
                : TextCodes.Outcome.FrozenFrontTitle),
            Explanation = LocalizedText.Of(bothExhausted
                ? TextCodes.Outcome.MutualExhaustionExplanation
                : TextCodes.Outcome.FrozenFrontExplanation),
            Turn = context.State.Turn,
        };
    }
}
