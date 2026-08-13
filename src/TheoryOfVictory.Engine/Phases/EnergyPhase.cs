using TheoryOfVictory.Core;

namespace TheoryOfVictory.Engine.Phases;

/// <summary>
/// Phase 1. Electricity never reaches the front but gates everything that does.
/// Load shedding is a threshold: damage under the margin costs nothing at all.
/// </summary>
public sealed class EnergyPhase : ITurnPhase
{
    /// <summary>
    /// Share of the sustainable productive capacity that is civilian plant — warehouses,
    /// distribution, consumer lines — rather than services, farmland or arms works. A working
    /// order of magnitude for both economies, and the parameter that decides whether burning a
    /// distribution hub is a nuisance or a crisis: at 0,24 a sustained campaign of two or three
    /// quarters costs eight to twelve points of discontent, which is sensible without ever
    /// being decisive on its own.
    /// </summary>
    private const double CivilianShareOfCapacity = 0.24d;

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

            // Warehouses are rebuilt on the same rhythm as substations, and the civilian base
            // is sized off the economy the first time anyone looks at it, so no scenario has
            // to state a figure it does not own.
            belligerent.Civilian.Repair();
            if (belligerent.Civilian.CapacityBillions <= 0d)
            {
                belligerent.Civilian.CapacityBillions =
                    belligerent.Economy.ProductiveCapacityBillions * CivilianShareOfCapacity;
            }

            double civilian = belligerent.Grid.CivilianSupplyRatio(context.State.Season);
            double industrial = belligerent.Grid.IndustrialSupplyRatio(context.State.Season);

            // THE single channel from the rear of the rear to the regime. It used to read the
            // power supply alone; it now reads the living standard, which is that same supply
            // multiplied by the share of the civilian plant still standing. With nothing struck
            // the two are the same number to the bit, which is what keeps the three runs where
            // they were — and what makes burning a distribution hub cost something at all.
            belligerent.Civilian.UpdateLivingStandard(civilian);
            double standard = belligerent.Civilian.LivingStandard;
            if (standard < 1d)
            {
                belligerent.Politics.PopularDiscontent = Math.Min(
                    100d,
                    belligerent.Politics.PopularDiscontent + ((1d - standard) * 8d));
            }

            double shortfall = belligerent.Grid.ShortfallRatio(context.State.Season);
            if (shortfall <= 0d)
            {
                continue;
            }

            // Cold homes cost morale; dark factories cost shells two turns later.
            belligerent.Politics.Morale = Math.Max(0d, belligerent.Politics.Morale - ((1d - civilian) * 6d));

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
