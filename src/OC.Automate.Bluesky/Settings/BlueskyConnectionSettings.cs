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
