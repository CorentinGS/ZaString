using System.Buffers;
using System.Globalization;
using System.Text;

namespace ZaString.Core;

/// <summary>
/// A zero-allocation UTF-8 writer that writes directly to a provided Span&lt;byte&gt;.
/// This is a ref struct to ensure it is only allocated on the stack.
/// </summary>
public ref struct ZaUtf8SpanWriter
{
    private readonly Span<byte> _buffer;
    public int Length { get; private set; }
    public int Capacity => _buffer.Length;
    public ReadOnlySpan<byte> WrittenSpan => _buffer[..Length];
    public Span<byte> RemainingSpan => _buffer[Length..];

    private ZaUtf8SpanWriter(Span<byte> buffer)
    {
        _buffer = buffer;
        Length = 0;
    }

    public static ZaUtf8SpanWriter Create(Span<byte> buffer) => new(buffer);

    public void Advance(int count)
    {
        System.Diagnostics.Debug.Assert(count >= 0, "Advance count must be non-negative.");
        System.Diagnostics.Debug.Assert(Length + count <= Capacity, "Advance would exceed capacity.");
        Length += count;
    }

    public void Clear() => Length = 0;

    public ReadOnlySpan<byte> AsSpan() => WrittenSpan;

    public override string ToString() => Encoding.UTF8.GetString(WrittenSpan);
} 