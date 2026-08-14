using System.Text.Json;
using System.Text.Json.Serialization;
using TheoryOfVictory.Core;
using TheoryOfVictory.Core.Localization;

namespace TheoryOfVictory.Engine.Scenarios;

/// <summary>
/// Loads the deck from data. Adding a card never touches the engine.
///
/// French carries the deck itself — its costs, its effects, its prose. Another language brings
/// nothing but words, laid over the French by card code: a translated deck that carried the
/// numbers again would let a rebalancing hold in one language and not in the other.
/// </summary>
public static class CardLibrary
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static List<EventCard> Load(Language language = Language.French, string? dataDirectory = null)
    {
        string path = ResolvePath(dataDirectory, "cards.fr.json");
        string json = File.ReadAllText(path);

        CardFile? file = JsonSerializer.Deserialize<CardFile>(json, Options);
        if (file is null || file.Cards.Count == 0)
        {
            throw new InvalidOperationException($"No cards found in {path}.");
        }

        Dictionary<string, CardTextDto> texts = LoadTexts(dataDirectory, language, file.Cards, path);

        List<EventCard> cards = [];
        foreach (CardDto dto in file.Cards)
        {
            CardTextDto text = texts.GetValueOrDefault(dto.Code) ?? new CardTextDto();
            List<CardEffect> effects = [];
            foreach (EffectDto effect in dto.Effects)
            {
                effects.Add(new CardEffect
                {
                    Kind = effect.Kind,
                    TargetSideCode = effect.TargetSideCode,
                    Value = effect.Value,
                    ResourceCode = effect.ResourceCode,
                    DelayTurns = effect.DelayTurns,
                });
            }

            cards.Add(new EventCard
            {
                Code = dto.Code,
                Title = text.Title ?? dto.Title,
                Family = dto.Family,
                Description = text.Description ?? dto.Description,
                Type = dto.Type,
                OwnerSideCode = dto.OwnerSideCode,
                PoliticalCost = dto.PoliticalCost,
                MoneyCost = dto.MoneyCost,
                BaseProbability = dto.BaseProbability,
                CountersCardCode = dto.CountersCardCode,
                Effects = effects,
            });
        }

        return cards;
    }

    /// <summary>
    /// The words of another language, by card code. A code the deck does not contain fails the
    /// load: it is a translation orphaned by a renaming, and nothing on the page would ever
    /// reveal it — the card would simply keep reading in French.
    /// </summary>
    private static Dictionary<string, CardTextDto> LoadTexts(
        string? dataDirectory, Language language, List<CardDto> deck, string deckPath)
    {
        if (language == Language.French)
        {
            return [];
        }

        string name = $"cards.{Languages.Code(language)}.json";
        string path = ResolvePath(dataDirectory, name, required: false);
        if (!File.Exists(path))
        {
            return [];
        }

        CardTextFile? file = JsonSerializer.Deserialize<CardTextFile>(File.ReadAllText(path), Options);
        if (file is null)
        {
            throw new InvalidDataException($"{name} could not be read.");
        }

        HashSet<string> codes = [.. deck.Select(card => card.Code)];
        foreach (string code in file.Cards.Keys)
        {
            if (!codes.Contains(code))
            {
                throw new InvalidOperationException(
                    $"{name} translates card '{code}', which {deckPath} does not contain: the text would never be printed again.");
            }
        }

        return file.Cards;
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

    private sealed class CardFile
    {
        public List<CardDto> Cards { get; set; } = [];
    }

    private sealed class CardTextFile
    {
        public Dictionary<string, CardTextDto> Cards { get; set; } = [];
    }

    private sealed class CardTextDto
    {
        public string? Title { get; set; }

        public string? Description { get; set; }
    }

    private sealed class CardDto
    {
        public string Code { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Family { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public CardType Type { get; set; } = CardType.Instant;

        public string? OwnerSideCode { get; set; }

        public double PoliticalCost { get; set; }

        public double MoneyCost { get; set; }

        public double BaseProbability { get; set; }

        public string? CountersCardCode { get; set; }

        public List<EffectDto> Effects { get; set; } = [];
    }

    private sealed class EffectDto
    {
        public EffectKind Kind { get; set; }

        public string? TargetSideCode { get; set; }

        public double Value { get; set; }

        public string? ResourceCode { get; set; }

        public int DelayTurns { get; set; }
    }
}
