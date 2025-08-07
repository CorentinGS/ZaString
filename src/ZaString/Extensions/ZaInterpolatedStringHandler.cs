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

    public readonly ZaSpanStringBuilder GetResult()
    {
        return _builder;
    }
}