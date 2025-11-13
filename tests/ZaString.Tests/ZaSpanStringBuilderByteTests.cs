using System.Runtime.InteropServices;
using ZaString.Core;
using ZaString.Extensions;

namespace ZaString.Tests;

public class ZaSpanStringBuilderByteTests
{
    [Fact]
    public void AsByteSpan_Empty_ReturnsEmpty()
    {
        var builder = ZaSpanStringBuilder.Create(Span<char>.Empty);
        var bytes = builder.AsByteSpan();
        Assert.True(bytes.IsEmpty);
        Assert.Equal(0, bytes.Length);
    }

    [Fact]
    public void ToByteArray_Empty_ReturnsEmptyArray()
    {
        var builder = ZaSpanStringBuilder.Create(Span<char>.Empty);
        var arr = builder.ToByteArray();
        Assert.Empty(arr);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("Hello")]
    [InlineData("é")]
    [InlineData("漢字")]
    [InlineData("🐱")] // surrogate pair
    public void AsByteSpan_ContentsMatchUnderlyingChars(string value)
    {
        var buffer = new char[value.Length];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.Append(value.AsSpan());

        var spanChars = builder.AsSpan();
        var expectedBytes = MemoryMarshal.AsBytes(spanChars);
        var actualBytes = builder.AsByteSpan();

        Assert.Equal(expectedBytes.Length, actualBytes.Length);
        Assert.True(expectedBytes.SequenceEqual(actualBytes));
    }

    [Theory]
    [InlineData("A")]
    [InlineData("Hello World!")]
    [InlineData("éçà")]
    [InlineData("🐱🐶")] // multiple surrogate pairs
    public void ToByteArray_ReturnsCopy(string value)
    {
        var buffer = new char[value.Length];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.Append(value.AsSpan());

        var arr = builder.ToByteArray();
        var expected = MemoryMarshal.AsBytes(builder.AsSpan());
        Assert.Equal(expected.Length, arr.Length);
        Assert.True(expected.SequenceEqual(arr));

        // Mutate builder to ensure array is a copy
        if (value.Length > 0)
        {
            builder.RemoveLast(1);
            Assert.True(expected.SequenceEqual(arr)); // original still matches array
        }
    }
}