using System;
using System.Globalization;
using BenchmarkDotNet.Attributes;
using ZaString.Extensions;

namespace ZaString.Benchmarks;

[MemoryDiagnoser]
[SimpleJob]
public class ToSpanVsToStringBenchmarks
{
    private const int TestInt = 123456789;
    private const long TestLong = 987654321012345678;
    private const double TestDouble = 12345.6789;
    private const float TestFloat = 3141.59f;
    private static readonly Guid TestGuid = new("00112233-4455-6677-8899-aabbccddeeff");
    private static readonly DateTime TestDateTime = new(2024, 12, 25, 10, 30, 45, DateTimeKind.Utc);

    // Integer
    [Benchmark(Baseline = true)]
    public string Int_ToString_Invariant()
    {
        return TestInt.ToString(CultureInfo.InvariantCulture);
    }

    [Benchmark]
    public int Int_ToSpan_Invariant()
    {
        Span<char> buffer = stackalloc char[32];
        var span = TestInt.ToSpan(buffer, default, CultureInfo.InvariantCulture);
        return span.Length;
    }

    // Long
    [Benchmark]
    public string Long_ToString_Invariant()
    {
        return TestLong.ToString(CultureInfo.InvariantCulture);
    }

    [Benchmark]
    public int Long_ToSpan_Invariant()
    {
        Span<char> buffer = stackalloc char[64];
        var span = TestLong.ToSpan(buffer, default, CultureInfo.InvariantCulture);
        return span.Length;
    }

    // Double with format
    [Benchmark]
    public string Double_ToString_F2()
    {
        return TestDouble.ToString("F2", CultureInfo.InvariantCulture);
    }

    [Benchmark]
    public int Double_ToSpan_F2()
    {
        Span<char> buffer = stackalloc char[64];
        var span = TestDouble.ToSpan(buffer, "F2", CultureInfo.InvariantCulture);
        return span.Length;
    }

    // Float with format
    [Benchmark]
    public string Float_ToString_G()
    {
        return TestFloat.ToString("G", CultureInfo.InvariantCulture);
    }

    [Benchmark]
    public int Float_ToSpan_G()
    {
        Span<char> buffer = stackalloc char[32];
        var span = TestFloat.ToSpan(buffer, "G", CultureInfo.InvariantCulture);
        return span.Length;
    }

    // Guid
    [Benchmark]
    public string Guid_ToString_D()
    {
        return TestGuid.ToString("D");
    }

    [Benchmark]
    public int Guid_ToSpan_D()
    {
        Span<char> buffer = stackalloc char[36];
        var span = TestGuid.ToSpan(buffer, "D", CultureInfo.InvariantCulture);
        return span.Length;
    }

    // DateTime with round-trip format
    [Benchmark]
    public string DateTime_ToString_O()
    {
        return TestDateTime.ToString("O", CultureInfo.InvariantCulture);
    }

    [Benchmark]
    public int DateTime_ToSpan_O()
    {
        Span<char> buffer = stackalloc char[64];
        var span = TestDateTime.ToSpan(buffer, "O", CultureInfo.InvariantCulture);
        return span.Length;
    }
}