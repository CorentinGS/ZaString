using System;
using System.Globalization;
using System.Text;
using BenchmarkDotNet.Attributes;
using ZaString.Core;
using ZaString.Extensions;

namespace ZaString.Benchmarks;

[MemoryDiagnoser]
[SimpleJob]
public class StringBuildingBenchmarks
{
    private const int StringLength = 100;
    private const string TestString = "Hello, World! ";
    private const int TestNumber = 42;
    private const double TestDouble = 3.14159;
    private const bool TestBool = true;

    [Benchmark(Baseline = true)]
    public string StringBuilder_BasicAppends()
    {
        var sb = new StringBuilder();
        sb.Append("Name: ");
        sb.Append("John Doe");
        sb.Append(", Age: ");
        sb.Append(TestNumber);
        sb.Append(", Balance: $");
        sb.Append(TestDouble);
        sb.Append(", Active: ");
        sb.Append(TestBool);
        return sb.ToString();
    }

    [Benchmark]
    public string StringConcatenation_BasicAppends()
    {
        return "Name: " + "John Doe" + ", Age: " + TestNumber + ", Balance: $" + TestDouble + ", Active: " + TestBool;
    }

    [Benchmark]
    public string StringInterpolation_BasicAppends()
    {
        return $"Name: {"John Doe"}, Age: {TestNumber}, Balance: ${TestDouble}, Active: {TestBool}";
    }

    [Benchmark]
    public int ZaSpanStringBuilder_BasicAppends()
    {
        Span<char> buffer = stackalloc char[200];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append("Name: ")
            .Append("John Doe")
            .Append(", Age: ")
            .Append(TestNumber)
            .Append(", Balance: $")
            .Append(TestDouble)
            .Append(", Active: ")
            .Append(TestBool);

        return builder.Length;
    }

    [Benchmark]
    public int ZaSpanStringBuilder_AsSpan()
    {
        Span<char> buffer = stackalloc char[200];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append("Name: ")
            .Append("John Doe")
            .Append(", Age: ")
            .Append(TestNumber)
            .Append(", Balance: $")
            .Append(TestDouble)
            .Append(", Active: ")
            .Append(TestBool);

        var span = builder.AsSpan();
        return span.Length;
    }


    [Benchmark]
    public string StringBuilder_ManyAppends()
    {
        var sb = new StringBuilder();
        for (var i = 0; i < 10; i++)
        {
            sb.Append("Item ");
            sb.Append(i);
            sb.Append(": ");
            sb.Append(TestString);
            sb.Append(TestDouble);
            sb.Append(" - ");
        }

        return sb.ToString();
    }

    [Benchmark]
    public int ZaSpanStringBuilder_ManyAppends()
    {
        Span<char> buffer = stackalloc char[500];
        var builder = ZaSpanStringBuilder.Create(buffer);

        for (var i = 0; i < 10; i++)
        {
            builder.Append("Item ")
                .Append(i)
                .Append(": ")
                .Append(TestString)
                .Append(TestDouble)
                .Append(" - ");
        }

        builder.AsSpan();

        return builder.Length;
    }

    [Benchmark]
    public string StringBuilder_NumberFormatting()
    {
        var sb = new StringBuilder();
        sb.Append("Hex: ");
        sb.Append(255.ToString("X4"));
        sb.Append(", Currency: ");
        sb.Append(1234.56.ToString("C"));
        sb.Append(", Percentage: ");
        sb.Append(0.85.ToString("P2"));
        return sb.ToString();
    }

    [Benchmark]
    public int ZaSpanStringBuilder_NumberFormatting()
    {
        Span<char> buffer = stackalloc char[200];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append("Hex: ")
            .Append(255, "X4")
            .Append(", Currency: ")
            .Append(1234.56, "C")
            .Append(", Percentage: ")
            .Append(0.85, "P2");

        return builder.Length;
    }

    [Benchmark]
    public string StringBuilder_LargeString()
    {
        var sb = new StringBuilder();
        for (var i = 0; i < 100; i++)
        {
            sb.Append("This is iteration number ");
            sb.Append(i);
            sb.Append(" of the benchmark test. ");
        }

        return sb.ToString();
    }

    [Benchmark]
    public int ZaSpanStringBuilder_LargeString()
    {
        Span<char> buffer = stackalloc char[8000];
        var builder = ZaSpanStringBuilder.Create(buffer);

        for (var i = 0; i < 100; i++)
        {
            builder.Append("This is iteration number ")
                .Append(i)
                .Append(" of the benchmark test. ");
        }

        builder.AsSpan();
        return builder.Length;
    }

    [Benchmark]
    public string StringBuilder_DateTimeFormatting()
    {
        var sb = new StringBuilder();
        var now = DateTime.Now;
        sb.Append("Today is ");
        sb.Append(now.ToString("yyyy-MM-dd"));
        sb.Append(" at ");
        sb.Append(now.ToString("HH:mm:ss"));
        return sb.ToString();
    }

    [Benchmark]
    public int ZaSpanStringBuilder_DateTimeFormatting()
    {
        Span<char> buffer = stackalloc char[200];
        var builder = ZaSpanStringBuilder.Create(buffer);
        var now = DateTime.Now;

        builder.Append("Today is ")
            .Append(now, "yyyy-MM-dd")
            .Append(" at ")
            .Append(now, "HH:mm:ss");

        builder.AsSpan();
        return builder.Length;
    }
}

[MemoryDiagnoser]
[SimpleJob]
public class NumberFormattingBenchmarks
{
    private const int TestInt = 12345;
    private const double TestDouble = 3.14159265359;
    private const float TestFloat = 2.71828f;
    private const long TestLong = 9876543210L;

    [Benchmark(Baseline = true)]
    public string ToString_Integer()
    {
        var sb = new StringBuilder();
        sb.Append(TestInt);
        return sb.ToString();
    }

    [Benchmark]
    public int ZaSpanStringBuilder_Integer()
    {
        Span<char> buffer = stackalloc char[20];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.Append(TestInt);
        builder.AsSpan();
        return builder.Length;
    }

    [Benchmark]
    public string ToString_Double()
    {
        var sb = new StringBuilder();
        sb.AppendFormat(CultureInfo.InvariantCulture, "{0}", TestDouble);
        return sb.ToString();
    }

    [Benchmark]
    public int ZaSpanStringBuilder_Double()
    {
        Span<char> buffer = stackalloc char[30];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.Append(TestDouble);
        builder.AsSpan();
        return builder.Length;
    }

    [Benchmark]
    public string ToString_Float()
    {
        var sb = new StringBuilder();
        sb.AppendFormat(CultureInfo.InvariantCulture, "{0}", TestFloat);
        return sb.ToString();
    }

    [Benchmark]
    public int ZaSpanStringBuilder_Float()
    {
        Span<char> buffer = stackalloc char[20];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.Append(TestFloat);
        builder.AsSpan();
        return builder.Length;
    }

    [Benchmark]
    public string ToString_Long()
    {
        var sb = new StringBuilder();
        sb.AppendFormat(CultureInfo.InvariantCulture, "{0}", TestLong);
        return sb.ToString();
    }

    [Benchmark]
    public int ZaSpanStringBuilder_Long()
    {
        Span<char> buffer = stackalloc char[25];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.Append(TestLong);
        builder.AsSpan();
        return builder.Length;
    }

    [Benchmark]
    public string ToString_IntegerFormatted()
    {
        var sb = new StringBuilder();
        sb.AppendFormat(CultureInfo.InvariantCulture, "{0:N0}", TestInt);
        return sb.ToString();
    }

    [Benchmark]
    public int ZaSpanStringBuilder_IntegerFormatted()
    {
        Span<char> buffer = stackalloc char[30];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.Append(TestInt, "N0");
        return builder.Length;
    }

    [Benchmark]
    public string ToString_DoubleFormatted()
    {
        var sb = new StringBuilder();
        sb.AppendFormat(CultureInfo.InvariantCulture, "{0:F2}", TestDouble);
        return sb.ToString();
    }

    [Benchmark]
    public int ZaSpanStringBuilder_DoubleFormatted()
    {
        Span<char> buffer = stackalloc char[20];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.Append(TestDouble, "F2");
        builder.AsSpan();
        return builder.Length;
    }
}

[MemoryDiagnoser]
[SimpleJob]
public class SpanVsStringBenchmarks
{
    private const string TestData = "This is a test string that will be used for comparison";
    private readonly char[] _buffer = new char[1000];

    [Benchmark(Baseline = true)]
    public int ProcessString_Traditional()
    {
        var sb = new StringBuilder();
        sb.Append(TestData);
        return sb.Length;
    }

    [Benchmark]
    public int ProcessSpan_ZaSpanStringBuilder()
    {
        var buffer = _buffer.AsSpan();
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.Append(TestData);
        return builder.Length;
    }

    [Benchmark]
    public ReadOnlySpan<char> GetSpan_ZaSpanStringBuilder()
    {
        var buffer = _buffer.AsSpan();
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.Append(TestData);
        return builder.AsSpan();
    }

    [Benchmark]
    public string GetString_ZaSpanStringBuilder()
    {
        var buffer = _buffer.AsSpan();
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.Append(TestData);
        return builder.ToString();
    }
}

[MemoryDiagnoser]
[SimpleJob]
public class ZaUtf8SpanWriterBenchmarks
{
    private const string TestString = "Hello, UTF-8 World!";
    private const int TestInt = 12345;
    private const double TestDouble = 3.14159;
    private static readonly DateTime TestDateTime = DateTime.Parse("2023-12-25T10:30:45");

    [Benchmark(Baseline = true)]
    public byte[] Encoding_UTF8_GetBytes()
    {
        return Encoding.UTF8.GetBytes(TestString);
    }

    [Benchmark]
    public int ZaUtf8SpanWriter_String()
    {
        Span<byte> buffer = stackalloc byte[256];
        var writer = ZaUtf8SpanWriter.Create(buffer);
        writer.Append(TestString);
        return writer.Length;
    }

    [Benchmark]
    public int ZaUtf8SpanWriter_Int()
    {
        Span<byte> buffer = stackalloc byte[32];
        var writer = ZaUtf8SpanWriter.Create(buffer);
        writer.Append(TestInt);
        return writer.Length;
    }

    [Benchmark]
    public int ZaUtf8SpanWriter_Double()
    {
        Span<byte> buffer = stackalloc byte[32];
        var writer = ZaUtf8SpanWriter.Create(buffer);
        writer.Append(TestDouble);
        return writer.Length;
    }

    [Benchmark]
    public int ZaUtf8SpanWriter_DateTime()
    {
        Span<byte> buffer = stackalloc byte[64];
        var writer = ZaUtf8SpanWriter.Create(buffer);
        writer.Append(TestDateTime);
        return writer.Length;
    }

    [Benchmark]
    public int ZaUtf8SpanWriter_Hex()
    {
        Span<byte> buffer = stackalloc byte[64];
        var writer = ZaUtf8SpanWriter.Create(buffer);
        var data = new byte[]
        {
            0xAB,
            0xCD,
            0xEF,
            0x12,
            0x34,
            0x56
        };
        writer.AppendHex(data, true);
        return writer.Length;
    }

    [Benchmark]
    public int ZaUtf8SpanWriter_Base64()
    {
        Span<byte> buffer = stackalloc byte[128];
        var writer = ZaUtf8SpanWriter.Create(buffer);
        var data = new byte[]
        {
            0x01,
            0x02,
            0x03,
            0x04,
            0x05,
            0x06,
            0x07,
            0x08
        };
        writer.AppendBase64(data);
        return writer.Length;
    }

    [Benchmark]
    public int ZaUtf8SpanWriter_Complex()
    {
        Span<byte> buffer = stackalloc byte[512];
        var writer = ZaUtf8SpanWriter.Create(buffer);

        writer.Append("User: ")
            .Append("John Doe")
            .Append(", ID: ")
            .Append(TestInt)
            .Append(", Balance: ")
            .Append(TestDouble)
            .Append(", Created: ")
            .Append(TestDateTime);

        return writer.Length;
    }
}

[MemoryDiagnoser]
[SimpleJob]
public class ZaPooledStringBuilderBenchmarks
{
    private const string TestString = "Hello, Pooled World!";
    private const int TestInt = 12345;
    private const double TestDouble = 3.14159;

    [Benchmark(Baseline = true)]
    public string StringBuilder_Pooled()
    {
        var sb = new StringBuilder();
        sb.Append("User: ").Append("John Doe").Append(", Age: ").Append(TestInt);
        return sb.ToString();
    }

    [Benchmark]
    public string ZaPooledStringBuilder_Basic()
    {
        using var builder = ZaPooledStringBuilder.Rent(128);
        builder.Append("User: ").Append("John Doe").Append(", Age: ").Append(TestInt);
        return builder.ToString();
    }

    [Benchmark]
    public string ZaPooledStringBuilder_Complex()
    {
        using var builder = ZaPooledStringBuilder.Rent(256);
        builder.Append("User: ")
            .Append("John Doe")
            .Append(", Age: ")
            .Append(TestInt)
            .Append(", Balance: $")
            .Append(TestDouble, "F2")
            .Append(", Active: ")
            .Append(true);
        return builder.ToString();
    }

    [Benchmark]
    public string ZaPooledStringBuilder_ManyAppends()
    {
        using var builder = ZaPooledStringBuilder.Rent(512);
        for (var i = 0; i < 10; i++)
        {
            builder.Append("Item ").Append(i).Append(": ").Append(TestString).Append(" - ");
        }

        return builder.ToString();
    }
}

[MemoryDiagnoser]
[SimpleJob]
public class EscapingAndEncodingBenchmarks
{
    private const string JsonString = "\"Hello\n\tWorld\"";
    private const string HtmlString = "<script>alert('xss')</script>";
    private const string UrlString = "Hello World!";
    private const string CsvString = "Value,with,commas";

    [Benchmark(Baseline = true)]
    public string Json_Manual()
    {
        return JsonString.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\t", "\\t");
    }

    [Benchmark]
    public int ZaSpanStringBuilder_JsonEscaped()
    {
        Span<char> buffer = stackalloc char[256];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.AppendJsonEscaped(JsonString);
        return builder.Length;
    }

    [Benchmark]
    public string Html_Manual()
    {
        return HtmlString.Replace("<", "&lt;").Replace(">", "&gt;").Replace("&", "&amp;").Replace("\"", "&quot;").Replace("'", "&#39;");
    }

    [Benchmark]
    public int ZaSpanStringBuilder_HtmlEscaped()
    {
        Span<char> buffer = stackalloc char[256];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.AppendHtmlEscaped(HtmlString);
        return builder.Length;
    }

    [Benchmark]
    public string Url_Manual()
    {
        return Uri.EscapeDataString(UrlString);
    }

    [Benchmark]
    public int ZaSpanStringBuilder_UrlEncoded()
    {
        Span<char> buffer = stackalloc char[256];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.AppendUrlEncoded(UrlString);
        return builder.Length;
    }

    [Benchmark]
    public int ZaSpanStringBuilder_CsvEscaped()
    {
        Span<char> buffer = stackalloc char[256];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.AppendCsvEscaped(CsvString);
        return builder.Length;
    }
}

[MemoryDiagnoser]
[SimpleJob]
public class InterpolationBenchmarks
{
    private const string Name = "John Doe";
    private const int Age = 30;
    private const double Balance = 1234.56;

    [Benchmark(Baseline = true)]
    public string StringInterpolation_Traditional()
    {
        return $"User: {Name}, Age: {Age}, Balance: {Balance:F2}";
    }

    [Benchmark]
    public int ZaSpanStringBuilder_Interpolation()
    {
        Span<char> buffer = stackalloc char[256];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.Append($"User: {Name}, Age: {Age}, Balance: {Balance:F2}");
        return builder.Length;
    }

    [Benchmark]
    public int ZaSpanStringBuilder_ManualAppends()
    {
        Span<char> buffer = stackalloc char[256];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.Append("User: ").Append(Name).Append(", Age: ").Append(Age).Append(", Balance: ").Append(Balance, "F2");
        return builder.Length;
    }
}