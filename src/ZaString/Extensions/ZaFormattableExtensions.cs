using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace ZaString.Extensions;

/// <summary>
///     Provides zero-allocation formatting helpers for values implementing <see cref="ISpanFormattable" /> and
///     <see cref="IUtf8SpanFormattable" />.
/// </summary>
public static class ZaFormattableExtensions
{
    /// <summary>
    ///     Formats the specified <paramref name="value" /> into the provided <paramref name="destination" /> buffer.
    /// </summary>
    /// <typeparam name="T">A type that implements <see cref="ISpanFormattable" />.</typeparam>
    /// <param name="value">The value to format.</param>
    /// <param name="destination">The destination buffer that receives the formatted characters.</param>
    /// <param name="format">An optional format.</param>
    /// <param name="provider">An optional format provider. Defaults to <see cref="CultureInfo.InvariantCulture" />.</param>
    /// <returns>A read-only span over the portion of <paramref name="destination" /> that was written.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the destination buffer is too small.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<char> ToSpan<T>(this T value, Span<char> destination, ReadOnlySpan<char> format = default,
        IFormatProvider? provider = null) where T : ISpanFormattable
    {
        provider ??= CultureInfo.InvariantCulture;
        if (!value.TryFormat(destination, out var written, format, provider))
        {
            ThrowOutOfRangeException();
        }

        return destination[..written];
    }

    /// <summary>
    ///     Attempts to format the specified <paramref name="value" /> into the provided buffer without throwing.
    /// </summary>
    /// <typeparam name="T">A type that implements <see cref="ISpanFormattable" />.</typeparam>
    /// <param name="value">The value to format.</param>
    /// <param name="destination">The destination buffer that receives the formatted characters.</param>
    /// <param name="writtenSpan">The span of characters that were written when the method returns true; otherwise default.</param>
    /// <param name="format">An optional format.</param>
    /// <param name="provider">An optional format provider. Defaults to <see cref="CultureInfo.InvariantCulture" />.</param>
    /// <returns><c>true</c> if formatting succeeded; otherwise <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryToSpan<T>(this T value, Span<char> destination, out ReadOnlySpan<char> writtenSpan,
        ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : ISpanFormattable
    {
        provider ??= CultureInfo.InvariantCulture;
        if (value.TryFormat(destination, out var written, format, provider))
        {
            writtenSpan = destination[..written];
            return true;
        }

        writtenSpan = default;
        return false;
    }

    /// <summary>
    ///     Formats the specified <paramref name="value" /> directly as UTF-8 into the provided <paramref name="destination" /> buffer.
    ///     This avoids transcoding from UTF-16 to UTF-8.
    /// </summary>
    /// <typeparam name="T">A type that implements <see cref="IUtf8SpanFormattable" />.</typeparam>
    /// <param name="value">The value to format.</param>
    /// <param name="destination">The destination buffer that receives the formatted bytes.</param>
    /// <param name="format">An optional format (standard format specifier recommended).</param>
    /// <param name="provider">An optional format provider. Defaults to <see cref="CultureInfo.InvariantCulture" />.</param>
    /// <returns>A read-only span over the portion of <paramref name="destination" /> that was written.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the destination buffer is too small.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<byte> ToUtf8Span<T>(this T value, Span<byte> destination, ReadOnlySpan<char> format = default,
        IFormatProvider? provider = null) where T : IUtf8SpanFormattable
    {
        provider ??= CultureInfo.InvariantCulture;
        if (!value.TryFormat(destination, out var written, format, provider))
        {
            ThrowOutOfRangeException();
        }

        return destination[..written];
    }

    /// <summary>
    ///     Attempts to format the specified <paramref name="value" /> as UTF-8 into the provided buffer without throwing.
    /// </summary>
    /// <typeparam name="T">A type that implements <see cref="IUtf8SpanFormattable" />.</typeparam>
    /// <param name="value">The value to format.</param>
    /// <param name="destination">The destination buffer that receives the formatted bytes.</param>
    /// <param name="writtenSpan">The span of bytes that were written when the method returns true; otherwise default.</param>
    /// <param name="format">An optional format (standard format specifier recommended).</param>
    /// <param name="provider">An optional format provider. Defaults to <see cref="CultureInfo.InvariantCulture" />.</param>
    /// <returns><c>true</c> if formatting succeeded; otherwise <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryToUtf8Span<T>(this T value, Span<byte> destination, out ReadOnlySpan<byte> writtenSpan,
        ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : IUtf8SpanFormattable
    {
        provider ??= CultureInfo.InvariantCulture;
        if (value.TryFormat(destination, out var written, format, provider))
        {
            writtenSpan = destination[..written];
            return true;
        }

        writtenSpan = default;
        return false;
    }

    // * Throws a standardized exception for insufficient destination capacity.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ThrowOutOfRangeException()
    {
        throw new ArgumentOutOfRangeException("destination", "The destination buffer is too small.");
    }
}