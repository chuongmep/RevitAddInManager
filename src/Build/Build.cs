using Nuke.Common;
using Nuke.Common.Execution;
using ricaun.Nuke;
using ricaun.Nuke.Components;

[CheckBuildProjectConfigurations]
class Build : NukeBuild, IPublishRevit
{
    bool IHazRevitPackageBuilder.NewVersions => true;
    public static int Main() => Execute<Build>(x => x.From<IPublishRevit>().Build);
}