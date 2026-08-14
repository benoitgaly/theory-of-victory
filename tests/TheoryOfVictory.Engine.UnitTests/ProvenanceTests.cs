using TheoryOfVictory.Core;
using TheoryOfVictory.Engine.Provenance;
using Xunit;

namespace TheoryOfVictory.Engine.UnitTests;

/// <summary>
/// The provenance database exists for one reason: so that a figure printed on the board can be
/// traced to something published, by anyone, without asking us. That guarantee breaks in two
/// silent ways — an observation citing a source the file never defines, and a source with no
/// address — and neither one breaks anything visible. The page keeps rendering, the value keeps
/// printing, and the citation simply is not there. So these tests hold the invariant on the REAL
/// file, which is the only one the site ever serves, and then check that the guard itself
/// actually fires, because an invariant nobody enforces is a comment.
/// </summary>
public sealed class ProvenanceTests
{
    private static readonly ProvenanceRegistry Registry = ProvenanceLibrary.Load();

    [Fact]
    public void TheDatabase_Loads_AndEveryFigureCarriesAtLeastOneObservation()
    {
        Assert.NotEmpty(Registry.Figures);
        Assert.All(Registry.Figures, figure => Assert.NotEmpty(figure.Observations));
        Assert.All(Registry.Figures, figure => Assert.False(string.IsNullOrWhiteSpace(figure.Label)));
    }

    /// <summary>
    /// The bibliography of a figure is DERIVED from the codes its observations cite, so a code
    /// that does not resolve does not print an error — it prints nothing at all, and the value
    /// ends up on the page looking exactly like a value nobody ever checked.
    /// </summary>
    [Fact]
    public void EveryCitedSource_IsDefinedByTheFile_OrTheValueWouldPrintWithNoCitationAtAll()
    {
        foreach (HistoricalFigure figure in Registry.Figures)
        {
            foreach (FigureObservation observation in figure.Observations)
            {
                if (observation.SourceCode is null)
                {
                    continue;
                }

                Assert.True(
                    Registry.Sources.ContainsKey(observation.SourceCode),
                    $"Figure '{figure.Code}', observation '{observation.Date}' cites '{observation.SourceCode}', which no source declares.");
            }
        }
    }

    [Fact]
    public void EverySource_CarriesAnAddress_BecauseACitationNobodyCanOpenIsNotACitation()
    {
        Assert.All(
            Registry.Sources.Values,
            source => Assert.False(string.IsNullOrWhiteSpace(source.Url), $"Source '{source.Code}' has no address."));
    }

    /// <summary>
    /// The band builds its links itself, in JavaScript, as <c>provenance-{post}-{ru|ua}.html</c>.
    /// A figure whose code is spelled any other way is documented and unreachable: the page exists,
    /// the board never points at it, and nothing anywhere fails.
    /// </summary>
    [Fact]
    public void EveryFigureCode_IsItsPostAndItsSide_BecauseThatIsHowTheBandBuildsTheLink()
    {
        foreach (HistoricalFigure figure in Registry.Figures)
        {
            string side = figure.EngineSide == "invader" ? "ru" : "ua";
            Assert.Equal($"{figure.EnginePost}-{side}", figure.Code);
        }
    }

    [Fact]
    public void AnObservationCitingAnUndefinedSource_StopsTheLoad_RatherThanPrintingAnUncitedValue()
    {
        string directory = WriteDatabase(
            """
            {
              "sources": [
                { "code": "connue", "organisation": "O", "title": "T", "url": "https://exemple.test/", "kind": "K", "note": "N" }
              ],
              "figures": [
                {
                  "code": "reserves-ru", "label": "L", "unit": "Md$", "engineSide": "invader", "enginePost": "reserves",
                  "observations": [
                    { "date": "1er octobre 2021", "value": 1.0, "unit": "Md$", "sourceCode": "inconnue", "confidence": "Haute", "confidenceWhy": "C", "retained": true, "why": "P" }
                  ]
                }
              ]
            }
            """);

        try
        {
            InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => ProvenanceLibrary.Load(directory));
            Assert.Contains("inconnue", error.Message, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void ASourceWithoutAnAddress_StopsTheLoad_ForTheSameReason()
    {
        string directory = WriteDatabase(
            """
            {
              "sources": [
                { "code": "sans-adresse", "organisation": "O", "title": "T", "kind": "K", "note": "N" }
              ],
              "figures": []
            }
            """);

        try
        {
            InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => ProvenanceLibrary.Load(directory));
            Assert.Contains("sans-adresse", error.Message, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>
    /// Sources are keyed by code, so a code declared twice keeps the last one silently — and the
    /// observations citing it change source without anything being edited.
    /// </summary>
    [Fact]
    public void ASourceDeclaredTwice_StopsTheLoad_BecauseTheSecondOneWouldQuietlyReplaceTheFirst()
    {
        string directory = WriteDatabase(
            """
            {
              "sources": [
                { "code": "doublon", "organisation": "O", "title": "Première", "url": "https://exemple.test/1", "kind": "K", "note": "N" },
                { "code": "doublon", "organisation": "O", "title": "Seconde", "url": "https://exemple.test/2", "kind": "K", "note": "N" }
              ],
              "figures": []
            }
            """);

        try
        {
            InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => ProvenanceLibrary.Load(directory));
            Assert.Contains("doublon", error.Message, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static string WriteDatabase(string content)
    {
        DirectoryInfo directory = Directory.CreateTempSubdirectory("tov-provenance");
        File.WriteAllText(Path.Combine(directory.FullName, "historical-figures.json"), content);
        return directory.FullName;
    }
}
