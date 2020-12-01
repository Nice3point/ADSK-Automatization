using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Plugin
{
    public class CopyAdsk : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return Result.Succeeded;
        }
    }
}