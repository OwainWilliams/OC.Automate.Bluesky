[![Downloads](https://img.shields.io/nuget/dt/OC.Automate.Bluesky?color=cc9900)](https://www.nuget.org/packages/OC.Automate.Bluesky/)
[![NuGet](https://img.shields.io/nuget/vpre/OC.Automate.Bluesky?color=0273B3)](https://www.nuget.org/packages/OC.Automate.Bluesky)
[![GitHub license](https://img.shields.io/github/license/OwainWilliams/OC.Automate.Bluesky?color=8AB803)](https://github.com/OwainWilliams/OC.Automate.Bluesky/blob/main/LICENSE)

# OC.Automate.Bluesky

A Bluesky connection type and action for [Umbraco Automate](https://github.com/umbraco/Umbraco.Automate).

Post to Bluesky as part of an automation workflow — for example, automatically posting when a blog post is published.

## Installation

```bash
dotnet add package OC.Automate.Bluesky
```

No further setup required. The composer registers itself automatically via Umbraco's `IComposer` discovery.

## Setup

### 1. Generate a Bluesky app password

In your Bluesky account go to **Settings → Privacy and Security → App Passwords** and create a new app password. Copy the generated password.

### 2. Create the connection in the backoffice

1. Go to **Automate → Connections** and create a new **Bluesky** connection.
2. Enter the **PDS URL** (defaults to `https://bsky.social`).
3. Enter your **Identifier** — your Bluesky handle (e.g. `yourname.bsky.social`) or email.
4. Enter the **App Password** you generated in step 1.
5. Click **Test connection** to verify.

### 3. (Optional) Store values in appsettings

Instead of entering values directly, you can store them under Umbraco Automate's built-in **Variables** (non-sensitive) and **Secrets** (sensitive) config sections:

```json
{
  "Umbraco": {
    "Automate": {
      "Variables": {
        "BlueskyPdsUrl": "https://bsky.social"
      },
      "Secrets": {
        "BlueskyAppPassword": "your-app-password-here"
      }
    }
  }
}
```

Then reference them in the connection fields using `$` syntax:

| Field | Reference |
|---|---|
| PDS URL | `$Umbraco:Automate:Variables:BlueskyPdsUrl` |
| App Password | `$Umbraco:Automate:Secrets:BlueskyAppPassword` |

The key names (`BlueskyPdsUrl`, `BlueskyAppPassword`) are your choice — they just have to match the `$` reference. For production, supply secrets via environment variables:

```
Umbraco__Automate__Secrets__BlueskyAppPassword=your-app-password-here
```

## Usage

Add the **Send Bluesky Post** action to any automation and select your Bluesky connection. Available fields:

| Field | Description |
|---|---|
| Content | The post text (max 300 characters). Supports `${ binding }` expressions. |
| Post URL | Optional URL appended to the post on a new line. |
| Content Warning | Optional label: `sexual`, `nudity`, `porn`, or `graphic-media`. |

## Migrating from 1.x

Version 2.x replaces the package-specific config section with Umbraco Automate's built-in Variables/Secrets pattern:

- The `OwainCodes:Automate:Bluesky:AppPasswords` / `Umbraco:Automate:Providers:OCAutomateBluesky` config sections are no longer read — remove them.
- The **Connection Name** field is gone. The app password is now entered directly on the connection, either as a literal value or as a `$Umbraco:Automate:Secrets:...` reference (see above).
- Existing Bluesky connections must be edited (or recreated) in the backoffice to add the app password.

## Compatibility

| Package version | Umbraco Automate | Umbraco CMS |
|---|---|---|
| 2.x | 17.x – 18.x | 17.x – 18.x |
| 1.x | 17.x – 18.x | 17.x – 18.x |

## License

MIT

## Links

- [Source code](https://github.com/OwainWilliams/OC.Automate.Bluesky)
- [Report an issue](https://github.com/OwainWilliams/OC.Automate.Bluesky/issues)
