using System;
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
        return TestDouble.ToString();
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
        return TestFloat.ToString();
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
        return TestLong.ToString();
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
        return TestInt.ToString("N0");
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
        return TestDouble.ToString("F2");
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