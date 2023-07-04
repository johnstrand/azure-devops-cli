using System.Diagnostics.CodeAnalysis;

namespace Ado;

public static class Stdin
{
    public static string Read()
    {
        if (!TryRead(out var data))
        {
            throw new("Expected data from stdin, but none was available");
        }

        return data;
    }

    public static bool TryRead([NotNullWhen(true)] out string? data)
    {
        data = null;
        if (!Console.IsInputRedirected)
        {
            return false;
        }

        data = Console.In.ReadToEnd();
        return true;
    }
}