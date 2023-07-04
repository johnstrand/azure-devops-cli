namespace Ado.Commands;

public static class ListCommands
{
    public static void Execute(ArgReader args)
    {
        args.TryGetNextCommand(out var area);

        args.EnsureAllRead();

        foreach (var group in (area == null ? OperationsLookup.ListOperations() : OperationsLookup.ListOperations(area)).GroupBy(o => o.area, o => o.operation))
        {
            Console.WriteLine(group.Key);
            foreach (var operation in group)
            {
                Console.WriteLine($"\t{operation.Name}");
            }
        }
    }
}