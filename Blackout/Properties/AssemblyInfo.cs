using MelonLoader;
using NAK.Blackout.Properties;
using System.Reflection;


[assembly: AssemblyVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyFileVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyInformationalVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyTitle(nameof(NAK.Blackout))]
[assembly: AssemblyCompany(AssemblyInfoParams.Author)]
[assembly: AssemblyProduct(nameof(NAK.Blackout))]

[assembly: MelonInfo(
    typeof(NAK.Blackout.Blackout),
    nameof(NAK.Blackout),
    AssemblyInfoParams.Version,
    AssemblyInfoParams.Author,
    downloadLink: "https://github.com/NotAKidOnSteam/NAK_CVR_Mods/tree/main/Blackout"
)]

[assembly: MelonGame(null, "ChilloutVR")]
[assembly: MelonPlatform(MelonPlatformAttribute.CompatiblePlatforms.WINDOWS_X64)]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.MONO)]
[assembly: MelonOptionalDependencies("UIExpansionKit", "BTKUILib")]

namespace NAK.Blackout.Properties;
internal static class AssemblyInfoParams
{
    public const string Version = "2.1.7";
    public const string Author = "Nirvash, NotAKidoS";
}