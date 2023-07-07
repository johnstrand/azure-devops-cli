using System.Text.Json.Serialization;

namespace Ado.Serialization;

public class EnumData
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("values")]
    public List<EnumValue> Values { get; set; } = new();
}
