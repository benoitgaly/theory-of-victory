using System.Text.Json;
using System.Text.Json.Serialization;
using TheoryOfVictory.Core;

namespace TheoryOfVictory.Engine.Scenarios;

/// <summary>Loads the deck from data. Adding a card never touches the engine.</summary>
public static class CardLibrary
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static List<EventCard> Load(string? dataDirectory = null)
    {
        string path = ResolvePath(dataDirectory);
        string json = File.ReadAllText(path);

        CardFile? file = JsonSerializer.Deserialize<CardFile>(json, Options);
        if (file is null || file.Cards.Count == 0)
        {
            throw new InvalidOperationException($"No cards found in {path}.");
        }

        List<EventCard> cards = [];
        foreach (CardDto dto in file.Cards)
        {
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
                Title = dto.Title,
                Family = dto.Family,
                Description = dto.Description,
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

    private static string ResolvePath(string? dataDirectory)
    {
        if (!string.IsNullOrWhiteSpace(dataDirectory))
        {
            return Path.Combine(dataDirectory, "cards.fr.json");
        }

        string local = Path.Combine(AppContext.BaseDirectory, "data", "cards.fr.json");
        if (File.Exists(local))
        {
            return local;
        }

        throw new FileNotFoundException("cards.fr.json not found next to the executable.", local);
    }

    private sealed class CardFile
    {
        public List<CardDto> Cards { get; set; } = [];
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
