using Umbraco.Automate.Core.Settings;

namespace OC.Automate.Bluesky;

public class BlueskyPostSettings
{
    [Field(Label = "Content",
        Description = "The content of the post (max 300 characters). Supports data bindings using ${ binding } syntax.",
        SupportsBindings = true,
        EditorUiAlias = "Umb.PropertyEditorUi.TextArea",
        EditorConfig = """[{ "alias": "rows", "value": 3 }]""")]
    public string Content { get; set; } = string.Empty;

    [Field(Label = "Post URL",
        Description = "Optional URL to append to the post.",
        SortOrder = 1,
        SupportsBindings = true)]
    public string? PostUrl { get; set; }

    [Field(Label = "Content Warning",
     Description = "Apply a content warning label (sexual, nudity, porn, or graphic-media). Leave empty for no warning.",
     SortOrder = 2)]
    public string? ContentWarning { get; set; }
}
