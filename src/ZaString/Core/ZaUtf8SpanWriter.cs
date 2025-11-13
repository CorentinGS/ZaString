using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

    public static unsafe ZaUtf8SpanWriter Create(byte* ptr, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        if (ptr == null && length != 0) throw new ArgumentNullException(nameof(ptr));
        return new ZaUtf8SpanWriter(new Span<byte>(ptr, length));
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

    public unsafe readonly byte* GetBytePointer()
    {
        if (Length == 0) return null;
        ref var r = ref MemoryMarshal.GetReference(WrittenSpan);
        return (byte*)Unsafe.AsPointer(ref r);
    }

    public override readonly string ToString()
    {
        return Encoding.UTF8.GetString(WrittenSpan);
    }
}