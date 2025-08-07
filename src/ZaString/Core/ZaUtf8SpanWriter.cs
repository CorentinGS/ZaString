using System.Diagnostics;
using System.Text;

namespace ZaString.Core;

/// <summary>
///     A zero-allocation UTF-8 writer that writes directly to a provided Span&lt;byte&gt;.
///     This is a ref struct to ensure it is only allocated on the stack.
/// </summary>
public ref struct ZaUtf8SpanWriter
{
    private readonly Span<byte> _buffer;
    public int Length { get; private set; }

    private readonly int Capacity
    {
        get => _buffer.Length;
    }

    public readonly ReadOnlySpan<byte> WrittenSpan
    {
        get => _buffer[..Length];
    }

    public readonly Span<byte> RemainingSpan
    {
        get => _buffer[Length..];
    }

    private ZaUtf8SpanWriter(Span<byte> buffer)
    {
        _buffer = buffer;
        Length = 0;
    }

    public static ZaUtf8SpanWriter Create(Span<byte> buffer)
    {
        return new ZaUtf8SpanWriter(buffer);
    }

    public void Advance(int count)
    {
        Debug.Assert(count >= 0, "Advance count must be non-negative.");
        Debug.Assert(Length + count <= Capacity, "Advance would exceed capacity.");
        Length += count;
    }

    public void Clear()
    {
        Length = 0;
    }

    public readonly ReadOnlySpan<byte> AsSpan()
    {
        return WrittenSpan;
    }

    public override readonly string ToString()
    {
        return Encoding.UTF8.GetString(WrittenSpan);
    }
}