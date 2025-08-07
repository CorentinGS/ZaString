using ZaString.Core;
using ZaString.Extensions;

namespace ZaString.Tests;

public class ZaSpanStringBuilderUrlHelpersTests
{
    [Fact]
    public void AppendUrlEncoded_Ascii_Unreserved_Untouched()
    {
        Span<char> buffer = stackalloc char[32];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.AppendUrlEncoded("abc-_.~123");

        Assert.Equal("abc-_.~123", builder.AsSpan());
    }

    [Fact]
    public void AppendUrlEncoded_Reserved_And_NonAscii_PercentEncoded()
    {
        Span<char> buffer = stackalloc char[64];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.AppendUrlEncoded("a b/€");

        // space -> %20, slash -> %2F, Euro (UTF-8: E2 82 AC) -> %E2%82%AC
        Assert.Equal("a%20b%2F%E2%82%AC", builder.AsSpan());
    }

    [Fact]
    public void AppendPathSegment_Joins_With_Single_Separator()
    {
        Span<char> buffer = stackalloc char[32];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.AppendPathSegment("api").AppendPathSegment("/v1/").AppendPathSegment("users");

        Assert.Equal("api/v1/users", builder.AsSpan());
    }

    [Fact]
    public void AppendQueryParam_Encodes_And_Uses_Correct_Delimiters()
    {
        Span<char> buffer = stackalloc char[64];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.Append("/search")
            .AppendQueryParam("q", "a b", true, true)
            .AppendQueryParam("page", "1", false);

        Assert.Equal("/search?q=a%20b&page=1", builder.AsSpan());
    }
}