using LibGit2Sharp;

using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Ado;

/// <summary>
/// Helper class for interacting with git
/// </summary>
internal static class Git
{
    // TODO: Make configurable

    /// <summary>
    /// Patterns to match against to find the remote
    /// </summary>
    internal static readonly string[] possibleRemotesPatterns = new[]
    {
        @"^(?<org>.+?)@.*\.visualstudio.com:v3\/\1\/(?<project>.+?)\/(?<repo>.+?)$",
        @"^git@.*\.dev\.azure\.com:v3\/(?<org>.+?)\/(?<project>.+?)\/(?<repo>.+?)$",
        @"^https:\/\/(?<org>.+?)[\.@].*?(visualstudio\.com|azure\.com\/\1)\/(?<project>.+?)\/_git\/(?<repo>.+?)$",
        @"^https:\/\/(?<org>.+?)[\.@].*?(visualstudio\.com|azure\.com\/\k<org>)\/(?<project>.+?)\/_git\/(?<repo>.+?)$"
    };

    /// <summary>
    /// Attempts to resolve the Azure DevOps configuration from the current git repository
    /// </summary>
    public static bool FindAzureDevopsInfo([NotNullWhen(true)] out string? org, [NotNullWhen(true)] out string? project, [NotNullWhen(true)] out string? repo)
    {
        org = project = repo = null;
        Log.Verbose("Attempting to resolve configuration from repository");

        if (!TryFindRemote(out var remotes))
        {
            throw new("Current directory is not a git repo or is missing a remote");
        }

        foreach (var remote in remotes)
        {
            foreach (var pattern in possibleRemotesPatterns)
            {
                var match = Regex.Match(remote, pattern);
                if (!match.Success)
                {
                    Log.Verbose($"{pattern} was not a match");
                    continue;
                }

                org = match.Groups["org"].Value;
                project = match.Groups["project"].Value;
                repo = match.Groups["repo"].Value;

                Log.Verbose($"Resolved configuration: Organization: {org}. Project: {project}. Repository: {repo}");

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Finds the remote for the current git repository
    /// </summary>
    public static bool TryFindRemote([NotNullWhen(true)] out string[] remotes)
    {
        remotes = Array.Empty<string>();
        var path = Environment.CurrentDirectory;

        while (path != null)
        {
            Log.Verbose($"Attempting to find remote in {path}");

            if (Repository.IsValid(path))
            {
                break;
            }

            Log.Verbose("LibGit2Sharp failed to find a valid repository");
            path = Path.GetDirectoryName(path);

            break;
        }

        if (path == null)
        {
            return false;
        }

        var repo = new Repository(path);

        if (!repo.Network.Remotes.Any())
        {
            Log.Verbose("LibGit2Sharp failed to find any remotes");
            return false;
        }

        remotes = repo.Network.Remotes.Select(r => r.Url).ToArray();
        Log.Verbose($"Found {remotes.Length} remote(s)");

        return true;
    }
}
