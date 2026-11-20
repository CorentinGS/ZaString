using System.Globalization;
using System.Runtime.CompilerServices;
using ZaString.Core;

namespace ZaString.Extensions;

[InterpolatedStringHandler]
public ref struct ZaInterpolatedStringHandler
{
    private ZaSpanStringBuilder _builder;
    private readonly IFormatProvider? _provider;

    public ZaInterpolatedStringHandler(int literalLength, int formattedCount, ref ZaSpanStringBuilder builder)
    {
        _builder = builder;
        _provider = CultureInfo.InvariantCulture;
    }

    public ZaInterpolatedStringHandler(int literalLength, int formattedCount, ref ZaSpanStringBuilder builder, IFormatProvider? provider)
    {
        _builder = builder;
        _provider = provider ?? CultureInfo.InvariantCulture;
    }

    public void AppendLiteral(string value)
    {
        _builder.Append(value);
    }

    public void AppendFormatted(string? value)
    {
        _builder.Append(value);
    }

    public void AppendFormatted(ReadOnlySpan<char> value)
    {
        _builder.Append(value);
    }

    public void AppendFormatted(char value)
    {
        _builder.Append(value);
    }

    // * Support boolean interpolation without requiring ISpanFormattable
    public void AppendFormatted(bool value)
    {
        _builder.Append(value ? "true" : "false");
    }

    public void AppendFormatted<T>(T value) where T : ISpanFormattable
    {
        _builder.Append(value, default, _provider);
    }

    public void AppendFormatted<T>(T value, string? format) where T : ISpanFormattable
    {
        _builder.Append(value, format, _provider);
    }

    public void AppendFormatted<T>(T value, int alignment) where T : ISpanFormattable
    {
        if (alignment == 0)
        {
            _builder.Append(value, default, _provider);
            return;
        }

        var remaining = _builder.RemainingSpan;
        if (remaining.Length < Math.Abs(alignment))
        {
            throw new ArgumentOutOfRangeException("value", "The destination buffer is too small.");
        }

        if (!value.TryFormat(remaining, out var charsWritten, default, _provider))
        {
            throw new ArgumentOutOfRangeException("value", "The destination buffer is too small.");
        }

        var padCount = alignment > 0 ? alignment - charsWritten : -alignment - charsWritten;

        if (padCount <= 0)
        {
            _builder.Advance(charsWritten);
            return;
        }

        if (alignment > 0)
        {
            if (remaining.Length < charsWritten + padCount)
            {
                throw new ArgumentOutOfRangeException("value", "The destination buffer is too small.");
            }

            for (var i = charsWritten - 1; i >= 0; i--)
            {
                remaining[i + padCount] = remaining[i];
            }

            for (var i = 0; i < padCount; i++)
            {
                remaining[i] = ' ';
            }

            _builder.Advance(charsWritten + padCount);
        }
        else
        {
            if (remaining.Length < charsWritten + padCount)
            {
                throw new ArgumentOutOfRangeException("value", "The destination buffer is too small.");
            }

            remaining.Slice(charsWritten, padCount).Fill(' ');
            _builder.Advance(charsWritten + padCount);
        }
    }

    public void AppendFormatted<T>(T value, int alignment, string? format) where T : ISpanFormattable
    {
        if (alignment == 0)
        {
            _builder.Append(value, format, _provider);
            return;
        }

        var remaining = _builder.RemainingSpan;
        if (remaining.Length < Math.Abs(alignment))
        {
            throw new ArgumentOutOfRangeException("value", "The destination buffer is too small.");
        }

        if (!value.TryFormat(remaining, out var charsWritten, format, _provider))
        {
            throw new ArgumentOutOfRangeException("value", "The destination buffer is too small.");
        }

        var padCount = alignment > 0 ? alignment - charsWritten : -alignment - charsWritten;

        if (padCount <= 0)
        {
            _builder.Advance(charsWritten);
            return;
        }

        if (alignment > 0)
        {
            if (remaining.Length < charsWritten + padCount)
            {
                throw new ArgumentOutOfRangeException("value", "The destination buffer is too small.");
            }

            for (var i = charsWritten - 1; i >= 0; i--)
            {
                remaining[i + padCount] = remaining[i];
            }

            for (var i = 0; i < padCount; i++)
            {
                remaining[i] = ' ';
            }

            _builder.Advance(charsWritten + padCount);
        }
        else
        {
            if (remaining.Length < charsWritten + padCount)
            {
                throw new ArgumentOutOfRangeException("value", "The destination buffer is too small.");
            }

            remaining.Slice(charsWritten, padCount).Fill(' ');
            _builder.Advance(charsWritten + padCount);
        }
    }


    public void AppendFormatted(string? value, int alignment)
    {
        AppendAligned(value is null ? ReadOnlySpan<char>.Empty : value.AsSpan(), alignment);
    }

    public void AppendFormatted(string? value, int alignment, string? format)
    {
        AppendAligned(value is null ? ReadOnlySpan<char>.Empty : value.AsSpan(), alignment);
    }

    public void AppendFormatted(ReadOnlySpan<char> value, int alignment)
    {
        AppendAligned(value, alignment);
    }

    public void AppendFormatted(ReadOnlySpan<char> value, int alignment, string? format)
    {
        AppendAligned(value, alignment);
    }

    private void AppendAligned(ReadOnlySpan<char> value, int alignment)
    {
        var width = Math.Abs(alignment);
        var totalWidth = Math.Max(width, value.Length);
        var remaining = _builder.RemainingSpan;

        if (remaining.Length < totalWidth)
        {
            throw new ArgumentOutOfRangeException("value", "The destination buffer is too small.");
        }

        var padCount = totalWidth - value.Length;
        if (alignment > 0)
        {
            remaining[..padCount].Fill(' ');
            value.CopyTo(remaining[padCount..]);
        }
        else
        {
            value.CopyTo(remaining);
            remaining.Slice(value.Length, padCount).Fill(' ');
        }

        _builder.Advance(totalWidth);
    }

    public readonly ZaSpanStringBuilder GetResult()
    {
        return _builder;
    }
}
