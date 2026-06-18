namespace OC.Automate.Bluesky;

public class BlueskySettings
{
    public const string SectionName = "OwainCodes:Automate:Bluesky";

    public Dictionary<string, string> AppPasswords { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
