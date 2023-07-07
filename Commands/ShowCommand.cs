using Ado.Arguments;
using Ado.Serialization;

namespace Ado.Commands;

internal static class ShowCommand
{
    private const char BottomLeftCornerThin = '\u2514';
    private const char BottomRightCornerThin = '\u2518';
    private const char CrossThin = '\u253C';
    private const char HorizontalLineThin = '\u2500';
    private const char TopLeftCornerThin = '\u250C';
    private const char TopRightCornerThin = '\u2510';
    private const char VerticalLineThin = '\u2502';

    public static void Execute(ArgReader args)
    {
        var name = args.GetCommand("Command name is missing");
        args.TryGetParameter(WellKnownParameters.Area, out var area);
        args.EnsureAllRead();

        foreach (var group in GetGroupedOperations(name, area))
        {
            foreach (var operation in group)
            {
                Console.WriteLine($@"Operation: {operation.Name}
Area: {group.Key}
Method: {operation.Verb}
URL template: {operation.UrlTemplate}
API version: {operation.ApiVersion}
Parameters:");
                Console.WriteLine($"  Body: {operation.Parameters.Body}");
                if (operation.Parameters.Query.Count > 0)
                {
                    var maxLength = operation.Parameters.Query.Max(q => q.Name.Length);
                    Console.WriteLine("  Query:");
                    foreach (var parameter in operation.Parameters.Query)
                    {
                        Console.WriteLine($"    {parameter.Name.PadRight(maxLength)} - {parameter.Type} {(parameter.Required ? "*" : "")}");
                    }
                }

                if (operation.Parameters.Path.Count > 0)
                {
                    Console.WriteLine("  Route:");
                    foreach (var parameter in operation.Parameters.Path)
                    {
                        Console.WriteLine($"    {parameter}");
                    }
                }
            }
        }
    }

    private static IEnumerable<IGrouping<string, Operation>> GetGroupedOperations(string name, string? area)
    {
        return OperationsLookup.GetOperationExact(name, area).GroupBy(o => o.area, o => o.operation);
    }
}