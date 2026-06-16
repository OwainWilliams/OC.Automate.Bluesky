using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Umbraco.Automate.Core.Connections;
using Umbraco.Automate.Core.Settings;

namespace OC.Automate.Bluesky;

public class BlueskyConnectionSettings
{
    [Field(Label = "PDS URL", Description = "The base URL of the Bluesky PDS (e.g. https://bsky.social).")]
    public string PdsUrl { get; set; } = "https://bsky.social";

    [Field(Label = "Identifier", Description = "Your Bluesky handle (e.g. yourname.bsky.social) or registered email.", SortOrder = 1)]
    public string Identifier { get; set; } = string.Empty;

    [Field(Label = "Connection Name", Description = "The key used to look up the app password in appsettings (OwainCodes:Automate:Bluesky:AppPasswords).", SortOrder = 2)]
    public string ConnectionName { get; set; } = string.Empty;
}

[ConnectionType("bluesky", "Bluesky")]
public class BlueskyConnectionType : ConnectionTypeBase<BlueskyConnectionSettings>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<BlueskySettings> _blueskySettings;

    public BlueskyConnectionType(
        ConnectionTypeInfrastructure infrastructure,
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<BlueskySettings> blueskySettings)
        : base(infrastructure)
    {
        _httpClientFactory = httpClientFactory;
        _blueskySettings = blueskySettings;
    }

    public override async Task<ConnectionValidationResult> ValidateAsync(object? settings, CancellationToken cancellationToken)
    {
        var blueskySettings = settings as BlueskyConnectionSettings;

        if (string.IsNullOrWhiteSpace(blueskySettings?.PdsUrl))
            return ConnectionValidationResult.Failure("PDS URL is required.");

        if (string.IsNullOrWhiteSpace(blueskySettings.Identifier))
            return ConnectionValidationResult.Failure("Identifier (handle or email) is required.");

        if (string.IsNullOrWhiteSpace(blueskySettings.ConnectionName))
            return ConnectionValidationResult.Failure("Connection name is required.");

        if (!_blueskySettings.CurrentValue.AppPasswords.TryGetValue(blueskySettings.ConnectionName, out var appPassword)
            || string.IsNullOrWhiteSpace(appPassword))
        {
            return ConnectionValidationResult.Failure(
                $"No app password found for connection name '{blueskySettings.ConnectionName}' in appsettings (OwainCodes:Automate:Bluesky:AppPasswords).");
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync(
                $"{blueskySettings.PdsUrl.TrimEnd('/')}/xrpc/com.atproto.server.createSession",
                new { identifier = blueskySettings.Identifier, password = appPassword },
                cancellationToken);

            if (!response.IsSuccessStatusCode)
                return ConnectionValidationResult.Failure(
                    $"Authentication failed ({(int)response.StatusCode}). Check your identifier and app password.");

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

    internal sealed class BlueskySessionResponse
    {
        public string? AccessJwt { get; set; }
        public string? Did { get; set; }
        public string? Handle { get; set; }
    }
}
