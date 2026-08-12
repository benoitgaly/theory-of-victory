using TheoryOfVictory.Core;

namespace TheoryOfVictory.Engine;

/// <summary>What one card was actually worth, measured by replaying the war without it.</summary>
public sealed class CardValue
{
    public required string Code { get; init; }

    public required string Title { get; init; }

    /// <summary>How many times the calendar played it.</summary>
    public required int Plays { get; init; }

    /// <summary>Outcome code of the run that includes the card.</summary>
    public required string BaselineOutcome { get; init; }

    /// <summary>Outcome code of the run that does not.</summary>
    public required string CounterfactualOutcome { get; init; }

    /// <summary>
    /// Turns the card brought the decision forward. Positive means removing it made the
    /// war last longer, so the card was worth that many quarters.
    /// </summary>
    public required double TurnsGained { get; init; }

    /// <summary>True when dropping the card hands the war to the other side entirely.</summary>
    public required bool ChangesTheWinner { get; init; }

    /// <summary>Invader combat power at the end without the card, minus with it.</summary>
    public required double InvaderPowerDelta { get; init; }

    public required double DefenderPowerDelta { get; init; }

    /// <summary>One line a player can read: what not playing this card would have cost.</summary>
    public required string Verdict { get; init; }
}

/// <summary>
/// The engine is deterministic and a whole war costs a few milliseconds, so the cost of
/// inaction does not have to be asserted — it can be measured. Replaying the same war
/// without one card and reading the difference is the honest version of "this was the
/// decisive decision", and it is the V2 arbitration made visible with no interface at all.
///
/// It also doubles as a balance instrument: a card worth zero quarters is a card that
/// does not belong in the deck.
/// </summary>
public static class CardValueAnalyser
{
    /// <summary>
    /// Replays <paramref name="build"/> once per distinct card in its calendar, each time
    /// with that card removed, and ranks them by what their absence costs.
    /// </summary>
    public static List<CardValue> Rank(Func<Scenario> build)
    {
        GameRunner runner = new();

        Scenario reference = build();
        PlayedGame baseline = runner.Run(reference);

        Dictionary<string, int> plays = [];
        foreach (ScheduledCard scheduled in reference.Calendar)
        {
            plays[scheduled.CardCode] = plays.GetValueOrDefault(scheduled.CardCode) + 1;
        }

        List<CardValue> values = [];
        foreach (KeyValuePair<string, int> entry in plays)
        {
            Scenario without = build();
            without.Calendar.RemoveAll(scheduled => scheduled.CardCode == entry.Key);

            PlayedGame counterfactual = runner.Run(without);
            values.Add(Compare(reference, baseline, counterfactual, entry.Key, entry.Value));
        }

        values.Sort((left, right) =>
        {
            int byWinner = right.ChangesTheWinner.CompareTo(left.ChangesTheWinner);
            return byWinner != 0 ? byWinner : right.TurnsGained.CompareTo(left.TurnsGained);
        });

        return values;
    }

    private static CardValue Compare(
        Scenario reference,
        PlayedGame baseline,
        PlayedGame counterfactual,
        string code,
        int plays)
    {
        string title = code;
        foreach (EventCard card in reference.Deck)
        {
            if (string.Equals(card.Code, code, StringComparison.OrdinalIgnoreCase))
            {
                title = card.Title;
                break;
            }
        }

        string baselineOutcome = baseline.Outcome?.Code ?? "unresolved";
        string counterfactualOutcome = counterfactual.Outcome?.Code ?? "unresolved";

        bool changesWinner = baseline.Outcome?.WinnerSideCode != counterfactual.Outcome?.WinnerSideCode;
        double turnsGained = counterfactual.Turns.Count - baseline.Turns.Count;

        double invaderDelta = counterfactual.Turns[^1].Invader.CombatPower - baseline.Turns[^1].Invader.CombatPower;
        double defenderDelta = counterfactual.Turns[^1].Defender.CombatPower - baseline.Turns[^1].Defender.CombatPower;

        return new CardValue
        {
            Code = code,
            Title = title,
            Plays = plays,
            BaselineOutcome = baselineOutcome,
            CounterfactualOutcome = counterfactualOutcome,
            TurnsGained = turnsGained,
            ChangesTheWinner = changesWinner,
            InvaderPowerDelta = invaderDelta,
            DefenderPowerDelta = defenderDelta,
            Verdict = Verdict(reference, baseline, changesWinner, turnsGained, counterfactualOutcome),
        };
    }

    private static string Verdict(
        Scenario reference,
        PlayedGame baseline,
        bool changesWinner,
        double turnsGained,
        string counterfactualOutcome)
    {
        if (changesWinner)
        {
            return $"Décisive — sans elle, la guerre finit en « {Readable(counterfactualOutcome)} ».";
        }

        string winner = Winner(reference, baseline);

        if (turnsGained > 0d)
        {
            return $"Elle fait tomber la décision {turnsGained:F0} trimestre(s) plus tôt, {winner}.";
        }

        if (turnsGained < 0d)
        {
            return $"Elle prolonge la guerre de {-turnsGained:F0} trimestre(s) sans en changer l'issue.";
        }

        return "Sans effet mesurable sur l'issue : elle rend le chemin plus confortable, rien de plus.";
    }

    /// <summary>
    /// Naming who the acceleration serves is what turns a delta into a lesson: a card
    /// that hastens its own player's defeat reads as a warning, not as an achievement.
    /// </summary>
    private static string Winner(Scenario reference, PlayedGame baseline)
    {
        string? code = baseline.Outcome?.WinnerSideCode;
        if (string.IsNullOrEmpty(code))
        {
            return "sans départager les camps";
        }

        Belligerent winner = Side.FromCode(code) == Side.Invader ? reference.Invader : reference.Defender;
        return $"au profit de : {winner.Name}";
    }

    private static string Readable(string outcomeCode)
    {
        return outcomeCode switch
        {
            "frozen_front" => "front figé",
            "mutual_exhaustion" => "épuisement mutuel",
            "military_collapse" => "effondrement militaire",
            "regime_collapse" => "chute du régime",
            "negotiated_capitulation" => "capitulation négociée",
            _ => "partie non conclue",
        };
    }
}
