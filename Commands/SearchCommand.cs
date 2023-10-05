using Ado.Arguments;
using Ado.Serialization;

namespace Ado.Commands;

internal static class SearchCommand
{
    public static void Execute(ArgReader args)
    {
        var match = args.GetCommand("Search term is missing");
        args.TryGetParameter(WellKnownParameters.Area, out var area);
        args.EnsureAllRead();

        foreach (var group in OperationsLookup.FindOperation(match, area).GroupBy(o => o.area, o => o.operation))
        {
            Console.WriteLine(group.Key);
            foreach (var operation in group.OrderBy(op => op.Name))
            {
                Console.WriteLine($"\t{operation.Name} - {operation.Verb} {operation.UrlTemplate}");
            }
        }
    }
}