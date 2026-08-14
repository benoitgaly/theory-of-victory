using TheoryOfVictory.Core;
using TheoryOfVictory.Core.Localization;

namespace TheoryOfVictory.Engine.Phases;

/// <summary>
/// Phase 1. Electricity never reaches the front but gates everything that does.
/// Load shedding is a threshold: damage under the margin costs nothing at all.
/// </summary>
public sealed class EnergyPhase : ITurnPhase
{
    /// <summary>Share of the gap to its target the civilian plant closes each quarter.</summary>
    private const double CivilianAdjustmentPerTurn = 0.2d;


    public string Name
    {
        get { return "Energy"; }
    }

    public void Execute(TurnContext context)
    {
        foreach (Side side in Side.All)
        {
            Belligerent belligerent = context.State.Get(side);
            belligerent.Grid.Repair();

            // Warehouses are rebuilt on the same rhythm as substations. The civilian plant is
            // then re-sized, every turn and not once at the outset: it used to be a fixed share
            // of the economy, posed at 0,24, which meant that a country pouring a fifth of its
            // GDP into the war owned exactly as much civilian industry as one at peace. What
            // the war takes, the civilian base stops getting — unless somebody else is paying,
            // which is the whole difference between the two belligerents here.
            belligerent.Civilian.Repair();

            // Le stock rejoint sa cible, il ne saute pas dessus. Posée telle quelle, la cible
            // faisait perdre trente pour cent d'appareil civil au trimestre où l'aide s'arrête
            // et les rendait au suivant : un yo-yo qu'aucune usine ne sait faire. Un cinquième
            // de l'écart par trimestre, c'est-à-dire quelques années pour encaisser un choc,
            // ce qui est le rythme auquel une industrie se démonte ou se rebâtit.
            double target = belligerent.Economy.CivilianCapacityBillions(belligerent.Foreign.EffectiveGrantBillions);
            belligerent.Civilian.CapacityBillions = belligerent.Civilian.CapacityBillions <= 0d
                ? target
                : belligerent.Civilian.CapacityBillions
                    + (target - belligerent.Civilian.CapacityBillions) * CivilianAdjustmentPerTurn;

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
                context.Say(LocalizedText.Of(
                    TextCodes.Narrative.IndustrialShedding,
                    belligerent.Name,
                    LocalizedText.Number((1d - industrial) * 100d, "F0")));
            }
            else if (context.State.Season == Season.Winter)
            {
                context.Say(LocalizedText.Of(TextCodes.Narrative.CivilianCuts, belligerent.Name));
            }
        }
    }
}
