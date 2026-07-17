using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using OC.Automate.Bluesky.RichText;
using Umbraco.Automate.Core.Actions;

namespace OC.Automate.Bluesky.Settings;

[Action("blueskySendPost", "Send Bluesky Post",
    ConnectionTypeAlias = "bluesky",
    Icon = "icon-flash",
    Group = "Social Networks")]
public class SendBlueskyPostAction : ActionBase<BlueskyPostSettings>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SendBlueskyPostAction> _logger;

    public SendBlueskyPostAction(
        ActionInfrastructure infrastructure,
        IHttpClientFactory httpClientFactory,
        ILogger<SendBlueskyPostAction> logger)
        : base(infrastructure)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public override async Task<ActionResult> ExecuteAsync(ActionContext context, CancellationToken cancellationToken)
    {
        var connectionSettings = context.Connection?.GetSettings<BlueskyConnectionSettings>();

        if (connectionSettings is null
            || string.IsNullOrWhiteSpace(connectionSettings.PdsUrl)
            || string.IsNullOrWhiteSpace(connectionSettings.Identifier)
            || string.IsNullOrWhiteSpace(connectionSettings.AppPassword))
        {
            return ActionResult.Failed(
                new InvalidOperationException("No Bluesky connection configured."),
                StepRunErrorCategory.ConfigurationError);
        }

        var settings = context.GetSettings<BlueskyPostSettings>();

        if (string.IsNullOrWhiteSpace(settings.Content))
            return ActionResult.Failed(
                new InvalidOperationException("Post content is required."),
                StepRunErrorCategory.Validation);

        var pdsUrl = connectionSettings.PdsUrl.TrimEnd('/');
        var client = _httpClientFactory.CreateClient();

        // Create session to get accessJwt and DID
        var sessionResponse = await client.PostAsJsonAsync(
            $"{pdsUrl}/xrpc/com.atproto.server.createSession",
            new { identifier = connectionSettings.Identifier, password = connectionSettings.AppPassword },
            cancellationToken);

        if (!sessionResponse.IsSuccessStatusCode)
        {
            var sessionError = await sessionResponse.Content.ReadAsStringAsync(cancellationToken);
            return ActionResult.Failed(
                new InvalidOperationException($"Failed to authenticate with Bluesky ({sessionResponse.StatusCode}): {sessionError}"),
                StepRunErrorCategory.ConfigurationError);
        }

        var session = await sessionResponse.Content.ReadFromJsonAsync<BlueskySessionResponse>(
            cancellationToken: cancellationToken);

        if (string.IsNullOrWhiteSpace(session?.AccessJwt) || string.IsNullOrWhiteSpace(session?.Did))
        {
            return ActionResult.Failed(
                new InvalidOperationException("Bluesky session response missing accessJwt or DID."),
                StepRunErrorCategory.InvalidResponse);
        }

        // Build the post text
        var text = settings.Content.Trim();
        if (!string.IsNullOrWhiteSpace(settings.PostUrl))
            text = $"{text}\n\n{settings.PostUrl}";

        // Build the record
        var record = new Dictionary<string, object>
        {
            ["$type"] = "app.bsky.feed.post",
            ["text"] = text,
            ["createdAt"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
        };

        // Bluesky does not auto-detect links; without facets the URL is dead plain text.
        var facets = BlueskyFacetBuilder.DetectFacets(text);
        if (facets.Count > 0)
            record["facets"] = facets;

        if (!string.IsNullOrWhiteSpace(settings.ContentWarning))
        {
            record["labels"] = new Dictionary<string, object>
            {
                ["$type"] = "com.atproto.label.defs#selfLabels",
                ["values"] = new[] { new { val = settings.ContentWarning } }
            };
        }

        var payload = new
        {
            repo = session.Did,
            collection = "app.bsky.feed.post",
            record
        };

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", session.AccessJwt);

        _logger.LogInformation(
            "Posting to Bluesky via {PdsUrl}: {TextLength} characters",
            pdsUrl, text.Length);

        var response = await client.PostAsJsonAsync(
            $"{pdsUrl}/xrpc/com.atproto.repo.createRecord",
            payload,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            return ActionResult.Failed(
                new InvalidOperationException($"Bluesky API returned {response.StatusCode}: {error}"),
                StepRunErrorCategory.InvalidResponse);
        }

        return Success();
    }
}
