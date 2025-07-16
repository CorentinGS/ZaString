```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4652/24H2/2024Update/HudsonValley)
AMD Ryzen 9 5950X 3.40GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 9.0.302
  [Host]     : .NET 8.0.18 (8.0.1825.31117), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.18 (8.0.1825.31117), X64 RyuJIT AVX2


```
| Method    | Mean      | Error     | StdDev    | Median    |
|---------- |----------:|----------:|----------:|----------:|
| Scenario1 | 0.0017 ns | 0.0045 ns | 0.0056 ns | 0.0000 ns |
| Scenario2 | 0.0146 ns | 0.0179 ns | 0.0167 ns | 0.0102 ns |
