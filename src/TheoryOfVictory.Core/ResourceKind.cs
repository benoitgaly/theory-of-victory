namespace TheoryOfVictory.Core;

/// <summary>
/// The physical flows of the game. Money is deliberately not one of them:
/// money buys flows, it never reaches the front itself.
/// </summary>
public sealed class ResourceKind : IEquatable<ResourceKind>
{
    // Front flows: consumed every turn by the troops in line. Counted in thousands of rounds / kilotonnes.
    public static readonly ResourceKind Weapons = new("weapons", "Armes", "front", 4.5d);
    public static readonly ResourceKind Fuel = new("fuel", "Carburant", "front", 0.9d);
    public static readonly ResourceKind Food = new("food", "Nourriture", "front", 0.4d);

    // Deep strike vectors: never reach the front, they attack what produces it.
    public static readonly ResourceKind StrikeDrones = new("strike_drones", "Drones d'attaque", "strike", 0.035d);
    public static readonly ResourceKind Missiles = new("missiles", "Missiles", "strike", 1.6d);

    // Interceptors: the cost-exchange battle is fought here.
    public static readonly ResourceKind CheapInterceptors = new("cheap_interceptors", "Défense bas coût", "defence", 0.02d);
    public static readonly ResourceKind HeavyInterceptors = new("heavy_interceptors", "Intercepteurs lourds", "defence", 3.2d);

    public static IReadOnlyList<ResourceKind> All { get; } =
        [Weapons, Fuel, Food, StrikeDrones, Missiles, CheapInterceptors, HeavyInterceptors];

    public static IReadOnlyList<ResourceKind> FrontFlows { get; } = [Weapons, Fuel, Food];

    public static IReadOnlyList<ResourceKind> StrikeVectors { get; } = [StrikeDrones, Missiles];

    public static IReadOnlyList<ResourceKind> Interceptors { get; } = [CheapInterceptors, HeavyInterceptors];

    private ResourceKind(string code, string displayName, string family, double unitCostMillions)
    {
        Code = code;
        DisplayName = displayName;
        Family = family;
        UnitCostMillions = unitCostMillions;
    }

    public string Code { get; }

    public string DisplayName { get; }

    /// <summary>front, strike or defence — drives grouping on the board display.</summary>
    public string Family { get; }

    /// <summary>Reference cost in millions, before procurement inflation. Drives the exchange ratio.</summary>
    public double UnitCostMillions { get; }

    public static ResourceKind FromCode(string code)
    {
        foreach (ResourceKind kind in All)
        {
            if (string.Equals(kind.Code, code, StringComparison.OrdinalIgnoreCase))
            {
                return kind;
            }
        }

        throw new ArgumentOutOfRangeException(nameof(code), code, "Unknown resource code.");
    }

    public bool Equals(ResourceKind? other)
    {
        return other is not null && other.Code == Code;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ResourceKind);
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
