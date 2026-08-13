namespace TheoryOfVictory.Core;

/// <summary>
/// How long a broken army takes to stop existing, and what ends the war once it has. These belong
/// to the scenario, not to the engine: the calendar is a scenario decision, and whoever writes the
/// calendar has to be able to move the ending without asking anyone for code.
///
/// The defaults describe a dissolution over four quarters. Two historical references bracket that
/// choice and they are far apart: the German army of autumn 1918 came apart in weeks once its rear
/// gave way, the Russian army of 1917 took the better part of a year. Three to four quarters sits
/// between them, and that is as precise as anyone can honestly be.
/// </summary>
public sealed class AftermathRules
{
    /// <summary>
    /// Share of its establishment the broken side loses from the line every quarter, once nobody
    /// pays it. Raise it to shorten the ending, lower it to draw it out.
    /// </summary>
    public double DissolutionPerTurn { get; init; } = 0.55d;

    /// <summary>
    /// Below this share of its establishment, the broken side no longer holds a front and the war
    /// stops. A threshold rather than a turn count, so the ending survives a change of calendar.
    /// </summary>
    public double ArmisticeManningRatio { get; init; } = 0.12d;

    /// <summary>
    /// Hard bound on the aftermath, whatever the threshold does. It exists so the timeline can
    /// never fill with empty quarters, and it ends the war rather than letting it drift.
    /// </summary>
    public int MaxTurns { get; init; } = 6;

    /// <summary>
    /// Quarters between the rupture and the armistice, for an army still at full establishment
    /// when it breaks. The armistice is declared on this quarter and that quarter is NOT played,
    /// so the war shows <c>QuartersToArmistice - 1</c> quarters of dissolution.
    ///
    /// This is the number the calendar needs: a scenario that wants the war to end on a given
    /// quarter has to make the rupture land that many quarters earlier. An army already under its
    /// establishment when it breaks — which is the ordinary case — gets there sooner.
    /// </summary>
    public int QuartersToArmistice
    {
        get
        {
            if (DissolutionPerTurn <= 0d || DissolutionPerTurn >= 1d || ArmisticeManningRatio <= 0d)
            {
                return MaxTurns;
            }

            double quarters = Math.Log(ArmisticeManningRatio) / Math.Log(1d - DissolutionPerTurn);
            return Math.Clamp((int)Math.Ceiling(quarters), 1, MaxTurns);
        }
    }

    /// <summary>Share of the establishment still in the line this many quarters after the rupture.</summary>
    public double RemainingShareAfter(int quarters)
    {
        return Math.Pow(1d - DissolutionPerTurn, Math.Max(0, quarters));
    }
}
