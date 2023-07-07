using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ado.Serialization;

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
