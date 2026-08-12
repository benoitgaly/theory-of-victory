namespace TheoryOfVictory.Core;

/// <summary>
/// One hex is 10 km. Power is allocated per sector, never per unit: this game has
/// no tactical manoeuvre, only the arithmetic of what reaches the line.
/// </summary>
public sealed class FrontSector
{
    public required string Code { get; init; }

    public required string Name { get; init; }

    /// <summary>Rivers, ridges and woods. Above one favours the defender.</summary>
    public double TerrainMultiplier { get; init; } = 1d;

    /// <summary>Cities are the cheapest defence there is, and the costliest to take.</summary>
    public double Urbanisation { get; init; }

    /// <summary>Prepared positions, per side: they only count for whoever is holding.</summary>
    public double InvaderFortification { get; set; }

    public double DefenderFortification { get; set; }

    /// <summary>Hexes gained by the invader since the start. Negative means pushed back.</summary>
    public double HexesGained { get; set; }

    /// <summary>Hex column count drawn on the board for this sector.</summary>
    public int Width { get; init; } = 6;

    /// <summary>How much this sector is worth if taken: rail nodes, industry, symbolism.</summary>
    public double StrategicValue { get; init; } = 1d;

    public double KilometresGained
    {
        get { return HexesGained * 10d; }
    }

    public double FortificationOf(Side side)
    {
        return side == Side.Invader ? InvaderFortification : DefenderFortification;
    }

    public void Fortify(Side side, double amount)
    {
        if (side == Side.Invader)
        {
            InvaderFortification = Math.Min(1.2d, InvaderFortification + amount);
            return;
        }

        DefenderFortification = Math.Min(1.2d, DefenderFortification + amount);
    }

    /// <summary>Total multiplier working for whoever is holding this ground.</summary>
    public double DefensiveMultiplier(Side holder)
    {
        return TerrainMultiplier * (1d + Urbanisation) * (1d + FortificationOf(holder));
    }
}

/// <summary>Result of one sector's quarterly resolution, kept for the board display.</summary>
public sealed class SectorResolution
{
    public required string SectorCode { get; init; }

    public required string SectorName { get; init; }

    public double AttackerPower { get; init; }

    public double DefenderPower { get; init; }

    public double Ratio { get; init; }

    public double HexesMoved { get; init; }

    /// <summary>Position of the line after this turn, needed to draw the board.</summary>
    public double HexesCumulative { get; init; }

    public int SectorWidth { get; init; }

    public double AttackerLosses { get; init; }

    public double DefenderLosses { get; init; }

    public required string Outcome { get; init; }
}
