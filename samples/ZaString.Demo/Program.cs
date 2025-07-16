using System.Diagnostics;
using System.Text;
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

        Console.WriteLine("Demo complete!");
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
}