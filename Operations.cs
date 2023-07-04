using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ado;

internal class OperationsLookup
{
    private static readonly Dictionary<string, Dictionary<string, List<Operation>>> operations;
    static OperationsLookup()
    {
        using var stream = typeof(OperationsLookup).Assembly.GetManifestResourceStream("Ado.operations.json");
        operations = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<Operation>>>>(stream!, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
        })!;
    }

    public static bool HasArea(string area)
    {
        return operations.ContainsKey(area);
    }

    public static IEnumerable<string> ListAreas()
    {
        return operations.Keys.Order();
    }

    public static IEnumerable<(string area, Operation operation)> ListOperations()
    {
        return operations.SelectMany(o => ListOperationsInternal(o.Key, o.Value));
    }

    public static IEnumerable<(string area, Operation operation)> ListOperations(string area)
    {
        return ListOperationsInternal(area, operations[area]);
    }

    private static IEnumerable<(string area, Operation operation)> ListOperationsInternal(string area, Dictionary<string, List<Operation>> source)
    {
        foreach (var item in source)
        {
            foreach (var operation in item.Value)
            {
                operation.Name = item.Key;

                yield return (area, operation);
            }
        }
    }

    public static IEnumerable<(string area, Operation operation)> FindOperation(string name, string? area = null)
    {
        foreach (var operationArea in operations)
        {
            if (area != null && operationArea.Key != area)
            {
                continue;
            }

            foreach (var operation in ListOperationsInternal(operationArea.Key, operationArea.Value))
            {
                if (operation.operation.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                {
                    yield return operation;
                }
            }
        }
    }

    public static IEnumerable<(string area, Operation operation)> GetOperationExact(string name, string? area = null)
    {
        foreach (var operationArea in operations)
        {
            if (area != null && operationArea.Key != area)
            {
                continue;
            }

            foreach (var operation in ListOperationsInternal(operationArea.Key, operationArea.Value))
            {
                if (string.Equals(operation.operation.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    yield return operation;
                }
            }
        }
    }
}

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

public class Parameter
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("description")]
    public string Description { get; set; } = null!;

    [JsonPropertyName("required")]
    public bool Required { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("format")]
    public string? Format { get; set; }

    [JsonPropertyName("enum")]
    public EnumData? EnumData { get; set; }
}

public class EnumData
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("values")]
    public List<EnumValue> Values { get; set; } = new();
}

public class EnumValue
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = null!;

    [JsonPropertyName("description")]
    public string Description { get; set; } = null!;
}