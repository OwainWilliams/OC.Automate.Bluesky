using Umbraco.Automate.Core.Settings;

namespace OC.Automate.Bluesky;

public sealed class BlueskyConnectionSettings
{
    [Field(
        Label = "Information",
        Description = "",
        EditorUiAlias = "Umb.PropertyEditorUi.Label",
        EditorConfig = """
            [{ "alias": "labelTemplate", "value": "**PDS URL**<br>If set in appsettings, reference it like $Umbraco:Automate:Variables:BlueskyPdsUrl<br><br>**App Password**<br>If set in appsettings, reference it like $Umbraco:Automate:Secrets:BlueskyAppPassword" }]
            """,
        SortOrder = 0)]
    public string? Labels { get; set; }

    [Field(Label = "Identifier", Description = "Your Bluesky handle (e.g. yourname.bsky.social) or registered email.", SortOrder = 2)]
    public string Identifier { get; set; } = string.Empty;

    [Field(Label = "PDS URL", Description = "The base URL of the Bluesky PDS (e.g. https://bsky.social).", SortOrder = 1)]
    public string PdsUrl { get; set; } = "https://bsky.social";

    [Field(Label = "App Password", Description = "A Bluesky app password (Settings → Privacy and Security → App Passwords).", IsSensitive = true, SortOrder = 3)]
    public string AppPassword { get; set; } = string.Empty;
}
