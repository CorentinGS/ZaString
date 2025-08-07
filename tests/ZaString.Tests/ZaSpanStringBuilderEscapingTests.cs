using ZaString.Core;
using ZaString.Extensions;

namespace ZaString.Tests;

public class ZaSpanStringBuilderEscapingTests
{
    [Fact]
    public void AppendJsonEscaped_Basic()
    {
        Span<char> buffer = stackalloc char[64];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.AppendJsonEscaped("\"Hello\n\tWorld\"");

        Assert.Equal("\\\"Hello\\n\\tWorld\\\"", builder.AsSpan());
    }

    [Fact]
    public void TryAppendJsonEscaped_ControlChars_Unicode()
    {
        Span<char> buffer = stackalloc char[64];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var ok = builder.TryAppendJsonEscaped("A\u0001B");
        Assert.True(ok);
        Assert.Equal("A\\u0001B", builder.AsSpan());
    }

    [Fact]
    public void AppendHtmlEscaped_Basic()
    {
        Span<char> buffer = stackalloc char[64];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.AppendHtmlEscaped("<a href=\"#\">'x' & y</a>");

        Assert.Equal("&lt;a href=&quot;#&quot;&gt;&#39;x&#39; &amp; y&lt;/a&gt;", builder.AsSpan());
    }

    [Fact]
    public void TryAppendCsvEscaped_Quotes_Commas_Newlines()
    {
        Span<char> buffer = stackalloc char[128];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var ok = builder.TryAppendCsvEscaped(" a,\"b\"\n");
        Assert.True(ok);
        Assert.Equal("\" a,\"\"b\"\"\n\"", builder.AsSpan());
    }
}

