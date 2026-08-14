using TheoryOfVictory.Core;
using TheoryOfVictory.Core.Localization;
using TheoryOfVictory.Engine;
using TheoryOfVictory.Engine.Scenarios;
using Xunit;

namespace TheoryOfVictory.Engine.UnitTests;

/// <summary>
/// Le moteur n'énonce plus que des faits, et un fait sans phrase ne se voit nulle part : il ne
/// casse rien au calcul, il ne casse rien à l'affichage français si la phrase existe en
/// français, et il n'apparaît qu'au lecteur anglais d'un trimestre rare — une frappe saturée,
/// un armistice, une capitulation négociée.
///
/// Ce test joue les trois déroulés et ÉCRIT tout ce qu'ils ont dit, dans les deux langues. Un
/// code sans phrase lève, une phrase vide échoue, et les branches rares du moteur sont
/// exactement celles que les trois déroulés traversent.
/// </summary>
public sealed class PhrasebookTests
{
    [Fact]
    public void EveryFactTheThreeRunsStateCanBeWritten_InBothLanguages()
    {
        List<LocalizedText> facts = [];

        foreach (SupportVariant variant in Enum.GetValues<SupportVariant>())
        {
            Collect(new GameRunner().Run(UkraineScenario.Build(variant)), facts);
        }

        // Le deck entier, et pas seulement les cartes que le calendrier a jouées : la page les
        // montre toutes, y compris celles restées en main.
        foreach (EventCard card in CardLibrary.Load())
        {
            Collect(CardPrinter.Print(card), facts);
        }

        Assert.NotEmpty(facts);

        foreach (Language language in Languages.All)
        {
            Localizer.Current = language;

            foreach (LocalizedText fact in facts)
            {
                string written = Phrasebook.Say(fact);
                Assert.False(
                    string.IsNullOrWhiteSpace(written),
                    $"'{fact.Code}' ne s'écrit pas en {Languages.Code(language)}.");
            }
        }

        Localizer.Current = Language.French;
    }

    private static void Collect(PlayedGame game, List<LocalizedText> facts)
    {
        Add(facts, game.Title, game.Subtitle, game.Description);
        Add(facts, game.Outcome?.Title, game.Outcome?.Explanation);

        foreach (FrontSector sector in game.FinalSectors)
        {
            Add(facts, sector.Name);
        }

        foreach (TurnSnapshot turn in game.Turns)
        {
            facts.AddRange(turn.Narrative);
            Add(facts, turn.Headline);
            Add(facts, turn.Outcome?.Title, turn.Outcome?.Explanation);

            foreach (PressureAlert alert in turn.Alerts)
            {
                Add(facts, alert.Title, alert.Detail);
            }

            foreach (SectorResolution sector in turn.Sectors)
            {
                Add(facts, sector.SectorName, sector.Outcome);
            }

            foreach (PlayedCard card in turn.CardsPlayed)
            {
                Collect(card, facts);
            }

            foreach (SideSnapshot side in new[] { turn.Invader, turn.Defender })
            {
                Add(facts, side.Name, side.BottleneckName);
                Add(facts, side.Chain?.Origin);

                foreach (CapitalLink link in side.Chain?.Links ?? [])
                {
                    Add(facts, link.Label);
                }

                foreach (CapitalPost post in side.Capital)
                {
                    Add(facts, post.Name, post.Unit, post.ThresholdLabel, post.SecondaryLabel,
                        post.SecondaryUnit, post.DestructionCause);
                }
            }
        }
    }

    private static void Collect(PlayedCard card, List<LocalizedText> facts)
    {
        Add(facts, card.TypeLine);
        facts.AddRange(card.RulesText);
    }

    private static void Add(List<LocalizedText> facts, params LocalizedText?[] texts)
    {
        facts.AddRange(texts.Where(text => text is not null)!);
    }
}
