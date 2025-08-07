using ZaString.Core;

namespace ZaString.Tests;

public class ZaPooledStringBuilderTests
{
    [Fact]
    public void Append_GrowsAndBuilds()
    {
        using var b = ZaPooledStringBuilder.Rent(4);
        b.Append("Hello").Append(", ").Append("World!");
        Assert.Equal("Hello, World!", b.AsSpan());
        Assert.Equal("Hello, World!", b.ToString());
    }

    [Fact]
    public void Append_Format_Primitives_Invariant()
    {
        using var b = ZaPooledStringBuilder.Rent(4);
        b.Append(255, "X2").Append(' ').Append(1.5);
        Assert.Equal("FF 1.5", b.AsSpan());
    }

    [Fact]
    public void Clear_ResetsLength()
    {
        using var b = ZaPooledStringBuilder.Rent(4);
        b.Append("abc");
        b.Clear();
        Assert.Equal(0, b.Length);
        Assert.Equal("", b.AsSpan());
    }
}