using ZaString.Escaping;

namespace ZaString.Tests;

public class JsonEscapeStrategyTests
{
    [Fact]
    public void TryEscape_WritesJsonEscapedOutputAndWrittenCount()
    {
        Span<char> destination = stackalloc char[64];

        var result = JsonEscapeStrategy.TryEscape("\"A\\\n\u0001\u2028\u2029", destination, out var written);

        Assert.True(result);
        Assert.Equal("\\\"A\\\\\\n\\u0001\\u2028\\u2029", destination[..written].ToString());
        Assert.Equal(JsonEscapeStrategy.GetEscapedLength("\"A\\\n\u0001\u2028\u2029"), written);
    }

    [Fact]
    public void TryEscape_WithInsufficientDestination_ReturnsFalseWithoutWrittenChars()
    {
        Span<char> destination = stackalloc char[3];
        destination.Fill('x');

        var result = JsonEscapeStrategy.TryEscape("\"test\"", destination, out var written);

        Assert.False(result);
        Assert.Equal(0, written);
        Assert.Equal("xxx", destination.ToString());
    }
}
