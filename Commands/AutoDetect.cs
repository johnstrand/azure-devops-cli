namespace Ado.Commands;

internal static class AutoDetect
{
    public static void Execute()
    {
        Console.WriteLine("Performing auto-detect");

        if (!Git.TryFindRemote(out var remote))
        {
            throw new("Unable to resolve remote");
        }

        Console.WriteLine($"Remote: {string.Join(", ", remote)}");
        if (!Git.FindAzureDevopsInfo(out var o, out var p, out var r))
        {
            throw new("Unable to resolve organization, project, and repository");
        }

        Console.WriteLine($"Organization: {o}");
        Console.WriteLine($"Project: {p}");
        Console.WriteLine($"Repository: {r}");
    }
}