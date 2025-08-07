using System.Text;
using ZaString.Core;
using ZaString.Extensions;

namespace ZaString.Tests;

public class ZaUtf8SpanWriterTests
{
    [Fact]
    public void Append_String_EncodesToUtf8()
    {
        Span<byte> buffer = stackalloc byte[32];
        var writer = ZaUtf8SpanWriter.Create(buffer);

        writer.Append("Hello");

        var expected = Encoding.UTF8.GetBytes("Hello");
        Assert.True(writer.AsSpan().SequenceEqual(expected));
    }

    [Fact]
    public void Append_Char_EncodesToUtf8()
    {
        Span<byte> buffer = stackalloc byte[8];
        var writer = ZaUtf8SpanWriter.Create(buffer);

        writer.Append('A');

        var expected = Encoding.UTF8.GetBytes("A");
        Assert.True(writer.AsSpan().SequenceEqual(expected));
    }

    [Fact]
    public void Append_Int_FormatsToUtf8()
    {
        Span<byte> buffer = stackalloc byte[16];
        var writer = ZaUtf8SpanWriter.Create(buffer);

        writer.Append(123);

        var expected = Encoding.UTF8.GetBytes("123");
        Assert.True(writer.AsSpan().SequenceEqual(expected));
    }

    [Fact]
    public void Append_Double_FormatsToUtf8()
    {
        Span<byte> buffer = stackalloc byte[16];
        var writer = ZaUtf8SpanWriter.Create(buffer);

        writer.Append(3.14);

        var expected = Encoding.UTF8.GetBytes("3.14");
        Assert.True(writer.AsSpan().SequenceEqual(expected));
    }

    [Fact]
    public void AppendHex_FormatsBytesAsHex()
    {
        Span<byte> buffer = stackalloc byte[16];
        var writer = ZaUtf8SpanWriter.Create(buffer);

        var data = new byte[]
        {
            0xAB,
            0xCD,
            0xEF
        };
        writer.AppendHex(data, true);

        var expected = Encoding.UTF8.GetBytes("ABCDEF");
        Assert.True(writer.AsSpan().SequenceEqual(expected));
    }

    [Fact]
    public void AppendBase64_EncodesBytes()
    {
        Span<byte> buffer = stackalloc byte[32];
        var writer = ZaUtf8SpanWriter.Create(buffer);

        var data = new byte[]
        {
            0x01,
            0x02,
            0x03
        };
        writer.AppendBase64(data);

        var expected = Convert.ToBase64String(data);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        Assert.True(writer.AsSpan().SequenceEqual(expectedBytes));
    }
}