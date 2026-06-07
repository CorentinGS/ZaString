namespace ZaString.Escaping;

/// <summary>
///     Provides span-to-span CSV field escaping without intermediate string allocation.
/// </summary>
public static class CsvEscapeStrategy
{
    public static int GetEscapedLength(ReadOnlySpan<char> value)
    {
        if (!NeedsQuoting(value))
        {
            return value.Length;
        }

        var quoteCount = 0;
        foreach (var c in value)
        {
            if (c == '"')
            {
                quoteCount++;
            }
        }

        return value.Length + quoteCount + 2;
    }

    public static bool TryEscape(ReadOnlySpan<char> value, Span<char> destination, out int written)
    {
        var required = GetEscapedLength(value);
        if (required > destination.Length)
        {
            written = 0;
            return false;
        }

        written = Escape(value, destination);
        return true;
    }

    private static int Escape(ReadOnlySpan<char> value, Span<char> destination)
    {
        if (!NeedsQuoting(value))
        {
            value.CopyTo(destination);
            return value.Length;
        }

        var w = 0;
        destination[w++] = '"';
        foreach (var c in value)
        {
            destination[w++] = c;
            if (c == '"')
            {
                destination[w++] = '"';
            }
        }

        destination[w++] = '"';
        return w;
    }

    private static bool NeedsQuoting(ReadOnlySpan<char> value)
    {
        if (value.Length == 0) return true;
        if (char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[^1])) return true;

        foreach (var c in value)
        {
            if (c is ',' or '"' or '\n' or '\r') return true;
        }

        return false;
    }
}
