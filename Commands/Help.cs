namespace Ado.Commands;

public static class Help
{
    public static void Execute(ArgReader _)
    {
        Console.WriteLine(@"Basic help:
Run the application, run ado <command> <parameters> flags
* In general, any command given will be used to find and call an Azure DevOps API method. (Exceptions below)
* Parameters are specified as name = value
* Flags are specified as --flag
* Any subcommands are specified as a single word

The following special commands are available:
* list-areas: Lists available areas
* list-commands: Lists all commands
* list-commands <area>: Lists all commands within an area
* search-command <pattern> area=(area): Search for a command, optionally limited to specific area
* show-command <command>: Show details about all commands with the given name
* auto-detect: Run auto-detect and present the results
* login <pat>: Set a PAT for the given organization and project

The following flags are available:
-v or --verbose: Enable verbose logging
-p or --pretty: Pretty-print the output
-s or --silent: Suppress any output
--what-if: Only applicable to API calls, instead of performing the call, the URL and body (if existing) will be printed
--query=<query>: A jq-style query to filter the result

The following parameters are available for all commands:
area=<area>: Indicate which area to search, only required if there's more than one command with the same name
method=<HTTP-verb>: Indicate which HTTP verb is expected, only required if there's more than one command with the same name
body=<body>: Data to be sent as request body (may also be piped through STDIN

The following parameters are optional, and the tool will attempt to resolve them from the current repository's remote
organization=<organization>: Azure DevOps organization to use
project=<project>: Azure DevOps project to use
repository=<repository>: Azure DevOps repository
");
    }
}