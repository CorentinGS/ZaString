```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4652/24H2/2024Update/HudsonValley)
AMD Ryzen 9 5950X 3.40GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 9.0.302
  [Host]     : .NET 9.0.7 (9.0.725.31616), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.7 (9.0.725.31616), X64 RyuJIT AVX2


```

| Method                                 |       Mean |    Error |   StdDev |     Median | Ratio | RatioSD |   Gen0 |   Gen1 | Allocated | Alloc Ratio |
|----------------------------------------|-----------:|---------:|---------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
| StringBuilder_BasicAppends             |   184.3 ns |  7.20 ns | 21.23 ns |   175.1 ns |  1.01 |    0.16 | 0.0286 |      - |     480 B |        1.00 |
| StringConcatenation_BasicAppends       |   122.4 ns |  2.40 ns |  2.24 ns |   122.5 ns |  0.67 |    0.08 | 0.0148 |      - |     248 B |        0.52 |
| StringInterpolation_BasicAppends       |   146.7 ns |  2.98 ns |  8.30 ns |   148.6 ns |  0.81 |    0.10 | 0.0081 |      - |     136 B |        0.28 |
| ZaSpanStringBuilder_BasicAppends       |   118.1 ns |  1.53 ns |  1.28 ns |   117.8 ns |  0.65 |    0.07 |      - |      - |         - |        0.00 |
| ZaSpanStringBuilder_AsSpan             |   120.4 ns |  0.95 ns |  0.74 ns |   120.4 ns |  0.66 |    0.07 |      - |      - |         - |        0.00 |
| ZaSpanStringBuilder_ToString           |   150.1 ns |  4.34 ns | 12.80 ns |   156.1 ns |  0.83 |    0.12 | 0.0081 |      - |     136 B |        0.28 |
| StringBuilder_ManyAppends              | 1,148.5 ns | 10.24 ns |  9.08 ns | 1,150.1 ns |  6.31 |    0.71 | 0.1259 |      - |    2120 B |        4.42 |
| ZaSpanStringBuilder_ManyAppends        | 1,078.3 ns | 10.85 ns |  9.62 ns | 1,077.2 ns |  5.93 |    0.67 |      - |      - |         - |        0.00 |
| StringBuilder_NumberFormatting         |   327.5 ns |  3.64 ns |  3.40 ns |   326.0 ns |  1.80 |    0.20 | 0.0348 |      - |     584 B |        1.22 |
| ZaSpanStringBuilder_NumberFormatting   |   240.7 ns |  1.65 ns |  1.46 ns |   240.6 ns |  1.32 |    0.15 |      - |      - |         - |        0.00 |
| StringBuilder_LargeString              | 2,328.4 ns | 46.09 ns | 89.90 ns | 2,322.3 ns | 12.80 |    1.52 | 1.6327 | 0.0954 |   27312 B |       56.90 |
| ZaSpanStringBuilder_LargeString        |         NA |       NA |       NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
| StringBuilder_DateTimeFormatting       |   203.6 ns |  2.17 ns |  2.03 ns |   203.6 ns |  1.12 |    0.13 | 0.0229 |      - |     384 B |        0.80 |
| ZaSpanStringBuilder_DateTimeFormatting |   154.1 ns |  3.07 ns |  6.55 ns |   152.7 ns |  0.85 |    0.10 |      - |      - |         - |        0.00 |

Benchmarks with issues:
StringBuildingBenchmarks.ZaSpanStringBuilder_LargeString: DefaultJob
