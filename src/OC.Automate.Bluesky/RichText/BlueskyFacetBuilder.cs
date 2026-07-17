using System.Text;
using System.Text.RegularExpressions;

namespace OC.Automate.Bluesky.RichText;

/// <summary>
/// Detects URLs in post text and produces the <see cref="BlueskyFacet"/> link
/// annotations Bluesky needs to render them as clickable links. Bluesky does not
/// auto-detect links, so without these facets a URL is rendered as dead plain text.
/// </summary>
public static partial class BlueskyFacetBuilder
{
    // Matches http(s) URLs. Trailing punctuation is trimmed separately so that
    // sentences like "see https://example.com." don't swallow the full stop.
    [GeneratedRegex(@"https?://[^\s]+", RegexOptions.IgnoreCase)]
    private static partial Regex UrlRegex();

    /// <summary>
    /// Returns a link facet for each URL found in <paramref name="text"/>, with byte
    /// offsets measured in UTF-8 bytes as the AT Protocol requires.
    /// </summary>
    public static IReadOnlyList<BlueskyFacet> DetectFacets(string? text)
    {
        var facets = new List<BlueskyFacet>();
        if (string.IsNullOrEmpty(text))
            return facets;

        foreach (Match match in UrlRegex().Matches(text))
        {
            var url = TrimTrailingPunctuation(match.Value);
            if (url.Length == 0)
                continue;

            // Byte offset of the match start = UTF-8 byte length of everything before it.
            var byteStart = Encoding.UTF8.GetByteCount(text.AsSpan(0, match.Index));
            var byteEnd = byteStart + Encoding.UTF8.GetByteCount(url);

            facets.Add(new BlueskyFacet
            {
                Index = new BlueskyFacetIndex { ByteStart = byteStart, ByteEnd = byteEnd },
                Features = new[] { new BlueskyFacetFeature { Uri = url } },
            });
        }

        return facets;
    }

    private static string TrimTrailingPunctuation(string url)
    {
        // Strip common trailing punctuation that is almost certainly sentence
        // punctuation rather than part of the URL.
        var end = url.Length;
        while (end > 0 && ".,;:!?\"'".IndexOf(url[end - 1]) >= 0)
            end--;

        var trimmed = url[..end];

        // A trailing ")" with no matching "(" is usually the closing paren of a
        // sentence like "(more at https://example.com)".
        if (trimmed.EndsWith(')') && !trimmed.Contains('('))
            trimmed = trimmed[..^1];

        return trimmed;
    }
}
