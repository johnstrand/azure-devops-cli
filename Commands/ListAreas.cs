namespace Ado.Commands;

public static class ListAreas
{
    public static void Execute(ArgReader args)
    {
        args.EnsureAllRead();
        foreach (var area in OperationsLookup.ListAreas())
        {
            Console.WriteLine(area);
        }
    }
}