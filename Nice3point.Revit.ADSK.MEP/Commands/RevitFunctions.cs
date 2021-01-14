using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace Nice3point.Revit.ADSK.MEP.Commands
{
    public static class RevitFunctions
    {
        public static List<Element> GetElementsOnRow(Document doc, ViewSchedule vs, int rowNumber)
        {
            var tableData = vs.GetTableData();
            var tableSectionData = tableData.GetSectionData(SectionType.Body);
            var elemIds = new FilteredElementCollector(doc, vs.Id)
                .ToElementIds()
                .ToList();

            using var t = new Transaction(doc, "Empty");
            t.Start();
            using (var st = new SubTransaction(doc))
            {
                st.Start();
                try
                {
                    tableSectionData.RemoveRow(rowNumber);
                }
                catch (Exception)
                {
                    return null;
                }

                st.Commit();
            }

            var remainingElementsIds = new FilteredElementCollector(doc, vs.Id)
                .ToElementIds()
                .ToList();
            t.RollBack();

            return elemIds
                .Where(id => !remainingElementsIds.Contains(id))
                .Select(doc.GetElement)
                .ToList();
        }

        public static void CopySystemNameValue(Document doc, IEnumerable<Element> elements)
        {
            using var tr = new Transaction(doc, "CopyNames");
            tr.Start();
            foreach (var curElement in elements)
            {
                var rbsName = curElement.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsString();
                if (curElement is FamilyInstance fInstance)
                {
                    if (null != fInstance.SuperComponent)
                    {
                        rbsName = fInstance.SuperComponent.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM)
                            .AsString();
                        fInstance.LookupParameter("ИмяСистемы").Set(rbsName);
                    }
                    else
                    {
                        fInstance.LookupParameter("ИмяСистемы").Set(rbsName);
                    }
                }
                else
                {
                    curElement.LookupParameter("ИмяСистемы").Set(rbsName);
                }
            }

            tr.Commit();
        }
    }
}