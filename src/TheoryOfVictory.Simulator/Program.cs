using System.Globalization;
using System.Text;
using TheoryOfVictory.Core;
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
    Console.WriteLine(new string('=', 100));
    Console.WriteLine($"  {game.Title.ToUpperInvariant()} — {game.Subtitle}");
    Console.WriteLine(new string('=', 100));
    Console.WriteLine();
    Console.WriteLine(
        "Tour  Saison        Brent  | RU pouvoir  goulot     ratio | UA pouvoir  goulot     ratio |      km² Réseau UA");
    Console.WriteLine(new string('-', 100));

    foreach (TurnSnapshot turn in game.Turns)
    {
        Console.WriteLine(string.Format(
            CultureInfo.CurrentCulture,
            "T{0,-4} {1,-8} {2,3} {3,5:F0}$ | {4,10:F0} {5,-10} {6,5:F2} | {7,10:F0} {8,-10} {9,5:F2} | {10,8:F0} {11,6:F1} GW",
            turn.Turn,
            turn.Season.ToFrench(),
            turn.Year % 100,
            turn.OilPrice,
            turn.Invader.CombatPower,
            Short(turn.Invader.BottleneckName),
            turn.Invader.ForceGenerationRatio,
            turn.Defender.CombatPower,
            Short(turn.Defender.BottleneckName),
            turn.Defender.ForceGenerationRatio,
            turn.SquareKilometresGained,
            turn.Defender.GridAvailableGw));

        foreach (PlayedCard card in turn.CardsPlayed)
        {
            Console.WriteLine($"        ► {card.Title}");
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

    Console.WriteLine();
    Console.WriteLine("Front final :");
    foreach (FrontSector sector in game.FinalSectors)
    {
        Console.WriteLine($"  {sector.Name,-26} {sector.HexesGained,6:F1} hex   ({sector.KilometresGained,6:F0} km)");
    }
}

static string Short(string? name)
{
    if (string.IsNullOrEmpty(name))
    {
        return "-";
    }

    return name.Length <= 10 ? name : name[..10];
}
