using ZaString.Escaping;

namespace ZaString.Tests;

public class HtmlEscapeStrategyTests
{
    [Fact]
    public void TryEscape_WritesHtmlEscapedOutputAndWrittenCount()
    {
        Span<char> destination = stackalloc char[80];

        var result = HtmlEscapeStrategy.TryEscape("<tag attr=\"'&'>", destination, out var written);

        Assert.True(result);
        Assert.Equal("&lt;tag attr=&quot;&#39;&amp;&#39;&gt;", destination[..written].ToString());
        Assert.Equal(HtmlEscapeStrategy.GetEscapedLength("<tag attr=\"'&'>"), written);
    }

    [Fact]
    public void TryEscape_WithInsufficientDestination_ReturnsFalseWithoutWrittenChars()
    {
        Span<char> destination = stackalloc char[4];
        destination.Fill('x');

        var result = HtmlEscapeStrategy.TryEscape("<>", destination, out var written);

        Assert.False(result);
        Assert.Equal(0, written);
        Assert.Equal("xxxx", destination.ToString());
    }
}
