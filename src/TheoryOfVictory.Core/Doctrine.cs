namespace TheoryOfVictory.Core;

/// <summary>
/// How a side splits its war budget. In V1 this is scripted by the scenario;
/// in V2 it becomes the player's only real decision.
/// No allocation may be dominant — if one always wins, the model is miscalibrated.
/// </summary>
public sealed class Doctrine
{
    public double RecruitmentShare { get; set; }

    public double WeaponsShare { get; set; }

    public double StrikeVectorsShare { get; set; }

    public double AirDefenceShare { get; set; }

    public double IndustrialExpansionShare { get; set; }

    public double InnovationShare { get; set; }

    public double FortificationShare { get; set; }

    public double AntiCorruptionShare { get; set; }

    public double CivilianShare { get; set; }

    /// <summary>Share of the budget spent abroad. Purchased-support side only.</summary>
    public double ForeignPurchaseShare { get; set; }

    /// <summary>Interceptors held back to protect the rear rather than the front.</summary>
    public double RearDefenceShare { get; set; } = 0.6d;

    public StrikeTarget PrimaryStrikeTarget { get; set; } = StrikeTarget.Logistics;

    /// <summary>Offensive effort per sector code. Values are relative weights.</summary>
    public Dictionary<string, double> SectorEffort { get; init; } = [];

    /// <summary>Share of combat power committed to attacking rather than holding.</summary>
    public double OffensivePosture { get; set; } = 0.5d;

    /// <summary>Innovation split across the three edges.</summary>
    public double InnovationTacticalShare { get; set; } = 0.5d;

    public double InnovationStrikeShare { get; set; } = 0.25d;

    public double InnovationCounterShare { get; set; } = 0.25d;

    public double TotalShare
    {
        get
        {
            return RecruitmentShare + WeaponsShare + StrikeVectorsShare + AirDefenceShare
                + IndustrialExpansionShare + InnovationShare + FortificationShare
                + AntiCorruptionShare + CivilianShare + ForeignPurchaseShare;
        }
    }

    public Doctrine Clone()
    {
        Doctrine copy = new()
        {
            RecruitmentShare = RecruitmentShare,
            WeaponsShare = WeaponsShare,
            StrikeVectorsShare = StrikeVectorsShare,
            AirDefenceShare = AirDefenceShare,
            IndustrialExpansionShare = IndustrialExpansionShare,
            InnovationShare = InnovationShare,
            FortificationShare = FortificationShare,
            AntiCorruptionShare = AntiCorruptionShare,
            CivilianShare = CivilianShare,
            ForeignPurchaseShare = ForeignPurchaseShare,
            RearDefenceShare = RearDefenceShare,
            PrimaryStrikeTarget = PrimaryStrikeTarget,
            OffensivePosture = OffensivePosture,
            InnovationTacticalShare = InnovationTacticalShare,
            InnovationStrikeShare = InnovationStrikeShare,
            InnovationCounterShare = InnovationCounterShare,
        };

        foreach (KeyValuePair<string, double> entry in SectorEffort)
        {
            copy.SectorEffort[entry.Key] = entry.Value;
        }

        return copy;
    }
}
