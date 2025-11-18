using System.Text;
using ZaString.Core;
using ZaString.Extensions;

namespace ZaString.Tests;

public class ZaSpanStringBuilderUtf8Tests
{
    [Fact]
    public void AsRawBytes_ReturnsUtf16Bytes_WhichMayLookLikeSingleLetterIfInterpretedAsAscii()
    {
        // This test demonstrates the "single letter" issue if interpreted as ASCII
        // 'A' is 0x41 0x00.
        Span<char> buffer = stackalloc char[10];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.Append("A");
        
        var bytes = builder.AsRawBytes();
        Assert.Equal(2, bytes.Length);
        Assert.Equal(0x41, bytes[0]);
        Assert.Equal(0x00, bytes[1]);
    }

    [Fact]
    public void TryCopyToUtf8_ReturnsUtf8Bytes()
    {
        Span<char> buffer = stackalloc char[10];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.Append("ABC");

        Span<byte> utf8Buffer = stackalloc byte[10];
        bool success = builder.TryCopyToUtf8(utf8Buffer, out int bytesWritten);

        Assert.True(success);
        Assert.Equal(3, bytesWritten);
        Assert.Equal((byte)'A', utf8Buffer[0]);
        Assert.Equal((byte)'B', utf8Buffer[1]);
        Assert.Equal((byte)'C', utf8Buffer[2]);
    }

    [Fact]
    public void ToUtf8Array_ReturnsUtf8Bytes()
    {
        Span<char> buffer = stackalloc char[10];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.Append("ABC");

        var utf8Bytes = builder.ToUtf8Array();

        Assert.Equal(3, utf8Bytes.Length);
        Assert.Equal((byte)'A', utf8Bytes[0]);
        Assert.Equal((byte)'B', utf8Bytes[1]);
        Assert.Equal((byte)'C', utf8Bytes[2]);
    }

    [Fact]
    public void TryCopyToUtf8_HandlesUnicode()
    {
        Span<char> buffer = stackalloc char[10];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.Append("é"); // 0xC3 0xA9 in UTF-8

        Span<byte> utf8Buffer = stackalloc byte[10];
        bool success = builder.TryCopyToUtf8(utf8Buffer, out int bytesWritten);

        Assert.True(success);
        Assert.Equal(2, bytesWritten);
        Assert.Equal(0xC3, utf8Buffer[0]);
        Assert.Equal(0xA9, utf8Buffer[1]);
    }
}
