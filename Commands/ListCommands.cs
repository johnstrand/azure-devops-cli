using Ado.Arguments;
using Ado.Serialization;

namespace Ado.Commands;

internal static class ListCommands
{
    public static void Execute(ArgReader args)
    {
        args.TryGetNextCommand(out var area);

        args.EnsureAllRead();

        foreach (var group in (area == null ? OperationsLookup.ListOperations() : OperationsLookup.ListOperations(area)).GroupBy(o => o.area, o => o.operation))
        {
            Console.WriteLine(group.Key);
            foreach (var operation in group.OrderBy(o => o.Name))
            {
                Console.WriteLine($"\t{operation.Name}");
            }
        }
    }
}