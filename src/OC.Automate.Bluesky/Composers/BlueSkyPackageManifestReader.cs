using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Infrastructure.Manifest;

namespace OC.Automate.Bluesky.Composers
{
    public class BlueSkyPackageManifestReader : IPackageManifestReader
    {
        public Task<IEnumerable<PackageManifest>> ReadPackageManifestsAsync()
        {
            var version = typeof(BlueSkyPackageManifestReader).Assembly.GetName().Version?.ToString() ?? "1.0.0";
            return Task.FromResult<IEnumerable<PackageManifest>>(new[]
            {
                new PackageManifest
                {
                    Id = "OC.Automate.Bluesky",
                    Name = "OC Automate Bluesky",
                    Version = version,
                    AllowTelemetry = true,
                    Extensions = [
                     new
                        {
                            name = "OC Automate Bluesky Bundle",
                            alias = "OC.Automate.Bluesky.Bundle",
                            type = "bundle",
                            js = "/App_Plugins/OC.Automate.Bluesky/oc-automate-bluesky.js?v=" + version
                        }]

                }
            });
        }
    }
}
