using System.Text;
using Ado.Arguments;

namespace Ado.Commands;

internal static class Login
{
    public static void Execute(ArgReader args)
    {
        var org = args.GetParameter(WellKnownParameters.Organization);

        if (!args.TryGetNextCommand(out var pat))
        {
            pat = Stdin.Read().Trim();
        }

        args.EnsureAllRead();

        var encodedPat = Convert.ToBase64String(Encoding.ASCII.GetBytes(":" + pat));

        var targetFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AdoCli", $"pat_{org}.txt");

        Log.Verbose($"Writing PAT to {targetFile}");

        Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);

        File.WriteAllText(targetFile, encodedPat);

        Console.WriteLine($"Personal access token for organization '{org}' stored");
    }

    public static string Retrieve(string organization)
    {
        var targetFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AdoCli", $"pat_{organization}.txt");

        Log.Verbose($"Reading PAT from {targetFile}");

        if (!Directory.Exists(Path.GetDirectoryName(targetFile)) || !File.Exists(targetFile))
        {
            throw new Exception($"No PAT registered for {organization}, run login command to store one");
        }

        var pat = File.ReadAllLines(targetFile).FirstOrDefault()?.Trim();

        if (string.IsNullOrEmpty(pat))
        {
            throw new Exception($"No PAT registered for {organization}, run login command to store one");
        }

        return pat;
    }
}