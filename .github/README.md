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

### 2. Add the app password to appsettings

App passwords are stored in configuration, not the backoffice. Add the following to your `appsettings.json` (or `appsettings.Production.json`):

```json
{
  "OwainCodes:Automate:Bluesky": {
    "AppPasswords": {
      "myaccount": "your-app-password-here"
    }
  }
}
```

The key (`myaccount` above) is a name you choose — you will reference it when creating the connection in the backoffice. You can add multiple entries if you need to post from more than one account.

For production it is recommended to supply passwords via environment variables rather than a config file:

```
OwainCodes__Automate__Bluesky__AppPasswords__myaccount=your-app-password-here
```

### 3. Create the connection in the backoffice

1. Go to **Automate → Connections** and create a new **Bluesky** connection.
2. Enter the PDS URL (defaults to `https://bsky.social`).
3. Enter your **Identifier** — your Bluesky handle (e.g. `yourname.bsky.social`) or email.
4. Enter the **Connection Name** — this must match the key you used in appsettings (e.g. `myaccount`).
5. Click **Test connection** to verify.

## Usage

Add the **Send Bluesky Post** action to any automation and select your Bluesky connection. Available fields:

| Field | Description |
|---|---|
| Content | The post text (max 300 characters). Supports `${ binding }` expressions. |
| Post URL | Optional URL appended to the post on a new line. |
| Content Warning | Optional label: `sexual`, `nudity`, `porn`, or `graphic-media`. |

## Compatibility

| Package version | Umbraco Automate | Umbraco CMS |
|---|---|---|
| 1.x | 17.x | 17.x |

## License

MIT

## Links

- [Source code](https://github.com/OwainWilliams/OC.Automate.Bluesky)
- [Report an issue](https://github.com/OwainWilliams/OC.Automate.Bluesky/issues)
