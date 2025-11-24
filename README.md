# ZaString

![ZaString logo](assets/logo.png)

[![.NET](https://github.com/CorentinGS/ZaString/actions/workflows/ci.yml/badge.svg)](https://github.com/CorentinGS/ZaString/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/ZaString.svg)](https://www.nuget.org/packages/ZaString)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

ZaString is a high-performance, zero-allocation string toolbox for modern .NET applications. It provides stack-first,
Span-based builders and helpers so you can assemble, format and encode strings with predictable, allocation-free
performance.

## 🚀 Why ZaString?

ZaString is built for scenarios where every allocation matters. If you're writing high-frequency, low-latency
services, middleware, or hotspots that produce a lot of transient strings (logging, serialization, templating),
ZaString helps keep GC pressure low while staying familiar and idiomatic to C# developers.

Key highlights:

- **Zero allocation**: Stack-first, Span-backed APIs avoid ephemeral heap allocations
- **High performance**: Faster than standard `StringBuilder` in many scenarios (see benchmarks)
- **Fluent ergonomic API**: Chainable methods for simple building and complex formatting
- **UTF‑8 support**: Write bytes directly when you need to emit UTF‑8
- **Escape helpers**: Ready-to-use JSON, HTML, CSV, URL escaping utilities
- **Interpolated string handlers**: Integrates with C# interpolation for zero-cost formatting

## 📦 Installation

Install from NuGet:

```bash
dotnet add package ZaString
```

## 🎯 Quick start

```csharp
using ZaString.Core;
using ZaString.Extensions;

// Create a stack-allocated buffer and a builder
Span<char> buffer = stackalloc char[100];
var builder = ZaSpanStringBuilder.Create(buffer);

// Build strings with a fluent, zero-allocation API
builder.Append("Hello, ")
       .Append("World!")
       .Append(" Number: ")
       .Append(42)
       .Append(" Pi: ")
       .Append(Math.PI, "F2");

var spanResult = builder.AsSpan(); // zero-allocation read-only span
Console.WriteLine(spanResult.ToString()); // prints: Hello, World! Number: 42 Pi: 3.14
```

## 🔧 Core components

### ZaSpanStringBuilder (stack-first builder)

The main string builder that writes directly to a provided `Span<char>`:

```csharp
Span<char> buffer = stackalloc char[64];
var builder = ZaSpanStringBuilder.Create(buffer);

builder.Append("User: ")
       .Append("John Doe")
       .Append(", Age: ")
       .Append(25)
       .Append(", Active: ")
       .Append(true);

// Access as ReadOnlySpan<char> (zero allocation)
var userInfo = builder.AsSpan();
```

### ZaPooledStringBuilder (pooled/heap-backed builder)

For scenarios requiring heap allocation with automatic buffer management:

```csharp
using var builder = ZaPooledStringBuilder.Rent(128);
builder.Append("Pooled string building")
       .Append(" with automatic cleanup");

var result = builder.ToString();
// Buffer automatically returned to pool when disposed
```

### ZaUtf8SpanWriter

UTF-8 byte-level string writing:

```csharp
Span<byte> buffer = stackalloc byte[256];
var writer = ZaUtf8SpanWriter.Create(buffer);

writer.Append("Hello, UTF-8 World!")
      .Append(" Number: ")
      .Append(123);

var utf8Bytes = writer.AsSpan();
```

## 🎨 Advanced features

### String Interpolation

```csharp
var name = "Alice";
var age = 30;
var pi = Math.PI;

builder.Append($"User: {name}, Age: {age}, Pi: {pi:F2}");
```

### Number Formatting

```csharp
builder.Append("Currency: ")
       .Append(1234.56, "C")           // "$1,234.56"
       .Append(", Number: ")
       .Append(12345, "N0")            // "12,345"
       .Append(", Percentage: ")
       .Append(0.85, "P2");            // "85.00%"
```

### Culture-Specific Formatting

```csharp
var fr = new CultureInfo("fr-FR");
builder.Append(1234.56, "C", fr);      // "1 234,56 €"
```

### Conditional Appending

```csharp
var isActive = true;
builder.Append("Status: ")
       .AppendIf(isActive, "Active", "Inactive");
```

### Escape Helpers

```csharp
// JSON escaping
builder.AppendJsonEscaped("Line1\nLine2\t\"Quote\"");

// HTML escaping
builder.AppendHtmlEscaped("<script>alert('xss')</script>");

// URL encoding
builder.AppendUrlEncoded("Hello World!");

// CSV escaping
builder.AppendCsvEscaped("Value,with,commas");
```

### URL Building

```csharp
builder.AppendPathSegment("api")
       .AppendPathSegment("v1")
       .AppendPathSegment("users")
       .AppendQueryParam("q", "search term", isFirst: true)
       .AppendQueryParam("page", "1");
// Result: "api/v1/users?q=search%20term&page=1"
```

### TryAppend Methods

Non-throwing variants for buffer overflow handling:

```csharp
Span<char> smallBuffer = stackalloc char[10];
var builder = ZaSpanStringBuilder.Create(smallBuffer);

if (builder.TryAppend("Hello, World!"))
{
    Console.WriteLine("Successfully appended");
}
else
{
    Console.WriteLine("Buffer too small");
}
```

## 📊 Performance

ZaString significantly outperforms traditional string-building approaches. See the benchmark results below and in
the `tests/ZaString.Benchmarks` project for details.

### Basic String Building Performance

| Method                     | Mean Time    | Memory Allocations | Performance Ratio |
|----------------------------|--------------|--------------------|-------------------|
| `StringBuilder` (Baseline) | 146.1 ns     | 480 B              | 1.00x             |
| `String` concatenation     | 116.3 ns     | 248 B              | 0.80x             |
| `String` interpolation     | 116.9 ns     | 136 B              | 0.80x             |
| **ZaSpanStringBuilder**    | **115.2 ns** | **0 B**            | **0.79x**         |

### Detailed Benchmark Results

**Basic String Building:**

- `StringBuilder`: 146.1 ns, 480 B allocated
- `StringConcatenation`: 116.3 ns, 248 B allocated
- `StringInterpolation`: 116.9 ns, 136 B allocated
- **ZaSpanStringBuilder**: **115.2 ns, 0 B allocated** ⚡

**Number Formatting:**

- `StringBuilder`: 295.3 ns, 584 B allocated
- **ZaSpanStringBuilder**: **234.9 ns, 0 B allocated** (20% faster)

**Large String Operations:**

- `StringBuilder`: 1,565.9 ns, 27,312 B allocated
- **ZaSpanStringBuilder**: **1,236.5 ns, 0 B allocated** (21% faster)

**DateTime Formatting:**

- `StringBuilder`: 189.0 ns, 384 B allocated
- **ZaSpanStringBuilder**: **135.7 ns, 0 B allocated** (28% faster)

**Span vs String Processing:**

- `StringBuilder`: 24.7 ns, 256 B allocated
- **ZaSpanStringBuilder**: **10.4 ns, 0 B allocated** (58% faster)

### Number Formatting Performance using Builder

These results use a builder baseline (StringBuilder.AppendFormat with InvariantCulture) for apples-to-apples comparison
against ZaSpanStringBuilder (zero allocation).

| Case                   | Builder Mean | Builder Alloc | ZaSpan Mean | ZaSpan Alloc |
|------------------------|-------------:|--------------:|------------:|-------------:|
| Double                 |    128.70 ns |         176 B |   104.26 ns |          0 B |
| Double (Formatted F2)  |     94.20 ns |         160 B |    73.33 ns |          0 B |
| Float                  |    105.12 ns |         168 B |    88.40 ns |          0 B |
| Long                   |     27.51 ns |         176 B |    12.58 ns |          0 B |
| Integer (Formatted N0) |     59.43 ns |         168 B |    38.28 ns |          0 B |

Environment: .NET 8.0.19, Ryzen 9 5950X, BenchmarkDotNet 0.15.2.

### Key Performance Benefits

- **Zero Memory Allocations**: ZaSpanStringBuilder uses stack-allocated buffers
- **20-58% Faster**: Significantly outperforms StringBuilder across most scenarios
- **Predictable Performance**: No GC pressure or memory fragmentation
- **Scalable**: Performance scales linearly with string size
- **Memory Efficient**: Up to 100% reduction in memory allocations

### Benchmark Example

```csharp
// Traditional approach - 146.1 ns, 480 B allocated
var sb = new StringBuilder();
sb.Append("Name: ").Append("John").Append(", Age: ").Append(25);
var result = sb.ToString();

// ZaString approach - 115.2 ns, 0 B allocated
Span<char> buffer = stackalloc char[50];
var builder = ZaSpanStringBuilder.Create(buffer);
builder.Append("Name: ").Append("John").Append(", Age: ").Append(25);
var result = builder.AsSpan(); // Zero allocation
```

## 🛠️ Use Cases

- **High-frequency string operations** in performance-critical applications
- **Parsing and formatting** without memory pressure
- **HTTP response building** in web servers
- **Logging and diagnostics** with minimal overhead
- **Data serialization** to avoid temporary allocations
- **Real-time applications** requiring predictable performance

## 🔍 API Reference

### Core Methods

- `Create(Span<char>)` - Create a new builder with a buffer
- `Append()` - Append various types with fluent API
- `AppendLine()` - Append with line terminator
- `TryAppend()` - Non-throwing append variants
- `AsSpan()` - Get result as `ReadOnlySpan<char>`
- `ToString()` - Get result as string (allocates)
- `Clear()` - Reset builder for reuse
- `SetLength(int)` - Set current length
- `RemoveLast(int)` - Remove characters from end

### Extension Methods

- `AppendIf()` - Conditional appending
- `AppendJoin()` - Join collections with separators
- `AppendRepeat()` - Repeat characters
- `AppendFormat()` - Composite formatting
- `AppendJsonEscaped()` - JSON string escaping
- `AppendHtmlEscaped()` - HTML entity escaping
- `AppendUrlEncoded()` - URL percent encoding
- `AppendCsvEscaped()` - CSV field escaping
- `AppendPathSegment()` - URL path building
- `AppendQueryParam()` - URL query parameter building

## 🧪 Testing & benchmarks

Run the full unit tests:

```bash
dotnet test
```

Run performance benchmarks (Release):

```bash
dotnet run --project tests/ZaString.Benchmarks -c Release
```

## 📝 Examples

See the [samples](samples/ZaString.Demo/) directory for complete working examples demonstrating all features.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to
discuss what you would like to change.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built with modern C# features and .NET 9.0
- Inspired by the performance benefits of `Span<T>` and `Memory<T>`
- Designed for zero-allocation scenarios in high-performance applications

---

Made with ❤️ for high-performance .NET applications
