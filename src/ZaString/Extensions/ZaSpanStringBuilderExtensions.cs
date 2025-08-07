using System.Globalization;
using System.Runtime.CompilerServices;
using ZaString.Core;

namespace ZaString.Extensions;

/// <summary>
///     Provides chainable, fluent extension methods for the <see cref="ZaSpanStringBuilder" />.
/// </summary>
public static class ZaSpanStringBuilderExtensions
{
        /// <summary>
        ///     Appends an interpolated string using a custom handler that writes directly into the builder.
        /// </summary>
        public static ref ZaSpanStringBuilder Append(ref this ZaSpanStringBuilder builder,
            [InterpolatedStringHandlerArgument("builder")] ZaInterpolatedStringHandler handler)
        {
            builder = handler.GetResult();
            return ref builder;
        }

        /// <summary>
        ///     Appends an interpolated string followed by the default line terminator.
        /// </summary>
        public static ref ZaSpanStringBuilder AppendLine(ref this ZaSpanStringBuilder builder,
            [InterpolatedStringHandlerArgument("builder")] ZaInterpolatedStringHandler handler)
        {
            builder = handler.GetResult();
            return ref builder.AppendLine();
        }

        /// <summary>
        ///     Attempts to reserve a writable span of the specified size without throwing.
        ///     On success, caller must write up to <paramref name="size"/> characters and then call <see cref="ZaSpanStringBuilder.Advance(int)"/> with the actual number written.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="size">Requested size to reserve.</param>
        /// <param name="writeSpan">The span the caller can write into.</param>
        /// <returns>true if reserved; false if capacity is insufficient.</returns>
        public static bool TryGetAppendSpan(ref this ZaSpanStringBuilder builder, int size, out Span<char> writeSpan)
        {
            if (size < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            if (size == 0)
            {
                writeSpan = Span<char>.Empty;
                return true;
            }

            if (builder.RemainingSpan.Length < size)
            {
                writeSpan = Span<char>.Empty;
                return false;
            }

            writeSpan = builder.RemainingSpan[..size];
            return true;
        }

        /// <summary>
        ///     Reserves a writable span of the specified size or throws if the capacity is insufficient.
        ///     On success, caller must write up to <paramref name="size"/> characters and then call <see cref="ZaSpanStringBuilder.Advance(int)"/> with the actual number written.
        /// </summary>
        public static ref ZaSpanStringBuilder GetAppendSpan(ref this ZaSpanStringBuilder builder, int size, out Span<char> writeSpan)
        {
            if (!TryGetAppendSpan(ref builder, size, out writeSpan))
            {
                ThrowOutOfRangeException();
            }

            return ref builder;
        }

        /// <summary>
        ///     Removes the last <paramref name="count"/> characters from the written span.
        /// </summary>
        public static ref ZaSpanStringBuilder RemoveLast(ref this ZaSpanStringBuilder builder, int count)
        {
            if (count < 0 || count > builder.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (count > 0)
            {
                builder.RemoveLast(count);
            }

            return ref builder;
        }

        /// <summary>
        ///     Sets the current length to <paramref name="newLength"/>. Must be between 0 and Capacity.
        ///     If <paramref name="newLength"/> is less than the current Length, the content is logically truncated.
        /// </summary>
        public static ref ZaSpanStringBuilder SetLength(ref this ZaSpanStringBuilder builder, int newLength)
        {
            if (newLength < 0 || newLength > builder.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(newLength));
            }

            builder.SetLength(newLength);
            return ref builder;
        }

        /// <summary>
        ///     Ensures the written span ends with the specified character; appends it if needed.
        /// </summary>
        public static ref ZaSpanStringBuilder EnsureEndsWith(ref this ZaSpanStringBuilder builder, char value)
        {
            if (builder.Length == 0 || builder[builder.Length - 1] != value)
            {
                builder.Append(value);
            }

            return ref builder;
        }
        /// <summary>
        ///     Appends the specified character repeated <paramref name="count"/> times.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if count is negative or buffer is too small.</exception>
        public static ref ZaSpanStringBuilder AppendRepeat(ref this ZaSpanStringBuilder builder, char value, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (count == 0)
            {
                return ref builder;
            }

            if (builder.RemainingSpan.Length < count)
            {
                ThrowOutOfRangeException();
            }

            builder.RemainingSpan[..count].Fill(value);
            builder.Advance(count);
            return ref builder;
        }

        /// <summary>
        ///     Attempts to append the specified character repeated <paramref name="count"/> times without throwing.
        /// </summary>
        /// <returns>true if appended; otherwise false when capacity is insufficient.</returns>
        public static bool TryAppendRepeat(ref this ZaSpanStringBuilder builder, char value, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (count == 0)
            {
                return true;
            }

            if (builder.RemainingSpan.Length < count)
            {
                return false;
            }

            builder.RemainingSpan[..count].Fill(value);
            builder.Advance(count);
            return true;
        }

        /// <summary>
        ///     Appends the elements separated by <paramref name="separator"/>.
        ///     Null elements are treated as empty strings.
        /// </summary>
        public static ref ZaSpanStringBuilder AppendJoin(ref this ZaSpanStringBuilder builder, ReadOnlySpan<char> separator, params string?[] values)
        {
            for (var i = 0; i < values.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(separator);
                }

                var s = values[i];
                if (s is not null)
                {
                    builder.Append(s);
                }
            }

            return ref builder;
        }

        /// <summary>
        ///     Appends the elements of <paramref name="values"/> separated by <paramref name="separator"/>.
        /// </summary>
        public static ref ZaSpanStringBuilder AppendJoin<T>(ref this ZaSpanStringBuilder builder, ReadOnlySpan<char> separator, ReadOnlySpan<T> values, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
            where T : ISpanFormattable
        {
            for (var i = 0; i < values.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(separator);
                }

                builder.Append(values[i], format, provider);
            }

            return ref builder;
        }
        /// <summary>
        ///     Attempts to append a read-only span of characters to the builder without throwing.
        /// </summary>
        /// <param name="builder">The builder instance.</param>
        /// <param name="value">The span of characters to append.</param>
        /// <returns><c>true</c> if the value was appended; otherwise <c>false</c> if there was not enough capacity.</returns>
        public static bool TryAppend(ref this ZaSpanStringBuilder builder, ReadOnlySpan<char> value)
        {
            if (value.Length > builder.RemainingSpan.Length)
            {
                return false;
            }

            value.CopyTo(builder.RemainingSpan);
            builder.Advance(value.Length);
            return true;
        }

        /// <summary>
        ///     Attempts to append a string to the builder without throwing.
        /// </summary>
        /// <param name="builder">The builder instance.</param>
        /// <param name="value">The string to append. If null, this is a no-op and returns true.</param>
        /// <returns><c>true</c> if the value was appended; otherwise <c>false</c> if there was not enough capacity.</returns>
        public static bool TryAppend(ref this ZaSpanStringBuilder builder, string? value)
        {
            return value is null || builder.TryAppend(value.AsSpan());
        }

        /// <summary>
        ///     Attempts to append a single character to the builder without throwing.
        /// </summary>
        /// <param name="builder">The builder instance.</param>
        /// <param name="value">The character to append.</param>
        /// <returns><c>true</c> if the value was appended; otherwise <c>false</c> if there was not enough capacity.</returns>
        public static bool TryAppend(ref this ZaSpanStringBuilder builder, char value)
        {
            if (builder.RemainingSpan.Length < 1)
            {
                return false;
            }

            builder.RemainingSpan[0] = value;
            builder.Advance(1);
            return true;
        }

        /// <summary>
        ///     Attempts to append a value of a type that implements <see cref="ISpanFormattable"/> without throwing.
        /// </summary>
        /// <typeparam name="T">The type of the value, which must implement ISpanFormattable.</typeparam>
        /// <param name="builder">The builder instance.</param>
        /// <param name="value">The value to format and append.</param>
        /// <param name="format">An optional format string for the value.</param>
        /// <param name="provider">Format provider to use. If null, <see cref="CultureInfo.InvariantCulture"/> is used.</param>
        /// <returns><c>true</c> if the value was formatted and appended; otherwise <c>false</c> if there was not enough capacity or formatting failed.</returns>
        public static bool TryAppend<T>(ref this ZaSpanStringBuilder builder, T value, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : ISpanFormattable
        {
            provider ??= CultureInfo.InvariantCulture;

            if (!value.TryFormat(builder.RemainingSpan, out var charsWritten, format, provider))
            {
                return false;
            }

            builder.Advance(charsWritten);
            return true;
        }

        /// <summary>
        ///     Attempts to append the default line terminator to the builder without throwing.
        /// </summary>
        /// <param name="builder">The builder instance.</param>
        /// <returns><c>true</c> if the newline was appended; otherwise <c>false</c> if there was not enough capacity.</returns>
        public static bool TryAppendLine(ref this ZaSpanStringBuilder builder)
        {
            var newline = Environment.NewLine.AsSpan();
            return builder.TryAppend(newline);
        }

        /// <summary>
        ///     Attempts to append a string followed by the default line terminator to the builder without throwing.
        ///     The operation is atomic with respect to capacity: if there is not enough space for both, nothing is written.
        /// </summary>
        /// <param name="builder">The builder instance.</param>
        /// <param name="value">The string to append. If null, only the newline is appended.</param>
        /// <returns><c>true</c> if the string and newline were appended; otherwise <c>false</c> if there was not enough capacity.</returns>
        public static bool TryAppendLine(ref this ZaSpanStringBuilder builder, string? value)
        {
            var valueLength = value?.Length ?? 0;
            var newlineLength = Environment.NewLine.Length;
            var required = valueLength + newlineLength;

            if (required > builder.RemainingSpan.Length)
            {
                return false;
            }

            if (valueLength > 0)
            {
                value!.AsSpan().CopyTo(builder.RemainingSpan);
                builder.Advance(valueLength);
            }

            Environment.NewLine.AsSpan().CopyTo(builder.RemainingSpan);
            builder.Advance(newlineLength);
            return true;
        }
    /// <summary>
    ///     Appends a read-only span of characters to the builder.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="value">The span of characters to append.</param>
    /// <returns>A reference to the builder to allow for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the buffer is too small to hold the value.</exception>
    public static ref ZaSpanStringBuilder Append(ref this ZaSpanStringBuilder builder, ReadOnlySpan<char> value)
    {
        if (value.Length > builder.RemainingSpan.Length)
        {
            ThrowOutOfRangeException();
        }

        value.CopyTo(builder.RemainingSpan);
        builder.Advance(value.Length);
        return ref builder;
    }

    /// <summary>
    ///     Appends a string to the builder.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="value">The string to append. If null, the operation is a no-op.</param>
    /// <returns>A reference to the builder to allow for method chaining.</returns>
    public static ref ZaSpanStringBuilder Append(ref this ZaSpanStringBuilder builder, string? value)
    {
        if (value is not null)
        {
            builder.Append(value.AsSpan());
        }

        return ref builder;
    }

    /// <summary>
    ///     Appends a single character to the builder.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="value">The character to append.</param>
    /// <returns>A reference to the builder to allow for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the buffer is too small to hold the character.</exception>
    public static ref ZaSpanStringBuilder Append(ref this ZaSpanStringBuilder builder, char value)
    {
        if (builder.RemainingSpan.Length < 1)
        {
            ThrowOutOfRangeException();
        }

        builder.RemainingSpan[0] = value;
        builder.Advance(1);
        return ref builder;
    }

    /// <summary>
    ///     Appends a boolean value as "true" or "false".
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="value">The boolean value to append.</param>
    /// <returns>A reference to the builder to allow for method chaining.</returns>
    public static ref ZaSpanStringBuilder Append(ref this ZaSpanStringBuilder builder, bool value)
    {
        return ref builder.Append(value ? "true" : "false");
    }

    /// <summary>
    ///     Appends a value of a type that implements <see cref="ISpanFormattable" />.
    ///     This is highly efficient for primitive types like numbers, dates, etc.
    /// </summary>
    /// <typeparam name="T">The type of the value, which must implement ISpanFormattable.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="value">The value to format and append.</param>
    /// <param name="format">An optional format string for the value.</param>
    /// <returns>A reference to the builder to allow for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the buffer is too small to hold the formatted value.</exception>
    /// <exception cref="FormatException">Thrown if the value cannot be formatted correctly.</exception>
    public static ref ZaSpanStringBuilder Append<T>(ref this ZaSpanStringBuilder builder, T value, ReadOnlySpan<char> format = default) where T : ISpanFormattable
    {
        return ref builder.Append(value, format, CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     Appends a value of a type that implements <see cref="ISpanFormattable" /> using the specified format provider.
    /// </summary>
    /// <typeparam name="T">The type of the value, which must implement ISpanFormattable.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="value">The value to format and append.</param>
    /// <param name="format">An optional format string for the value.</param>
    /// <param name="provider">Format provider to use. If null, <see cref="CultureInfo.InvariantCulture"/> is used.</param>
    /// <returns>A reference to the builder to allow for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the buffer is too small to hold the formatted value.</exception>
    /// <exception cref="FormatException">Thrown if the value cannot be formatted correctly.</exception>
    public static ref ZaSpanStringBuilder Append<T>(ref this ZaSpanStringBuilder builder, T value, ReadOnlySpan<char> format, IFormatProvider? provider) where T : ISpanFormattable
    {
        provider ??= CultureInfo.InvariantCulture;

        if (!value.TryFormat(builder.RemainingSpan, out var charsWritten, format, provider))
        {
            ThrowOutOfRangeException();
        }

        builder.Advance(charsWritten);
        return ref builder;
    }

    /// <summary>
    ///     Appends the default line terminator to the builder.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <returns>A reference to the builder to allow for method chaining.</returns>
    public static ref ZaSpanStringBuilder AppendLine(ref this ZaSpanStringBuilder builder)
    {
        return ref builder.Append(Environment.NewLine);
    }

    /// <summary>
    ///     Appends a string followed by the default line terminator to the builder.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="value">The string to append. If null, only the line terminator is appended.</param>
    /// <returns>A reference to the builder to allow for method chaining.</returns>
    public static ref ZaSpanStringBuilder AppendLine(ref this ZaSpanStringBuilder builder, string? value)
    {
        if (value is not null)
        {
            builder.Append(value);
        }
        return ref builder.AppendLine();
    }

    /// <summary>
    ///     Appends a read-only span of characters followed by the default line terminator to the builder.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="value">The span of characters to append.</param>
    /// <returns>A reference to the builder to allow for method chaining.</returns>
    public static ref ZaSpanStringBuilder AppendLine(ref this ZaSpanStringBuilder builder, ReadOnlySpan<char> value)
    {
        builder.Append(value);
        return ref builder.AppendLine();
    }

    /// <summary>
    ///     Conditionally appends a string to the builder if the condition is true.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="value">The string to append if the condition is true. If null, the operation is a no-op.</param>
    /// <returns>A reference to the builder to allow for method chaining.</returns>
    public static ref ZaSpanStringBuilder AppendIf(ref this ZaSpanStringBuilder builder, bool condition, string? value)
    {
        if (condition)
        {
            builder.Append(value);
        }
        return ref builder;
    }

    /// <summary>
    ///     Conditionally appends a read-only span of characters to the builder if the condition is true.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="value">The span of characters to append if the condition is true.</param>
    /// <returns>A reference to the builder to allow for method chaining.</returns>
    public static ref ZaSpanStringBuilder AppendIf(ref this ZaSpanStringBuilder builder, bool condition, ReadOnlySpan<char> value)
    {
        if (condition)
        {
            builder.Append(value);
        }
        return ref builder;
    }

    /// <summary>
    ///     Conditionally appends a single character to the builder if the condition is true.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="value">The character to append if the condition is true.</param>
    /// <returns>A reference to the builder to allow for method chaining.</returns>
    public static ref ZaSpanStringBuilder AppendIf(ref this ZaSpanStringBuilder builder, bool condition, char value)
    {
        if (condition)
        {
            builder.Append(value);
        }
        return ref builder;
    }

    /// <summary>
    ///     Conditionally appends a formatted value to the builder if the condition is true.
    /// </summary>
    /// <typeparam name="T">The type of the value, which must implement ISpanFormattable.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="value">The value to format and append if the condition is true.</param>
    /// <param name="format">An optional format string for the value.</param>
    /// <returns>A reference to the builder to allow for method chaining.</returns>
    public static ref ZaSpanStringBuilder AppendIf<T>(ref this ZaSpanStringBuilder builder, bool condition, T value, ReadOnlySpan<char> format = default) where T : ISpanFormattable
    {
        if (condition)
        {
            builder.Append(value, format);
        }
        return ref builder;
    }

    /// <summary>
    ///     Throws a standardized exception for out-of-range errors.
    /// </summary>
    private static void ThrowOutOfRangeException()
    {
        throw new ArgumentOutOfRangeException("value", "The destination buffer is too small.");
    }

        // Escaping helpers

        public static ref ZaSpanStringBuilder AppendJsonEscaped(ref this ZaSpanStringBuilder builder, ReadOnlySpan<char> value)
        {
            if (!TryAppendJsonEscaped(ref builder, value))
            {
                ThrowOutOfRangeException();
            }
            return ref builder;
        }

        public static bool TryAppendJsonEscaped(ref this ZaSpanStringBuilder builder, ReadOnlySpan<char> value)
        {
            var required = GetJsonEscapedLength(value);
            if (required > builder.RemainingSpan.Length)
            {
                return false;
            }
            if (required == value.Length)
            {
                value.CopyTo(builder.RemainingSpan);
                builder.Advance(value.Length);
                return true;
            }
            var dest = builder.RemainingSpan;
            var w = 0;
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                switch (c)
                {
                    case '"': dest[w++] = '\\'; dest[w++] = '"'; break;
                    case '\\': dest[w++] = '\\'; dest[w++] = '\\'; break;
                    case '\b': dest[w++] = '\\'; dest[w++] = 'b'; break;
                    case '\f': dest[w++] = '\\'; dest[w++] = 'f'; break;
                    case '\n': dest[w++] = '\\'; dest[w++] = 'n'; break;
                    case '\r': dest[w++] = '\\'; dest[w++] = 'r'; break;
                    case '\t': dest[w++] = '\\'; dest[w++] = 't'; break;
                    default:
                        if (c < ' ')
                        {
                            dest[w++] = '\\';
                            dest[w++] = 'u';
                            dest[w++] = '0';
                            dest[w++] = '0';
                            WriteHexByte((byte)c, dest.Slice(w, 2));
                            w += 2;
                        }
                        else
                        {
                            dest[w++] = c;
                        }
                        break;
                }
            }
            builder.Advance(required);
            return true;
        }

        private static int GetJsonEscapedLength(ReadOnlySpan<char> value)
        {
            var extra = 0;
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                switch (c)
                {
                    case '"':
                    case '\\':
                    case '\b':
                    case '\f':
                    case '\n':
                    case '\r':
                    case '\t':
                        extra += 1; // becomes two chars instead of one
                        break;
                    default:
                        if (c < ' ')
                        {
                            extra += 5; // \u00XX adds 5 extra over the original 1
                        }
                        break;
                }
            }
            return value.Length + extra;
        }

        private static void WriteHexByte(byte b, Span<char> dest)
        {
            const string hex = "0123456789ABCDEF";
            dest[0] = hex[(b >> 4) & 0xF];
            dest[1] = hex[b & 0xF];
        }

        public static ref ZaSpanStringBuilder AppendHtmlEscaped(ref this ZaSpanStringBuilder builder, ReadOnlySpan<char> value)
        {
            if (!TryAppendHtmlEscaped(ref builder, value))
            {
                ThrowOutOfRangeException();
            }
            return ref builder;
        }

        public static bool TryAppendHtmlEscaped(ref this ZaSpanStringBuilder builder, ReadOnlySpan<char> value)
        {
            var required = GetHtmlEscapedLength(value);
            if (required > builder.RemainingSpan.Length)
            {
                return false;
            }
            if (required == value.Length)
            {
                value.CopyTo(builder.RemainingSpan);
                builder.Advance(value.Length);
                return true;
            }
            var dest = builder.RemainingSpan;
            var w = 0;
            for (var i = 0; i < value.Length; i++)
            {
                switch (value[i])
                {
                    case '&': dest[w++] = '&'; dest[w++] = 'a'; dest[w++] = 'm'; dest[w++] = 'p'; dest[w++] = ';'; break; // &amp;
                    case '<': dest[w++] = '&'; dest[w++] = 'l'; dest[w++] = 't'; dest[w++] = ';'; break; // &lt;
                    case '>': dest[w++] = '&'; dest[w++] = 'g'; dest[w++] = 't'; dest[w++] = ';'; break; // &gt;
                    case '"': dest[w++] = '&'; dest[w++] = 'q'; dest[w++] = 'u'; dest[w++] = 'o'; dest[w++] = 't'; dest[w++] = ';'; break; // &quot;
                    case '\'': dest[w++] = '&'; dest[w++] = '#'; dest[w++] = '3'; dest[w++] = '9'; dest[w++] = ';'; break; // &#39;
                    default: dest[w++] = value[i]; break;
                }
            }
            builder.Advance(required);
            return true;
        }

        private static int GetHtmlEscapedLength(ReadOnlySpan<char> value)
        {
            var extra = 0;
            for (var i = 0; i < value.Length; i++)
            {
                switch (value[i])
                {
                    case '&': extra += 4; break; // &amp; (5) - 1 original = +4
                    case '<':
                    case '>': extra += 3; break; // &lt; or &gt; (4) -1 = +3
                    case '"': extra += 5; break; // &quot; (6) -1 = +5
                    case '\'': extra += 4; break; // &#39; (5) -1 = +4
                }
            }
            return value.Length + extra;
        }

        public static ref ZaSpanStringBuilder AppendCsvEscaped(ref this ZaSpanStringBuilder builder, ReadOnlySpan<char> value)
        {
            if (!TryAppendCsvEscaped(ref builder, value))
            {
                ThrowOutOfRangeException();
            }
            return ref builder;
        }

        public static bool TryAppendCsvEscaped(ref this ZaSpanStringBuilder builder, ReadOnlySpan<char> value)
        {
            var needsQuote = NeedsCsvQuoting(value);
            if (!needsQuote)
            {
                return builder.TryAppend(value);
            }
            var quoteCount = 0;
            for (var i = 0; i < value.Length; i++) if (value[i] == '"') quoteCount++;
            var required = value.Length + quoteCount + 2;
            if (required > builder.RemainingSpan.Length)
            {
                return false;
            }
            var dest = builder.RemainingSpan;
            var w = 0;
            dest[w++] = '"';
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                dest[w++] = c;
                if (c == '"')
                {
                    dest[w++] = '"';
                }
            }
            dest[w++] = '"';
            builder.Advance(required);
            return true;
        }

        private static bool NeedsCsvQuoting(ReadOnlySpan<char> value)
        {
            if (value.Length == 0) return false;
            if (char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[^1])) return true;
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (c == ',' || c == '"' || c == '\n' || c == '\r') return true;
            }
            return false;
        }
}