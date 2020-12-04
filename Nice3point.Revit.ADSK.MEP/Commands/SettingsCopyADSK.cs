using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Nice3point.Revit.ADSK.MEP.UI;

namespace Nice3point.Revit.ADSK.MEP.Commands
{
    [Transaction(TransactionMode.ReadOnly)] 
    public class SettingsCopyAdsk : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var settingsWindow = new CopyAdskSettings();
            settingsWindow.ShowDialog();
            return Result.Succeeded;
        }
    }
}