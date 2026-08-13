using System.Text.Json;
using System.Text.Json.Serialization;
using TheoryOfVictory.Core;

namespace TheoryOfVictory.Engine.Scenarios;

/// <summary>
/// Loads the chronicle of the real front from data, the way <see cref="CardLibrary"/> loads the
/// deck. Nothing here feeds the engine: the file is read so the BOARD can draw the documented
/// quarters, and the model never consults it. Keeping the two apart is the whole point — a
/// scenario dressed up as an output of the model would discredit everything the model does compute.
/// </summary>
public static class FrontHistoryLibrary
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static FrontHistory Load(string? dataDirectory = null)
    {
        string path = ResolvePath(dataDirectory);
        FrontHistory? history = JsonSerializer.Deserialize<FrontHistory>(File.ReadAllText(path), Options);

        if (history is null || history.Quarters.Count == 0)
        {
            throw new InvalidOperationException($"No documented quarter found in {path}.");
        }

        Validate(history, path);
        return history;
    }

    /// <summary>
    /// A zone the map does not know how to draw would silently vanish from the picture, and the
    /// picture would still look plausible. It is caught here instead, at load time.
    /// </summary>
    private static void Validate(FrontHistory history, string path)
    {
        HashSet<string> vocabulary = [.. history.Vocabulary];

        foreach (FrontQuarter quarter in history.Quarters)
        {
            foreach (string zone in quarter.HeldByInvader.Concat(quarter.Contested).Concat(quarter.HeldByDefender))
            {
                if (!vocabulary.Contains(zone))
                {
                    throw new InvalidOperationException(
                        $"Zone '{zone}' is used in {quarter.Season} {quarter.Year} but is not in the vocabulary of {path}.");
                }
            }

            // A zone cannot be held and disputed at the same time: the three lists partition control.
            IEnumerable<string> twice = quarter.HeldByInvader
                .Concat(quarter.Contested)
                .Concat(quarter.HeldByDefender)
                .GroupBy(zone => zone)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);

            foreach (string zone in twice)
            {
                throw new InvalidOperationException(
                    $"Zone '{zone}' is listed twice in {quarter.Season} {quarter.Year} of {path}.");
            }
        }
    }

    private static string ResolvePath(string? dataDirectory)
    {
        if (!string.IsNullOrWhiteSpace(dataDirectory))
        {
            return Path.Combine(dataDirectory, "front-history.json");
        }

        string local = Path.Combine(AppContext.BaseDirectory, "data", "front-history.json");
        if (File.Exists(local))
        {
            return local;
        }

        throw new FileNotFoundException("front-history.json not found next to the executable.", local);
    }
}
