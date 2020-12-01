using System;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

namespace Nice3point.Revit.ADSK.MEP
{
    public class Application : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            var panel = application.CreateRibbonPanel("Шаблон ADSK");

            var buttonCopyAdsk = panel.AddItem(new PushButtonData("CopyADSK", "Копировать значения",
                Assembly.GetExecutingAssembly().Location, typeof(CopyAdsk).FullName)) as PushButton;
            buttonCopyAdsk.ToolTip = "Копирование параметров ADSK в спецификациях";
            buttonCopyAdsk.Image = new BitmapImage(new Uri(
                "pack://application:,,,/Nice3point.Revit.ADSK.MEP;component/Resources/CopyADSK16.png"));
            buttonCopyAdsk.LargeImage = new BitmapImage(new Uri(
                "pack://application:,,,/Nice3point.Revit.ADSK.MEP;component/Resources/CopyADSK32.png"));

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        private static BitmapSource GetEmbeddedImage(string name)
        {
            try
            {
                var a = Assembly.GetExecutingAssembly();
                var s = a.GetManifestResourceStream(name);
                return BitmapFrame.Create(s);
            }
            catch
            {
                return null;
            }
        }
    }
}