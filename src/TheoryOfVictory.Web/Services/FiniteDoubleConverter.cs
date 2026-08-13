using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheoryOfVictory.Web.Services;

/// <summary>
/// The engine uses infinity as a sentinel — "no countdown running", "no ceiling" —
/// which JSON cannot express. It travels as null, so the page tests for absence.
/// </summary>
public sealed class FiniteDoubleConverter : JsonConverter<double>
{
    public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType == JsonTokenType.Null ? double.PositiveInfinity : reader.GetDouble();
    }

    public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
    {
        if (double.IsFinite(value))
        {
            writer.WriteNumberValue(value);
            return;
        }

        writer.WriteNullValue();
    }
}
