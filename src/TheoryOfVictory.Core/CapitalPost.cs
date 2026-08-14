using TheoryOfVictory.Core.Localization;

namespace TheoryOfVictory.Core;

/// <summary>
/// What a post is made of, and therefore what it may be added to. A balance sheet that added
/// an asset to a year of aid would be counting a holding and an income as the same thing, so
/// the band totals them apart and says which is which.
/// </summary>
public enum CapitalNature
{
    /// <summary>
    /// An asset the side owns: the cash in the fund, and every production valued at five years
    /// of itself. That multiple is what makes an oil field, a power fleet and a shell line
    /// comparable at all — it is the rule of the balance sheet, not a detail of the reading.
    /// </summary>
    Stock = 0,

    /// <summary>
    /// A year of something the side has no title to and cannot capitalise: what it is given
    /// from outside and can lose in a day, and what it can still spend holding itself in place.
    /// </summary>
    AnnualFlow = 1,

    /// <summary>Not a possession — a position, read on a hundred. Never enters either total.</summary>
    Position = 2,
}

/// <summary>
/// One post of war capital at the close of a quarter — what a side still HOLDS to make war
/// with, as opposed to what it is currently putting on the front.
///
/// Every post is priced in billions of dollars, both camps, because a capital is counted in
/// money: an index in base 100, gigawatts, points of margin and a ratio are five languages,
/// and five languages make a list rather than a balance sheet. The conversion coefficients and
/// their uncertainty live in <c>docs/design/08-capital-de-guerre.md</c>.
///
/// A level on its own teaches nothing, so every post carries the move that produced it, split
/// by cause: an ordinary draw-down and a destroyed assembly line are the same figure and not
/// the same event. And the move is printed as a percentage of where the post opened — the one
/// form that reads alike on a post worth 310 billions and on one worth 1,3.
/// </summary>
public sealed class CapitalPost
{
    public required string Code { get; init; }

    public required LocalizedText Name { get; init; }

    /// <summary>Billions of dollars on the seven posts of the band, and nothing else.</summary>
    public required LocalizedText Unit { get; init; }

    public required CapitalNature Nature { get; init; }

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

    /// <summary>What the threshold is, in words: a red line nobody can name is a red line nobody trusts.</summary>
    public LocalizedText? ThresholdLabel { get; init; }

    /// <summary>
    /// A second reading of the same post, in its own physical unit: the gigawatts behind the
    /// valuation, the living standard, the tank behind the tap. The money says how much is
    /// held; this says what it is made of.
    /// </summary>
    public double? Secondary { get; init; }

    public LocalizedText? SecondaryLabel { get; init; }

    public LocalizedText? SecondaryUnit { get; init; }

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
    public LocalizedText? DestructionCause { get; init; }

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

    /// <summary>Signed move of the quarter, in billions.</summary>
    public double Delta
    {
        get { return Value - Opening; }
    }

    /// <summary>
    /// The move of the quarter as the band prints it: a share of where the post opened, in
    /// percent, counted capital-wise. It is the one form that works on every post — a bill and
    /// a receipt, a post worth 310 billions and a post worth 1,3 — and it is the reading that
    /// survives the band being drawn on a scale shared by two camps of very different size,
    /// where a Ukrainian mass is short by construction and its trajectory can no longer be read
    /// off the bar. Percent for the level, and a mass for the level: two questions, two answers.
    /// </summary>
    public double PercentDelta
    {
        get
        {
            double opening = OpeningIndex;
            if (opening <= 0.01d)
            {
                return 0d;
            }

            return (Index - opening) / opening * 100d;
        }
    }

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

/// <summary>One link of the consequence ribbon: which post moved, and by how much.</summary>
public sealed class CapitalLink
{
    public required string PostCode { get; init; }

    public required LocalizedText Label { get; init; }

    /// <summary>
    /// Signed move of the quarter, as a share of where the post stood. The same form as
    /// everywhere else on the band: a percentage rather than billions, so a post whose base
    /// happened to be tiny does not print an unreadable figure next to a post whose base was
    /// large. The ribbon has to be read in one pass, and every link has to be on the same
    /// scale for that to work.
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
    public required LocalizedText Origin { get; init; }

    public List<CapitalLink> Links { get; init; } = [];
}
