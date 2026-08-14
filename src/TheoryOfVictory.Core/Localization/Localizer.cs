using System.Reflection;
using System.Text;
using System.Text.Json;

namespace TheoryOfVictory.Core.Localization;

/// <summary>
/// The one way a displayed string reaches the screen.
///
/// The key IS the French text — <c>Loc("Réserves")</c>, never <c>Loc("capital.reserves")</c> —
/// so the code stays readable in the language it was written in and a missing translation
/// degrades into correct French instead of into an identifier. Accented characters are handled
/// natively: no unicode escape ever enters a key.
///
/// Interpolation uses %1, %2 … so a translator can move an argument to where the sentence needs
/// it, which a positional format string glued to the French word order would forbid.
///
/// The catalogues are embedded in the assembly rather than read from a database: this site is
/// published as frozen HTML on a host with no server and no database. What Green Acres reads
/// from a table, this reads from a JSON loaded once at startup — same API, same reflexes.
/// </summary>
public static class Localizer
{
    private static readonly AsyncLocal<Language?> Ambient = new();

    private static readonly Lazy<IReadOnlyDictionary<Language, IReadOnlyDictionary<string, string>>> Catalogues =
        new(LoadCatalogues);

    /// <summary>
    /// The language every <see cref="Loc(string, object?[])"/> of the current flow answers in.
    /// It is ambient rather than passed down because it crosses every layer — a view, a phase, a
    /// serializer — and threading it through all of them would put the reader's language in the
    /// signature of the model.
    /// </summary>
    public static Language Current
    {
        get { return Ambient.Value ?? Language.French; }
        set { Ambient.Value = value; }
    }

    public static string Loc(string french, params object?[] arguments)
    {
        return LocIn(Current, french, arguments);
    }

    /// <summary>
    /// The same, in a stated language. Used where the reader's language is data rather than
    /// context: publishing both sites in one run, and the tests.
    /// </summary>
    public static string LocIn(Language language, string french, params object?[] arguments)
    {
        string text = Translate(language, french);
        return arguments.Length == 0 ? text : Interpolate(text, language, arguments);
    }

    /// <summary>
    /// The translation, or the French text when nothing is registered for it. The fall-back is
    /// silent on purpose: an English page that has not been written yet must read as French
    /// prose, not as a hole or a key.
    /// </summary>
    public static string Translate(Language language, string french)
    {
        if (language == Language.French)
        {
            return french;
        }

        if (Catalogues.Value.TryGetValue(language, out IReadOnlyDictionary<string, string>? catalogue)
            && catalogue.TryGetValue(french, out string? translated)
            && !string.IsNullOrWhiteSpace(translated))
        {
            return translated;
        }

        return french;
    }

    /// <summary>The whole catalogue of a language, for the tests that guard it.</summary>
    public static IReadOnlyDictionary<string, string> Catalogue(Language language)
    {
        return Catalogues.Value.TryGetValue(language, out IReadOnlyDictionary<string, string>? catalogue)
            ? catalogue
            : new Dictionary<string, string>();
    }

    private static string Interpolate(string text, Language language, object?[] arguments)
    {
        StringBuilder builder = new(text.Length + 16);
        for (int index = 0; index < text.Length; index++)
        {
            char current = text[index];
            if (current != '%' || index + 1 >= text.Length || !char.IsDigit(text[index + 1]))
            {
                builder.Append(current);
                continue;
            }

            int position = text[index + 1] - '1';
            index++;

            if (position < 0 || position >= arguments.Length)
            {
                // A placeholder nobody passed an argument for. Printing it raw is how the hole
                // gets noticed, where dropping it would leave a sentence that reads fine and
                // says something else.
                builder.Append('%').Append(text[index]);
                continue;
            }

            builder.Append(Format(arguments[position], language));
        }

        return builder.ToString();
    }

    private static string Format(object? argument, Language language)
    {
        if (argument is null)
        {
            return string.Empty;
        }

        return argument is IFormattable formattable
            ? formattable.ToString(null, Languages.Culture(language))
            : argument.ToString() ?? string.Empty;
    }

    private static IReadOnlyDictionary<Language, IReadOnlyDictionary<string, string>> LoadCatalogues()
    {
        Dictionary<Language, IReadOnlyDictionary<string, string>> catalogues = [];
        foreach (Language language in Languages.All)
        {
            catalogues[language] = Read($"ui.{Languages.Code(language)}.json");
        }

        return catalogues;
    }

    private static IReadOnlyDictionary<string, string> Read(string fileName)
    {
        Assembly assembly = typeof(Localizer).Assembly;
        string resource = $"{assembly.GetName().Name}.i18n.{fileName}";
        using Stream? stream = assembly.GetManifestResourceStream(resource);
        if (stream is null)
        {
            throw new InvalidOperationException(
                $"The translation catalogue '{resource}' is not embedded in the assembly: every displayed string would silently fall back to French.");
        }

        Dictionary<string, string>? entries = JsonSerializer.Deserialize<Dictionary<string, string>>(stream);
        if (entries is null)
        {
            throw new InvalidDataException($"The translation catalogue '{resource}' could not be read.");
        }

        entries.Remove(CommentKey);
        return entries;
    }

    /// <summary>JSON has no comments, so the file carries its own instructions under this key.</summary>
    public const string CommentKey = "_comment";
}
