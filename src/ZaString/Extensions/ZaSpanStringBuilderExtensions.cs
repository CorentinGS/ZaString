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
        if (!value.TryFormat(builder.RemainingSpan, out var charsWritten, format, null))
        {
            // This can mean two things: buffer is too small, or format is invalid.
            // We throw for "out of range" as it's the most common and actionable reason.
            ThrowOutOfRangeException();
        }

        builder.Advance(charsWritten);
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