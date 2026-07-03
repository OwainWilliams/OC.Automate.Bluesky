using Microsoft.Extensions.DependencyInjection;
using OC.Automate.Bluesky.Composers;
using OC.Automate.Bluesky.Settings;
using Umbraco.Automate.Core.Actions;
using Umbraco.Automate.Core.Connections;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Manifest;

namespace OC.Automate.Bluesky;

public class BlueskyComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddOptions<BlueskySettings>()
            .BindConfiguration(BlueskySettings.SectionName);

        builder.WithCollectionBuilder<ConnectionTypeCollectionBuilder>()
            .Add<BlueskyConnectionType>();

        builder.WithCollectionBuilder<ActionCollectionBuilder>()
            .Add<SendBlueskyPostAction>();

        builder.Services.AddSingleton<IPackageManifestReader, BlueSkyPackageManifestReader>();
    }
}
