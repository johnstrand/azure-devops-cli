using Ado.Commands;

using System.Diagnostics.CodeAnalysis;

namespace Ado.Arguments;

/// <summary>
/// Class to help with parsing command line arguments
/// </summary>
internal sealed class ArgReader
{
    /// <summary>
    /// Arguments that have not yet been read
    /// </summary>
    private readonly Queue<string> args;

    /// <summary>
    /// Commands that have been read
    /// </summary>
    private readonly Queue<string> commands = new();

    /// <summary>
    /// Parameters that have been read
    /// </summary>
    private readonly Dictionary<string, string> parameters = new();

    /// <summary>
    /// Flags that have been read
    /// </summary>
    private readonly Dictionary<string, string> flags = new();

    /// <summary>
    /// From a given set of arguments, read all commands, parameters, and flags
    /// </summary>
    private ArgReader(string[] args)
    {
        this.args = new(args);
    }

    /* The following methods are used to ensure that no unexpected commands, parameters, or flags were specified */

    /// <summary>
    /// Ensure that all commands, parameters, and flags have been read. This makes sure that no invalid arguments were specified
    /// </summary>
    public void EnsureAllRead()
    {
        // Some command won't use organization or project, so we need to make sure that they aren't flagged as errors,
        // as they'll be resolved no matter what
        TryGetParameter(WellKnownParameters.Organization, out var _);
        TryGetParameter(WellKnownParameters.Project, out var _);

        EnsureCommandBufferEmpty();
        EnsureParameterListEmpty();
        EnsureFlagSetEmpty();
    }

    /// <summary>
    /// Ensure that the parameter list is empty. This makes sure that no invalid parameters were specified
    /// </summary>
    public void EnsureParameterListEmpty()
    {
        if (parameters.Count == 0)
        {
            return;
        }

        throw new($"Unxpected parameters: {string.Join(", ", parameters.Keys)}");
    }

    /// <summary>
    /// Ensure that the command buffer is empty. This makes sure that no invalid commands were specified
    /// </summary>
    public void EnsureCommandBufferEmpty()
    {
        if (commands.Count == 0)
        {
            return;
        }

        throw new($"Unexpected commands: {string.Join(", ", commands)}");
    }

    /// <summary>
    /// Ensure that the flag set is empty. This makes sure that no invalid flags were specified
    /// </summary>
    public void EnsureFlagSetEmpty()
    {
        if (flags.Count == 0)
        {
            return;
        }

        throw new($"Unexpected flags: {string.Join(",", flags)}");
    }

    /* The following commands are all related to reading commands, parameters, or flags, or check if they exist */

    /// <summary>
    /// Try to read a command from the argument list
    /// </summary>
    /// <param name="command">Contains the next command if one is available, or null if not</param>
    public bool TryGetNextCommand([NotNullWhen(true)] out string? command)
    {
        return commands.TryDequeue(out command);
    }

    /// <summary>
    /// Read a non-optional command from the list, raising an error if one is not available
    /// </summary>
    /// <param name="errorIfMissing">The error text that will be thrown</param>
    public string GetCommand(string errorIfMissing)
    {
        if (!TryGetNextCommand(out var command))
        {
            throw new(errorIfMissing);
        }

        return command;
    }

    /// <summary>
    /// Compare the next command to the given command, and if there is another command and they match, remove it from the list and return true
    /// </summary>
    public bool TryMatchCommand(string command)
    {
        if (!HasCommands() || commands.Peek() != command)
        {
            return false;
        }

        commands.Dequeue();

        return true;
    }

    /// <summary>
    /// Check if there are any more commands
    /// </summary>
    public bool HasCommands()
    {
        return commands.Count > 0;
    }

    /// <summary>
    /// Check if a specific parameter has been specified
    /// </summary>
    public bool HasParameter(string name)
    {
        return parameters.ContainsKey(name);
    }

    /// <summary>
    /// Set a parameter to a given value
    /// </summary>
    public void SetParameter(string name, string value)
    {
        parameters[name] = value;
    }

    /// <summary>
    /// Attempt to read a parameter without removing it from the list, throwing an error if it is not available
    /// </summary>
    public string PeekParameter(string name)
    {
        if (!HasParameter(name))
        {
            throw new($"Missing required parameter '{name}'");
        }

        return parameters[name];
    }

    /// <summary>
    /// Get a  specificparameter from the list, throwing an error if it is not available
    /// </summary>
    public string GetParameter(string name)
    {
        if (!TryGetParameter(name, out var value))
        {
            throw new($"Missing required parameter '{name}'");
        }

        return value;
    }

    /// <summary>
    /// Get a specific parameter from the list, if it is available
    /// </summary>
    public bool TryGetParameter(string name, [NotNullWhen(true)] out string? parameter)
    {
        return parameters.Remove(name, out parameter);
    }

    /// <summary>
    /// Get a specific flag from the list, if it is available
    /// </summary>
    public bool TryGetFlag(string name, [NotNullWhen(true)] out string? value)
    {
        return TryGetFlag(new[] { name }, out value);
    }

    /// <summary>
    /// Get a specific flag (based on the full name or the short version) from the list, if it is available
    /// </summary>
    public bool TryGetFlag(string name, string altName, [NotNullWhen(true)] out string? value)
    {
        return TryGetFlag(new[] { name, altName }, out value);
    }

    /// <summary>
    /// Internal helper method for fetching a flag from the list, if it is available
    /// </summary>
    private bool TryGetFlag(string[] names, [NotNullWhen(true)] out string? value)
    {
        value = null;
        foreach (var name in names)
        {
            if (flags.Remove(name, out value))
            {
                return true;
            }
        }

        return false;
    }

    /* The following commands are involved in parsing the given command line */

    /// <summary>
    /// Main entry point for parsing command line arguments
    /// </summary>
    public static ArgReader Parse(string[] args)
    {
        var reader = new ArgReader(args);
        while (reader.HasMore())
        {
            var arg = reader.ReadNextArgument();
            if (arg.IsParameter)
            {
                reader.SetParameter(arg.Name, arg.Value);
            }
            else if (arg.IsFlag)
            {
                reader.flags[arg.Name] = arg.Value ?? "";
            }
            else
            {
                reader.commands.Enqueue(arg.Name);
            }
        }

        return reader;
    }

    /// <summary>
    /// Returns true if there are more arguments to be read
    /// </summary>
    private bool HasMore()
    {
        return args.Count > 0;
    }

    /// <summary>
    /// Helper method to ensure that the argument list is not empty
    /// </summary>
    private ArgReader RequireMore(string context)
    {
        if (HasMore())
        {
            return this;
        }

        throw new($"Unexpected end of argument list {context}");
    }

    /// <summary>
    /// If we think that we're reading a flag, ensure that name is valid
    /// </summary>
    private static string EnsureValidFlagName(string name)
    {
        if (name.StartsWith("--"))
        {
            if (name.Length == 2)
            {
                throw new("Encountered -- without a flag name");
            }

            return name[2..];
        }

        if (name.Length == 1)
        {
            throw new("Encountered - without a flag name");
        }

        return name[1..];
    }

    /// <summary>
    /// Parses the next command, flag, or parameter from the list
    /// </summary>
    private Argument ReadNextArgument()
    {
        // Should never be called if there are no more arguments
        var name = Read();
        ArgumentType? flagType = null;

        // If the name starts with -, then it's a flag
        if (name.StartsWith('-'))
        {
            name = EnsureValidFlagName(name);
            flagType = ArgumentType.Flag;
        }

        // We may have a flag or parameter in the form name=value
        if (name.Contains('='))
        {
            // At this point, if the name starts with =, then something has gone wrong
            if (name[0] == '=')
            {
                throw new($"Unexpected = prefix in argument {name}");
            }

            // Split the name into two parts, discarding any empty entries
            var pair = name.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);

            // If we actually have two parts, then we have name/value pair
            if (pair.Length == 2)
            {
                // Return the name/value pair as a parameter, unless we already know that it's a flag
                return new(pair[0], pair[1], flagType ?? ArgumentType.Parameter);
            }

            // If we only have one part, then the value is the next argument, so we'll read it and return
            return new(pair[0], Read($"value for parameter {pair[0]}"), flagType ?? ArgumentType.Parameter);
        }

        // If the argument list is empty, then we're done
        if (!HasMore())
        {
            return new(name, "", flagType ?? ArgumentType.Command);
        }

        // Let's see what the next argument is
        var next = Peek().Trim();

        // If the next argument is just =, then we need to discard that and read the value next
        if (next == "=")
        {
            // Consume the =
            Read();
            // We need to again check if we're at the end of the argument list, and treat an empty list as a empty value
            return new(name, HasMore() ? Read() : "", flagType ?? ArgumentType.Parameter);
        }

        // If the next argument starts with =, then we have the value
        if (next.StartsWith('='))
        {
            // So, let's consume the value
            var value = Read().Trim();
            // And we need to remember to discard the = prefix
            return new(name, value[1..], flagType ?? ArgumentType.Parameter);
        }

        // If we made it all the way here, we have a command
        return new(name, "", flagType ?? ArgumentType.Command);
    }

    /// <summary>
    /// Return the next argument without consuming it
    /// </summary>
    private string Peek()
    {
        return args.Peek();
    }

    /// <summary>
    /// Read the next argument from the list, throwing an exception based on <paramref name="context"/> if the list is empty
    /// </summary>
    private string Read(string context)
    {
        return RequireMore(context).Read();
    }

    /// <summary>
    /// Consume the next argument from the list
    /// </summary>
    private string Read()
    {
        return args.Dequeue();
    }
}
