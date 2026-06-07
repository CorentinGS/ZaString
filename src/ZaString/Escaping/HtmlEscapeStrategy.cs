namespace ZaString.Escaping;

/// <summary>
///     Provides span-to-span HTML escaping without intermediate string allocation.
/// </summary>
public static class HtmlEscapeStrategy
{
    public static int GetEscapedLength(ReadOnlySpan<char> value)
    {
        var extra = 0;
        foreach (var c in value)
        {
            switch (c)
            {
                case '&': extra += 4; break;
                case '<':
                case '>': extra += 3; break;
                case '"': extra += 5; break;
                case '\'': extra += 4; break;
            }
        }

        return value.Length + extra;
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
        var w = 0;
        foreach (var c in value)
        {
            switch (c)
            {
                case '&':
                    destination[w++] = '&';
                    destination[w++] = 'a';
                    destination[w++] = 'm';
                    destination[w++] = 'p';
                    destination[w++] = ';';
                    break;
                case '<':
                    destination[w++] = '&';
                    destination[w++] = 'l';
                    destination[w++] = 't';
                    destination[w++] = ';';
                    break;
                case '>':
                    destination[w++] = '&';
                    destination[w++] = 'g';
                    destination[w++] = 't';
                    destination[w++] = ';';
                    break;
                case '"':
                    destination[w++] = '&';
                    destination[w++] = 'q';
                    destination[w++] = 'u';
                    destination[w++] = 'o';
                    destination[w++] = 't';
                    destination[w++] = ';';
                    break;
                case '\'':
                    destination[w++] = '&';
                    destination[w++] = '#';
                    destination[w++] = '3';
                    destination[w++] = '9';
                    destination[w++] = ';';
                    break;
                default:
                    destination[w++] = c;
                    break;
            }
        }

        return w;
    }
}
