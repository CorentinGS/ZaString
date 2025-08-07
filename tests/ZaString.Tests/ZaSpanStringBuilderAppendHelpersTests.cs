using System.Globalization;
using ZaString.Core;
using ZaString.Extensions;

namespace ZaString.Tests;

public class ZaSpanStringBuilderAppendHelpersTests
{
    [Fact]
    public void AppendRepeat_AppendsCorrectly()
    {
        Span<char> buffer = stackalloc char[5];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.AppendRepeat('x', 5);

        Assert.Equal("xxxxx", builder.AsSpan());
        Assert.Equal(5, builder.Length);
    }

    [Fact]
    public void AppendRepeat_Zero_NoChange()
    {
        Span<char> buffer = stackalloc char[3];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.AppendRepeat('x', 0);

        Assert.Equal(0, builder.Length);
        Assert.Equal("", builder.AsSpan());
    }

    [Fact]
    public void TryAppendRepeat_Succeeds_WhenCapacitySufficient()
    {
        Span<char> buffer = stackalloc char[4];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var ok = builder.TryAppendRepeat('a', 4);

        Assert.True(ok);
        Assert.Equal("aaaa", builder.AsSpan());
    }

    [Fact]
    public void TryAppendRepeat_InsufficientCapacity_ReturnsFalse_NoChange()
    {
        Span<char> buffer = stackalloc char[3];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var ok = builder.TryAppendRepeat('a', 4);

        Assert.False(ok);
        Assert.Equal(0, builder.Length);
        Assert.Equal("", builder.AsSpan());
    }

    [Fact]
    public void AppendJoin_Strings_Works()
    {
        Span<char> buffer = stackalloc char[20];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.AppendJoin(", ".AsSpan(), "a", null, "c");

        Assert.Equal("a, , c", builder.AsSpan());
    }

    [Fact]
    public void AppendJoin_ISpanFormattable_Works_WithProvider()
    {
        var fr = new CultureInfo("fr-FR");
        Span<char> buffer = stackalloc char[20];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var values = new[] { 1.5, 2.5 };
        builder.AppendJoin<double>("; ".AsSpan(), values, provider: fr);

        Assert.Equal("1,5; 2,5", builder.AsSpan());
    }
}

