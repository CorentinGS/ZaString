using System.Text;
using ZaString.Core;

namespace ZaString.Tests;

public class ZaPooledStringBuilderUtf8Tests
{
    [Fact]
    public void ToUtf8NullTerminated_ReturnsCorrectBytes()
    {
        using var builder = ZaPooledStringBuilder.Rent();
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
        using var builder = ZaPooledStringBuilder.Rent();

        using var handle = builder.ToUtf8NullTerminated();
        var span = handle.Span;

        Assert.Equal(1, span.Length);
        Assert.Equal(0, span[0]);
    }

    [Fact]
    public void ToUtf8NullTerminated_SpecialCharacters_ReturnsCorrectBytes()
    {
        using var builder = ZaPooledStringBuilder.Rent();
        builder.Append("Héllo Wörld €");

        using var handle = builder.ToUtf8NullTerminated();
        var span = handle.Span;

        // "Héllo Wörld €"
        // é is 2 bytes (C3 A9)
        // ö is 2 bytes (C3 B6)
        // € is 3 bytes (E2 82 AC)
        // H, l, l, o,  , W, r, l, d,  are 1 byte each (10 chars)
        // Total bytes = 10 + 2 + 2 + 3 = 17 bytes
        // + 1 null terminator = 18 bytes

        Assert.Equal(18, span.Length);
        Assert.Equal(0, span[17]);

        var str = Encoding.UTF8.GetString(span[..17]);
        Assert.Equal("Héllo Wörld €", str);
    }

    [Fact]
    public void TryToUtf8NullTerminated_WritesToBuffer_WhenBufferIsLargeEnough()
    {
        using var builder = ZaPooledStringBuilder.Rent();
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
        using var builder = ZaPooledStringBuilder.Rent();
        builder.Append("Hello");

        Span<byte> buffer = stackalloc byte[5]; // Too small (needs 6)
        var success = builder.TryToUtf8NullTerminated(buffer, out var bytesWritten);

        Assert.False(success);
        Assert.Equal(0, bytesWritten);
    }

    [Fact]
    public unsafe void TryToUtf8NullTerminated_WritesToPointer_WhenBufferIsLargeEnough()
    {
        using var builder = ZaPooledStringBuilder.Rent();
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

    [Fact]
    public unsafe void ZaUtf8Handle_Pointer_ReturnsValidPointer()
    {
        using var builder = ZaPooledStringBuilder.Rent();
        builder.Append("Test");

        using var handle = builder.ToUtf8NullTerminated();
        var ptr = handle.Pointer;

        Assert.True(ptr != null);
        Assert.Equal((byte)'T', *ptr);
        Assert.Equal((byte)'e', *(ptr + 1));
        Assert.Equal((byte)'s', *(ptr + 2));
        Assert.Equal((byte)'t', *(ptr + 3));
        Assert.Equal(0, *(ptr + 4));
    }
}
