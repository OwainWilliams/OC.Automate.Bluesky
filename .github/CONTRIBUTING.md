# Contributing

Contributions to this package are most welcome!

## Development setup

A `NuGet.config` is included at the repo root to pull in the Umbraco nightly feed, which is needed for the `Umbraco.Automate.Core` beta package.

To get started:

1. Clone the repository
2. Open `OC.Automate.Bluesky.slnx` in Visual Studio or Rider
3. Build the solution — `dotnet build`

To test against a real Umbraco site, add a project reference from your local Umbraco site to `src/OC.Automate.Bluesky/OC.Automate.Bluesky.csproj`.

## Releasing

Releases are published to NuGet automatically via GitHub Actions when a version tag is pushed:

```bash
git tag 1.0.0
git push origin 1.0.0
```

The `NUGET_API_KEY` secret must be set in the repository settings.
