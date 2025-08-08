using System;
using System.Globalization;
using ZaString.Extensions;
using Xunit;

namespace ZaString.Tests;

public class ToSpanExtensionsTests
{
    [Fact]
    public void ToSpan_Int_WritesExpected()
    {
        Span<char> buffer = stackalloc char[8];
        var span = 12345.ToSpan(buffer);
        Assert.Equal("12345", span.ToString());
    }

    [Fact]
    public void TryToSpan_Int_Succeeds()
    {
        Span<char> buffer = stackalloc char[8];
        var ok = 67890.TryToSpan(buffer, out var written);
        Assert.True(ok);
        Assert.Equal("67890", written.ToString());
    }

    [Fact]
    public void TryToSpan_BufferTooSmall_ReturnsFalse()
    {
        Span<char> buffer = stackalloc char[2];
        var ok = 12345.TryToSpan(buffer, out var written);
        Assert.False(ok);
        Assert.True(written.IsEmpty);
    }

    [Fact]
    public void ToSpan_WithFormat_AndProvider()
    {
        Span<char> buffer = stackalloc char[32];
        var number = 1234.5;
        var span = number.ToSpan(buffer, "F2", CultureInfo.InvariantCulture);
        Assert.Equal("1234.50", span.ToString());
    }

    [Fact]
    public void ToUtf8Span_Guid_WritesExpected()
    {
        var guid = new Guid("00112233-4455-6677-8899-aabbccddeeff");
        Span<byte> buffer = stackalloc byte[36];
        var bytes = guid.ToUtf8Span(buffer, "D", CultureInfo.InvariantCulture);
        Assert.Equal("00112233-4455-6677-8899-aabbccddeeff", System.Text.Encoding.UTF8.GetString(bytes));
    }

    [Fact]
    public void TryToUtf8Span_BufferTooSmall_ReturnsFalse()
    {
        var guid = Guid.Empty;
        Span<byte> buffer = stackalloc byte[10];
        var ok = guid.TryToUtf8Span(buffer, out var written, "D", CultureInfo.InvariantCulture);
        Assert.False(ok);
        Assert.True(written.IsEmpty);
    }
}