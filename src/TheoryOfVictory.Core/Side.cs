namespace TheoryOfVictory.Core;

/// <summary>String-backed enumeration, GA convention: no magic ints in persisted data.</summary>
public sealed class Side : IEquatable<Side>
{
    public static readonly Side Invader = new("invader", "Russie");
    public static readonly Side Defender = new("defender", "Ukraine");

    public static IReadOnlyList<Side> All { get; } = [Invader, Defender];

    private Side(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }

    public string Code { get; }

    public string DisplayName { get; }

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
        return DisplayName;
    }
}
