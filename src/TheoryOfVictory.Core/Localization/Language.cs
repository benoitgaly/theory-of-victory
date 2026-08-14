using System.Globalization;

namespace TheoryOfVictory.Core.Localization;

/// <summary>
/// The languages the site is published in. French is first because it is the source: every
/// translation key IS its French text, so French needs no catalogue to be complete.
/// </summary>
public enum Language
{
    French = 0,

    English = 1,
}

public static class Languages
{
    public static IReadOnlyList<Language> All { get; } = [Language.French, Language.English];

    /// <summary>The code that goes in the URL and in the html lang attribute.</summary>
    public static string Code(Language language)
    {
        return language == Language.English ? "en" : "fr";
    }

    /// <summary>
    /// The culture that formats the numbers. It travels with the language because a number is
    /// read, not computed: 2 064 and 2,064 are the same quantity written for two readers, and a
    /// page that translates its words but keeps the other one's separators reads as a translation.
    /// </summary>
    public static CultureInfo Culture(Language language)
    {
        return language == Language.English ? EnglishCulture : FrenchCulture;
    }

    /// <summary>Anything unknown is French: the source language is the safe default.</summary>
    public static Language Parse(string? code)
    {
        return string.Equals(code, "en", StringComparison.OrdinalIgnoreCase)
            ? Language.English
            : Language.French;
    }

    private static readonly CultureInfo FrenchCulture = new("fr-FR");

    private static readonly CultureInfo EnglishCulture = new("en-GB");
}
