namespace ZaString.Escaping;

/// <summary>
///     Provides span-to-span form URL encoding without intermediate string allocation.
/// </summary>
public static class FormUrlEscapeStrategy
{
    public static int GetEscapedLength(ReadOnlySpan<char> value)
    {
        var length = 0;
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (c == ' ')
            {
                length += 1;
            }
            else if (c <= 0x7F)
            {
                length += UrlEscapeStrategy.IsUnreservedAscii(c) ? 1 : 3;
            }
            else if (char.IsHighSurrogate(c) && i + 1 < value.Length && char.IsLowSurrogate(value[i + 1]))
            {
                length += 4 * 3;
                i++;
            }
            else
            {
                length += 9;
            }
        }

        return length;
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
            if (c == ' ')
            {
                destination[w++] = '+';
            }
            else if (c <= 0x7F)
            {
                if (UrlEscapeStrategy.IsUnreservedAscii(c))
                {
                    destination[w++] = c;
                }
                else
                {
                    destination[w++] = '%';
                    UrlEscapeStrategy.WriteHexByte((byte)c, destination.Slice(w, 2));
                    w += 2;
                }
            }
            else if (char.IsHighSurrogate(c) && i + 1 < value.Length && char.IsLowSurrogate(value[i + 1]))
            {
                var low = value[++i];
                var codePoint = 0x10000 + (c - 0xD800 << 10 | low - 0xDC00);
                w += UrlEscapeStrategy.PercentEncodeUtf8FromCodePoint(codePoint, destination[w..]);
            }
            else
            {
                w += WriteReplacementChar(destination[w..]);
            }
        }

        return w;
    }

    private static int WriteReplacementChar(Span<char> destination)
    {
        destination[0] = '%';
        destination[1] = 'E';
        destination[2] = 'F';
        destination[3] = '%';
        destination[4] = 'B';
        destination[5] = 'F';
        destination[6] = '%';
        destination[7] = 'B';
        destination[8] = 'D';
        return 9;
    }
}
