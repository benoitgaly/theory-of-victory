using TheoryOfVictory.Core;

namespace TheoryOfVictory.Engine.Phases;

/// <summary>
/// Phase 6. V1.0 has no dice: the calendar is written in advance, which is exactly
/// what makes the two runs comparable.
/// </summary>
public sealed class EventPhase : ITurnPhase
{
    public string Name
    {
        get { return "Événements"; }
    }

    public void Execute(TurnContext context)
    {
        ResolvePending(context);
        PlayScheduledCards(context);
    }

    private void ResolvePending(TurnContext context)
    {
        List<PendingEffect> due = [];
        foreach (PendingEffect pending in context.State.PendingEffects)
        {
            pending.TurnsRemaining--;
            if (pending.TurnsRemaining <= 0)
            {
                due.Add(pending);
            }
        }

        foreach (PendingEffect pending in due)
        {
            context.State.PendingEffects.Remove(pending);
            CardEffectApplier.Apply(context.State, pending.Effect, context.Narrative);
            context.Say($"Effet différé : {pending.CardTitle}.");
        }
    }

    private void PlayScheduledCards(TurnContext context)
    {
        foreach (ScheduledCard scheduled in context.Scenario.Calendar)
        {
            if (scheduled.Turn != context.State.Turn)
            {
                continue;
            }

            EventCard? card = FindCard(context.Scenario, scheduled.CardCode);
            if (card is null)
            {
                continue;
            }

            context.CardsPlayed.Add(CardPrinter.Print(card));

            foreach (CardEffect effect in card.Effects)
            {
                if (effect.DelayTurns > 0)
                {
                    context.State.PendingEffects.Add(new PendingEffect
                    {
                        Effect = effect,
                        CardTitle = card.Title,
                        TurnsRemaining = effect.DelayTurns,
                    });
                    continue;
                }

                CardEffectApplier.Apply(context.State, effect, context.Narrative);
            }
        }
    }

    private EventCard? FindCard(Scenario scenario, string code)
    {
        foreach (EventCard card in scenario.Deck)
        {
            if (string.Equals(card.Code, code, StringComparison.OrdinalIgnoreCase))
            {
                return card;
            }
        }

        return null;
    }
}
