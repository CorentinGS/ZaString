using System.Globalization;
using ZaString.Core;

namespace ZaString.Extensions;

/// <summary>
///     Provides chainable, fluent extension methods for the <see cref="ZaSpanStringBuilder" />.
/// </summary>
public static class ZaSpanStringBuilderExtensions
{
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