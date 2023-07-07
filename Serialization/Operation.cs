using System.Text.Json.Serialization;

namespace Ado.Serialization;

internal class Operation
{
    public string Name { get; set; } = null!;

    [JsonPropertyName("urlTemplate")]
    public string UrlTemplate { get; set; } = null!;

    [JsonPropertyName("verb")]
    public string Verb { get; set; } = null!;

    [JsonPropertyName("apiVersion")]
    public string ApiVersion { get; set; } = null!;

    [JsonPropertyName("description")]
    public string Description { get; set; } = null!;

    [JsonPropertyName("operationId")]
    public string OperationId { get; set; } = null!;

    [JsonPropertyName("parameters")]
    public Parameters Parameters { get; set; } = new();

    [JsonPropertyName("host")]
    public string Host { get; set; } = null!;
}
