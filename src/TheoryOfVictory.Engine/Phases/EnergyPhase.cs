using TheoryOfVictory.Core;

namespace TheoryOfVictory.Engine.Phases;

/// <summary>
/// Phase 1. Electricity never reaches the front but gates everything that does.
/// Load shedding is a threshold: damage under the margin costs nothing at all.
/// </summary>
public sealed class EnergyPhase : ITurnPhase
{
    public string Name
    {
        get { return "Énergie"; }
    }

    public void Execute(TurnContext context)
    {
        foreach (Side side in Side.All)
        {
            Belligerent belligerent = context.State.Get(side);
            belligerent.Grid.Repair();

            double shortfall = belligerent.Grid.ShortfallRatio(context.State.Season);
            if (shortfall <= 0d)
            {
                continue;
            }

            double civilian = belligerent.Grid.CivilianSupplyRatio(context.State.Season);
            double industrial = belligerent.Grid.IndustrialSupplyRatio(context.State.Season);

            // Cold homes cost morale; dark factories cost shells two turns later.
            belligerent.Politics.Morale = Math.Max(0d, belligerent.Politics.Morale - ((1d - civilian) * 6d));
            belligerent.Politics.PopularDiscontent = Math.Min(100d, belligerent.Politics.PopularDiscontent + ((1d - civilian) * 8d));

            if (industrial < 1d)
            {
                context.Say($"{belligerent.Name} : délestage industriel, {(1d - industrial) * 100d:F0} % de la production d'armes perdue.");
            }
            else if (context.State.Season == Season.Winter)
            {
                context.Say($"{belligerent.Name} : coupures civiles en plein hiver, le moral encaisse.");
            }
        }
    }
}
