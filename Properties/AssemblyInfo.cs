using System.Resources;
using System.Reflection;
using System.Runtime.InteropServices;
using MelonLoader;
using AudicaModding;

[assembly: AssemblyTitle(GrindMode.BuildInfo.Name)]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(GrindMode.BuildInfo.Company)]
[assembly: AssemblyProduct(GrindMode.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + GrindMode.BuildInfo.Author)]
[assembly: AssemblyTrademark(GrindMode.BuildInfo.Company)]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
//[assembly: Guid("")]
[assembly: AssemblyVersion(GrindMode.BuildInfo.Version)]
[assembly: AssemblyFileVersion(GrindMode.BuildInfo.Version)]
[assembly: NeutralResourcesLanguage("en")]
[assembly: MelonModInfo(typeof(GrindMode), GrindMode.BuildInfo.Name, GrindMode.BuildInfo.Version, GrindMode.BuildInfo.Author, GrindMode.BuildInfo.DownloadLink)]


// Create and Setup a MelonModGame to mark a Mod as Universal or Compatible with specific Games.
// If no MelonModGameAttribute is found or any of the Values for any MelonModGame on the Mod is null or empty it will be assumed the Mod is Universal.
// Values for MelonModGame can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: MelonModGame(null, null)]