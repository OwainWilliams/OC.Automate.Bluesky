using System.Net.Http.Json;
using Umbraco.Automate.Core.Connections;

namespace OC.Automate.Bluesky.Settings;

[ConnectionType("bluesky", "Bluesky",
    Description = "Connect to Bluesky",
    Group = "Social Networks",
    Icon = "icon-flash")]
public sealed class BlueskyConnectionType : ConnectionTypeBase<BlueskyConnectionSettings>
{
    private readonly IHttpClientFactory _httpClientFactory;

    public BlueskyConnectionType(
        ConnectionTypeInfrastructure infrastructure,
        IHttpClientFactory httpClientFactory)
        : base(infrastructure)
    {
        _httpClientFactory = httpClientFactory;
    }

    public override async Task<ConnectionValidationResult> ValidateAsync(object? settings, CancellationToken cancellationToken)
    {
        var blueskySettings = settings as BlueskyConnectionSettings;

        if (string.IsNullOrWhiteSpace(blueskySettings?.PdsUrl))
            return ConnectionValidationResult.Failure("PDS URL is required.");

        if (string.IsNullOrWhiteSpace(blueskySettings.Identifier))
            return ConnectionValidationResult.Failure("Identifier (handle or email) is required.");

        if (string.IsNullOrWhiteSpace(blueskySettings.AppPassword))
            return ConnectionValidationResult.Failure("App password is required.");

        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync(
                $"{blueskySettings.PdsUrl.TrimEnd('/')}/xrpc/com.atproto.server.createSession",
                new { identifier = blueskySettings.Identifier, password = blueskySettings.AppPassword },
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                return ConnectionValidationResult.Failure(
                    $"Authentication failed ({(int)response.StatusCode}). Check your identifier and app password. Bluesky response: {error}");
            }

            var session = await response.Content.ReadFromJsonAsync<BlueskySessionResponse>(
                cancellationToken: cancellationToken);

            return ConnectionValidationResult.Success(
                $"Connected as @{session?.Handle ?? "unknown"}.");
        }
        catch (HttpRequestException ex)
        {
            return ConnectionValidationResult.Failure($"Could not reach {blueskySettings.PdsUrl}: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return ConnectionValidationResult.Failure("Connection timed out.");
        }
    }
}
