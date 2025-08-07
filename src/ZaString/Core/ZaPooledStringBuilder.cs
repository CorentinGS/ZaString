using System.Buffers;
using System.Globalization;

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
        _pool = pool;
        _buffer = pool.Rent(Math.Max(1, initialCapacity));
        Length = 0;
    }

    public int Length { get; private set; }

    public int Capacity
    {
        get => _buffer.Length;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        var buf = _buffer;
        _buffer = [];
        _pool.Return(buf);
    }

    public static ZaPooledStringBuilder Rent(int initialCapacity = 256, ArrayPool<char>? pool = null)
    {
        return new ZaPooledStringBuilder(pool ?? ArrayPool<char>.Shared, initialCapacity);
    }

    public ReadOnlySpan<char> AsSpan()
    {
        return _buffer.AsSpan(0, Length);
    }

    public override string ToString()
    {
        return new string(_buffer, 0, Length);
    }

    public void Clear()
    {
        Length = 0;
    }

    private void EnsureCapacity(int additionalRequired)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(additionalRequired);
        
        var required = Length + additionalRequired;
        if (required <= _buffer.Length) return;

        var newCapacity = _buffer.Length;
        if (newCapacity == 0) newCapacity = 1;
        while (newCapacity < required)
        {
            newCapacity *= 2;
        }

        var newBuffer = _pool.Rent(newCapacity);
        _buffer.AsSpan(0, Length).CopyTo(newBuffer);
        _pool.Return(_buffer);
        _buffer = newBuffer;
    }

    public ZaPooledStringBuilder Append(ReadOnlySpan<char> value)
    {
        EnsureCapacity(value.Length);
        value.CopyTo(_buffer.AsSpan(Length));
        Length += value.Length;
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
        _buffer[Length++] = value;
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
            if (value.TryFormat(_buffer.AsSpan(Length), out var written, format, provider))
            {
                Length += written;
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