using System.Reflection;
using Autodesk.Revit.UI;

namespace Plugin
{
    public class Application : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            var panel = application.CreateRibbonPanel("Шаблон ADSK");
            var button = new PushButtonData("Copy ADSK", "Копирование параметров ADSK в спецификациях",
                Assembly.GetExecutingAssembly().Location, typeof(CopyAdsk).FullName);
            var pushButton = panel.AddItem(button) as PushButton;
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}