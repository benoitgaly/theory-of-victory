namespace TheoryOfVictory.Core;

/// <summary>
/// One post of war capital at the close of a quarter — what a side still HOLDS to make war
/// with, as opposed to what it is currently putting on the front.
///
/// A level on its own teaches nothing, so every post carries the move that produced it, split
/// by cause: an ordinary draw-down and a destroyed assembly line are the same figure and not
/// the same event. And every post carries its own base-100 index, read against this side's
/// first quarter and never against the other camp: 310 billions of Russian reserve against 29
/// Ukrainian ones on a single scale would only say, falsely, that the war was over before it
/// started. The one question worth asking is which side burns its capital faster.
/// </summary>
public sealed class CapitalPost
{
    public required string Code { get; init; }

    public required string Name { get; init; }

    public required string Unit { get; init; }

    /// <summary>Decimals the board prints. A grid in GW and a budget in billions do not round alike.</summary>
    public int Decimals { get; init; }

    /// <summary>
    /// True when a rising figure is a falling capital. Ukrainian oil is a bill, not a receipt:
    /// a barrel going up takes Ukrainian capital down, which is channel two of the oil rule
    /// obtained without a single new line of model.
    /// </summary>
    public bool Inverted { get; init; }

    public required double Value { get; init; }

    /// <summary>Where the post stood before the ten phases ran. A slope needs two points.</summary>
    public required double Opening { get; init; }

    /// <summary>The same post on this side's first quarter. The base of the index, and of nothing else.</summary>
    public required double Reference { get; init; }

    /// <summary>Where this post breaks, when it has such a value. Four of the seven do not, and draw none.</summary>
    public double? Threshold { get; init; }

    /// <summary>A second reading of the same post: the winter demand, the tap, the living standard.</summary>
    public double? Secondary { get; init; }

    public string? SecondaryLabel { get; init; }

    /// <summary>Repair, growth, revenue, aid received. Drawn engraved: ordinary is never solid matter.</summary>
    public double Regeneration { get; init; }

    /// <summary>Ordinary draw, upkeep, erosion. Engraved too.</summary>
    public double Consumption { get; init; }

    /// <summary>Strike, card, permanent loss. The only part drawn as solid matter, with its notch.</summary>
    public double Destruction { get; init; }

    /// <summary>
    /// What destroyed it, by name — the card title or the target of the wave. A red figure
    /// nobody can argue with is exactly what a teaching tool has to refuse, so a fall nothing
    /// can be named for is booked as ordinary consumption rather than dressed up as damage.
    /// </summary>
    public string? DestructionCause { get; init; }

    /// <summary>True when the loss does not come back inside this war. Draws the padlock.</summary>
    public bool PermanentLoss { get; init; }

    /// <summary>Base 100 at this side's first quarter, capital-wise: up is always healthier.</summary>
    public double Index
    {
        get { return ToIndex(Value); }
    }

    public double OpeningIndex
    {
        get { return ToIndex(Opening); }
    }

    /// <summary>Signed move of the quarter, in the post's own unit.</summary>
    public double Delta
    {
        get { return Value - Opening; }
    }

    /// <summary>
    /// The move as the board prints it. It differs from <see cref="Delta"/> on the one post
    /// whose unit is not a possession but a shared quantity — a bill that shrinks is naturally
    /// written as a fall, while a diplomatic latitude has to be signed from the point of view
    /// of the camp reading it, or the band puts a minus under the side that just gained.
    /// </summary>
    public double DisplayDelta { get; init; }

    private double ToIndex(double value)
    {
        if (Reference <= 0d)
        {
            return 100d;
        }

        if (!Inverted)
        {
            return value / Reference * 100d;
        }

        // A charge: the index is the reference over the value, so a bill twice as heavy
        // halves the capital rather than doubling it.
        return value <= 0d ? 100d : Reference / value * 100d;
    }
}

/// <summary>One link of the consequence ribbon: which post moved, and by how many index points.</summary>
public sealed class CapitalLink
{
    public required string PostCode { get; init; }

    public required string Label { get; init; }

    /// <summary>
    /// Signed move of the quarter, as a share of where the post stood. A percentage rather
    /// than index points, so a post whose base quarter happened to be tiny does not print an
    /// unreadable figure next to a post whose base was large: the ribbon has to be read in one
    /// pass, and every link has to be on the same scale for that to work.
    /// </summary>
    public double PercentDelta { get; init; }
}

/// <summary>
/// The sharpest destruction of the quarter, followed as far downstream as the state honestly
/// allows. One per side and per turn, and none at all on a quiet quarter — a thin band is
/// itself a piece of information.
/// </summary>
public sealed class CapitalChain
{
    /// <summary>The card title or the strike that started it. Never a guess.</summary>
    public required string Origin { get; init; }

    public List<CapitalLink> Links { get; init; } = [];
}
