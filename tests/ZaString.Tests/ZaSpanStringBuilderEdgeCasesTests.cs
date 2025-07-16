using System.Globalization;
using ZaString.Core;
using ZaString.Extensions;

namespace ZaString.Tests;

public class ZaSpanStringBuilderEdgeCasesTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(1000)]
    public void Create_WithVariousBufferSizes_WorksCorrectly(int bufferSize)
    {
        var buffer = bufferSize > 0 ? stackalloc char[bufferSize] : Span<char>.Empty;
        var builder = ZaSpanStringBuilder.Create(buffer);

        Assert.Equal(bufferSize, builder.Capacity);
        Assert.Equal(0, builder.Length);
    }

    [Fact]
    public void Append_ToEmptyBuffer_ThrowsImmediately()
    {
        var buffer = Span<char>.Empty;
        var builder = ZaSpanStringBuilder.Create(buffer);

        var exceptionThrown = false;
        try
        {
            builder.Append("x");
        }
        catch (ArgumentOutOfRangeException)
        {
            exceptionThrown = true;
        }

        Assert.True(exceptionThrown);
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("Hello")]
    [InlineData("This is a longer string")]
    public void Append_StringToSufficientBuffer_WorksCorrectly(string input)
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append(input);

        Assert.Equal(input, builder.AsSpan());
        Assert.Equal(input.Length, builder.Length);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    public void Append_IntegerValues_WorksCorrectly(int value)
    {
        Span<char> buffer = stackalloc char[20];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append(value);

        Assert.Equal(value.ToString(), builder.AsSpan());
    }

    [Theory]
    [InlineData(long.MinValue)]
    [InlineData(-1L)]
    [InlineData(0L)]
    [InlineData(1L)]
    [InlineData(long.MaxValue)]
    public void Append_LongValues_WorksCorrectly(long value)
    {
        Span<char> buffer = stackalloc char[25];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append(value);

        Assert.Equal(value.ToString(), builder.AsSpan());
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    [InlineData(3.14159)]
    [InlineData(double.MinValue)]
    [InlineData(double.MaxValue)]
    public void Append_DoubleValues_WorksCorrectly(double value)
    {
        Span<char> buffer = stackalloc char[50];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append(value);

        Assert.Equal(value.ToString(CultureInfo.InvariantCulture), builder.AsSpan());
    }

    [Theory]
    [InlineData(0.0f)]
    [InlineData(1.0f)]
    [InlineData(-1.0f)]
    [InlineData(3.14159f)]
    [InlineData(float.MinValue)]
    [InlineData(float.MaxValue)]
    public void Append_FloatValues_WorksCorrectly(float value)
    {
        Span<char> buffer = stackalloc char[50];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append(value);

        Assert.Equal(value.ToString(CultureInfo.InvariantCulture), builder.AsSpan());
    }

    [Fact]
    public void MultipleAppends_ToExactCapacity_WorksCorrectly()
    {
        Span<char> buffer = stackalloc char[10];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append("Hello").Append("World");

        Assert.Equal("HelloWorld", builder.AsSpan());
        Assert.Equal(10, builder.Length);
        Assert.Equal(0, builder.RemainingSpan.Length);
    }

    [Fact]
    public void Append_UnicodeCharacters_WorksCorrectly()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append("Hello 🌍 World! 🚀");

        Assert.Equal("Hello 🌍 World! 🚀", builder.AsSpan());
    }

    [Fact]
    public void Append_ReadOnlySpanEmpty_DoesNothing()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append(ReadOnlySpan<char>.Empty);

        Assert.Equal("", builder.AsSpan());
        Assert.Equal(0, builder.Length);
    }

    [Fact]
    public void WrittenSpan_IsReadOnlyReference_ToBuffer()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append("Test");
        var written = builder.WrittenSpan;

        Assert.True(written.SequenceEqual("Test".AsSpan()));

        builder.Append("ing");
        written = builder.WrittenSpan;

        Assert.True(written.SequenceEqual("Testing".AsSpan()));
    }
}