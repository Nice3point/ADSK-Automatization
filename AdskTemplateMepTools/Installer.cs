using System;
using System.IO;
using WixSharp;
using WixSharp.CommonTasks;

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
                Version = new Version(1, 4, int.Parse(RevitVersion)),
                Platform = Platform.x64,
                UI = WUI.WixUI_InstallDir,
                InstallScope = InstallScope.perUser,
                BackgroundImage = $@"{Directory.GetParent(Directory.GetCurrentDirectory())}/Resources/Icons/Installer/BackgroundImage.png",
                GUID = new Guid("577796C3-A5D7-49F1-8985-85D129DA091C"),
                Dirs = new[]
                {
                    new Dir($"{InstallationDir}",
                        new Files(@"Addin\*.*"))
                }
            };
            project.RemoveDialogsBetween(WixSharp.Controls.NativeDialogs.WelcomeDlg, WixSharp.Controls.NativeDialogs.InstallDirDlg);
            project.BuildMsi();
        }
    }
}