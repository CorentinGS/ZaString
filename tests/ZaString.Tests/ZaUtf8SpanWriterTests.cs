using System.Globalization;
using System.Text;
using ZaString.Core;
using ZaString.Extensions;

namespace ZaString.Tests;

public class ZaUtf8SpanWriterTests
{
    [Fact]
    public void Create_WithBuffer_ReturnsWriterWithCorrectLength()
    {
        Span<byte> buffer = stackalloc byte[64];
        var writer = ZaUtf8SpanWriter.Create(buffer);

        Assert.Equal(0, writer.Length);
    }

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
    public void Append_NullString_DoesNothing()
    {
        Span<byte> buffer = stackalloc byte[32];
        var writer = ZaUtf8SpanWriter.Create(buffer);

        writer.Append((string?)null);

        Assert.Equal(0, writer.Length);
        Assert.True(writer.AsSpan().IsEmpty);
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
    public void Append_IntWithFormat_FormatsCorrectly()
    {
        Span<byte> buffer = stackalloc byte[16];
        var writer = ZaUtf8SpanWriter.Create(buffer);

        writer.Append(123, "D5");

        var expected = Encoding.UTF8.GetBytes("00123");
        Assert.True(writer.AsSpan().SequenceEqual(expected));
    }

    [Fact]
    public void Append_Long_FormatsToUtf8()
    {
        Span<byte> buffer = stackalloc byte[32];
        var writer = ZaUtf8SpanWriter.Create(buffer);

        writer.Append(123456789L);

        var expected = Encoding.UTF8.GetBytes("123456789");
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
    public void Append_DoubleWithFormat_FormatsCorrectly()
    {
        Span<byte> buffer = stackalloc byte[16];
        var writer = ZaUtf8SpanWriter.Create(buffer);

        writer.Append(3.14159, "F2");

        var expected = Encoding.UTF8.GetBytes("3.14");
        Assert.True(writer.AsSpan().SequenceEqual(expected));
    }

    [Fact]
    public void Append_DateTime_FormatsToUtf8()
    {
        Span<byte> buffer = stackalloc byte[64];
        var writer = ZaUtf8SpanWriter.Create(buffer);
        var dateTime = new DateTime(2023, 12, 25, 10, 30, 45);

        writer.Append(dateTime, "O");

        var expected = Encoding.UTF8.GetBytes(dateTime.ToString("O", CultureInfo.InvariantCulture));
        Assert.True(writer.AsSpan().SequenceEqual(expected));
    }

    [Fact]
    public void Append_DateTimeWithFormat_FormatsCorrectly()
    {
        Span<byte> buffer = stackalloc byte[64];
        var writer = ZaUtf8SpanWriter.Create(buffer);
        var dateTime = new DateTime(2023, 12, 25, 10, 30, 45);

        writer.Append(dateTime, "O");
        var expected = Encoding.UTF8.GetBytes(dateTime.ToString("O", CultureInfo.InvariantCulture));

        Assert.True(writer.AsSpan().SequenceEqual(expected));
    }

    [Fact]
    public void AppendLine_AppendsNewline()
    {
        Span<byte> buffer = stackalloc byte[32];
        var writer = ZaUtf8SpanWriter.Create(buffer);

        writer.AppendLine();

        var expected = Encoding.UTF8.GetBytes(Environment.NewLine);
        Assert.True(writer.AsSpan().SequenceEqual(expected));
    }

    [Fact]
    public void AppendLine_WithString_AppendsStringAndNewline()
    {
        Span<byte> buffer = stackalloc byte[32];
        var writer = ZaUtf8SpanWriter.Create(buffer);

        writer.AppendLine("Hello");

        var expected = Encoding.UTF8.GetBytes("Hello" + Environment.NewLine);
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
    public void AppendHex_Lowercase_FormatsBytesAsLowercaseHex()
    {
        Span<byte> buffer = stackalloc byte[16];
        var writer = ZaUtf8SpanWriter.Create(buffer);

        var data = new byte[]
        {
            0xAB,
            0xCD,
            0xEF
        };
        writer.AppendHex(data);

        var expected = Encoding.UTF8.GetBytes("abcdef");
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

    [Fact]
    public void Append_ComplexScenario_WorksCorrectly()
    {
        Span<byte> buffer = stackalloc byte[512];
        var writer = ZaUtf8SpanWriter.Create(buffer);

        writer.Append("User: ")
            .Append("John Doe")
            .Append(", ID: ")
            .Append(12345)
            .Append(", Balance: ")
            .Append(1234.56, "F2")
            .Append(", Created: ")
            .Append(new DateTime(2023, 12, 25));

        var result = writer.ToString();
        Assert.Contains("User: John Doe", result);
        Assert.Contains("ID: 12345", result);
        Assert.Contains("Balance: 1234.56", result);
    }

    [Fact]
    public void Advance_IncreasesLength()
    {
        Span<byte> buffer = stackalloc byte[32];
        var writer = ZaUtf8SpanWriter.Create(buffer);

        writer.Advance(5);

        Assert.Equal(5, writer.Length);
    }

    [Fact]
    public void Clear_ResetsLength()
    {
        Span<byte> buffer = stackalloc byte[32];
        var writer = ZaUtf8SpanWriter.Create(buffer);

        writer.Append("Hello");
        Assert.Equal(5, writer.Length);

        writer.Clear();
        Assert.Equal(0, writer.Length);
    }

    [Fact]
    public void AsSpan_ReturnsWrittenSpan()
    {
        Span<byte> buffer = stackalloc byte[32];
        var writer = ZaUtf8SpanWriter.Create(buffer);

        writer.Append("Hello");
        var span = writer.AsSpan();

        Assert.Equal(5, span.Length);
        var expected = Encoding.UTF8.GetBytes("Hello");
        Assert.True(span.SequenceEqual(expected));
    }

    [Fact]
    public void ToString_ReturnsUtf8String()
    {
        Span<byte> buffer = stackalloc byte[32];
        var writer = ZaUtf8SpanWriter.Create(buffer);

        writer.Append("Hello, World!");

        Assert.Equal("Hello, World!", writer.ToString());
    }

    [Fact]
    public void Append_BufferTooSmall_ThrowsException()
    {
        Span<byte> buffer = stackalloc byte[1];
        var writer = ZaUtf8SpanWriter.Create(buffer);

        var exceptionThrown = false;
        try
        {
            writer.Append("Hello");
        }
        catch (ArgumentOutOfRangeException ex)
        {
            exceptionThrown = true;
            Assert.Contains("destination buffer is too small", ex.Message);
        }

        Assert.True(exceptionThrown, "Expected ArgumentOutOfRangeException to be thrown");
    }

    [Fact]
    public void AppendHex_BufferTooSmall_ThrowsException()
    {
        Span<byte> buffer = stackalloc byte[1];
        var writer = ZaUtf8SpanWriter.Create(buffer);

        var data = new byte[]
        {
            0x01,
            0x02,
            0x03
        };
        var exceptionThrown = false;
        try
        {
            writer.AppendHex(data);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            exceptionThrown = true;
            Assert.Contains("destination buffer is too small", ex.Message);
        }

        Assert.True(exceptionThrown, "Expected ArgumentOutOfRangeException to be thrown");
    }

    [Fact]
    public void AppendBase64_BufferTooSmall_ThrowsException()
    {
        Span<byte> buffer = stackalloc byte[1];
        var writer = ZaUtf8SpanWriter.Create(buffer);

        var data = new byte[]
        {
            0x01,
            0x02,
            0x03,
            0x04,
            0x05,
            0x06
        };
        var exceptionThrown = false;
        try
        {
            writer.AppendBase64(data);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            exceptionThrown = true;
            Assert.Contains("destination buffer is too small", ex.Message);
        }

        Assert.True(exceptionThrown, "Expected ArgumentOutOfRangeException to be thrown");
    }

    private static void AppendToWriter(ref ZaUtf8SpanWriter writer, string value)
    {
        writer.Append(value);
    }

    private static void AppendHexToWriter(ref ZaUtf8SpanWriter writer, byte[] data)
    {
        writer.AppendHex(data);
    }

    private static void AppendBase64ToWriter(ref ZaUtf8SpanWriter writer, byte[] data)
    {
        writer.AppendBase64(data);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void Create_WithVariousBufferSizes_WorksCorrectly(int bufferSize)
    {
        var buffer = bufferSize > 0 ? stackalloc byte[bufferSize] : Span<byte>.Empty;
        var writer = ZaUtf8SpanWriter.Create(buffer);

        Assert.Equal(0, writer.Length);
    }

    [Fact]
    public void RemainingSpan_ReturnsCorrectSpan()
    {
        Span<byte> buffer = stackalloc byte[32];
        var writer = ZaUtf8SpanWriter.Create(buffer);

        writer.Append("Hello");
        var remaining = writer.RemainingSpan;

        Assert.Equal(27, remaining.Length); // 32 - 5 (Hello)
    }
}