using System.Globalization;
using ZaString.Core;

namespace ZaString.Extensions;

/// <summary>
///     Provides chainable, fluent extension methods for the <see cref="ZaSpanStringBuilder" />.
/// </summary>
public static class ZaSpanStringBuilderExtensions
{
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
}