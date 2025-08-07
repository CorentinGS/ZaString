using System.Buffers;
using System.Globalization;

namespace ZaString.Core;

/// <summary>
/// A growable, pooled string builder that minimizes allocations by renting buffers from ArrayPool.
/// </summary>
public sealed class ZaPooledStringBuilder : IDisposable
{
    private char[] _buffer;
    private int _length;
    private readonly ArrayPool<char> _pool;
    private bool _disposed;

    private ZaPooledStringBuilder(ArrayPool<char> pool, int initialCapacity)
    {
        _pool = pool;
        _buffer = pool.Rent(Math.Max(1, initialCapacity));
        _length = 0;
    }

    public static ZaPooledStringBuilder Rent(int initialCapacity = 256, ArrayPool<char>? pool = null)
    {
        return new ZaPooledStringBuilder(pool ?? ArrayPool<char>.Shared, initialCapacity);
    }

    public int Length => _length;
    public int Capacity => _buffer.Length;

    public ReadOnlySpan<char> AsSpan() => _buffer.AsSpan(0, _length);

    public override string ToString() => new string(_buffer, 0, _length);

    public void Clear() => _length = 0;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        var buf = _buffer;
        _buffer = Array.Empty<char>();
        _pool.Return(buf);
    }

    private void EnsureCapacity(int additionalRequired)
    {
        if (additionalRequired < 0) throw new ArgumentOutOfRangeException(nameof(additionalRequired));
        var required = _length + additionalRequired;
        if (required <= _buffer.Length) return;

        var newCapacity = _buffer.Length;
        if (newCapacity == 0) newCapacity = 1;
        while (newCapacity < required)
        {
            newCapacity = newCapacity * 2;
        }

        var newBuffer = _pool.Rent(newCapacity);
        _buffer.AsSpan(0, _length).CopyTo(newBuffer);
        _pool.Return(_buffer);
        _buffer = newBuffer;
    }

    public ZaPooledStringBuilder Append(ReadOnlySpan<char> value)
    {
        EnsureCapacity(value.Length);
        value.CopyTo(_buffer.AsSpan(_length));
        _length += value.Length;
        return this;
    }

    public ZaPooledStringBuilder Append(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            Append(value.AsSpan());
        }
        return this;
    }

    public ZaPooledStringBuilder Append(char value)
    {
        EnsureCapacity(1);
        _buffer[_length++] = value;
        return this;
    }

    public ZaPooledStringBuilder Append(bool value)
    {
        return Append(value ? "true" : "false");
    }

    public ZaPooledStringBuilder Append<T>(T value, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : ISpanFormattable
    {
        provider ??= CultureInfo.InvariantCulture;
        while (true)
        {
            if (value.TryFormat(_buffer.AsSpan(_length), out var written, format, provider))
            {
                _length += written;
                return this;
            }
            // Grow and retry
            EnsureCapacity(Math.Max(1, _buffer.Length));
        }
    }

    public ZaPooledStringBuilder AppendLine()
    {
        return Append(Environment.NewLine);
    }

    public ZaPooledStringBuilder AppendLine(string? value)
    {
        if (value is not null)
        {
            Append(value);
        }
        return AppendLine();
    }
}

