using ZaString.Core;
using ZaString.Extensions;

namespace ZaString.Tests;

public class ZaSpanStringBuilderBasicTests
{
    [Fact]
    public void Create_WithBuffer_ReturnsBuilderWithCorrectCapacity()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        Assert.Equal(100, builder.Capacity);
        Assert.Equal(0, builder.Length);
    }

    [Fact]
    public void Append_String_AppendsCorrectly()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append("Hello");

        Assert.Equal("Hello", builder.AsSpan());
        Assert.Equal(5, builder.Length);
    }

    [Fact]
    public void Append_NullString_DoesNothing()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append(null);

        Assert.Equal("", builder.AsSpan());
        Assert.Equal(0, builder.Length);
    }

    [Fact]
    public void Append_ReadOnlySpan_AppendsCorrectly()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);
        var span = "World".AsSpan();

        builder.Append(span);

        Assert.Equal("World", builder.AsSpan());
        Assert.Equal(5, builder.Length);
    }

    [Fact]
    public void Append_Boolean_AppendsCorrectly()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append(true).Append(false);

        Assert.Equal("truefalse", builder.AsSpan());
        Assert.Equal(9, builder.Length);
    }

    [Fact]
    public void Append_Integer_AppendsCorrectly()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append(42).Append(-123);

        Assert.Equal("42-123", builder.AsSpan());
        Assert.Equal(6, builder.Length);
    }

    [Fact]
    public void Append_IntegerWithFormat_AppendsCorrectly()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append(42, "X4");

        Assert.Equal("002A", builder.AsSpan());
        Assert.Equal(4, builder.Length);
    }

    [Fact]
    public void Append_Double_AppendsCorrectly()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append(3.14159);

        Assert.Equal("3.14159", builder.AsSpan());
        Assert.Equal(7, builder.Length);
    }

    [Fact]
    public void Append_DoubleWithFormat_AppendsCorrectly()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append(3.14159, "F2");

        Assert.Equal("3.14", builder.AsSpan());
        Assert.Equal(4, builder.Length);
    }

    [Fact]
    public void ChainedAppends_WorksCorrectly()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append("Hello, ")
            .Append("World!")
            .Append(" The answer is ")
            .Append(42);

        Assert.Equal("Hello, World! The answer is 42", builder.AsSpan());
        Assert.Equal(30, builder.Length);
    }

    [Fact]
    public void WrittenSpan_ReturnsCorrectContent()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append("Test");

        Assert.True(builder.WrittenSpan.SequenceEqual("Test".AsSpan()));
    }

    [Fact]
    public void AsSpan_ReturnsCorrectContent()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append("Test");

        Assert.True(builder.AsSpan().SequenceEqual("Test".AsSpan()));
    }

    [Fact]
    public void RemainingSpan_ReturnsCorrectSize()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append("Hello");

        Assert.Equal(95, builder.RemainingSpan.Length);
    }

    [Fact]
    public void Advance_UpdatesLengthCorrectly()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        "Test".AsSpan().CopyTo(builder.RemainingSpan);
        builder.Advance(4);

        Assert.Equal(4, builder.Length);
        Assert.Equal("Test", builder.AsSpan());
    }

    [Fact]
    public void Append_ExactBufferSize_Works()
    {
        Span<char> buffer = stackalloc char[5];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append("Hello");

        Assert.Equal("Hello", builder.AsSpan());
        Assert.Equal(5, builder.Length);
        Assert.Equal(0, builder.RemainingSpan.Length);
    }

    [Fact]
    public void Append_EmptyString_DoesNothing()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append("");

        Assert.Equal("", builder.AsSpan());
        Assert.Equal(0, builder.Length);
    }

    [Fact]
    public void Append_DateTime_AppendsCorrectly()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);
        var dateTime = new DateTime(2023, 12, 25, 10, 30, 45);

        builder.Append(dateTime, "yyyy-MM-dd HH:mm:ss");

        Assert.Equal("2023-12-25 10:30:45", builder.AsSpan());
    }

    [Fact]
    public void Append_Guid_AppendsCorrectly()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);
        var guid = new Guid("12345678-1234-5678-9012-123456789012");

        builder.Append(guid);

        Assert.Equal("12345678-1234-5678-9012-123456789012", builder.AsSpan());
    }

    [Fact]
    public void ComplexScenario_MixedAppends_WorksCorrectly()
    {
        Span<char> buffer = stackalloc char[200];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append("User: ")
            .Append("John Doe")
            .Append(", Age: ")
            .Append(30)
            .Append(", Balance: $")
            .Append(1234.56, "F2")
            .Append(", Active: ")
            .Append(true);

        Assert.Equal("User: John Doe, Age: 30, Balance: $1234.56, Active: true", builder.AsSpan());
    }

    [Fact]
    public void Indexer_ReadCharacter_ReturnsCorrectCharacter()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.Append("Hello");

        Assert.Equal('H', builder[0]);
        Assert.Equal('e', builder[1]);
        Assert.Equal('l', builder[2]);
        Assert.Equal('l', builder[3]);
        Assert.Equal('o', builder[4]);
    }

    [Fact]
    public void Indexer_ModifyCharacter_ChangesCharacterCorrectly()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.Append("Hello");

        builder[0] = 'J';
        builder[4] = 'y';

        Assert.Equal("Jelly", builder.AsSpan());
    }

    [Fact]
    public void Indexer_ModifyMultipleCharacters_WorksCorrectly()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.Append("World");

        builder[0] = 'w';
        builder[1] = 'o';
        builder[2] = 'r';
        builder[3] = 'd';
        builder[4] = 's';

        Assert.Equal("words", builder.AsSpan());
    }

    [Fact]
    public void Indexer_NegativeIndex_ThrowsIndexOutOfRangeException()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.Append("Test");

        var threwForRead = false;
        var threwForWrite = false;

        try
        {
            var _ = builder[-1];
        }
        catch (IndexOutOfRangeException)
        {
            threwForRead = true;
        }

        try
        {
            builder[-1] = 'X';
        }
        catch (IndexOutOfRangeException)
        {
            threwForWrite = true;
        }

        Assert.True(threwForRead);
        Assert.True(threwForWrite);
    }

    [Fact]
    public void Indexer_IndexEqualToLength_ThrowsIndexOutOfRangeException()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.Append("Test");

        var threwForRead = false;
        var threwForWrite = false;

        try
        {
            var _ = builder[4];
        }
        catch (IndexOutOfRangeException)
        {
            threwForRead = true;
        }

        try
        {
            builder[4] = 'X';
        }
        catch (IndexOutOfRangeException)
        {
            threwForWrite = true;
        }

        Assert.True(threwForRead);
        Assert.True(threwForWrite);
    }

    [Fact]
    public void Indexer_IndexGreaterThanLength_ThrowsIndexOutOfRangeException()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.Append("Test");

        var threwForRead = false;
        var threwForWrite = false;

        try
        {
            var _ = builder[10];
        }
        catch (IndexOutOfRangeException)
        {
            threwForRead = true;
        }

        try
        {
            builder[10] = 'X';
        }
        catch (IndexOutOfRangeException)
        {
            threwForWrite = true;
        }

        Assert.True(threwForRead);
        Assert.True(threwForWrite);
    }

    [Fact]
    public void Indexer_EmptyBuilder_ThrowsIndexOutOfRangeException()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var threwForRead = false;
        var threwForWrite = false;

        try
        {
            var _ = builder[0];
        }
        catch (IndexOutOfRangeException)
        {
            threwForRead = true;
        }

        try
        {
            builder[0] = 'X';
        }
        catch (IndexOutOfRangeException)
        {
            threwForWrite = true;
        }

        Assert.True(threwForRead);
        Assert.True(threwForWrite);
    }

    [Fact]
    public void Indexer_ModifyAfterAppend_WorksCorrectly()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append("Hello");
        builder[4] = '!';
        builder.Append(" World");
        builder[10] = '!';

        Assert.Equal("Hell! Worl!", builder.AsSpan());
    }

    [Fact]
    public void Indexer_RefReturn_AllowsDirectModification()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.Append("abcde");

        ref var c = ref builder[2];
        c = 'X';

        Assert.Equal("abXde", builder.AsSpan());
        Assert.Equal('X', builder[2]);
    }

    [Fact]
    public void AppendLine_Empty_AppendsNewLine()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.AppendLine();

        Assert.Equal(Environment.NewLine, builder.AsSpan());
        Assert.Equal(Environment.NewLine.Length, builder.Length);
    }

    [Fact]
    public void AppendLine_String_AppendsStringWithNewLine()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.AppendLine("Hello");

        var expected = "Hello" + Environment.NewLine;
        Assert.Equal(expected, builder.AsSpan());
        Assert.Equal(expected.Length, builder.Length);
    }

    [Fact]
    public void AppendLine_NullString_AppendsOnlyNewLine()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.AppendLine(null);

        Assert.Equal(Environment.NewLine, builder.AsSpan());
        Assert.Equal(Environment.NewLine.Length, builder.Length);
    }

    [Fact]
    public void AppendLine_ReadOnlySpan_AppendsSpanWithNewLine()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);
        var span = "World".AsSpan();

        builder.AppendLine(span);

        var expected = "World" + Environment.NewLine;
        Assert.Equal(expected, builder.AsSpan());
        Assert.Equal(expected.Length, builder.Length);
    }

    [Fact]
    public void AppendLine_ChainedCalls_WorksCorrectly()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.AppendLine("Line 1")
            .AppendLine("Line 2")
            .AppendLine();

        var expected = "Line 1" + Environment.NewLine + "Line 2" + Environment.NewLine + Environment.NewLine;
        Assert.Equal(expected, builder.AsSpan());
        Assert.Equal(expected.Length, builder.Length);
    }

    [Fact]
    public void AppendLine_MixedWithAppend_WorksCorrectly()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append("Hello ")
            .AppendLine("World")
            .Append("Number: ")
            .Append(42);

        var expected = "Hello World" + Environment.NewLine + "Number: 42";
        Assert.Equal(expected, builder.AsSpan());
        Assert.Equal(expected.Length, builder.Length);
    }

    [Fact]
    public void Append_Char_AppendsCorrectly()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append('A');

        Assert.Equal("A", builder.AsSpan());
        Assert.Equal(1, builder.Length);
    }

    [Fact]
    public void Append_MultipleChars_AppendsCorrectly()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append('H').Append('e').Append('l').Append('l').Append('o');

        Assert.Equal("Hello", builder.AsSpan());
        Assert.Equal(5, builder.Length);
    }

    [Fact]
    public void Append_CharMixedWithStrings_WorksCorrectly()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append("Hello").Append(' ').Append("World").Append('!');

        Assert.Equal("Hello World!", builder.AsSpan());
        Assert.Equal(12, builder.Length);
    }

    [Fact]
    public void Clear_ResetsBuilder_ToEmptyState()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append("Hello World");
        Assert.Equal(11, builder.Length);

        builder.Clear();

        Assert.Equal(0, builder.Length);
        Assert.Equal("", builder.AsSpan());
        Assert.Equal(100, builder.RemainingSpan.Length);
    }

    [Fact]
    public void Clear_AllowsReuse_AfterClear()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append("First");
        builder.Clear();
        builder.Append("Second");

        Assert.Equal("Second", builder.AsSpan());
        Assert.Equal(6, builder.Length);
    }

    [Fact]
    public void Clear_EmptyBuilder_HasNoEffect()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Clear();

        Assert.Equal(0, builder.Length);
        Assert.Equal("", builder.AsSpan());
        Assert.Equal(100, builder.Capacity);
    }

    [Fact]
    public void Clear_PreservesCapacity()
    {
        Span<char> buffer = stackalloc char[50];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append("Test");
        var originalCapacity = builder.Capacity;
        
        builder.Clear();

        Assert.Equal(originalCapacity, builder.Capacity);
        Assert.Equal(50, builder.Capacity);
    }
}