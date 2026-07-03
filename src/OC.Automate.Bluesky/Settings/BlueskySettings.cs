namespace OC.Automate.Bluesky;

public class BlueskySettings
{
    public const string SectionName = "Umbraco:Automate:Providers:OCAutomateBluesky";

    public Dictionary<string, string> AppPasswords { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
