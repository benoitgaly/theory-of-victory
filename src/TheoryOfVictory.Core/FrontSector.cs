namespace TheoryOfVictory.Core;

/// <summary>
/// The engine's internal unit is the thousand men; every unit that leaves it is the man.
///
/// Two conversions, and the difference between them is a rule rather than an oversight:
///
/// <list type="bullet">
/// <item><b>A count is rounded to the thousand.</b> Exposing 671 412 men would claim a
/// precision nobody has — the estimates underneath carry ± 15 %, so the last three digits
/// would be invention dressed as a census.</item>
/// <item><b>The terms of a published quotient are not.</b> When the board prints two figures
/// AND their ratio, the reader can divide them himself: rounding 2 143 and 3 008 to the
/// thousand turns a ratio of 0,71 into a visible 0,67, and two surfaces that contradict each
/// other cost more than either of them is worth.</item>
/// </list>
/// </summary>
public static class ManCount
{
    private const double ThousandsToMen = 1000d;

    /// <summary>A headcount, rounded to the finest grain the sources actually support.</summary>
    public static double FromThousands(double thousands)
    {
        return Math.Round(thousands, MidpointRounding.AwayFromZero) * ThousandsToMen;
    }

    /// <summary>A term the reader will divide by another. Converted, never rounded.</summary>
    public static double ExactFromThousands(double thousands)
    {
        return thousands * ThousandsToMen;
    }
}

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

    /// <summary>Anchor of the sector on the real contact line, February 2022.</summary>
    public double Longitude { get; init; }

    public double Latitude { get; init; }

    /// <summary>Degrees the line moves per hex gained, along this sector's axis of advance.</summary>
    public double PushLongitude { get; init; }

    public double PushLatitude { get; init; }

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

/// <summary>
/// Result of one sector's quarterly resolution, kept for the board display.
///
/// UNIT — every man count below is in MEN, like <see cref="SideSnapshot"/> and unlike the
/// engine, which works in thousands throughout. The board reads people: a counter printing
/// "48" says nothing, "48 000 hommes" is read at once. Nothing leaves the engine in thousands.
///
/// The resolution pair and the per-side pair are NOT the same reading, and confusing them is
/// the easiest mistake to make here. <see cref="AttackerPush"/> against
/// <see cref="HolderResistance"/> is what produced <see cref="Ratio"/>, and the resistance
/// already carries terrain, urbanisation, fortification, drone friction and the season.
/// <see cref="InvaderCommitted"/> and <see cref="DefenderCommitted"/> are the raw power each
/// side put into this sector, comparable to each other and to nothing else.
/// </summary>
public sealed class SectorResolution
{
    public required string SectorCode { get; init; }

    public required string SectorName { get; init; }

    /// <summary>
    /// Which side was pushing. Derivable from the sign of <see cref="HexesMoved"/> only when
    /// the sector moved — that is, almost never, since a still front is the model's normal
    /// result. Published so the board never has to guess.
    /// </summary>
    public required string AttackerSideCode { get; init; }

    /// <summary>What the attacker committed to the assault. The numerator of the ratio.</summary>
    public double AttackerPush { get; init; }

    /// <summary>
    /// What the attacker had to overcome: the holder's cover, multiplied by terrain,
    /// urbanisation, his own fortification and drone friction, divided by the season. The
    /// denominator of the ratio, and never the holder's power.
    /// </summary>
    public double HolderResistance { get; init; }

    /// <summary>Raw power the invader put into this sector — assault and cover together.</summary>
    public double InvaderCommitted { get; init; }

    public double DefenderCommitted { get; init; }

    /// <summary>
    /// What that power would be if the shortest stave covered the need in full. The gap with
    /// <see cref="InvaderCommitted"/> is the men who are present and unsupplied: Liebig, read
    /// on the front rather than on the barrel.
    /// </summary>
    public double InvaderEstablishment { get; init; }

    public double DefenderEstablishment { get; init; }

    public double Ratio { get; init; }

    public double HexesMoved { get; init; }

    /// <summary>Position of the line after this turn, needed to draw the board.</summary>
    public double HexesCumulative { get; init; }

    public int SectorWidth { get; init; }

    /// <summary>Rivers, ridges and woods, as they applied this turn.</summary>
    public double TerrainMultiplier { get; init; }

    public double Urbanisation { get; init; }

    /// <summary>Prepared positions per side. Only the holder's ever counted in the resistance.</summary>
    public double InvaderFortification { get; init; }

    public double DefenderFortification { get; init; }

    /// <summary>
    /// The two factors that explain a stalled sector the raw powers alone do not: tactical
    /// drones make every attack dearer for both sides, and winter takes the edge off an assault.
    /// </summary>
    public double DroneFriction { get; init; }

    public double SeasonModifier { get; init; }

    public double AttackerLosses { get; init; }

    public double DefenderLosses { get; init; }

    public required string Outcome { get; init; }
}

/// <summary>
/// What a side ordered on the front this quarter: how much of its power went to attacking
/// rather than holding, and how that effort was spread. Scripted by the scenario in V1,
/// played by the player in V2 — the display reads the same field either way.
/// </summary>
public sealed class SectorOrders
{
    public required string SideCode { get; init; }

    /// <summary>Share of combat power committed to attacking rather than holding.</summary>
    public double OffensivePosture { get; init; }

    /// <summary>Effort per sector code, normalised so the values sum to one.</summary>
    public Dictionary<string, double> EffortShare { get; init; } = [];
}
