using System.Text.Json;
using System.Text.Json.Serialization;
using TheoryOfVictory.Core.Localization;

namespace TheoryOfVictory.Web.Services;

/// <summary>
/// Un fait du moteur, écrit dans la page comme une phrase.
///
/// Le plateau reçoit du texte, comme il en a toujours reçu : c'est la mise en phrase qui a
/// changé de place, pas le contrat. Elle se fait ICI, au moment où la page est sérialisée, donc
/// dans la langue de cette page — et les trois déroulés, eux, n'ont été joués qu'une fois.
/// </summary>
public sealed class LocalizedTextConverter : JsonConverter<LocalizedText>
{
    public override LocalizedText Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException("A sentence already written never turns back into a fact.");
    }

    public override void Write(Utf8JsonWriter writer, LocalizedText value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(Phrasebook.Say(value));
    }
}
