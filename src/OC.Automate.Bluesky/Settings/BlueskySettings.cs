namespace OC.Automate.Bluesky;

public class BlueskySettings
{
    public const string SectionName = "Umbraco:OwainCodes:Automate:Bluesky";

    public Dictionary<string, string> AppPasswords { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
