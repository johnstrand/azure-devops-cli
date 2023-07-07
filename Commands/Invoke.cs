using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json.Nodes;

namespace Ado.Commands;

public static class Invoke
{
    /// <summary>
    /// Attempts to resolve an API call based on the given arguments and executes it
    /// </summary>
    public static async Task Execute(ArgReader args)
    {
        // Read name of the command to match
        var command = args.GetCommand("Command name is missing");

        // Ensure that there are no other commands specified
        args.EnsureCommandBufferEmpty();

        // Find the personal access token for the given organization
        var pat = Login.Retrieve(args.PeekParameter(WellKnownParameters.Organization));

        // Find the area (if specified)
        args.TryGetParameter(WellKnownParameters.Area, out var area);

        Log.Verbose($"Attempting find command '{command}' in {area ?? "any area"}");

        // Find the command based on name and area
        var matchedCommands = OperationsLookup.GetOperationExact(command, area).ToList();
        Log.Verbose($"Lookup matched {matchedCommands.Count} command(s)");

        // Nothing matched? Exit with error
        if (matchedCommands.Count == 0)
        {
            Help.Execute(args);
            throw new("Unable to find a command matching given input");
        }

        // More than one matched and the user specified a verb? Try to disambiguate
        if (matchedCommands.Count > 1 && args.TryGetParameter(WellKnownParameters.Verb, out var verb))
        {
            Log.Verbose("More than one command found, attempting to use HTTP verb to disambiguate");
            matchedCommands = matchedCommands.Where(c => verb == null || string.Equals(c.operation.Verb, verb)).ToList();
        }

        // Still more than one matched? Exit with error and list all matches
        if (matchedCommands.Count > 1)
        {
            var error = "Given input matched more than one command, try adding verb and/or area parameters to disambiguate\n";

            foreach (var (commandArea, operation) in matchedCommands)
            {
                error += $"{commandArea} - {operation.Name} ({operation.Verb.ToUpper()})\n";
            }

            throw new(error);
        }

        // Deconstruct the matched command
        var (_area, resolvedCommand) = matchedCommands.Single();

        // If the user did not specify an area, the variable will be null at this point and we'll need
        // the value later on, so we'll set it to the resolved command's area
        area = _area;

        var route = resolvedCommand.UrlTemplate;

        Log.Verbose($"Command resolved to route {route} in area {area}");

        // Create a query builder and add the API version
        var qs = new QueryBuilder().AddQuery("api-version", resolvedCommand.ApiVersion);

        // Loop through the command's query parameters
        foreach (var parameter in resolvedCommand.Parameters.Query)
        {
            // If the user did not specify a value for the parameter, check if it's optional
            // and throw if it's not
            if (!args.TryGetParameter(parameter.Name, out var value))
            {
                if (parameter.Required)
                {
                    throw new($"Required query parameter '{parameter.Name}' missing");
                }

                continue;
            }

            qs.AddQuery(parameter.Name, value);
        }

        // Loop through the command's path (or route) parameters
        foreach (var parameter in resolvedCommand.Parameters.Path)
        {
            // Route parameters are always required, so throw if the user did not specify a value
            if (!args.TryGetParameter(parameter.Name, out var value))
            {
                if (parameter.Name == "repositoryId")
                {
                    value = args.GetParameter(WellKnownParameters.Repository);
                }
                else
                {
                    throw new($"Required path parameter '{parameter.Name}' missing");
                }
            }

            route = route.Replace($"{{{parameter.Name}}}", value);
        }

        // Produce the final URL for the request
        var uri = $"https://{resolvedCommand.Host}{route}{qs.ToString(true)}";

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new("Basic", pat);
        client.DefaultRequestHeaders.Accept.Add(new("application/json"));

        // Loop through the command's header parameters
        foreach (var parameter in resolvedCommand.Parameters.Header)
        {
            if (!args.TryGetParameter(parameter.Name, out var value))
            {
                if (parameter.Required)
                {
                    throw new($"Required header parameter '{parameter.Name}' missing");
                }

                continue;
            }

            client.DefaultRequestHeaders.Add(parameter.Name, value);
        }

        var body = "";

        // Body required but not provided? Throw an error and exit
        if (resolvedCommand.Parameters.Body != null && !TryReadBody(args, out body))
        {
            throw new("Method expects a body, but none was provided via STDIN");
        }

        static bool TrueFlag(string value)
        {
            return string.IsNullOrEmpty(value) || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }

        // Check which boolean flags were specified
        var whatIf = args.TryGetFlag("what-if", out var _whatif) && TrueFlag(_whatif);
        var silent = args.TryGetFlag("silent", "s", out var _silent) && TrueFlag(_silent);
        var pretty = args.TryGetFlag("pretty", "p", out var _pretty) && TrueFlag(_pretty);

        // Check if the user specified a query to extract from the response
        args.TryGetFlag("query", "q", out var query);

        // Ensure that all arguments, commands, and flags have been read
        args.EnsureAllRead();

        // If the user specified the 'what-if' flag, print the request and exit
        if (whatIf)
        {
            Console.WriteLine($"{resolvedCommand.Verb.ToUpper()} {uri}");
            foreach (var header in client.DefaultRequestHeaders)
            {
                Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }

            if (resolvedCommand.Parameters.Body != null)
            {
                Console.WriteLine();
                Console.WriteLine(body);
            }

            return;
        }

        // TODO: Check that body is valid JSON

        // Some commands, despite being POST, PUT, or PATCH, do not require a body, so we'll return null if that's the case
        StringContent? CreateContent(string body) => resolvedCommand.Parameters.Body != null ? new StringContent(body, Encoding.UTF8, "application/json") : null;

        using var response = await (resolvedCommand.Verb.ToLower() switch
        {
            "get" => client.GetAsync(uri),
            "delete" => client.DeleteAsync(uri),
            "post" => client.PostAsync(uri, CreateContent(body)),
            "patch" => client.PatchAsync(uri, CreateContent(body)),
            "put" => client.PutAsync(uri, CreateContent(body)),
            _ => throw new($"No handler for {resolvedCommand.Verb} available")
        });

        // At this point, we have a response, but we don't know if it's an error or not
        // nor do we know if the response body is JSON or not, so we'll read it as a string
        var content = await response.Content.ReadAsStringAsync();

        // Error code? Just throw the response body as an exception
        if (!response.IsSuccessStatusCode)
        {
            throw new(content);
        }

        // Status code that we don't know how to handle? Throw an exception and alert the user of a possible cause
        if ((int)response.StatusCode >= 300)
        {
            throw new($"Status '{response.StatusCode}' was received, this may indicate that your PAT is not valid");
        }

        // If the user specified the 'silent' flag, don't print anything and exit
        if (silent)
        {
            return;
        }

        // TODO: Rework this segment
        if (TryParse(content, out var parsed))
        {
            var json = parsed.ToJsonString(new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = pretty,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            if (query != null)
            {
                Console.WriteLine(new JUST.JsonTransformer().Transform($"\"#valueof({query})\"", json));
            }
            else
            {
                Console.WriteLine(json);
            }
        }
        else
        {
            Console.WriteLine(content);
        }
    }

    /// <summary>
    /// Try reading the body from 'body' parameter in the given <see cref="ArgReader"/> or, if that fails, from STDIN
    /// </summary>
    private static bool TryReadBody(ArgReader reader, [NotNullWhen(true)] out string? body)
    {
        return reader.TryGetParameter(WellKnownParameters.Body, out body) || Stdin.TryRead(out body);
    }

    /// <summary>
    /// Try parsing the given JSON string into a <see cref="JsonNode"/>, returning true if successful
    /// </summary>
    private static bool TryParse(string json, [NotNullWhen(true)] out JsonNode? node)
    {
        node = null;
        try
        {
            node = JsonNode.Parse(json)!;
            return true;
        }
        catch
        {
        }

        return false;
    }
}
