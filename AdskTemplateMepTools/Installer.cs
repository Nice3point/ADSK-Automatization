using System;
using System.IO;
using WixSharp;

namespace AdskTemplateMepTools
{
    public class Installer
    {
#if R21
        private const string RevitVersion = "2021";
#elif R22
        private const string RevitVersion = "2022";
#endif
        private const string InstallationDir = @"%AppDataFolder%\Autodesk\Revit\Addins\" + RevitVersion;

        public static void Install()
        {
            var project = new Project
            {
                Name = "Revit ADSK MEP PlugIn",
                OutFileName = "AdskMepTools" + RevitVersion,
                Version = new Version(1, 0, int.Parse(RevitVersion)),
                Platform = Platform.x64,
                UI = WUI.WixUI_InstallDir,
                InstallScope = InstallScope.perUser,
                BackgroundImage = $@"{Directory.GetParent(Directory.GetCurrentDirectory())}/Resources/Icons/Installer/BackgroundImage.png",
                GUID = new Guid("71BB7C79-88F5-4A03-B4DA-E3953800471C"),
                Dirs = new[]
                {
                    new Dir($"{InstallationDir}",
                        new Files(@"Addin\*.*"))
                }
            };
            WixSharp.CommonTasks.Tasks.RemoveDialogsBetween(project, 
                WixSharp.Controls.NativeDialogs.WelcomeDlg,
                WixSharp.Controls.NativeDialogs.InstallDirDlg);
            project.BuildMsi();
        }
    }
}