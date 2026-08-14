using System.Globalization;

namespace TheoryOfVictory.Core.Localization;

/// <summary>
/// Ce que le moteur produit à la place d'une phrase : un FAIT — un code d'événement et ses
/// paramètres. La mise en phrase se fait à la lecture, dans la langue demandée.
///
/// Un moteur ne parle aucune langue. Tant qu'il écrivait « frappe sur les usines d'armement »,
/// la seule façon de le lire en anglais aurait été de traduire une phrase déjà composée, avec
/// son ordre des mots français et ses accords déjà faits. Il dit maintenant QUOI, et le
/// <see cref="Phrasebook"/> dit COMMENT on l'écrit.
/// </summary>
public sealed class LocalizedText : IFormattable
{
    public required string Code { get; init; }

    public IReadOnlyList<object?> Arguments { get; init; } = [];

    public static LocalizedText Of(string code)
    {
        return new LocalizedText { Code = code };
    }

    public static LocalizedText Of(string code, params object?[] arguments)
    {
        return new LocalizedText { Code = code, Arguments = arguments };
    }

    /// <summary>
    /// Un nombre qui emporte SON format. Sans lui, une valeur passée en argument s'écrirait
    /// avec toutes ses décimales : le moteur choisit la précision — un trimestre et demi de
    /// couverture, pas 1,4972 — et cette précision-là appartient au fait, pas à la langue.
    /// </summary>
    public static IFormattable Number(double value, string format)
    {
        return new FormattedNumber(value, format);
    }

    /// <summary>Rendu dans la langue ambiante. C'est la lecture, jamais le calcul.</summary>
    public override string ToString()
    {
        return Phrasebook.Say(this);
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return Phrasebook.Say(this);
    }

    private sealed class FormattedNumber(double value, string format) : IFormattable
    {
        public string ToString(string? _, IFormatProvider? formatProvider)
        {
            return value.ToString(format, formatProvider ?? CultureInfo.CurrentCulture);
        }

        public override string ToString()
        {
            return ToString(null, CultureInfo.CurrentCulture);
        }
    }
}
