using System.Buffers;
using System.Buffers.Text;
using System.Globalization;
using System.Text;
using ZaString.Core;

namespace ZaString.Extensions;

/// <summary>
///     Provides extension methods for the <see cref="ZaUtf8SpanWriter" />.
/// </summary>
public static class ZaUtf8SpanWriterExtensions
{
    public static ref ZaUtf8SpanWriter Append(ref this ZaUtf8SpanWriter writer, ReadOnlySpan<byte> value)
    {
        if (value.Length > writer.RemainingSpan.Length)
        {
            ThrowOutOfRangeException();
        }

        value.CopyTo(writer.RemainingSpan);
        writer.Advance(value.Length);
        return ref writer;
    }

    public static ref ZaUtf8SpanWriter Append(ref this ZaUtf8SpanWriter writer, string? value)
    {
        if (value is not null)
        {
            var bytes = Encoding.UTF8.GetByteCount(value);
            if (bytes > writer.RemainingSpan.Length)
            {
                ThrowOutOfRangeException();
            }

            var written = Encoding.UTF8.GetBytes(value, writer.RemainingSpan);
            writer.Advance(written);
        }

        return ref writer;
    }

    public static ref ZaUtf8SpanWriter Append(ref this ZaUtf8SpanWriter writer, char value)
    {
        var bytes = Encoding.UTF8.GetByteCount(stackalloc char[1]
        {
            value
        });
        if (bytes > writer.RemainingSpan.Length)
        {
            ThrowOutOfRangeException();
        }

        var written = Encoding.UTF8.GetBytes(stackalloc char[1]
        {
            value
        }, writer.RemainingSpan);
        writer.Advance(written);
        return ref writer;
    }

    public static ref ZaUtf8SpanWriter Append(ref this ZaUtf8SpanWriter writer, int value, ReadOnlySpan<char> format = default)
    {
        var standardFormat = ParseStandardFormat(format);
        if (!Utf8Formatter.TryFormat(value, writer.RemainingSpan, out var bytesWritten, standardFormat))
        {
            ThrowOutOfRangeException();
        }

        writer.Advance(bytesWritten);
        return ref writer;
    }

    public static ref ZaUtf8SpanWriter Append(ref this ZaUtf8SpanWriter writer, long value, ReadOnlySpan<char> format = default)
    {
        var standardFormat = ParseStandardFormat(format);
        if (!Utf8Formatter.TryFormat(value, writer.RemainingSpan, out var bytesWritten, standardFormat))
        {
            ThrowOutOfRangeException();
        }

        writer.Advance(bytesWritten);
        return ref writer;
    }

    public static ref ZaUtf8SpanWriter Append(ref this ZaUtf8SpanWriter writer, double value, ReadOnlySpan<char> format = default)
    {
        var standardFormat = ParseStandardFormat(format);
        if (!Utf8Formatter.TryFormat(value, writer.RemainingSpan, out var bytesWritten, standardFormat))
        {
            ThrowOutOfRangeException();
        }

        writer.Advance(bytesWritten);
        return ref writer;
    }

    /// <summary>
    ///     Appends a <see cref="DateTime" /> value using a format.
    /// </summary>
    /// <remarks>
    ///     WARNING: When <paramref name="format" /> is longer than a single standard format specifier (e.g., "O", "R"),
    ///     this method falls back to <see cref="DateTime.ToString(string?, IFormatProvider?)" /> with
    ///     <see cref="CultureInfo.InvariantCulture" />, which allocates a temporary <see cref="string" />.
    ///     Prefer single-letter standard formats to preserve zero-allocation behavior.
    /// </remarks>
    public static ref ZaUtf8SpanWriter Append(ref this ZaUtf8SpanWriter writer, DateTime value, ReadOnlySpan<char> format = default)
    {
        if (format.Length <= 1)
        {
            var standardFormat = format.Length == 1 ? new StandardFormat(format[0]) : default;
            if (!Utf8Formatter.TryFormat(value, writer.RemainingSpan, out var bytesWritten, standardFormat))
            {
                ThrowOutOfRangeException();
            }

            writer.Advance(bytesWritten);
            return ref writer;
        }

        // Fallback path for custom/multi-character formats (allocates)
        var s = value.ToString(format.ToString(), CultureInfo.InvariantCulture);
        var required = Encoding.UTF8.GetByteCount(s);
        if (required > writer.RemainingSpan.Length)
        {
            ThrowOutOfRangeException();
        }

        var written = Encoding.UTF8.GetBytes(s, writer.RemainingSpan);
        writer.Advance(written);
        return ref writer;
    }

    public static ref ZaUtf8SpanWriter AppendLine(ref this ZaUtf8SpanWriter writer)
    {
        return ref writer.Append(Encoding.UTF8.GetBytes(Environment.NewLine));
    }

    public static ref ZaUtf8SpanWriter AppendLine(ref this ZaUtf8SpanWriter writer, string? value)
    {
        if (value is not null)
        {
            writer.Append(value);
        }

        return ref writer.AppendLine();
    }

    public static ref ZaUtf8SpanWriter AppendHex(ref this ZaUtf8SpanWriter writer, ReadOnlySpan<byte> data, bool uppercase = false)
    {
        var required = data.Length * 2;
        if (required > writer.RemainingSpan.Length)
        {
            ThrowOutOfRangeException();
        }

        var dest = writer.RemainingSpan;
        var hex = uppercase ? "0123456789ABCDEF" : "0123456789abcdef";
        var w = 0;

        for (var i = 0; i < data.Length; i++)
        {
            var b = data[i];
            dest[w++] = (byte)hex[b >> 4 & 0xF];
            dest[w++] = (byte)hex[b & 0xF];
        }

        writer.Advance(required);
        return ref writer;
    }

    public static ref ZaUtf8SpanWriter AppendBase64(ref this ZaUtf8SpanWriter writer, ReadOnlySpan<byte> data)
    {
        var required = Base64.GetMaxEncodedToUtf8Length(data.Length);
        if (required > writer.RemainingSpan.Length)
        {
            ThrowOutOfRangeException();
        }

        var status = Base64.EncodeToUtf8(data, writer.RemainingSpan, out var bytesConsumed, out var bytesWritten);
        if (status != OperationStatus.Done)
        {
            ThrowOutOfRangeException();
        }

        writer.Advance(bytesWritten);
        return ref writer;
    }

    private static void ThrowOutOfRangeException()
    {
        throw new ArgumentOutOfRangeException("value", "The destination buffer is too small.");
    }

    private static StandardFormat ParseStandardFormat(ReadOnlySpan<char> format)
    {
        if (format.Length == 0)
        {
            return default;
        }

        if (format.Length == 1)
        {
            return new StandardFormat(format[0]);
        }

        if (byte.TryParse(format[1..], out var precision))
        {
            return new StandardFormat(format[0], precision);
        }

        return new StandardFormat(format[0]);
    }
}