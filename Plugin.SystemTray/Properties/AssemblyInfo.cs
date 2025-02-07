using System.Reflection;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: Guid("17e36154-39a6-419f-b33b-28b8557a249a")]
[assembly: System.CLSCompliant(true)]

#if NETCOREAPP
[assembly: AssemblyMetadata("ProjectUrl", "https://dkorablin.ru/project/Default.aspx?File=94")]
#else

[assembly: AssemblyTitle("Plugin.SystemTray")]
[assembly: AssemblyDescription("Add application to the system tray when host is collapsed")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany("Danila Korablin")]
[assembly: AssemblyProduct("Plugin.SystemTray")]
[assembly: AssemblyCopyright("Copyright © Danila Korablin 2012-2025")]
#endif