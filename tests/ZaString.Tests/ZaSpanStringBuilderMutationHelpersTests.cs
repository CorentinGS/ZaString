using ZaString.Core;
using ZaString.Extensions;

namespace ZaString.Tests;

public class ZaSpanStringBuilderMutationHelpersTests
{
    [Fact]
    public void SetLength_Truncates()
    {
        Span<char> buffer = stackalloc char[16];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.Append("abcdef");

        builder.SetLength(3);

        Assert.Equal(3, builder.Length);
        Assert.Equal("abc", builder.AsSpan());
    }

    [Fact]
    public void RemoveLast_RemovesCorrectly()
    {
        Span<char> buffer = stackalloc char[16];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.Append("abcdef");

        builder.RemoveLast(2);

        Assert.Equal("abcd", builder.AsSpan());
        Assert.Equal(4, builder.Length);
    }

    [Fact]
    public void EnsureEndsWith_AppendsWhenMissing()
    {
        Span<char> buffer = stackalloc char[10];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.Append("abc");

        builder.EnsureEndsWith('x');

        Assert.Equal("abcx", builder.AsSpan());
        Assert.Equal('x', builder[builder.Length - 1]);
    }

    [Fact]
    public void EnsureEndsWith_NoOpWhenAlreadyEnding()
    {
        Span<char> buffer = stackalloc char[10];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.Append("abcx");

        builder.EnsureEndsWith('x');

        Assert.Equal("abcx", builder.AsSpan());
    }
}

