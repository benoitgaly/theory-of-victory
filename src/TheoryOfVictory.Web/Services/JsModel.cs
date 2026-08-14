using System.Text.Json;
using TheoryOfVictory.Core.Localization;

namespace TheoryOfVictory.Web.Services;

/// <summary>
/// What the server hands the page: the language, the culture that formats its numbers, and the
/// catalogue the scripts translate against.
///
/// The scripts call <c>tov.t("Réserves")</c> — the same key as <c>Localizer.Loc("Réserves")</c>
/// in C#, because there is one convention and one catalogue for the whole site. In French the
/// catalogue is EMPTY and every call falls back on its own key: the source language costs
/// nothing to serve.
/// </summary>
public static class JsModel
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private static readonly Dictionary<Language, string> Rendered = [];

    public static string Render(Language language)
    {
        lock (Rendered)
        {
            if (Rendered.TryGetValue(language, out string? json))
            {
                return json;
            }

            json = JsonSerializer.Serialize(
                new
                {
                    Language = Languages.Code(language),
                    NumberLocale = Languages.Culture(language).Name,
                    Translations = language == Language.French
                        ? new Dictionary<string, string>()
                        : new Dictionary<string, string>(Localizer.Catalogue(language)),
                },
                Options);

            Rendered[language] = json;
            return json;
        }
    }
}
