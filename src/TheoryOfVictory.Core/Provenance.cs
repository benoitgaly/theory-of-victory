namespace TheoryOfVictory.Core;

/// <summary>
/// How a paragraph finds the figure it belongs to, now that the prose and the numbers live in
/// two files. Written once, here, because the identifier has to be rebuilt identically by
/// everything that touches the split — the loader, the tests, and the script that produced it.
/// </summary>
public static class ProvenanceIds
{
    /// <summary>
    /// figure, date, source, unit. The source alone does not separate them: SIPRI publishes two
    /// readings of the same year — a share of GDP and a share of public spending — from the same
    /// fact sheet, and both belong in the registry. The VALUE is deliberately left out: a figure
    /// gets corrected, and a correction must not orphan the paragraph that explains it.
    /// </summary>
    public static string Observation(string figureCode, string date, string? sourceCode, string unit)
    {
        return string.Join('|', figureCode, date, sourceCode ?? string.Empty, unit);
    }
}

/// <summary>
/// One source, named once and reusable by as many observations as need it. That reuse is the
/// reason sources are a table rather than a field.
/// </summary>
public sealed class FigureSource
{
    public required string Code { get; init; }

    public required string Organisation { get; init; }

    public required string Title { get; init; }

    /// <summary>
    /// The address, and it must carry a date. A live page republished at the same URL cannot
    /// prove what it showed on the day it was cited, so a dated archive is preferred to the
    /// page itself whenever the page has no date of its own.
    /// </summary>
    public string? Url { get; init; }

    /// <summary>When we read it.</summary>
    public string? Capture { get; init; }

    /// <summary>What date the publisher itself stamps on the page.</summary>
    public string? StatedUpdate { get; init; }

    public string Kind { get; init; } = string.Empty;

    /// <summary>What this source proves, and where it stops proving anything.</summary>
    public string Note { get; init; } = string.Empty;
}

/// <summary>
/// One dated observation: a value, at a date, from a source, with how much it can be trusted and
/// why it is — or is not — the value the engine carries.
///
/// This is the whole point of the registry. Everything else on the provenance page is a heading.
/// </summary>
public sealed class FigureObservation
{
    public required string Date { get; init; }

    public required double Value { get; init; }

    public required string Unit { get; init; }

    /// <summary>Null when nothing published supports it — and the page says so in those words.</summary>
    public string? SourceCode { get; init; }

    /// <summary>Haute, Moyenne, Basse.</summary>
    public required string Confidence { get; init; }

    public string ConfidenceWhy { get; init; } = string.Empty;

    /// <summary>True when the engine actually carries this value at this date.</summary>
    public bool Retained { get; init; }

    public string Why { get; init; } = string.Empty;
}

/// <summary>One figure of the model and the dated observations behind it.</summary>
public sealed class HistoricalFigure
{
    public required string Code { get; init; }

    public required string Label { get; init; }

    public required string Unit { get; init; }

    public required string EngineSide { get; init; }

    public required string EnginePost { get; init; }

    public List<FigureObservation> Observations { get; init; } = [];
}

public sealed class ProvenanceRegistry
{
    public Dictionary<string, FigureSource> Sources { get; init; } = [];

    public List<HistoricalFigure> Figures { get; init; } = [];

    public HistoricalFigure? Find(string code)
    {
        foreach (HistoricalFigure figure in Figures)
        {
            if (string.Equals(figure.Code, code, StringComparison.OrdinalIgnoreCase))
            {
                return figure;
            }
        }

        return null;
    }

    /// <summary>
    /// The sources actually cited by this figure's observations, in the order they first appear.
    /// Derived rather than listed: a source nobody cites has no business on the page.
    /// </summary>
    public List<FigureSource> SourcesOf(HistoricalFigure figure)
    {
        List<FigureSource> found = [];
        foreach (FigureObservation observation in figure.Observations)
        {
            if (observation.SourceCode is null)
            {
                continue;
            }

            if (!Sources.TryGetValue(observation.SourceCode, out FigureSource? source))
            {
                continue;
            }

            if (!found.Contains(source))
            {
                found.Add(source);
            }
        }

        return found;
    }
}
