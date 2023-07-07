namespace Ado.Arguments;

/// <summary>
/// Represents a single argument
/// </summary>
/// <param name="Name">Name of the argument</param>
/// <param name="Value">Value of the argument, will be an empty string in case of a command</param>
/// <param name="Type">Argument type</param>
internal record Argument(string Name, string Value, ArgumentType Type)
{
    /// <summary>
    /// True if this argument is a command
    /// </summary>
    public bool IsCommand => Type == ArgumentType.Command;

    /// <summary>
    /// True if this argument is a parameter
    /// </summary>
    public bool IsParameter => Type == ArgumentType.Parameter;

    /// <summary>
    /// True if this argument is a flag
    /// </summary>
    public bool IsFlag => Type == ArgumentType.Flag;
}
