using ZaString.Escaping;

namespace ZaString.Tests;

public class CsvEscapeStrategyTests
{
    [Theory]
    [InlineData("", "\"\"")]
    [InlineData("plain", "plain")]
    [InlineData(" leading", "\" leading\"")]
    [InlineData("trailing ", "\"trailing \"")]
    [InlineData("a,b", "\"a,b\"")]
    [InlineData("a\"b", "\"a\"\"b\"")]
    [InlineData("a\nb", "\"a\nb\"")]
    [InlineData("a\rb", "\"a\rb\"")]
    public void TryEscape_WritesCsvEscapedOutputAndWrittenCount(string input, string expected)
    {
        Span<char> destination = stackalloc char[32];

        var result = CsvEscapeStrategy.TryEscape(input, destination, out var written);

        Assert.True(result);
        Assert.Equal(expected, destination[..written].ToString());
        Assert.Equal(CsvEscapeStrategy.GetEscapedLength(input), written);
    }

    [Fact]
    public void TryEscape_WithInsufficientDestination_ReturnsFalseWithoutWrittenChars()
    {
        Span<char> destination = stackalloc char[4];
        destination.Fill('x');

        var result = CsvEscapeStrategy.TryEscape("a,b", destination, out var written);

        Assert.False(result);
        Assert.Equal(0, written);
        Assert.Equal("xxxx", destination.ToString());
    }
}
