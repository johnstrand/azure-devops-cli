using System.Text.Json.Serialization;

namespace Ado.Serialization;

public class EnumValue
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = null!;

    [JsonPropertyName("description")]
    public string Description { get; set; } = null!;
}