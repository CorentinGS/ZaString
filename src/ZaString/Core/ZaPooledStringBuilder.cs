using System.Buffers;
using System.Globalization;
using System.Text;
using ZaString.Escaping;

namespace ZaString.Core;

/// <summary>
///     A growable, pooled string builder that minimizes allocations by renting buffers from ArrayPool.
/// </summary>
public sealed class ZaPooledStringBuilder : IDisposable
{
    private readonly ArrayPool<char> _pool;
    private char[] _buffer;
    private bool _disposed;

    private ZaPooledStringBuilder(ArrayPool<char> pool, int initialCapacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(initialCapacity);
        _pool = pool;
        _buffer = pool.Rent(Math.Max(1, initialCapacity));
        Length = 0;
    }

    public int Length { get; private set; }

    public int Capacity
    {
        get
        {
            ThrowIfDisposed();
            return _buffer.Length;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        var buf = _buffer;
        _buffer = [];
        _pool.Return(buf);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ZaPooledStringBuilder));
    }

    public static ZaPooledStringBuilder Rent(int initialCapacity = 256, ArrayPool<char>? pool = null)
    {
        return new ZaPooledStringBuilder(pool ?? ArrayPool<char>.Shared, initialCapacity);
    }

    public ReadOnlySpan<char> AsSpan()
    {
        ThrowIfDisposed();
        return _buffer.AsSpan(0, Length);
    }

    public override string ToString()
    {
        ThrowIfDisposed();
        return new string(_buffer, 0, Length);
    }

    public void Clear()
    {
        ThrowIfDisposed();
        Length = 0;
    }

    /// <summary>
    ///     Sets the length to the specified value. Only truncation is allowed.
    /// </summary>
    public void SetLength(int newLength)
    {
        ThrowIfDisposed();
        if ((uint)newLength > (uint)Length)
            throw new ArgumentOutOfRangeException(nameof(newLength));

        Length = newLength;
    }

    /// <summary>
    ///     Removes the last <paramref name="count" /> characters.
    /// </summary>
    public void RemoveLast(int count)
    {
        ThrowIfDisposed();
        if ((uint)count > (uint)Length)
            throw new ArgumentOutOfRangeException(nameof(count));

        Length -= count;
    }

    /// <summary>
    ///     Reserves a writable span of the specified size, growing the rented buffer if needed.
    ///     Call <see cref="Advance" /> with the number of characters written to commit the append.
    /// </summary>
    public ZaPooledStringBuilder GetAppendSpan(int size, out Span<char> writeSpan)
    {
        ThrowIfDisposed();
        ArgumentOutOfRangeException.ThrowIfNegative(size);

        EnsureCapacity(size);
        writeSpan = _buffer.AsSpan(Length, size);
        return this;
    }

    public void Advance(int count)
    {
        ThrowIfDisposed();
        if ((uint)count > (uint)(_buffer.Length - Length))
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        Length += count;
    }

    /// <summary>
    ///     Gets or sets the character at the specified index.
    /// </summary>
    public char this[int index]
    {
        get
        {
            ThrowIfDisposed();
            if ((uint)index >= (uint)Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _buffer[index];
        }
        set
        {
            ThrowIfDisposed();
            if ((uint)index >= (uint)Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            _buffer[index] = value;
        }
    }

    /// <summary>
    ///     Attempts to append a read-only span without throwing.
    /// </summary>
    public bool TryAppend(ReadOnlySpan<char> value)
    {
        ThrowIfDisposed();

        if (value.Length > Array.MaxLength - Length)
        {
            return false;
        }

        try
        {
            EnsureCapacity(value.Length);
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }

        value.CopyTo(_buffer.AsSpan(Length));
        Length += value.Length;
        return true;
    }

    /// <summary>
    ///     Attempts to append a string without throwing.
    /// </summary>
    public bool TryAppend(string? value)
    {
        return value is null || TryAppend(value.AsSpan());
    }

    /// <summary>
    ///     Attempts to append a single character without throwing.
    /// </summary>
    public bool TryAppend(char value)
    {
        ThrowIfDisposed();

        if (Length >= Array.MaxLength)
        {
            return false;
        }

        EnsureCapacity(1);
        _buffer[Length++] = value;
        return true;
    }

    private void EnsureCapacity(int additionalRequired)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(additionalRequired);

        if (Length > Array.MaxLength - additionalRequired)
            throw new ArgumentOutOfRangeException(nameof(additionalRequired), "Required capacity exceeds maximum array length.");

        var required = Length + additionalRequired;
        if (required <= _buffer.Length) return;

        var newCapacity = ComputeExpandedCapacity(_buffer.Length, required);

        var newBuffer = _pool.Rent(newCapacity);
        _buffer.AsSpan(0, Length).CopyTo(newBuffer);
        _pool.Return(_buffer);
        _buffer = newBuffer;
    }

    private static int ComputeExpandedCapacity(int currentCapacity, int required)
    {
        var grown = currentCapacity <= Array.MaxLength - (currentCapacity / 2)
            ? currentCapacity + (currentCapacity / 2)
            : Array.MaxLength;

        var newCapacity = Math.Max(required, grown);
        if (newCapacity < 256)
        {
            newCapacity = 256;
        }

        return Math.Min(newCapacity, Array.MaxLength);
    }

    public ZaPooledStringBuilder Append(ReadOnlySpan<char> value)
    {
        ThrowIfDisposed();
        EnsureCapacity(value.Length);
        value.CopyTo(_buffer.AsSpan(Length));
        Length += value.Length;
        return this;
    }

    public ZaPooledStringBuilder Append(string? value)
    {
        ThrowIfDisposed();
        if (!string.IsNullOrEmpty(value))
        {
            Append(value.AsSpan());
        }

        return this;
    }

    public ZaPooledStringBuilder Append(char value)
    {
        ThrowIfDisposed();
        EnsureCapacity(1);
        _buffer[Length++] = value;
        return this;
    }

    public ZaPooledStringBuilder Append(bool value)
    {
        ThrowIfDisposed();
        return Append(value ? "true" : "false");
    }

    public ZaPooledStringBuilder AppendJsonEscaped(ReadOnlySpan<char> value)
    {
        var required = JsonEscapeStrategy.GetEscapedLength(value);
        GetAppendSpan(required, out var destination);
        JsonEscapeStrategy.TryEscape(value, destination, out var written);
        Advance(written);
        return this;
    }

    public ZaPooledStringBuilder AppendHtmlEscaped(ReadOnlySpan<char> value)
    {
        var required = HtmlEscapeStrategy.GetEscapedLength(value);
        GetAppendSpan(required, out var destination);
        HtmlEscapeStrategy.TryEscape(value, destination, out var written);
        Advance(written);
        return this;
    }

    public ZaPooledStringBuilder AppendCsvEscaped(ReadOnlySpan<char> value)
    {
        var required = CsvEscapeStrategy.GetEscapedLength(value);
        GetAppendSpan(required, out var destination);
        CsvEscapeStrategy.TryEscape(value, destination, out var written);
        Advance(written);
        return this;
    }

    public ZaPooledStringBuilder Append<T>(T value, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : ISpanFormattable
    {
        ThrowIfDisposed();
        provider ??= CultureInfo.InvariantCulture;

        while (true)
        {
            if (value.TryFormat(_buffer.AsSpan(Length), out var written, format, provider))
            {
                Length += written;
                return this;
            }

            var remaining = _buffer.Length - Length;
            var growBy = remaining + 1;
            EnsureCapacity(growBy);
        }
    }

    public ZaPooledStringBuilder AppendLine()
    {
        ThrowIfDisposed();
        return Append(Environment.NewLine);
    }

    public ZaPooledStringBuilder AppendLine(string? value)
    {
        ThrowIfDisposed();
        if (value is not null)
        {
            Append(value);
        }

        return AppendLine();
    }

    public ZaUtf8Handle ToUtf8NullTerminated()
    {
        ThrowIfDisposed();
        var span = AsSpan();
        var byteCount = Encoding.UTF8.GetByteCount(span);

        var bytePool = ArrayPool<byte>.Shared;
        var byteBuffer = bytePool.Rent(byteCount + 1);

        Encoding.UTF8.TryGetBytes(span, byteBuffer, out var bytesWritten);
        byteBuffer[bytesWritten] = 0;

        return new ZaUtf8Handle(byteBuffer, bytesWritten + 1, bytePool);
    }

    public bool TryToUtf8NullTerminated(Span<byte> destination, out int bytesWritten)
    {
        ThrowIfDisposed();
        var span = AsSpan();
        var byteCount = Encoding.UTF8.GetByteCount(span);
        var required = byteCount + 1;

        if (destination.Length < required)
        {
            bytesWritten = 0;
            return false;
        }

        Encoding.UTF8.TryGetBytes(span, destination, out bytesWritten);
        destination[bytesWritten] = 0;
        bytesWritten++;
        return true;
    }

    public unsafe bool TryToUtf8NullTerminated(byte* buffer, int length, out int bytesWritten)
    {
        ThrowIfDisposed();
        if (length < 0)
        {
            bytesWritten = 0;
            return false;
        }
        if (buffer == null)
        {
            bytesWritten = 0;
            return false;
        }

        return TryToUtf8NullTerminated(new Span<byte>(buffer, length), out bytesWritten);
    }
}
