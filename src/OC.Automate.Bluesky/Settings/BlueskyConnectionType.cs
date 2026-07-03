using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using Umbraco.Automate.Core.Connections;

namespace OC.Automate.Bluesky.Settings;

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
}
