using System.Text.Json;
using System.Text.Json.Serialization;
using TheoryOfVictory.Core;
using TheoryOfVictory.Core.Localization;

namespace TheoryOfVictory.Engine.Provenance;

/// <summary>
/// Reads the historical-figures database. The registry used to be written as C# prose — long
/// paragraphs compiled into the engine — which meant that adding an observation required a
/// rebuild and that nothing could be checked without reading code. It is a data file now, and
/// the rule that goes with it is simple: an observation without a dated source does not go in.
///
/// The file was split in two once the site became bilingual. The FIGURES live in
/// historical-figures.json and exist exactly once: duplicating them per language would mean
/// repeating every correction in every language, and losing the day someone forgets one. The
/// PROSE lives in historical-figures.&lt;lang&gt;.json, keyed by an identifier rebuilt from the data
/// itself. French is always loaded, and the requested language is laid over it — an observation
/// nobody has translated yet reads in French rather than not at all.
/// </summary>
public static class ProvenanceLibrary
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    public static ProvenanceRegistry Load(Language language = Language.French, string? dataDirectory = null)
    {
        string path = ResolvePath(dataDirectory, "historical-figures.json");
        ProvenanceFile? file = JsonSerializer.Deserialize<ProvenanceFile>(File.ReadAllText(path), Options);
        if (file is null)
        {
            throw new InvalidDataException("historical-figures.json could not be read.");
        }

        TextFile texts = LoadTexts(dataDirectory, Language.French);
        if (language != Language.French)
        {
            texts.Overlay(LoadTexts(dataDirectory, language));
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

            TextDto text = texts.Sources.GetValueOrDefault(dto.Code) ?? new TextDto();

            // The name of an organisation, the date we read it and the date its publisher stamps
            // on it are printed as sentences, so they belong to a language even though they live
            // in the data file. The catalogue carries a label for each, and the data value is the
            // fall-back: a source added without one reads in French rather than not at all.
            registry.Sources[dto.Code] = new FigureSource
            {
                Code = dto.Code,
                Organisation = text.Organisation ?? dto.Organisation,
                Title = text.Title ?? dto.Code,
                Url = dto.Url,
                Capture = text.Capture ?? dto.Capture,
                StatedUpdate = text.StatedUpdate ?? dto.StatedUpdate,
                Kind = dto.Kind,
                Note = text.Note ?? string.Empty,
            };
        }

        foreach (FigureDto dto in file.Figures)
        {
            List<FigureObservation> observations = [];
            foreach (ObservationDto observation in dto.Observations)
            {
                string id = ProvenanceIds.Observation(dto.Code, observation.Date, observation.SourceCode, observation.Unit);
                TextDto text = texts.Observations.GetValueOrDefault(id) ?? new TextDto();

                observations.Add(new FigureObservation
                {
                    Date = observation.Date,
                    DateLabel = text.Date ?? observation.Date,
                    Value = observation.Value,
                    Unit = observation.Unit,
                    UnitLabel = text.Unit ?? observation.Unit,
                    SourceCode = observation.SourceCode,
                    Confidence = observation.Confidence,
                    ConfidenceWhy = text.ConfidenceWhy ?? string.Empty,
                    Retained = observation.Retained,
                    Why = text.Why ?? string.Empty,
                });
            }

            TextDto figureText = texts.Figures.GetValueOrDefault(dto.Code) ?? new TextDto();

            registry.Figures.Add(new HistoricalFigure
            {
                Code = dto.Code,
                Label = figureText.Label ?? dto.Code,
                Unit = dto.Unit,
                EngineSide = dto.EngineSide,
                EnginePost = dto.EnginePost,
                Observations = observations,
            });
        }

        Validate(registry, texts, path);
        return registry;
    }

    /// <summary>
    /// The ways this database could lie while looking impeccable, all caught at load rather than
    /// on the page.
    ///
    /// An observation citing a source the file does not define prints its value with nothing
    /// beside it — <see cref="ProvenanceRegistry.SourcesOf"/> derives the bibliography from what
    /// resolves, so the missing citation removes itself from the page instead of showing up as a
    /// gap. And a source without an address cannot be opened, which makes it a claim rather than
    /// a citation: the page would name an organisation and a title that nobody can go and check.
    ///
    /// The last one belongs to the split: a text keyed on something the data no longer contains
    /// is a paragraph that will never be printed again. It is invisible from the page — the page
    /// simply falls back — so it can only be caught here, and it is the signature of an
    /// identifier that moved under the prose.
    /// </summary>
    private static void Validate(ProvenanceRegistry registry, TextFile texts, string path)
    {
        foreach (FigureSource source in registry.Sources.Values)
        {
            if (string.IsNullOrWhiteSpace(source.Url))
            {
                throw new InvalidOperationException(
                    $"Source '{source.Code}' of {path} has no address: a citation nobody can open proves nothing.");
            }
        }

        HashSet<string> observationIds = [];

        foreach (HistoricalFigure figure in registry.Figures)
        {
            foreach (FigureObservation observation in figure.Observations)
            {
                observationIds.Add(ProvenanceIds.Observation(
                    figure.Code, observation.Date, observation.SourceCode, observation.Unit));

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

        foreach (string code in texts.Sources.Keys)
        {
            if (!registry.Sources.ContainsKey(code))
            {
                throw new InvalidOperationException(
                    $"A translated text is keyed on source '{code}', which no longer exists in {path}: the paragraph would never be printed again.");
            }
        }

        foreach (string code in texts.Figures.Keys)
        {
            if (registry.Find(code) is null)
            {
                throw new InvalidOperationException(
                    $"A translated text is keyed on figure '{code}', which no longer exists in {path}: the label would never be printed again.");
            }
        }

        foreach (string id in texts.Observations.Keys)
        {
            if (!observationIds.Contains(id))
            {
                throw new InvalidOperationException(
                    $"A translated text is keyed on observation '{id}', which no longer exists in {path}: the paragraph would never be printed again.");
            }
        }
    }

    private static TextFile LoadTexts(string? dataDirectory, Language language)
    {
        string name = $"historical-figures.{Languages.Code(language)}.json";
        string path = ResolvePath(dataDirectory, name, required: language == Language.French);
        if (!File.Exists(path))
        {
            // No catalogue at all for that language: everything falls back to French, which is
            // the expected state of a language whose prose has not been written yet.
            return new TextFile();
        }

        TextFile? texts = JsonSerializer.Deserialize<TextFile>(File.ReadAllText(path), Options);
        if (texts is null)
        {
            throw new InvalidDataException($"{name} could not be read.");
        }

        return texts;
    }

    private static string ResolvePath(string? dataDirectory, string fileName, bool required = true)
    {
        if (!string.IsNullOrWhiteSpace(dataDirectory))
        {
            return Path.Combine(dataDirectory, fileName);
        }

        string local = Path.Combine(AppContext.BaseDirectory, "data", fileName);
        if (File.Exists(local) || !required)
        {
            return local;
        }

        throw new FileNotFoundException($"{fileName} not found next to the executable.", local);
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

        public string? Url { get; set; }

        public string? Capture { get; set; }

        public string? StatedUpdate { get; set; }

        public string Kind { get; set; } = string.Empty;
    }

    private sealed class FigureDto
    {
        public string Code { get; set; } = string.Empty;

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

        public bool Retained { get; set; }
    }

    /// <summary>
    /// The prose of one language. One shape for the three families of text, because they are all
    /// the same thing: a paragraph attached to an identifier that lives in the data file.
    /// </summary>
    private sealed class TextFile
    {
        public Dictionary<string, TextDto> Sources { get; set; } = [];

        public Dictionary<string, TextDto> Figures { get; set; } = [];

        public Dictionary<string, TextDto> Observations { get; set; } = [];

        /// <summary>
        /// Lays another language over French, field by field rather than entry by entry: a
        /// translator who wrote the label but not the paragraph gets the label translated and
        /// the paragraph in French, instead of losing one of the two.
        /// </summary>
        public void Overlay(TextFile other)
        {
            Merge(Sources, other.Sources);
            Merge(Figures, other.Figures);
            Merge(Observations, other.Observations);
        }

        private static void Merge(Dictionary<string, TextDto> into, Dictionary<string, TextDto> from)
        {
            foreach (KeyValuePair<string, TextDto> entry in from)
            {
                TextDto? existing = into.GetValueOrDefault(entry.Key);
                if (existing is null)
                {
                    into[entry.Key] = entry.Value;
                    continue;
                }

                into[entry.Key] = new TextDto
                {
                    Label = Pick(entry.Value.Label, existing.Label),
                    Title = Pick(entry.Value.Title, existing.Title),
                    Organisation = Pick(entry.Value.Organisation, existing.Organisation),
                    Capture = Pick(entry.Value.Capture, existing.Capture),
                    StatedUpdate = Pick(entry.Value.StatedUpdate, existing.StatedUpdate),
                    Date = Pick(entry.Value.Date, existing.Date),
                    Unit = Pick(entry.Value.Unit, existing.Unit),
                    Note = Pick(entry.Value.Note, existing.Note),
                    Why = Pick(entry.Value.Why, existing.Why),
                    ConfidenceWhy = Pick(entry.Value.ConfidenceWhy, existing.ConfidenceWhy),
                };
            }
        }

        private static string? Pick(string? translated, string? french)
        {
            return string.IsNullOrWhiteSpace(translated) ? french : translated;
        }
    }

    private sealed class TextDto
    {
        public string? Label { get; set; }

        public string? Title { get; set; }

        /// <summary>
        /// The three fields below shadow a value of the data file rather than adding one: the
        /// figures are written once and never duplicated per language, but the words wrapped
        /// around them — an organisation, two dates — have to be readable in the reader's own.
        /// </summary>
        public string? Organisation { get; set; }

        public string? Capture { get; set; }

        public string? StatedUpdate { get; set; }

        /// <summary>
        /// The date and the unit as the page prints them. The data file keeps the two that index
        /// the observation, and they never move: a translated identifier orphans its own prose.
        /// </summary>
        public string? Date { get; set; }

        public string? Unit { get; set; }

        public string? Note { get; set; }

        public string? Why { get; set; }

        public string? ConfidenceWhy { get; set; }
    }
}
