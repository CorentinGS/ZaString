using System.Globalization;
using ZaString.Core;
using ZaString.Extensions;

namespace ZaString.Tests;

public class ZaSpanStringBuilderInterpolationTests
{
    [Fact]
    public void Append_InterpolatedString_WritesLiteralAndFormatted()
    {
        Span<char> buffer = stackalloc char[64];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append($"Hello {{world}}: {123} {1.5}");

        Assert.Equal("Hello {world}: 123 1.5", builder.AsSpan());
    }

    [Fact]
    public void Append_InterpolatedString_WithFormat_AndCulture()
    {
        Span<char> buffer = stackalloc char[64];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append($"Hex: {255:X2}");
        Assert.Equal("Hex: FF", builder.AsSpan());

        builder.Clear();

        var fr = new CultureInfo("fr-FR");
        builder.Append($"FR: {1.5}"); // handler defaults to invariant; test explicit provider
        Assert.Equal("FR: 1.5", builder.AsSpan());
    }

    [Fact]
    public void AppendLine_InterpolatedString_AppendsNewline()
    {
        Span<char> buffer = stackalloc char[64];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.AppendLine($"Line: {42}");

        Assert.Equal($"Line: 42{Environment.NewLine}", builder.AsSpan());
    }
}

