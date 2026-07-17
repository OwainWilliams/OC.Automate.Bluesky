using System.Text.Json.Serialization;

namespace OC.Automate.Bluesky.RichText;

/// <summary>
/// A single rich-text annotation on a Bluesky post, marking a byte range of the
/// post text as a feature such as a link. See the <c>app.bsky.richtext.facet</c> lexicon.
/// </summary>
public sealed class BlueskyFacet
{
    [JsonPropertyName("index")]
    public required BlueskyFacetIndex Index { get; init; }

    [JsonPropertyName("features")]
    public required IReadOnlyList<BlueskyFacetFeature> Features { get; init; }
}

/// <summary>
/// The byte range the facet applies to. Offsets are UTF-8 byte offsets into the
/// post text (NOT UTF-16 character indices), as required by the AT Protocol.
/// </summary>
public sealed class BlueskyFacetIndex
{
    [JsonPropertyName("byteStart")]
    public required int ByteStart { get; init; }

    [JsonPropertyName("byteEnd")]
    public required int ByteEnd { get; init; }
}

/// <summary>
/// A link feature (<c>app.bsky.richtext.facet#link</c>) that makes the annotated
/// range clickable and point at <see cref="Uri"/>.
/// </summary>
public sealed class BlueskyFacetFeature
{
    [JsonPropertyName("$type")]
    public string Type { get; init; } = "app.bsky.richtext.facet#link";

    [JsonPropertyName("uri")]
    public required string Uri { get; init; }
}
