using System;
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
        
        Assert.Equal("Hello", builder.ToString());
        Assert.Equal(5, builder.Length);
    }

    [Fact]
    public void Append_NullString_DoesNothing()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);
        
        builder.Append((string?)null);
        
        Assert.Equal("", builder.ToString());
        Assert.Equal(0, builder.Length);
    }

    [Fact]
    public void Append_ReadOnlySpan_AppendsCorrectly()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);
        ReadOnlySpan<char> span = "World".AsSpan();
        
        builder.Append(span);
        
        Assert.Equal("World", builder.ToString());
        Assert.Equal(5, builder.Length);
    }

    [Fact]
    public void Append_Boolean_AppendsCorrectly()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);
        
        builder.Append(true).Append(false);
        
        Assert.Equal("truefalse", builder.ToString());
        Assert.Equal(9, builder.Length);
    }

    [Fact]
    public void Append_Integer_AppendsCorrectly()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);
        
        builder.Append(42).Append(-123);
        
        Assert.Equal("42-123", builder.ToString());
        Assert.Equal(6, builder.Length);
    }

    [Fact]
    public void Append_IntegerWithFormat_AppendsCorrectly()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);
        
        builder.Append(42, "X4");
        
        Assert.Equal("002A", builder.ToString());
        Assert.Equal(4, builder.Length);
    }

    [Fact]
    public void Append_Double_AppendsCorrectly()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);
        
        builder.Append(3.14159);
        
        Assert.Equal("3.14159", builder.ToString());
        Assert.Equal(7, builder.Length);
    }

    [Fact]
    public void Append_DoubleWithFormat_AppendsCorrectly()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);
        
        builder.Append(3.14159, "F2");
        
        Assert.Equal("3.14", builder.ToString());
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
        
        Assert.Equal("Hello, World! The answer is 42", builder.ToString());
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
        Assert.Equal("Test", builder.ToString());
    }

    [Fact]
    public void Append_ExactBufferSize_Works()
    {
        Span<char> buffer = stackalloc char[5];
        var builder = ZaSpanStringBuilder.Create(buffer);
        
        builder.Append("Hello");
        
        Assert.Equal("Hello", builder.ToString());
        Assert.Equal(5, builder.Length);
        Assert.Equal(0, builder.RemainingSpan.Length);
    }

    [Fact]
    public void Append_EmptyString_DoesNothing()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);
        
        builder.Append("");
        
        Assert.Equal("", builder.ToString());
        Assert.Equal(0, builder.Length);
    }

    [Fact]
    public void Append_DateTime_AppendsCorrectly()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);
        var dateTime = new DateTime(2023, 12, 25, 10, 30, 45);
        
        builder.Append(dateTime, "yyyy-MM-dd HH:mm:ss");
        
        Assert.Equal("2023-12-25 10:30:45", builder.ToString());
    }

    [Fact]
    public void Append_Guid_AppendsCorrectly()
    {
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);
        var guid = new Guid("12345678-1234-5678-9012-123456789012");
        
        builder.Append(guid);
        
        Assert.Equal("12345678-1234-5678-9012-123456789012", builder.ToString());
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
        
        Assert.Equal("User: John Doe, Age: 30, Balance: $1234.56, Active: true", builder.ToString());
    }
}