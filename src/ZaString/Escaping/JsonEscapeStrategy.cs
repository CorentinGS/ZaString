namespace ZaString.Escaping;

/// <summary>
///     Provides span-to-span JSON escaping without intermediate string allocation.
/// </summary>
public static class JsonEscapeStrategy
{
    public static int GetEscapedLength(ReadOnlySpan<char> value)
    {
        var extra = 0;
        foreach (var c in value)
        {
            switch (c)
            {
                case '"':
                case '\\':
                case '\b':
                case '\f':
                case '\n':
                case '\r':
                case '\t':
                    extra += 1;
                    break;
                case '\u2028':
                case '\u2029':
                    extra += 5;
                    break;

                default:
                    if (c < ' ')
                    {
                        extra += 5;
                    }

                    break;
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
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            switch (c)
            {
                case '"':
                    destination[w++] = '\\';
                    destination[w++] = '"';
                    break;
                case '\\':
                    destination[w++] = '\\';
                    destination[w++] = '\\';
                    break;
                case '\b':
                    destination[w++] = '\\';
                    destination[w++] = 'b';
                    break;
                case '\f':
                    destination[w++] = '\\';
                    destination[w++] = 'f';
                    break;
                case '\n':
                    destination[w++] = '\\';
                    destination[w++] = 'n';
                    break;
                case '\r':
                    destination[w++] = '\\';
                    destination[w++] = 'r';
                    break;
                case '\t':
                    destination[w++] = '\\';
                    destination[w++] = 't';
                    break;
                case '\u2028':
                    destination[w++] = '\\';
                    destination[w++] = 'u';
                    destination[w++] = '2';
                    destination[w++] = '0';
                    destination[w++] = '2';
                    destination[w++] = '8';
                    break;
                case '\u2029':
                    destination[w++] = '\\';
                    destination[w++] = 'u';
                    destination[w++] = '2';
                    destination[w++] = '0';
                    destination[w++] = '2';
                    destination[w++] = '9';
                    break;

                default:
                    if (c < ' ')
                    {
                        destination[w++] = '\\';
                        destination[w++] = 'u';
                        destination[w++] = '0';
                        destination[w++] = '0';
                        WriteHexByte((byte)c, destination.Slice(w, 2));
                        w += 2;
                    }
                    else
                    {
                        destination[w++] = c;
                    }

                    break;
            }
        }

        return w;
    }

    private static void WriteHexByte(byte value, Span<char> destination)
    {
        const string hex = "0123456789ABCDEF";
        destination[0] = hex[value >> 4 & 0xF];
        destination[1] = hex[value & 0xF];
    }
}
