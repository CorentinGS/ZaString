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

    [Fact]
    public void AppendQueryParam_WithRefBool_TracksFirstAndSubsequent()
    {
        Span<char> buffer = stackalloc char[64];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var isFirst = true;
        builder.Append("/search")
            .AppendQueryParam("q", "a b", ref isFirst)
            .AppendQueryParam("page", "1", ref isFirst)
            .AppendQueryParam("limit", "10", ref isFirst);

        Assert.Equal("/search?q=a%20b&page=1&limit=10", builder.AsSpan());
        Assert.False(isFirst);
    }

    [Fact]
    public void AppendQueryParam_WithRefBool_StartingFalse()
    {
        Span<char> buffer = stackalloc char[64];
        var builder = ZaSpanStringBuilder.Create(buffer);

        var isFirst = false;
        builder.Append("/search?q=existing")
            .AppendQueryParam("page", "1", ref isFirst);

        Assert.Equal("/search?q=existing&page=1", builder.AsSpan());
        Assert.False(isFirst);
    }

    [Fact]
    public void AppendQueryParam_WithRefBool_Failure_IsAtomic()
    {
        Span<char> buffer = stackalloc char[8];
        var builder = ZaSpanStringBuilder.Create(buffer);
        builder.Append("/s");

        var isFirst = true;

        try
        {
            builder.AppendQueryParam("longkey", "x", ref isFirst);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException)
        {
        }

        Assert.Equal("/s", builder.AsSpan());
        Assert.True(isFirst);
    }

    [Fact]
    public void AppendFormUrlEncoded_EncodesSpacesAsPlus()
    {
        Span<char> buffer = stackalloc char[64];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.AppendFormUrlEncoded("hello world");

        Assert.Equal("hello+world", builder.AsSpan());
    }

    [Fact]
    public void AppendFormUrlEncoded_EncodesSpecialChars()
    {
        Span<char> buffer = stackalloc char[64];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.AppendFormUrlEncoded("a=b&c");

        Assert.Equal("a%3Db%26c", builder.AsSpan());
    }

    [Fact]
    public void AppendFormUrlEncoded_MixedContent()
    {
        Span<char> buffer = stackalloc char[128];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.AppendFormUrlEncoded("hello world! a+b=c");

        Assert.Equal("hello+world%21+a%2Bb%3Dc", builder.AsSpan());
    }

    [Fact]
    public void AppendFormUrlEncoded_LoneHighSurrogate_UsesReplacementCharacter()
    {
        Span<char> buffer = stackalloc char[32];
        var builder = ZaSpanStringBuilder.Create(buffer);

        builder.AppendFormUrlEncoded("\uD800");

        Assert.Equal("%EF%BF%BD", builder.AsSpan());
    }
}
