# OC.Automate.LinkedIn Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a standalone NuGet package `OC.Automate.LinkedIn` that adds LinkedIn connection validation and post-creation actions to Umbraco Automate, mirroring the architecture of [OC.Automate.Bluesky](https://github.com/OwainWilliams/OC.Automate.Bluesky).

**Architecture:** A single C# class library targeting .NET 10.0, depending on `Umbraco.Automate.Core` (17.0.0-beta) and `Umbraco.Cms.Core` ([17.4.0, 18.0.0)). The package registers a `LinkedInConnectionType` (validates OAuth2 access tokens via LinkedIn's token introspection endpoint) and a `SendLinkedInPostAction` (creates text posts via the LinkedIn REST Posts API). Access tokens are stored in `appsettings.json` under `OwainCodes:Automate:LinkedIn:AccessTokens`, keyed by connection name. All registration uses the Umbraco `IComposer` pattern with `IPackageManifestReader`.

**Tech Stack:** .NET 10.0, Umbraco.Automate.Core, LinkedIn REST API v2 (Posts API + Token Introspection), OAuth2 Bearer tokens, `IHttpClientFactory`, `IOptionsMonitor<T>`

**Important context:** This is a **new repository/project** — it does NOT go inside the existing OC.Automate.Bluesky repo. Create it as a sibling directory at `C:\Users\Owain Williams\Dev\OC.Automate.LinkedIn\`.

---

## File Structure

```
OC.Automate.LinkedIn/
├── src/OC.Automate.LinkedIn/
│   ├── OC.Automate.LinkedIn.csproj
│   ├── LinkedInConnectionType.cs
│   ├── SendLinkedInPostAction.cs
│   ├── Composers/
│   │   ├── LinkedInComposer.cs
│   │   └── LinkedInPackageManifestReader.cs
│   ├── Models/
│   │   └── LinkedInTokenIntrospectionResponse.cs
│   └── Settings/
│       ├── LinkedInConnectionSettings.cs
│       ├── LinkedInPostSettings.cs
│       └── LinkedInSettings.cs
├── docs/
│   └── README_nuget.md
├── NuGet.config
├── umbraco-marketplace.json
├── LICENSE
└── README.md
```

---

## Task 1: Scaffold the project

**Files:**
- Create: `src/OC.Automate.LinkedIn/OC.Automate.LinkedIn.csproj`
- Create: `NuGet.config`
- Create: `README.md`
- Create: `LICENSE`

- [ ] **Step 1: Create the solution directory and project**

```bash
cd "C:\Users\Owain Williams\Dev"
mkdir -p OC.Automate.LinkedIn/src/OC.Automate.LinkedIn
cd OC.Automate.LinkedIn
git init
```

- [ ] **Step 2: Create the .csproj file**

Create `src/OC.Automate.LinkedIn/OC.Automate.LinkedIn.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <PackageId>OC.Automate.LinkedIn</PackageId>
    <Version>1.0.0-beta001</Version>
    <Title>OC Automate LinkedIn</Title>
    <Description>LinkedIn integration for Umbraco Automate - post content to LinkedIn as part of your automation workflows.</Description>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <Authors>Owain Williams</Authors>
    <Copyright>Owain Williams</Copyright>
    <PackageProjectUrl>https://github.com/OwainWilliams/OC.Automate.LinkedIn</PackageProjectUrl>
    <RepositoryUrl>https://github.com/OwainWilliams/OC.Automate.LinkedIn</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
    <PackageTags>umbraco;automate;linkedin;social-media</PackageTags>
    <PackageReadmeFile>README_nuget.md</PackageReadmeFile>
    <PackageIcon>icon.png</PackageIcon>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Umbraco.Automate.Core" Version="17.0.0-beta" />
    <PackageReference Include="Umbraco.Cms.Core" Version="[17.4.0, 18.0.0)" />
  </ItemGroup>

  <ItemGroup>
    <None Include="..\..\docs\README_nuget.md" Pack="true" PackagePath="" />
    <None Include="..\..\LICENSE" Pack="true" PackagePath="" />
    <None Include="..\..\icon.png" Pack="true" PackagePath="" Condition="Exists('..\..\icon.png')" />
  </ItemGroup>

</Project>
```

- [ ] **Step 3: Create NuGet.config**

Create `NuGet.config` at the repo root:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="Umbraco Nightly" value="https://www.myget.org/F/umbraconightly/api/v3/index.json" />
  </packageSources>
</configuration>
```

- [ ] **Step 4: Create placeholder README and LICENSE**

Create `README.md`:

```markdown
# OC.Automate.LinkedIn

LinkedIn integration for [Umbraco Automate](https://github.com/umbraco/Umbraco.Automate). Post content to LinkedIn as part of your automation workflows.

## Installation

```bash
dotnet add package OC.Automate.LinkedIn
```

## Prerequisites

You'll need a LinkedIn App with the right permissions before this package can post on your behalf. Follow these steps:

### Step 1: Create a LinkedIn App

1. Go to [https://www.linkedin.com/developers/apps](https://www.linkedin.com/developers/apps) and sign in.
2. Click **Create app**.
3. Fill in:
   - **App name:** e.g. "My Umbraco Automate"
   - **LinkedIn Page:** Select your company page (required — create one first if you don't have one)
   - **Logo:** Upload any image
4. Accept the terms and click **Create app**.

### Step 2: Request API Access

1. On your app page, go to the **Products** tab.
2. Request access to **Share on LinkedIn** (for posting as a person) and/or **Community Management API** (for posting as an organization).
3. Wait for approval — **Share on LinkedIn** is usually instant, **Community Management API** may take a few days.

### Step 3: Get your Client ID and Client Secret

1. On your app page, go to the **Auth** tab.
2. Copy your **Client ID** and **Client Secret** — you'll need these for your `appsettings.json`.

### Step 4: Generate an Access Token

The simplest way to get a token for testing:

1. On your app's **Auth** tab, scroll down to **OAuth 2.0 tools**.
2. Click **Create token** (or use the LinkedIn Token Generator).
3. Select the scopes: **w_member_social** (post as yourself) or **w_organization_social** (post as an organization).
4. Complete the authorization flow — LinkedIn will show you an access token.
5. Copy the token.

> **⚠️ Important:** Access tokens expire (typically after 60 days). For production use, you'll need to implement a token refresh flow outside of this package, or manually rotate tokens when they expire.

### Step 5: Find your Author URN

**To post as yourself (person):**
1. Use the LinkedIn Token Generator or call the API: `GET https://api.linkedin.com/v2/userinfo` with your access token.
2. The response contains your `sub` field — your Author URN is `urn:li:person:{sub}`.

**To post as an organization:**
1. Go to your LinkedIn Company Page.
2. The URL looks like `https://www.linkedin.com/company/12345678/` — the number is your organization ID.
3. Your Author URN is `urn:li:organization:12345678`.

## Configuration

Add your LinkedIn credentials to `appsettings.json`:

```json
{
  "OwainCodes": {
    "Automate": {
      "LinkedIn": {
        "ClientId": "your-client-id",
        "ClientSecret": "your-client-secret",
        "AccessTokens": {
          "my-linkedin": "your-access-token"
        }
      }
    }
  }
}
```

| Setting | Where to find it |
|---------|-----------------|
| `ClientId` | App → Auth tab → Client ID |
| `ClientSecret` | App → Auth tab → Client Secret |
| `AccessTokens` key | A name you choose (e.g. `"my-linkedin"`) — you'll enter this same name in the Umbraco connection setup |
| `AccessTokens` value | The OAuth2 access token from Step 4 |

## Usage

1. In the Umbraco backoffice, go to **Automate** and create a new **LinkedIn** connection.
2. Enter your **Author URN** (from Step 5 above) and **Connection Name** (must match the key you used in `AccessTokens`).
3. Click **Validate** to confirm the token is working.
4. Create an automation action using **Send LinkedIn Post**.
5. Configure the post content (supports `${binding}` syntax for dynamic values like content names, URLs, etc.).
```

Create `LICENSE` with MIT license text (author: Owain Williams, year: 2026).

Create `docs/README_nuget.md` with the same content as README.md.

- [ ] **Step 5: Verify the project restores**

```bash
cd "C:\Users\Owain Williams\Dev\OC.Automate.LinkedIn"
dotnet restore src/OC.Automate.LinkedIn/OC.Automate.LinkedIn.csproj
```

Expected: Restore succeeds with no errors.

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "chore: scaffold OC.Automate.LinkedIn project"
```

---

## Task 2: Settings classes

**Files:**
- Create: `src/OC.Automate.LinkedIn/Settings/LinkedInSettings.cs`
- Create: `src/OC.Automate.LinkedIn/Settings/LinkedInConnectionSettings.cs`
- Create: `src/OC.Automate.LinkedIn/Settings/LinkedInPostSettings.cs`

- [ ] **Step 1: Create LinkedInSettings.cs**

This is the root configuration class bound to `appsettings.json`. It holds access tokens keyed by connection name, same pattern as `BlueskySettings.AppPasswords`.

Create `src/OC.Automate.LinkedIn/Settings/LinkedInSettings.cs`:

```csharp
namespace OC.Automate.LinkedIn;

public class LinkedInSettings
{
    public Dictionary<string, string> AccessTokens { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
```

- [ ] **Step 2: Create LinkedInConnectionSettings.cs**

These are the fields the user fills in when creating a LinkedIn connection in the Umbraco backoffice. LinkedIn needs: the author URN (person or org) and a connection name to look up the access token.

Create `src/OC.Automate.LinkedIn/Settings/LinkedInConnectionSettings.cs`:

```csharp
using Umbraco.Automate.Core.Settings;

namespace OC.Automate.LinkedIn;

public class LinkedInConnectionSettings
{
    [Field(
        Label = "Author URN",
        Description = "Your LinkedIn author URN (e.g. urn:li:person:abc123 or urn:li:organization:12345)")]
    public string AuthorUrn { get; set; } = string.Empty;

    [Field(
        Label = "Connection Name",
        Description = "The key used to look up the access token in appsettings.json under OwainCodes:Automate:LinkedIn:AccessTokens",
        SortOrder = 1)]
    public string ConnectionName { get; set; } = string.Empty;
}
```

- [ ] **Step 3: Create LinkedInPostSettings.cs**

These are the fields the user fills in when configuring the "Send LinkedIn Post" action. LinkedIn posts support text content, an optional URL, and visibility (PUBLIC or CONNECTIONS).

Create `src/OC.Automate.LinkedIn/Settings/LinkedInPostSettings.cs`:

```csharp
using Umbraco.Automate.Core.Settings;

namespace OC.Automate.LinkedIn;

public class LinkedInPostSettings
{
    [Field(
        Label = "Content",
        Description = "The post content (max 3000 chars). Supports ${binding} syntax for dynamic values.",
        SupportsBindings = true)]
    public string Content { get; set; } = string.Empty;

    [Field(
        Label = "Post URL",
        Description = "Optional URL to append to the post content.",
        SortOrder = 1,
        SupportsBindings = true)]
    public string? PostUrl { get; set; }

    [Field(
        Label = "Visibility",
        Description = "Post visibility: PUBLIC or CONNECTIONS. Defaults to PUBLIC.",
        SortOrder = 2)]
    public string Visibility { get; set; } = "PUBLIC";
}
```

- [ ] **Step 4: Verify the project builds**

```bash
dotnet build src/OC.Automate.LinkedIn/OC.Automate.LinkedIn.csproj
```

Expected: Build succeeds.

- [ ] **Step 5: Commit**

```bash
git add src/OC.Automate.LinkedIn/Settings/
git commit -m "feat: add LinkedIn settings classes"
```

---

## Task 3: Token introspection response model

**Files:**
- Create: `src/OC.Automate.LinkedIn/Models/LinkedInTokenIntrospectionResponse.cs`

- [ ] **Step 1: Create the model**

LinkedIn's token introspection endpoint (`POST https://www.linkedin.com/oauth/v2/introspectToken`) returns a JSON object. We only need the `active` field and `expires_at` to validate.

Create `src/OC.Automate.LinkedIn/Models/LinkedInTokenIntrospectionResponse.cs`:

```csharp
using System.Text.Json.Serialization;

namespace OC.Automate.LinkedIn;

public sealed class LinkedInTokenIntrospectionResponse
{
    [JsonPropertyName("active")]
    public bool Active { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }

    [JsonPropertyName("client_id")]
    public string? ClientId { get; set; }

    [JsonPropertyName("expires_at")]
    public long? ExpiresAt { get; set; }
}
```

- [ ] **Step 2: Commit**

```bash
git add src/OC.Automate.LinkedIn/Models/
git commit -m "feat: add LinkedIn token introspection response model"
```

---

## Task 4: LinkedInConnectionType

**Files:**
- Create: `src/OC.Automate.LinkedIn/LinkedInConnectionType.cs`

- [ ] **Step 1: Create LinkedInConnectionType.cs**

This class validates LinkedIn connections by introspecting the stored access token. It follows the same pattern as `BlueskyConnectionType`: extends `ConnectionTypeBase<T>`, uses `[ConnectionType]` attribute, and overrides `ValidateAsync`.

**Important difference from Bluesky:** LinkedIn uses OAuth2 bearer tokens (stored in config) rather than creating a session with username/password. Validation calls the token introspection endpoint to confirm the token is still active.

**Note:** Token introspection requires `client_id` and `client_secret`. These are stored alongside access tokens in `LinkedInSettings`. For simplicity we store them as reserved keys `_clientId` and `_clientSecret` in the `AccessTokens` dictionary. Alternatively, add dedicated properties — but this matches the minimal Bluesky pattern.

Actually, let's add dedicated properties. Update `LinkedInSettings.cs` first:

```csharp
namespace OC.Automate.LinkedIn;

public class LinkedInSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public Dictionary<string, string> AccessTokens { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
```

Now create `src/OC.Automate.LinkedIn/LinkedInConnectionType.cs`:

```csharp
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Automate.Core.Connections;
using Umbraco.Automate.Core.Connections.Attributes;

namespace OC.Automate.LinkedIn;

[ConnectionType("linkedin", "LinkedIn")]
public class LinkedInConnectionType : ConnectionTypeBase<LinkedInConnectionSettings>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<LinkedInSettings> _linkedInSettings;

    public LinkedInConnectionType(
        ConnectionTypeInfrastructure infrastructure,
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<LinkedInSettings> linkedInSettings)
        : base(infrastructure)
    {
        _httpClientFactory = httpClientFactory;
        _linkedInSettings = linkedInSettings;
    }

    public override async Task<ConnectionValidationResult> ValidateAsync(
        object? settings,
        CancellationToken cancellationToken)
    {
        var connectionSettings = MapSettings(settings);
        if (connectionSettings is null)
        {
            return ConnectionValidationResult.Failure("Invalid connection settings.");
        }

        if (string.IsNullOrWhiteSpace(connectionSettings.AuthorUrn))
        {
            return ConnectionValidationResult.Failure("Author URN is required.");
        }

        if (string.IsNullOrWhiteSpace(connectionSettings.ConnectionName))
        {
            return ConnectionValidationResult.Failure("Connection Name is required.");
        }

        var linkedInSettings = _linkedInSettings.CurrentValue;

        if (!linkedInSettings.AccessTokens.TryGetValue(connectionSettings.ConnectionName, out var accessToken)
            || string.IsNullOrWhiteSpace(accessToken))
        {
            return ConnectionValidationResult.Failure(
                $"No access token found for connection name '{connectionSettings.ConnectionName}' in configuration.");
        }

        if (string.IsNullOrWhiteSpace(linkedInSettings.ClientId) ||
            string.IsNullOrWhiteSpace(linkedInSettings.ClientSecret))
        {
            return ConnectionValidationResult.Failure(
                "LinkedIn ClientId and ClientSecret are required in configuration for token validation.");
        }

        try
        {
            var httpClient = _httpClientFactory.CreateClient();

            var introspectContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = linkedInSettings.ClientId,
                ["client_secret"] = linkedInSettings.ClientSecret,
                ["token"] = accessToken
            });

            var response = await httpClient.PostAsync(
                "https://www.linkedin.com/oauth/v2/introspectToken",
                introspectContent,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return ConnectionValidationResult.Failure(
                    $"Token introspection failed with status {response.StatusCode}.");
            }

            var introspectionResult = await response.Content
                .ReadFromJsonAsync<LinkedInTokenIntrospectionResponse>(cancellationToken);

            if (introspectionResult is null || !introspectionResult.Active)
            {
                return ConnectionValidationResult.Failure(
                    "The access token is invalid or expired. Please generate a new token.");
            }

            return ConnectionValidationResult.Success(
                $"LinkedIn connection validated for {connectionSettings.AuthorUrn}.");
        }
        catch (HttpRequestException ex)
        {
            return ConnectionValidationResult.Failure($"Failed to connect to LinkedIn: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return ConnectionValidationResult.Failure("Connection to LinkedIn timed out.");
        }
    }
}
```

- [ ] **Step 2: Verify the project builds**

```bash
dotnet build src/OC.Automate.LinkedIn/OC.Automate.LinkedIn.csproj
```

Expected: Build succeeds.

- [ ] **Step 3: Commit**

```bash
git add src/OC.Automate.LinkedIn/LinkedInConnectionType.cs src/OC.Automate.LinkedIn/Settings/LinkedInSettings.cs
git commit -m "feat: add LinkedIn connection type with token introspection"
```

---

## Task 5: SendLinkedInPostAction

**Files:**
- Create: `src/OC.Automate.LinkedIn/SendLinkedInPostAction.cs`

- [ ] **Step 1: Create SendLinkedInPostAction.cs**

This class posts content to LinkedIn via the REST Posts API (`POST https://api.linkedin.com/rest/posts`). It follows the same pattern as `SendBlueskyPostAction`: extends `ActionBase<T>`, uses `[Action]` attribute with `ConnectionTypeAlias`, and overrides `ExecuteAsync`.

The LinkedIn Posts API requires:
- `Authorization: Bearer {token}` header
- `X-Restli-Protocol-Version: 2.0.0` header
- `LinkedIn-Version: 202506` header (YYYYMM format)
- JSON body with `author`, `commentary`, `visibility`, `distribution`, `lifecycleState`

Create `src/OC.Automate.LinkedIn/SendLinkedInPostAction.cs`:

```csharp
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Automate.Core.Actions;
using Umbraco.Automate.Core.Actions.Attributes;

namespace OC.Automate.LinkedIn;

[Action("linkedInSendPost", "Send LinkedIn Post", ConnectionTypeAlias = "linkedin")]
public class SendLinkedInPostAction : ActionBase<LinkedInPostSettings>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SendLinkedInPostAction> _logger;
    private readonly IOptionsMonitor<LinkedInSettings> _linkedInSettings;

    public SendLinkedInPostAction(
        ActionInfrastructure infrastructure,
        IHttpClientFactory httpClientFactory,
        ILogger<SendLinkedInPostAction> logger,
        IOptionsMonitor<LinkedInSettings> linkedInSettings)
        : base(infrastructure)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _linkedInSettings = linkedInSettings;
    }

    public override async Task<ActionResult> ExecuteAsync(
        ActionContext context,
        CancellationToken cancellationToken)
    {
        var connectionSettings = context.Connection.GetSettings<LinkedInConnectionSettings>();
        if (connectionSettings is null)
        {
            return ActionResult.Failure("Invalid connection settings.", ActionErrorCategory.Configuration);
        }

        if (string.IsNullOrWhiteSpace(connectionSettings.AuthorUrn))
        {
            return ActionResult.Failure("Author URN is required.", ActionErrorCategory.Configuration);
        }

        if (string.IsNullOrWhiteSpace(connectionSettings.ConnectionName))
        {
            return ActionResult.Failure("Connection Name is required.", ActionErrorCategory.Configuration);
        }

        var linkedInSettings = _linkedInSettings.CurrentValue;

        if (!linkedInSettings.AccessTokens.TryGetValue(connectionSettings.ConnectionName, out var accessToken)
            || string.IsNullOrWhiteSpace(accessToken))
        {
            return ActionResult.Failure(
                $"No access token found for connection name '{connectionSettings.ConnectionName}'.",
                ActionErrorCategory.Configuration);
        }

        var actionSettings = GetActionSettings(context);
        if (actionSettings is null || string.IsNullOrWhiteSpace(actionSettings.Content))
        {
            return ActionResult.Failure("Post content is required.", ActionErrorCategory.Validation);
        }

        var postText = actionSettings.Content;
        if (!string.IsNullOrWhiteSpace(actionSettings.PostUrl))
        {
            postText += $"\n\n{actionSettings.PostUrl}";
        }

        var visibility = actionSettings.Visibility?.ToUpperInvariant() switch
        {
            "CONNECTIONS" => "CONNECTIONS",
            _ => "PUBLIC"
        };

        try
        {
            var httpClient = _httpClientFactory.CreateClient();

            var postBody = new
            {
                author = connectionSettings.AuthorUrn,
                commentary = postText,
                visibility = visibility,
                distribution = new
                {
                    feedDistribution = "MAIN_FEED",
                    targetEntities = Array.Empty<object>(),
                    thirdPartyDistributionChannels = Array.Empty<object>()
                },
                lifecycleState = "PUBLISHED",
                isReshareDisabledByAuthor = false
            };

            var json = JsonSerializer.Serialize(postBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.linkedin.com/rest/posts")
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Add("X-Restli-Protocol-Version", "2.0.0");
            request.Headers.Add("LinkedIn-Version", "202506");

            var response = await httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("LinkedIn API error {StatusCode}: {Body}", response.StatusCode, errorBody);
                return ActionResult.Failure(
                    $"LinkedIn API returned {response.StatusCode}.",
                    ActionErrorCategory.InvalidResponse);
            }

            _logger.LogInformation("Successfully posted to LinkedIn for {AuthorUrn}", connectionSettings.AuthorUrn);
            return ActionResult.Success();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to LinkedIn API");
            return ActionResult.Failure($"Failed to connect to LinkedIn: {ex.Message}", ActionErrorCategory.Api);
        }
        catch (TaskCanceledException)
        {
            return ActionResult.Failure("Request to LinkedIn timed out.", ActionErrorCategory.Api);
        }
    }
}
```

- [ ] **Step 2: Verify the project builds**

```bash
dotnet build src/OC.Automate.LinkedIn/OC.Automate.LinkedIn.csproj
```

Expected: Build succeeds.

- [ ] **Step 3: Commit**

```bash
git add src/OC.Automate.LinkedIn/SendLinkedInPostAction.cs
git commit -m "feat: add Send LinkedIn Post action"
```

---

## Task 6: Composer and PackageManifestReader

**Files:**
- Create: `src/OC.Automate.LinkedIn/Composers/LinkedInComposer.cs`
- Create: `src/OC.Automate.LinkedIn/Composers/LinkedInPackageManifestReader.cs`

- [ ] **Step 1: Create LinkedInComposer.cs**

This registers all LinkedIn services with the Umbraco DI container, following the exact same pattern as `BlueskyComposer`.

Create `src/OC.Automate.LinkedIn/Composers/LinkedInComposer.cs`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Automate.Core.Actions;
using Umbraco.Automate.Core.Connections;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Manifest;

namespace OC.Automate.LinkedIn.Composers;

public class LinkedInComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.Configure<LinkedInSettings>(
            builder.Config.GetSection("OwainCodes:Automate:LinkedIn"));

        builder.ConnectionTypes().Add<LinkedInConnectionType>();
        builder.Actions().Add<SendLinkedInPostAction>();

        builder.Services.AddSingleton<IPackageManifestReader, LinkedInPackageManifestReader>();
    }
}
```

- [ ] **Step 2: Create LinkedInPackageManifestReader.cs**

Create `src/OC.Automate.LinkedIn/Composers/LinkedInPackageManifestReader.cs`:

```csharp
using System.Reflection;
using Umbraco.Cms.Core.Manifest;

namespace OC.Automate.LinkedIn.Composers;

public class LinkedInPackageManifestReader : IPackageManifestReader
{
    public Task<IEnumerable<PackageManifest>> ReadPackageManifestsAsync()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

        PackageManifest manifest = new()
        {
            Id = "OC.Automate.LinkedIn",
            Name = "OC Automate LinkedIn",
            Version = version,
            AllowTelemetry = true,
            Extensions =
            [
                new
                {
                    name = "OC Automate LinkedIn Bundle",
                    alias = "OC.Automate.LinkedIn.Bundle",
                    type = "bundle",
                    js = $"/App_Plugins/OC.Automate.LinkedIn/oc-automate-linkedin.js?v={version}"
                }
            ]
        };

        return Task.FromResult<IEnumerable<PackageManifest>>([manifest]);
    }
}
```

- [ ] **Step 3: Verify the project builds**

```bash
dotnet build src/OC.Automate.LinkedIn/OC.Automate.LinkedIn.csproj
```

Expected: Build succeeds.

- [ ] **Step 4: Commit**

```bash
git add src/OC.Automate.LinkedIn/Composers/
git commit -m "feat: add LinkedIn composer and package manifest reader"
```

---

## Task 7: Marketplace metadata and final polish

**Files:**
- Create: `umbraco-marketplace.json`

- [ ] **Step 1: Create umbraco-marketplace.json**

Create `umbraco-marketplace.json` at the repo root:

```json
{
  "$schema": "https://marketplace.umbraco.com/umbraco-marketplace-schema.json",
  "Title": "OC Automate LinkedIn",
  "Description": "LinkedIn integration for Umbraco Automate. Post content to LinkedIn as part of your automation workflows.",
  "Category": "Editor Tools",
  "AuthorDetails": {
    "Name": "Owain Williams",
    "Url": "https://owain.codes",
    "ImageUrl": ""
  },
  "LicenseTypes": [
    "MIT"
  ],
  "NuGetPackageId": "OC.Automate.LinkedIn",
  "Tags": [
    "automate",
    "linkedin",
    "social media"
  ],
  "RelatedPackages": [
    {
      "Name": "OC.Automate.Bluesky",
      "PackageId": "OC.Automate.Bluesky"
    },
    {
      "Name": "OC.Automate.Mastodon",
      "PackageId": "OC.Automate.Mastodon"
    }
  ],
  "OpenCollective": "",
  "OpenForContributions": true
}
```

- [ ] **Step 2: Verify full build and pack**

```bash
dotnet build src/OC.Automate.LinkedIn/OC.Automate.LinkedIn.csproj
dotnet pack src/OC.Automate.LinkedIn/OC.Automate.LinkedIn.csproj --no-build -o ./artifacts
```

Expected: Both succeed. A `.nupkg` file is created in `./artifacts`.

- [ ] **Step 3: Commit**

```bash
git add umbraco-marketplace.json
git commit -m "chore: add Umbraco marketplace metadata"
```

---

## Key Differences from Bluesky Package

| Aspect | Bluesky | LinkedIn |
|--------|---------|----------|
| Auth mechanism | App password → session creation per request | OAuth2 bearer token stored in config |
| Connection validation | Creates AT Proto session | Introspects token via LinkedIn endpoint |
| Post endpoint | `/xrpc/com.atproto.repo.createRecord` | `POST https://api.linkedin.com/rest/posts` |
| Post char limit | 300 | 3000 |
| Extra headers | None | `X-Restli-Protocol-Version`, `LinkedIn-Version` |
| Config section | `OwainCodes:Automate:Bluesky` | `OwainCodes:Automate:LinkedIn` |
| Connection settings | PDS URL, Identifier, ConnectionName | AuthorURN, ConnectionName |
| Additional config | — | ClientId, ClientSecret (for introspection) |

## Example appsettings.json

```json
{
  "OwainCodes": {
    "Automate": {
      "LinkedIn": {
        "ClientId": "your-linkedin-app-client-id",
        "ClientSecret": "your-linkedin-app-client-secret",
        "AccessTokens": {
          "my-linkedin": "your-oauth2-access-token"
        }
      }
    }
  }
}
```
