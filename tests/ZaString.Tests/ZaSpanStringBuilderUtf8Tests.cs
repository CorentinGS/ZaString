using System.Text;
using ZaString.Core;
using ZaString.Extensions;

namespace ZaString.Tests;

public class ZaSpanStringBuilderUtf8Tests
{
    [Fact]
    public void ToUtf8NullTerminated_ReturnsCorrectBytes()
    {
        Span<char> buffer = stackalloc char[128];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.Append("Hello World");

        using var handle = builder.ToUtf8NullTerminated();
        var span = handle.Span;

        Assert.Equal(12, span.Length); // 11 chars + 1 null terminator
        Assert.Equal(0, span[11]); // Null terminator

        var str = Encoding.UTF8.GetString(span[..11]);
        Assert.Equal("Hello World", str);
    }

    [Fact]
    public void ToUtf8NullTerminated_EmptyString_ReturnsNullTerminatorOnly()
    {
        Span<char> buffer = stackalloc char[128];
        var builder = ZaSpanStringBuilder.Create(buffer);

        using var handle = builder.ToUtf8NullTerminated();
        var span = handle.Span;

        Assert.Equal(1, span.Length);
        Assert.Equal(0, span[0]);
    }

    [Fact]
    public void ToUtf8NullTerminated_SpecialCharacters_ReturnsCorrectBytes()
    {
        Span<char> buffer = stackalloc char[128];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.Append("Héllo Wörld €");

        using var handle = builder.ToUtf8NullTerminated();
        var span = handle.Span;

        Assert.Equal(18, span.Length);
        Assert.Equal(0, span[17]);

        var str = Encoding.UTF8.GetString(span[..17]);
        Assert.Equal("Héllo Wörld €", str);
    }

    [Fact]
    public void TryToUtf8NullTerminated_WritesToBuffer_WhenBufferIsLargeEnough()
    {
        Span<char> charBuffer = stackalloc char[128];
        var builder = ZaSpanStringBuilder.Create(charBuffer);
        builder.Append("Hello");

        Span<byte> buffer = stackalloc byte[10];
        var success = builder.TryToUtf8NullTerminated(buffer, out var bytesWritten);

        Assert.True(success);
        Assert.Equal(6, bytesWritten); // 5 chars + 1 null terminator
        Assert.Equal((byte)'H', buffer[0]);
        Assert.Equal((byte)'e', buffer[1]);
        Assert.Equal((byte)'l', buffer[2]);
        Assert.Equal((byte)'l', buffer[3]);
        Assert.Equal((byte)'o', buffer[4]);
        Assert.Equal(0, buffer[5]);
    }

    [Fact]
    public void TryToUtf8NullTerminated_ReturnsFalse_WhenBufferIsTooSmall()
    {
        Span<char> charBuffer = stackalloc char[128];
        var builder = ZaSpanStringBuilder.Create(charBuffer);
        builder.Append("Hello");

        Span<byte> buffer = stackalloc byte[5]; // Too small (needs 6)
        var success = builder.TryToUtf8NullTerminated(buffer, out var bytesWritten);

        Assert.False(success);
        Assert.Equal(0, bytesWritten);
    }

    [Fact]
    public unsafe void TryToUtf8NullTerminated_WritesToPointer_WhenBufferIsLargeEnough()
    {
        Span<char> charBuffer = stackalloc char[128];
        var builder = ZaSpanStringBuilder.Create(charBuffer);
        builder.Append("Hello");

        Span<byte> buffer = stackalloc byte[10];
        fixed (byte* ptr = buffer)
        {
            var success = builder.TryToUtf8NullTerminated(ptr, 10, out var bytesWritten);

            Assert.True(success);
            Assert.Equal(6, bytesWritten);
            Assert.Equal((byte)'H', buffer[0]);
            Assert.Equal(0, buffer[5]);
        }
    }
}
