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
        List<EventCard> turnCards = [];
        foreach (ScheduledCard scheduled in context.Scenario.Calendar)
        {
            if (scheduled.Turn != context.State.Turn)
            {
                continue;
            }

            EventCard? found = FindCard(context.Scenario, scheduled.CardCode);
            if (found is not null)
            {
                turnCards.Add(found);
            }
        }

        // A counter resolves before what it answers, as a counterspell does: the countered
        // card is still played and still seen, it simply does nothing. Seeing a card spent
        // for nothing is the whole pleasure of the mechanic.
        HashSet<string> countered = [];
        foreach (EventCard card in turnCards)
        {
            if (card.Type == CardType.Counter && !string.IsNullOrWhiteSpace(card.CountersCardCode))
            {
                countered.Add(card.CountersCardCode);
            }
        }

        foreach (EventCard card in turnCards)
        {
            PlayedCard printed = CardPrinter.Print(card);
            printed.AffordedInFull = Charge(context, card);
            context.CardsPlayed.Add(printed);

            if (countered.Contains(card.Code))
            {
                printed.Countered = true;
                context.Say($"« {card.Title} » est contrée : la carte est jouée, elle ne produit rien.");
                continue;
            }

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

    /// <summary>
    /// Debits the political cost from whoever owns the card. V1.0 plays its calendar
    /// whatever the balance — the runs have to stay comparable — but the overdraft is
    /// recorded, which is how the V2 currency gets tested before it gates anything.
    /// </summary>
    private static bool Charge(TurnContext context, EventCard card)
    {
        if (card.PoliticalCost <= 0d || string.IsNullOrWhiteSpace(card.OwnerSideCode))
        {
            return true;
        }

        PoliticalState politics = context.State.Get(Side.FromCode(card.OwnerSideCode)).Politics;
        double shortfall = Math.Max(0d, card.PoliticalCost - politics.PoliticalCapital);

        politics.PoliticalCapital = Math.Max(0d, politics.PoliticalCapital - card.PoliticalCost);
        politics.PoliticalCapitalOverdraft += shortfall;

        if (shortfall > 0d)
        {
            context.Say($"« {card.Title} » coûte {card.PoliticalCost:F0} de capital politique — "
                + $"il en manquait {shortfall:F0}. En V2, cette carte reste en main.");
        }

        return shortfall <= 0d;
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
