using ZaString.Escaping;

namespace ZaString.Tests;

public class UrlEscapeStrategyTests
{
    [Theory]
    [InlineData("abc-_.~123", "abc-_.~123")]
    [InlineData("a b/!", "a%20b%2F%21")]
    [InlineData("€", "%E2%82%AC")]
    [InlineData("😀", "%F0%9F%98%80")]
    public void TryEscape_WritesUrlEncodedOutputAndWrittenCount(string input, string expected)
    {
        Span<char> destination = stackalloc char[64];

        var result = UrlEscapeStrategy.TryEscape(input, destination, out var written);

        Assert.True(result);
        Assert.Equal(expected, destination[..written].ToString());
        Assert.Equal(UrlEscapeStrategy.GetEscapedLength(input), written);
    }

    [Fact]
    public void TryEscape_WithLoneHighSurrogate_UsesReplacementCharacter()
    {
        Span<char> destination = stackalloc char[16];
        var input = new string('\uD800', 1);

        var result = UrlEscapeStrategy.TryEscape(input, destination, out var written);

        Assert.True(result);
        Assert.Equal("%EF%BF%BD", destination[..written].ToString());
        Assert.Equal(UrlEscapeStrategy.GetEscapedLength(input), written);
    }

    [Fact]
    public void TryEscape_WithInsufficientDestination_ReturnsFalseWithoutWrittenChars()
    {
        Span<char> destination = stackalloc char[4];
        destination.Fill('x');

        var result = UrlEscapeStrategy.TryEscape("a b", destination, out var written);

        Assert.False(result);
        Assert.Equal(0, written);
        Assert.Equal("xxxx", destination.ToString());
    }
}
