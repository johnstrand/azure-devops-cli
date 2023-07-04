namespace Ado.Commands;

public static class SearchCommand
{
    public static void Execute(ArgReader args)
    {
        var match = args.GetCommand("Search term is missing");
        args.TryGetParameter(WellKnownParameters.Area, out var area);
        args.EnsureAllRead();

        foreach (var group in OperationsLookup.FindOperation(match, area).GroupBy(o => o.area, o => o.operation))
        {
            Console.WriteLine(group.Key);
            foreach (var operation in group)
            {
                Console.WriteLine($"\t{operation.Name} - {operation.Verb} {operation.UrlTemplate}");
            }
        }
    }
}