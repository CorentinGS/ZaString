using System;
using System.Text;
using Xunit;
using ZaString.Core;
using ZaString.Extensions;

namespace ZaString.Tests;

public class ZaUtf8SpanWriterUnsafeTests
{
    [Fact]
    public unsafe void Create_FromPointer_WritesUtf8()
    {
        const string value = "Hello";
        var arr = new byte[32];
        fixed (byte* p = arr)
        {
            var writer = ZaUtf8SpanWriter.Create(p, arr.Length);
            var utf8 = Encoding.UTF8.GetBytes(value);
            if (utf8.Length > writer.RemainingSpan.Length)
            {
                throw new Exception("Test buffer too small");
            }
            utf8.CopyTo(writer.RemainingSpan);
            writer.Advance(utf8.Length);
            Assert.Equal(utf8.Length, writer.Length);
            Assert.Equal(utf8, arr.AsSpan(0, utf8.Length).ToArray());
        }
    }

    [Fact]
    public unsafe void GetBytePointer_ReturnsWrittenBytes()
    {
        const string value = "✅"; // multi-byte UTF-8
        var arr = new byte[16];
        fixed (byte* p = arr)
        {
            var writer = ZaUtf8SpanWriter.Create(p, arr.Length);
            var utf8 = Encoding.UTF8.GetBytes(value);
            utf8.CopyTo(writer.RemainingSpan);
            writer.Advance(utf8.Length);
            var ptr = writer.GetBytePointer();
            for (var i = 0; i < writer.Length; i++) Assert.Equal(utf8[i], ptr[i]);
        }
    }

    [Fact]
    public unsafe void EmptyWriter_GetBytePointer_Null()
    {
        var writer = ZaUtf8SpanWriter.Create(Span<byte>.Empty);
        var ptr = writer.GetBytePointer();
        Assert.True(ptr == null);
    }
}
