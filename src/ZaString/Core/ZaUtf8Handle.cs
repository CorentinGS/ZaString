using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ZaString.Core;

/// <summary>
///     A disposable handle for a pooled UTF-8 byte buffer.
///     This struct MUST be disposed to return the buffer to the pool.
/// </summary>
public struct ZaUtf8Handle : IDisposable
{
    private byte[]? _buffer;
    private readonly ArrayPool<byte> _pool;
    private readonly int _length;

    internal ZaUtf8Handle(byte[] buffer, int length, ArrayPool<byte> pool)
    {
        _buffer = buffer;
        _length = length;
        _pool = pool;
    }

    /// <summary>
    ///     Gets the span representing the UTF-8 data, including the null terminator.
    /// </summary>
    public ReadOnlySpan<byte> Span
    {
        get
        {
            return _buffer == null ? ReadOnlySpan<byte>.Empty : new ReadOnlySpan<byte>(_buffer, 0, _length);
        }
    }

    /// <summary>
    ///     Gets a pointer to the underlying buffer.
    ///     WARNING: This pointer is valid only as long as the handle is not disposed and the underlying array is pinned (if needed).
    ///     Since this uses pooled arrays, they are pinned by default in modern .NET during usage if not moved, but exercise caution.
    ///     Actually, standard arrays are NOT pinned. Users should pin if they need a stable pointer for async/external calls.
    ///     For immediate synchronous usage (like ImGui), it is usually fine.
    /// </summary>
    public readonly unsafe byte* Pointer
    {
        get
        {
            if (_buffer == null) return null;
            return (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(_buffer));
        }
    }

    /// <summary>
    ///     Returns the underlying buffer to the pool.
    /// </summary>
    public void Dispose()
    {
        if (_buffer == null)
            return;
        _pool.Return(_buffer);
        _buffer = null;
    }
}
