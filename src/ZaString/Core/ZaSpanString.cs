using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ZaString.Core;

/// <summary>
///     A zero-allocation string builder that writes directly to a provided Span
///     <char>
///         .
///         This is a ref struct to ensure it is only allocated on the stack.
///         Append operations are provided as extension methods to allow for a fluent, chainable API.
/// </summary>
public ref struct ZaSpanStringBuilder
{
    private readonly Span<char> _buffer;

    /// <summary>
    ///     Gets the portion of the buffer that has been written to.
    /// </summary>
    public readonly ReadOnlySpan<char> WrittenSpan
    {
        get => _buffer[..Length];
    }

    /// <summary>
    ///     Gets the remaining, unused portion of the buffer.
    /// </summary>
    public readonly Span<char> RemainingSpan
    {
        get => _buffer[Length..];
    }

    /// <summary>
    ///     Gets the current length of the built string.
    /// </summary>
    public int Length { get; private set; }

    /// <summary>
    ///     Gets the total capacity of the underlying buffer.
    /// </summary>
    public readonly int Capacity
    {
        get => _buffer.Length;
    }

    /// <summary>
    ///     Gets or sets the character at the specified index within the written portion of the buffer.
    ///     This allows for modification of characters that have already been written.
    /// </summary>
    /// <param name="index">The zero-based index of the character to access.</param>
    /// <returns>A reference to the character at the specified index.</returns>
    /// <exception cref="IndexOutOfRangeException">
    ///     Thrown when the index is negative or greater than or equal to the current
    ///     length.
    /// </exception>
    public readonly ref char this[int index]
    {
        get
        {
            if ((uint)index >= (uint)Length)
                throw new IndexOutOfRangeException();
            return ref _buffer[index];
        }
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ZaSpanStringBuilder" /> struct.
    /// </summary>
    /// <param name="buffer">The character buffer to write into.</param>
    private ZaSpanStringBuilder(Span<char> buffer)
    {
        _buffer = buffer;
        Length = 0;
    }

    /// <summary>
    ///     Creates a new <see cref="ZaSpanStringBuilder" /> instance with the provided buffer.
    /// </summary>
    /// <param name="buffer">The buffer to be used for building the string.</param>
    /// <returns>A new <see cref="ZaSpanStringBuilder" /> instance.</returns>
    public static ZaSpanStringBuilder Create(Span<char> buffer)
    {
        return new ZaSpanStringBuilder(buffer);
    }

    /// <summary>
    ///     Creates a new <see cref="ZaSpanStringBuilder" /> instance with the provided pointer and length.
    ///     This overload is unsafe and allows for pointer-based operations.
    /// </summary>
    /// <param name="ptr">The pointer to the character buffer.</param>
    /// <param name="length">The length of the buffer.</param>
    /// <returns>A new <see cref="ZaSpanStringBuilder" /> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when length is negative.</exception>
    /// <exception cref="ArgumentNullException">Thrown when ptr is null and length is not zero.</exception>
    public static unsafe ZaSpanStringBuilder Create(char* ptr, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        if (ptr == null && length != 0) throw new ArgumentNullException(nameof(ptr));
        return new ZaSpanStringBuilder(new Span<char>(ptr, length));
    }

    /// <summary>
    ///     Advances the write position in the buffer.
    ///     This should be called by append-like operations after writing to the RemainingSpan.
    /// </summary>
    /// <param name="count">The number of characters written.</param>
    public void Advance(int count)
    {
        Debug.Assert(count >= 0, "Advance count must be non-negative.");
        Debug.Assert(Length + count <= Capacity, "Advance would exceed capacity.");
        Length += count;
    }

    /// <summary>
    ///     Resets the builder to an empty state, allowing the buffer to be reused.
    ///     This does not clear the underlying buffer content, only resets the write position.
    /// </summary>
    public void Clear()
    {
        Length = 0;
    }

    /// <summary>
    ///     Reduces the current length to the specified value. Only truncation is allowed.
    /// </summary>
    /// <param name="newLength">The new length, which must be between 0 and the current Length.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if newLength is negative or greater than the current Length.</exception>
    public void SetLength(int newLength)
    {
        if ((uint)newLength > (uint)Length)
            throw new ArgumentOutOfRangeException(nameof(newLength));

        Length = newLength;
    }

    /// <summary>
    ///     Removes the last <paramref name="count" /> characters from the written span.
    /// </summary>
    /// <param name="count">Number of characters to remove. Must be between 0 and current Length.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if count is negative or greater than current Length.</exception>
    public void RemoveLast(int count)
    {
        if ((uint)count > (uint)Length)
            throw new ArgumentOutOfRangeException(nameof(count));

        Length -= count;
    }

    /// <summary>
    ///     Returns the built string as a <see cref="ReadOnlySpan{Char}" />.
    /// </summary>
    /// <returns>A read-only span representing the characters written to the buffer.</returns>
    public readonly ReadOnlySpan<char> AsSpan()
    {
        return WrittenSpan;
    }

    /// <summary>
    ///     Returns the built string as a new <see cref="string" /> instance.
    ///     Note: This method allocates a new string.
    /// </summary>
    /// <returns>A new string containing the characters written to the buffer.</returns>
    public override readonly string ToString()
    {
        return new string(WrittenSpan);
    }

    /// <summary>
    ///     Returns the built content as a read-only span of bytes representing the UTF-16 little-endian code units.
    ///     This is a non-allocating view over the same memory as <see cref="WrittenSpan" />.
    /// </summary>
    /// <remarks>
    ///     The resulting bytes are the raw in-memory representation of the <see cref="char" /> data (UTF-16 code units),
    ///     not an encoded form like UTF-8. Use higher-level encoding APIs if you need UTF-8 bytes.
    /// </remarks>
    public readonly ReadOnlySpan<byte> AsByteSpan()
    {
        return MemoryMarshal.AsBytes(WrittenSpan);
    }

    /// <summary>
    ///     Copies the built content into a newly allocated <see cref="byte" /> array representing UTF-16 little-endian code
    ///     units.
    /// </summary>
    /// <returns>A new byte array whose length is <c>Length * sizeof(char)</c>.</returns>
    public readonly byte[] ToByteArray()
    {
        if (Length == 0)
        {
            return [];
        }

        var sourceBytes = MemoryMarshal.AsBytes(WrittenSpan);
        var result = new byte[sourceBytes.Length];
        sourceBytes.CopyTo(result);
        return result;
    }

    /// <summary>
    ///     Returns a pointer to the underlying character buffer.
    ///     This method is unsafe and should be used with caution.
    /// </summary>
    /// <returns>A pointer to the character buffer, or null if the length is zero.</returns>
    public unsafe readonly char* GetCharPointer()
    {
        if (Length == 0) return null;
        ref var r = ref MemoryMarshal.GetReference(WrittenSpan);
        return (char*)Unsafe.AsPointer(ref r);
    }

    /// <summary>
    ///     Returns a pointer to the underlying byte buffer representing the UTF-16 little-endian code units.
    ///     This method is unsafe and should be used with caution.
    /// </summary>
    /// <returns>A pointer to the byte buffer, or null if the length is zero.</returns>
    public unsafe readonly byte* GetBytePointer()
    {
        if (Length == 0) return null;
        var byteSpan = MemoryMarshal.AsBytes(WrittenSpan);
        ref var r = ref MemoryMarshal.GetReference(byteSpan);
        return (byte*)Unsafe.AsPointer(ref r);
    }
}