using System.Diagnostics;
using System.Text;
using System.Globalization;
using ZaString.Core;
using ZaString.Extensions;

namespace ZaString.Demo;

public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== ZaString Demo ===");
        Console.WriteLine("Zero-allocation string building with ZaSpanStringBuilder");
        Console.WriteLine();

        BasicUsageDemo();
        Console.WriteLine();

        FluentApiDemo();
        Console.WriteLine();

        NumberFormattingDemo();
        Console.WriteLine();

        DateTimeFormattingDemo();
        Console.WriteLine();

        PerformanceComparisonDemo();
        Console.WriteLine();

        ZeroAllocationProofDemo();
        Console.WriteLine();

        BufferSizeDemo();
        Console.WriteLine();

        CharacterModificationDemo();
        Console.WriteLine();

        TryAppendDemo();
        Console.WriteLine();

        AppendHelpersDemo();
        Console.WriteLine();

        InterpolationDemo();
        Console.WriteLine();

        JsonEscapingDemo();
        Console.WriteLine();

        UrlHelpersDemo();
        Console.WriteLine();

        PooledBuilderDemo();
        Console.WriteLine();

        Utf8WriterDemo();
        Console.WriteLine();

        FormatDemo();
        Console.WriteLine();

        Console.WriteLine("Demo complete!");
    }

    private static void TryAppendDemo()
    {
        Console.WriteLine("--- TryAppend ---");

        // Small buffer: demonstrate non-throwing false return
        Span<char> small = stackalloc char[3];
        var builder = ZaSpanStringBuilder.Create(small);
        var ok = builder.TryAppend("Hello");
        Console.WriteLine($"TryAppend(\"Hello\") ok: {ok}, Length: {builder.Length}");
        ok = builder.TryAppend('A');
        Console.WriteLine($"TryAppend('A') ok: {ok}, Text: '{builder.AsSpan()}'");

        // Sufficient buffer: show newline and culture-safe formatting
        Span<char> buffer = stackalloc char[32];
        builder = ZaSpanStringBuilder.Create(buffer);
        builder.TryAppend("Hi");
        builder.TryAppend(' ');
        builder.TryAppendLine();
        builder.TryAppend(1.5); // uses InvariantCulture by default => "1.5"
        Console.WriteLine($"With newline + double (invariant): '{builder.AsSpan()}'");

        // Custom provider formatting via TryAppend<T>
        Span<char> bufferFr = stackalloc char[16];
        var fr = new CultureInfo("fr-FR");
        builder = ZaSpanStringBuilder.Create(bufferFr);
        builder.TryAppend(1.5, provider: fr);
        Console.WriteLine($"Double with fr-FR: '{builder.AsSpan()}'");
    }

    private static void AppendHelpersDemo()
    {
        Console.WriteLine("--- Append Helpers (Repeat/Join) ---");

        Span<char> buffer = stackalloc char[128];
        var builder = ZaSpanStringBuilder.Create(buffer);

        // AppendRepeat and TryAppendRepeat
        builder.AppendRepeat('-', 10).AppendLine();
        var repeatedOk = builder.TryAppendRepeat('*', 5);
        builder.AppendLine();
        Console.WriteLine($"Repeat appended ok: {repeatedOk}");

        // AppendJoin with strings (null treated as empty)
        builder.AppendJoin(", ".AsSpan(), "a", null, "c").AppendLine();

        // AppendJoin with ISpanFormattable values and culture
        var values = new double[] { 1.5, 2.5, 3.5 };
        builder.AppendJoin<double>("; ".AsSpan(), values, provider: new CultureInfo("fr-FR"));

        Console.WriteLine(builder.AsSpan().ToString());
    }

    private static void InterpolationDemo()
    {
        Console.WriteLine("--- Interpolated String Handler ---");

        Span<char> buffer = stackalloc char[128];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var name = "Alice";
        var age = 30;
        var pi = Math.PI;

        builder.Append($"User: {name}, Age: {age}, Pi: {pi:F2}");
        Console.WriteLine(builder.AsSpan().ToString());

        builder.Clear();
        builder.AppendLine($"Line: {42}");
        Console.WriteLine(builder.AsSpan().ToString());
    }

    private static void JsonEscapingDemo()
    {
        Console.WriteLine("--- JSON Escaping ---");

        // Build a small JSON object with escaped string values
        Span<char> buffer = stackalloc char[256];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var name = "Alice \"A\"\n\t";
        var message = "Line1\r\nLine2\t\"Quote\"";

        builder.Append('{')
               .Append("\"name\":\"")
               .AppendJsonEscaped(name)
               .Append("\",\"message\":\"")
               .AppendJsonEscaped(message)
               .Append("\"}");

        Console.WriteLine(builder.AsSpan().ToString());

        // Demonstrate non-throwing variant
        Span<char> tiny = stackalloc char[10];
        var b2 = ZaSpanStringBuilder.Create(tiny);
        var ok = b2.TryAppendJsonEscaped(message);
        Console.WriteLine($"TryAppendJsonEscaped ok={ok}, len={b2.Length}");

        // Success case: allocate a conservatively sized buffer and retry
        Span<char> big = stackalloc char[message.Length * 6]; // safe upper-bound for escaping
        var b3 = ZaSpanStringBuilder.Create(big);
        var ok2 = b3.TryAppendJsonEscaped(message);
        Console.WriteLine($"TryAppendJsonEscaped ok={ok2}, value='{b3.AsSpan().ToString()}'");
    }

    private static void UrlHelpersDemo()
    {
        Console.WriteLine("--- URL Helpers ---");

        Span<char> buffer = stackalloc char[256];
        var builder = ZaSpanStringBuilder.Create(buffer);

        // Path composition ensures single separators
        builder.AppendPathSegment("api").AppendPathSegment("/v1/").AppendPathSegment("users");
        builder.AppendQueryParam("q", "a b", isFirst: true);
        builder.AppendQueryParam("tag", "c#");

        Console.WriteLine(builder.AsSpan().ToString()); // api/v1/users?q=a%20b&tag=c%23
    }

    private static void PooledBuilderDemo()
    {
        Console.WriteLine("--- Pooled Builder ---");

        using var b = ZaPooledStringBuilder.Rent(initialCapacity: 8);
        b.Append("Hello").Append(", ").Append("World!").Append(' ').Append(123);
        Console.WriteLine(b.AsSpan().ToString());
        Console.WriteLine(b.ToString());
    }

    private static void BasicUsageDemo()
    {
        Console.WriteLine("--- Basic Usage ---");

        // Create a buffer on the stack
        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        // Build a string
        builder.Append("Hello, ");
        builder.Append("World!");

        Console.WriteLine($"Built span: {builder.AsSpan()}");
        Console.WriteLine($"Length: {builder.Length}");
        Console.WriteLine($"Remaining capacity: {builder.RemainingSpan.Length}");
    }

    private static void FluentApiDemo()
    {
        Console.WriteLine("--- Fluent API ---");

        Span<char> buffer = stackalloc char[200];
        var builder = ZaSpanStringBuilder.Create(buffer);

        // Chain operations using fluent API
        builder.Append("Name: ")
            .Append("John Doe")
            .Append(", Age: ")
            .Append(25)
            .Append(", Balance: $")
            .Append(1234.56)
            .Append(", Active: ")
            .Append(true);


        var span = builder.AsSpan();

        Console.WriteLine($"User info: {span}");

        // Access as span (zero allocation)
        Console.WriteLine($"As span length: {span.Length}");
    }

    private static void NumberFormattingDemo()
    {
        Console.WriteLine("--- Number Formatting ---");

        Span<char> buffer = stackalloc char[150];
        var builder = ZaSpanStringBuilder.Create(buffer);

        const int number = 12345;
        const double currency = 1234.56;
        const double percentage = 0.85;

        builder.Append("Number: ")
            .Append(number, "N0")
            .Append(", Currency: ")
            .Append(currency, "C")
            .Append(", Percentage: ")
            .Append(percentage, "P2");

        Console.WriteLine($"Formatted: {builder.AsSpan()}");
    }

    private static void DateTimeFormattingDemo()
    {
        Console.WriteLine("--- DateTime Formatting ---");

        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);
        var now = DateTime.Now;

        builder.Append("Today is ")
            .Append(now, "yyyy-MM-dd")
            .Append(" at ")
            .Append(now, "HH:mm:ss");

        Console.WriteLine($"DateTime: {builder.AsSpan()}");
    }

    private static void PerformanceComparisonDemo()
    {
        Console.WriteLine("--- Performance Comparison ---");

        const int iterations = 100000;

        // StringBuilder approach
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < iterations; i++)
        {
            var sb = new StringBuilder();
            sb.Append("Hello, ");
            sb.Append("World!");
            sb.Append(" Number: ");
            sb.Append(i);
            var result = sb.ToString();
        }

        sw.Stop();
        Console.WriteLine($"StringBuilder: {sw.ElapsedMilliseconds}ms");

        // ZaSpanStringBuilder approach
        sw.Restart();
        Span<char> buffer = stackalloc char[50];
        for (var i = 0; i < iterations; i++)
        {
            var builder = ZaSpanStringBuilder.Create(buffer);
            builder.Append("Hello, ")
                .Append("World!")
                .Append(" Number: ")
                .Append(i);
            var result = builder.AsSpan();
        }

        sw.Stop();
        Console.WriteLine($"ZaSpanStringBuilder: {sw.ElapsedMilliseconds}ms");
    }

    private static void ZeroAllocationProofDemo()
    {
        Console.WriteLine("--- Zero Allocation Proof ---");

        // Force garbage collection to get a clean baseline
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var gen0Before = GC.CollectionCount(0);
        var gen1Before = GC.CollectionCount(1);
        var gen2Before = GC.CollectionCount(2);

        // Perform operations without ToString() - should be zero allocation
        const int iterations = 10000;
        Span<char> buffer = stackalloc char[100];
        for (var i = 0; i < iterations; i++)
        {
            var builder = ZaSpanStringBuilder.Create(buffer);

            builder.Append("Item ")
                .Append(i)
                .Append(": ")
                .Append("Test Value ")
                .Append(3.14159);

            // Access the result without allocation
            var span = builder.AsSpan();
            var length = span.Length; // Force usage
        }

        var gen0After = GC.CollectionCount(0);
        var gen1After = GC.CollectionCount(1);
        var gen2After = GC.CollectionCount(2);

        Console.WriteLine($"GC Collections after {iterations} operations:");
        Console.WriteLine($"  Gen 0: {gen0After - gen0Before}");
        Console.WriteLine($"  Gen 1: {gen1After - gen1Before}");
        Console.WriteLine($"  Gen 2: {gen2After - gen2Before}");
        Console.WriteLine("Zero collections = zero allocations!");
    }

    private static void BufferSizeDemo()
    {
        Console.WriteLine("--- Buffer Size Handling ---");

        // Small buffer example
        Span<char> smallBuffer = stackalloc char[10];
        var builder = ZaSpanStringBuilder.Create(smallBuffer);

        try
        {
            builder.Append("This is a very long string that won't fit");
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Console.WriteLine($"Buffer overflow handled: {ex.Message}");
        }

        // Proper buffer size
        Span<char> properBuffer = stackalloc char[100];
        builder = ZaSpanStringBuilder.Create(properBuffer);

        builder.Append("This fits perfectly!");
        Console.WriteLine($"Success: {builder.AsSpan()}");
        Console.WriteLine($"Used: {builder.Length}/{properBuffer.Length} characters");
    }

    private static void CharacterModificationDemo()
    {
        Console.WriteLine("--- Character Modification ---");

        Span<char> buffer = stackalloc char[100];
        var builder = ZaSpanStringBuilder.Create(buffer);

        // Build initial content
        builder.Append("Hello, World!");
        Console.WriteLine($"Original: {builder.AsSpan()}");

        // Example 1: Simple character replacement using indexer
        builder[0] = 'J'; // Change 'H' to 'J'
        builder[4] = 'y'; // Change 'o' to 'y'
        Console.WriteLine($"After simple modifications: {builder.AsSpan()}");

        // Example 2: Reading characters using indexer
        Console.WriteLine($"Character at index 7: '{builder[7]}'");
        Console.WriteLine($"Character at index 12: '{builder[12]}'");

        // Example 3: Using ref return for direct manipulation
        ref var exclamation = ref builder[12];
        exclamation = '?';
        Console.WriteLine($"After changing exclamation to question: {builder.AsSpan()}");

        // Example 4: Text processing - convert to uppercase
        builder.Append(" Converting to UPPER!");
        Console.WriteLine($"Before uppercase conversion: {builder.AsSpan()}");

        for (var i = 0; i < builder.Length; i++)
        {
            if (char.IsLower(builder[i]))
            {
                builder[i] = char.ToUpper(builder[i]);
            }
        }

        Console.WriteLine($"After uppercase conversion: {builder.AsSpan()}");

        // Example 5: Pattern replacement - replace all 'E' with '3'
        for (var i = 0; i < builder.Length; i++)
        {
            if (builder[i] == 'E')
            {
                builder[i] = '3';
            }
        }

        Console.WriteLine($"After replacing E with 3: {builder.AsSpan()}");

        // Example 6: Demonstrate bounds checking
        Console.WriteLine("\nDemonstrating bounds checking:");
        try
        {
            var invalidChar = builder[builder.Length]; // This should throw
        }
        catch (IndexOutOfRangeException)
        {
            Console.WriteLine("✓ IndexOutOfRangeException caught for index >= Length");
        }

        try
        {
            builder[-1] = 'X'; // This should throw
        }
        catch (IndexOutOfRangeException)
        {
            Console.WriteLine("✓ IndexOutOfRangeException caught for negative index");
        }
    }

    private static void Utf8WriterDemo()
    {
        Console.WriteLine("--- UTF-8 Writer Demo ---");

        // Create a buffer for UTF-8 bytes
        Span<byte> utf8Buffer = stackalloc byte[1024];
        var writer = ZaUtf8SpanWriter.Create(utf8Buffer);

        // Write some text
        writer.Append("Hello, ");
        writer.Append("World!");
        writer.Append(" This is a test of ");
        writer.Append(123);
        writer.Append(" bytes.");

        // Access the written bytes
        Console.WriteLine($"Written UTF-8 bytes: {writer.Length}");
        Console.WriteLine(Encoding.UTF8.GetString(writer.AsSpan()));

        // Demonstrate hex and base64
        writer.Clear();
        var data = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        writer.Append("Hex: ").AppendHex(data, uppercase: true);
        writer.Append(", Base64: ").AppendBase64(data);
        Console.WriteLine(Encoding.UTF8.GetString(writer.AsSpan()));
    }

    private static void FormatDemo()
    {
        Console.WriteLine("--- Composite Formatting ---");

        Span<char> buffer = stackalloc char[128];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var name = "Alice";
        var age = 30;
        var pi = Math.PI;

        // Composite formatting using AppendFormat
        builder.AppendFormat("User: {0}, Age: {1}, Pi: {2:F2}", name, age, pi);
        Console.WriteLine(builder.AsSpan().ToString());

        // Clear and demonstrate AppendFormat with culture-specific formatting
        builder.Clear();
        var fr = new CultureInfo("fr-FR");
        builder.AppendFormat(fr, "User: {0}, Age: {1}, Pi: {2:F2}", name, age, pi);
        Console.WriteLine(builder.AsSpan().ToString());

        // Clear and demonstrate AppendFormat with custom formatting
        builder.Clear();
        builder.AppendFormat(fr, "Currency: {0:C}", 1234.56);
        Console.WriteLine(builder.AsSpan().ToString());

        // Clear and demonstrate AppendFormat with multiple arguments
        builder.Clear();
        builder.AppendFormat(fr, "User: {0}, Age: {1}, Pi: {2:F2}, Currency: {3:C}", name, age, pi, 1234.56);
        Console.WriteLine(builder.AsSpan().ToString());
    }
}