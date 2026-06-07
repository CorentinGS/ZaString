using ZaString.Escaping;

namespace ZaString.Tests;

public class FormUrlEscapeStrategyTests
{
    [Theory]
    [InlineData("abc-_.~123", "abc-_.~123")]
    [InlineData("a b/!", "a+b%2F%21")]
    [InlineData("€", "%EF%BF%BD")]
    [InlineData("😀", "%F0%9F%98%80")]
    public void TryEscape_WritesFormUrlEncodedOutputAndWrittenCount(string input, string expected)
    {
        Span<char> destination = stackalloc char[64];

        var result = FormUrlEscapeStrategy.TryEscape(input, destination, out var written);

        Assert.True(result);
        Assert.Equal(expected, destination[..written].ToString());
        Assert.Equal(FormUrlEscapeStrategy.GetEscapedLength(input), written);
    }

    [Fact]
    public void TryEscape_WithLoneHighSurrogate_UsesReplacementCharacter()
    {
        Span<char> destination = stackalloc char[16];
        var input = new string('\uD800', 1);

        var result = FormUrlEscapeStrategy.TryEscape(input, destination, out var written);

        Assert.True(result);
        Assert.Equal("%EF%BF%BD", destination[..written].ToString());
        Assert.Equal(FormUrlEscapeStrategy.GetEscapedLength(input), written);
    }

    [Fact]
    public void TryEscape_WithInsufficientDestination_ReturnsFalseWithoutWrittenChars()
    {
        Span<char> destination = stackalloc char[4];
        destination.Fill('x');

        var result = FormUrlEscapeStrategy.TryEscape("a b/", destination, out var written);

        Assert.False(result);
        Assert.Equal(0, written);
        Assert.Equal("xxxx", destination.ToString());
    }
}
