namespace ZaString.Core;

/// <summary>
/// A zero-allocation string builder that writes directly to a provided Span<char>.
/// This is a ref struct to ensure it is only allocated on the stack.
/// Append operations are provided as extension methods to allow for a fluent, chainable API.
/// </summary>
public ref struct ZaSpanStringBuilder
{
    private readonly Span<char> _buffer;
    private int _position;

    /// <summary>
    /// Gets the portion of the buffer that has been written to.
    /// </summary>
    public ReadOnlySpan<char> WrittenSpan
    {
        get => _buffer[.._position];
    }

    /// <summary>
    /// Gets the remaining, unused portion of the buffer.
    /// </summary>
    public Span<char> RemainingSpan
    {
        get => _buffer[_position..];
    }

    /// <summary>
    /// Gets the current length of the built string.
    /// </summary>
    public int Length
    {
        get => _position;
    }

    /// <summary>
    /// Gets the total capacity of the underlying buffer.
    /// </summary>
    public int Capacity
    {
        get => _buffer.Length;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ZaSpanStringBuilder"/> struct.
    /// </summary>
    /// <param name="buffer">The character buffer to write into.</param>
    private ZaSpanStringBuilder(Span<char> buffer)
    {
        _buffer = buffer;
        _position = 0;
    }

    /// <summary>
    /// Creates a new <see cref="ZaSpanStringBuilder"/> instance with the provided buffer.
    /// </summary>
    /// <param name="buffer">The buffer to be used for building the string.</param>
    /// <returns>A new <see cref="ZaSpanStringBuilder"/> instance.</returns>
    public static ZaSpanStringBuilder Create(Span<char> buffer)
    {
        return new ZaSpanStringBuilder(buffer);
    }

    /// <summary>
    /// Advances the write position in the buffer.
    /// This should be called by append-like operations after writing to the RemainingSpan.
    /// </summary>
    /// <param name="count">The number of characters written.</param>
    public void Advance(int count)
    {
        _position += count;
    }

    /// <summary>
    /// Returns the built string as a <see cref="ReadOnlySpan{Char}"/>.
    /// </summary>
    /// <returns>A read-only span representing the characters written to the buffer.</returns>
    public ReadOnlySpan<char> AsSpan()
    {
        return WrittenSpan;
    }

    /// <summary>
    /// Returns the built string as a new <see cref="string"/> instance.
    /// Note: This method allocates a new string.
    /// </summary>
    /// <returns>A new string containing the characters written to the buffer.</returns>
    public override string ToString()
    {
        return new string(WrittenSpan);
    }
}