using TheoryOfVictory.Core;

namespace TheoryOfVictory.Engine.Phases;

/// <summary>
/// Phase 10. The force generation ratio is itself a minimum: men replaced over men
/// lost, and materiel delivered over materiel burnt. Collapse is a threshold.
/// </summary>
public sealed class ControlPhase : ITurnPhase
{
    public const double CollapseThreshold = 0.75d;

    public const int TurnsBeforeCollapse = 3;

    private const double RegimeCollapseStress = 58d;

    public string Name
    {
        get { return "Contrôle"; }
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
        double menRatio = Math.Max(replacementRatio, belligerent.Manpower.InfantryCoverage);
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

        // An army whose salaries are unfunded does not regenerate either.
        return Math.Min(floor, belligerent.GetCoverage("payroll") + 0.25d);
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
            belligerent.CollapseReason = "génération de force sous le seuil trois tours de suite";
            context.Say($"RUPTURE — {belligerent.Name} ne régénère plus assez de force : le front cède.");

            context.State.Outcome ??= new GameOutcome
            {
                Code = "military_collapse",
                Title = $"Effondrement militaire — {belligerent.Name}",
                Explanation = "Le ratio de génération de force est resté sous le seuil trois tours consécutifs. "
                    + "Le front n'a pas cédé sous un assaut : il a cédé parce que le flux s'est tari.",
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
            string title = authoritarian
                ? $"Chute du régime — {belligerent.Name}"
                : $"Capitulation négociée — {belligerent.Name}";

            string explanation = authoritarian
                ? "L'appareil s'est fracturé. Ce n'est pas la rue qui a renversé le régime : "
                    + "c'est la guerre qui a cessé de payer ceux qui comptaient."
                : "La volonté de continuer s'est épuisée et le pays a négocié. "
                    + "On perd aussi par l'arrière.";

            belligerent.HasCollapsed = true;
            belligerent.CollapseReason = authoritarian ? "chute du régime" : "épuisement de la volonté";

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
            Title = bothExhausted ? "Épuisement mutuel" : "Front figé",
            Explanation = !bothExhausted
                ? "Les deux camps régénèrent autant qu'ils consomment. Le front tient, personne ne gagne : "
                    + "l'égalité industrielle produit l'enlisement, pas la paix."
                : "Les deux camps sont passés sous leur seuil de régénération. "
                    + "Armistice sur la ligne atteinte, faute de pouvoir continuer.",
            Turn = context.State.Turn,
        };
    }
}
