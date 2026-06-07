namespace ZaString.Escaping;

/// <summary>
///     Provides span-to-span URL percent encoding without intermediate string allocation.
/// </summary>
public static class UrlEscapeStrategy
{
    public static int GetEscapedLength(ReadOnlySpan<char> value)
    {
        var length = 0;
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (c <= 0x7F)
            {
                length += IsUnreservedAscii(c) ? 1 : 3;
            }
            else if (char.IsHighSurrogate(c) && i + 1 < value.Length && char.IsLowSurrogate(value[i + 1]))
            {
                length += 4 * 3;
                i++;
            }
            else
            {
                length += char.IsSurrogate(c) ? 9 : c <= 0x7FF ? 2 * 3 : 3 * 3;
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

    internal static bool IsUnreservedAscii(char c)
    {
        return c is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '-' or '_' or '.' or '~';
    }

    internal static void WriteHexByte(byte value, Span<char> destination)
    {
        const string hex = "0123456789ABCDEF";
        destination[0] = hex[value >> 4 & 0xF];
        destination[1] = hex[value & 0xF];
    }

    internal static int PercentEncodeUtf8FromCodePoint(int codePoint, Span<char> destination)
    {
        switch (codePoint)
        {
            case <= 0x7F:
                destination[0] = '%';
                WriteHexByte((byte)codePoint, destination.Slice(1, 2));
                return 3;

            case <= 0x7FF:
                {
                    var b1 = (byte)(0b1100_0000 | codePoint >> 6);
                    var b2 = (byte)(0b1000_0000 | codePoint & 0b0011_1111);
                    destination[0] = '%';
                    WriteHexByte(b1, destination.Slice(1, 2));
                    destination[3] = '%';
                    WriteHexByte(b2, destination.Slice(4, 2));
                    return 6;
                }

            case <= 0xFFFF:
                {
                    var b1 = (byte)(0b1110_0000 | codePoint >> 12);
                    var b2 = (byte)(0b1000_0000 | codePoint >> 6 & 0b0011_1111);
                    var b3 = (byte)(0b1000_0000 | codePoint & 0b0011_1111);
                    destination[0] = '%';
                    WriteHexByte(b1, destination.Slice(1, 2));
                    destination[3] = '%';
                    WriteHexByte(b2, destination.Slice(4, 2));
                    destination[6] = '%';
                    WriteHexByte(b3, destination.Slice(7, 2));
                    return 9;
                }

            default:
                {
                    var b1 = (byte)(0b1111_0000 | codePoint >> 18);
                    var b2 = (byte)(0b1000_0000 | codePoint >> 12 & 0b0011_1111);
                    var b3 = (byte)(0b1000_0000 | codePoint >> 6 & 0b0011_1111);
                    var b4 = (byte)(0b1000_0000 | codePoint & 0b0011_1111);
                    destination[0] = '%';
                    WriteHexByte(b1, destination.Slice(1, 2));
                    destination[3] = '%';
                    WriteHexByte(b2, destination.Slice(4, 2));
                    destination[6] = '%';
                    WriteHexByte(b3, destination.Slice(7, 2));
                    destination[9] = '%';
                    WriteHexByte(b4, destination.Slice(10, 2));
                    return 12;
                }
        }
    }

    private static int Escape(ReadOnlySpan<char> value, Span<char> destination)
    {
        var w = 0;
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (c <= 0x7F)
            {
                if (IsUnreservedAscii(c))
                {
                    destination[w++] = c;
                }
                else
                {
                    destination[w++] = '%';
                    WriteHexByte((byte)c, destination.Slice(w, 2));
                    w += 2;
                }
            }
            else if (char.IsHighSurrogate(c) && i + 1 < value.Length && char.IsLowSurrogate(value[i + 1]))
            {
                var low = value[++i];
                var codePoint = 0x10000 + (c - 0xD800 << 10 | low - 0xDC00);
                w += PercentEncodeUtf8FromCodePoint(codePoint, destination[w..]);
            }
            else
            {
                var codePoint = (int)c;
                w += char.IsSurrogate(c)
                    ? WriteReplacementChar(destination[w..])
                    : PercentEncodeUtf8FromCodePoint(codePoint, destination[w..]);
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
