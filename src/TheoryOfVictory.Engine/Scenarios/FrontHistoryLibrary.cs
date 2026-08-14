using System.Text.Json;
using System.Text.Json.Serialization;
using TheoryOfVictory.Core;
using TheoryOfVictory.Core.Localization;

namespace TheoryOfVictory.Engine.Scenarios;

/// <summary>
/// Loads the chronicle of the real front from data, the way <see cref="CardLibrary"/> loads the
/// deck. Nothing here feeds the engine: the file is read so the BOARD can draw the documented
/// quarters, and the model never consults it. Keeping the two apart is the whole point — a
/// scenario dressed up as an output of the model would discredit everything the model does compute.
///
/// The position of the front exists once, in front-history.json. The sentence that sums up a
/// quarter is prose and lives in front-history.&lt;lang&gt;.json, keyed by year and season.
/// </summary>
public static class FrontHistoryLibrary
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static FrontHistory Load(Language language = Language.French, string? dataDirectory = null)
    {
        string path = ResolvePath(dataDirectory, "front-history.json");
        FrontFile? file = JsonSerializer.Deserialize<FrontFile>(File.ReadAllText(path), Options);

        if (file is null || file.Quarters.Count == 0)
        {
            throw new InvalidOperationException($"No documented quarter found in {path}.");
        }

        Dictionary<string, string> headlines = LoadHeadlines(dataDirectory, Language.French);
        if (language != Language.French)
        {
            foreach (KeyValuePair<string, string> entry in LoadHeadlines(dataDirectory, language))
            {
                if (!string.IsNullOrWhiteSpace(entry.Value))
                {
                    headlines[entry.Key] = entry.Value;
                }
            }
        }

        List<FrontQuarter> quarters = [];
        foreach (QuarterDto dto in file.Quarters)
        {
            string id = FrontHistoryIds.Quarter(dto.Year, dto.Season);
            quarters.Add(new FrontQuarter
            {
                Year = dto.Year,
                Season = dto.Season,
                HeldByInvader = dto.HeldByInvader,
                Contested = dto.Contested,
                HeldByDefender = dto.HeldByDefender,
                Headline = headlines.GetValueOrDefault(id, string.Empty),
                Sources = dto.Sources,
                Confidence = dto.Confidence,
            });
        }

        FrontHistory history = new()
        {
            Vocabulary = file.Vocabulary,
            Quarters = quarters,
        };

        Validate(history, headlines, path);
        return history;
    }

    private static Dictionary<string, string> LoadHeadlines(string? dataDirectory, Language language)
    {
        string name = $"front-history.{Languages.Code(language)}.json";
        string path = ResolvePath(dataDirectory, name, required: language == Language.French);
        if (!File.Exists(path))
        {
            return [];
        }

        HeadlineFile? file = JsonSerializer.Deserialize<HeadlineFile>(File.ReadAllText(path), Options);
        if (file is null)
        {
            throw new InvalidDataException($"{name} could not be read.");
        }

        return file.Headlines;
    }

    /// <summary>
    /// A zone the map does not know how to draw would silently vanish from the picture, and the
    /// picture would still look plausible. It is caught here instead, at load time.
    /// </summary>
    private static void Validate(FrontHistory history, Dictionary<string, string> headlines, string path)
    {
        HashSet<string> vocabulary = [.. history.Vocabulary];
        HashSet<string> slots = [.. history.Quarters.Select(quarter => FrontHistoryIds.Quarter(quarter.Year, quarter.Season))];

        foreach (string id in headlines.Keys)
        {
            if (!slots.Contains(id))
            {
                throw new InvalidOperationException(
                    $"A headline is keyed on quarter '{id}', which {path} does not document: the sentence would never be printed again.");
            }
        }

        foreach (FrontQuarter quarter in history.Quarters)
        {
            if (string.IsNullOrWhiteSpace(quarter.Headline))
            {
                throw new InvalidOperationException(
                    $"Quarter {quarter.Season} {quarter.Year} of {path} has no sentence in any language: the map would draw a position nobody explains.");
            }

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

    private static string ResolvePath(string? dataDirectory, string fileName, bool required = true)
    {
        if (!string.IsNullOrWhiteSpace(dataDirectory))
        {
            return Path.Combine(dataDirectory, fileName);
        }

        string local = Path.Combine(AppContext.BaseDirectory, "data", fileName);
        if (File.Exists(local) || !required)
        {
            return local;
        }

        throw new FileNotFoundException($"{fileName} not found next to the executable.", local);
    }

    private sealed class FrontFile
    {
        public IReadOnlyList<string> Vocabulary { get; set; } = [];

        public List<QuarterDto> Quarters { get; set; } = [];
    }

    private sealed class QuarterDto
    {
        public int Year { get; set; }

        public Season Season { get; set; }

        public IReadOnlyList<string> HeldByInvader { get; set; } = [];

        public IReadOnlyList<string> Contested { get; set; } = [];

        public IReadOnlyList<string> HeldByDefender { get; set; } = [];

        public IReadOnlyList<string> Sources { get; set; } = [];

        public string Confidence { get; set; } = string.Empty;
    }

    private sealed class HeadlineFile
    {
        public Dictionary<string, string> Headlines { get; set; } = [];
    }
}
