namespace Ado;

/// <summary>
/// Helper class for verbose logging
/// </summary>
internal static class Log
{
    private static bool verbose;

    /// <summary>
    /// Writes debug messages to the console if verbose logging is enabled
    /// </summary>
    public static void Verbose(string text)
    {
        if (!verbose)
        {
            return;
        }

        Console.WriteLine($"[DBG]: {text}");
    }

    /// <summary>
    /// Enables verbose logging
    /// </summary>
    public static void EnableVerbose()
    {
        verbose = true;
    }
}