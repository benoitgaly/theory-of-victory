using TheoryOfVictory.Core.Localization;

namespace TheoryOfVictory.Core;

/// <summary>String-backed enumeration, GA convention: no magic ints in persisted data.</summary>
public sealed class Side : IEquatable<Side>
{
    public static readonly Side Invader = new(
        "invader", TextCodes.Side.Invader, TextCodes.Side.InvaderInProse, TextCodes.Side.InvaderOpening);

    public static readonly Side Defender = new(
        "defender", TextCodes.Side.Defender, TextCodes.Side.DefenderInProse, TextCodes.Side.DefenderOpening);

    public static IReadOnlyList<Side> All { get; } = [Invader, Defender];

    private Side(string code, string label, string inProse, string opening)
    {
        Code = code;
        Label = LocalizedText.Of(label);
        LabelInProse = LocalizedText.Of(inProse);
        LabelOpeningSentence = LocalizedText.Of(opening);
    }

    public string Code { get; }

    /// <summary>Le nom nu, sur une étiquette.</summary>
    public LocalizedText Label { get; }

    /// <summary>Le nom avec son article, au milieu d'une phrase : « les dépôts de la Russie ».</summary>
    public LocalizedText LabelInProse { get; }

    /// <summary>Le même en tête de phrase, où l'article prend la majuscule.</summary>
    public LocalizedText LabelOpeningSentence { get; }

    public Side Opponent
    {
        get { return this == Invader ? Defender : Invader; }
    }

    public static Side FromCode(string code)
    {
        foreach (Side side in All)
        {
            if (string.Equals(side.Code, code, StringComparison.OrdinalIgnoreCase))
            {
                return side;
            }
        }

        throw new ArgumentOutOfRangeException(nameof(code), code, "Unknown side code.");
    }

    public bool Equals(Side? other)
    {
        return other is not null && other.Code == Code;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Side);
    }

    public override int GetHashCode()
    {
        return Code.GetHashCode(StringComparison.Ordinal);
    }

    public override string ToString()
    {
        return Label.ToString();
    }
}
