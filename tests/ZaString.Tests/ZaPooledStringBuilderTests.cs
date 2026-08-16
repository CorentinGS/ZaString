using System.Buffers;
using System.Globalization;
using ZaString.Core;

namespace ZaString.Tests;

/// <summary>
/// A custom ISpanFormattable that always fails to format, used to test safety limits.
/// </summary>
public readonly struct FailingFormattable : ISpanFormattable
{
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        charsWritten = 0;
        return false;
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return string.Empty;
    }

    public override string ToString()
    {
        return string.Empty;
    }
}

file sealed class LimitedArrayPool : ArrayPool<char>
{
    private readonly int _maxCapacity;

    public LimitedArrayPool(int maxCapacity)
    {
        _maxCapacity = maxCapacity;
    }

    public override char[] Rent(int minimumLength)
    {
        if (minimumLength > _maxCapacity)
        {
            throw new InvalidOperationException("Pool capacity exceeded");
        }

        return new char[minimumLength];
    }

    public override void Return(char[] array, bool clearArray = false)
    {
    }
}

file sealed class ThrowingArrayPool : ArrayPool<char>
{
    private int _rentCount;

    public override char[] Rent(int minimumLength)
    {
        if (_rentCount++ == 0)
        {
            return new char[Math.Max(1, minimumLength)];
        }

        throw new InvalidOperationException("Pool rent failed");
    }

    public override void Return(char[] array, bool clearArray = false)
    {
    }
}


public readonly struct LargeFormattable : ISpanFormattable
{
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (destination.Length < 2_000_000)
        {
            charsWritten = 0;
            return false;
        }

        "big".AsSpan().CopyTo(destination);
        charsWritten = 3;
        return true;
    }

    public string ToString(string? format, IFormatProvider? formatProvider) => "big";
}

public class ZaPooledStringBuilderTests
{
    [Fact]
    public void Rent_WithCapacity_ReturnsBuilder()
    {
        using var builder = ZaPooledStringBuilder.Rent(128);
        Assert.Equal(0, builder.Length);
        Assert.True(builder.Capacity >= 128);
    }

    [Fact]
    public void Rent_WithDefaultCapacity_ReturnsBuilder()
    {
        using var builder = ZaPooledStringBuilder.Rent();
        Assert.Equal(0, builder.Length);
        Assert.True(builder.Capacity >= 256);
    }

    [Fact]
    public void Append_String_AppendsCorrectly()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        builder.Append("Hello");
        Assert.Equal("Hello", builder.ToString());
        Assert.Equal(5, builder.Length);
    }

    [Fact]
    public void Append_NullString_DoesNothing()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        builder.Append(null);
        Assert.Equal("", builder.ToString());
        Assert.Equal(0, builder.Length);
    }

    [Fact]
    public void Append_ReadOnlySpan_AppendsCorrectly()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        var span = "World".AsSpan();
        builder.Append(span);
        Assert.Equal("World", builder.ToString());
        Assert.Equal(5, builder.Length);
    }

    [Fact]
    public void Append_Char_AppendsCorrectly()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        builder.Append('A');
        Assert.Equal("A", builder.ToString());
        Assert.Equal(1, builder.Length);
    }

    [Fact]
    public void Append_Boolean_AppendsCorrectly()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        builder.Append(true).Append(false);
        Assert.Equal("truefalse", builder.ToString());
        Assert.Equal(9, builder.Length);
    }

    [Fact]
    public void Append_Integer_AppendsCorrectly()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        builder.Append(42).Append(-123);
        Assert.Equal("42-123", builder.ToString());
        Assert.Equal(6, builder.Length);
    }

    [Fact]
    public void Append_Double_AppendsCorrectly()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        builder.Append(3.14159);
        Assert.Equal("3.14159", builder.ToString());
    }

    [Fact]
    public void Append_DoubleWithFormat_AppendsCorrectly()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        builder.Append(3.14159, "F2");
        Assert.Equal("3.14", builder.ToString());
    }

    [Fact]
    public void Append_DateTime_AppendsCorrectly()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        var dateTime = new DateTime(2023, 12, 25, 10, 30, 45);
        builder.Append(dateTime);
        Assert.Equal(dateTime.ToString(CultureInfo.InvariantCulture), builder.ToString());
    }

    [Fact]
    public void Append_DateTimeWithFormat_AppendsCorrectly()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        var dateTime = new DateTime(2023, 12, 25, 10, 30, 45);
        builder.Append(dateTime, "yyyy-MM-dd");
        Assert.Equal("2023-12-25", builder.ToString());
    }

    [Fact]
    public void AppendLine_AppendsNewline()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        builder.AppendLine();
        Assert.Equal(Environment.NewLine, builder.ToString());
    }

    [Fact]
    public void AppendLine_WithString_AppendsStringAndNewline()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        builder.AppendLine("Hello");
        Assert.Equal("Hello" + Environment.NewLine, builder.ToString());
    }

    [Fact]
    public void Clear_ResetsLength()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        builder.Append("Hello");
        Assert.Equal(5, builder.Length);

        builder.Clear();
        Assert.Equal(0, builder.Length);
        Assert.Equal("", builder.ToString());
    }

    [Fact]
    public void AsSpan_ReturnsWrittenSpan()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        builder.Append("Hello");
        var span = builder.AsSpan();

        Assert.Equal(5, span.Length);
        Assert.Equal("Hello", span.ToString());
    }

    [Fact]
    public void ComplexScenario_WorksCorrectly()
    {
        using var builder = ZaPooledStringBuilder.Rent();
        builder.Append("User: ")
            .Append("John Doe")
            .Append(", Age: ")
            .Append(30)
            .Append(", Balance: $")
            .Append(1234.56, "F2")
            .Append(", Active: ")
            .Append(true);

        var expected = "User: John Doe, Age: 30, Balance: $1234.56, Active: true";
        Assert.Equal(expected, builder.ToString());
    }

    [Fact]
    public void ManyAppends_GrowsBuffer()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        for (var i = 0; i < 100; i++)
        {
            builder.Append($"Item {i}: ");
        }

        Assert.True(builder.Length > 0);
        Assert.Contains("Item 0:", builder.ToString());
        Assert.Contains("Item 99:", builder.ToString());
    }

    [Fact]
    public void Dispose_ReturnsBufferToPool()
    {
        var builder = ZaPooledStringBuilder.Rent(128);
        builder.Append("Test");
        var capacity = builder.Capacity;

        builder.Dispose();

        // Create a new builder to verify the pool is working
        using var newBuilder = ZaPooledStringBuilder.Rent(128);
        Assert.True(newBuilder.Capacity >= 128);
    }

    [Fact]
    public void UsingStatement_DisposesCorrectly()
    {
        ZaPooledStringBuilder builder;
        using (builder = ZaPooledStringBuilder.Rent(128))
        {
            builder.Append("Test");
            Assert.Equal("Test", builder.ToString());
        }

        // Builder should be disposed after using block
        Assert.Throws<ObjectDisposedException>(() => builder.Append("Test"));
    }

    [Fact]
    public void Append_AfterDispose_ThrowsObjectDisposedException()
    {
        var builder = ZaPooledStringBuilder.Rent(128);
        builder.Dispose();

        Assert.Throws<ObjectDisposedException>(() => builder.Append("test"));
        Assert.Throws<ObjectDisposedException>(() => builder.Append('x'));
        Assert.Throws<ObjectDisposedException>(() => builder.Append("test".AsSpan()));
        Assert.Throws<ObjectDisposedException>(() => builder.Append(true));
        Assert.Throws<ObjectDisposedException>(() => builder.Append(42));
    }

    [Fact]
    public void ToString_AfterDispose_ThrowsObjectDisposedException()
    {
        var builder = ZaPooledStringBuilder.Rent(128);
        builder.Append("Test");
        builder.Dispose();

        Assert.Throws<ObjectDisposedException>(() => builder.ToString());
    }

    [Fact]
    public void AsSpan_AfterDispose_ThrowsObjectDisposedException()
    {
        var builder = ZaPooledStringBuilder.Rent(128);
        builder.Append("Test");
        builder.Dispose();

        Assert.Throws<ObjectDisposedException>(() => builder.AsSpan());
    }

    [Fact]
    public void AppendLine_AfterDispose_ThrowsObjectDisposedException()
    {
        var builder = ZaPooledStringBuilder.Rent(128);
        builder.Dispose();

        Assert.Throws<ObjectDisposedException>(() => builder.AppendLine());
        Assert.Throws<ObjectDisposedException>(() => builder.AppendLine("test"));
    }

    [Fact]
    public void ToUtf8NullTerminated_AfterDispose_ThrowsObjectDisposedException()
    {
        var builder = ZaPooledStringBuilder.Rent(128);
        builder.Append("Test");
        builder.Dispose();

        Assert.Throws<ObjectDisposedException>(() => builder.ToUtf8NullTerminated());
    }

    [Fact]
    public void TryToUtf8NullTerminated_AfterDispose_ThrowsObjectDisposedException()
    {
        var builder = ZaPooledStringBuilder.Rent(128);
        builder.Append("Test");
        builder.Dispose();

        Assert.Throws<ObjectDisposedException>(() => builder.TryToUtf8NullTerminated(Span<byte>.Empty, out _));
    }

    [Fact]
    public void Clear_AfterDispose_ThrowsObjectDisposedException()
    {
        var builder = ZaPooledStringBuilder.Rent(128);
        builder.Append("Test");
        builder.Dispose();

        Assert.Throws<ObjectDisposedException>(() => builder.Clear());
    }

    [Fact]
    public void Capacity_AfterDispose_ThrowsObjectDisposedException()
    {
        var builder = ZaPooledStringBuilder.Rent(128);
        builder.Append("Test");
        builder.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _ = builder.Capacity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void Rent_WithVariousCapacities_WorksCorrectly(int capacity)
    {
        using var builder = ZaPooledStringBuilder.Rent(capacity);
        Assert.Equal(0, builder.Length);
        Assert.True(builder.Capacity >= Math.Max(1, capacity));
    }

    [Fact]
    public void Append_ISpanFormattable_WorksCorrectly()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        builder.Append(123.456m, "F2");
        Assert.Equal("123.46", builder.ToString());
    }

    [Fact]
    public void Append_WithCulture_WorksCorrectly()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        var culture = new CultureInfo("fr-FR");

        builder.Append(1234.56, "C", culture);

        var expected = 1234.56.ToString("C", culture);
        Assert.Equal(expected, builder.ToString());
    }

    [Fact]
    public void Append_FailingISpanFormattable_ThrowsWhenPoolExhausted()
    {
        var pool = new LimitedArrayPool(1024);
        using var builder = ZaPooledStringBuilder.Rent(4, pool);
        var failingValue = new FailingFormattable();

        Assert.Throws<InvalidOperationException>(() => builder.Append(failingValue));
    }

    [Fact]
    public void EnsureCapacity_Overflow_ThrowsArgumentOutOfRangeException()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        builder.Append(new string('x', 100));

        var ensureCapacity = typeof(ZaPooledStringBuilder).GetMethod("EnsureCapacity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(ensureCapacity);

        var ex = Assert.Throws<System.Reflection.TargetInvocationException>(() => ensureCapacity.Invoke(builder, new object[] { int.MaxValue }));
        Assert.IsType<ArgumentOutOfRangeException>(ex.InnerException);
    }

    [Fact]
    public void SetLength_TruncatesCorrectly()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        builder.Append("Hello World");
        Assert.Equal(11, builder.Length);

        builder.SetLength(5);
        Assert.Equal(5, builder.Length);
        Assert.Equal("Hello", builder.ToString());
    }

    [Fact]
    public void SetLength_Zero_ClearsContent()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        builder.Append("Test");
        Assert.Equal(4, builder.Length);

        builder.SetLength(0);
        Assert.Equal(0, builder.Length);
        Assert.Equal("", builder.ToString());
    }

    [Fact]
    public void SetLength_ExceedsLength_ThrowsArgumentOutOfRangeException()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        builder.Append("Hi");

        Assert.Throws<ArgumentOutOfRangeException>(() => builder.SetLength(5));
    }

    [Fact]
    public void SetLength_Negative_ThrowsArgumentOutOfRangeException()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        builder.Append("Hi");

        Assert.Throws<ArgumentOutOfRangeException>(() => builder.SetLength(-1));
    }

    [Fact]
    public void RemoveLast_RemovesCorrectCount()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        builder.Append("Hello World");
        Assert.Equal(11, builder.Length);

        builder.RemoveLast(6);
        Assert.Equal(5, builder.Length);
        Assert.Equal("Hello", builder.ToString());
    }

    [Fact]
    public void RemoveLast_AllCharacters_Clears()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        builder.Append("Test");

        builder.RemoveLast(4);
        Assert.Equal(0, builder.Length);
        Assert.Equal("", builder.ToString());
    }

    [Fact]
    public void RemoveLast_ExceedsLength_ThrowsArgumentOutOfRangeException()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        builder.Append("Hi");

        Assert.Throws<ArgumentOutOfRangeException>(() => builder.RemoveLast(5));
    }

    [Fact]
    public void RemoveLast_Negative_ThrowsArgumentOutOfRangeException()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        builder.Append("Hi");

        Assert.Throws<ArgumentOutOfRangeException>(() => builder.RemoveLast(-1));
    }

    [Fact]
    public void Indexer_Get_ReturnsCorrectCharacter()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        builder.Append("Hello");

        Assert.Equal('H', builder[0]);
        Assert.Equal('e', builder[1]);
        Assert.Equal('o', builder[4]);
    }

    [Fact]
    public void Indexer_Get_OutOfRange_ThrowsArgumentOutOfRangeException()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        builder.Append("Hi");

        Assert.Throws<ArgumentOutOfRangeException>(() => builder[2]);
    }

    [Fact]
    public void Indexer_Set_ModifiesCharacter()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        builder.Append("Hello");

        builder[0] = 'J';
        builder[4] = 'y';

        Assert.Equal("Jelly", builder.ToString());
    }

    [Fact]
    public void Indexer_Set_OutOfRange_ThrowsArgumentOutOfRangeException()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        builder.Append("Hi");

        Assert.Throws<ArgumentOutOfRangeException>(() => builder[2] = 'x');
    }

    [Fact]
    public void TryAppend_ReadOnlySpan_Succeeds()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        var ok = builder.TryAppend("Hello".AsSpan());

        Assert.True(ok);
        Assert.Equal("Hello", builder.ToString());
    }

    [Fact]
    public void TryAppend_String_Succeeds()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        var ok = builder.TryAppend("Hello");

        Assert.True(ok);
        Assert.Equal("Hello", builder.ToString());
    }

    [Fact]
    public void TryAppend_String_Null_ReturnsTrue_NoChange()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        var ok = builder.TryAppend(null);

        Assert.True(ok);
        Assert.Equal("", builder.ToString());
        Assert.Equal(0, builder.Length);
    }

    [Fact]
    public void TryAppend_Char_Succeeds()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        var ok = builder.TryAppend('A');

        Assert.True(ok);
        Assert.Equal("A", builder.ToString());
        Assert.Equal(1, builder.Length);
    }

    [Fact]
    public void TryAppend_UnexpectedPoolFailure_PropagatesException()
    {
        var pool = new ThrowingArrayPool();

        using var builder = ZaPooledStringBuilder.Rent(1, pool);
        builder.Append('A');

        Assert.Throws<InvalidOperationException>(() => builder.TryAppend("BC".AsSpan()));
        Assert.Throws<InvalidOperationException>(() => builder.TryAppend('B'));
    }

    [Fact]
    public void SetLength_OnEmptyBuilder_Works()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        builder.SetLength(0);
        Assert.Equal(0, builder.Length);
        Assert.Equal("", builder.ToString());
    }

    [Fact]
    public void RemoveLast_OnEmptyBuilder_ThrowsArgumentOutOfRangeException()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.RemoveLast(1));
    }

    [Fact]
    public void SetLength_AfterDispose_ThrowsObjectDisposedException()
    {
        var builder = ZaPooledStringBuilder.Rent(128);
        builder.Append("Test");
        builder.Dispose();

        Assert.Throws<ObjectDisposedException>(() => builder.SetLength(0));
    }

    [Fact]
    public void RemoveLast_AfterDispose_ThrowsObjectDisposedException()
    {
        var builder = ZaPooledStringBuilder.Rent(128);
        builder.Append("Test");
        builder.Dispose();

        Assert.Throws<ObjectDisposedException>(() => builder.RemoveLast(1));
    }

    [Fact]
    public void Indexer_Get_AfterDispose_ThrowsObjectDisposedException()
    {
        var builder = ZaPooledStringBuilder.Rent(128);
        builder.Append("Test");
        builder.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _ = builder[0]);
    }

    [Fact]
    public void Indexer_Set_AfterDispose_ThrowsObjectDisposedException()
    {
        var builder = ZaPooledStringBuilder.Rent(128);
        builder.Append("Test");
        builder.Dispose();

        Assert.Throws<ObjectDisposedException>(() => builder[0] = 'X');
    }

    [Fact]
    public void TryAppend_AfterDispose_ThrowsObjectDisposedException()
    {
        var builder = ZaPooledStringBuilder.Rent(128);
        builder.Append("Test");
        builder.Dispose();

        Assert.Throws<ObjectDisposedException>(() => builder.TryAppend("x"));
        Assert.Throws<ObjectDisposedException>(() => builder.TryAppend('x'));
        Assert.Throws<ObjectDisposedException>(() => builder.TryAppend("x".AsSpan()));
    }

    [Fact]
    public void Append_LargeISpanFormattable_GrowsUntilItSucceeds()
    {
        using var builder = ZaPooledStringBuilder.Rent(4);

        builder.Append(new LargeFormattable());

        Assert.Equal("big", builder.ToString());
    }

    [Theory]
    [InlineData(200, 300, 300)]
    [InlineData(200, 100, 300)]
    [InlineData(0, 1, 256)]
    public void ComputeExpandedCapacity_ClampsAndGrowsSafely(int currentCapacity, int required, int expected)
    {
        var computeExpandedCapacity = typeof(ZaPooledStringBuilder)
            .GetMethod("ComputeExpandedCapacity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        Assert.NotNull(computeExpandedCapacity);
        var actual = (int)computeExpandedCapacity.Invoke(null, new object[] { currentCapacity, required })!;
        Assert.Equal(expected, actual);
    }
}
