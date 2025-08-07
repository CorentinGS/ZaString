using System.Globalization;
using ZaString.Core;
using ZaString.Extensions;

namespace ZaString.Tests;

public class ZaSpanStringBuilderInterpolationTests
{
    [Fact]
    public void Append_InterpolatedString_WritesLiteralAndFormatted()
    {
        Span<char> buffer = stackalloc char[64];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append($"Hello {{world}}: {123} {1.5}");

        Assert.Equal("Hello {world}: 123 1.5", builder.AsSpan());
    }

    [Fact]
    public void Append_InterpolatedString_WithFormat_AndCulture()
    {
        Span<char> buffer = stackalloc char[64];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append($"Hex: {255:X2}");
        Assert.Equal("Hex: FF", builder.AsSpan());

        builder.Clear();

        var fr = new CultureInfo("fr-FR");
        builder.Append($"FR: {1.5}"); // handler defaults to invariant; test explicit provider
        Assert.Equal("FR: 1.5", builder.AsSpan());
    }

    [Fact]
    public void AppendLine_InterpolatedString_AppendsNewline()
    {
        Span<char> buffer = stackalloc char[64];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.AppendLine($"Line: {42}");

        Assert.Equal($"Line: 42{Environment.NewLine}", builder.AsSpan());
    }

    [Fact]
    public void Append_InterpolatedString_WithNullValues()
    {
        Span<char> buffer = stackalloc char[64];
        var builder = ZaSpanStringBuilder.Create(buffer);

        string? nullString = null;
        builder.Append($"Null: {nullString}, Text: {"test"}");

        Assert.Equal("Null: , Text: test", builder.AsSpan());
    }

    [Fact]
    public void Append_InterpolatedString_WithComplexTypes()
    {
        Span<char> buffer = stackalloc char[128];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var dateTime = new DateTime(2023, 12, 25, 10, 30, 45);
        var guid = new Guid("12345678-1234-5678-9012-123456789012");

        builder.Append($"Date: {dateTime:yyyy-MM-dd}, GUID: {guid}");

        Assert.Equal("Date: 2023-12-25, GUID: 12345678-1234-5678-9012-123456789012", builder.AsSpan());
    }

    [Fact]
    public void Append_InterpolatedString_WithNestedExpressions()
    {
        Span<char> buffer = stackalloc char[64];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var items = new[]
        {
            "a",
            "b",
            "c"
        };
        builder.Append($"Count: {items.Length}, First: {items[0]}");

        Assert.Equal("Count: 3, First: a", builder.AsSpan());
    }

    [Fact]
    public void Append_InterpolatedString_WithConditionalExpressions()
    {
        Span<char> buffer = stackalloc char[64];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var isActive = true;
        builder.Append($"Status: {(isActive ? "Active" : "Inactive")}");

        Assert.Equal("Status: Active", builder.AsSpan());
    }

    [Fact]
    public void Append_InterpolatedString_WithMethodCalls()
    {
        Span<char> buffer = stackalloc char[64];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var text = "Hello";
        builder.Append($"Length: {text.Length}, Upper: {text.ToUpper()}");

        Assert.Equal("Length: 5, Upper: HELLO", builder.AsSpan());
    }

    [Fact]
    public void Append_InterpolatedString_WithFormatSpecifiers()
    {
        Span<char> buffer = stackalloc char[64];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var enUS = new CultureInfo("en-US");
        builder.Append(enUS, $"Number: {123.456:F2}, Hex: {255:X2}, Currency: {1234.56:C}");

        Assert.Equal("Number: 123.46, Hex: FF, Currency: $1,234.56", builder.AsSpan());
    }

    [Fact]
    public void Append_InterpolatedString_WithEscapedBraces()
    {
        Span<char> buffer = stackalloc char[64];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append($"Escaped: {{literal}}, Value: {42}");

        Assert.Equal("Escaped: {literal}, Value: 42", builder.AsSpan());
    }

    [Fact]
    public void Append_InterpolatedString_WithMultipleEscapedBraces()
    {
        Span<char> buffer = stackalloc char[64];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append($"Multiple: {{a}}, {{b}}, Value: {123}");

        Assert.Equal("Multiple: {a}, {b}, Value: 123", builder.AsSpan());
    }

    [Fact]
    public void Append_InterpolatedString_WithEmptyExpressions()
    {
        Span<char> buffer = stackalloc char[64];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append($"Empty: , Text: {"test"}");

        Assert.Equal("Empty: , Text: test", builder.AsSpan());
    }

    [Fact]
    public void Append_InterpolatedString_WithLargeNumbers()
    {
        Span<char> buffer = stackalloc char[128];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var largeNumber = 1234567890L;
        builder.Append($"Large: {largeNumber:N0}");

        Assert.Equal("Large: 1,234,567,890", builder.AsSpan());
    }

    [Fact]
    public void Append_InterpolatedString_WithDecimalPrecision()
    {
        Span<char> buffer = stackalloc char[64];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var precise = 3.14159265359m;
        builder.Append($"Pi: {precise:F5}");

        Assert.Equal("Pi: 3.14159", builder.AsSpan());
    }

    [Fact]
    public void Append_InterpolatedString_WithBooleanValues()
    {
        Span<char> buffer = stackalloc char[64];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var isTrue = true;
        var isFalse = false;
        builder.Append($"True: {isTrue}, False: {isFalse}");

        Assert.Equal("True: true, False: false", builder.AsSpan());
    }

    [Fact]
    public void Append_InterpolatedString_WithEnumValues()
    {
        Span<char> buffer = stackalloc char[64];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var day = DayOfWeek.Monday;
        builder.Append($"Day: {day}");

        Assert.Equal("Day: Monday", builder.AsSpan());
    }

    [Fact]
    public void Append_InterpolatedString_WithCustomFormatting()
    {
        Span<char> buffer = stackalloc char[64];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var time = new TimeSpan(1, 2, 3, 4);
        builder.Append($"Time: {time:hh\\:mm\\:ss}");

        Assert.Equal("Time: 02:03:04", builder.AsSpan());
    }

    [Fact]
    public void Append_InterpolatedString_WithMultipleLines()
    {
        Span<char> buffer = stackalloc char[128];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append($"Line 1: {1}\nLine 2: {2}\nLine 3: {3}");

        var result = builder.AsSpan().ToString();
        Assert.Contains("Line 1: 1", result);
        Assert.Contains("Line 2: 2", result);
        Assert.Contains("Line 3: 3", result);
    }

    [Fact]
    public void Append_InterpolatedString_WithSpecialCharacters()
    {
        Span<char> buffer = stackalloc char[64];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append($"Special: {"\t\n\r"}");

        Assert.Equal("Special: \t\n\r", builder.AsSpan());
    }
}