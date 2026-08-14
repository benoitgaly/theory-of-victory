using System.Globalization;
using System.Text;
using TheoryOfVictory.Core;
using TheoryOfVictory.Core.Localization;
using TheoryOfVictory.Engine;
using TheoryOfVictory.Engine.Scenarios;

Console.OutputEncoding = Encoding.UTF8;
CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

GameRunner runner = new();

foreach (SupportVariant variant in Enum.GetValues<SupportVariant>())
{
    Scenario scenario = UkraineScenario.Build(variant);
    PlayedGame game = runner.Run(scenario);

    Console.WriteLine();
    Console.WriteLine(new string('=', 118));
    Console.WriteLine($"  {game.Title.ToString().ToUpperInvariant()} — {game.Subtitle}");
    Console.WriteLine(new string('=', 118));
    Console.WriteLine();
    Console.WriteLine(
        "Tour  Saison        Brent  |     RU pouvoir goulot     ratio menace |     UA pouvoir goulot     ratio menace |      km²");
    Console.WriteLine(new string('-', 118));

    foreach (TurnSnapshot turn in game.Turns)
    {
        Console.WriteLine(string.Format(
            CultureInfo.CurrentCulture,
            "T{0,-4} {1,-8} {2,3} {3,5:F0}$ | {4,14:F0} {5,-10} {6,5:F2} {7,5:F0}  | {8,14:F0} {9,-10} {10,5:F2} {11,5:F0}  | {12,8:F0}",
            turn.Turn,
            turn.Season.Label(),
            turn.Year % 100,
            turn.OilPrice,
            turn.Invader.CombatPower,
            Short(turn.Invader.BottleneckName),
            turn.Invader.ForceGenerationRatio,
            turn.Invader.Pressure?.ThreatIndex ?? 0d,
            turn.Defender.CombatPower,
            Short(turn.Defender.BottleneckName),
            turn.Defender.ForceGenerationRatio,
            turn.Defender.Pressure?.ThreatIndex ?? 0d,
            turn.SquareKilometresGained));

        foreach (PlayedCard card in turn.CardsPlayed)
        {
            string mark = card.Countered ? "✗" : "►";
            string note = card.Countered ? "  (contrée — sans effet)" : string.Empty;
            Console.WriteLine($"        {mark} {card.Title}{note}");
        }

        foreach (PressureAlert alert in turn.Alerts)
        {
            if (alert.Level == AlertLevel.Watch)
            {
                continue;
            }

            string mark = alert.Level == AlertLevel.Critical ? "!!" : " !";
            Console.WriteLine($"        {mark} [{Flag(alert.SideCode)}] {alert.Title}");
        }
    }

    Console.WriteLine();
    if (game.Outcome is not null)
    {
        Console.WriteLine($"ISSUE : {game.Outcome.Title}");
        Console.WriteLine($"        {game.Outcome.Explanation}");
    }
    else
    {
        Console.WriteLine("ISSUE : partie non conclue.");
    }

    // What this run would have cost in V2, where the cards have to be paid for.
    SideSnapshot lastInvader = game.Turns[^1].Invader;
    SideSnapshot lastDefender = game.Turns[^1].Defender;
    Console.WriteLine();
    Console.WriteLine(string.Format(
        CultureInfo.CurrentCulture,
        "Capital politique — RU : {0:F0} en caisse, {1:F0} à découvert | UA : {2:F0} en caisse, {3:F0} à découvert",
        lastInvader.PoliticalCapital,
        lastInvader.PoliticalCapitalOverdraft,
        lastDefender.PoliticalCapital,
        lastDefender.PoliticalCapitalOverdraft));

    Console.WriteLine();
    Console.WriteLine("Économie de guerre russe — pourquoi le baril décide :");
    Console.WriteLine("Tour  Brent  Raffinage  Recettes pétrole  Recettes ordinaires  Ponction réserves  Réserves  Manque");
    foreach (TurnSnapshot turn in game.Turns)
    {
        SideSnapshot side = turn.Invader;
        Console.WriteLine(string.Format(
            CultureInfo.CurrentCulture,
            "T{0,-4} {1,4:F0}$ {2,9:P0} {3,16:F1} {4,20:F1} {5,18:F1} {6,9:F0} {7,7:P0}",
            turn.Turn,
            turn.OilPrice,
            side.RefiningIntegrity,
            side.OilRevenue,
            side.OrdinaryWarFunding,
            side.ReserveDraw,
            side.Reserves,
            side.FundingGap));
    }

    Console.WriteLine();
    Console.WriteLine("Chaîne des armes — où le goulot se place vraiment :");
    Console.WriteLine("Tour |  RU besoin  produit  dépôt couv. |  UA besoin  produit  dépôt couv. | RU budget  plafond");
    foreach (TurnSnapshot turn in game.Turns)
    {
        Console.WriteLine(string.Format(
            CultureInfo.CurrentCulture,
            "T{0,-3} | {1,9:F0} {2,8:F0} {3,6:F0} {4,5:F2} | {5,9:F0} {6,8:F0} {7,6:F0} {8,5:F2} | {9,9:F1} {10,8:F1}",
            turn.Turn,
            Look(turn.Invader.Need),
            Look(turn.Invader.Produced),
            Look(turn.Invader.Stocks),
            Look(turn.Invader.Coverage),
            Look(turn.Defender.Need),
            Look(turn.Defender.Produced),
            Look(turn.Defender.Stocks),
            Look(turn.Defender.Coverage),
            turn.Invader.WarFundable,
            turn.Invader.WarBudgetCeiling));
    }

    Console.WriteLine();
    Console.WriteLine("Effectifs, en hommes — trois grandeurs qu'on confond en permanence :");
    Console.WriteLine("  sous les drapeaux (ce que les dirigeants annoncent) · au théâtre (ce qui consomme) "
        + "· en contact (ce qui tient le terrain)");
    Console.WriteLine("Tour |    RU drapeaux   théâtre   contact  pertes cum. "
        + "|    UA drapeaux   théâtre   contact  pertes cum.");
    foreach (TurnSnapshot turn in game.Turns)
    {
        Console.WriteLine(string.Format(
            CultureInfo.CurrentCulture,
            "T{0,-3} | {1,12:N0} {2,9:N0} {3,9:N0} {4,12:N0} | {5,12:N0} {6,9:N0} {7,9:N0} {8,12:N0}",
            turn.Turn,
            turn.Invader.MenUnderArms,
            turn.Invader.MenInTheatre,
            turn.Invader.MenInContact,
            turn.Invader.MenLost,
            turn.Defender.MenUnderArms,
            turn.Defender.MenInTheatre,
            turn.Defender.MenInContact,
            turn.Defender.MenLost));
    }

    Console.WriteLine();
    Console.WriteLine("Front final :");
    foreach (FrontSector sector in game.FinalSectors)
    {
        Console.WriteLine($"  {sector.Name,-26} {sector.HexesGained,6:F1} hex   ({sector.KilometresGained,6:F0} km)");
    }

    Mechanisms(game);
}

// A mechanism that never fires demonstrates nothing. This block answers the only question
// that matters about the three central rules: did they ever actually happen in this run?
static void Mechanisms(PlayedGame game)
{
    int pinched = 0;
    int shed = 0;
    int saturated = 0;
    double worstInterception = 1d;
    double worstCoverage = 1d;

    foreach (TurnSnapshot turn in game.Turns)
    {
        foreach (SideSnapshot side in new[] { turn.Invader, turn.Defender })
        {
            if (side.MaterialCoverage < 0.97d)
            {
                pinched++;
            }

            worstCoverage = Math.Min(worstCoverage, side.MaterialCoverage);

            if (side.GridShortfall > 0d)
            {
                shed++;
            }
        }

        foreach (StrikeResolution? strike in new[] { turn.InvaderStrike, turn.DefenderStrike })
        {
            if (strike is null)
            {
                continue;
            }

            if (strike.Saturated)
            {
                saturated++;
            }

            worstInterception = Math.Min(worstInterception, strike.InterceptionRate);
        }
    }

    Console.WriteLine();
    Console.WriteLine("Mécanismes — un mécanisme qui ne se déclenche jamais ne démontre rien :");
    Console.WriteLine($"  Règle du minimum   {pinched,3} lectures camp-tour sous 0,97   (plus basse : {worstCoverage:P0})");
    Console.WriteLine($"  Délestage          {shed,3} lectures camp-tour en coupure");
    Console.WriteLine($"  Saturation         {saturated,3} vagues saturées                (interception la plus basse : {worstInterception:P0})");

    // The magazines the event cards draw from: a card that always empties one entirely has
    // stopped being a dose and become a switch.
    double heavyLow = double.MaxValue;
    double heavyHigh = 0d;
    foreach (TurnSnapshot turn in game.Turns)
    {
        double held = turn.Defender.Stocks[ResourceKind.HeavyInterceptors.Code];
        heavyLow = Math.Min(heavyLow, held);
        heavyHigh = Math.Max(heavyHigh, held);
    }

    Console.WriteLine($"  Magasin lourd UA   {heavyLow,3:F0} au plus bas, {heavyHigh:F0} au plus haut");
}

static string Short(LocalizedText? name)
{
    string written = name?.ToString() ?? string.Empty;
    if (written.Length == 0)
    {
        return "-";
    }

    return written.Length <= 10 ? written : written[..10];
}

// What each card was worth, measured by replaying the same war without it. This is the
// V2 arbitration made visible with no interface: the cost of not playing.
Console.WriteLine();
Console.WriteLine(new string('=', 118));
Console.WriteLine("  CE QUE CHAQUE CARTE VALAIT — même guerre rejouée sans elle");
Console.WriteLine(new string('=', 118));
Console.WriteLine();

foreach (SupportVariant variant in new[] { SupportVariant.Resolve, SupportVariant.Collapses })
{
    Scenario reference = UkraineScenario.Build(variant);
    Console.WriteLine($"— {reference.Title} —");

    List<CardValue> values = CardValueAnalyser.Rank(() => UkraineScenario.Build(variant));
    foreach (CardValue value in values)
    {
        if (value.TurnsGained == 0d && !value.ChangesTheWinner)
        {
            continue;
        }

        Console.WriteLine(string.Format(
            CultureInfo.CurrentCulture,
            "  {0,-42} ×{1}  {2}",
            value.Title,
            value.Plays,
            value.Verdict));
    }

    int inert = values.Count(value => value.TurnsGained == 0d && !value.ChangesTheWinner);
    Console.WriteLine($"  ({inert} cartes sans effet mesurable sur l'issue.)");
    Console.WriteLine();
}

// The balance criterion the design document names: on the same war and the same political
// capital budget, the deep strike deck has to beat the frontal attrition deck.
Console.WriteLine();
Console.WriteLine(new string('=', 118));
Console.WriteLine("  LE DECK EST LA THÉORIE DE LA VICTOIRE — même guerre, même budget politique");
Console.WriteLine(new string('=', 118));
Console.WriteLine();
Console.WriteLine("Deck                   Coût  Cartes  Décidé au  Issue                              RU pouvoir       km²");
Console.WriteLine(new string('-', 118));

foreach (DuelResult duel in DeckDuel.Compare())
{
    Console.WriteLine(string.Format(
        CultureInfo.CurrentCulture,
        "{0,-20} {1,5:F0} {2,7} {3,10} {4,-34} {5,10:F0} {6,9:F0}",
        duel.Name,
        duel.PoliticalCost,
        duel.Plays,
        $"T{duel.DecidedOnTurn}",
        duel.OutcomeTitle,
        duel.InvaderPowerAtEnd,
        duel.GroundTaken));
}

static double Look(Dictionary<string, double> map)
{
    return map.GetValueOrDefault(ResourceKind.Weapons.Code);
}

static string Flag(string sideCode)
{
    return sideCode == Side.Invader.Code ? "RU" : "UA";
}
