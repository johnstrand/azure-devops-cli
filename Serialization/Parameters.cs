using System.Text.Json.Serialization;

namespace Ado.Serialization;

public class Parameters
{
    [JsonPropertyName("path")]
    public List<Parameter> Path { get; set; } = new();

    [JsonPropertyName("query")]
    public List<Parameter> Query { get; set; } = new();

    [JsonPropertyName("header")]
    public List<Parameter> Header { get; set; } = new();

    [JsonPropertyName("body")]
    public Parameter? Body { get; set; }
}
