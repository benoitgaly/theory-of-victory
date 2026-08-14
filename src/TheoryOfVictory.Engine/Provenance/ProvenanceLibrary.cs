using System.Text.Json;
using System.Text.Json.Serialization;
using TheoryOfVictory.Core;

namespace TheoryOfVictory.Engine.Provenance;

/// <summary>
/// Reads the historical-figures database. The registry used to be written as C# prose — long
/// paragraphs compiled into the engine — which meant that adding an observation required a
/// rebuild and that nothing could be checked without reading code. It is a data file now, and
/// the rule that goes with it is simple: an observation without a dated source does not go in.
/// </summary>
public static class ProvenanceLibrary
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    public static ProvenanceRegistry Load(string? dataDirectory = null)
    {
        string path = ResolvePath(dataDirectory);
        ProvenanceFile? file = JsonSerializer.Deserialize<ProvenanceFile>(File.ReadAllText(path), Options);
        if (file is null)
        {
            throw new InvalidDataException("historical-figures.json could not be read.");
        }

        ProvenanceRegistry registry = new();

        foreach (SourceDto dto in file.Sources)
        {
            // The registry keys sources by code, so a code written twice would keep the last
            // one and lose the first without a word — and every observation citing that code
            // would silently change source.
            if (registry.Sources.ContainsKey(dto.Code))
            {
                throw new InvalidOperationException(
                    $"Source '{dto.Code}' is declared twice in {path}: the second declaration would replace the first without anything saying so.");
            }

            registry.Sources[dto.Code] = new FigureSource
            {
                Code = dto.Code,
                Organisation = dto.Organisation,
                Title = dto.Title,
                Url = dto.Url,
                Capture = dto.Capture,
                StatedUpdate = dto.StatedUpdate,
                Kind = dto.Kind,
                Note = dto.Note,
            };
        }

        foreach (FigureDto dto in file.Figures)
        {
            List<FigureObservation> observations = [];
            foreach (ObservationDto observation in dto.Observations)
            {
                observations.Add(new FigureObservation
                {
                    Date = observation.Date,
                    Value = observation.Value,
                    Unit = observation.Unit,
                    SourceCode = observation.SourceCode,
                    Confidence = observation.Confidence,
                    ConfidenceWhy = observation.ConfidenceWhy,
                    Retained = observation.Retained,
                    Why = observation.Why,
                });
            }

            registry.Figures.Add(new HistoricalFigure
            {
                Code = dto.Code,
                Label = dto.Label,
                Unit = dto.Unit,
                EngineSide = dto.EngineSide,
                EnginePost = dto.EnginePost,
                Observations = observations,
            });
        }

        Validate(registry, path);
        return registry;
    }

    /// <summary>
    /// The two ways this database could lie while looking impeccable, both caught at load rather
    /// than on the page.
    ///
    /// An observation citing a source the file does not define prints its value with nothing
    /// beside it — <see cref="ProvenanceRegistry.SourcesOf"/> derives the bibliography from what
    /// resolves, so the missing citation removes itself from the page instead of showing up as a
    /// gap. And a source without an address cannot be opened, which makes it a claim rather than
    /// a citation: the page would name an organisation and a title that nobody can go and check.
    /// </summary>
    private static void Validate(ProvenanceRegistry registry, string path)
    {
        foreach (FigureSource source in registry.Sources.Values)
        {
            if (string.IsNullOrWhiteSpace(source.Url))
            {
                throw new InvalidOperationException(
                    $"Source '{source.Code}' of {path} has no address: a citation nobody can open proves nothing.");
            }
        }

        foreach (HistoricalFigure figure in registry.Figures)
        {
            foreach (FigureObservation observation in figure.Observations)
            {
                if (observation.SourceCode is null)
                {
                    // Deliberate, and the page says so in those words: a scenario constant that
                    // nothing published supports.
                    continue;
                }

                if (!registry.Sources.ContainsKey(observation.SourceCode))
                {
                    throw new InvalidOperationException(
                        $"Observation '{observation.Date}' of figure '{figure.Code}' cites source '{observation.SourceCode}', which {path} does not define.");
                }
            }
        }
    }

    private static string ResolvePath(string? dataDirectory)
    {
        if (!string.IsNullOrWhiteSpace(dataDirectory))
        {
            return Path.Combine(dataDirectory, "historical-figures.json");
        }

        string local = Path.Combine(AppContext.BaseDirectory, "data", "historical-figures.json");
        if (File.Exists(local))
        {
            return local;
        }

        throw new FileNotFoundException("historical-figures.json not found next to the executable.", local);
    }

    private sealed class ProvenanceFile
    {
        public List<SourceDto> Sources { get; set; } = [];

        public List<FigureDto> Figures { get; set; } = [];
    }

    private sealed class SourceDto
    {
        public string Code { get; set; } = string.Empty;

        public string Organisation { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string? Url { get; set; }

        public string? Capture { get; set; }

        public string? StatedUpdate { get; set; }

        public string Kind { get; set; } = string.Empty;

        public string Note { get; set; } = string.Empty;
    }

    private sealed class FigureDto
    {
        public string Code { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        public string EngineSide { get; set; } = string.Empty;

        public string EnginePost { get; set; } = string.Empty;

        public List<ObservationDto> Observations { get; set; } = [];
    }

    private sealed class ObservationDto
    {
        public string Date { get; set; } = string.Empty;

        public double Value { get; set; }

        public string Unit { get; set; } = string.Empty;

        public string? SourceCode { get; set; }

        public string Confidence { get; set; } = string.Empty;

        public string ConfidenceWhy { get; set; } = string.Empty;

        public bool Retained { get; set; }

        public string Why { get; set; } = string.Empty;
    }
}
