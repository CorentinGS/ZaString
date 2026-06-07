using ZaString.Core;
using ZaString.Escaping;
using ZaString.Extensions;

namespace ZaString.Tests;

public class EscapeStrategyParityTests
{
    public static TheoryData<EscapeKind, string> RepresentativeCases()
    {
        var urlInput = "a b/!\u20AC\ud83d\ude00" + new string('\uD800', 1);
        return new TheoryData<EscapeKind, string>
        {
            { EscapeKind.Json, "quote: \" slash: \\ newline: \n separator: \u2028" },
            { EscapeKind.Html, "<tag attr=\"value\">Tom & 'Jerry'</tag>" },
            { EscapeKind.Csv, " value, \"quoted\"\n" },
            { EscapeKind.Url, urlInput },
            { EscapeKind.FormUrl, urlInput }
        };
    }

    [Theory]
    [MemberData(nameof(RepresentativeCases))]
    public void PooledEscaping_MatchesStackOwnedEscaping(EscapeKind kind, string input)
    {
        Span<char> stackBuffer = stackalloc char[512];
        var stackBuilder = ZaSpanStringBuilder.Create(stackBuffer);
        AppendStackOwned(ref stackBuilder, kind, input);

        using var pooledBuilder = ZaPooledStringBuilder.Rent(16);
        AppendPooled(pooledBuilder, kind, input);

        Assert.Equal(stackBuilder.AsSpan().ToString(), pooledBuilder.ToString());
    }

    [Theory]
    [MemberData(nameof(RepresentativeCases))]
    public void EscapeStrategy_TryEscape_DoesNotAllocate(EscapeKind kind, string input)
    {
        Span<char> destination = stackalloc char[512];

        var before = GC.GetAllocatedBytesForCurrentThread();
        var result = TryEscapeStrategy(kind, input, destination, out var written);
        var after = GC.GetAllocatedBytesForCurrentThread();

        Assert.True(result);
        Assert.True(written > 0);
        Assert.Equal(before, after);
    }

    [Theory]
    [MemberData(nameof(RepresentativeCases))]
    public void StackOwnedEscaping_DoesNotAllocateWithSufficientCapacity(EscapeKind kind, string input)
    {
        Span<char> stackBuffer = stackalloc char[512];
        var stackBuilder = ZaSpanStringBuilder.Create(stackBuffer);

        var before = GC.GetAllocatedBytesForCurrentThread();
        AppendStackOwned(ref stackBuilder, kind, input);
        var after = GC.GetAllocatedBytesForCurrentThread();

        Assert.True(stackBuilder.Length > 0);
        Assert.Equal(before, after);
    }

    [Theory]
    [MemberData(nameof(RepresentativeCases))]
    public void PooledEscaping_DoesNotAllocateWithSufficientCapacity(EscapeKind kind, string input)
    {
        using var pooledBuilder = ZaPooledStringBuilder.Rent(512);

        var before = GC.GetAllocatedBytesForCurrentThread();
        AppendPooled(pooledBuilder, kind, input);
        var after = GC.GetAllocatedBytesForCurrentThread();

        Assert.True(pooledBuilder.Length > 0);
        Assert.Equal(before, after);
    }

    private static void AppendStackOwned(ref ZaSpanStringBuilder builder, EscapeKind kind, ReadOnlySpan<char> input)
    {
        switch (kind)
        {
            case EscapeKind.Json:
                builder.AppendJsonEscaped(input);
                break;
            case EscapeKind.Html:
                builder.AppendHtmlEscaped(input);
                break;
            case EscapeKind.Csv:
                builder.AppendCsvEscaped(input);
                break;
            case EscapeKind.Url:
                builder.AppendUrlEncoded(input);
                break;
            case EscapeKind.FormUrl:
                builder.AppendFormUrlEncoded(input);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(kind));
        }
    }

    private static void AppendPooled(ZaPooledStringBuilder builder, EscapeKind kind, ReadOnlySpan<char> input)
    {
        switch (kind)
        {
            case EscapeKind.Json:
                builder.AppendJsonEscaped(input);
                break;
            case EscapeKind.Html:
                builder.AppendHtmlEscaped(input);
                break;
            case EscapeKind.Csv:
                builder.AppendCsvEscaped(input);
                break;
            case EscapeKind.Url:
                builder.AppendUrlEncoded(input);
                break;
            case EscapeKind.FormUrl:
                builder.AppendFormUrlEncoded(input);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(kind));
        }
    }

    private static bool TryEscapeStrategy(EscapeKind kind, ReadOnlySpan<char> input, Span<char> destination, out int written)
    {
        return kind switch
        {
            EscapeKind.Json => JsonEscapeStrategy.TryEscape(input, destination, out written),
            EscapeKind.Html => HtmlEscapeStrategy.TryEscape(input, destination, out written),
            EscapeKind.Csv => CsvEscapeStrategy.TryEscape(input, destination, out written),
            EscapeKind.Url => UrlEscapeStrategy.TryEscape(input, destination, out written),
            EscapeKind.FormUrl => FormUrlEscapeStrategy.TryEscape(input, destination, out written),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
    }
}

public enum EscapeKind
{
    Json,
    Html,
    Csv,
    Url,
    FormUrl
}
