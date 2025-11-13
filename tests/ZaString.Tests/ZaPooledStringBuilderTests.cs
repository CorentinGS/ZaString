using System.Globalization;
using ZaString.Core;

namespace ZaString.Tests;

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
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.Append("Test"));
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
}