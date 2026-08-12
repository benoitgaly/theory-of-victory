namespace TheoryOfVictory.Core;

/// <summary>
/// Holds both the real quantity and what the belligerent believes it holds.
/// The gap is produced by corruption and is only discovered on consumption.
/// </summary>
public sealed class Stockpile
{
    private readonly Dictionary<string, double> _actual = [];
    private readonly Dictionary<string, double> _reported = [];

    public double GetActual(ResourceKind kind)
    {
        return _actual.TryGetValue(kind.Code, out double value) ? value : 0d;
    }

    public double GetReported(ResourceKind kind)
    {
        return _reported.TryGetValue(kind.Code, out double value) ? value : 0d;
    }

    /// <summary>Adds real units; <paramref name="phantomRatio"/> inflates the reported figure only.</summary>
    public void Add(ResourceKind kind, double actualUnits, double phantomRatio = 0d)
    {
        if (actualUnits <= 0d)
        {
            return;
        }

        _actual[kind.Code] = GetActual(kind) + actualUnits;
        _reported[kind.Code] = GetReported(kind) + (actualUnits * (1d + phantomRatio));
    }

    /// <summary>Consumes up to <paramref name="requested"/> and returns what really existed.</summary>
    public double Consume(ResourceKind kind, double requested)
    {
        if (requested <= 0d)
        {
            return 0d;
        }

        double available = GetActual(kind);
        double taken = Math.Min(available, requested);
        _actual[kind.Code] = available - taken;

        // Reported stock decays proportionally, so phantom units survive until audited.
        double reported = GetReported(kind);
        double reportedTaken = Math.Min(reported, requested);
        _reported[kind.Code] = reported - reportedTaken;

        return taken;
    }

    /// <summary>Attrition, sabotage and deep strikes remove stock without consumption.</summary>
    public void Destroy(ResourceKind kind, double units)
    {
        Consume(kind, units);
    }

    /// <summary>An audit realigns the reported figure onto reality, at a political cost.</summary>
    public void Audit()
    {
        foreach (string code in _actual.Keys)
        {
            _reported[code] = _actual[code];
        }
    }

    public double PhantomShare(ResourceKind kind)
    {
        double reported = GetReported(kind);
        if (reported <= 0d)
        {
            return 0d;
        }

        return Math.Clamp(1d - (GetActual(kind) / reported), 0d, 1d);
    }
}
