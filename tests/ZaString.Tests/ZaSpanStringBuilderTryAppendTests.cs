using System.Globalization;
using ZaString.Core;
using ZaString.Extensions;

namespace ZaString.Tests;

public class ZaSpanStringBuilderTryAppendTests
{
    [Fact]
    public void TryAppend_ReadOnlySpan_Succeeds()
    {
        Span<char> buffer = stackalloc char[10];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var ok = builder.TryAppend("Hello".AsSpan());

        Assert.True(ok);
        Assert.Equal(5, builder.Length);
        Assert.Equal("Hello", builder.AsSpan());
    }

    [Fact]
    public void TryAppend_ReadOnlySpan_InsufficientCapacity_ReturnsFalse_NoChange()
    {
        Span<char> buffer = stackalloc char[3];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var ok = builder.TryAppend("Hello".AsSpan());

        Assert.False(ok);
        Assert.Equal(0, builder.Length);
        Assert.Equal("", builder.AsSpan());
    }

    [Fact]
    public void TryAppend_String_Null_ReturnsTrue_NoChange()
    {
        Span<char> buffer = stackalloc char[3];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var ok = builder.TryAppend((string?)null);

        Assert.True(ok);
        Assert.Equal(0, builder.Length);
        Assert.Equal("", builder.AsSpan());
    }

    [Fact]
    public void TryAppend_String_Succeeds()
    {
        Span<char> buffer = stackalloc char[5];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var ok = builder.TryAppend("Hello");

        Assert.True(ok);
        Assert.Equal("Hello", builder.AsSpan());
    }

    [Fact]
    public void TryAppend_Char_Succeeds()
    {
        Span<char> buffer = stackalloc char[1];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var ok = builder.TryAppend('A');

        Assert.True(ok);
        Assert.Equal("A", builder.AsSpan());
        Assert.Equal(1, builder.Length);
    }

    [Fact]
    public void TryAppend_Char_Insufficient_ReturnsFalse_NoChange()
    {
        var builder = ZaSpanStringBuilder.Create(Span<char>.Empty);

        var ok = builder.TryAppend('A');

        Assert.False(ok);
        Assert.Equal(0, builder.Length);
        Assert.Equal("", builder.AsSpan());
    }

    [Fact]
    public void TryAppend_ISpanFormattable_Double_DefaultProvider_UsesInvariant()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            // Set a culture with comma decimal separator to ensure provider default is invariant
            CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

            Span<char> buffer = stackalloc char[10];
            var builder = ZaSpanStringBuilder.Create(buffer);

            var ok = builder.TryAppend(1.5);

            Assert.True(ok);
            Assert.Equal("1.5", builder.AsSpan());
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void TryAppend_ISpanFormattable_Double_CustomProvider_RespectsProvider()
    {
        var fr = new CultureInfo("fr-FR");
        Span<char> buffer = stackalloc char[10];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var ok = builder.TryAppend(1.5, provider: fr);

        Assert.True(ok);
        Assert.Equal("1,5", builder.AsSpan());
    }

    [Fact]
    public void TryAppendLine_OnlyNewline_Succeeds_WhenCapacitySufficient()
    {
        var newlineLen = Environment.NewLine.Length;
        Span<char> buffer = stackalloc char[newlineLen];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var ok = builder.TryAppendLine();

        Assert.True(ok);
        Assert.Equal(Environment.NewLine, builder.AsSpan());
        Assert.Equal(newlineLen, builder.Length);
    }

    [Fact]
    public void TryAppendLine_OnlyNewline_InsufficientCapacity_ReturnsFalse_NoChange()
    {
        var newlineLen = Environment.NewLine.Length;
        var capacity = Math.Max(0, newlineLen - 1);
        Span<char> buffer = capacity == 0 ? Span<char>.Empty : stackalloc char[capacity];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var ok = builder.TryAppendLine();

        Assert.False(ok);
        Assert.Equal(0, builder.Length);
        Assert.Equal("", builder.AsSpan());
    }

    [Fact]
    public void TryAppendLine_String_AtomicFailure_NoPartialWrite()
    {
        var value = "Hi"; // length 2
        var newlineLen = Environment.NewLine.Length;
        var required = value.Length + newlineLen;
        var capacity = Math.Max(value.Length, required - 1); // ensure not enough for both, but possibly enough for value

        Span<char> buffer = stackalloc char[capacity];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var ok = builder.TryAppendLine(value);

        Assert.False(ok);
        Assert.Equal(0, builder.Length);
        Assert.Equal("", builder.AsSpan());
    }

    [Fact]
    public void TryAppendLine_String_Succeeds_WhenCapacitySufficient()
    {
        var value = "Hello";
        var newlineLen = Environment.NewLine.Length;
        var required = value.Length + newlineLen;

        Span<char> buffer = stackalloc char[required];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var ok = builder.TryAppendLine(value);

        Assert.True(ok);
        Assert.Equal(value + Environment.NewLine, builder.AsSpan());
        Assert.Equal(required, builder.Length);
    }
}

