namespace Ado;

public class QueryBuilder
{
    private readonly List<KeyValuePair<string, object>> queryParameters = new();

    /// <summary>
    /// Adds a query parameter to the query string.
    /// </summary>
    public QueryBuilder AddQuery(string name, object? value)
    {
        if (value == null)
        {
            return this;
        }

        Log.Verbose($"Adding query parameter: {name}={value}");
        queryParameters.Add(new(name, value.ToString() ?? ""));
        return this;
    }

    /// <summary>
    /// Adds all the key/value pairs in <paramref name="query"/> to the query string.
    /// </summary>
    public QueryBuilder AddQuery(IDictionary<string, object?> query)
    {
        foreach (var item in query)
        {
            if (item.Value == null)
            {
                continue;
            }

            AddQuery(item.Key, item.Value);
        }
        return this;
    }

    /// <summary>
    /// Converts the instance to a query string, if <paramref name="includePrefix"/> is true, prepend a '?' to the string.
    /// </summary>
    public string ToString(bool includePrefix)
    {
        var qs = ToString();

        return (string.IsNullOrWhiteSpace(qs) || !includePrefix ? "" : "?") + qs;
    }

    /// <summary>
    /// Converst the instance to a query string, without a leading '?'.
    /// </summary>
    public override string ToString()
    {
        return string.Join("&", queryParameters.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value.ToString() ?? "")}"));
    }
}
