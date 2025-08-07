using System.Globalization;
using ZaString.Core;
using ZaString.Extensions;

namespace ZaString.Tests;

public class ZaSpanStringBuilderFormatTests
{
    [Fact]
    public void AppendFormat_WithParameters_FormatsCorrectly()
    {
        Span<char> buffer = stackalloc char[256];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.AppendFormat("User: {0}, Balance: {1:C}, Active: {2}", "John", 1234.56, true);

        var expected = string.Format(CultureInfo.InvariantCulture, "User: {0}, Balance: {1:C}, Active: {2}", "John", 1234.56, true);
        Assert.Equal(expected, builder.AsSpan().ToString());
    }

    [Fact]
    public void AppendFormat_WithCulture_FormatsWithCulture()
    {
        Span<char> buffer = stackalloc char[256];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var culture = new CultureInfo("fr-FR");
        builder.AppendFormat(culture, "Balance: {0:C}", 1234.56);

        var expected = string.Format(culture, "Balance: {0:C}", 1234.56);
        Assert.Equal(expected, builder.AsSpan().ToString());
    }

    [Fact]
    public void AppendFormat_WithNullValues_HandlesNulls()
    {
        Span<char> buffer = stackalloc char[256];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.AppendFormat("Null: {0}, Text: {1}, Another null: {2}", null, "test", null);

        var expected = string.Format("Null: {0}, Text: {1}, Another null: {2}", null, "test", null);
        Assert.Equal(expected, builder.AsSpan().ToString());
    }

    [Fact]
    public void AppendFormat_WithISpanFormattable_UsesSpanFormattable()
    {
        Span<char> buffer = stackalloc char[256];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.AppendFormat("Int: {0:N0}, Double: {1:F2}", 123, 456.789);

        var expected = string.Format("Int: {0:N0}, Double: {1:F2}", 123, 456.789);
        Assert.Equal(expected, builder.AsSpan().ToString());
    }
}