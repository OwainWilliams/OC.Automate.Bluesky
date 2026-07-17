using System.Text;
using System.Text.Json;
using OC.Automate.Bluesky.RichText;
using Xunit;

namespace OC.Automate.Bluesky.Tests;

public class BlueskyFacetBuilderTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("just some text with no links")]
    public void No_url_produces_no_facets(string? text)
    {
        Assert.Empty(BlueskyFacetBuilder.DetectFacets(text));
    }

    [Fact]
    public void Detects_a_single_https_url_with_correct_byte_offsets()
    {
        const string text = "Read https://example.com";

        var facet = Assert.Single(BlueskyFacetBuilder.DetectFacets(text));

        Assert.Equal(5, facet.Index.ByteStart);
        Assert.Equal(24, facet.Index.ByteEnd);
        var feature = Assert.Single(facet.Features);
        Assert.Equal("https://example.com", feature.Uri);
        Assert.Equal("app.bsky.richtext.facet#link", feature.Type);
    }

    [Fact]
    public void Detects_http_urls_too()
    {
        var facet = Assert.Single(BlueskyFacetBuilder.DetectFacets("http://example.com"));

        Assert.Equal("http://example.com", Assert.Single(facet.Features).Uri);
    }

    [Fact]
    public void Byte_offsets_account_for_multibyte_characters_before_the_url()
    {
        // "🚀" is 2 UTF-16 chars but 4 UTF-8 bytes; the space is 1 byte -> URL starts at byte 5.
        const string text = "🚀 https://example.com";

        var facet = Assert.Single(BlueskyFacetBuilder.DetectFacets(text));

        Assert.Equal(5, facet.Index.ByteStart);
        Assert.Equal(5 + Encoding.UTF8.GetByteCount("https://example.com"), facet.Index.ByteEnd);
        Assert.Equal("https://example.com", Assert.Single(facet.Features).Uri);
    }

    [Fact]
    public void Byte_range_slices_back_to_the_exact_url()
    {
        const string text = "🚀 see https://example.com/path now";
        var bytes = Encoding.UTF8.GetBytes(text);

        var facet = Assert.Single(BlueskyFacetBuilder.DetectFacets(text));

        var slice = Encoding.UTF8.GetString(bytes, facet.Index.ByteStart, facet.Index.ByteEnd - facet.Index.ByteStart);
        Assert.Equal("https://example.com/path", slice);
    }

    [Theory]
    [InlineData("Read https://example.com.", "https://example.com")]
    [InlineData("Read https://example.com!", "https://example.com")]
    [InlineData("Wow https://example.com?", "https://example.com")]
    [InlineData("(more at https://example.com)", "https://example.com")]
    public void Trims_trailing_sentence_punctuation(string text, string expectedUri)
    {
        var facet = Assert.Single(BlueskyFacetBuilder.DetectFacets(text));

        Assert.Equal(expectedUri, Assert.Single(facet.Features).Uri);
    }

    [Fact]
    public void Keeps_balanced_parentheses_inside_a_url()
    {
        const string text = "https://en.wikipedia.org/wiki/Foo_(bar)";

        var facet = Assert.Single(BlueskyFacetBuilder.DetectFacets(text));

        Assert.Equal("https://en.wikipedia.org/wiki/Foo_(bar)", Assert.Single(facet.Features).Uri);
    }

    [Fact]
    public void Detects_multiple_urls()
    {
        var facets = BlueskyFacetBuilder.DetectFacets("a https://one.com b https://two.com");

        Assert.Equal(2, facets.Count);
        Assert.Equal("https://one.com", facets[0].Features[0].Uri);
        Assert.Equal("https://two.com", facets[1].Features[0].Uri);
    }

    [Fact]
    public void Serializes_using_the_at_protocol_lexicon_field_names()
    {
        // This is the exact shape sent to com.atproto.repo.createRecord, so the
        // field names must match the app.bsky.richtext.facet lexicon precisely.
        var facets = BlueskyFacetBuilder.DetectFacets("go https://example.com");

        var json = JsonSerializer.Serialize(facets);
        using var document = JsonDocument.Parse(json);

        var facet = Assert.Single(document.RootElement.EnumerateArray().ToArray());
        var index = facet.GetProperty("index");
        Assert.Equal(3, index.GetProperty("byteStart").GetInt32());
        Assert.Equal(22, index.GetProperty("byteEnd").GetInt32());

        var feature = Assert.Single(facet.GetProperty("features").EnumerateArray().ToArray());
        Assert.Equal("app.bsky.richtext.facet#link", feature.GetProperty("$type").GetString());
        Assert.Equal("https://example.com", feature.GetProperty("uri").GetString());
    }
}
